using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Freight;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Freight.Integration.Forwarding;
using static Enterprise.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FreightDataRegistry))]
	sealed class FreightDataRegistryTest : RegistryItemSetTestCaseWithFactory<FreightDataRegistry>
	{
		#region Release Types

		public void TestReleaseTypes()
		{
			AssertEquals("Name", "ReleaseTypes", ItemSet.ReleaseTypes.Name);
			AssertEquals("Caption", "Release Types", ItemSet.ReleaseTypes.Caption);
			AssertEquals("Hint", "This list defines the different Release Type options available on a shipment and consolidation. You can add your own items to this list but cannot change code and description of the system defined items.", ItemSet.ReleaseTypes.Hint);
			AssertEquals("Category", "Freight", ItemSet.ReleaseTypes.Category);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.ReleaseTypes.Storage);

			AssertEquals("System List", new CodeDescriptionPairList(OLookUpEditType.ShipmentReleaseType).Count, ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList().Count);

			ReleaseTypes defValues = ItemSet.ReleaseTypes.Value;
			AssertEquals(3, defValues.OriginalsNumber);
			AssertEquals(3, defValues.CopiesNumber);
			foreach (ReleaseType defValue in defValues.Types)
			{
				AssertEquals(true, defValue.SystemDefined);
				if (defValue.Code == Constants.ShipmentReleaseTypes.ExpressBofL)
				{
					AssertEquals(0, defValue.OriginalsNumber);
					AssertEquals(1, defValue.CopiesNumber);
				}
				else if (defValue.Code == Constants.ShipmentReleaseTypes.SeaWaybill)
				{
					AssertEquals(0, defValue.OriginalsNumber);
					AssertEquals(3, defValue.CopiesNumber);
				}
				else
				{
					AssertEquals(3, defValue.OriginalsNumber);
					AssertEquals(3, defValue.CopiesNumber);
				}
			}

			ReleaseTypes newValue = new ReleaseTypes();
			ReleaseType value1 = new ReleaseType();
			value1.Code = "AAA";
			value1.Description = (NoResString)"blah blah blah";

			newValue.Types.Add(value1);

			ItemSet.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			AssertEquals(new CodeDescriptionPairList(OLookUpEditType.ShipmentReleaseType).Count + 1, ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList().Count);
			AssertEquals("AAA", ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList()[0].Code);
			AssertEquals("blah blah blah", ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList()[0].Description);
			AssertEquals(false, ItemSet.ReleaseTypes.Value.Types[0].SystemDefined);

			newValue = ItemSet.ReleaseTypes.DefaultValue;
			value1 = new ReleaseType();
			value1.Code = "AAA";
			value1.Description = (NoResString)"blah blah blah";

			newValue.Types.Add(value1);
			newValue.OriginalsNumber = 4;
			newValue.CopiesNumber = 5;

			ItemSet.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			AssertEquals(4, ItemSet.ReleaseTypes.Value.OriginalsNumber);
			AssertEquals(5, ItemSet.ReleaseTypes.Value.CopiesNumber);
			AssertEquals(new CodeDescriptionPairList(OLookUpEditType.ShipmentReleaseType).Count + 1, ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList().Count);
			AssertEquals("AAA", ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList()[ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList().Count - 1].Code);
			AssertEquals("blah blah blah", ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList()[ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList().Count - 1].Description);
			AssertEquals(false, ItemSet.ReleaseTypes.Value.Types[ItemSet.ReleaseTypes.Value.Types.GetCodeDescriptionPairList().Count - 1].SystemDefined);
			for (int i = 0; i < ItemSet.ReleaseTypes.Value.Types.Count - 1; i++)
			{
				AssertEquals(true, ItemSet.ReleaseTypes.Value.Types[i].SystemDefined);
			}
		}

		public void TestSeralizedReleaseTypesLocalization()
		{
			var releaseTypes = FreightDataRegistry.Instance.ReleaseTypes.Value;
			releaseTypes.Types.Add(new ReleaseType() { Code = "XXX", Description = (NoResString)"Custom Item" });
			FreightDataRegistry.Instance.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, releaseTypes);
			((IRegistryItemInternals)FreightDataRegistry.Instance.ReleaseTypes).ClearCache();
			FreightDataRegistry.Instance.ReleaseTypes.Value.Types.ContainsCode("XXX");
			Assert("Should be some default release types", FreightDataRegistry.Instance.ReleaseTypes.Value.Types.Count > 1);
			const string hao = "好";
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter((string key) => { return new ResourceStringData(key, hao); });
				foreach (ReleaseType releaseType in FreightDataRegistry.Instance.ReleaseTypes.Value.Types)
				{
					if (releaseType.Code != "XXX")
					{
						AssertEquals("Default release type description should be localized", hao, (string)releaseType.Description);
					}
				}
			}
		}

		public void TestReleaseType()
		{
			TestRegistryItem(
				ItemSet.ReleaseType,
				"ReleaseType",
				FreightDataRegistry.Categories.Freight_HouseBills,
				"Release Type",
				"Default Release Type for Shipments",
				RegistryStorageFlags.All,
				ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList(),
				""
			);
		}

		#endregion

		#region Outturn

		#region Outurn Responsible Party ID Override

		public void OuturnResponsiblePartyIDOverrideTest()
		{
			AssertEquals("Name", "OuturnResponsiblePartyIDOverride", ItemSet.OuturnResponsiblePartyIDOverride.Name);
			AssertEquals("Don't auto-deliver by default", "Outurn Responsible Party ID Override", ItemSet.OuturnResponsiblePartyIDOverride.Caption);
			AssertEquals("Storage", "Freight/CFS/Outturn/", ItemSet.OuturnResponsiblePartyIDOverride.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.OuturnResponsiblePartyIDOverride.Storage);
			AssertEquals("Hint", "", ItemSet.OuturnResponsiblePartyIDOverride.Hint);
		}

		#endregion

		#endregion

		#region Supply Chain Security

		#region Shipment Inspection Types

		public void TestShipmentInspectionTypes()
		{
			TestShipmentInspectionTypeRegistryItem(ItemSet.ShipmentInspectionTypeRegistryItem,
				"",
				"ShipmentInspectionTypes",
				"Freight/Supply Chain Security",
				"Default IATA Inspection Types");
		}

		void TestShipmentInspectionTypeRegistryItem(ShipmentInspectionTypeRegistryItem itemToTest, string countryCode, string name, string category, string caption)
		{
			AssertEquals(name, itemToTest.Name);
			AssertEquals(category, itemToTest.Category);
			AssertEquals(caption, itemToTest.Caption);
			AssertEquals(RegistryStorageFlags.System, itemToTest.Storage);
			Assert("Should not Preserve Test Value", !itemToTest.HasOption(RegistryOptions.PreserveTestValue));

			var config = ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfigurationForCountry(countryCode);
			var expectIsOnlyForDevelopers = config.IsOnlyForDevelopers;
			if (expectIsOnlyForDevelopers)
			{
				Assert("Expected to be Developer Only", itemToTest.HasOption(RegistryOptions.IsOnlyForDevelopers));
			}
			else
			{
				Assert("Should not be Developer Only", !itemToTest.HasOption(RegistryOptions.IsOnlyForDevelopers));
			}

			ShipmentInspectionTypeCollection col = itemToTest.Value.Types;
			foreach (ShipmentInspectionType item in col)
			{
				AssertEquals("System defined items can't be deleted", false, ((ICanDelete)item).CanDelete);
				AssertEquals("System defined codes are read only", true, item.CodeInfo.ReadOnly);
				AssertEquals("System defined descriptions are read only", true, item.CodeInfo.ReadOnly);
				AssertEquals("System defined ShowInLists can be edited", false, item.ShowInListInfo.ReadOnly);
				AssertEquals("System defined AllowedOnPassengerFlights can be edited", false, item.AllowedOnPassengerFlightsInfo.ReadOnly);
			}
		}

		#endregion

		#region EXM Exemption Code Removal Date

		public void TestEXMExemptionCodeRemovalDate()
		{
			var expectedHint = "Indicates the date on which the EXM Exemption Code was removed from CW1.\r\n\r\n" +
						"Shipments created before this date that still contain the EXM code will not be affected by validation.";

			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EXMExemptionCodeRemovalDate.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden, ItemSet.EXMExemptionCodeRemovalDate.Options);
			AssertEquals("EXMExemptionCodeRemovalDate", ItemSet.EXMExemptionCodeRemovalDate.Name);
			AssertEquals("Freight/Supply Chain Security", ItemSet.EXMExemptionCodeRemovalDate.Category);
			AssertEquals("Hint", expectedHint, ItemSet.EXMExemptionCodeRemovalDate.Hint);
		}

		#endregion

		#region Shipment Inspection Type Default

		public void TestShipmentInspectionTypeDefault()
		{
			var expectedHint = @"This is the default value for the Shipment and Pack Line Inspection Type, when consignor/shipper is not known/approved.

This registry does not apply to countries with a licensed Supply Chain Security module that have their own configuration for inspection type for unknown organizations.";

			TestShipmentInspectionTypeDefault(
				ItemSet.ShipmentInspectionTypeDefault,
				"ShipmentInspectionTypesDefault",
				"Freight/Supply Chain Security",
				"Inspection Type Default for Unknown Organizations",
				expectedHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		void TestShipmentInspectionTypeDefault(CodePairRegistryItem itemToTest, ZString name, ZString category, ZString caption, ZString hint, RegistryStorageFlags storage)
		{
			AssertEquals(name, itemToTest.Name);
			AssertEquals(category, itemToTest.Category);
			AssertEquals(caption, itemToTest.Caption);
			AssertEquals(hint, itemToTest.Hint);
			AssertEquals(storage, itemToTest.Storage);
		}

		public void TestShipperLoadAndCount()
		{
			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, FreightDataRegistry.Instance.ShipperLoadAndCount.Options);
			}

			RegistryItemDictionary.Instance.PurgeAll();

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.PreserveTestValue, FreightDataRegistry.Instance.ShipperLoadAndCount.Options);
			}
		}

		#endregion

		#region Australia

		public void TestEnableSupplyChainSecurityForAustralia()
		{
			TestRegistryItem(ItemSet.EnableSupplyChainSecurity_AU,
				"EnableSupplyChainSecurity_AU",
				"Freight/Supply Chain Security/Australia",
				"Enable Supply Chain Security for Australia",
				"Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes.",
				RegistryStorageFlags.System,
				true);
		}

		public void TestShipmentInspectionTypeDefault_Australia()
		{
			TestShipmentInspectionTypeDefault(
					ItemSet.ShipmentInspectionTypeDefault_Australia,
					"ShipmentInspectionTypesDefault_AU",
					"Freight/Supply Chain Security/Australia",
					"Inspection Type Default for Unknown Organizations",
					"This is the default value for the Shipment and Pack Line Inspection Type, when consignor/shipper is not known/approved.",
					RegistryStorageFlags.System);
		}

		public void TestShipmentInspectionOrganisationToUse_Australia()
		{
			var itemToTest = ItemSet.ShipmentInspectionOrganisationToUse_Australia;
			AssertEquals("ShipmentInspectionOrganisationToUse_AU", itemToTest.Name);
			AssertEquals("Freight/Supply Chain Security/Australia", itemToTest.Category);
			AssertEquals("Organizations to Use for Supply Chain Security", itemToTest.Caption);
			AssertEquals(RegistryStorageFlags.System, itemToTest.Storage);

			var defaultValueToTest = itemToTest.Value;
			Assert("CON", defaultValueToTest.ContainsCode("CON"));
			Assert("LOC", defaultValueToTest.ContainsCode("LOC"));
			Assert("TRS", defaultValueToTest.ContainsCode("TRS"));
			Assert("CFS", defaultValueToTest.ContainsCode("CFS"));
			Assert("FOR", defaultValueToTest.ContainsCode("FOR"));
			Assert("CAR", defaultValueToTest.ContainsCode("CAR"));
			Assert("TRC", defaultValueToTest.ContainsCode("TRC"));
			Assert("CFC", defaultValueToTest.ContainsCode("CFC"));
			Assert("COL", defaultValueToTest.ContainsCode("COL"));
			Assert("MAW", defaultValueToTest.ContainsCode("MAW"));
			Assert("PCU", defaultValueToTest.ContainsCode("PCU"));

			Assert("AGT should not be included for Australia", !defaultValueToTest.ContainsCode("AGT"));
		}

		public void TestShipmentInspectionTypes_Australia()
		{
			TestShipmentInspectionTypeRegistryItem(ItemSet.ShipmentInspectionTypes_Australia,
				"AU",
				"ShipmentInspectionTypes_AU",
				"Freight/Supply Chain Security/Australia",
				"Inspection Types");
		}

		#endregion

		#region United States

		public void TestEnableSupplyChainSecurityForUnitedStates()
		{
			TestRegistryItem(ItemSet.EnableSupplyChainSecurity_US,
				"EnableSupplyChainSecurity_US",
				"Freight/Supply Chain Security/United States",
				"Enable Supply Chain Security for United States",
				"Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		#endregion

		#region Japan

		public void TestShipmentInspectionTypes_Japan()
		{
			TestShipmentInspectionTypeRegistryItem(ItemSet.ShipmentInspectionTypes_Japan,
				"JP",
				"ShipmentInspectionTypes_JP",
				"Freight/Supply Chain Security/Japan",
				"Inspection Types");
		}

		public void TestShipmentInspectionTypeDefault_Japan()
		{
			TestShipmentInspectionTypeDefault(
				ItemSet.ShipmentInspectionTypeDefault_Japan,
				"ShipmentInspectionTypesDefault_JP",
				"Freight/Supply Chain Security/Japan",
				"Inspection Type Default for Unknown Organizations",
				"This is the default value for the Shipment and Pack Line Inspection Type, when consignor/shipper is not known/approved.",
				RegistryStorageFlags.System);
		}

		public void TestEnableSupplyChainSecurityForJapan()
		{
			TestRegistryItem(ItemSet.EnableSupplyChainSecurity_JP,
				"EnableSupplyChainSecurity_JP",
				"Freight/Supply Chain Security/Japan",
				"Enable Supply Chain Security for Japan",
				"Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes.",
				RegistryStorageFlags.System,
				true);
		}

		#endregion

		#region Hong Kong

		public void TestShipmentInspectionOrganisationToUse_HongKong()
		{
			var itemToTest = ItemSet.ShipmentInspectionOrganisationToUse_HongKong;
			AssertEquals("ShipmentInspectionOrganisationToUse_HK", itemToTest.Name);
			AssertEquals("Freight/Supply Chain Security/Hong Kong", itemToTest.Category);
			AssertEquals("Organizations to Use for Supply Chain Security", itemToTest.Caption);
			AssertEquals(RegistryStorageFlags.System, itemToTest.Storage);

			var defaultValueToTest = itemToTest.Value;
			Assert("CON", defaultValueToTest.ContainsCode("CON"));
			Assert("LOC", defaultValueToTest.ContainsCode("LOC"));
			Assert("TRS", defaultValueToTest.ContainsCode("TRS"));
			Assert("CFS", defaultValueToTest.ContainsCode("CFS"));
			Assert("FOR", defaultValueToTest.ContainsCode("FOR"));
			Assert("AGT", defaultValueToTest.ContainsCode("AGT"));
			Assert("CAR", defaultValueToTest.ContainsCode("CAR"));
			Assert("TRC", defaultValueToTest.ContainsCode("TRC"));
			Assert("CFC", defaultValueToTest.ContainsCode("CFC"));
			Assert("COL", defaultValueToTest.ContainsCode("COL"));
			Assert("MAW", defaultValueToTest.ContainsCode("MAW"));
		}

		public void TestShipmentInspectionTypes_HongKong()
		{
			TestShipmentInspectionTypeRegistryItem(ItemSet.ShipmentInspectionTypes_HongKong,
				"HK",
				"ShipmentInspectionTypes_HK",
				"Freight/Supply Chain Security/Hong Kong",
				"Inspection Types");
		}

		public void TestShipmentInspectionTypeDefault_HongKong()
		{
			TestShipmentInspectionTypeDefault(
				ItemSet.ShipmentInspectionTypeDefault_HongKong,
				"ShipmentInspectionTypesDefault_HK",
				"Freight/Supply Chain Security/Hong Kong",
				"Inspection Type Default for Unknown Organizations",
				"This is the default value for the Shipment and Pack Line Inspection Type, when consignor/shipper is not known/approved.",
				RegistryStorageFlags.System);
		}

		public void TestEnableSupplyChainSecurityForHongKong()
		{
			TestRegistryItem(ItemSet.EnableSupplyChainSecurity_HK,
				"EnableSupplyChainSecurity_HK",
				"Freight/Supply Chain Security/Hong Kong",
				"Enable Supply Chain Security for Hong Kong",
				"Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes.",
				RegistryStorageFlags.System,
				true);
		}

		#endregion

		#region European Union

		public void TestShipmentInspectionTypes_EuropeanUnion()
		{
			TestShipmentInspectionTypeRegistryItem(ItemSet.ShipmentInspectionTypes_EU,
				"EU",
				"ShipmentInspectionTypes_EU",
				"Freight/Supply Chain Security/European Union",
				"Inspection Types");
		}

		public void TestShipmentInspectionOrganisationToUse_EU()
		{
			var itemToTest = ItemSet.ShipmentInspectionOrganisationToUse_EuropeanUnion;
			AssertEquals("ShipmentInspectionOrganisationToUse_EU", itemToTest.Name);
			AssertEquals("Freight/Supply Chain Security/European Union", itemToTest.Category);
			AssertEquals("Organizations to Use for Supply Chain Security", itemToTest.Caption);
			AssertEquals(RegistryStorageFlags.System, itemToTest.Storage);

			var defaultValueToTest = itemToTest.Value;
			Assert("CON", defaultValueToTest.ContainsCode("CON"));
			Assert("LOC", defaultValueToTest.ContainsCode("LOC"));
			Assert("TRS", defaultValueToTest.ContainsCode("TRS"));
			Assert("CFS", defaultValueToTest.ContainsCode("CFS"));
			Assert("FOR", defaultValueToTest.ContainsCode("FOR"));
			Assert("AGT", defaultValueToTest.ContainsCode("AGT"));
			Assert("CAR", defaultValueToTest.ContainsCode("CAR"));
			Assert("TRC", defaultValueToTest.ContainsCode("TRC"));
			Assert("CFC", defaultValueToTest.ContainsCode("CFC"));
			Assert("COL", defaultValueToTest.ContainsCode("COL"));
			Assert("MAW", defaultValueToTest.ContainsCode("MAW"));
			Assert("PCU", defaultValueToTest.ContainsCode("PCU"));

			Assert("IsOnlyForDevelopers is false", !itemToTest.HasOption(RegistryOptions.IsOnlyForDevelopers));
		}

		public void TestEnableSupplyChainSecurityForEuropeanUnion()
		{
			TestRegistryItem(ItemSet.EnableSupplyChainSecurity_EU,
				"EnableSupplyChainSecurity_EU",
				"Freight/Supply Chain Security/European Union",
				"Enable Supply Chain Security for the European Union",
				"Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				true);
		}

		public void TestAviationSecurityTrainingRestrictionsForEuropeanUnion()
		{
			var itemToTest = ItemSet.AviationSecurityTrainingRestrictions_EU;
			AssertEquals("AviationSecurityTrainingRestrictions_EU", itemToTest.Name);
			AssertEquals("Freight/Supply Chain Security/European Union", itemToTest.Category);
			AssertEquals("Aviation Security Training Restrictions for the European Union", itemToTest.Caption);
			AssertEquals("Override this registry to configure certification restrictions for editing the Shipment and Consolidation fields related to aviation security, as well as for movement of restricted documents issued from air export jobs from EU, Iceland, Switzerland, Norway or Liechtenstein, and organizations located in these countries.", itemToTest.Hint);
			AssertEquals(RegistryStorageFlags.System, itemToTest.Storage);

			var defaultValueToTest = itemToTest.Value;
			AssertEquals(true, defaultValueToTest.Enabled);
			AssertEquals(false, defaultValueToTest.ApplyCertificationRestriction);
		}

		#endregion

		#region United Kingdom

		public void TestShipmentInspectionOrganisationToUse_UK()
		{
			var itemToTest = ItemSet.ShipmentInspectionOrganisationToUse_UK;
			AssertEquals("ShipmentInspectionOrganisationToUse_UK", itemToTest.Name);
			AssertEquals("Freight/Supply Chain Security/United Kingdom", itemToTest.Category);
			AssertEquals("Organizations to Use for Supply Chain Security", itemToTest.Caption);
			AssertEquals(RegistryStorageFlags.System, itemToTest.Storage);

			var defaultValueToTest = itemToTest.Value;
			Assert("CON", defaultValueToTest.ContainsCode("CON"));
			Assert("LOC", defaultValueToTest.ContainsCode("LOC"));
			Assert("TRS", defaultValueToTest.ContainsCode("TRS"));
			Assert("CFS", defaultValueToTest.ContainsCode("CFS"));
			Assert("FOR", defaultValueToTest.ContainsCode("FOR"));
			Assert("AGT", defaultValueToTest.ContainsCode("AGT"));
			Assert("CAR", defaultValueToTest.ContainsCode("CAR"));
			Assert("TRC", defaultValueToTest.ContainsCode("TRC"));
			Assert("CFC", defaultValueToTest.ContainsCode("CFC"));
			Assert("COL", defaultValueToTest.ContainsCode("COL"));
			Assert("MAW", defaultValueToTest.ContainsCode("MAW"));
			Assert("PCU", defaultValueToTest.ContainsCode("PCU"));

			Assert("IsOnlyForDevelopers is false", !itemToTest.HasOption(RegistryOptions.IsOnlyForDevelopers));
		}

		public void TestEnableSupplyChainSecurityForUK()
		{
			TestRegistryItem(ItemSet.EnableSupplyChainSecurity_UK,
				"EnableSupplyChainSecurity_UK",
				"Freight/Supply Chain Security/United Kingdom",
				"Enable Supply Chain Security for United Kingdom",
				"Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				true);
		}

		#endregion

		#region Singapore

		public void TestEnableSupplyChainSecurityForSingapore()
		{
			TestRegistryItem(ItemSet.EnableSupplyChainSecurity_SG,
				"EnableSupplyChainSecurity_SG",
				"Freight/Supply Chain Security/Singapore",
				"Enable Supply Chain Security for Singapore",
				"Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes.",
				RegistryStorageFlags.System,
				true);
		}

		#endregion

		#region South Africa

		public void TestShipmentInspectionTypes_SouthAfrica()
		{
			TestShipmentInspectionTypeRegistryItem(ItemSet.ShipmentInspectionTypes_SouthAfrica,
				"ZA",
				"ShipmentInspectionTypes_ZA",
				"Freight/Supply Chain Security/South Africa",
				"Inspection Types");
		}

		public void TestDefaultPackageScreeningMethodsZA()
		{
			var systemDefinedTypes = ItemSet.ShipmentInspectionTypes_SouthAfrica.DefaultValue.SystemDefinedTypes;
			AssertEquals("SystemDefinedTypes.Count", 14, systemDefinedTypes.Count);
			AssertEquals(ScreeningMethodsZA.Codes.XRayEquipment, systemDefinedTypes.GetCodeFromDescription(ScreeningMethodsZA.Descriptions.XRayEquipment));
			AssertEquals(ScreeningMethodsZA.Codes.ExplosiveDetectionSystem, systemDefinedTypes.GetCodeFromDescription(ScreeningMethodsZA.Descriptions.ExplosiveDetectionSystem));
			AssertEquals(ScreeningMethodsZA.Codes.ExplosivesTraceDetectionEquipment, systemDefinedTypes.GetCodeFromDescription(ScreeningMethodsZA.Descriptions.ExplosivesTraceDetectionEquipment));
			AssertEquals(ScreeningMethodsZA.Codes.PhysicalInspectionAndOrHandSearch, systemDefinedTypes.GetCodeFromDescription(ScreeningMethodsZA.Descriptions.PhysicalInspectionAndOrHandSearch));
			AssertEquals(ScreeningMethodsZA.Codes.VisualCheck, systemDefinedTypes.GetCodeFromDescription(ScreeningMethodsZA.Descriptions.VisualCheck));
			AssertEquals(ScreeningMethodsZA.Codes.ExplosiveDetectionDogs, systemDefinedTypes.GetCodeFromDescription(ScreeningMethodsZA.Descriptions.ExplosiveDetectionDogs));
			AssertEquals(ScreeningMethodsZA.Codes.FreeRunningExplosiveDetectionDogs, systemDefinedTypes.GetCodeFromDescription(ScreeningMethodsZA.Descriptions.FreeRunningExplosiveDetectionDogs));
			AssertEquals(ScreeningMethodsZA.Codes.RemoteExplosiveScentTracingExplosiveDetectionDogs, systemDefinedTypes.GetCodeFromDescription(ScreeningMethodsZA.Descriptions.RemoteExplosiveScentTracingExplosiveDetectionDogs));

			AssertEquals(ExemptionCodesZA.Codes.DiplomaticBagsOrDiplomaticMail, systemDefinedTypes.GetCodeFromDescription(ExemptionCodesZA.Descriptions.DiplomaticBagsOrDiplomaticMail));
			AssertEquals(ExemptionCodesZA.Codes.HumanRemainsOrAshes, systemDefinedTypes.GetCodeFromDescription(ExemptionCodesZA.Descriptions.HumanRemainsOrAshes));
			AssertEquals(ExemptionCodesZA.Codes.LifeSavingMaterials, systemDefinedTypes.GetCodeFromDescription(ExemptionCodesZA.Descriptions.LifeSavingMaterials));
			AssertEquals(ExemptionCodesZA.Codes.LiveAnimals, systemDefinedTypes.GetCodeFromDescription(ExemptionCodesZA.Descriptions.LiveAnimals));
			AssertEquals(ExemptionCodesZA.Codes.NuclearMaterial, systemDefinedTypes.GetCodeFromDescription(ExemptionCodesZA.Descriptions.NuclearMaterial));
			AssertEquals(ExemptionCodesZA.Codes.TransferOrTransshipment, systemDefinedTypes.GetCodeFromDescription(ExemptionCodesZA.Descriptions.TransferOrTransshipment));
		}

		#endregion

		#region Taiwan

		public void TestEnableSupplyChainSecurityForTaiwan()
		{
			TestGenericRegistryItem(ItemSet.EnableSupplyChainSecurity_TW,
				"EnableSupplyChainSecurity_TW",
				"Freight/Supply Chain Security/Taiwan",
				"Enable Supply Chain Security for Taiwan",
				"Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country’s air export processes.",
				RegistryStorageFlags.System,
				true);
		}

		public void TestAgentsRestrictedArticlesNumber()
		{
			TestGenericRegistryItem(ItemSet.RegulatedAgentNumber_TW,
				"RegulatedAgentNumber_TW",
				"Freight/Supply Chain Security/Taiwan",
				"Regulated Agent Number",
				"Agent is approved to handle restricted articles",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				"");
		}

		#endregion

		#region Canada

		public void TestEnableSupplyChainSecurityForCanada()
		{
			TestRegistryItem(ItemSet.EnableSupplyChainSecurity_CA,
				"EnableSupplyChainSecurity_CA",
				"Freight/Supply Chain Security/Canada",
				"Enable Supply Chain Security for Canada",
				"Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country’s air export processes.",
				RegistryStorageFlags.System,
				true);
		}

		public void TestShipmentInspectionTypes_Canada()
		{
			TestShipmentInspectionTypeRegistryItem(ItemSet.ShipmentInspectionTypes_Canada,
				"CA",
				"ShipmentInspectionTypes_CA",
				"Freight/Supply Chain Security/Canada",
				"Inspection Types");
		}

		public void TestShipmentInspectionOrganisationToUse_Canada()
		{
			var itemToTest = ItemSet.ShipmentInspectionOrganisationToUse_Canada;
			AssertEquals("ShipmentInspectionOrganisationToUse_CA", itemToTest.Name);
			AssertEquals("Freight/Supply Chain Security/Canada", itemToTest.Category);
			AssertEquals("Organizations to Use for Supply Chain Security", itemToTest.Caption);
			AssertEquals(RegistryStorageFlags.System, itemToTest.Storage);

			var defaultValueToTest = itemToTest.Value;
			Assert("CON", defaultValueToTest.ContainsCode("CON"));
			Assert("LOC", defaultValueToTest.ContainsCode("LOC"));
			Assert("TRS", defaultValueToTest.ContainsCode("TRS"));
			Assert("CFS", defaultValueToTest.ContainsCode("CFS"));
			Assert("FOR", defaultValueToTest.ContainsCode("FOR"));
			Assert("AGT", defaultValueToTest.ContainsCode("AGT"));
			Assert("CAR", defaultValueToTest.ContainsCode("CAR"));
			Assert("TRC", defaultValueToTest.ContainsCode("TRC"));
			Assert("CFC", defaultValueToTest.ContainsCode("CFC"));
			Assert("COL", defaultValueToTest.ContainsCode("COL"));
			Assert("MAW", defaultValueToTest.ContainsCode("MAW"));
			Assert("PCU", defaultValueToTest.ContainsCode("PCU"));
		}

		#endregion

		#region South Africa

		public void TestEnableSupplyChainSecurityForSouthAfrica()
		{
			TestRegistryItem(ItemSet.EnableSupplyChainSecurity_ZA,
				"EnableSupplyChainSecurity_ZA",
				"Freight/Supply Chain Security/South Africa",
				"Enable Supply Chain Security for South Africa",
				"Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country’s air export processes.",
				RegistryStorageFlags.System,
				true);
		}

		public void TestShipmentInspectionOrganisationToUse_SouthAfrica()
		{
			var itemToTest = ItemSet.ShipmentInspectionOrganisationToUse_SouthAfrica;
			AssertEquals("ShipmentInspectionOrganisationToUse_ZA", itemToTest.Name);
			AssertEquals("Freight/Supply Chain Security/South Africa", itemToTest.Category);
			AssertEquals("Organizations to Use for Supply Chain Security", itemToTest.Caption);
			AssertEquals(RegistryStorageFlags.System, itemToTest.Storage);
			AssertEquals(RegistryOptions.Default, itemToTest.Options);

			var defaultValueToTest = itemToTest.Value;
			Assert("CON", defaultValueToTest.ContainsCode("CON"));
			Assert("LOC", defaultValueToTest.ContainsCode("LOC"));
			Assert("TRS", defaultValueToTest.ContainsCode("TRS"));
			Assert("CFS", defaultValueToTest.ContainsCode("CFS"));
			Assert("FOR", defaultValueToTest.ContainsCode("FOR"));
			Assert("AGT", defaultValueToTest.ContainsCode("AGT"));
			Assert("CAR", defaultValueToTest.ContainsCode("CAR"));
			Assert("TRC", defaultValueToTest.ContainsCode("TRC"));
			Assert("CFC", defaultValueToTest.ContainsCode("CFC"));
			Assert("COL", defaultValueToTest.ContainsCode("COL"));
			Assert("MAW", defaultValueToTest.ContainsCode("MAW"));
			Assert("PCU", defaultValueToTest.ContainsCode("PCU"));
		}

		#endregion

		public void TestShipmentInspectionOrganisationToUse()
		{
			string expectedHint = "This Registry allows you to configure at a Company level, which organization, the Consignor/Shipper or the Local Client from the Billing tab, should be used for the calculation of Known/Approved security inspection status on Shipments.\r\n\r\n" +
					"This registry does not apply to countries with a licensed Supply Chain Security module that have their own configuration for \"Organizations to Use for Supply Chain Security\".";

			AssertEquals("ShipmentInspectionOrganisationToUse", ItemSet.ShipmentInspectionOrganisationToUse.Name);
			AssertEquals("Freight/Supply Chain Security", ItemSet.ShipmentInspectionOrganisationToUse.Category);
			AssertEquals("Organizations To Use for Supply Chain Security", ItemSet.ShipmentInspectionOrganisationToUse.Caption);
			AssertMultilineASCIIEquals("Expected hint", expectedHint, ItemSet.ShipmentInspectionOrganisationToUse.Hint);

			AssertEquals("Default Value", "CON", ItemSet.ShipmentInspectionOrganisationToUse.DefaultValue);
			ComboBoxRegistryEditorInfo info = (ComboBoxRegistryEditorInfo)ItemSet.ShipmentInspectionOrganisationToUse.EditorInfo;
			AssertEquals("CON, LOC", info.LookUpList.CodesAsString);
		}

		public void TestApprovedOrganisationRequiredDocType()
		{
			AssertEquals("Default Required Document Type for Approved Organization", ItemSet.ApprovedOrganisationRequiredDocType.Caption);
			AssertEquals("Select the Document Type that must be attached to an organization before flagging it as Known/Approved.", ItemSet.ApprovedOrganisationRequiredDocType.Hint);
		}

		#endregion

		#region Shipment

		public void TestShipperCODPaymentTypes()
		{
			Guid uSCompanyPK = Guid.NewGuid();
			Guid aUCompanyPK = Guid.NewGuid();

			string currencyNK = (string)Db.Connection.ExecuteScalar("select top 1 RX_Code from dbo.RefCurrency");
			Db.Connection.ExecuteNonQuery("insert into dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('" + uSCompanyPK + "', 'RRR', 'US company', '" + Core.Constants.CountryCodes.UnitedStates + "',  '" + currencyNK + "')");
			Db.Connection.ExecuteNonQuery("insert into dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('" + aUCompanyPK + "', 'YYY', 'AU company', '" + Core.Constants.CountryCodes.Australia + "',  '" + currencyNK + "')");

			ReadOnlyCodeDescriptionPairList uSList = ItemSet.ShipperCODPaymentTypes.GetValueWithoutFallback(uSCompanyPK, Guid.Empty, Guid.Empty);
			ReadOnlyCodeDescriptionPairList aUList = ItemSet.ShipperCODPaymentTypes.GetValueWithoutFallback(aUCompanyPK, Guid.Empty, Guid.Empty);

			AssertEquals(3, uSList.Count);
			AssertEquals(2, aUList.Count);

			AssertEquals(2, ItemSet.ShipperCODPaymentTypes.Categories.Length);
			AssertEquals(FreightDataRegistry.Categories.Freight_Shipment, ItemSet.ShipperCODPaymentTypes.Categories[0]);
			AssertEquals(WarehouseDataRegistry.Categories.Warehouse_Freight, ItemSet.ShipperCODPaymentTypes.Categories[1]);

			AssertEquals(true, ItemSet.ShipperCODPaymentTypes.IsTranslatable);
			AssertContainsExactElementsInAnyOrder(new MultilingualString[] {
				(NoResString)"Company Check",
				(NoResString)"Certified Check",
				(NoResString)"Cashier's Check",
				(NoResString)"Bank Check"
			}, ItemSet.ShipperCODPaymentTypes.DefaultStrings);
		}

		[TestDate(2019, 12, 10)]
		public void TestCourierIncoTerm_Before2020()
		{
			CodeDescriptionPairList expectedLookUpList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
			TestRegistryItem(ItemSet.CourierIncoTerm, "CourierIncoTerm", "Freight/Incoterms", "Courier Incoterm", "Default a Courier specific Incoterm to display on all courier manifest shipments.", RegistryStorageFlags.BranchDepartment, RegistryOptions.PreserveTestValue, expectedLookUpList, Constants.IncoTerms.DeliveredAtPlace);
		}

		[TestDate(2020, 1, 1)]
		public void TestCourierIncoTerm_After2020()
		{
			CodeDescriptionPairList expectedLookUpList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
			TestRegistryItem(ItemSet.CourierIncoTerm, "CourierIncoTerm", "Freight/Incoterms", "Courier Incoterm", "Default a Courier specific Incoterm to display on all courier manifest shipments.", RegistryStorageFlags.BranchDepartment, RegistryOptions.PreserveTestValue, expectedLookUpList, Constants.IncoTerms.DeliveredAtPlaceUnloaded);
		}

		public void TestAllowAutoCalculationOfInternationalTax()
		{
			AssertEquals("Default", false, ItemSet.AllowAutoCalculationOfInternationalTax.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Allow Auto Calculation of Tax for International AWBs", ItemSet.AllowAutoCalculationOfInternationalTax.Caption);
			ItemSet.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Freight/AWB", ItemSet.AllowAutoCalculationOfInternationalTax.Category);
			AssertEquals(true, ItemSet.AllowAutoCalculationOfInternationalTax.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDefaultRateOriginDestination()
		{
			TestRegistryItem(
				ItemSet.DefaultRateOriginDestination,
				"DefaultRateOriginDestination",
				FreightDataRegistry.Categories.Freight_Shipment,
				"Default Rate Origin/Destination",
				"Enable this option to default values of Rate Origin and Rate Destination fields from the Related City/Port of the corresponding Shipment > Pickup > CFS and Shipment > Delivery > CFS addresses.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				false
			);
		}

		#region Delivery Due Date

		static IDisposable MockDeliveryDueDateFeatureControl(bool enabled = true)
		{
			var mock = new Mock<IDeliveryDueDateFeatureControlHelper>();
			mock.Setup(m => m.Enabled).Returns(enabled);
			return ObjectFactory.Substitute(mock.Object);
		}

		public void TestEnableShipmentCalculateDeliveryDueDate()
		{
			TestGenericRegistryItem(
				ItemSet.CalculateDeliveryDueDateByTransportMode,
				"CalculateDeliveryDueDateByTransportMode",
				FreightDataRegistry.Categories.Freight_Shipment_DeliveryDueDate,
				"Calculate Delivery Due Date",
				"Enable Delivery Due Date calculation on Bookings and Shipments.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				new CalculateDeliveryDueDateOptions(new CalculateDeliveryDueDateTransportModeList(RegistryFactory.Instance).GetDefaultCalculateDeliveryDueDateOptions()) { IsActive = false }
			);
		}

		public void TestCalculateDeliveryDueDateByTransportMode_DeliveryDueDateFeatureEnabled()
		{
			using (MockDeliveryDueDateFeatureControl(true))
			{
				TestGenericRegistryItem(
					ItemSet.CalculateDeliveryDueDateByTransportMode,
					"CalculateDeliveryDueDateByTransportMode",
					FreightDataRegistry.Categories.Freight_Shipment_DeliveryDueDate,
					"Calculate Delivery Due Date",
					"Enable Delivery Due Date calculation on Bookings and Shipments.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					new CalculateDeliveryDueDateOptions(new CalculateDeliveryDueDateTransportModeList(RegistryFactory.Instance).GetDefaultCalculateDeliveryDueDateOptions()) { IsActive = false }
				);
			}
		}

		public void TestCalculateDeliveryDueDateByTransportMode_DeliveryDueDateFeatureDisabled()
		{
			using (MockDeliveryDueDateFeatureControl(false))
			{
				TestGenericRegistryItem(
					ItemSet.CalculateDeliveryDueDateByTransportMode,
					"CalculateDeliveryDueDateByTransportMode",
					FreightDataRegistry.Categories.Freight_Shipment_DeliveryDueDate,
					"Calculate Delivery Due Date",
					"Enable Delivery Due Date calculation on Bookings and Shipments.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					new CalculateDeliveryDueDateOptions(new CalculateDeliveryDueDateTransportModeList(RegistryFactory.Instance).GetDefaultCalculateDeliveryDueDateOptions()) { IsActive = false }
				);
			}
		}

		public void TestEnableShipmentCalculateDeliveryDueDateValidation()
		{
			AssertEquals("CalculateDeliveryDateWithExceptionsOptionsRegistryItemDataType.DefaultValue",
				new CalculateDeliveryDueDateOptions(new CalculateDeliveryDueDateTransportModeList(RegistryFactory.Instance).GetDefaultCalculateDeliveryDueDateOptions()) { IsActive = false }, ItemSet.CalculateDeliveryDueDateByTransportMode.DefaultValue);

			AssertEquals("Expected error", "When the Registry for calculation of Delivery Due Date is enabled, at least one Transport Mode must be selected.",
				ItemSet.CalculateDeliveryDueDateByTransportMode.GetValidationErrorMessage(new CalculateDeliveryDueDateOptions(GetDefaultCalculateDeliveryDueDateOptionsForTest()) { IsActive = true }, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		CalculateDeliveryDueDateTransportModeCollection GetDefaultCalculateDeliveryDueDateOptionsForTest()
		{
			var result = new CalculateDeliveryDueDateTransportModeCollection();
			result.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, false);
			result.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, false);
			result.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, false);
			result.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, false);
			return result;
		}

		public void TestCalculateDeliveryDateWithExceptions()
		{
			TestGenericRegistryItem(
				ItemSet.CalculateDeliveryDateWithExceptions,
				"CalculateDeliveryDateWithExceptions",
				FreightDataRegistry.Categories.Freight_Shipment_DeliveryDueDate,
				"Calculate Delivery Date with Exceptions",
				"Override this registry to specify the maximum combined delay duration per calendar day.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 0, UnlimitedDuration = true });
		}

		public void TestCalculateDeliveryDateWithExceptions_DeliveryDueDateFeatureEnabled()
		{
			using (MockDeliveryDueDateFeatureControl(true))
			{
				TestGenericRegistryItem(
					ItemSet.CalculateDeliveryDateWithExceptions,
					"CalculateDeliveryDateWithExceptions",
					FreightDataRegistry.Categories.Freight_Shipment_DeliveryDueDate,
					"Calculate Delivery Date with Exceptions",
					"Override this registry to specify the maximum combined delay duration per calendar day.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default,
					new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 0, UnlimitedDuration = true }
				);
			}
		}

		public void TestCalculateDeliveryDateWithExceptions_DeliveryDueDateFeatureDisabled()
		{
			using (MockDeliveryDueDateFeatureControl(false))
			using (CWNextFeatureTestHelper.DisableCWNext())
			{
				TestGenericRegistryItem(
					ItemSet.CalculateDeliveryDateWithExceptions,
					"CalculateDeliveryDateWithExceptions",
					FreightDataRegistry.Categories.Freight_Shipment_DeliveryDueDate,
					"Calculate Delivery Date with Exceptions",
					"Override this registry to specify the maximum combined delay duration per calendar day.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 0, UnlimitedDuration = true }
				);
			}
		}

		public void TestCalculateDeliveryDateWithExceptionsBoundryInputValue()
		{
			AssertEquals("CalculateDeliveryDateWithExceptionsOptionsRegistryItemDataType.DefaultValue",
				new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 0, UnlimitedDuration = true }, ItemSet.CalculateDeliveryDateWithExceptions.DefaultValue);

			AssertEquals("Expected error", "Value must be greater than or equal to the minimum (0)",
				ItemSet.CalculateDeliveryDateWithExceptions.GetValidationErrorMessage(new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = -1 }, Guid.Empty, Guid.Empty, Guid.Empty));

			AssertEquals("Expected error", "Value must be less than or equal to the maximum (24)",
				ItemSet.CalculateDeliveryDateWithExceptions.GetValidationErrorMessage(new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 25 }, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		#endregion

		public void TestDefaultShipmentOriginFromConsolLoad()
		{
			AssertEquals("DefaultValue", false, ItemSet.DefaultShipmentOriginFromConsolLoad.DefaultValue);

			ItemSet.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, true);
			AssertEquals("Value", true, ItemSet.DefaultShipmentOriginFromConsolLoad.Value);
		}

		public void TestDefaultShipmentDestinationFromConsolDischarge()
		{
			AssertEquals("DefaultValue", false, ItemSet.DefaultShipmentDestinationFromConsolDischarge.DefaultValue);

			ItemSet.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, true);
			AssertEquals("Value", true, ItemSet.DefaultShipmentDestinationFromConsolDischarge.Value);
		}

		public void TestStorageNumberButtonActivation()
		{
			AssertEquals("DefaultValue", false, ItemSet.StorageNumberButtonActivation.DefaultValue);

			ItemSet.StorageNumberButtonActivation.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, true);
			AssertEquals("Value", true, ItemSet.StorageNumberButtonActivation.Value);
		}

		public void TestStorageNumberCustomisation()
		{
			AssertEquals(RegistryStorageFlags.All, ItemSet.StorageNumberCustomisation.Storage);

			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();

			customisation.RemoveFountainPrefix = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";

			ItemSet.StorageNumberCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisation);

			BillOfLadingNumberCustomisation value = ItemSet.StorageNumberCustomisation.Value;

			AssertEquals("Value.RemoveSPrefix", true, value.RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Default, value.Categories);
			AssertEquals("Value.AllowNonAlphanumericCharacters", true, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", false, value.EnableMacroInsertion);
		}

		public void TestDefaultDeliveryWhenDelayIsNotSet()
		{
			TestRegistryItem(ItemSet.DefaultDeliveryWhenDelayIsNotSet, "DefaultDeliveryWhenDelayIsNotSet", "Freight/Shipment", "Default Delivery When Delay Is Not Set", "The Estimated Delivery on Shipments and Declarations is set from the Est. Delay Until Delivery on the Port Default Delivery Time record.  When yes, the Estimated Delivery will always default from the Port Default Delivery Time record.  When no, the Estimated Delivery will not default from the Port Default Delivery Time record if the Est. Delay Until Delivery is 0.", RegistryStorageFlags.Branch | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, true);
		}

		public void TestShowSubHouseBills()
		{
			TestRegistryItem(
				ItemSet.ShowSubHouseBills,
				"ShowSubHouseBills",
				"Freight/Consolidations",
				"Show Sub House Bills",
				"Specify whether you would like, by default, to see sub house bills from the consol screen",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				false
			);
		}

		public void TestEnableSendingForwardingConsolToTWHLogSubscriber()
		{
			TestRegistryItem(
				ItemSet.EnableSendingForwardingConsolToTWHAsynchronously,
				"EnableSendingForwardingConsolToTWHAsynchronously",
				"Freight/Consolidations",
				"Enable Sending Forwarding Consol to Transit Warehouse Asynchronously",
				"Specify the default value for sending forwarding consol to Transit Warehouse Asynchronously.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true
			);
		}

		public void TestHouseBillOfLadingTypesDefaultValue()
		{
			TestGenericRegistryItem(ItemSet.HouseBillOfLadingTypesForSea, "HouseBillOfLadingTypes", "Freight/House Bills/House Bill of Lading Types", "For Sea", string.Empty, RegistryStorageFlags.Company | RegistryStorageFlags.System);

			var defaultValue = ItemSet.HouseBillOfLadingTypesForSea.DefaultValue;
			AssertEquals("DefaultValue.Count", 15, defaultValue.Count);
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"IAU\")", "IT Club Australia", defaultValue.GetDescriptionFromCode("IAU"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"ITP\")", "IT Club Australia Preprinted", defaultValue.GetDescriptionFromCode("ITP"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"INZ\")", "IT Club New Zealand", defaultValue.GetDescriptionFromCode("INZ"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"INP\")", "IT Club New Zealand Preprinted", defaultValue.GetDescriptionFromCode("INP"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"TNZ\")", "TT Club / Australia / NZ", defaultValue.GetDescriptionFromCode("TNZ"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"TTP\")", "TT Club / Australia / NZ Preprinted", defaultValue.GetDescriptionFromCode("TTP"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"FIA\")", "FIATA HBL", defaultValue.GetDescriptionFromCode("FIA"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"FIP\")", "FIATA HBL Preprinted", defaultValue.GetDescriptionFromCode("FIP"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"TAN\")", "TAN HBL", defaultValue.GetDescriptionFromCode("TAN"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"TAP\")", "TAN HBL Preprinted", defaultValue.GetDescriptionFromCode("TAP"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"EAG\")", "CargoWise Bill", defaultValue.GetDescriptionFromCode("EAG"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"EAP\")", "CargoWise Bill Preprinted", defaultValue.GetDescriptionFromCode("EAP"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"DHK\")", "DataHawk Bill", defaultValue.GetDescriptionFromCode("DHK"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"TUS\")", "TT Club United States", defaultValue.GetDescriptionFromCode("TUS"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"TUP\")", "TT Club United States Preprinted", defaultValue.GetDescriptionFromCode("TUP"));
			AssertEquals(string.Empty, ItemSet.HouseBillOfLadingTypesForSea.DefaultValue.DefaultCode);
		}

		public void TestHouseBillOfLadingTypesForRailDefaultValue()
		{
			TestGenericRegistryItem(ItemSet.HouseBillOfLadingTypesForRail, "HouseBillOfLadingTypesForRail", "Freight/House Bills/House Bill of Lading Types", "For Rail", string.Empty, RegistryStorageFlags.Company | RegistryStorageFlags.System);

			var defaultValue = ItemSet.HouseBillOfLadingTypesForRail.DefaultValue;
			AssertEquals("DefaultValue.Count", 2, defaultValue.Count);
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"FIA\")", "FIATA HBL", defaultValue.GetDescriptionFromCode("FIA"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"EAG\")", "CargoWise Bill", defaultValue.GetDescriptionFromCode("EAG"));
			AssertEquals(string.Empty, ItemSet.HouseBillOfLadingTypesForRail.DefaultValue.DefaultCode);
		}

		public void TestHouseBillOfLadingTypesForRoadDefaultValue()
		{
			TestGenericRegistryItem(ItemSet.HouseBillOfLadingTypesForRoad, "HouseBillOfLadingTypesForRoad", "Freight/House Bills/House Bill of Lading Types", "For Road", string.Empty, RegistryStorageFlags.Company | RegistryStorageFlags.System);

			var defaultValue = ItemSet.HouseBillOfLadingTypesForRoad.DefaultValue;
			AssertEquals("DefaultValue.Count", 3, defaultValue.Count);
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"FIA\")", "FIATA HBL", defaultValue.GetDescriptionFromCode("FIA"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"EAG\")", "CargoWise Bill", defaultValue.GetDescriptionFromCode("EAG"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"CPT\")", "Carta Porte - Spanish", defaultValue.GetDescriptionFromCode("CPT"));
			AssertEquals(string.Empty, ItemSet.HouseBillOfLadingTypesForRoad.DefaultValue.DefaultCode);
		}

		public void TestEnableGoodsValueForShippingInstruction()
		{
			TestRegistryItem(ItemSet.EnableGoodsValueForShippingInstruction,
				"EnableGoodsValueForShippingInstruction",
				"Freight/Consolidations/Ocean Carrier Messaging",
				"Shipping Instruction Goods Value",
				"Override this setting if you would like to remove Goods Value from Carrier Shipping Instruction. Do you want to show Goods Value in Shipping Instruction?",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		public void TestSCMTREnableDate()
		{
			TestGenericRegistryItem(ItemSet.SCMTREnableDate,
				"SCMTREnableDate",
				"Freight/Consolidations/Ocean Carrier Messaging",
				"Enable SCMTR Date",
				"This registry setting controls when the India Customs' Sea Cargo Manifest and Transshipment Regulations (SCMTR) will take effect. After this date the validations will be added to the Shipping Instruction for cargo originating from, destined to, or transiting through India.",
				RegistryStorageFlags.System,
				new DateTime(2022, 1, 1, 0, 0, 0));
		}

		public void TestEnablePackageGrouping()
		{
			AssertEquals(true, FreightDataRegistry.Instance.EnablePackageGrouping.Value);

			if (ZDateTime.Today > new ZDateTime(2025, 01, 31))
			{
				AssertEnablePackageGrouping("EnablePackageGroupingV2", RegistryOptions.IsReadOnly);
			}
			else
			{
				AssertEnablePackageGrouping("EnablePackageGrouping", RegistryOptions.Default);
			}

			void AssertEnablePackageGrouping(string expectedName, RegistryOptions options)
			{
				TestRegistryItem(
					ItemSet.EnablePackageGrouping,
					expectedName,
					"Freight/Consolidations/Ocean Carrier Messaging",
					"Enable Package Grouping",
					@"Enable package grouping functionality on Booking Request, Shipping Order (China) & Shipping Instruction messages.

					Note: Overriding this registry to 'No' may result in rejection of your electronic messages.
					This registry will be set to 'Yes' and read-only from 31/01/2025.",
					RegistryStorageFlags.System,
					options,
					true
				);
			}
		}

		public void TestAllowCompanyTaxIDOverride()
		{
			TestRegistryItem(
				ItemSet.AllowCompanyTaxIDOverride,
				"AllowCompanyTaxIDOverride",
				"Freight/Consolidations/Ocean Carrier Messaging",
				"Allow Company Tax ID Override",
				"Enable this setting to allow company Tax ID override on the Shipping Instruction form.\r\nImportant: invalid override may result in a rejection from the carrier.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false
			);
		}

		public void TestEnableOceanCarrierMessagingConnectionValidation()
		{
			TestRegistryItem(
				ItemSet.EnableOceanCarrierMessagingConnectionValidation,
				"EnableOceanCarrierMessagingConnectionValidation",
				"Freight/Consolidations/Ocean Carrier Messaging",
				"Enable Ocean Carrier Messaging Connection Validation",
				"Override this setting, if you would like to enable ocean carrier messaging connection validation.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				true
			);
		}

		public void TestEnableCarrierMessagingConnectionValidationTestUrl()
		{
			TestRegistryItem(
				ItemSet.EnableCarrierMessagingConnectionValidationTestUrl,
				"EnableCarrierMessagingConnectionValidationTestUrl",
				"Freight/Consolidations/Ocean Carrier Messaging",
				"Enable Ocean Carrier Messaging Connection Test Url",
				"Override this setting to choose between production/test Ocean Carrier Messaging Connection Url.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false
			);
		}

		public void TestCarrierMessagingConnectionValidationTimeout()
		{
			TestRegistryItem(
				ItemSet.CarrierMessagingConnectionValidationTimeout,
				"CarrierMessagingConnectionValidationTimeout",
				"Freight/Consolidations/Ocean Carrier Messaging",
				"The timeout in seconds for Ocean Carrier Route Validation service",
				"Override this setting to set timeout when connecting to Ocean Carrier Route Validation service.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				30,
				5,
				100
			);
		}

		[ExpectNoExceptions]
		public void TestHouseBillOfLadingTypeForRoad_DefaultNotMandatory()
		{
			var item = ItemSet.HouseBillOfLadingTypesForRoad;

			var list = new CodeDescriptionPairList();
			list.AddPair("TY1", "Type 1");
			list.AddPair("TY2", "Type 2");

			var typesWithDefault = new SystemDefinableCodeDescriptionBoolCollection(3, list, true);

			using (item.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, typesWithDefault))
			{
			}
		}

		[ExpectNoExceptions]
		public void TestHouseBillOfLadingTypeForSea_DefaultNotMandatory()
		{
			var item = ItemSet.HouseBillOfLadingTypesForSea;

			var list = new CodeDescriptionPairList();
			list.AddPair("TY1", "Type 1");
			list.AddPair("TY2", "Type 2");

			var typesWithDefault = new SystemDefinableCodeDescriptionBoolCollection(3, list, true);

			using (item.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, typesWithDefault))
			{
			}
		}

		[ExpectNoExceptions]
		public void TestHouseBillOfLadingTypeForRail_DefaultNotMandatory()
		{
			var item = ItemSet.HouseBillOfLadingTypesForRail;

			var list = new CodeDescriptionPairList();
			list.AddPair("TY1", "Type 1");
			list.AddPair("TY2", "Type 2");

			var typesWithDefault = new SystemDefinableCodeDescriptionBoolCollection(3, list, true);

			using (item.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, typesWithDefault))
			{
			}
		}

		public void TestShipmentCustomAttributeRegistryItems()
		{
			string category = "Freight/Shipment/Custom Attributes";
			string hint = "A user defined field that is available on the shipment and declaration screens.";
			TestGenericRegistryItem(ItemSet.ShipmentCustomText1, "ShipmentCustomText1", category, "Text 1", hint, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.ShipmentCustomText2, "ShipmentCustomText2", category, "Text 2", hint, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.ShipmentCustomDate1, "ShipmentCustomDate1", category, "Date 1", hint, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.ShipmentCustomDate2, "ShipmentCustomDate2", category, "Date 2", hint, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.ShipmentCustomDecimalNo1, "ShipmentCustomDecimalNo1", category, "Decimal No. 1", hint, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.ShipmentCustomDecimalNo2, "ShipmentCustomDecimalNo2", category, "Decimal No. 2", hint, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.ShipmentCustomFlag1, "ShipmentCustomFlag1", category, "Flag 1", hint, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.ShipmentCustomFlag2, "ShipmentCustomFlag2", category, "Flag 2", hint, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);

			ItemSet.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("C1", "H1"));
			ItemSet.ShipmentCustomText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("C2", "H2"));
			ItemSet.ShipmentCustomDate1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("C3", "H3"));
			ItemSet.ShipmentCustomDate2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("C4", "H4"));
			ItemSet.ShipmentCustomDecimalNo1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("C5", "H5"));
			ItemSet.ShipmentCustomDecimalNo2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("C6", "H6"));
			ItemSet.ShipmentCustomFlag1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("C7", "H7"));
			ItemSet.ShipmentCustomFlag2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("C8", "H8"));

			AssertEquals("ShipmentText1.Value.Caption", "C1", ItemSet.ShipmentCustomText1.Value.Caption);
			AssertEquals("ShipmentText1.Value.Hint", "H1", ItemSet.ShipmentCustomText1.Value.Hint);

			AssertEquals("ShipmentText2.Value.Caption", "C2", ItemSet.ShipmentCustomText2.Value.Caption);
			AssertEquals("ShipmentText2.Value.Hint", "H2", ItemSet.ShipmentCustomText2.Value.Hint);

			AssertEquals("ShipmentDate1.Value.Caption", "C3", ItemSet.ShipmentCustomDate1.Value.Caption);
			AssertEquals("ShipmentDate1.Value.Hint", "H3", ItemSet.ShipmentCustomDate1.Value.Hint);

			AssertEquals("ShipmentDate2.Value.Caption", "C4", ItemSet.ShipmentCustomDate2.Value.Caption);
			AssertEquals("ShipmentDate2.Value.Hint", "H4", ItemSet.ShipmentCustomDate2.Value.Hint);

			AssertEquals("ShipmentDecimalNo1.Value.Caption", "C5", ItemSet.ShipmentCustomDecimalNo1.Value.Caption);
			AssertEquals("ShipmentDecimalNo1.Value.Hint", "H5", ItemSet.ShipmentCustomDecimalNo1.Value.Hint);

			AssertEquals("ShipmentDecimalNo2.Value.Caption", "C6", ItemSet.ShipmentCustomDecimalNo2.Value.Caption);
			AssertEquals("ShipmentDecimalNo2.Value.Hint", "H6", ItemSet.ShipmentCustomDecimalNo2.Value.Hint);

			AssertEquals("ShipmentFlag1.Value.Caption", "C7", ItemSet.ShipmentCustomFlag1.Value.Caption);
			AssertEquals("ShipmentFlag1.Value.Hint", "H7", ItemSet.ShipmentCustomFlag1.Value.Hint);

			AssertEquals("ShipmentFlag2.Value.Caption", "C8", ItemSet.ShipmentCustomFlag2.Value.Caption);
			AssertEquals("ShipmentFlag2.Value.Hint", "H8", ItemSet.ShipmentCustomFlag2.Value.Hint);
		}

		public void TestConsignorShipperTerminology()
		{
			string expectedDefault = Env.CurrentCompany.Country.Code == Constants.CountryCodes.UnitedStates ||
				Env.CurrentCompany.Country.Code == Constants.CountryCodes.Canada ? "Shipper" : "Consignor";

			TestRegistryItem(ItemSet.ConsignorShipperTerminology,
				"ConsignorShipperTerminology",
				"Freight/Shipment",
				"Consignor / Shipper Terminology",
				"By setting this, all references to the consignor / shipper will appear as per this setting.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				TextEditorType.TextBox,
				RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue,
				expectedDefault);

			ZString alternativeCountry = Env.CurrentCompany.Country.Code == Constants.CountryCodes.UnitedStates ||
				Env.CurrentCompany.Country.Code == Constants.CountryCodes.Canada ?
				Constants.CountryCodes.China : Constants.CountryCodes.UnitedStates;

			BusinessObject glbCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject glbBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));

			glbCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = alternativeCountry;
			glbBranch[GlbBranchSchema.Constants.GB_GC] = glbCompany.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("ConsignorShipperTerminology");

				expectedDefault = Env.CurrentCompany.Country.Code == Constants.CountryCodes.UnitedStates ||
					Env.CurrentCompany.Country.Code == Constants.CountryCodes.Canada ? "Shipper" : "Consignor";

				TestRegistryItem(ItemSet.ConsignorShipperTerminology,
					"ConsignorShipperTerminology",
					"Freight/Shipment",
					"Consignor / Shipper Terminology",
					"By setting this, all references to the consignor / shipper will appear as per this setting.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					TextEditorType.TextBox,
					RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue,
					expectedDefault);
			}
		}

		public void TestExportStatementSettingRegistryItem()
		{
			ExportStatementSettingRegistryItem item = ItemSet.ExportStatementSettings;
			AssertEquals("ExportStatementSetting", item.Name);
			AssertEquals("Export Statements", item.Caption);
			AssertEquals("The different Statements to be shown on certain documents like House Bill or Manifest", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			Assert("There are default items in the collection", item.DefaultValue.Count != 0);//details are asserted in the collection
		}

		public void TestExportDestinationValueInUXML()
		{
			var registryItem = ItemSet.ExportDestinationValueInUXML;
			AssertEquals("Name", "ExportDestinationValueInUXML", registryItem.Name);
			AssertEquals("Caption", "Export Destination Value In UXML", registryItem.Caption);
			AssertEquals("Hint", "If turned on, Destination Good Value, Currency and Destination Exchange Rate calculated on Shipment will be exported into Universal Shipment XML.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, registryItem.Options);
			AssertEquals("DefaultValue", false, registryItem.DefaultValue);
		}

		public void TestLastSequenceNumberForMultilineConsolsRegistryItemIsHidden()
		{
			IntRegistryItem item = ItemSet.LastSequenceNumberForMultilineConsols;
			AssertEquals(RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue, item.Options);
		}

		public void TestInsuranceValueDefaulting()
		{
			AssertEquals("InsuranceValueDefaulting", ItemSet.InsuranceValueDefaulting.Name);
			AssertEquals("Freight/Shipment", ItemSet.InsuranceValueDefaulting.Category);
			AssertEquals("Default Insurance Value from Goods Value", ItemSet.InsuranceValueDefaulting.Caption);
			AssertEquals("Enable this option to default Insurance Value from Goods Value.", ItemSet.InsuranceValueDefaulting.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.InsuranceValueDefaulting.Storage);

			AssertEquals("DefaultValue", true, ItemSet.InsuranceValueDefaulting.DefaultValue);

			ItemSet.InsuranceValueDefaulting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.InsuranceValueDefaulting.Value);
		}

		public void TestSeaMinimumChargeableWeightRegistryItem()
		{
			AssertEquals("SeaMinimumChargeableWeight", ItemSet.SeaMinimumChargeableWeight.Name);
			AssertEquals("Minimum Chargeable", ItemSet.SeaMinimumChargeableWeight.Caption);
			AssertEquals(@"The Minimum Chargeable Unit (SEA Shipments *only*).

When the chargeable unit calculated is less than the minimum specified, the system will display the specified value as the ""Chargeable Weight"" in the Shipment Form.

By default, the value is 0, which indicates that no minimum chargeable is applicable.", ItemSet.SeaMinimumChargeableWeight.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.SeaMinimumChargeableWeight.Storage);
			AssertEquals("Default min chargeable", 0.0M, ItemSet.SeaMinimumChargeableWeight.Value);
		}

		public void TestHBLDeliveryMode_FCL()
		{
			AssertHBLDeliveryModeRegistryItem(ItemSet.HBLDeliveryMode_FCL, "HBLDeliveryModeFCL", FreightDataRegistry.Categories.Freight_Shipment_HBLDeliveryMode, "HBL Delivery Mode for FCL");
		}

		public void TestHBLDeliveryMode_LCL()
		{
			AssertHBLDeliveryModeRegistryItem(ItemSet.HBLDeliveryMode_LCL, "HBLDeliveryModeLCL", FreightDataRegistry.Categories.Freight_Shipment_HBLDeliveryMode, "HBL Delivery Mode for LCL");
		}

		public void TestHBLDeliveryMode_BCN()
		{
			AssertHBLDeliveryModeRegistryItem(ItemSet.HBLDeliveryMode_BCN, "HBLDeliveryModeBCN", FreightDataRegistry.Categories.Freight_Shipment_HBLDeliveryMode, "HBL Delivery Mode for BCN");
		}

		public void TestHBLDeliveryMode_SCN()
		{
			AssertHBLDeliveryModeRegistryItem(ItemSet.HBLDeliveryMode_SCN, "HBLDeliveryModeSCN", FreightDataRegistry.Categories.Freight_Shipment_HBLDeliveryMode, "HBL Delivery Mode for SCN");
		}

		public void TestHBLDeliveryMode_BBK_ROR_BLK_LQD()
		{
			AssertHBLDeliveryModeRegistryItem(ItemSet.HBLDeliveryMode_BBK_ROR_BLK_LQD, "HBLDeliveryModeBBKRORBLKLQD", FreightDataRegistry.Categories.Freight_Shipment_HBLDeliveryMode, "HBL Delivery Mode for Break Bulk, Roll On/Roll Off, Bulk, Liquid");
		}

		public void TestHBLDeliveryMode_LSE()
		{
			AssertHBLDeliveryModeRegistryItem(ItemSet.HBLDeliveryMode_LSE, "HBLDeliveryModeLSE", FreightDataRegistry.Categories.Freight_Shipment_HBLDeliveryMode, "HBL Delivery Mode for LSE");
		}

		public void TestHBLDeliveryMode_ULD()
		{
			AssertHBLDeliveryModeRegistryItem(ItemSet.HBLDeliveryMode_ULD, "HBLDeliveryModeULD", FreightDataRegistry.Categories.Freight_Shipment_HBLDeliveryMode, "HBL Delivery Mode for ULD");
		}

		void AssertHBLDeliveryModeRegistryItem(HBLDeliveryModeRegistryItem itemToTest, string name, string category, string caption)
		{
			AssertEquals(name, itemToTest.Name);
			AssertEquals(category, itemToTest.Category);
			AssertEquals(caption, itemToTest.Caption);
			AssertEquals(RegistryStorageFlags.System, itemToTest.Storage);
			AssertEquals("Default HBL Delivery Code is blank", "", itemToTest.Value.DefaultHBLDeliveryMode);

			HBLDeliveryModeCollection col = itemToTest.Value.Modes;
			foreach (HBLDeliveryMode item in col)
			{
				AssertEquals("Item can't be deleted", false, ((ICanDelete)item).CanDelete);
				AssertEquals("Code is read only", true, item.CodeInfo.ReadOnly);
				AssertEquals("Description is read only", true, item.Description_ReadOnly);
				AssertEquals("English Description is read only", true, item.EnglishDescription_ReadOnly);
				AssertEquals("ShowInLists can be edited", false, item.ShowInListInfo.ReadOnly);
				AssertEquals("ShowInLists default as ticked", true, item.ShowInList);
			}
		}

		#endregion

		#region Bookings

		public void TestDefaultBookingNewButton()
		{
			AssertEquals(BookingNewButtonLabelList.Codes.BookingWithQuote, ItemSet.DefaultBookingNewButton.DefaultValue);

			ComboBoxRegistryEditorInfo info = (ComboBoxRegistryEditorInfo)ItemSet.DefaultBookingNewButton.EditorInfo;
			AssertEquals("BWQ, PAL, BKG", info.LookUpList.CodesAsString);
		}

		public void TestEBookingApiUrl()
		{
			AssertEquals("EBookingServicesApiUrl", ItemSet.EBookingServicesApiUrl.Name);
			AssertEquals(FreightDataRegistry.Categories.Freight_EBookings, ItemSet.EBookingServicesApiUrl.Category);
			AssertEquals("eBooking API Service", ItemSet.EBookingServicesApiUrl.Caption);
			AssertEquals("The URL used to connect to eBooking API", ItemSet.EBookingServicesApiUrl.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.EBookingServicesApiUrl.Storage);
			AssertEquals("Default EBookingApiUrl is production URL", "https://abe.wisegrid.net/v1", ItemSet.EBookingServicesApiUrl.Value.SelectedUrl);
		}

		public void TestEnableAirlineConnectMenu()
		{
			TestRegistryItem(
				ItemSet.EnableAirlineConnectMenu,
				"EnableAirlineConnectMenu",
				FreightDataRegistry.Categories.Freight_EBookings,
				"Show AirlineConnect Menu Item",
				"When this registry is enabled, the AirlineConnect menu item will be shown from Actions menu of consol form.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestCustomEBookingApiUrl()
		{
			TestRegistryItem(
				ItemSet.CustomEBookingApiServiceUrl,
				"CustomEBookingApiServiceUrl",
				FreightDataRegistry.Categories.Freight_EBookings,
				"eBooking CUSTOM API Service URL",
				"The URL used to connect to a customized eBooking API Service URL. If you override default value, your customized URL will be used instead of eBooking API service URL",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport,
				string.Empty);
		}

		public void TestEBookingApiTimeoutInSeconds()
		{
			TestRegistryItem(
				ItemSet.EBookingApiTimeoutInSeconds,
				"EBookingApiTimeoutInSeconds",
				FreightDataRegistry.Categories.Freight_EBookings,
				"eBooking API Requests Timeout",
				"Timeout is seconds when eBooking requests are executed",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				120,
				1,
				3600);
		}

		public void TestEBookingCarrierConfigurationApiTimeoutInSeconds()
		{
			TestRegistryItem(
				ItemSet.EBookingCarrierConfigurationApiTimeoutInSeconds,
				"EBookingCarrierConfigurationApiTimeoutInSeconds",
				FreightDataRegistry.Categories.Freight_EBookings,
				"eBooking Carrier Configuration Request Timeout",
				"Timeout is seconds when eBooking requests are executed",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				5,
				1,
				300);
		}

		public void TestShowDetailedRequestFailureForEBookings()
		{
			TestRegistryItem(
				ItemSet.ShowDetailedRequestFailureForEBookings,
				"ShowDetailedRequestFailureForEBookings",
				FreightDataRegistry.Categories.Freight_EBookings,
				"Show Detailed Request Failure For eBookings",
				"When set to true it will display detailed request failure",
				RegistryStorageFlags.System,
				false);
		}

		public void TestIncludeAuthorizationWhenSendingEBookings()
		{
			TestRegistryItem(
				ItemSet.IncludeAuthorizationWhenSendingEBookings,
				"IncludeAuthorizationWhenSendingEBookings",
				FreightDataRegistry.Categories.Freight_EBookings,
				"Include Authorization When Sending eBookings",
				"When set to true the request will contain authorization details",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestEnableAirBookingCarrierConfiguration()
		{
			TestRegistryItem(
				ItemSet.EnableAirBookingCarrierConfiguration,
				"EnableAirBookingCarrierConfiguration",
				FreightDataRegistry.Categories.Freight_EBookings,
				"Enable eBooking Carrier Configuration",
				"By default the eBooking Carrier Configuation is enabled. Change this setting to Yes to enable eBooking Carrier Configuration.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestPacklineWeightDistribution()
		{
			var registryItem = ItemSet.PacklineWeightDistribution;
			AssertEquals("Name", "PacklineWeightDistribution", registryItem.Name);
			AssertEquals("Category", "Freight/Bookings/Electronic Bookings", registryItem.Category);
			AssertEquals("Caption", "Packline Weight Distribution", registryItem.Caption);
			AssertEquals("Hint", "By default the Packline Weight Distribution is disabled. Change this setting to Yes to enable the Actual Weight Distribution with the option to also enable the Volumetric Weight Distribution as well.", registryItem.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals("Options", RegistryOptions.Default, registryItem.Options);
			AssertEquals("Default Enable Packline Weight Distribution", false, registryItem.DefaultValue.EnablePacklineWeightDistribution);
			AssertEquals("Default Enable Actual Weight Distribution", false, registryItem.DefaultValue.EnableActualWeightDistribution);
			AssertEquals("Default Enable Volumetric Weight Distribution", false, registryItem.DefaultValue.EnableVolumetricWeightDistribution);
		}

		#endregion

		#region CommunityRegionsForDirectionCalculation

		public void TestCommunityRegionsForDirectionCalculation()
		{
			var collection = new CountryListCollection(Factory);
			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			CountryListElement element1 = collection.AddNew();
			CountryListElement element2 = collection.AddNew();
			element1.CountryPK = guid1;
			element2.CountryPK = guid2;

			ItemSet.CommunityRegionsForDirectionCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { guid1, guid2 });

			AssertEquals("CommunityRegionsForDirectionCalculation", ItemSet.CommunityRegionsForDirectionCalculation.Name);
			AssertEquals("Freight", ItemSet.CommunityRegionsForDirectionCalculation.Category);
			AssertEquals("Community Region for Direction Calculation Purposes", ItemSet.CommunityRegionsForDirectionCalculation.Caption);
			AssertEquals("Use this registry to specify which countries/regions in your company’s country/region should be treated as local to this region when trading with the rest of the world for job direction calculation purposes.\r\n\r\nAny jobs originated in any country/region listed here including your home country/region with the destination outside of this region will be treated as export jobs, and any jobs originated outside of this group with a destination inside of this region including your home country/region will be treated as import jobs.\r\n\r\nThe registry settings will allow you to override the default cross-trade calculation (when jobs originated by a logged in country/region with origin or destination outside of this country/region are treated as cross-trade ) which in turn will take an effect on Shipment and Bill Number generation (when job direction is used in number generating algorithms), on calculation of local client and overseas agent, department and revenue recognition date.", ItemSet.CommunityRegionsForDirectionCalculation.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.CommunityRegionsForDirectionCalculation.Storage);
			AssertContainsExactElementsInAnyOrder(new[] { guid1, guid2 }, ItemSet.CommunityRegionsForDirectionCalculation.Value);
		}

		#endregion

		#region Routing

		public void TestDefaultRoutingLegStatus()
		{
			AssertEquals(Constants.TransportStatus.Confirmed, ItemSet.DefaultRoutingLegStatus.DefaultValue);

			ComboBoxRegistryEditorInfo info = (ComboBoxRegistryEditorInfo)ItemSet.DefaultRoutingLegStatus.EditorInfo;
			AssertEquals("CNF, PLN", info.LookUpList.CodesAsString);
		}

		public void TestS8LoginSuppressionTimeoutInMinutes()
		{
			TestRegistryItem(ItemSet.S8LoginSuppressionTimeoutInMinutes,
				"S8LoginSuppressionTimeoutInMinutes",
				FreightDataRegistry.Categories.Freight_Routing,
				"Login Suppression Timeout In Minutes",
				"Suppression Timeout is minutes of next login after previous login failed.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				20, 0, 60);
		}

		public void TestS8CargoWebServiceSessionFailOverToken()
		{
			TestRegistryItem(ItemSet.S8CargoWebServiceSessionFailOverToken,
				"S8CargoWebServiceSessionFailOverToken",
				FreightDataRegistry.Categories.Freight_Routing,
				"Web Service Session Fail-Over Token",
				"The Current Fail-Over Token for transactions to S8 Cargo's Web Service",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				TextEditorType.TextBox,
				"");
		}

		public void TestS8CommunicationTimeoutInSeconds()
		{
			TestRegistryItem(ItemSet.S8CommunicationTimeoutInSeconds,
				"S8CommunicationTimeoutInSeconds",
				FreightDataRegistry.Categories.Freight_Routing,
				"Communication Timeout In Seconds",
				"Communication Timeout in seconds for the connection.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				10, 1, 60);
		}

		public void TestS8CargoWebServicePassword()
		{
			TestGenericRegistryItem(ItemSet.S8CargoWebServicePassword,
				"SPServCWargoice8DEncrypted",
				FreightDataRegistry.Categories.Freight_Routing,
				"Web Service Password",
				string.Empty,
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers);
			AssertType<SecureStringRegistryDataType>(ItemSet.S8CargoWebServicePassword.DataType);
		}

		public void TestAutomaticUpdatingofPlannedLegs_Air()
		{
			TestRegistryItem(ItemSet.AutomaticUpdatingofPlannedLegs_Air, "AutomaticUpdatingofPlannedLegs_Air", FreightDataRegistry.Categories.Freight_Routing_AutomaticUpdatingofPlannedLegs, "Air", "This will enable the automatic updating of Planned Legs from Actual Events as they are received/processed.", RegistryStorageFlags.System, false);
		}

		public void TestAutomaticUpdatingofPlannedLegs_Sea_FromBookingConfirmation()
		{
			TestRegistryItem(ItemSet.AutomaticUpdatingofPlannedLegs_Sea_FromBookingConfirmation,
				"AutomaticUpdatingofPlannedLegs_Sea_FromBookingConfirmation",
				FreightDataRegistry.Categories.Freight_Routing_AutomaticUpdatingofPlannedLegs_Sea,
				"From Booking Confirmation",
				"If enabled, the system will automatically update routing legs on Consols using information received in the Booking Confirmation message.\r\nIf disabled, the system will still process Booking Confirmation message, update relevant details on Consols, however, it will not override Consol routing legs.\r\nRouting details provided in Booking Confirmation message are visible in the Routing > Actual > Booking Confirmation grid, for review and manual update as required.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestAutomaticUpdatingofPlannedLegs_Sea_FromContainerAutomationEvents()
		{
			TestRegistryItem(ItemSet.AutomaticUpdatingofPlannedLegs_Sea_FromContainerAutomationEvents,
				"AutomaticUpdatingofPlannedLegs_Sea_FromContainerAutomationEvents",
				FreightDataRegistry.Categories.Freight_Routing_AutomaticUpdatingofPlannedLegs_Sea,
				"From Container Automation Events",
				"If enabled, Consol routing legs with matching load and/or discharge ports will be automatically updated with vessel name, voyage number and dates from Container Automation Events.\r\nIf disabled, Consol routing legs will still be automatically updated with estimated and actual dates matching load and/or discharge ports on the legs, but vessel and voyage details on these legs will not be automatically overridden.\r\nRouting details provided in the latest Container Automation messages are visible in the Routing > Actual > Events grid as legs constructed from these events for review and manual update as required.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region AWB

		public void TestPrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabel()
		{
			TestRegistryItem(ItemSet.PrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabel, "PrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabel", "Freight/AWB", "Print Total Number of Pieces on Shipment AWB Barcode Label", "For AWB Barcode Label printed from a shipment show 'Total Number of Pieces' only if this registry item is set to 'Yes'.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, true);
		}

		public void TestDefaultShipmentGoodsValueToHAWBAndDirectMAWB()
		{
			TestRegistryItem(ItemSet.DefaultShipmentGoodsValueToHAWBAndDirectMAWB,
				"DefaultShipmentGoodsValueToHAWBAndDirectMAWB",
				"Freight/AWB",
				"Default Shipment Goods Value to the HAWB and Direct MAWB",
				"Specify whether the Shipment Goods Value should default to the HAWB and Direct MAWB.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		public void TestShowCollectOtherWarning()
		{
			TestRegistryItem(ItemSet.ShowCollectOtherWarning, "ShowCollectOtherWarning", "Freight/AWB/MAWB", "Show Collect Other Charge Warning", "Turn this on to validate that if the MAWB Payment Method is Collect then at least 1 charge must be specified.", RegistryStorageFlags.All, false);
		}

		public void TestIssuedByDetailsUseShipmentOrigin()
		{
			TestRegistryItem(ItemSet.IssuedByDetailsUseShipmentOrigin, "IssuedByDetailsUseShipmentOrigin", "Freight/AWB/HAWB", "Use shipment origin port to determine 'Issued By' details.", "If turned on, shipment origin port will be used to determine 'Issued By' details instead of using consol Flight 1 load port.", RegistryStorageFlags.System, false);
		}

		public void TestMAWBBillingSellRate()
		{
			TestRegistryItem(
				ItemSet.MAWBBillingSellRate,
				"MAWBBillingSellRate",
				"Freight/AWB/MAWB",
				"Populate Billing Sell Rate on Direct Air Consolidations",
				"Use this registry to define Sell Rates to display on the Master Air Waybill for collect and/or prepaid direct air consolidations instead of consol Chargeable Rate.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				new CodeDescriptionPairList(OLookUpEditType.MAWBBillingSellRateModes),
				Constants.AWB.MAWBBillingSellRateModes.None);
		}

		public void TestPrintHawbNumbersInBodyOfMawb()
		{
			TestRegistryItem(ItemSet.PrintHawbNumbersInBodyOfMawb, "PRINT_HAWB_NUMBERS_IN_BODY_OF_MAWB", "Freight/AWB/MAWB", "Print HAWB Numbers in Body of MAWB",
				"The HAWB numbers will print in the description of the MAWB when the consignee is in any of the countries/regions listed below.",
				RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, typeof(CountryListRegistryEditorInfo), new Guid[] { Core.Constants.CountryGuids.Brazil });
		}

		public void TestAWBSecurityStatementTextToUSAAndUSTerritories()
		{
			TestRegistryItem(ItemSet.AWBSecurityStatementTextToUSAAndUSTerritories, "AWBSecurityStatementTextToUSAAndUSTerritories", "Freight/AWB/Security Statement to the USA", "Statement Text", "Security statement confirming that goods to or via the USA and its overseas territories have not originated from, transferred from, or transited through any country listed under the registry: Freight > AWB > Security Statement to the USA > List of Countries of Origin or Transshipment.\r\n\r\nThis statement is included in the eAWB message and shown on the Security Declaration.", RegistryStorageFlags.System, TextEditorType.AWBCustomisableText, "<CompanyName> has reviewed all available documentation and has determined that none of the cargo being offered in this consignment or consolidation has/either originated in, transferred from, or transited through any point in <TSASecurityStatementCountriesText>.");
		}

		public void TestAWBCountriesOfOriginOrTransshipmentInUSAAndUSTerritoriesSecuirtyStatement()
		{
			TestRegistryItem(ItemSet.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement, "AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement", "Freight/AWB/Security Statement to the USA", "List of Countries/Regions of Origin or Transhipment", "List of countries/regions to be included in the Security Statement, confirming that goods to or via the USA have not originated from, transferred from, or transited through any of the countries/regions listed below.\r\n\r\nThis statement is included in the eAWB message and shown on the Security Declaration.", RegistryStorageFlags.System, typeof(CountryListRegistryEditorInfo), new Guid[] { Constants.CountryGuids.Egypt, Constants.CountryGuids.Somalia, Constants.CountryGuids.SyrianArabRepublic, Constants.CountryGuids.Yemen });
		}

		public void TestMAWBRecyclePeriod()
		{
			TestRegistryItem(ItemSet.MAWBRecyclePeriod, "MAWBRecyclePeriod", "Freight/AWB/MAWB", "Allow re-use of MAWB number after (months).", "The number of months after which a MAWB number may be re-used on another shipment.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, 12, 0, 120);
		}

		public void TestMAWBDefaultLowStockLevel()
		{
			TestRegistryItem(ItemSet.MAWBDefaultLowStockLevel,
				"MAWBDefaultLowStockLevel",
				"Freight/AWB/MAWB",
				"Default Low Stock Level",
				"Sets the minimum default value to warn users of low MAWB stock levels. This will apply as the fallback warning value when there is no MAWB threshold configuration stored for the airline record that the CW1 branch is using.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				10, 1, 9999);
		}

		public void TestAllocateMAWBNumberFromMAWBStockUponXMLImport()
		{
			TestRegistryItem(ItemSet.AllocateMAWBNumberFromMAWBStockUponXMLImport,
				"AllocateMAWBNumberFromMAWBStockUponXMLImport",
				"Freight/AWB/MAWB",
				"Allocate MAWB number from MAWB Stock upon XML Import",
				"Enable this Registry setting to allow the system to auto-allocate MAWB Stock to a Consol on the import of Universal Shipment XML’s when only the MAWB Prefix is found inside the XML.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestPrintSignatureForHAWBDocuments()
		{
			TestRegistryItem(ItemSet.PrintSignatureForHAWBDocuments,
				"PrintSignatureForHAWBDocuments",
				"Freight/AWB/HAWB",
				"Print Signature",
				"Use this Registry to allow staff signature to be printed on HAWB documents where a dedicated signature box is provided.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false
				);
		}

		public void TestPrintSignatureForMAWBDocuments()
		{
			TestRegistryItem(ItemSet.PrintSignatureForMAWBDocuments,
				"PrintSignatureForMAWBDocuments",
				"Freight/AWB/MAWB",
				"Print Signature",
				"Use this Registry to allow staff signature to be printed on MAWB document. Warning!  Not all airlines may support this approach, check with all airlines concerned prior to enabling this Registry.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false
				);
		}

		public void TestPrintSignatureForHBLDocuments()
		{
			TestRegistryItem(ItemSet.PrintSignatureForHBLDocuments,
				"PrintSignatureForHBLDocuments",
				"Freight/House Bills",
				"Print Signature",
				"Use this Registry to allow staff signature to be printed on House Bill documents where a dedicated signature box is provided.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false
				);
		}

		public void TestAWBRoundings()
		{
			AssertEquals("AWBRoundings", ItemSet.AWBRoundings.Name);
			AssertEquals("Freight/AWB", ItemSet.AWBRoundings.Category);
			AssertEquals("Rounding Of Chargeable Weight", ItemSet.AWBRoundings.Caption);
			AssertEquals("Specify rounding options for Chargeable Weight for different types of Air Waybills.", ItemSet.AWBRoundings.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.AWBRoundings.Storage);

			AWBRoundingCollection collection = ItemSet.AWBRoundings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("RegistryItem contains four rows", 4, collection.Count);

			Func<AWBRoundingCollection, string> awbRoundingContents = (awbRoundingCollection) =>
			{
				string result = "";
				foreach (AWBRounding awbRounding in awbRoundingCollection)
				{
					result += string.Format("{0} {1} {2}\r\n", awbRounding.AWBType, awbRounding.RoundingMode, awbRounding.RoundingScale);
				}

				return result;
			};

			string expectedContents = awbRoundingContents(AWBRoundingCollection.GetDefault());
			string actualContents = awbRoundingContents(collection);
			AssertEquals("Should have default values from AWBRoundingCollection", expectedContents, actualContents);
		}

		public void TestFreightChargeableWeightRoundings()
		{
			AssertEquals("FreightChargeableWeightRoundings", ItemSet.FreightChargeableWeightRoundings.Name);
			AssertEquals("Freight", ItemSet.FreightChargeableWeightRoundings.Category);
			AssertEquals("Rounding Of Chargeable Weight - Air", ItemSet.FreightChargeableWeightRoundings.Caption);
			AssertEquals("Specify rounding options for Chargeable Weight for Air freight", ItemSet.FreightChargeableWeightRoundings.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.FreightChargeableWeightRoundings.Storage);

			ChargeableWeightRoundingCollection collection = ItemSet.FreightChargeableWeightRoundings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("RegistryItem contains one row", 1, collection.Count);
		}

		public void TestAWBApprovedExporterText()
		{
			var item = ItemSet.AWBApprovedExporterText;

			var company = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			company[GlbCompanySchema.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Germany;

			var branch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			branch[GlbBranchSchema.GB_GC] = company.PK;

			Factory.Save();

			AssertEquals("", item.DefaultValue);
			AssertEquals("AWBApprovedExporterText", item.Name);
			AssertEquals(FreightDataRegistry.Categories.Freight_SupplyChainSecurity, item.Category);
			AssertEquals("Exporter Text - Approved", item.Caption);
			AssertEquals("The text that will display on the AWB for Approved Exporter movements. To include this on other documents, you must reference the macro <InspectedShipmentText>.", item.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals(TextEditorType.AWBCustomisableText, ((TextRegistryEditorInfo)item.EditorInfo).EditorType);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("SPX; <CustomsCode(<BranchProxy>, DE, GCA)>; <BranchProxyName>", item.DefaultValue);
			}
		}

		public void TestAWBNonApprovedExporterText()
		{
			var item = ItemSet.AWBNonApprovedExporterText;

			var company = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			company[GlbCompanySchema.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Germany;

			var branch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			branch[GlbBranchSchema.GB_GC] = company.PK;

			Factory.Save();

			AssertEquals("", ItemSet.AWBNonApprovedExporterText.DefaultValue);
			AssertEquals("AWBNonApprovedExporterText", item.Name);
			AssertEquals(FreightDataRegistry.Categories.Freight_SupplyChainSecurity, item.Category);
			AssertEquals("Exporter Text - Non Approved", item.Caption);
			AssertEquals("The text that will display on the AWB for Non-Approved Exporter movements. To include this on other documents, you must reference the macro <InspectedShipmentText>.", item.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals(TextEditorType.AWBCustomisableText, ((TextRegistryEditorInfo)item.EditorInfo).EditorType);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("Not secured; <CustomsCode(<BranchProxy>, DE, GCA)>; <BranchProxyName>", ItemSet.AWBNonApprovedExporterText.DefaultValue);
			}
		}

		public void TestAWBLabelCustomisation()
		{
			AWBLabelCustomisationRegistryItem item = ItemSet.AWBBarcodeLabelCustomisation;

			AssertEquals("AWBBarcodeLabelCustomisation", item.Name);
			AssertEquals("Freight/AWB", item.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);

			AWBLabelCustomisation awbLabel = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("Default design", AWBLabelCustomDesignList.Codes.Default, awbLabel.CustomDesign);

			awbLabel.CustomDesign = AWBLabelCustomDesignList.Codes.Design1;
			awbLabel.OptionalInformation1 = AWBLabelOptionalInformationList.Codes.ConsolDestination;
			awbLabel.BarcodeConsolDestination = true;

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, awbLabel);
			AssertEquals("New design", AWBLabelCustomDesignList.Codes.Design1, awbLabel.CustomDesign);
			AssertEquals("Optional info field was set", AWBLabelOptionalInformationList.Codes.ConsolDestination, awbLabel.OptionalInformation1);
			AssertEquals("Barcode checkbox was set", true, awbLabel.BarcodeConsolDestination);
		}

		public void TestSetHAWBCurrencyToFirstCollectInvoiceCurrency()
		{
			TestRegistryItem(ItemSet.SetHAWBCurrencyToFirstCollectInvoiceCurrency,
				"SetHAWBCurrencyToFirstCollectInvoiceCurrency",
				"Freight/AWB/HAWB",
				"Use Currency of First Collect Freight Charge",
				"If turned on, the HAWB Currency will default to the currency of the first collect freight accounting charge. The default behavior is to use the current company's currency.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestEnforceUniqueHAWBNumbers()
		{
			TestGenericRegistryItem(ItemSet.EnforceUniqueHAWBNumbers,
				"EnforceUniqueHAWBNumbers",
				"Freight/AWB/HAWB",
				"Enforce Unique HAWB Numbers",
				@"When this registry setting is set to 'Yes' the system will ensure HAWB numbers are unique and allow you to regenerate house bill numbers using 'Regenerate House Bill Number' menu action even if HAWB number is read only.
Default setting is 'No', meaning the warning will be given on duplicate HAWB(and HBL) numbers.",
				RegistryStorageFlags.System);

			AssertEquals(false, ItemSet.EnforceUniqueHAWBNumbers.DefaultValue);
		}

		public void TestSetHAWBCurrencyToFirstPrepaidInvoiceCurrency()
		{
			TestRegistryItem(ItemSet.SetHAWBCurrencyToFirstPrepaidInvoiceCurrency,
				"SetHAWBCurrencyToFirstPrepaidInvoiceCurrency",
				"Freight/AWB/HAWB",
				"Use Currency of First Prepaid Freight Charge",
				"If turned on, the HAWB Currency will default to the currency of the first prepaid freight accounting charge. The default behavior is to use the current company's currency.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestSendFWBNatureAndQuantityOfGoodsType()
		{
			TestGenericRegistryItem(ItemSet.SendFWBNatureAndQuantityOfGoodsType,
				"SendFWBNatureAndQuantityOfGoodsType",
				"Freight/AWB/CargoIMP/FWB Messaging",
				"Send Nature and Quantity of Goods Type on FWB",
				"Specifies whether the 'Nature and Quantity of Goods Type' will be sent on the FWB",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		public void TestMAWBSuppressULDTareWeight()
		{
			TestGenericRegistryItem(ItemSet.MAWBSuppressULDTareWeight,
				"MAWBSuppressULDTareWeight",
				"Freight/AWB/MAWB",
				"Suppress ULD Tare Weight on MAWB",
				"When enabled, show Gross Weight as 0 for the ULD Rate Class X.\r\n\r\nChanging this registry can cause non-compliant MAWB messages to be sent, please use with caution.",
				RegistryStorageFlags.Company,
				false);
		}

		public void TestMessagingViaPelicanServer()
		{
			TestRegistryItem(ItemSet.MessagingViaPelicanServer,
				"MessagingViaPelicanServer",
				"Freight/AWB/Airline Messaging",
				"Messaging via API",
				"Specifies whether airline messages will are sent via airline messaging API gateway.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestPelicanApiUrl()
		{
			TestStringRegistryItem(
				ItemSet.PelicanApiUrl,
				"PelicanApiUrl",
				"Freight/AWB/Airline Messaging",
				"API URL",
				"Specifies the API gateway URL used for sending airline messages.",
				RegistryStorageFlags.System,
				TextEditorType.Url,
				RegistryOptions.IsOnlyForSupport,
				"https://airmessaging.wisegrid.net/",
				CharacterCase.Normal);
		}

		public void TestPelicanApiTimeout()
		{
			TestRegistryItem(
				ItemSet.PelicanApiTimeout,
				"PelicanApiTimeout",
				"Freight/AWB/Airline Messaging",
				"API Request Timeout",
				"Specifies the timeout (in seconds) when the airline messaging API gateway is requested or called.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				30,
				1,
				300
			);
		}

		public void TestPelicanApiRetryAttempts()
		{
			TestRegistryItem(
				ItemSet.PelicanApiRetryAttempts,
				"PelicanApiRetryAttempts",
				"Freight/AWB/Airline Messaging",
				"API Request Retry Attempts",
				"Specifies the maximum number of failed request attempts to the airline messaging API gateway before an official error is returned.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				3,
				0,
				10
			);
		}

		public void TestAirlineMessagingCargoImpVersion()
		{
			var registryItem = ItemSet.AirlineMessagingCargoImpVersion;
			AssertEquals("Name", "AirlineMessagingCargoImpVersion", registryItem.Name);
			AssertEquals("Category", "Freight/AWB/Airline Messaging", registryItem.Category);
			AssertEquals("Caption", "CargoIMP Version", registryItem.Caption);
			AssertEquals("Hint", "Use this registry to set the FWB/FHL version sent to specific airlines based on their AWB prefix. V16 (FWB/16 and FHL/4) is the default message version setting for all airlines.\r\n\r\nPlease ensure you have received confirmation from the airline regarding their FWB/FHL message version capabilities before changing the default setting to avoid any message rejections.", registryItem.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, registryItem.Storage);
			AssertEquals("Options", RegistryOptions.Default, registryItem.Options);
			AssertEquals("Default Collection", 0, registryItem.DefaultValue.AirlineImpVersionMappings.Count);
			AssertEquals("Default IMP Version", "V16", registryItem.DefaultValue.DefaultImpVersion);
		}

		#region HAWB/MAWB Customisable Extra Texts

		public void TestHAWBHandlingInformationExtraText()
		{
			StringRegistryItem item = ItemSet.HAWBHandlingInformationExtraText;
			TestRegistryItem(item, item.Name, item.Category, "Handling Information Extra Text", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text.", RegistryStorageFlags.All, TextEditorType.AWBCustomisableText, "");
			AssertNonEnglishValidation(item);
		}

		public void TestHAWBNatureAndQtyOfGoodsExtraText()
		{
			StringRegistryItem item = ItemSet.HAWBNatureAndQtyOfGoodsExtraText;
			RegistryStorageFlags storageFlags = RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch;
			TestRegistryItem(item, item.Name, item.Category, "Nature and Quantity of Goods Extra Text", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text.", storageFlags, TextEditorType.AWBCustomisableText, "");
			AssertNonEnglishValidation(item);
		}

		public void TestHAWBOptionalShippingInfoOneExtraText()
		{
			StringRegistryItem item = ItemSet.HAWBOptionalShippingInfoOneExtraText;
			RegistryStorageFlags storageFlags = RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch;
			TestRegistryItem(item, item.Name, item.Category, "Optional Shipping Info 1 Extra Text", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text.", storageFlags, TextEditorType.AWBCustomisableText, "TERMS: <INCO>");
			AssertNonEnglishValidation(item);
		}

		public void TestHAWBOptionalShippingInfoTwoExtraText()
		{
			StringRegistryItem item = ItemSet.HAWBOptionalShippingInfoTwoExtraText;
			RegistryStorageFlags storageFlags = RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch;
			TestRegistryItem(item, item.Name, item.Category, "Optional Shipping Info 2 Extra Text", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text.", storageFlags, TextEditorType.AWBCustomisableText, "");
			AssertNonEnglishValidation(item);
		}

		public void TestHAWBAccountingInfoExtraText()
		{
			CodeDescriptionPairListRegistryItem item = ItemSet.HAWBAccountingInfoExtraText;
			RegistryStorageFlags storageFlags = RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch;
			TestRegistryItem(item, item.Name, item.Category, "Accounting Info Extra Text", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text. To insert a macro, please place the cursor inside the cell in the grid and press the 'Insert Field' button.", storageFlags, 256, 0, Array.Empty<ICodeDescription>());
		}

		[TestDate(2019, 12, 27)]
		public void TestSDRValueOnHAWBTermsAndConditionsChangesBasedOnDate()
		{
			var item = ItemSet.HAWBTermsAndConditionsBody1;
			Assert("Since date is ON/BEFORE 29 December, SDR should be 19 SDRs", item.DefaultValue.Contains("19 SDRs"));

			RegistryItemDictionary.Instance.PurgeAll();

			TestDateAttribute.Date = new DateTime(2019, 12, 30);
			item = ItemSet.HAWBTermsAndConditionsBody1;
			Assert("Since date is AFTER 29 December 2019, should show 22 SDRs", item.DefaultValue.Contains("22 SDRs"));

			RegistryItemDictionary.Instance.PurgeAll();

			TestDateAttribute.Date = new DateTime(2024, 12, 28);
			item = ItemSet.HAWBTermsAndConditionsBody1;
			Assert("Since date is ON 28 December 2024, should show 26 SDRs", item.DefaultValue.Contains("26 SDRs"));

			RegistryItemDictionary.Instance.PurgeAll();

			TestDateAttribute.Date = new DateTime(2024, 12, 29);
			item = ItemSet.HAWBTermsAndConditionsBody1;
			Assert("Since date is AFTER 28 December 2024, should show 26 SDRs", item.DefaultValue.Contains("26 SDRs"));
		}

		public void TestMAWBHandlingInformationExtraText()
		{
			StringRegistryItem item = ItemSet.MAWBHandlingInformationExtraText;
			TestRegistryItem(item, item.Name, item.Category, "Handling Information Extra Text", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text.", RegistryStorageFlags.All, TextEditorType.AWBCustomisableText, "");
			AssertNonEnglishValidation(item);
		}

		public void TestMAWBNatureAndQtyOfGoodsExtraText()
		{
			StringRegistryItem item = ItemSet.MAWBNatureAndQtyOfGoodsExtraText;
			RegistryStorageFlags storageFlags = RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch;
			TestRegistryItem(item, item.Name, item.Category, "Nature and Quantity of Goods Extra Text", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text.", storageFlags, TextEditorType.AWBCustomisableText, "");
			AssertNonEnglishValidation(item);
		}

		public void TestMAWBOptionalShippingInfoOneExtraText()
		{
			StringRegistryItem item = ItemSet.MAWBOptionalShippingInfoOneExtraText;
			RegistryStorageFlags storageFlags = RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch;
			TestRegistryItem(item, item.Name, item.Category, "Optional Shipping Info 1 Extra Text", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text.", storageFlags, TextEditorType.AWBCustomisableText, "");
			AssertNonEnglishValidation(item);
		}

		public void TestMAWBOptionalShippingInfoTwoExtraText()
		{
			StringRegistryItem item = ItemSet.MAWBOptionalShippingInfoTwoExtraText;
			RegistryStorageFlags storageFlags = RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch;
			TestRegistryItem(item, item.Name, item.Category, "Optional Shipping Info 2 Extra Text", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text.", storageFlags, TextEditorType.AWBCustomisableText, "");
			AssertNonEnglishValidation(item);
		}

		public void TestMAWBAccountingInfoExtraText()
		{
			CodeDescriptionPairListRegistryItem item = ItemSet.MAWBAccountingInfoExtraText;
			RegistryStorageFlags storageFlags = RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch;
			TestRegistryItem(item, item.Name, item.Category, "Accounting Info Extra Text", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text. To insert a macro, please place the cursor inside the cell in the grid and press the 'Insert Field' button.", storageFlags, 256, 0, Array.Empty<ICodeDescription>());
		}

		void AssertNonEnglishValidation(StringRegistryItem item)
		{
			AssertNotNull("To prevent Empty Test warning", item);

			try
			{
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Abcde");
			}
			catch (RegistryValidationException)
			{
				Fail("No validation exception should be thrown");
			}

			try
			{
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Абвгд");
				Fail("Validation exception should have been thrown");
			}
			catch (RegistryValidationException)
			{
			}
		}

		#endregion

		#endregion

		#region House Bills

		#region PrintOuterBreakdownPackingDetailsOnBOL

		public void TestPrintOuterBreakdownPackingDetailsOnBOL()
		{
			TestRegistryItem(ItemSet.ShowPackLineDetailsOnHouseBills,
				"ShowPackLineDetailsOnHouseBills",
				"Freight/House Bills",
				"Show Pack Line Details On House Bills",
				"Enable this option to show the breakdown of Outer Packline details for each container on House Bills.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				false);
		}

		#endregion

		#region BOLSendingForwarder

		public void TestBOLSendingForwarderCurrentBranchOrgProxy()
		{
			TestRegistryItem(ItemSet.BOLSendingForwarderCurrentBranchOrgProxy,
				"BOLSendingForwarderCurrentBranchOrgProxy",
				"Freight/House Bills",
				"Set Bill of Lading Sending Forwarder",
				"This registry item controls whether the Sending Forwarder displayed on Bill of Lading is the consol's sending agent or current branch's organization proxy.\r\n\r\nSelect 'Yes' - The Current Branch's organization proxy will be used.\r\n\r\nSelect 'No' - The consol's sending agent will be used.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		#endregion

		#region BOLClause

		[TestDate(2016, 10, 19)]
		public void TestBOLClause_US_BeforeDCSEffectiveDate()
		{
			string expectedDefault = "These commodities, technology or software were exported from the United States in accordance with the Export Administration Regulations. Diversion contrary to U.S. law is prohibited.";
			AssertBOLClause(Core.Constants.CountryCodes.UnitedStates, "BOLClause", "Standard", "This house bill clause will appear on house bills in the main body section.", expectedDefault, RegistryOptions.PreserveTestValue);
		}

		[TestDate(2016, 11, 15)]
		public void TestBOLClause_US_AfterDCSEffectiveDate()
		{
			string expectedDefault = string.Empty;
			AssertBOLClause(Core.Constants.CountryCodes.UnitedStates, "BOLClause", "Standard", "This house bill clause will appear on house bills in the main body section.", expectedDefault, RegistryOptions.PreserveTestValue);
		}

		[TestDate(2016, 10, 19)]
		public void TestBOLClause_Other_BeforeDCSEffectiveDate()
		{
			string expectedDefault = string.Empty;
			AssertBOLClause(Core.Constants.CountryCodes.Australia, "BOLClause", "Standard", "This house bill clause will appear on house bills in the main body section.", expectedDefault, RegistryOptions.PreserveTestValue);
		}

		[TestDate(2016, 11, 15)]
		public void TestBOLClause_Other_AfterDCSEffectiveDate()
		{
			string expectedDefault = string.Empty;
			AssertBOLClause(Core.Constants.CountryCodes.Australia, "BOLClause", "Standard", "This house bill clause will appear on house bills in the main body section.", expectedDefault, RegistryOptions.PreserveTestValue);
		}

		[TestDate(2016, 10, 19)]
		public void TestBOLClauseITAR_US_BeforeDCSEffectiveDate()
		{
			string expectedHint = "This house bill clause for International Traffic in Arms Regulations (ITAR) shipments will appear on house bills in the main body section. This is based on the License Number and Registration Number being completed on the Invoice Header on the Export Declaration tab of the Shipment.";
			string expectedDefault = "These commodities are authorized by the U.S. Government for export only to <BillOfLading.DeclarationDestinationCountry/Region> for end use by <BillOfLading.USUltimateConsignee>. They may not be transferred, transhipped on a non-continuous voyage, or otherwise be disposed of in any other country/region, either in their original form or after being incorporated into other end-items, without the prior approval of the U.S. Department of State.";
			AssertBOLClause(Core.Constants.CountryCodes.UnitedStates, "BOLClauseITAR", "Re-export/re-transfer 123.9 (B)", expectedHint, expectedDefault);
		}

		[TestDate(2016, 11, 15)]
		public void TestBOLClauseITAR_US_AfterDCSEffectiveDate()
		{
			string expectedHint = "This house bill clause for International Traffic in Arms Regulations (ITAR) shipments will appear on house bills in the main body section. This is based on the License Number and Registration Number being completed on the Invoice Header on the Export Declaration tab of the Shipment.";
			string expectedDefault = "These commodities are authorized by the U.S. Government for export only to <BillOfLading.DeclarationDestinationCountry/Region> for end use by <BillOfLading.USUltimateConsignee>. They may not be transferred, transhipped on a non-continuous voyage, or otherwise be disposed of in any other country/region, either in their original form or after being incorporated into other end-items, without the prior approval of the U.S. Department of State.";
			AssertBOLClause(Core.Constants.CountryCodes.UnitedStates, "BOLClauseITAR", "Re-export/re-transfer 123.9 (B)", expectedHint, expectedDefault, RegistryOptions.IsHidden);
		}

		void AssertBOLClause(string countryCode, string expectedName, string expectedCaption, string expectedHint, string expectedDefault, RegistryOptions expectedOptions = RegistryOptions.Default)
		{
			BusinessObject company = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject branch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			company[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = countryCode;
			branch[GlbBranchSchema.Constants.GB_GC] = company.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				TestRegistryItem(expectedName == "BOLClause" ? ItemSet.BOLClause : ItemSet.BOLClauseITAR,
					expectedName,
					FreightDataRegistry.Categories.Freight_HouseBills_Clauses,
					expectedCaption,
					expectedHint,
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					TextEditorType.Memo,
					expectedOptions,
					expectedDefault);
			}
		}

		[TestDate(2016, 10, 19)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestBOLClause_NullCountry()
		{
			Env.SetUserContext(new UserContext("", Guid.Empty, Guid.Empty));
			TestRegistryItem(ItemSet.BOLClause,
				"BOLClause",
				FreightDataRegistry.Categories.Freight_HouseBills_Clauses,
				"Standard",
				"This house bill clause will appear on house bills in the main body section.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				TextEditorType.Memo,
				RegistryOptions.PreserveTestValue,
				string.Empty);
		}

		#endregion

		#region UseFormBuilderHouseBills

		public void TestUseFormBuilderHouseBills()
		{
			TestRegistryItem(RawDataRegistry.Instance.UseFormBuilderHouseBills,
				"UseFormBuilderHouseBills",
				"Freight/House Bills",
				"Use new Form Builder House Bills",
				"If turned on CW1 will use new Form Builder house bills.",
				RegistryStorageFlags.System,
				true);
		}

		#endregion

		#region PrintChargesBilledToLocalClientAtDestAsCollect

		public void TestPrintChargesBilledToLocalClientAtDestAsCollect()
		{
			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(ItemSet.PrintChargesBilledToLocalClientAtDestAsCollect,
					"PrintChargesBilledToLocalClientAtDestAsCollect",
					"Freight/House Bills",
					"Print Charges billed to Local Client at Destination as Collect",
					"This registry allows you to configure Shipments under the combination of Transport Mode, Country and Direction printing charges billed to Local Client at Destination as Collect Charges.",
					RegistryStorageFlags.System,
					RegistryOptions.Default
				);
			}

			RegistryItemDictionary.Instance.PurgeAll();

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.Options);
			}
		}

		#endregion

		public void TestEnableHouseBillOfLadingRegistryItems()
		{
			TestRegistryItem(ItemSet.EnableHouseBillOfLadingRegistryItems, "EnableHouseBillOfLadingRegistryItems",
				"Freight/House Bills/House Bill of Lading Types",
				"Enable House Bill of Lading Registry Items",
				"By default the House Bill of Lading Registry Items are visible in the Registry. Change this setting to NO to make them hidden.\r\nThis is a Support only registry setting.",
				RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue, true);
		}

		public void TestFIATAAuthorised()
		{
			TestRegistryItem(ItemSet.FIATAAuthorised, "FIATAAuthorised",
				"Freight/House Bills/House Bill of Lading Types/FIATA HBL Settings",
				"FIATA Authorization (Y/N)",
				"Only a valid and financial member of a forwarding association that is itself a member of FIATA is authorized to use the FIATA logo on the FIATA Bill of Lading.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, false);
		}

		public void TestEnableFIATAHouseBillsFeatures()
		{
			TestRegistryItem(ItemSet.EnableFIATAHouseBillsFeatures,
				"EnableFIATAHouseBillsFeatures",
				"Freight/House Bills/House Bill of Lading Types/FIATA HBL Settings",
				"Enable electronic FIATA Bills of Lading (eFBL)",
				"If yes, electronic FIATA Bills of Lading functionality will be enabled for to use.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestAddRegistryItemsIfEnableHouseBillOfLadingRegistryItemsIsFalse()
		{
			ItemSet.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertNotVisible(ItemSet.HouseBillOfLadingLogoImages);
			AssertNotVisible(ItemSet.HouseBillOfLadingTermsAndConditionsImages);
			AssertNotVisible(ItemSet.HouseBillOfLadingLogoTypes);
			AssertNotVisible(ItemSet.FIATAAuthorised);
		}

		public void TestAddRegistryItemsIfEnableHouseBillOfLadingRegistryItemsIsTrue()
		{
			ItemSet.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertVisible(ItemSet.HouseBillOfLadingLogoImages);
			AssertVisible(ItemSet.HouseBillOfLadingTermsAndConditionsImages);
			AssertVisible(ItemSet.HouseBillOfLadingLogoTypes);
			AssertVisible(ItemSet.FIATAAuthorised);
		}

		public void TestHBLPackLinesDisplayOrder()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(Constants.HBLPackLinesDisplayOrders.ContainerAndPackingOrder, (NoResString)"Container + Packing Order");
			list.AddPair(Constants.HBLPackLinesDisplayOrders.ShowDGCargoFirst, (NoResString)"Show DG Cargo First");

			TestRegistryItem(
				ItemSet.HBLPackLinesDisplayOrder,
				"HBLPackLinesDisplayOrder",
				"Freight/House Bills",
				"Show Pack Lines in Order",
				"Enable this option to show pack lines in order on House Bills.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				list,
				Constants.HBLPackLinesDisplayOrders.ContainerAndPackingOrder);
		}

		public void TestHouseBillOfLadingTermsAndConditionsImages()
		{
			HouseBillOfLadingTermsAndConditionsCollection collection = new HouseBillOfLadingTermsAndConditionsCollection();
			HouseBillOfLadingTermsAndConditions termsAndConditions = collection.AddNew();

			using (Image image = new Bitmap(1, 1))
			{
				termsAndConditions.Code = "ABC";
				termsAndConditions.Description = (NoResString)"ABC Description";
				termsAndConditions.Image = image;
				termsAndConditions.DeliveryMode = "ALL";

				ItemSet.HouseBillOfLadingTermsAndConditionsImages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
				HouseBillOfLadingTermsAndConditionsCollection value = ItemSet.HouseBillOfLadingTermsAndConditionsImages.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

				AssertEquals("Value.Count", 1, value.Count);
				AssertEquals("Value[0].Code", "ABC", value[0].Code);
				AssertEquals("Value[0].Description", "ABC Description", value[0].Description);
				AssertEquals("Value[0].Image", true, Utilities.IsImageEqual(image, value[0].Image));
				AssertEquals("Value[0].DeliveryMode", "ALL", value[0].DeliveryMode);
			}
		}

		public void TestRegistryImageContainer()
		{
			TestRegistryItem(ItemSet.RegistryImageContainer, "REGISTRY_IMAGE_CONTAINER", "", "", "", RegistryStorageFlags.Company, RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue);
		}

		#region House Bill of Lading Types

		public void TestHouseBillOfLadingLogoTypes()
		{
			var collection = new HouseBillOfLadingTypeCollection();

			var houseBillOfLadingType = collection.AddNew();

			houseBillOfLadingType.LogoImageCodeList.AddPair("123", "");
			houseBillOfLadingType.TermsAndConditionImageCodeList.AddPair("456", "");

			houseBillOfLadingType.Code = "ABC";
			houseBillOfLadingType.Description = (NoResString)"XYZ";
			houseBillOfLadingType.LogoCode = "123";
			houseBillOfLadingType.TermsAndConditionsCode = "456";

			ItemSet.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var value = ItemSet.HouseBillOfLadingLogoTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("Value.Count", 1, value.Count);
			AssertEquals("Value[0].Code", "ABC", value[0].Code);
			AssertEquals("Value[0].Description", "XYZ", value[0].Description);
		}

		public void TestHouseBillOfLadingLogoTypesForSeaDefaultValue()
		{
			var defaultValue = GetNewItemSet().HouseBillOfLadingLogoTypes.DefaultValue;

			AssertEquals("DefaultValue.Count", 19, defaultValue.Count);
			AssertHouseBillOfLadingTypeProperties(defaultValue[0], "EAG", "CargoWise Bill", "", "", false, true, PrintLogoOptions.Codes.All);
			AssertHouseBillOfLadingTypeProperties(defaultValue[1], "EAP", "CargoWise Bill Preprinted", "", "", true, false, PrintLogoOptions.Codes.None);
			AssertHouseBillOfLadingTypeProperties(defaultValue[2], "FIA", "FIATA Bill", "", "FIA", false, true, PrintLogoOptions.Codes.All);
			AssertHouseBillOfLadingTypeProperties(defaultValue[3], "FIP", "FIATA Bill Preprinted", "", "", true, false, PrintLogoOptions.Codes.None);
			AssertHouseBillOfLadingTypeProperties(defaultValue[4], "IAU", "IT Club Australia", "", "IAU", false, true, PrintLogoOptions.Codes.All);
			AssertHouseBillOfLadingTypeProperties(defaultValue[5], "ISI", "IT Club Australia No Terms", "", "", false, true, PrintLogoOptions.Codes.All);
			AssertHouseBillOfLadingTypeProperties(defaultValue[6], "INN", "IT Club Australia No Terms No Law", "", "", false, true, PrintLogoOptions.Codes.All);
			AssertHouseBillOfLadingTypeProperties(defaultValue[7], "ITP", "IT Club Australia Preprinted", "", "", true, false, PrintLogoOptions.Codes.None);
			AssertHouseBillOfLadingTypeProperties(defaultValue[8], "TNZ", "TT Club Bill", "", "TTC", false, true, PrintLogoOptions.Codes.All);
			AssertHouseBillOfLadingTypeProperties(defaultValue[9], "TTN", "TT Club Bill No Terms", "", "", false, true, PrintLogoOptions.Codes.All);
			AssertHouseBillOfLadingTypeProperties(defaultValue[10], "TTP", "TT Club Bill Preprinted", "", "", true, false, PrintLogoOptions.Codes.None);
			AssertHouseBillOfLadingTypeProperties(defaultValue[11], "TAN", "TAN Bill of Lading", "", "", false, true, PrintLogoOptions.Codes.All);
			AssertHouseBillOfLadingTypeProperties(defaultValue[12], "TAP", "TAN Bill Preprinted", "", "", true, false, PrintLogoOptions.Codes.None);
			AssertHouseBillOfLadingTypeProperties(defaultValue[13], "DHK", "Datahawk Bill of Lading", "", "", false, true, PrintLogoOptions.Codes.All);
			AssertHouseBillOfLadingTypeProperties(defaultValue[14], "INZ", "IT Club New Zealand", "", "INZ", false, true, PrintLogoOptions.Codes.All);
			AssertHouseBillOfLadingTypeProperties(defaultValue[15], "INP", "IT Club New Zealand Preprinted", "", "", true, false, PrintLogoOptions.Codes.None);
			AssertHouseBillOfLadingTypeProperties(defaultValue[16], "TUS", "TT Club United States", "", "TUS", false, false, PrintLogoOptions.Codes.None);
			AssertHouseBillOfLadingTypeProperties(defaultValue[17], "TUP", "TT Club United States Preprinted", "", "", true, false, PrintLogoOptions.Codes.None);
			AssertHouseBillOfLadingTypeProperties(defaultValue[18], "CPT", "Carta Porte - Spanish", "", "", false, true, PrintLogoOptions.Codes.All);
		}

		void AssertHouseBillOfLadingTypeProperties(HouseBillOfLadingType hblType, string code, string description, string logoCode, string termsAndConditionsCode, bool prePrinted, ZBool printLogo, ZString printLogoInFormBuilder)
		{
			AssertEquals("hblType.Code", code, hblType.Code);
			AssertEquals("hblType.Description", description, hblType.Description);
			AssertEquals("hblType.LogoCode", logoCode, hblType.LogoCode);
			AssertEquals("hblType.TermsAndConditionsCode", termsAndConditionsCode, hblType.TermsAndConditionsCode);
			AssertEquals("hblType.PrePrinted", prePrinted, hblType.PrePrinted);
			AssertEquals("hblType.PrintLogo", printLogo, hblType.PrintLogo);
			AssertEquals("hblType.PrintLogoInFormBuilder", printLogoInFormBuilder, hblType.PrintLogoInFormBuilder);
		}

		#endregion

		#region Addtional House Bill of Lading Types

		public void TestAddtionalHouseBillOfLadingTypes()
		{
			var itemSet = GetNewItemSet();
			var defaultValue = itemSet.AddtionalHouseBillOfLadingTypes.DefaultValue;

			var defaultValuesFormatted = defaultValue
				.OfType<AdditionalHouseBillOfLadingType>()
				.Select(v => $"{v.Code}|{v.Description}|{v.Enable}|{v.EnableMessaging}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder("",
				new[]
				{
					"YUS|Yusen HBL|N|N",
					"YS1|Yusen HBL Style 1|N|N",
					"YS2|Yusen HBL Style 2|N|N",
					"YS3|Yusen HBL Style 3|N|N",
					"YS4|Yusen HBL Style 4|N|N",
					"DHL|DHL HBL|N|N",
					"DH1|DHL HBL Style 1|N|N",
					"DH2|DHL HBL Style 2|N|N"
				},
				defaultValuesFormatted);

			var defaultValuesOverride = new AdditionalHouseBillOfLadingTypeCollection();
			var yusType = new AdditionalHouseBillOfLadingType
			{
				Code = Constants.AddtionalHouseBillTypeMenu.Code.YusenHBL,
				Description = (NoResString)"Yusen HBL",
				Enable = true,
				EnableMessaging = true
			};
			defaultValuesOverride.Add(yusType);

			itemSet.AddtionalHouseBillOfLadingTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValuesOverride);
			var value = itemSet.AddtionalHouseBillOfLadingTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("DefaultValue.Count", 1, value.Count);
			AssertAddtionalHouseBillOfLadingTypesProperties(value[0], "YUS", "Yusen HBL", true, true);
		}

		void AssertAddtionalHouseBillOfLadingTypesProperties(AdditionalHouseBillOfLadingType hblType, string code, string description, bool enable, bool enalbeMessaging)
		{
			AssertEquals("hblType.Code", code, hblType.Code);
			AssertEquals("hblType.Description", description, hblType.Description);
			AssertEquals("hblType.Enable", enable, hblType.Enable);
			AssertEquals("hblType.EnableMessaging", enalbeMessaging, hblType.EnableMessaging);
		}

		#endregion

		#region House Bill Number Customisation

		public void TestHouseBillNumberCustomisation()
		{
			AssertEquals(RegistryStorageFlags.All, ItemSet.HouseBillNumberCustomisation.Storage);

			BillOfLadingNumberCustomisationsByServiceLevel customisations = new BillOfLadingNumberCustomisationsByServiceLevel();

			customisations.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix = true;
			customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";

			ItemSet.HouseBillNumberCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisations);

			BillOfLadingNumberCustomisationsByServiceLevel value = ItemSet.HouseBillNumberCustomisation.Value;

			AssertEquals("Value.RemoveSPrefix", true, value.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Default, value.Categories);
			AssertEquals("Value.AllowNonAlphanumericCharacters", true, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", true, value.EnableMacroInsertion);

			var dataType = ItemSet.HouseBillNumberCustomisation.DataType as BillCustomisationByServiceLevelRegistryDataType;
			AssertEquals("DataType.MacroType", ObjectFactory.GetType<IForwardingShipment>(), dataType.MacroType);
		}

		public void TestHouseBillNumberCustomisationIsAddedToRegistry()
		{
			AssertVisible(ItemSet.HouseBillNumberCustomisation);
		}

		public void TestHouseBillNumberCustomisation_SEA()
		{
			AssertEquals(RegistryStorageFlags.All, ItemSet.HouseBillNumberCustomisation_SEA.Storage);

			BillOfLadingNumberCustomisationsByServiceLevel customisations = new BillOfLadingNumberCustomisationsByServiceLevel();

			customisations.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix = true;
			customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";

			ItemSet.HouseBillNumberCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisations);

			BillOfLadingNumberCustomisationsByServiceLevel value = ItemSet.HouseBillNumberCustomisation_SEA.Value;

			AssertEquals("Value.RemoveSPrefix", true, value.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Default, value.Categories);
			AssertEquals("Value.AllowNonAlphanumericCharacters", true, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", true, value.EnableMacroInsertion);

			var dataType = ItemSet.HouseBillNumberCustomisation_SEA.DataType as BillCustomisationByServiceLevelRegistryDataType;
			AssertEquals("DataType.MacroType", ObjectFactory.GetType<IForwardingShipment>(), dataType.MacroType);
		}

		public void TestHouseBillNumberCustomisation_AIR()
		{
			AssertEquals(RegistryStorageFlags.All, ItemSet.HouseBillNumberCustomisation_AIR.Storage);

			BillOfLadingNumberCustomisationsByServiceLevel customisations = new BillOfLadingNumberCustomisationsByServiceLevel();

			customisations.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix = true;
			customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";

			ItemSet.HouseBillNumberCustomisation_AIR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisations);

			BillOfLadingNumberCustomisationsByServiceLevel value = ItemSet.HouseBillNumberCustomisation_AIR.Value;

			AssertEquals("Value.RemoveSPrefix", true, value.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Default, value.Categories);
			AssertEquals("Value.AllowNonAlphanumericCharacters", true, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", true, value.EnableMacroInsertion);

			var dataType = ItemSet.HouseBillNumberCustomisation_AIR.DataType as BillCustomisationByServiceLevelRegistryDataType;
			AssertEquals("DataType.MacroType", ObjectFactory.GetType<IForwardingShipment>(), dataType.MacroType);
		}

		public void TestHouseBillNumberCustomisation_RAIL()
		{
			AssertEquals(RegistryStorageFlags.All, ItemSet.HouseBillNumberCustomisation_RAIL.Storage);

			BillOfLadingNumberCustomisationsByServiceLevel customisations = new BillOfLadingNumberCustomisationsByServiceLevel();

			customisations.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix = true;
			customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";

			ItemSet.HouseBillNumberCustomisation_RAIL.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisations);

			BillOfLadingNumberCustomisationsByServiceLevel value = ItemSet.HouseBillNumberCustomisation_RAIL.Value;

			AssertEquals("Value.RemoveSPrefix", true, value.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Default, value.Categories);
			AssertEquals("Value.AllowNonAlphanumericCharacters", true, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", true, value.EnableMacroInsertion);

			var dataType = ItemSet.HouseBillNumberCustomisation_RAIL.DataType as BillCustomisationByServiceLevelRegistryDataType;
			AssertEquals("DataType.MacroType", ObjectFactory.GetType<IForwardingShipment>(), dataType.MacroType);
		}

		public void TestHouseBillNumberCustomisation_ROAD()
		{
			AssertEquals(RegistryStorageFlags.All, ItemSet.HouseBillNumberCustomisation_ROAD.Storage);

			BillOfLadingNumberCustomisationsByServiceLevel customisations = new BillOfLadingNumberCustomisationsByServiceLevel();

			customisations.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix = true;
			customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";

			ItemSet.HouseBillNumberCustomisation_ROAD.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisations);

			BillOfLadingNumberCustomisationsByServiceLevel value = ItemSet.HouseBillNumberCustomisation_ROAD.Value;

			AssertEquals("Value.RemoveSPrefix", true, value.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Default, value.Categories);
			AssertEquals("Value.AllowNonAlphanumericCharacters", true, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", true, value.EnableMacroInsertion);

			var dataType = ItemSet.HouseBillNumberCustomisation_ROAD.DataType as BillCustomisationByServiceLevelRegistryDataType;
			AssertEquals("DataType.MacroType", ObjectFactory.GetType<IForwardingShipment>(), dataType.MacroType);
		}

		#endregion

		#endregion

		#region Container

		public void TestDefaultContainerDetentionFreeDaysForImport()
		{
			TestGenericRegistryItem(
				ItemSet.DefaultContainerDetentionFreeDaysForImport,
				"DefaultContainerDetentionFreeDaysForImport",
				"Freight/Container",
				"Container Detention Free Days for Import", "Number of days that a container will be held in detention free of charge.\r\n\r\nIf Unlimited Free Days is ticked this assumes no penalty to apply.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new ContainerPenaltyFreeDaysOptions { FreeDays = 10, UnlimitedFreeDays = false });
		}

		public void TestDefaultContainerDetentionFreeDaysForExport()
		{
			TestGenericRegistryItem(
				ItemSet.DefaultContainerDetentionFreeDaysForExport,
				"DefaultContainerDetentionFreeDaysForExport",
				"Freight/Container",
				"Container Detention Free Days for Export",
				"Number of days that a container will be held in detention free of charge.\r\n\r\nIf Unlimited Free Days is ticked this assumes no penalty to apply.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue, new ContainerPenaltyFreeDaysOptions { FreeDays = 10, UnlimitedFreeDays = false });
		}

		public void TestDefaultFreeCTOStorageDaysForImport()
		{
			TestGenericRegistryItem(
				ItemSet.DefaultFreeCTOStorageDaysForImport,
				"DefaultFreeCTOStorageDaysForImport",
				"Freight/Container",
				"CTO Storage Free Days for Import",
				"Number of days that a container will be stored for free with the CTO for imports before incurring CTO storage charges.\r\n\r\nIf Unlimited Free Days is ticked this assumes no penalty to apply.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue, new ContainerPenaltyFreeDaysOptions { FreeDays = 0, UnlimitedFreeDays = false });
		}

		public void TestDefaultFreeCTOStorageDaysForExport()
		{
			TestGenericRegistryItem(
				ItemSet.DefaultFreeCTOStorageDaysForExport,
				"DefaultFreeCTOStorageDaysForExport",
				"Freight/Container",
				"CTO Storage Free Days for Export",
				"Number of days that a container will be stored for free with the CTO for exports before incurring CTO storage charges.\r\n\r\nIf Unlimited Free Days is ticked this assumes no penalty to apply.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue, new ContainerPenaltyFreeDaysOptions { FreeDays = 0, UnlimitedFreeDays = false });
		}

		public void TestVGMVerifiedByDefaultsTo()
		{
			AssertEquals("", ItemSet.VGMVerifiedByDefaultsTo.Value);
			ItemSet.VGMVerifiedByDefaultsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.VGMVerifiedParties.OwnSendingAgent);
			AssertEquals("Freight/Container", ItemSet.VGMVerifiedByDefaultsTo.Category);
			AssertEquals(Constants.VGMVerifiedParties.OwnSendingAgent, ItemSet.VGMVerifiedByDefaultsTo.Value);

			TestRegistryItem(ItemSet.VGMVerifiedByDefaultsTo,
				"VGMVerifiedByDefaultsTo",
				"Freight/Container",
				"VGM Verified By Defaults To",
				"This governs which address appears in the Consol > Container > VGM > VGM Verified By for non-Direct consols by default",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				ItemSet.VGMVerifiedPartiesListProvider.CodeDescriptionPairList,
				ZString.Empty);
		}

		public void TestContainerDeliveryModeList()
		{
			var testList = ItemSet.ContainerDeliveryModeList.DefaultValue;

			AssertEquals("ContainerDeliveryModeList", ItemSet.ContainerDeliveryModeList.Name);
			AssertEquals("Freight/Container", ItemSet.ContainerDeliveryModeList.Category);
			AssertEquals("A list of container delivery mode types.", ItemSet.ContainerDeliveryModeList.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.ContainerDeliveryModeList.Storage);
			AssertEquals("CFS/CFS", ItemSet.ContainerDeliveryModeList.DefaultValue.DefaultCode);
			Assert("Should contain 1 default element", ItemSet.ContainerDeliveryModeList.DefaultValue.ContainsCode(testList[0].Code));
			Assert("Should contain 2 default element", ItemSet.ContainerDeliveryModeList.DefaultValue.ContainsCode(testList[1].Code));
			Assert("Should contain 3 default element", ItemSet.ContainerDeliveryModeList.DefaultValue.ContainsCode(testList[2].Code));
			Assert("Should contain 4 default element", ItemSet.ContainerDeliveryModeList.DefaultValue.ContainsCode(testList[3].Code));
		}

		public void TestContainerDeliveryModeOverride()
		{
			TestRegistryItem(ItemSet.ContainerDeliveryModeOverride, "ContainerDeliveryModeOverride", "Freight/Container", "Container Delivery Mode Override", "Use this registry upon customer request to substitute system defined Container Delivery Modes with user preferred options in Forwarding and Liner & Agency containers.  System defined codes will be substituted on the forms and documents but not in the database schema or Universal Shipment XML as to not break the integrity of external integrations.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestContainerQuantityList()
		{
			var expectedDefaultList = new List<(string, string)>();
			expectedDefaultList.Add(("GEN", "General"));
			expectedDefaultList.Add(("RIC", "Rice"));
			expectedDefaultList.Add(("HID", "Hides"));
			expectedDefaultList.Add(("VEN", "Vent General"));
			expectedDefaultList.Add(("REP", "Reposition"));
			expectedDefaultList.Add(("LBG", "Liner Bag"));
			expectedDefaultList.Add(("SFR", "Super Freezer"));
			expectedDefaultList.Add(("FOD", "Food"));
			expectedDefaultList.Add(("GOH", "Garments on Hangers"));
			expectedDefaultList.Add(("GOS", "Garments on Hangers Single Bar"));
			expectedDefaultList.Add(("GOD", "Garments on Hangers Double Bar"));

			AssertEquals("ContainerQualityList", ItemSet.ContainerQualityList.Name);
			AssertEquals("Freight/Container", ItemSet.ContainerQualityList.Category);
			AssertEquals("A list of container quality types.", ItemSet.ContainerQualityList.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ContainerQualityList.Storage);
			AssertEquals(expectedDefaultList.Count, ItemSet.ContainerQualityList.DefaultValue.Count);

			foreach (var (code, description) in expectedDefaultList)
			{
				Assert($"Should contains default value {code}", ItemSet.ContainerQualityList.DefaultValue.ContainsCode(code));
				AssertEquals(description, ItemSet.ContainerQualityList.DefaultValue.GetDescriptionFromCode(code));
			}
		}

		#endregion

		#region Landed Costing Preferences

		public void TestLandedCostingPreferences()
		{
			LandedCostingGroupCollection defaultValue = ItemSet.LandedCostingPreferences.DefaultValue;

			AssertEquals("DefaultValue.Count", 5, defaultValue.Count);

			CheckLandedCostingGroupProperties(defaultValue[0], 1, "Origin Charges", new string[] { "ORG", "LOD" });
			CheckLandedCostingGroupProperties(defaultValue[1], 2, "Intl Freight Charges", new string[] { "FRT" });
			CheckLandedCostingGroupProperties(defaultValue[2], 3, "Destination Charges", new string[] { "DST", "UNL" });
			CheckLandedCostingGroupProperties(defaultValue[3], 4, "Entry Charges", Array.Empty<string>());
			CheckLandedCostingGroupProperties(defaultValue[4], 5, "Brokerage Charges", new string[] { "BRK" });

			LandedCostingGroupCollection collection = new LandedCostingGroupCollection();
			LandedCostingGroup landedCostingGroup = collection.AddNew();

			landedCostingGroup.GroupID = 123;
			landedCostingGroup.GroupName = "Group 123";
			landedCostingGroup.CostDistributionCode = "VOL";

			ItemSet.LandedCostingPreferences.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			AssertEquals("Value[0].GroupID", 123, ItemSet.LandedCostingPreferences.Value[0].GroupID);
			AssertEquals("Value[0].GroupName", "Group 123", ItemSet.LandedCostingPreferences.Value[0].GroupName);
			AssertEquals("Value[0].CostDistributionCode", "VOL", ItemSet.LandedCostingPreferences.Value[0].CostDistributionCode);
		}

		void CheckLandedCostingGroupProperties(LandedCostingGroup landedCostingGroup, int groupID, string groupName, string[] chargeGroups)
		{
			AssertEquals("GroupID", groupID, landedCostingGroup.GroupID);
			AssertEquals("GroupName", groupName, landedCostingGroup.GroupName);
			AssertEquals("CostDistributionCode", "AWV", landedCostingGroup.CostDistributionCode);
			AssertEquals("Charges.Count", chargeGroups.Length, landedCostingGroup.Charges.Count);

			foreach (string chargeGroup in chargeGroups)
			{
				bool foundGroup = false;

				foreach (ChargeGroupAndChargeCode charge in landedCostingGroup.Charges)
				{
					if (charge.ChargeGroupCode == chargeGroup)
					{
						foundGroup = true;
						break;
					}
				}

				AssertEquals(string.Format("The Charge Group \"{0}\" was not found.", chargeGroup), true, foundGroup);
			}
		}

		#endregion

		#region Notifications

		#region Sailing Schedules

		public void TestSailingScheduleItemsAddedToList()
		{
			AssertVisible(ItemSet.SeaScheduleChangeNotificationGroup);
			AssertVisible(ItemSet.AirScheduleChangeNotificationGroup);
			AssertVisible(ItemSet.RoadScheduleChangeNotificationGroup);
			AssertVisible(ItemSet.RailScheduleChangeNotificationGroup);

			AssertVisible(ItemSet.SailingSchedulesFeedLastReceivedItem, true);
		}

		#region ScheduleChangeNotificationGroups

		public void TestSeaScheduleChangeNotificationGroup()
		{
			TestScheduleChangeNotificationGroup("SeaScheduleChangeNotificationGroup", ItemSet.SeaScheduleChangeNotificationGroup);
		}

		public void TestAirScheduleChangeNotificationGroup()
		{
			TestScheduleChangeNotificationGroup("AirScheduleChangeNotificationGroup", ItemSet.AirScheduleChangeNotificationGroup);
		}

		public void TestRoadScheduleChangeNotificationGroup()
		{
			TestScheduleChangeNotificationGroup("RoadScheduleChangeNotificationGroup", ItemSet.RoadScheduleChangeNotificationGroup);
		}

		public void TestRailScheduleChangeNotificationGroup()
		{
			TestScheduleChangeNotificationGroup("RailScheduleChangeNotificationGroup", ItemSet.RailScheduleChangeNotificationGroup);
		}

		void TestScheduleChangeNotificationGroup(string expectedName, GuidRegistryItem notificationGroupRegistryItem)
		{
			AssertEquals("Name", expectedName, notificationGroupRegistryItem.Name);
			AssertEquals("Storage", RegistryStorageFlags.System, notificationGroupRegistryItem.Storage);
			AssertEquals("Options", RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue, notificationGroupRegistryItem.Options);
			AssertEquals("EditorInfo", RegistryFindBoxCollection.GlbGroup, ((GuidFindBoxRegistryEditorInfo)notificationGroupRegistryItem.EditorInfo).FindBoxCollection);

			AssertEquals("Default to the All staff", Core.Constants.Groups.AllPK, notificationGroupRegistryItem.Value);
			Guid groupPK = Guid.NewGuid();
			notificationGroupRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupPK);
			AssertEquals("Value", groupPK, notificationGroupRegistryItem.Value);
		}

		#endregion

		#endregion

		#region HourlyFreightNotificationEmailsLastSent

		public void TestHourlyFreightNotificationEmailsLastSent()
		{
			AssertVisible(ItemSet.HourlyFreightNotificationEmailsLastSent, true);

			AssertEquals("Defaults to an empty date", DateTime.MinValue, ItemSet.HourlyFreightNotificationEmailsLastSent.Value);
			ZDateTime newValue = ZDateTime.Now;
			ItemSet.HourlyFreightNotificationEmailsLastSent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue.ToDateTime());
			AssertEquals("Value", newValue, ItemSet.HourlyFreightNotificationEmailsLastSent.Value);

			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.HourlyFreightNotificationEmailsLastSent.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue, ItemSet.HourlyFreightNotificationEmailsLastSent.Options);
		}

		#endregion

		#endregion

		#region 1-Stop Integration

		#region OneStopContainerEventsEnabled

		public void TestOneStopContainerEventsEnabled()
		{
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.OneStopContainerEventsEnabled.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue, ItemSet.OneStopContainerEventsEnabled.Options);
			AssertEquals("Default to true", true, ItemSet.OneStopContainerEventsEnabled.Value);

			ItemSet.OneStopContainerEventsEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.OneStopContainerEventsEnabled.Value);
		}

		public void TestOneStopContainerEventsEnabledNZ()
		{
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.OneStopContainerEventsEnabledNZ.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden, ItemSet.OneStopContainerEventsEnabledNZ.Options);
			AssertEquals("Default to true", true, ItemSet.OneStopContainerEventsEnabledNZ.Value);

			ItemSet.OneStopContainerEventsEnabledNZ.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.OneStopContainerEventsEnabledNZ.Value);
		}

		public void TestComTracTestMode()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ComTracTestMode.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ComTracTestMode.Options);
			AssertEquals("Default to false", false, ItemSet.ComTracTestMode.Value);

			ItemSet.ComTracTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.ComTracTestMode.Value);
		}

		public void TestOneStopContainerEventsEnabledIsVisibleWhenAllowed()
		{
			ItemSet.HasOneStopAUContainerIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, ItemSet.OneStopContainerEventsEnabled.Options);
		}

		public void TestOneStopContainerEventsEnabledNZIsVisibleWhenAllowed()
		{
			ItemSet.HasOneStopNZContainerIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.OneStopContainerEventsEnabledNZ.Options);
		}

		#endregion

		#region OneStopNotificationGroup

		public void TestOneStopNotificationGroup()
		{
			AssertEquals("Name", "OneStopNotificationGroup", ItemSet.OneStopNotificationGroup.Name);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.OneStopNotificationGroup.Storage);
			Assert("Options", (ItemSet.OneStopNotificationGroup.Options & RegistryOptions.IsValueOptional) != 0 && (ItemSet.OneStopNotificationGroup.Options & RegistryOptions.IsHidden) != 0);
			AssertEquals("EditorInfo", RegistryFindBoxCollection.GlbGroup, ((GuidFindBoxRegistryEditorInfo)ItemSet.OneStopNotificationGroup.EditorInfo).FindBoxCollection);

			AssertEquals("Default to the All staff", Core.Constants.Groups.AllPK, ItemSet.OneStopNotificationGroup.Value);
			Guid groupPK = Guid.NewGuid();
			ItemSet.OneStopNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupPK);
			AssertEquals("Value", groupPK, ItemSet.OneStopNotificationGroup.Value);
		}

		public void TestOneStopNotificationGroupIsVisibleWhenAllowed()
		{
			ItemSet.HasOneStopAUContainerIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("Options", (ItemSet.OneStopNotificationGroup.Options & RegistryOptions.IsValueOptional) != 0 && (ItemSet.OneStopNotificationGroup.Options & RegistryOptions.IsHidden) == 0);
		}

		#endregion

		#region SailingSchedulesFeedLastReceived

		public void TestSailingSchedulesFeedLastReceived()
		{
			AssertEquals("Defaults to an empty date", ZDateTime.Empty, ItemSet.SailingSchedulesFeedLastReceived);

			ItemSet.SailingSchedulesFeedLastReceived = new ZDateTime(2000, 5, 5);
			AssertEquals("A valid date", new ZDateTime(2000, 5, 5), ItemSet.SailingSchedulesFeedLastReceived);

			ItemSet.SailingSchedulesFeedLastReceived = ZDateTime.Empty;
			AssertEquals("An empty date", ZDateTime.Empty, ItemSet.SailingSchedulesFeedLastReceived);

			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.SailingSchedulesFeedLastReceivedItem.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue, ItemSet.SailingSchedulesFeedLastReceivedItem.Options);
		}
		#endregion

		#endregion

		#region Chargeable

		public void DefaultChargeableVolumeWeightUnitsTest()
		{
			AssertEquals(FreightDataRegistry.Instance.GetDefaultChargeableWeightUnit(true), Constants.Weight.Kilograms);
			AssertEquals(FreightDataRegistry.Instance.GetDefaultChargeableWeightUnit(false), Constants.Weight.Pounds);
			AssertEquals(FreightDataRegistry.Instance.GetDefaultChargeableWeightUnit(true), Constants.Volume.CubicMetres);
			AssertEquals(FreightDataRegistry.Instance.GetDefaultChargeableWeightUnit(false), Constants.Volume.CubicFeet);
		}

		public void WeightChargeableTransportModesTest()
		{
			AssertContainsExactElementsInAnyOrder(new string[]
			{
				Constants.TransportModes.Air,
				Constants.TransportModes.AirSea,
				Constants.TransportModes.Road,
				Constants.TransportModes.Courier,
				Constants.TransportModes.Mail,
				Constants.TransportModes.WarehouseHandling,
				Constants.TransportModes.Storage
			}, FreightDataRegistry.Instance.WeightChargableTransportModes);
		}

		public void VolumeChargeableTransportModesTest()
		{
			AssertContainsExactElementsInAnyOrder(new string[]
			{
				Constants.TransportModes.Rail,
				Constants.TransportModes.Sea,
				Constants.TransportModes.SeaAir,
			}, FreightDataRegistry.Instance.VolumeChargableTransportModes);
		}

		const string WeightChargeableHint = @"The chargeable factor will be used to convert the actual volume into a chargeable weight.

The chargeable weight is the greater of the actual weight or actual volume divided by the chargeable factor.

Max of [ weight(KG) , volume(CM3) / factor ] = KG

Default chargeable factor is 6000 CC/KG.

Max of [ weight(LB) , volume(CI) / factor ] = LB

Default chargeable factor is 194 CI/LB.";

		const string WeightChargeableHintForRoadTransport = @"The chargeable factor will be used to convert the actual volume into a chargeable weight.

The chargeable weight is the greater of the actual weight or actual volume divided by the chargeable factor.

Max of [ weight(KG) , volume(CM3) / factor ] = KG

Default chargeable factor is 333 KG/M3.

Max of [ weight(LB) , volume(CI) / factor ] = LB

Default chargeable factor is 194 CI/LB.";

		const string WeightChargeableHintForInternationalAir = @"The chargeable factor will be used to convert the actual volume into a chargeable weight.

The chargeable weight is the greater of the actual weight or actual volume divided by the chargeable factor.

Max of [ weight(KG) , volume(CM3) / factor ] = KG

Default chargeable factor is 6000 CC/KG.

Max of [ weight(LB) , volume(CI) / factor ] = LB

Default chargeable factor is 166 CI/LB.";

		const string VolumeChargeableHint = @"The chargeable factor will be used to convert the actual weight into a chargeable volume.

The chargeable volume is the greater of the actual volume or actual weight divided by the chargeable factor.

Max of [ volume(M3) , weight(KG) / factor ] = M3

Default chargeable factor is 1000 KG/M3.

Max of [ volume(CF) , weight(LB) / factor ] = CF

Default chargeable factor is 100 LB/CF.";

		#region Domestic Chargeable Factor

		public void TestDomesticChargeableFactorAir()
		{
			BusinessObject australianCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject australianBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Core.Constants.CountryCodes.Australia;
			australianBranch[GlbBranchSchema.Constants.GB_GC] = australianCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, australianBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				IRegistryItemInternals internals = ItemSet.DomesticChargeableFactorAir;

				TestGenericRegistryItem(internals,
				"DomesticChargeableFactorAir", "Freight/Chargeable/Domestic Chargeable",
				"Chargeable Factor for Air", WeightChargeableHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic));
			}
		}

		public void TestDomesticChargeableFactorRoad()
		{
			BusinessObject australianCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject australianBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Core.Constants.CountryCodes.Australia;
			australianBranch[GlbBranchSchema.Constants.GB_GC] = australianCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, australianBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				IRegistryItemInternals internals = ItemSet.DomesticChargeableFactorRoad;

				TestGenericRegistryItem(internals,
				"DomesticChargeableFactorRoad", "Freight/Chargeable/Domestic Chargeable",
				"Chargeable Factor for Road", WeightChargeableHintForRoadTransport,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Road, ConversionFactor.Standard.Imperial.Domestic));
			}
		}

		public void TestDomesticChargeableFactorCourier()
		{
			BusinessObject australianCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject australianBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Core.Constants.CountryCodes.Australia;
			australianBranch[GlbBranchSchema.Constants.GB_GC] = australianCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, australianBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				IRegistryItemInternals internals = ItemSet.DomesticChargeableFactorCourier;

				TestGenericRegistryItem(internals,
				"DomesticChargeableFactorCourier", "Freight/Chargeable/Domestic Chargeable",
				"Chargeable Factor for Courier", WeightChargeableHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic));
			}
		}

		public void TestDomesticChargeableFactorSea()
		{
			BusinessObject usCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject usBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			usCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Core.Constants.CountryCodes.UnitedStates;
			usBranch[GlbBranchSchema.Constants.GB_GC] = usCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, usBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				IRegistryItemInternals internals = ItemSet.DomesticChargeableFactorSea;

				TestGenericRegistryItem(internals,
				"DomesticChargeableFactorSea", "Freight/Chargeable/Domestic Chargeable",
				"Chargeable Factor for Sea", VolumeChargeableHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Sea, ConversionFactor.Standard.Imperial.Sea));
			}
		}

		public void TestDomesticChargeableFactorRail()
		{
			BusinessObject australianCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject australianBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Core.Constants.CountryCodes.Australia;
			australianBranch[GlbBranchSchema.Constants.GB_GC] = australianCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, australianBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				IRegistryItemInternals internals = ItemSet.DomesticChargeableFactorRail;

				TestGenericRegistryItem(internals,
				"DomesticChargeableFactorRail", "Freight/Chargeable/Domestic Chargeable",
				"Chargeable Factor for Rail", VolumeChargeableHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Rail, ConversionFactor.Standard.Imperial.Sea));
			}
		}

		#endregion

		#region International Chargeable Factor

		public void TestInternationalChargeableFactorAir()
		{
			BusinessObject usCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject usBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			usCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Core.Constants.CountryCodes.UnitedStates;
			usBranch[GlbBranchSchema.Constants.GB_GC] = usCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, usBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				IRegistryItemInternals internals = ItemSet.InternationalChargeableFactorAir;

				TestGenericRegistryItem(internals,
				"InternationalChargeableFactorAir", "Freight/Chargeable/International Chargeable",
				"Chargeable Factor for Air", WeightChargeableHintForInternationalAir,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Air));
			}
		}

		public void TestInternationalChargeableFactorRoad()
		{
			BusinessObject australianCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject australianBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Core.Constants.CountryCodes.Australia;
			australianBranch[GlbBranchSchema.Constants.GB_GC] = australianCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, australianBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				IRegistryItemInternals internals = ItemSet.InternationalChargeableFactorRoad;

				TestGenericRegistryItem(internals,
				"InternationalChargeableFactorRoad", "Freight/Chargeable/International Chargeable",
				"Chargeable Factor for Road", WeightChargeableHintForRoadTransport,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Road, ConversionFactor.Standard.Imperial.Domestic));
			}
		}

		public void TestInternationalChargeableFactorCourier()
		{
			BusinessObject usCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject usBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			usCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Core.Constants.CountryCodes.UnitedStates;
			usBranch[GlbBranchSchema.Constants.GB_GC] = usCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, usBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				IRegistryItemInternals internals = ItemSet.InternationalChargeableFactorCourier;

				TestGenericRegistryItem(internals,
				"InternationalChargeableFactorCourier", "Freight/Chargeable/International Chargeable",
				"Chargeable Factor for Courier", WeightChargeableHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic));
			}
		}

		public void TestInternationalChargeableFactorSea()
		{
			BusinessObject australianCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject australianBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Core.Constants.CountryCodes.Australia;
			australianBranch[GlbBranchSchema.Constants.GB_GC] = australianCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, australianBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				IRegistryItemInternals internals = ItemSet.InternationalChargeableFactorSea;

				TestGenericRegistryItem(internals,
				"InternationalChargeableFactorSea", "Freight/Chargeable/International Chargeable",
				"Chargeable Factor for Sea", VolumeChargeableHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Sea, ConversionFactor.Standard.Imperial.Sea));
			}
		}

		public void TestInternationalChargeableFactorRail()
		{
			BusinessObject australianCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject australianBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Core.Constants.CountryCodes.Australia;
			australianBranch[GlbBranchSchema.Constants.GB_GC] = australianCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, australianBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				IRegistryItemInternals internals = ItemSet.InternationalChargeableFactorRail;

				TestGenericRegistryItem(internals,
				"InternationalChargeableFactorRail", "Freight/Chargeable/International Chargeable",
				"Chargeable Factor for Rail", VolumeChargeableHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Rail, ConversionFactor.Standard.Imperial.Rail));
			}
		}

		#endregion

		#endregion

		#region Loading Meters

		public void TestEnableRoadLoadingMeters()
		{
			TestRegistryItem(ItemSet.EnableRoadLoadingMeters,
				"EnableRoadLoadingMeters",
				FreightDataRegistry.Categories.Freight_Chargeable_LoadingMeters,
				"Enable Road Loading Meters",
				"Override default to Yes to expose Loading Meters on Shipments & Consols.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestRoadLoadingMetersWeightPerLDM()
		{
			TestRegistryItem(ItemSet.RoadLoadingMetersWeightPerLDM,
				"RoadLoadingMetersWeightPerLDM",
				FreightDataRegistry.Categories.Freight_Chargeable_LoadingMeters,
				"Weight Per Loading Meter",
				"Equivalent weight in kilograms of Loading Meters will be calculated using this factor.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				1750m);
		}

		#endregion

		#region UAEDeliveryOrderNumberPrefix

		public void TestUAEDeliveryOrderNumberPrefix()
		{
			TestRegistryItem(ItemSet.UAEDeliveryOrderNumberPrefix,
				"UAEDeliveryOrderNumberPrefix",
				FreightDataRegistry.Categories.Freight_Shipment_UnitedArabEmirates,
				"UAE Delivery Order Number Prefix",
				"This specifies the prefix used on a import sea shipment to generate delivery order number.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsHidden, TextEditorType.TextBox,
				"A0");

			BusinessObject glbCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject glbBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));

			glbCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Constants.CountryCodes.UnitedArabEmirates;
			glbBranch[GlbBranchSchema.Constants.GB_GC] = glbCompany.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("UAEDeliveryOrderNumberPrefix");

				TestRegistryItem(ItemSet.UAEDeliveryOrderNumberPrefix,
					"UAEDeliveryOrderNumberPrefix",
					FreightDataRegistry.Categories.Freight_Shipment_UnitedArabEmirates,
					"UAE Delivery Order Number Prefix",
					"This specifies the prefix used on a import sea shipment to generate delivery order number.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default, TextEditorType.TextBox,
					"A0");
			}
		}

		#endregion

		#region CanadaCargoControlNumbers

		public void TestCanadaCargoControlNumberBasedOnHousebill()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(Constants.ShipmentCCNCustomizationTypes.Code.HouseBill, Constants.ShipmentCCNCustomizationTypes.Description.HouseBill);
			list.AddPair(Constants.ShipmentCCNCustomizationTypes.Code.ShipmentNumber, Constants.ShipmentCCNCustomizationTypes.Description.ShipmentNumber);
			list.AddPair(Constants.ShipmentCCNCustomizationTypes.Code.NumberFountain, Constants.ShipmentCCNCustomizationTypes.Description.NumberFountain);

			TestRegistryItem(ItemSet.CanadaCargoControlNumberCustomization,
					"CanadaCargoControlNumberCustomization",
					FreightDataRegistry.Categories.Freight_Shipment_Canada,
					"Cargo Control Number Customization",
					"Use the last x digits of the shipment HAWB/HBL or shipment number to be the last x digits of the cargo control number, where x is specified in the registry item 'Cargo Control Number House Bill/Shipment Digits'.  If not, the last 8 digits of the CCN will be generated from system number fountain.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					list,
					Constants.ShipmentCCNCustomizationTypes.Code.HouseBill);
		}

		public void TestCanadaCargoControlNumberHBLDigits()
		{
			((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("CanadaCargoControlNumberHBLDigits");
			TestGenericRegistryItem(ItemSet.CanadaCargoControlNumberHBLDigits,
					"CanadaCargoControlNumberHBLDigits",
					FreightDataRegistry.Categories.Freight_Shipment_Canada,
					"Cargo Control Number House Bill/Shipment Digits",
					"The number of digits of the shipment HAWB/HBL or Shipment number to be used in generating the Cargo Control Number. A value of zero will ensure the entire HAWB/HBL is included.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					8);
		}

		public void TestCanadaCargoControlNumberBranchPrefix()
		{
			TestGenericRegistryItem(ItemSet.CanadaCargoControlNumberBranchPrefix,
					"CanadaCargoControlNumberBranchPrefix",
					FreightDataRegistry.Categories.Freight_Shipment_Canada,
					"Cargo Control Number Branch Prefix",
					"If cargo control numbers are generated from system number fountain, this specifies the branch prefix used on generating new numbers.\r\nPrefix must contain two digits, i.e. from 10 to 99.",
					RegistryStorageFlags.System | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					0);
		}

		#endregion

		#region Organisations

		public void TestOrganisationsDefaultCartageCompany()
		{
			TestRegistryItem(ItemSet.LCLCartageCompany,
				"LCLCartageCompany",
				FreightDataRegistry.Categories.Freight_Organizations_DefaultPortTransportCompany,
				"LCL Port Transport Company",
				"",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryFindBoxCollection.ShippingProvider,
				Guid.Empty);

			TestRegistryItem(ItemSet.FCLCartageCompany,
				"FCLCartageCompany",
				FreightDataRegistry.Categories.Freight_Organizations_DefaultPortTransportCompany,
				"FCL Port Transport Company",
				"",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryFindBoxCollection.ShippingProvider,
				Guid.Empty);

			TestRegistryItem(ItemSet.AIRCartageCompany,
				"AIRCartageCompany",
				FreightDataRegistry.Categories.Freight_Organizations_DefaultPortTransportCompany,
				"AIR Port Transport Company",
				"",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryFindBoxCollection.ShippingProvider,
				Guid.Empty);
		}

		public void TestOrganisationsDefaultFumigationContractor()
		{
			TestRegistryItem(ItemSet.LCLFumigationContractor,
				"LCLFumigationContractor",
				FreightDataRegistry.Categories.Freight_Organizations_DefaultFumigationContractor,
				"LCL Fumigation Contractor",
				"",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryFindBoxCollection.FumigationContractors,
				Guid.Empty);

			TestRegistryItem(ItemSet.FCLFumigationContractor,
				"FCLFumigationContractor",
				FreightDataRegistry.Categories.Freight_Organizations_DefaultFumigationContractor,
				"FCL Fumigation Contractor",
				"",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryFindBoxCollection.FumigationContractors,
				Guid.Empty);

			TestRegistryItem(ItemSet.AIRFumigationContractor,
				"AIRFumigationContractor",
				FreightDataRegistry.Categories.Freight_Organizations_DefaultFumigationContractor,
				"AIR Fumigation Contractor",
				"",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryFindBoxCollection.FumigationContractors,
				Guid.Empty);
		}

		#endregion

		public void TestSuppressValidationOfConsolsSendingOrReceivingAgents()
		{
			TestRegistryItem(ItemSet.SuppressValidationOfConsolsSendingOrReceivingAgents,
				"SuppressValidationOfConsolsSendingOrReceivingAgents",
				FreightDataRegistry.Categories.Freight_Consolidations,
				"Validation Of Sending/Receiving Agents",
				"Override this value if you would like to suppress validations of Sending Agent and Receiving Agent organizations against their Freight Handling settings. Recommended practice is to use the default registry value and validate Sending and Receiving Agents against their published, handling or appointed agent details, transport modes and locations.",
				RegistryStorageFlags.Company,
				false);
		}

		#region CanadaCargoControlNumbers

		public void TestCanadaConsolCargoControlNumberCustomization()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair(Constants.ConsolidationCCNCustomizationTypes.Code.Non, Constants.ConsolidationCCNCustomizationTypes.Description.Non);
			list.AddPair(Constants.ConsolidationCCNCustomizationTypes.Code.MasterBill, Constants.ConsolidationCCNCustomizationTypes.Description.MasterBill);
			BusinessObject glbCompany = base.Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject glbBranch = base.Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			glbCompany["GC_RN_NKCountryCode"] = Core.Constants.CountryCodes.Canada;
			glbBranch["GB_GC"] = glbCompany.PK;
			base.Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("CanadaConsolCargoControlNumberCustomization");
				base.TestRegistryItem(base.ItemSet.CanadaConsolCargoControlNumberCustomization,
					"CanadaConsolCargoControlNumberCustomization",
					FreightDataRegistry.Categories.Freight_Consolidations_Canada,
					"Cargo Control Number Customization",
					"If MBL is selected then the CCN on AIR consols will be defaulted to the MAWB and other transport modes will be the carrier code from the carrier organization + the master bill number.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					list,
					Constants.ConsolidationCCNCustomizationTypes.Code.Non);
			}
			glbCompany["GC_RN_NKCountryCode"] = Core.Constants.CountryCodes.UnitedStates;
			glbBranch["GB_GC"] = glbCompany.PK;
			base.Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("CanadaConsolCargoControlNumberCustomization");
				base.TestRegistryItem(base.ItemSet.CanadaConsolCargoControlNumberCustomization,
					"CanadaConsolCargoControlNumberCustomization",
					FreightDataRegistry.Categories.Freight_Consolidations_Canada,
					"Cargo Control Number Customization",
					"If MBL is selected then the CCN on AIR consols will be defaulted to the MAWB and other transport modes will be the carrier code from the carrier organization + the master bill number.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					list,
					Constants.ConsolidationCCNCustomizationTypes.Code.Non);
			}
		}

		public void TestCanadaConsolPreviousCargoControlNumberCustomization()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair(Constants.ConsolidationCCNCustomizationTypes.Code.Non, Constants.ConsolidationCCNCustomizationTypes.Description.Non);
			list.AddPair(Constants.ConsolidationCCNCustomizationTypes.Code.CarrierCode, Constants.ConsolidationCCNCustomizationTypes.Description.CarrierCode);
			list.AddPair(Constants.ConsolidationCCNCustomizationTypes.Code.CCN, Constants.ConsolidationCCNCustomizationTypes.Description.CCN);
			BusinessObject glbCompany = base.Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject glbBranch = base.Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			glbCompany["GC_RN_NKCountryCode"] = Core.Constants.CountryCodes.Canada;
			glbBranch["GB_GC"] = glbCompany.PK;
			base.Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("CanadaConsolPreviousCargoControlNumberCustomization");
				base.TestRegistryItem(base.ItemSet.CanadaConsolPreviousCargoControlNumberCustomization,
					"CanadaConsolPreviousCargoControlNumberCustomization",
					FreightDataRegistry.Categories.Freight_Consolidations_Canada,
					"Previous Cargo Control Number Customization",
					"The PCN may default to the carrier code to then be completed, or the CCN.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					list,
					Constants.ConsolidationCCNCustomizationTypes.Code.CarrierCode);
			}
			glbCompany["GC_RN_NKCountryCode"] = Core.Constants.CountryCodes.UnitedStates;
			glbBranch["GB_GC"] = glbCompany.PK;
			base.Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("CanadaConsolPreviousCargoControlNumberCustomization");
				base.TestRegistryItem(base.ItemSet.CanadaConsolPreviousCargoControlNumberCustomization,
					"CanadaConsolPreviousCargoControlNumberCustomization",
					FreightDataRegistry.Categories.Freight_Consolidations_Canada,
					"Previous Cargo Control Number Customization",
					"The PCN may default to the carrier code to then be completed, or the CCN.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					list,
					Constants.ConsolidationCCNCustomizationTypes.Code.CarrierCode);
			}
		}

		#endregion

		public void TestEnableEnhancedSECEvent()
		{
			TestGenericRegistryItem(ItemSet.EnableEnhancedSECEvent,
				"EnableEnhancedSECEvent",
				FreightDataRegistry.Categories.Freight_Shipment,
				"Enable Enhanced SEC Events",
				"Import XML changes should only apply when this registry is enabled. Workflow(TWH) should use the registry. Fallback to existing behavior if registry is disabled. Workflow(GUI) should remove the registry and the fallback from Workflow(UXML) and Workflow(TWH).",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableVisibilityEventDelivery()
		{
			TestGenericRegistryItem(ItemSet.EnableVisibilityEventDelivery,
				"EnableVisibilityEventDelivery",
				FreightDataRegistry.Categories.Freight_Shipment,
				"Enable Visibility Event Delivery",
				"Enable Visibility Event Delivery if set to true",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestCreateBrokerageJobAutomatically()
		{
			var internals = ItemSet.CreateBrokerageJobAutomatically;

			TestGenericRegistryItem(internals,
				"CreateBrokerageJobAutomatically",
				FreightDataRegistry.Categories.Freight_Shipment,
				FreightDataRegistry.CreateBrokerageJobAutomaticallyCaption,
				FreightDataRegistry.CreateBrokerageJobAutomaticallyCaptionHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestCustomsPermitClearanceNumbers()
		{
			IRegistryItemInternals internals = ItemSet.CustomsPermitClearanceNumbers;

			TestGenericRegistryItem(internals,
			"CustomsPermitClearanceNumbers",
			"Freight/Customs Numbers",
			"Customs Permit/Clearance Number Types",
			"This list defines the customs permit/clearance types available on the Shipment screen for the currently chosen country/region. Note that this only applies to countries/regions which do not have a defined list of permit/clearance numbers.",
			RegistryStorageFlags.Company);

			AssertContainsExactElementsInAnyOrder(new[] { "TSN|Transhipment Number", "ATA|ATA Carnet Number" },
				((IList)internals.DefaultValue)
				.Cast<ICodeDescription>()
				.Select((number) => string.Format("{0}|{1}", number.Code, number.Description))
				.ToArray());
		}

		public void TestCustomsAdditionalReferenceNumbers()
		{
			RatingDataRegistry.Instance.EnableSpotRatingBehaviourFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			IRegistryItemInternals internals = ItemSet.CustomsAdditionalReferenceNumbers;

			TestGenericRegistryItem(internals,
			"CustomsAdditionalReferenceNumbers",
			"Freight/Customs Numbers",
			"Customs Additional Reference Number Types",
			$"This list defines the additional reference number types available under the Consol > Details > Numbers tab, Containers > Numbers tab, Shipment > Additional Detail > Reference Numbers of the Shipment. These reference types may also be used across modules in {Constants.ProductName}, such as Liner & Agency > Bill of Lading > Containers. These types are in addition to any number types already in the system unless a country specific reference number type already uses the entered type code. The flag \"Edit via UXML only\" restricts the ability to add, edit or delete the corresponding Additional Reference Number via UXML only.",
			RegistryStorageFlags.System);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"COC|Customs Office Code (Override)|Y",
					"AMS|AMS Number|Y",
					"UBR|Under Bond Approval Reference Number|Y",
					"CON|Carrier Contract Number|Y",
					"CLC|Client Contract Number|Y",
					"BKG|Carrier Booking Reference|N",
					"RLB|Railway Bill Number|N",
					"UCR|External (3rd party) Unique Consignment Reference|N",
					"CAR|Customs Authorization Reference|N",
					"ACA|Pentant Advance Cargo Advice Reference|N",
					"LCR|Letter Of Credit Number|Y",
					"CCN|Cargo Control Number|N",
					"PCN|Previous Cargo Control Number|N",
					"NAC|Contract Named Account|Y",
					"BAG|Courier Bag Reference|Y",
					"COU|Courier Consignment Reference|Y",
					"OAG|Other Agent Reference|Y",
					"TWR|Transit Warehouse Receive|Y",
					"HIR|eHub Interchange Reference|Y",
					"CQN|Carrier Quote Number|N",
					"SPO|Spot Rate|N",
					"ACI|Advance Cargo Information Reference|N",
					"ISF|US Import Security Filing (ISF) Reference|Y",
					"JDR|Declaration Reference|Y",
					"CLR|Customer Load Reference|N",
					"CMR|Carrier Message Reference|Y",
					"EOE|Exporter EORI Number|Y",
					"EOI|Importer EORI Number|Y",
					"CTK|Cargo Tracking Note|Y"
				},
				((CustomsReferenceNumberTypeCollection)internals.DefaultValue)
				.Cast<CustomsReferenceNumberType>()
				.Select((number) => string.Format("{0}|{1}|{2}", number.Code, number.Description, number.IsUnique))
				.ToArray());
		}

		public void TestNotClearedByAgentStatement()
		{
			MultilingualStringRegistryItem item = ItemSet.NotClearedByAgentStatement;
			AssertEquals("NotClearedByAgentStatement", item.Name);
			AssertEquals(FreightDataRegistry.Categories.Freight_Shipment, item.Category);
			AssertEquals("Not Cleared by Agent Statement", item.Caption);
			AssertEquals("This statement can be printed on any shipment related document by placing the macro <EvaluateInnerContent(<Shipment.NotClearedByAgentStatement>)> on your document. The text will only print if a 'Not Cleared By Agent' Customs Entry type is chosen on the shipment.", item.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.Default, item.Options);
			AssertType(typeof(TextRegistryEditorInfo), item.EditorInfo);
			AssertEquals(TextEditorType.Memo, ((TextRegistryEditorInfo)item.EditorInfo).EditorType);
			AssertEquals("", item.DefaultValue);

			BusinessObject glbCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject glbBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));

			glbCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Constants.CountryCodes.Germany;
			glbBranch[GlbBranchSchema.Constants.GB_GC] = glbCompany.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("NotClearedByAgentStatement");
				item = ItemSet.NotClearedByAgentStatement;
				AssertEquals(@"! ! ! ! ! A C H T U N G  Z O L L G U T ! ! ! ! !
UNVERZOLLTE WARE, DIE SOFORT INNERHALB DER GESTELLUNGSFRIST ZUSAMMEN
MIT DEM BEIGEFUEGTEN ZOLLVERSANDSCHEIN ( T1 )
!!                                                                  !!
MRN NR.: <NotClearedByAgentNumber> VOM: <NotClearedByAgentIssueDate.Date>  GESTELLUNGSFRIST: <NotClearedByAgentExpiryDate.Date>
!!                                                                  !!
DEM FUER DEN EMPFANGSORT ZUSTAENDIGEN ZOLLAMT VORZUFUEHREN IST.
ZUR ORDNUNGSGEMAESSEN WIEDERGESTELLUNG VERPFLICHTEN WIR/ICH UNS.
ICH/WIR SIND UEBER DIE PFLICHTEN FUER EINE ZOLLGUTBEFOERDERUNG BELEHRT
WORDEN. ZOLLPLOMBEN UND -SCHNUERE DUERFEN NICHT BESCHAEDIGT WERDEN.", item.DefaultValue);
			}
		}

		public void TestDefaultShipmentControllingCustomer()
		{
			TestRegistryItem(ItemSet.DefaultShipmentControllingCustomer, "DefaultShipmentControllingCustomer", "Freight/Shipment", "Default Shipment Controlling Customer", "Enable this option to default the Shipment Controlling Customer when saving a New shipment.", RegistryStorageFlags.System, false);
			AssertContainsExactElementsInAnyOrder(new string[] { "Freight/Shipment", "Customs" }, ItemSet.DefaultShipmentControllingCustomer.Categories);
		}

		public void TestDefaultControllingCustomerRuleRegistryItem()
		{
			var hint = ResString.GetMultilingualString("f2455b00-edb9-4076-bd89-2c96313572ae", @"This registry specifies an order of precedence for calculation of which shipment or booking organization the Controlling Customer should be defaulted from.

Use override to reprioritize these parties if required.

Notes:

Freight Bill To Party – is  a local client organization responsible for paying freight as per Incoterms (i.e. local client at origin for freight prepaid or destination for freight collect), and this is not a logistics provider such as a broker, freight forwarder etc (in which case the system will check next party in the priority list).

Booking Party - is the Client organization on the quick booking/booking with quote.

Consignor/Shipper for prepaid freight – if a shipment is freight prepaid, the system will first try to find a Controlling Customer for IFT (Invoice Freight Jobs To) party of the Shipper, and if blank – Controlling Customer of the Shipper organization.  Consignee for collect freight – will first try to find Controlling Customer for IFT party of Consignee with the fallback to Consignee organization.");

			TestGenericRegistryItem(ItemSet.DefaultControllingCustomerRule,
				"DefaultControllingCustomerPrecedence",
				"Freight/Shipment",
				"Controlling Customer – Precedence Defaulting Logic",
				hint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
			AssertContainsExactElementsInAnyOrder(new string[] { "Freight/Shipment", "Customs" }, ItemSet.DefaultControllingCustomerRule.Categories);
		}

		#region Vessels

		public void TestVesselInReferenceFileMandatoryOnShipments()
		{
			TestRegistryItem(ItemSet.VesselInReferenceFileMandatoryOnShipments,
				"VesselInReferenceFileMandatoryOnShipments",
				FreightDataRegistry.Categories.Freight + "/Vessel",
				"Reference File attached to Vessel Mandatory",
				@"Set to ""Yes"" to change the warning to an error when users either input a Vessel that is not attached to a Reference File or an Inactive Vessel on Operational Jobs.",
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		#region ALPO Integration

		public void TestALPOExportClientNo()
		{
			TestRegistryItem(ItemSet.ALPOExportClientNo,
				"ALPOExportClientNo",
				FreightDataRegistry.Categories.Freight + "/ALPO/Export",
				"Client Identification in ALPO (ZKV)",
				"Specify the Client Identification for your organization. This will be the ZKV in the ALPO Application.",
				RegistryStorageFlags.Company,
				TextEditorType.TextBox, "");
		}

		public void TestALPOExportFileName()
		{
			TestRegistryItem(ItemSet.ALPOExportFileName,
				"ALPOExportFileName",
				FreightDataRegistry.Categories.Freight + "/ALPO/Export",
				"Default Filename for ALPO Export (ZKV)",
				"Specify the Default Filename for your organization without extension. A time stamp will automatically be added when the export is performed.",
				RegistryStorageFlags.Company,
				TextEditorType.TextBox, "");
		}

		#endregion

		public void TestUSAMSGroupNotification()
		{
			TestGroupNotificationRegistryItem(ItemSet.USAMSGroupNotification,
				"USAMSGroupNotification",
				FreightDataRegistry.Categories.Freight_AMS,
				"Notification Group",
				"Group to receive AMS Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email.",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ManifestGroupNotification.Default);
		}

		public void TestUSHVLVAMSGroupNotification()
		{
			TestGroupNotificationRegistryItem(ItemSet.USHVLVAMSGroupNotification,
				"USHVLVAMSGroupNotification",
				FreightDataRegistry.Categories.Freight_AMS,
				"HVLV Notifications",
				"Group to Receive AMS messages sent by Customs for Bills that originate from HVL shipments. If 'Send Error Only' ticked, Only when an error occurs then send the notification email.",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				new ManifestGroupNotification(Constants.EmailTo.StaffMember, Guid.Empty, true));
		}

		void TestGroupNotificationRegistryItem(IRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint,
			RegistryStorageFlags expectedStorage, RegistryOptions options, GroupNotification expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, options);
			AssertEquals(((GroupNotification)item.DefaultValue).SendMode, expectedDefaultValue.SendMode);
			AssertEquals(((GroupNotification)item.DefaultValue).SendGroupPK, expectedDefaultValue.SendGroupPK);
		}

		public void TestUseFIRMSCodeAsFilerForPTTMessages()
		{
			TestRegistryItem(ItemSet.UseFIRMSCodeAsFilerForPTTMessages,
				"UseFIRMSCodeAsFilerForPTTMessages",
				"Freight/AMS",
				"Use FIRMS code as Filer for PTT Messages (NOVCC)",
				"If 'Yes', Stand Alone PTT(NVOCC) messages will use the FIRMS code on the Branch or Company Organization Proxy as the AMS Filer, in lieu of a SCAC code.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestEstimatedExportCustomsClearanceDateMandatory()
		{
			TestRegistryItem(ItemSet.EstimatedExportCustomsClearanceDateMandatory,
				"EstimatedExportCustomsClearanceDateMandatory",
				"Freight/Shipment",
				"Estimated Export Customs Clearance Date Mandatory",
				"Specify whether the shipment Estimated Export Customs Clearance Date should be a mandatory field. This setting only applies to Export Shipments.",
				RegistryStorageFlags.BranchDepartment,
				false);
		}

		public void TestShipmentsPerConsolLimit()
		{
			TestRegistryItem(ItemSet.ShipmentsPerConsolLimit,
				"ShipmentsPerConsolLimit",
				"Freight",
				"Maximum number of Shipments per Consol",
				"This registry determines the maximum number of Shipments that may be attached to a Consol.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				1000);
		}

		public void TestShipmentsPerConsolLimitIntroductionTimeUTC()
		{
			TestGenericRegistryItem(ItemSet.ShipmentsPerConsolLimitIntroductionTimeUTC,
				"ShipmentsPerConsolLimitIntroductionTimeUTC",
				"Freight",
				"Maximum number of Shipments per Consol Active Time",
				"Time (in UTC) of introduction of the 'Maximum number of Shipments per Consol' registry.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden);
		}

		public void TestOrdersPerShipmentLimit()
		{
			TestRegistryItem(ItemSet.OrdersPerShipmentLimit,
				"OrdersPerShipmentLimit",
				"Freight",
				"Maximum number of Orders per Shipment",
				"This registry determines the maximum number of Orders that may be attached to a Shipment.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				100);
		}

		public void TestOrdersPerShipmentLimitIntroductionTimeUTC()
		{
			TestGenericRegistryItem(ItemSet.OrdersPerShipmentLimitIntroductionTimeUTC,
				"OrdersPerShipmentLimitIntroductionTimeUTC",
				"Freight",
				"Maximum number of Orders per Shipment Active Time",
				"Time (in UTC) of introduction of the 'Maximum number of Orders per Shipment' registry.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden);
		}

		public void TestDefaultControllingAgentRuleRegistryItem()
		{
			ResourceString hint = ResString.GetMultilingualString("17380c1c-f486-4901-8cc6-039bd74fab3c", @"This registry specifies an order of precedence for calculation of which shipment or booking organization the Controlling Agent should be defaulted from.

Use override to re-prioritize these parties if required.

Notes:
Freight Bill To Party - is a local client organization responsible for paying freight as per Incoterms (i.e. local client at origin for freight prepaid or destination for freight collect), and this is not a logistics provider such as a broker, freight forwarder etc (in which case the system will check next party in the priority list).

Booking Party - is the Client organization on the quick booking/booking with quote.

Shipper for prepaid freight – if a shipment is freight prepaid, the system will first try to find a Controlling Agent for IFT (Invoice Freight Jobs To) party of the Shipper, and if blank – Controlling Agent of the Shipper organization.  Consignee for collect freight – will first try to find Controlling Agent for IFT party of Consignee with the fallback to Consignee organization.");

			TestGenericRegistryItem(ItemSet.DefaultControllingAgentRule,
				"DefaultControllingAgentPrecedence",
				"Freight/Shipment",
				"Controlling Agent – Precedence Defaulting Logic",
				hint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
			AssertContainsExactElementsInAnyOrder(new string[] { "Freight/Shipment", "Customs" }, ItemSet.DefaultControllingAgentRule.Categories);
		}

		#region TestMandatoryControllingCustomerEffectiveDateSeaRegistryItem

		readonly ResourceString controllingCustomerHint = ResString.GetMultilingualString("98466294-95D0-4FCB-869A-02D0EADEC18E", "The date specified in this registry will trigger validation of user or group security rights set to prevent \"Allow Save Without Controlling Customer\" on shipments and bookings created on or after this date.\r\n\r\nOverride this registry to specify the date in UTC format.");

		public void TestMandatoryControllingCustomerEffectiveDateRegistryItem_AllModes()
		{
			TestGenericRegistryItem(ItemSet.MandatoryControllingCustomerEffectiveDateAll,
				"MandatoryControllingCustomerEffectiveDateAll",
				"Freight/Shipment/Effective Date for Mandatory Controlling Customer",
				"All Transport Modes",
				controllingCustomerHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestMandatoryControllingCustomerEffectiveDateSeaRegistryItem_Sea()
		{
			TestGenericRegistryItem(ItemSet.MandatoryControllingCustomerEffectiveDateSea,
				"MandatoryControllingCustomerEffectiveDateSea",
				"Freight/Shipment/Effective Date for Mandatory Controlling Customer",
				"For Sea",
				controllingCustomerHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestMandatoryControllingCustomerEffectiveDateRegistryItem_Air()
		{
			TestGenericRegistryItem(ItemSet.MandatoryControllingCustomerEffectiveDateAir,
				"MandatoryControllingCustomerEffectiveDateAir",
				"Freight/Shipment/Effective Date for Mandatory Controlling Customer",
				"For Air",
				controllingCustomerHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestMandatoryControllingCustomerEffectiveDateRegistryItem_Road()
		{
			TestGenericRegistryItem(ItemSet.MandatoryControllingCustomerEffectiveDateRoad,
				"MandatoryControllingCustomerEffectiveDateRoad",
				"Freight/Shipment/Effective Date for Mandatory Controlling Customer",
				"For Road",
				controllingCustomerHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestMandatoryControllingCustomerEffectiveDateRegistryItem_Rail()
		{
			TestGenericRegistryItem(ItemSet.MandatoryControllingCustomerEffectiveDateRail,
				"MandatoryControllingCustomerEffectiveDateRail",
				"Freight/Shipment/Effective Date for Mandatory Controlling Customer",
				"For Rail",
				controllingCustomerHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		#endregion

		#region TestMandatoryControllingAgentEffectiveDateSeaRegistryItem

		readonly ResourceString controllingAgentHint = ResString.GetMultilingualString("A0F07BB1-46ED-424F-8188-DD60ABEEC6A7", "The date specified in this registry will trigger validation of user or group security rights set to prevent \"Allow Save Without Controlling Agent\" on shipments and bookings created on or after this date.\r\n\r\nOverride this registry to specify the date in UTC format.");

		public void TestMandatoryControllingAgentEffectiveDateRegistryItem_AllModes()
		{
			TestGenericRegistryItem(ItemSet.MandatoryControllingAgentEffectiveDateAll,
				"MandatoryControllingAgentEffectiveDateAll",
				"Freight/Shipment/Effective Date for Mandatory Controlling Agent",
				"All Transport Modes",
				controllingAgentHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestMandatoryControllingAgentEffectiveDateRegistryItem_Sea()
		{
			TestGenericRegistryItem(ItemSet.MandatoryControllingAgentEffectiveDateSea,
				"MandatoryControllingAgentEffectiveDateSea",
				"Freight/Shipment/Effective Date for Mandatory Controlling Agent",
				"For Sea",
				controllingAgentHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestMandatoryControllingAgentEffectiveDateRegistryItem_Air()
		{
			TestGenericRegistryItem(ItemSet.MandatoryControllingAgentEffectiveDateAir,
				"MandatoryControllingAgentEffectiveDateAir",
				"Freight/Shipment/Effective Date for Mandatory Controlling Agent",
				"For Air",
				controllingAgentHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestMandatoryControllingAgentEffectiveDateRegistryItem_Road()
		{
			TestGenericRegistryItem(ItemSet.MandatoryControllingAgentEffectiveDateRoad,
				"MandatoryControllingAgentEffectiveDateRoad",
				"Freight/Shipment/Effective Date for Mandatory Controlling Agent",
				"For Road",
				controllingAgentHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestMandatoryControllingAgentEffectiveDateRegistryItem_Rail()
		{
			TestGenericRegistryItem(ItemSet.MandatoryControllingAgentEffectiveDateRail,
				"MandatoryControllingAgentEffectiveDateRail",
				"Freight/Shipment/Effective Date for Mandatory Controlling Agent",
				"For Rail",
				controllingAgentHint,
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		#endregion

		public void TestDefaultShipmentControllingAgent()
		{
			TestRegistryItem(ItemSet.DefaultShipmentControllingAgent, "DefaultShipmentControllingAgent", "Freight/Shipment", "Default Shipment Controlling Agent", "Enable this option to default the Shipment Controlling Agent when saving a New shipment.", RegistryStorageFlags.System, false);
			AssertContainsExactElementsInAnyOrder(new string[] { "Freight/Shipment", "Customs" }, ItemSet.DefaultShipmentControllingAgent.Categories);
		}

		#region TestGetChargeableFactorVolumeToWeightHint

		public void TestGetChargeableFactorVolumeToWeightHint()
		{
			var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic);
			var description = FreightDataRegistry.GetChargeableFactorVolumeToWeightHint(defaultFactor);
			AssertEquals(
				@"The chargeable factor will be used to convert the actual volume into a chargeable weight.

The chargeable weight is the greater of the actual weight or actual volume divided by the chargeable factor.

Max of [ weight(KG) , volume(CM3) / factor ] = KG

Default chargeable factor is 6000 CC/KG.

Max of [ weight(LB) , volume(CI) / factor ] = LB

Default chargeable factor is 194 CI/LB.", description);
		}

		#endregion

		#region Global Tracking

		public void TestContainerAutomation()
		{
			TestRegistryItem(ItemSet.ContainerAutomation,
				"ContainerAutomation",
				"Freight/Global Tracking",
				"Container Automation",
				"Enable Container Automation?",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestContainerAutomationEHubID()
		{
			TestRegistryItem(ItemSet.ContainerAutomationEHubID,
				"ContainerAutomationEHubID",
				"Freight/Global Tracking",
				"Container Automation eHub ID",
				"Container Automation eHub ID",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport,
				"CONTAINER_TRACKING");
		}

		public void TestAWBTracking()
		{
			TestRegistryItem(
				ItemSet.AWBTracking,
				"AWBTracking",
				"Freight/Global Tracking",
				"AWB Tracking",
				"Enable AWB Tracking?",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestAWBTrackingEHubID()
		{
			TestRegistryItem(
				ItemSet.AWBTrackingEHubID,
				"AWBTrackingEHubID",
				"Freight/Global Tracking",
				"AWB Tracking eHub ID",
				"AWB Tracking eHub ID",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport,
				"FLIGHT_MONITORING_SYSTEM");
		}

		public void TestGlobalTrackingShipmentVisibility()
		{
			TestGenericRegistryItem(
				ItemSet.GlobalTrackingShipmentVisibility,
				"GlobalTrackingShipmentVisibility",
				FreightDataRegistry.Categories.Freight_GlobalTracking,
				"Shipment Visibility",
				"Enable Shipment Visibility?",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				new GlobalTrackingShipmentVisibilityOptions { IsActive = false, IsDefaultContainerAutomation = false });
		}

		public void TestGlobalTrackingShipmentVisibilityInputValue()
		{
			AssertEquals("GlobalTrackingShipmentVisibilityOptionsRegistryItemDataType.DefaultValue",
				new GlobalTrackingShipmentVisibilityOptions { IsActive = false, IsDefaultContainerAutomation = false }, ItemSet.GlobalTrackingShipmentVisibility.DefaultValue);
		}

		public void TestAutomaticContainerCreation()
		{
			AssertEquals("Name", "DisableAutomaticContainerCreation", ItemSet.AutomaticContainerCreation.Name);
			AssertEquals("Category", "Freight/Global Tracking", ItemSet.AutomaticContainerCreation.Category);
			AssertEquals("Caption", "Automatic Container Creation", ItemSet.AutomaticContainerCreation.Caption);
			AssertEquals("Hint", @"This registry setting controls the behavior of the creation of Containers on a Consolidation via Container Automation.

Always Create: This option allows Container Automation events to automatically populate container numbers onto Consolidation when the messages received detect a new Container number which does not already exist on the Consolidation.
Note - A CID Event (Change of Identifier) will be created when a container number is populated.

Create up to ATD or Shipping Instruction: This is the default option for this functionality. This option allows Container Automation events to automatically populate container numbers onto Consolidation, but only up to the point that the Consolidation either receives an ATD, or the Shipping Instruction message is submitted electronically. After that point, Containers will no longer auto-populate on the Consolidation.

Never Create: This option will disable any automatic Container creation from messages received from Container Automation.", ItemSet.AutomaticContainerCreation.Hint);
			AssertEquals("Options", RegistryStorageFlags.System, ItemSet.AutomaticContainerCreation.Storage);
		}

		public void TestEnableLastForeignAndFirstArrivalPortMatcher()
		{
			TestRegistryItem(ItemSet.EnableLastForeignAndFirstArrivalPortMatcher,
				"EnableLastForeignAndFirstArrivalPortMatcher",
				"Freight/Global Tracking",
				"Enable Last Foreign and First Arrival Port Matcher",
				"When this registry is enabled, Last Foreign and First Arrival Ports will be automatically populated.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		#region Freight / Global Tracking / Global Sailing Schedules

		public void TestEnableScheduleFeedService()
		{
			TestRegistryItem(
				ItemSet.EnableScheduleFeedService,
				"EnableScheduleFeedService",
				"Freight/Global Tracking/Global Sailing Schedules",
				"Enable Global Sailing Schedules",
				"Enable Global Sailing Schedules?",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestOnlineSailingSchedulesUrl()
		{
			TestStringRegistryItem(
				ItemSet.OnlineSailingSchedulesUrl,
				"OnlineSailingSchedulesUrl",
				"Freight/Global Tracking/Global Sailing Schedules",
				"Global Sailing Schedules Service URL",
				"The URL used to connect to Global Sailing Schedules",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport,
				"https://gss.wisegrid.net",
				CharacterCase.Normal);
		}

		public void TestOnlineSailingSchedulesTokenOverride()
		{
			TestStringRegistryItem(
				ItemSet.OnlineSailingSchedulesTokenOverride,
				nameof(ItemSet.OnlineSailingSchedulesTokenOverride),
				"Freight/Global Tracking/Global Sailing Schedules",
				"GSS Token Override",
				"An override for the auth. token used to connect to Global Sailing Schedules",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport,
				"",
				CharacterCase.Normal);
		}

		public void TestScheduleFeedTrackingEHubID()
		{
			TestRegistryItem(
				ItemSet.ScheduleFeedTrackingEHubID,
				"ScheduleFeedTrackingEHubID",
				"Freight/Global Tracking/Global Sailing Schedules",
				"Schedule Feed Tracking eHub ID",
				"Schedule Feed Tracking eHub ID",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport,
				"SCHEDULE_FEED_SERVICE");
		}

		public void TestOnlineSailingSchedulesAutoInitiatedTimeoutInSeconds()
		{
			TestRegistryItem(
				ItemSet.OnlineSailingSchedulesAutoInitiatedTimeoutInSeconds,
				"OnlineSailingSchedulesAutoInitiatedTimeoutInSeconds",
				"Freight/Global Tracking/Global Sailing Schedules",
				"Auto Requests Timeout",
				"Timeout is seconds when automatic requests are executed to populate dates",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				5,
				0,
				60);
		}

		public void TestOnlineSailingSchedulesUserInitiatedTimeoutInSeconds()
		{
			TestRegistryItem(
				ItemSet.OnlineSailingSchedulesUserInitiatedTimeoutInSeconds,
				"OnlineSailingSchedulesUserInitiatedTimeoutInSeconds",
				"Freight/Global Tracking/Global Sailing Schedules",
				"User Initiated Requests Timeout",
				"Timeout is seconds when user initiated search requests are executed",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				20,
				0,
				60);
		}

		#endregion // Freight / Global Tracking / Global Sailing Schedules

		#region Route Visualizer
		public void TestEnableRouteVisualizer()
		{
			TestRegistryItem(
				ItemSet.EnableRouteVisualizer,
				"EnableRouteVisualizer",
				"Freight/Global Tracking/Route Visualizer",
				"Enable Route Visualizer",
				"When this registry is enabled, the Route Visualizer Map button will be shown on to all users on the Routing tab. Otherwise, the button is only visible to CW1 support user.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestRouteVisualizerUrl()
		{
			TestRegistryItem(
				ItemSet.RouteVisualizerUrl,
				"RouteVisualizerUrl",
				"Freight/Global Tracking/Route Visualizer",
				"Route Visualizer URL",
				"Determines the base server URL used for the Route Visualizer Map.",
				RegistryStorageFlags.System,
				TextEditorType.Url,
				RegistryOptions.IsOnlyForSupport,
				"https://vt.wisegrid.net/");
		}

		public void TestRouteVisualizerApiUrl()
		{
			TestRegistryItem(
				ItemSet.RouteVisualizerApiUrl,
				"RouteVisualizerApiUrl",
				"Freight/Global Tracking/Route Visualizer",
				"Route Visualizer API URL",
				"Determines the base server URL used for the Route Visualizer API.",
				RegistryStorageFlags.System,
				TextEditorType.Url,
				RegistryOptions.IsOnlyForSupport,
				"https://api-vt.wisegrid.net/");
		}

		#endregion

		#region CargoTracker

		public void TestEnableCargoTracker()
		{
			TestRegistryItem(
				ItemSet.EnableCargoTracker,
				"EnableCargoTracker",
				"Freight/Global Tracking/Cargo Tracker",
				"Enable Cargo Tracker",
				"When this registry is enabled, the View Consignment in Cargo Tracker menu item will be shown in Consol form Actions menu to all users.",
				RegistryStorageFlags.System,
				true);
		}

		public void TestCargoTrackerUrl()
		{
			TestRegistryItem(
				ItemSet.CargoTrackerUrl,
				"CargoTrackerUrl",
				"Freight/Global Tracking/Cargo Tracker",
				"Cargo Tracker URL",
				"Determines the base server URL used for Cargo Tracker.",
				RegistryStorageFlags.System,
				TextEditorType.Url,
				RegistryOptions.IsOnlyForSupport,
				"https://vt.wisegrid.net/");
		}

		public void TestCargoTrackerApiAudienceId()
		{
			TestRegistryItem(
				ItemSet.CargoTrackerApiAudienceId,
				"CargoTrackerApiAudienceId",
				"Freight/Global Tracking/Cargo Tracker",
				"Cargo Tracker API Audience ID",
				"Determines the Audience ID used to generate the authentication token required for accessing Cargo Tracker API.",
				RegistryStorageFlags.System,
				TextEditorType.Guid,
				RegistryOptions.IsOnlyForSupport,
				"8c619652-9df1-43e9-a14c-6abb26ec363e");
		}

		public void TestAisApiAudienceId()
		{
			TestRegistryItem(
				ItemSet.AisApiAudienceId,
				"AisApiAudienceId",
				"Freight/Global Tracking/Cargo Tracker",
				"AIS API Audience ID",
				"Determines the Audience ID used to generate the authentication token required for accessing AIS API.",
				RegistryStorageFlags.System,
				TextEditorType.Guid,
				RegistryOptions.IsOnlyForSupport,
				"659d969a-dee4-4756-8a8d-69e6b84e2c8c");
		}

		#endregion

		#region Performance Reporting

		public void TestReportingUrl()
		{
			TestRegistryItem(
				ItemSet.ReportingUrl,
				"ReportingUrl",
				FreightDataRegistry.Categories.Freight_GlobalTracking_MarketIntelligenceAndAnalytics,
				"Reporting URL",
				"Determines the base server URL used for the Performance Reports.",
				RegistryStorageFlags.System,
				TextEditorType.Url,
				RegistryOptions.IsOnlyForSupport,
				"https://cpr.wisegrid.net");
		}

		#endregion

		#endregion

		public void TestControllingCustomerUseSalesRep()
		{
			TestRegistryItem(ItemSet.ControllingCustomerUseSalesRep,
				"ControllingCustomerUseSalesRep",
				"Freight/Shipment",
				"Controlling Customer - Use Sales Rep",
				"Set this registry to Yes to default Controlling Customer's sales rep to the billing tab of a job in lieu of Local Client's sales rep.",
				RegistryStorageFlags.Company,
				false);
		}

		#region EnableSailingGenerationOnServiceTaskSaving

		public void TestEnableSailingGenerationOnServiceTaskSaving()
		{
			AssertEquals("Should be true by default for EnableSailingGenerationOnServiceTaskSaving", true, ItemSet.EnableSailingGenerationOnServiceTaskSaving.DefaultValue);
			AssertEquals("Should be visible for support or Developer only", RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport, ItemSet.EnableSailingGenerationOnServiceTaskSaving.Options);
			ItemSet.EnableSailingGenerationOnServiceTaskSaving.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Should set the registry item", false, ItemSet.EnableSailingGenerationOnServiceTaskSaving.Value);
		}

		#endregion

		#region TestJobServices

		public void TestJobServices()
		{
			TestGenericRegistryItem
			(
				ItemSet.JobServices,
				"JobServices",
				"Freight/Shipment",
				"Job Services",
				"Services that can be performed on a Shipment or Container",
				RegistryStorageFlags.System | RegistryStorageFlags.Company
			);

			var expectedDefaultCodes = typeof(FreightServiceTypes.Codes)
				.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(x => x.FieldType == typeof(string))
				.Select(x => (string)x.GetValue(null));

			AssertContainsExactElementsInAnyOrder("all default freight JobServices codes", expectedDefaultCodes, ItemSet.JobServices.DefaultValue.Cast<CodeDescriptionBool>().Select(x => x.Code.ToString()));

			var expectedDefaultDescriptions = typeof(FreightServiceTypes.Descriptions)
				.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(x => x.FieldType == typeof(string))
				.Select(x => (string)x.GetValue(null));

			AssertContainsExactElementsInAnyOrder("all default freight JobServices descriptions", expectedDefaultDescriptions, ItemSet.JobServices.DefaultValue.Cast<CodeDescriptionBool>().Select(x => x.Description.ToString()));
		}

		#endregion

		public void TestJobServices_DefaultsCodeAndDescriptionAreReadOnly()
		{
			CombineAssertions(() =>
			{
				foreach (CodeDescriptionBool cdb in ItemSet.JobServices.DefaultValue)
				{
					AssertEquals($"Default value code & description should be readonly - {cdb.Description}", true, cdb.SystemDefined);
				}
			});
		}

		public void TestEnableBookingConfirmation()
		{
			TestRegistryItem(ItemSet.EnableBookingConfirmation,
				"EnableBookingConfirmation",
				"Freight/Consolidations/Ocean Carrier Messaging",
				"Enable Booking Confirmation form",
				@"If yes, enables new Booking Confirmation form for testing. It will also send FormVersion 2 in Booking Request/Shipping Instruction message.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestEnableDraftBillOfLadingForm()
		{
			TestRegistryItem(ItemSet.EnableDraftBillOfLadingForm,
				"EnableDraftBillOfLadingForm",
				"Freight/Consolidations/Ocean Carrier Messaging",
				"Enable Draft Bill Of Lading form",
				"If yes, enable new Draft Bill Of Lading form for testing.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableBoleroEHBLIntegration()
		{
			AssertEnableBoleroEBLAndEHBLIntegrationRegistryItem(ItemSet.EnableBoleroEHBLIntegration,
				"EnableBoleroEHBLIntegration",
				"Freight/Shipment",
				"Enable Bolero eHBL Integration",
				"If yes, enable Bolero eHBL Integration for testing.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
				new BoleroEBLConfiguration()
				{
					EnableEBLIntegration = false,
					GalileoEndPointUrl = "https://galileo.boleroserve.net/galileo-portal/jwt/login",
					GalileoAudience = "ab2e99f4-15c9-41c8-a138-7d873c3c4f9f",
					GalileoTestEndPointUrl = "https://galileo.training.boleroserve.net/galileo-portal/jwt/login",
					GalileoTestAudience = "ef46c619-92d6-4f56-af3d-327cf3646508",
				});
		}

		void AssertEnableBoleroEBLAndEHBLIntegrationRegistryItem(
			IRegistryItem item,
			string expectedName,
			string expectedCategory,
			string expectedCaption,
			string expectedHint,
			RegistryStorageFlags expectedStorage,
			RegistryOptions options,
			BoleroEBLConfiguration expectedDefaultValue)
		{
			CombineAssertions(() =>
			{
				TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, options);
				AssertEquals(((BoleroEBLConfiguration)item.Value).EnableEBLIntegration, expectedDefaultValue.EnableEBLIntegration);
				AssertEquals(((BoleroEBLConfiguration)item.Value).GalileoAudience, expectedDefaultValue.GalileoAudience);
				AssertEquals(((BoleroEBLConfiguration)item.Value).GalileoEndPointUrl, expectedDefaultValue.GalileoEndPointUrl);
				AssertEquals(((BoleroEBLConfiguration)item.Value).GalileoTestAudience, expectedDefaultValue.GalileoTestAudience);
				AssertEquals(((BoleroEBLConfiguration)item.Value).GalileoTestEndPointUrl, expectedDefaultValue.GalileoTestEndPointUrl);
			});
		}

		#region TestEnableBoleroEBLIntegration

		public void TestEnableBoleroEBLIntegration()
		{
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.IntegrationBoleroElectronicMasterBLIntegration, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEnableBoleroEBLAndEHBLIntegrationRegistryItem(ItemSet.EnableBoleroEBLIntegration,
				"EnableBoleroEBLIntegration",
				"Freight/Consolidations",
				"Enable Bolero eBL Integration",
				"If yes, enable Bolero eBL Integration for testing.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
				new BoleroEBLConfiguration()
				{
					EnableEBLIntegration = true,
					GalileoEndPointUrl = "https://galileo.boleroserve.net/galileo-portal/jwt/login",
					GalileoAudience = "ab2e99f4-15c9-41c8-a138-7d873c3c4f9f",
					GalileoTestEndPointUrl = "https://galileo.training.boleroserve.net/galileo-portal/jwt/login",
					GalileoTestAudience = "ef46c619-92d6-4f56-af3d-327cf3646508",
				});
		}

		public void TestEnableBoleroEBLIntegration_Disabled()
		{
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.IntegrationBoleroElectronicMasterBLIntegration, CancellationToken.None)).Returns(Task.FromResult(null as IFeatureData));
			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEnableBoleroEBLAndEHBLIntegrationRegistryItem(ItemSet.EnableBoleroEBLIntegration,
				"EnableBoleroEBLIntegration",
				"Freight/Consolidations",
				"Enable Bolero eBL Integration",
				"If yes, enable Bolero eBL Integration for testing.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden,
				new BoleroEBLConfiguration()
				{
					EnableEBLIntegration = false,
					GalileoEndPointUrl = "https://galileo.boleroserve.net/galileo-portal/jwt/login",
					GalileoAudience = "ab2e99f4-15c9-41c8-a138-7d873c3c4f9f",
					GalileoTestEndPointUrl = "https://galileo.training.boleroserve.net/galileo-portal/jwt/login",
					GalileoTestAudience = "ef46c619-92d6-4f56-af3d-327cf3646508",
				});
		}

		#endregion

		#region Greenhouse Gas Emissions

		static IDisposable MockCO2eFeatureControl(bool enabled = true)
		{
			var mock = new Mock<ICO2eFeatureControlHelper>();
			mock.Setup(m => m.Enabled).Returns(enabled);
			return ObjectFactory.Substitute(mock.Object);
		}

		public void TestCO2eUserRequestProcessingMethod()
		{
			AssertEquals(CO2eUserRequestProcessingMethodCodeList.Codes.Api, ItemSet.CO2eUserRequestProcessingMethod.DefaultValue);

			ComboBoxRegistryEditorInfo info = (ComboBoxRegistryEditorInfo)ItemSet.CO2eUserRequestProcessingMethod.EditorInfo;
			AssertEquals("API, EHUB", info.LookUpList.CodesAsString);

			using (MockCO2eFeatureControl(false))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				AssertEquals(RegistryOptions.IsHidden, FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.Options);
			}
		}

		public void TestCO2eApiUrl()
		{
			using (MockCO2eFeatureControl(true))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				TestStringRegistryItem(
					ItemSet.CO2eApiUrl,
					"CO2eApiUrl",
					"Freight/Greenhouse Gas Emissions",
					"CO2e API Service URL",
					"The URL used to connect to CO2e API.",
					RegistryStorageFlags.System,
					TextEditorType.TextBox,
					RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
					"https://ec.wisegrid.net/",
					CharacterCase.Normal);
			}

			using (MockCO2eFeatureControl(false))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				AssertEquals(RegistryOptions.IsHidden, ItemSet.CO2eApiUrl.Options);
			}
		}

		public void TestCO2eApiRequestsTimeout()
		{
			using (MockCO2eFeatureControl(true))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				TestRegistryItem(
				ItemSet.CO2eApiRequestsTimeout,
				"CO2eApiRequestsTimeout",
				FreightDataRegistry.Categories.Freight_GreenhouseGasEmission,
				"CO2e API Requests Timeout",
				"Timeout in seconds when greenhouse gas emissions requests are executed",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				15,
				1,
				3600);
			}

			using (MockCO2eFeatureControl(false))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				AssertEquals(RegistryOptions.IsHidden, ItemSet.CO2eApiRequestsTimeout.Options);
			}
		}

		#endregion

		#region Pre Post Carriage Calculation

		public void TestEnablePrePostCarriageCalculation()
		{
			// Greenhouse gas calculation enabled
			using (MockCO2eFeatureControl(true))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				TestRegistryItem(ItemSet.EnablePrePostCarriageCalculation,
				"EnablePrePostCarriageCalculation",
				"Freight/Greenhouse Gas Emissions",
				"Enables Pre/On Carriage Calculation",
				"When enabled, the GHG calculation request via Action menu or Workflow trigger will include pre/on carriage legs",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				true);
			}

			// Greenhouse gas calculation disabled
			using (MockCO2eFeatureControl(false))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				AssertEquals(RegistryOptions.IsHidden, ItemSet.EnablePrePostCarriageCalculation.Options);
			}
		}

		#endregion

		#region UseHBLDeliveryModeForGHGCalculation

		public void TestUseHBLDeliveryModeForGHGCalculation()
		{
			// Greenhouse gas calculation enabled
			using (MockCO2eFeatureControl(true))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				TestRegistryItem(ItemSet.UseHBLDeliveryModeForGHGCalculation,
					"UseHBLDeliveryModeForGHGCalculation",
					"Freight/Greenhouse Gas Emissions",
					"Use HBL Delivery Mode",
					"If yes, system uses HBL Delivery Mode (when applicable) to determine the Pre-Carriage and On-Carriage legs included in the total CO2e emissions.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					true);
			}

			// Greenhouse gas calculation disabled
			using (MockCO2eFeatureControl(false))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				AssertEquals(RegistryOptions.IsHidden, ItemSet.UseHBLDeliveryModeForGHGCalculation.Options);
			}
		}

		#endregion

		#region EnableGlobalFlightSchedulesCalculation

		public void TestEnableGlobalFlightSchedulesCalculation()
		{
			// Greenhouse gas calculation enabled
			using (MockCO2eFeatureControl(true))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				TestRegistryItem(ItemSet.EnableGlobalFlightSchedulesCalculation,
					"EnableGlobalFlightSchedulesCalculation",
					"Freight/Greenhouse Gas Emissions",
					"Enable Global Flight Schedules Calculation",
					"Enables GHG calculation for Global Flight Schedule",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
					true);
			}

			// Greenhouse gas calculation disabled
			using (MockCO2eFeatureControl(false))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				AssertEquals(RegistryOptions.IsHidden, ItemSet.EnableGlobalFlightSchedulesCalculation.Options);
			}
		}

		#endregion

		#region EnableGlobalSailingSchedulesCalculation

		public void TestEnableGlobalSailingSchedulesCalculation()
		{
			// Greenhouse gas calculation enabled
			using (MockCO2eFeatureControl(true))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				TestRegistryItem(ItemSet.EnableGlobalSailingSchedulesCalculation,
					"EnableGlobalSailingSchedulesCalculation",
					"Freight/Greenhouse Gas Emissions",
					"Enable Global Sailing Schedules Calculation",
					"Enables GHG calculation for Global Sailing Schedule",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
					false);
			}

			// Greenhouse gas calculation disabled
			using (MockCO2eFeatureControl(false))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				AssertEquals(RegistryOptions.IsHidden, ItemSet.EnableGlobalSailingSchedulesCalculation.Options);
			}
		}

		#endregion

		#region AddEmissionsCalculationLogForShipment

		public void TestAddEmissionsCalculationLogForShipment()
		{
			// Greenhouse gas calculation enabled
			using (MockCO2eFeatureControl(true))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				TestRegistryItem(ItemSet.AddEmissionsCalculationLogForShipment,
					"AddEmissionsCalculationLogForShipment",
					"Freight/Greenhouse Gas Emissions",
					"Add Emissions Calculation Log For Shipment",
					"If yes, an Emissions Calculation Log will be added to the Shipment > Notes tab when the GHG calculation succeeds.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
					false);
			}

			// Greenhouse gas calculation disabled
			using (MockCO2eFeatureControl(false))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				AssertEquals(RegistryOptions.IsHidden, ItemSet.AddEmissionsCalculationLogForShipment.Options);
			}
		}

		#endregion

		public void TestEnableInternationalTradeDocumentsFunctionality()
		{
			TestRegistryItem(ItemSet.EnableInternationalTradeDocumentsFunctionality,
				"EnableInternationalTradeDocumentsFunctionality",
				"Freight",
				"Enables Certificate of Origin functionaltiy for International Trade Documents",
				"If yes, enables Certificate of Origin functionality.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				false);
		}

		#region EnableMexicanPortIntegrationFeatures

		public void TestEnableMexicanPortIntegrationFeatures()
		{
			TestRegistryItem(ItemSet.EnableMexicanPortIntegrationFeatures,
				"EnableMexicanPortIntegrationFeatures",
				"Freight",
				"Enables various Mexican port integration features",
				@"If yes, enable new Mexican port integration functionality for testing.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region EnableSpanishPortIntegrationFeatures

		public void TestEnableSpanishPortIntegrationFeatures()
		{
			TestRegistryItem(ItemSet.EnableSpanishPortIntegrationFeatures,
				"EnableSpanishPortIntegrationFeatures",
				"Freight",
				"Enables various Spanish sea port integration features",
				@"If yes, enable new Spanish sea port integration functionality for testing.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region OSMG

		public void TestConsolAllowAccessRegardlessOfShipmentsOSMGRights()
		{
			TestRegistryItem(
				ItemSet.ConsolAllowAccessRegardlessOfShipmentsOSMGRights,
				"ConsolAllowAccessRegardlessOfShipmentsOSMGRights",
				FreightDataRegistry.Categories.Freight_Consolidations,
				"Allow access to consol regardless of shipments OSMG rights",
				@"When this registry setting is set to ' Yes', the OSMG security rights on shipments will be ignored while accessing a consolidation.
When set to 'No', a consolidation will be accessible only if the user has rights to access all of its related shipments.",
				RegistryStorageFlags.System,
				true);
		}

		#endregion

		#region Assembly Master On Direct Consol Compliance Disclaimer

		public void TestAssemblyMasterOnDirectConsolComplianceDisclaimer()
		{
			TestRegistryItem(ItemSet.AssemblyMasterOnDirectConsolComplianceDisclaimer,
				"AssemblyMasterOnDirectConsolComplianceDisclaimer",
				FreightDataRegistry.Categories.Freight_Consolidations,
				"Assembly Master on Direct Consol - Compliance Disclaimer",
				@"Enable this system registry to allow users to check the tick box 'I acknowledge' to confirm their awareness that using Assembly Master as Direct Master might breach customs law.
When it is set to No, users will be required to type the disclaimer 'I AM AWARE THAT ASSEMBLY MASTER AS DIRECT MASTER MAY BRIDGE CUSTOMS LAW'.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		#endregion

		public void TestDeliveryDueDateAPIInboundAuthentications()
		{
			TestGenericRegistryItem(ItemSet.DeliveryDueDateAPIInboundAuthentications,
				"DeliveryDueDateAPIInboundAuthentications",
				"Freight",
				"Delivery Due Date API Inbound Authentications",
				"This configuration, defines username/password that is needed to allow access to Delivery Due Date API.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue);
		}

		#region FreightEnableComplianceWise

		public void TestFreightEnableComplianceWise()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name", "FreightEnableComplianceWise", ItemSet.FreightEnableComplianceWise.Name);
				AssertEquals("Categories.Length", 1, ItemSet.FreightEnableComplianceWise.Categories.Length);
				AssertEquals("Category", "Freight/Compliance", ItemSet.FreightEnableComplianceWise.Category);
				AssertEquals("Caption", "Enable ComplianceWise", ItemSet.FreightEnableComplianceWise.Caption);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.FreightEnableComplianceWise.Storage);
				AssertEquals("Hint", @"When disabled, ComplianceWise will not be available in the Booking, Shipment and Consolidation modules.

When enabled, the Booking, Shipment and Consolidation modules will use the ComplianceWise system to perform compliance risk checks. Compliance workflows and other compliance capabilities will be available to use in these modules.

Refer to WiseTech Academy eLearning to prepare and plan your transition to ComplianceWise.", ItemSet.FreightEnableComplianceWise.Hint);
				AssertEquals("Registry options", (RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.IsOnlyForSupport : (RegistryOptions.IsOnlyForSupport | RegistryOptions.IsHidden)), ItemSet.FreightEnableComplianceWise.Options);
				AssertEquals("Registry visible", RawDataRegistry.Instance.EnableComplianceRisk.Value, ItemSet.FreightEnableComplianceWise.IsVisible(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
				Assert("Registry default value", ItemSet.FreightEnableComplianceWise.Value.EnableComplianceWise);
			});
		}

		#endregion

		#region EnableCarrierAndClientContractModules

		public void TestEnableCarrierAndClientContractModules()
		{
			TestRegistryItem(ItemSet.EnableCarrierAndClientContractModules,
				"EnableCarrierAndClientContractModulesCWNext",
				"Freight/Contracts",
				"Enable Carrier Contract & Client Contract Modules",
				@"Set this registry to 'Yes' to enable Carrier Contract & Allocations module and Client Contract & Allocations module for production of clients who have subscribed to these functions.
Set this registry to 'Yes' to enable Carrier Contract & Allocations module and Client Contract & Allocations module for trial or testing in a non-Production environment.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region AllowSameUNLOCOForContractInternationalZones

		public void TestAllowSameUNLOCOForContractInternationalZones()
		{
			TestRegistryItem(ItemSet.AllowSameUNLOCOForContractInternationalZones,
				"AllowSameUNLOCOForContractInternationalZones",
				"Freight/Contracts",
				"Allow the same UNLOCO for multiple Contract International Zones",
				(NoResString)@"Allow the same UNLOCO to be added to multiple International Zones with Type of ‘CON’.

Note: Setting this registry to Yes may result in Allocation Routes with different but overlapping International Zones as Load / Discharge Ports being loaded into the ‘Contract & Allocation Routes Search Form’ launched from relevant jobs.",
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "EnableBoleroEBLIntegration";
				yield return "EnableScheduleFeedService";
				yield return "OnlineSailingSchedulesUrl";
				yield return nameof(FreightDataRegistry.OnlineSailingSchedulesTokenOverride);
				yield return "CustomEBookingApiServiceUrl";
				yield return "ScheduleFeedTrackingEHubID";
				yield return "OnlineSailingSchedulesAutoInitiatedTimeoutInSeconds";
				yield return "OnlineSailingSchedulesUserInitiatedTimeoutInSeconds";
				yield return "BOLClauseITAR";
				yield return "AirExpressShipmentServiceLevelCodeSendingarnumer";
				yield return "AirExpressShipmentBreakValueInEuro";
				yield return "ForwarderSendingarnumerCodeForSeafreight";
				yield return "ForwarderSendingarnumerCodeForAirfreight";
				yield return "OneStopContainerEventsEnabled";
				yield return "OneStopContainerEventsEnabledNZ";
				yield return "OneStopNotificationGroup";
				yield return "UAEDeliveryOrderNumberPrefix";
				yield return "CanadaCargoControlNumberCustomization";
				yield return "CanadaCargoControlNumberHBLDigits";
				yield return "CanadaCargoControlNumberBranchPrefix";
				yield return "CanadaConsolCargoControlNumberCustomization";
				yield return "CanadaConsolPreviousCargoControlNumberCustomization";
				yield return "CalculateDeliveryDueDateByTransportMode";
				yield return "CalculateDeliveryDateWithExceptions";
				yield return "ShipmentInspectionTypes_CA";
				yield return "ShipmentInspectionTypes_ZA";
				yield return "EXMExemptionCodeRemovalDate";
				yield return "EnableFlightMonitoringSystem";
				yield return "FlightMonitoringSystemEHubID";
				yield return "EbookingApiEndpoint";
				yield return "MAWBDefaultLowStockLevel";
				yield return "HVLVConsignmentsAndItemsPurgePeriod";
				yield return "GlobalTrackingShipmentVisibility";
				yield return "AutomaticUpdatingofPlannedLegs_Sea_FromBookingConfirmation";
				yield return "AutomaticUpdatingofPlannedLegs_Sea_FromContainerAutomationEvents";
				yield return nameof(FreightDataRegistry.PrintChargesBilledToLocalClientAtDestAsCollect);
				yield return nameof(FreightDataRegistry.ShipperLoadAndCount);
				yield return nameof(FreightDataRegistry.CO2eUserRequestProcessingMethod);
				yield return nameof(FreightDataRegistry.EnableGlobalFlightSchedulesCalculation);
				yield return nameof(FreightDataRegistry.EnableGlobalSailingSchedulesCalculation);
				yield return nameof(FreightDataRegistry.CO2eApiRequestsTimeout);
				yield return nameof(FreightDataRegistry.CO2eApiUrl);
				yield return nameof(FreightDataRegistry.UseHBLDeliveryModeForGHGCalculation);
				yield return nameof(FreightDataRegistry.EnablePrePostCarriageCalculation);
				yield return nameof(FreightDataRegistry.FreightEnableComplianceWise);
				yield return nameof(FreightDataRegistry.EnableSupplyChainSecurity_US);
				yield return nameof(FreightDataRegistry.EnableEnhancedSECEvent);
				yield return nameof(FreightDataRegistry.EnableVisibilityEventDelivery);
			}
		}
	}
}
