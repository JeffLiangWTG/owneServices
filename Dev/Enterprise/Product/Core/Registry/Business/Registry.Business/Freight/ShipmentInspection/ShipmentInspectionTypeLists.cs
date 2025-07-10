using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Registry.Business
{
	public class ShipmentInspectionTypeLists
	{
		public ShipmentInspectionTypeLists(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}

		readonly BusinessObjectFactory factory;

		public ShipmentInspectionTypeCollection SystemDefinedAdditionalInspectionalTypeList
		{
			get
			{
				return factory.GetCachedValue<ShipmentInspectionTypeCollection>("ShipmentInspectionTypeLists.SystemDefinedAdditionalInspectionalTypeList", delegate
				{
					var list = new ShipmentInspectionTypeCollection(ZString.Empty);

					list.Add(AdditionalScreeningMethods.Codes.PhysicalInspectionAndHandSearch, AdditionalScreeningMethods.Descriptions.PhysicalInspectionAndHandSearch, true, true);
					list.Add(AdditionalScreeningMethods.Codes.VisualCheck, AdditionalScreeningMethods.Descriptions.VisualCheck, true, true);
					list.Add(AdditionalScreeningMethods.Codes.XRayEquipment, AdditionalScreeningMethods.Descriptions.XRayEquipment, true, true);
					list.Add(AdditionalScreeningMethods.Codes.EDSEquipment, AdditionalScreeningMethods.Descriptions.EDSEquipment, true, true);
					list.Add(AdditionalScreeningMethods.Codes.AnyOtherMethod, AdditionalScreeningMethods.Descriptions.AnyOtherMethod, true, true);
					list.Add(AdditionalScreeningMethods.Codes.ExplosiveDetectionDogs, AdditionalScreeningMethods.Descriptions.ExplosiveDetectionDogs, true, true);
					list.Add(AdditionalScreeningMethods.Codes.ETDEquipment, AdditionalScreeningMethods.Descriptions.ETDEquipment, true, true);
					list.Add(AdditionalScreeningMethods.Codes.MetalDetectionEquipment, AdditionalScreeningMethods.Descriptions.MetalDetectionEquipment, true, true);
					list.Add(AdditionalScreeningMethods.Codes.ExplosivesVaporDetection, AdditionalScreeningMethods.Descriptions.ExplosivesVaporDetection, true, true);

					return list;
				});
			}
		}

		public ShipmentInspectionTypeCollection SystemDefinedList_IATA
		{
			get
			{
				return factory.GetCachedValue<ShipmentInspectionTypeCollection>("ShipmentInspectionTypeLists.SystemDefinedList_IATA", delegate
				{
					var list = new ShipmentInspectionTypeCollection(ZString.Empty);

					list.Add(ScreeningMethods.Codes.PhysicalInspectionAndHandSearch, ScreeningMethods.Descriptions.PhysicalInspectionAndHandSearch, true, true);
					list.Add(ScreeningMethods.Codes.VisualCheck, ScreeningMethods.Descriptions.VisualCheck, true, true);
					list.Add(ScreeningMethods.Codes.XRayEquipment, ScreeningMethods.Descriptions.XRayEquipment, true, true);
					list.Add(ScreeningMethods.Codes.ExplosiveDetectionSystem, ScreeningMethods.Descriptions.ExplosiveDetectionSystem, true, true);
					list.Add(ScreeningMethods.Codes.SubjectedToAnyOtherMeans, ScreeningMethods.Descriptions.SubjectedToAnyOtherMeans, true, true);
					list.Add(ScreeningMethods.Codes.ExplosiveDetectionDogs, ScreeningMethods.Descriptions.ExplosiveDetectionDogs, true, true);
					list.Add(ScreeningMethods.Codes.ExplosivesTraceDetectionEquipment, ScreeningMethods.Descriptions.ExplosivesTraceDetectionEquipment, true, true);
					list.Add(ScreeningMethods.Codes.CargoMetalDetection, ScreeningMethods.Descriptions.CargoMetalDetection, true, true);
					list.Add(ScreeningMethods.Codes.ExplosivesVaporDetection, ScreeningMethods.Descriptions.ExplosivesVaporDetection, true, true);

					list.Add(ExemptionCodes.Codes.SmallUndersizedShipments, ExemptionCodes.Descriptions.SmallUndersizedShipments, true, true);
					list.Add(ExemptionCodes.Codes.Mail, ExemptionCodes.Descriptions.Mail, true, true);
					list.Add(ExemptionCodes.Codes.BiomedicalSamples, ExemptionCodes.Descriptions.BiomedicalSamples, true, true);
					list.Add(ExemptionCodes.Codes.DiplomaticBagsOrDiplomaticMail, ExemptionCodes.Descriptions.DiplomaticBagsOrDiplomaticMail, true, true);
					list.Add(ExemptionCodes.Codes.LifeSavingMaterials, ExemptionCodes.Descriptions.LifeSavingMaterials, true, true);
					list.Add(ExemptionCodes.Codes.NuclearMaterial, ExemptionCodes.Descriptions.NuclearMaterial, true, true);
					list.Add(ExemptionCodes.Codes.TransferOrTransshipment, ExemptionCodes.Descriptions.TransferOrTransshipment, true, true);

					return list;
				});
			}
		}

		public ShipmentInspectionTypeCollection SystemDefinedList_Australia
		{
			get
			{
				return factory.GetCachedValue<ShipmentInspectionTypeCollection>("ShipmentInspectionTypeLists.SystemDefinedList_AU", delegate
				{
					var list = new ShipmentInspectionTypeCollection(Core.Constants.CountryCodes.Australia);

					list.Add(ScreeningMethodsAU.Codes.XRay, ScreeningMethodsAU.Descriptions.XRay, true, true);
					list.Add(ScreeningMethodsAU.Codes.ElectronicMetalDetection, ScreeningMethodsAU.Descriptions.ElectronicMetalDetection, true, true);
					list.Add(ScreeningMethodsAU.Codes.ExplosivesTraceDetectionEquipment, ScreeningMethodsAU.Descriptions.ExplosivesTraceDetectionEquipment, true, true);
					list.Add(ScreeningMethodsAU.Codes.PhysicalExamination, ScreeningMethodsAU.Descriptions.PhysicalExamination, true, true);

					list.Add(ExemptionCodesAU.Codes.BiomedicalSamples, ExemptionCodesAU.Descriptions.BiomedicalSamples, true, true);
					list.Add(ExemptionCodesAU.Codes.DiplomaticBags, ExemptionCodesAU.Descriptions.DiplomaticBags, true, true);
					list.Add(ExemptionCodesAU.Codes.LifeSavingMaterials, ExemptionCodesAU.Descriptions.LifeSavingMaterials, true, true);
					list.Add(ExemptionCodesAU.Codes.Mail, ExemptionCodesAU.Descriptions.Mail, true, true);
					list.Add(ExemptionCodesAU.Codes.NuclearMaterial, ExemptionCodesAU.Descriptions.NuclearMaterial, true, true);

					return list;
				});
			}
		}

		public ShipmentInspectionTypeCollection SystemDefinedList_EuropeanUnion
		{
			get
			{
				return factory.GetCachedValue<ShipmentInspectionTypeCollection>("ShipmentInspectionTypeLists.SystemDefinedList_EU", delegate
				{
					var list = new ShipmentInspectionTypeCollection(Core.Constants.CountryCodes.EuropeanUnion);

					list.Add(ScreeningMethods.Codes.PhysicalInspectionAndHandSearch, ScreeningMethods.Descriptions.PhysicalInspectionAndHandSearch, true, true);
					list.Add(ScreeningMethods.Codes.VisualCheck, ScreeningMethods.Descriptions.VisualCheck, true, true);
					list.Add(ScreeningMethods.Codes.XRayEquipment, ScreeningMethods.Descriptions.XRayEquipment, true, true);
					list.Add(ScreeningMethods.Codes.ExplosiveDetectionSystem, ScreeningMethods.Descriptions.ExplosiveDetectionSystem, true, true);
					list.Add(ScreeningMethods.Codes.SubjectedToAnyOtherMeans, ScreeningMethods.Descriptions.SubjectedToAnyOtherMeans, true, true);
					list.Add(ScreeningMethods.Codes.ExplosiveDetectionDogs, ScreeningMethods.Descriptions.ExplosiveDetectionDogs, true, true);
					list.Add(ScreeningMethods.Codes.ExplosivesTraceDetectionEquipment, ScreeningMethods.Descriptions.ExplosivesTraceDetectionEquipment, true, true);
					list.Add(ScreeningMethods.Codes.CargoMetalDetection, ScreeningMethods.Descriptions.CargoMetalDetection, true, true);
					list.Add(ScreeningMethods.Codes.ExplosivesVaporDetection, ScreeningMethods.Descriptions.ExplosivesVaporDetection, true, true);

					list.Add(ExemptionCodes.Codes.SmallUndersizedShipments, ExemptionCodes.Descriptions.SmallUndersizedShipments, true, true);
					list.Add(ExemptionCodes.Codes.Mail, ExemptionCodes.Descriptions.Mail, true, true);
					list.Add(ExemptionCodes.Codes.BiomedicalSamples, ExemptionCodes.Descriptions.BiomedicalSamples, true, true);
					list.Add(ExemptionCodes.Codes.DiplomaticBagsOrDiplomaticMail, ExemptionCodes.Descriptions.DiplomaticBagsOrDiplomaticMail, true, true);
					list.Add(ExemptionCodes.Codes.LifeSavingMaterials, ExemptionCodes.Descriptions.LifeSavingMaterials, true, true);
					list.Add(ExemptionCodes.Codes.NuclearMaterial, ExemptionCodes.Descriptions.NuclearMaterial, true, true);
					list.Add(ExemptionCodes.Codes.TransferOrTransshipment, ExemptionCodes.Descriptions.TransferOrTransshipment, true, true);
					list.Add(ExemptionCodes.Codes.GovernmentApprovedReliableOrganization, ExemptionCodes.Descriptions.GovernmentApprovedReliableOrganization, true, true);
					list.Add(ExemptionCodes.Codes.AdHocMovementsOfCargo, ExemptionCodes.Descriptions.AdHocMovementsOfCargo, true, true);

					return list;
				});
			}
		}

		public ShipmentInspectionTypeCollection SystemDefinedList_Japan
		{
			get
			{
				return factory.GetCachedValue<ShipmentInspectionTypeCollection>("ShipmentInspectionTypeLists.SystemDefinedList_JP", delegate
				{
					var list = new ShipmentInspectionTypeCollection(Core.Constants.CountryCodes.Japan);

					list.Add(ScreeningMethodsJP.Codes.PhysicalInspectionAndHandSearch, ScreeningMethodsJP.Descriptions.PhysicalInspectionAndHandSearch, true, true);
					list.Add(ScreeningMethodsJP.Codes.VisualCheck, ScreeningMethodsJP.Descriptions.VisualCheck, true, true);
					list.Add(ScreeningMethodsJP.Codes.XRayEquipment, ScreeningMethodsJP.Descriptions.XRayEquipment, true, true);
					list.Add(ScreeningMethodsJP.Codes.ExplosiveDetectionSystem, ScreeningMethodsJP.Descriptions.ExplosiveDetectionSystem, true, true);
					list.Add(ScreeningMethodsJP.Codes.ExplosivesTraceDetectionEquipment, ScreeningMethodsJP.Descriptions.ExplosivesTraceDetectionEquipment, true, true);

					list.Add(ExemptionCodesJP.Codes.SmallUndersizedShipments, ExemptionCodesJP.Descriptions.SmallUndersizedShipments, true, true);
					list.Add(ExemptionCodesJP.Codes.DiplomaticBagsOrDiplomaticMail, ExemptionCodesJP.Descriptions.DiplomaticBagsOrDiplomaticMail, true, true);
					list.Add(ExemptionCodesJP.Codes.TransferOrTransshipment, ExemptionCodesJP.Descriptions.TransferOrTransshipment, true, true);
					list.Add(ExemptionCodesJP.Codes.ObviousSafety, ExemptionCodesJP.Descriptions.ObviousSafety, true, true);
					list.Add(ExemptionCodesJP.Codes.LiveAnimal, ExemptionCodesJP.Descriptions.LiveAnimal, true, true);

					return list;
				});
			}
		}

		public ShipmentInspectionTypeCollection SystemDefinedList_HongKong
		{
			get
			{
				return factory.GetCachedValue<ShipmentInspectionTypeCollection>("ShipmentInspectionTypeLists.SystemDefinedList_HK", delegate
				{
					var list = new ShipmentInspectionTypeCollection(Core.Constants.CountryCodes.HongKong);

					list.Add(ScreeningMethodsHK.Codes.PhysicalInspectionAndHandSearch, ScreeningMethodsHK.Descriptions.PhysicalInspectionAndHandSearch, true, true);
					list.Add(ScreeningMethodsHK.Codes.XRayEquipment, ScreeningMethodsHK.Descriptions.XRayEquipment, true, true);
					list.Add(ScreeningMethodsHK.Codes.ExplosivesTraceDetectionEquipment, ScreeningMethodsHK.Descriptions.ExplosivesTraceDetectionEquipment, true, true);

					list.Add(ExemptionCodesHK.Codes.MailOrSmallCargo, ExemptionCodesHK.Descriptions.MailOrSmallCargo, true, true);
					list.Add(ExemptionCodesHK.Codes.MedicalDrugsOrLifeSavingSupplies, ExemptionCodesHK.Descriptions.MedicalDrugsOrLifeSavingSupplies, true, true);
					list.Add(ExemptionCodesHK.Codes.TransitOrTransferCargo, ExemptionCodesHK.Descriptions.TransitOrTransferCargo, true, true);
					list.Add(ExemptionCodesHK.Codes.HumanRemainsOrAshes, ExemptionCodesHK.Descriptions.HumanRemainsOrAshes, true, true);
					list.Add(ExemptionCodesHK.Codes.Livestock, ExemptionCodesHK.Descriptions.Livestock, true, true);

					return list;
				});
			}
		}

		public ShipmentInspectionTypeCollection SystemDefinedList_Singapore
		{
			get
			{
				return factory.GetCachedValue<ShipmentInspectionTypeCollection>("ShipmentInspectionTypeLists.SystemDefinedList_SG", delegate
				{
					var list = new ShipmentInspectionTypeCollection(Core.Constants.CountryCodes.Singapore);
					return list;
				});
			}
		}

		public ShipmentInspectionTypeCollection SystemDefinedList_Canada
		{
			get
			{
				return factory.GetCachedValue<ShipmentInspectionTypeCollection>("ShipmentInspectionTypeLists.SystemDefinedList_CA", delegate
				{
					var list = new ShipmentInspectionTypeCollection(Core.Constants.CountryCodes.Canada);

					list.Add(ScreeningMethodsCA.Codes.PhysicalInspectionAndOrHandSearch, ScreeningMethodsCA.Descriptions.PhysicalInspectionAndOrHandSearch, true, true);
					list.Add(ScreeningMethodsCA.Codes.XRayEquipment, ScreeningMethodsCA.Descriptions.XRayEquipment, true, true);
					list.Add(ScreeningMethodsCA.Codes.ExplosivesTraceDetectionEquipment, ScreeningMethodsCA.Descriptions.ExplosivesTraceDetectionEquipment, true, true);
					list.Add(ScreeningMethodsCA.Codes.ExplosiveDetectionDogs, ScreeningMethodsCA.Descriptions.ExplosiveDetectionDogs, true, true);

					list.Add(ExemptionCodesCA.Codes.BiomedicalSamples, ExemptionCodesCA.Descriptions.BiomedicalSamples, true, true);
					list.Add(ExemptionCodesCA.Codes.DiplomaticBagsOrDiplomaticMail, ExemptionCodesCA.Descriptions.DiplomaticBagsOrDiplomaticMail, true, true);
					list.Add(ExemptionCodesCA.Codes.TransferOrTransshipment, ExemptionCodesCA.Descriptions.TransferOrTransshipment, true, true);

					return list;
				});
			}
		}

		public ShipmentInspectionTypeCollection SystemDefinedList_SouthAfrica
		{
			get
			{
				return factory.GetCachedValue("ShipmentInspectionTypeLists.SystemDefinedList_ZA", delegate
				{
					var list = new ShipmentInspectionTypeCollection(Constants.CountryCodes.SouthAfrica);

					list.Add(ScreeningMethodsZA.Codes.ExplosiveDetectionSystem, ScreeningMethodsZA.Descriptions.ExplosiveDetectionSystem, true, true);
					list.Add(ScreeningMethodsZA.Codes.ExplosivesTraceDetectionEquipment, ScreeningMethodsZA.Descriptions.ExplosivesTraceDetectionEquipment, true, true);
					list.Add(ScreeningMethodsZA.Codes.PhysicalInspectionAndOrHandSearch, ScreeningMethodsZA.Descriptions.PhysicalInspectionAndOrHandSearch, true, true);
					list.Add(ScreeningMethodsZA.Codes.VisualCheck, ScreeningMethodsZA.Descriptions.VisualCheck, true, true);
					list.Add(ScreeningMethodsZA.Codes.XRayEquipment, ScreeningMethodsZA.Descriptions.XRayEquipment, true, true);
					list.Add(ScreeningMethodsZA.Codes.ExplosiveDetectionDogs, ScreeningMethodsZA.Descriptions.ExplosiveDetectionDogs, true, true);
					list.Add(ScreeningMethodsZA.Codes.FreeRunningExplosiveDetectionDogs, ScreeningMethodsZA.Descriptions.FreeRunningExplosiveDetectionDogs, true, true);
					list.Add(ScreeningMethodsZA.Codes.RemoteExplosiveScentTracingExplosiveDetectionDogs, ScreeningMethodsZA.Descriptions.RemoteExplosiveScentTracingExplosiveDetectionDogs, true, true);

					list.Add(ExemptionCodesZA.Codes.DiplomaticBagsOrDiplomaticMail, ExemptionCodesZA.Descriptions.DiplomaticBagsOrDiplomaticMail, true, true);
					list.Add(ExemptionCodesZA.Codes.HumanRemainsOrAshes, ExemptionCodesZA.Descriptions.HumanRemainsOrAshes, true, true);
					list.Add(ExemptionCodesZA.Codes.LifeSavingMaterials, ExemptionCodesZA.Descriptions.LifeSavingMaterials, true, true);
					list.Add(ExemptionCodesZA.Codes.LiveAnimals, ExemptionCodesZA.Descriptions.LiveAnimals, true, true);
					list.Add(ExemptionCodesZA.Codes.NuclearMaterial, ExemptionCodesZA.Descriptions.NuclearMaterial, true, true);
					list.Add(ExemptionCodesZA.Codes.TransferOrTransshipment, ExemptionCodesZA.Descriptions.TransferOrTransshipment, true, true);

					return list;
				});
			}
		}

		public ShipmentInspectionTypeCollection GetCountrySpecificSystemDefinedList(string countryCode)
		{
			switch (countryCode)
			{
				case Constants.CountryCodes.Australia:
					return SystemDefinedList_Australia;

				case Constants.CountryCodes.Canada:
					return SystemDefinedList_Canada;

				case Constants.CountryCodes.EuropeanUnion:
					return SystemDefinedList_EuropeanUnion;

				case Constants.CountryCodes.Japan:
					return SystemDefinedList_Japan;

				case Constants.CountryCodes.HongKong:
					return SystemDefinedList_HongKong;

				case Constants.CountryCodes.Singapore:
					return SystemDefinedList_Singapore;

				case Constants.CountryCodes.SouthAfrica:
					return SystemDefinedList_SouthAfrica;

				default:
					return SystemDefinedList_IATA;
			}
		}

		public static CodeDescriptionPairList GetCountrySpecificScreeningMethods(string countryCode)
		{
			switch (countryCode)
			{
				case Constants.CountryCodes.Australia:
					return new ScreeningMethodsAU();

				case Constants.CountryCodes.Canada:
					return new ScreeningMethodsCA();

				case Constants.CountryCodes.Japan:
					return new ScreeningMethodsJP();

				case Constants.CountryCodes.HongKong:
					return new ScreeningMethodsHK();

				case Constants.CountryCodes.Singapore:
					return new ScreeningMethodsSG();

				case Constants.CountryCodes.SouthAfrica:
					return new ScreeningMethodsZA();

				default:
					return new ScreeningMethods();
			}
		}

		public static CodeDescriptionPairList GetCountrySpecificExemptionCodes(string countryCode)
		{
			if (FallBackToIATAListWhileUnderDevelopment(countryCode))
			{
				return new ExemptionCodes();
			}

			switch (countryCode)
			{
				case Constants.CountryCodes.Australia:
					return new ExemptionCodesAU();

				case Constants.CountryCodes.Canada:
					return new ExemptionCodesCA();

				case Constants.CountryCodes.Japan:
					return new ExemptionCodesJP();

				case Constants.CountryCodes.HongKong:
					return new ExemptionCodesHK();

				case Constants.CountryCodes.Singapore:
					return new ExemptionCodesSG();

				case Constants.CountryCodes.SouthAfrica:
					return new ExemptionCodesZA();

				default:
					return new ExemptionCodes();
			}
		}

		static bool FallBackToIATAListWhileUnderDevelopment(string countryCode)
		{
			var supplyChainSecurityConfiguration = ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfigurationForCountry(countryCode);
			return !supplyChainSecurityConfiguration.IsEnabled && supplyChainSecurityConfiguration.IsOnlyForDevelopers;
		}
	}
}
