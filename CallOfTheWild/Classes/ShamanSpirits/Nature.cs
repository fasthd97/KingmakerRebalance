﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    partial class Shaman
    {
        internal class NatureSpirit
        {
            internal static BlueprintFeature spirit_ability;
            internal static BlueprintFeature greater_spirit_ability;
            internal static BlueprintFeature true_spirit_ability;
            internal static BlueprintFeature true_spirit_ability_wandering;
            internal static BlueprintFeature manifestation;
            internal static BlueprintFeature entangling_curse;
            internal static BlueprintFeature erosion_curse;
            internal static BlueprintFeature friend_to_animals;
            internal static BlueprintFeature storm_walker;
            internal static BlueprintAbility[] spells;
            internal static BlueprintFeature[] hexes;

            internal static Spirit create()
            {

                entangling_curse = hex_engine.createEntanglingCurse("ShamanEntanglingCurse",
                                                 "Entangling Curse",
                                                 "The shaman entangles a creature within 30 feet for a number of rounds equal to the shaman’s Charisma modifier (minimum 1). A successful Reflex saving throw negates this effect. Whether or not the save is successful, the creature cannot be the target of this hex again for 24 hours."
                                                 );

                erosion_curse = hex_engine.createErosionCurse("ShamanErosionCurse",
                                                                  "Erosion Curse",
                                                                  "The shaman summons the powers of nature to erode a construct or object within 30 feet. This erosion deals 1d6 points of damage per 2 shaman levels, ignoring hardness and damage reduction. If used against a construct or an object in another creature’s possession, the construct or the creature possessing the object can attempt a Reflex saving throw to halve the damage. Once an object or a construct is damaged by this erosion, it cannot be the target of this hex again for 24 hours."
                                                                  );

                friend_to_animals = hex_engine.createFriendToAnimals("ShamanFriendToAnimals",
                                                                      "Friend to Animals",
                                                                      "The shaman can spontaneously cast summon nature’s ally spells as a druid. In addition, all animals within 30 feet of the shaman receive a sacred bonus on all saving throws equal to the shaman’s Charisma modifier."
                                                                     );
                storm_walker = hex_engine.createStormWalker("ShamanStormWalker",
                                                            "Stormwalker",
                                                            "The shaman can move through non-magical fog, rain, mist, snow, and other environmental effects without penalty. She is never slowed by such effects. "
                                                           );

                createSpiritAbility();
                createGreaterSpiritAbility();
                createTrueSpiritAbility();
                createManifestation();

                spells = new BlueprintAbility[9]
                {
                    library.Get<BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33"), //Magic Fang
                    library.Get<BlueprintAbility>("5b77d7cc65b8ab74688e74a37fc2f553"), //barkskin
                    library.Get<BlueprintAbility>("754c478a2aa9bb54d809e648c3f7ac0e"), //dominate animal
                    library.Get<BlueprintAbility>("e418c20c8ce362943a8025d82c865c1c"), //cape of wasps
                    library.Get<BlueprintAbility>("6d1d48a939ce475409f06e1b376bc386"), //vinetrap
                    library.Get<BlueprintAbility>("7c5d556b9a5883048bf030e20daebe31"), //stone skin communal
                    library.Get<BlueprintAbility>("b974af13e45639a41a04843ce1c9aa12"), //creeping doom
                    library.Get<BlueprintAbility>("7cfbefe0931257344b2cb7ddc4cdff6f"), //stormbolts
                    library.Get<BlueprintAbility>("d8144161e352ca846a73cf90e85bf9ac"), //tsunami
                };



                hexes = new BlueprintFeature[]
                {
                    entangling_curse,
                    erosion_curse,
                    friend_to_animals,
                    storm_walker,
                };

                return new Spirit("Nature",
                                  "Nature",
                                  "A shaman who selects the nature spirit takes on an appearance that reflects the aspect of the natural world she has the closest connection to. A nature shaman from the forest has a green tinge to her skin and hair, with eyes of sparkling emerald and the scent of green leaves and flowers about her. A nature shaman from the tundra is typically alabaster pale, with platinum hair and crystal blue eyes, and her skin always seems strangely cold.",
                                  manifestation.Icon,
                                  "",
                                  new BlueprintFeature[] { spirit_ability, spirit_ability },
                                  new BlueprintFeature[] { greater_spirit_ability, greater_spirit_ability },
                                  new BlueprintFeature[] { true_spirit_ability, true_spirit_ability_wandering },
                                  manifestation,
                                  hexes,
                                  spells);
            }


            static void createSpiritAbility()
            {
                var icon = library.Get<BlueprintAbility>("093ed1d67a539ad4c939d9d05cfe192c").Icon; //sirocco
                var resource = Helpers.CreateAbilityResource("ShamanStormBurstResource", "", "", "", null);
                resource.SetIncreasedByStat(3, StatType.Charisma);
                var buff = Helpers.CreateBuff("ShamanStormBurstBuff",
                                              "Storm Burst",
                                              "As a standard action, the shaman causes a small storm of swirling wind and rain to form around one creature within 30 feet. This storm causes the target to treat all foes as if they had concealment, suffering a 20% miss chance for 1 round plus 1 round for every 4 shaman levels she possesses. The shaman can use this ability a number of times per day equal to 3 + her Charisma modifier.",
                                              "",
                                              icon,
                                              Common.createPrefabLink("6dc97e33e73b5ec49bd03b90c2345d7f"), //air elemental medium cycle
                                              Helpers.Create<OutgoingConcealementMechanics.AddOutgoingConcealment>(a => { a.Descriptor = ConcealmentDescriptor.Fog; a.Concealment = Concealment.Partial; })
                                              );
                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);
                var ability = Helpers.CreateAbility("ShamanStormBurstAbility",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Supernatural,
                                                    CommandType.Standard,
                                                    AbilityRange.Close,
                                                    "1 round + 1 round/4 levels",
                                                    Helpers.savingThrowNone,
                                                    Helpers.CreateRunActions(apply_buff),
                                                    Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                                    stepLevel: 4, classes: getShamanArray()),
                                                    Helpers.CreateResourceLogic(resource)
                                                    );
                ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);

                var thundering = library.Get<BlueprintWeaponEnchantment>("690e762f7704e1f4aa1ac69ef0ce6a96");

                var thundering_weapon_feature = Helpers.CreateFeature("ShamanStormBurstThunderingWeaponFeature",
                                                              "",
                                                              "",
                                                              "",
                                                              null,
                                                              FeatureGroup.None,
                                                              Helpers.Create<NewMechanics.EnchantmentMechanics.PersistentWeaponEnchantment>(p => p.enchant = thundering)
                                                              );

                thundering_weapon_feature.HideInCharacterSheetAndLevelUp = true;

                spirit_ability = Helpers.CreateFeature("ShamanStormBurstFeature",
                                                       ability.Name,
                                                       "As a standard action, the shaman causes a small storm of swirling wind and rain to form around one creature within 30 feet. This storm causes the target to treat all foes as if they had concealment, suffering a 20% miss chance for 1 round plus 1 round for every 4 shaman levels she possesses. The shaman can use this ability a number of times per day equal to 3 + her Charisma modifier. At 11th level, any weapon she wields is treated as a thundering weapon.",
                                                       "",
                                                       ability.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(ability),
                                                       Helpers.CreateAddAbilityResource(resource),
                                                       Helpers.CreateAddFeatureOnClassLevel(thundering_weapon_feature, 11, getShamanArray())
                                                       );
            }


            static void createGreaterSpiritAbility()
            {
                var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/SpiritOfNature.png");
                var buff = Helpers.CreateBuff("ShamanSpiritOfNatureBuff",
                                              "Spirit of Nature",
                                              "Whenever the shaman is reduced to below 25% hit points, she gains fast healing 1 for 1d4 rounds. At 15th level, this increases to fast healing 3.",
                                              "",
                                              icon,
                                              null,
                                              Common.createAddContextEffectFastHealing(Helpers.CreateContextValue(AbilityRankType.Default)),
                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Custom,
                                                                              classes: getShamanArray(), customProgression: new (int, int)[] { (14, 1), (20, 3) })
                                             );

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1), dispellable: false);
                var effect = Helpers.CreateConditional(new Condition[] {Common.createContextConditionHasFact(buff, has: false),
                                                                        Helpers.Create<NewMechanics.ContextConditionCompareTargetHPPercent>(c => c.Value = 25) },
                                                                        apply_buff);

                greater_spirit_ability = Helpers.CreateFeature("ShamanSpiritOfNatureFeature",
                                                               buff.Name,
                                                               buff.Description,
                                                               "",
                                                               icon,
                                                               FeatureGroup.None,
                                                               Common.createIncomingDamageTrigger(effect)
                                                               //Helpers.CreateAddFactContextActions(newRound: effect)
                                                               );
            }


            static void createTrueSpiritAbility()
            {
                var animal_companion_rank = library.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d");
                var animal_companion_progression = Helpers.CreateProgression("ShamanAnimalCompanionProgression",
                                                                             "",
                                                                             "",
                                                                             "",
                                                                             null,
                                                                             FeatureGroup.None);
                animal_companion_progression.LevelEntries = new LevelEntry[]
                {
                    Helpers.LevelEntry(17, animal_companion_rank),
                    Helpers.LevelEntry(18, animal_companion_rank),
                    Helpers.LevelEntry(19, animal_companion_rank),
                    Helpers.LevelEntry(20, animal_companion_rank),
                };
                animal_companion_progression.HideInUI = true;
                true_spirit_ability = library.CopyAndAdd<BlueprintFeature>("571f8434d98560c43935e132df65fe76", "ShamanAnimalCompanionFeature", "");
                true_spirit_ability.SetDescription("The shaman acquires an animal companion of her choice, using her shaman level as her effective druid level");
                var add_rank = Helpers.Create<AddFeatureOnApply>(a => a.Feature = animal_companion_rank);
                true_spirit_ability.ComponentsArray = Enumerable.Repeat(add_rank, 16).ToArray();
                true_spirit_ability.AddComponent(Helpers.Create<AddFeatureOnApply>(a => a.Feature = animal_companion_progression));

                true_spirit_ability_wandering = library.CopyAndAdd<BlueprintFeature>("571f8434d98560c43935e132df65fe76", "ShamanAnimalCompanionWanderingFeature", "");
                true_spirit_ability_wandering.ComponentsArray = Enumerable.Repeat(add_rank, 20).ToArray();
            }


            static void createManifestation()
            {
                var resource = Helpers.CreateAbilityResource("ShamanNatureManifestationResource", "", "", "", null);
                resource.SetFixedResource(1);
                var friend_to_animals_buff = friend_to_animals.GetComponent<AuraFeatureComponent>().Buff.GetComponent<AddAreaEffect>().AreaEffect.GetComponent<AbilityAreaEffectBuff>().Buff;
                var apply_friend_to_animals = Common.createContextActionApplyBuff(friend_to_animals_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);

                var animal_fact = library.Get<BlueprintFeature>("a95311b3dc996964cbaa30ff9965aaf6");
                var plant_fact = library.Get<BlueprintFeature>("706e61781d692a042b35941f14bc41c5");

                var animal_buff = Helpers.CreateBuff("ShamanNatureManifestationAnimalBuff",
                                                     "Manifestation: Animal",
                                                     "Upon reaching 20th level, the shaman becomes a spirit of nature. Once per day, she can change her type to plant, animal, or humanoid, and gain superficial physical characteristics of the chosen type as appropriate. She must choose a type that is different from her current type. This change doesn’t alter her Hit Dice, hit points, saving throws, skill ranks, class skills, or proficiencies.",
                                                     "",
                                                     library.Get<BlueprintAbility>("de7a025d48ad5da4991e7d3c682cf69d").Icon, //cat's grace
                                                     null,
                                                     Helpers.CreateAddFact(animal_fact),
                                                     Helpers.CreateAddFactContextActions(Helpers.CreateConditional(Helpers.CreateConditionCasterHasFact(friend_to_animals), apply_friend_to_animals),
                                                                                         Common.createContextActionRemoveBuff(friend_to_animals_buff))
                                                     );
                var apply_animal = Common.createContextActionApplyBuff(animal_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
                var plant_buff = Helpers.CreateBuff("ShamanNatureManifestationPlantBuff",
                                                     "Manifestation: Plant",
                                                     animal_buff.Description,
                                                     "",
                                                     library.Get<BlueprintAbility>("0fd00984a2c0e0a429cf1a911b4ec5ca").Icon, //entnagle
                                                     null,
                                                     Helpers.CreateAddFact(plant_fact)
                                                     );
                var apply_plant = Common.createContextActionApplyBuff(plant_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
                var precast_actions = Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(animal_buff), Common.createContextActionRemoveBuff(plant_buff));


                var manifestation_animal = Helpers.CreateAbility("ShamanNatureManifestationAnimalAbility",
                                                                animal_buff.Name,
                                                                animal_buff.Description,
                                                                "",
                                                                animal_buff.Icon,
                                                                AbilityType.Supernatural,
                                                                CommandType.Standard,
                                                                AbilityRange.Personal,
                                                                "Permanent",
                                                                "",
                                                                Helpers.CreateRunActions(apply_animal),
                                                                Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(precast_actions)),
                                                                Common.createAbilitySpawnFx("b5fc8209a9e75ff47acfd132540e0ba6", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                                Common.createAbilityCasterHasNoFacts(animal_fact),
                                                                Helpers.CreateResourceLogic(resource)
                                                                );
                Common.setAsFullRoundAction(manifestation_animal);
                var manifestation_plant = Helpers.CreateAbility("ShamanNatureManifestationPlantAbility",
                                                                    plant_buff.Name,
                                                                    plant_buff.Description,
                                                                    "",
                                                                    plant_buff.Icon,
                                                                    AbilityType.Supernatural,
                                                                    CommandType.Standard,
                                                                    AbilityRange.Personal,
                                                                    "Permanent",
                                                                    "",
                                                                    Helpers.CreateRunActions(apply_plant),
                                                                    Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(precast_actions)),
                                                                    Common.createAbilitySpawnFx("b5fc8209a9e75ff47acfd132540e0ba6", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                                    Common.createAbilityCasterHasNoFacts(plant_fact),
                                                                    Helpers.CreateResourceLogic(resource)
                                                                    );
                Common.setAsFullRoundAction(manifestation_plant);
                var manifestation_humanoid = Helpers.CreateAbility("ShamanNatureManifestationHumanoidAbility",
                                                                    "Manifestation: Humanoid",
                                                                    plant_buff.Description,
                                                                    "",
                                                                    library.Get<BlueprintAbility>("bd09b025ee2a82f46afab922c4decca9").Icon, //turn back
                                                                    AbilityType.Supernatural,
                                                                    CommandType.Standard,
                                                                    AbilityRange.Personal,
                                                                    "Permanent",
                                                                    "",
                                                                    Helpers.CreateRunActions(precast_actions),
                                                                    Common.createAbilitySpawnFx("b5fc8209a9e75ff47acfd132540e0ba6", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                                    Common.createAbilityCasterHasFacts(plant_fact, animal_fact),
                                                                    Helpers.CreateResourceLogic(resource)
                                                                    );
                Common.setAsFullRoundAction(manifestation_humanoid);

                var wrapper = Common.createVariantWrapper("ShamanNatureManifestationAbility", "", manifestation_animal, manifestation_humanoid, manifestation_plant);
                wrapper.SetName("Manifestation");
                wrapper.SetIcon(manifestation_plant.Icon);

                manifestation = Helpers.CreateFeature("ShamanManifestationFeature",
                                                      wrapper.Name,
                                                      wrapper.Description,
                                                      "",
                                                      wrapper.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(wrapper),
                                                      Helpers.CreateAddAbilityResource(resource)
                                                      );
            }

        }
    }
}
