using System;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVDataRegistry))]
	sealed class HVLVDataRegistryTest : RegistryItemSetTestCaseWithFactory<HVLVDataRegistry>
	{
		public void TestIsHVLVAutoRatingEnabled()
		{
			TestRegistryItem(ItemSet.IsHVLVAutoRatingEnabled,
				"IsHVLVAutoRatingEnabled",
				"Freight/HVLV",
				"Enable HVLV Auto Rating",
				@"This is to enable HVLV Auto Rating.

If 'Yes' is selected, Items are rated considering Consignment/Items parameters. If 'No' is selected, Shipments are rated considering Shipment parameters.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				true);
		}

		public void TestIsHVLVLoadListMasterHouseDefault()
		{
			TestRegistryItem(ItemSet.IsHVLVLoadListMasterHouseDefault,
				"IsHVLVLoadListMasterHouseDefault",
				"Freight/HVLV",
				"HVLV Load List Master House Default",
				"Use to default HVLV Load List as a Master House.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestHVLVOriginLoadListTestingMode()
		{
			TestRegistryItem(ItemSet.HVLVOriginLoadListTestingMode,
				"HVLVOriginLoadListTestingMode",
				"Freight/HVLV",
				"Enable HVLV Origin Load List Testing Mode",
				"Turn this setting on to enable testing mode on HVLV Origin Load List module.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestIsHVLVOriginLoadListConsolAllocatorEnabled()
		{
			TestRegistryItem(ItemSet.ProcessLoadListUsingUniversalXML,
				"ProcessLoadListUsingUniversalXML",
				"Freight/HVLV",
				"Process Load List Using Universal XML",
				"Set to 'Yes' to process load list with universal XML. This is to replace current HVLV Origin Load List Helper with better readability and maintainability.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#region CalculateHVLVChargeablePerItem

		public void TestCalculateHVLVChargeablePerItem()
		{
			TestRegistryItem(ItemSet.CalculateHVLVChargeablePerItem,
				"CalculateHVLVChargeablePerItem",
				"Freight/HVLV",
				"Calculate HVLV Chargeable per Item",
				@"This registry determines how Chargeable is calculated for a HVLV Consignment.

When this registry is enabled, Consignment Chargeable will be calculated by first calculating the Chargeable of each attached Item, and then totaling the result.

When this registry is disabled, Consignment Chargeable will be calculated by first totaling the measurements of each attached Item, and then calculating Chargeable from the totaled measurements.",
				RegistryStorageFlags.All,
				false);
		}

		#endregion

		#region EnableHVLVFHLMessaging

		public void TestEnableHVLVFHLMessaging()
		{
			TestRegistryItem(ItemSet.EnableHVLVFHLMessaging,
				"EnableHVLVFHLMessaging",
				"Freight/HVLV",
				"Enable HVLV FHL Messaging",
				@"This is to enable FHL message for each HVLV Consignment. When set to 'Yes', an FHL message will be sent for the Consignments within the HVL Shipment but not the HVL Shipment itself.
Whereas when set to 'No', the FHL message will be sent only for the HVL Shipment, not its Consignments.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region Bulk Populate Consignment and Item IDs

		public void TestHVLVAutoGenerateConsignmentAndItemIDs()
		{
			var expectedHint = @"This registry determines how Consignment and Item IDs are generated.

When set to ‘Yes’, Consignment and Item IDs are generated from SSCC or CargoWise number fountain.

When set to ‘No’, Consignment and Item IDs are generated using system defaulting logic.";

			TestRegistryItem(ItemSet.AutoGenerateConsignmentAndItemIDs,
				"AutoGenerateConsignmentAndItemIDs",
				"Freight/HVLV",
				"Auto-generate Consignment and Item IDs",
				expectedHint,
				RegistryStorageFlags.System,
				true);
		}

		#endregion

		#region HVLV Last Mile Carrier Depot Calculations

		public void TestHVLVLMCDepotDetailsCalculation()
		{
			TestRegistryItem(ItemSet.HVLVAutomaticallyCalculateLMCDepotDetails,
				"HVLVAutomaticallyCalculateLMCDepotDetails",
				"Freight/HVLV/Last Mile Carrier",
				"Automatically Calculate HVLV Last Mile Carrier & Depot Details",
				@"Enables automatic calculation of Last Mile Carrier & Depot Details for HVLV Consignments via Service Tasks.

Automatic calculation only works when changes made to the Port/Depot/Carrier Selection related fields on HVLV Consignment are detected by Service Tasks. For existing HVLV Consignments without changes made, please use manual action to calculate.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestHVLVConsignmentOnlyPopulatesEmptyLMCDepotDetails()
		{
			TestRegistryItem(ItemSet.HVLVConsignmentOnlyPopulatesEmptyLMCDepotDetails,
				"HVLVConsignmentOnlyPopulatesEmptyLMCDepotDetails",
				"Freight/HVLV/Last Mile Carrier",
				"Last Mile Carrier and Depot Recalculation",
				@"Automatic re-calculation of Last Mile Carrier & Depot Details for HVLV Consignments via Service Tasks when Registry Item: Automatically Calculate HVLV Last Mile Carrier & Depot Details is enabled.

When set to 'No', behavior will be to populate and override if needed. Service Tasks will re-calculate and populate HVLV Consignment Last Mile Carrier and Depot Details when changes are made to the Port/Depot/Carrier Selection related fields on a HVLV Consignment. Already populated fields will be overridden after re-calculation.

When set to 'Yes', behavior will be to only populate if empty. Service Tasks will only re-calculate and populate HVLV Consignment Last Mile Carrier and Depot Details that are not already populated when changes are made to the Port/Depot/Carrier Selection related fields on a HVLV Consignment.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region HVLV Validation & Screening

		public void TestHVLVDetailsPreScreeningConfiguration()
		{
			TestGenericRegistryItem(
				ItemSet.HVLVDetailsPreScreeningConfiguration,
				"HVLVDetailsPreScreeningConfiguration",
				"Freight/HVLV",
				"HVLV Pre-Screening",
				"Use this registry setting to configure pre-screening on specific HVLV fields for stop words, phrases or values.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.NotCached);
		}

		public void TestHVLVDetailsPreScreeningConfiguration_PopulatesRegistryValue()
		{
			var newValue = new HVLVDetailsPreScreeningConfiguration();
			var ruleLine = newValue.Rules.AddNew();
			newValue.IsEnabled = true;
			ruleLine.TransportMode = "AIR";
			ruleLine.OriginCountryCode = "AU";
			ruleLine.DestinationCountryCode = "US";

			var fieldLine = ruleLine.Fields.AddNew();
			fieldLine.FieldDescription = "Consignee";
			fieldLine.ValidationRule = "ERR";

			var screeningValue = fieldLine.ScreeningValues.AddNew();
			screeningValue.ScreeningValue = "TestScreeningValue";

			using (ItemSet.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue))
			{
				var value = ItemSet.HVLVDetailsPreScreeningConfiguration.Value;
				AssertEquals(true, value.IsEnabled);
				AssertEquals(1, value.Rules.Count);

				var rule = value.Rules[0];
				CombineAssertions(() =>
				{
					AssertEquals("TransportMode:", "AIR", rule.TransportMode);
					AssertEquals("OriginCountryCode:", "AU", rule.OriginCountryCode);
					AssertEquals("DestinationCountryCode:", "US", rule.DestinationCountryCode);
					AssertEquals("FieldDescription:", "Consignee", rule.Fields[0].FieldDescription);
					AssertEquals("Field NotificationType:", "ERR", rule.Fields[0].ValidationRule);
					AssertEquals("Field ScreeningValue:", "TestScreeningValue", rule.Fields[0].ScreeningValues[0].ScreeningValue);
				});
			}
		}

		#endregion

		#region Commodity Code

		public void TestHVLVOuterPackageCommodityCode()
		{
			object defaultHVLVCommodityCodeObj = Db.Connection.ExecuteScalar(
				"select " + RefCommodityCodeSchema.PK.Name +
				" from " + RefCommodityCodeSchema.Constants.SqlSchemaName + "." + RefCommodityCodeSchema.Constants.TableName +
				" where " + RefCommodityCodeSchema.RH_Code.Name + " = @code",
				cmd => cmd.AddParameterBasedOnDbColumn("@code", "GEN", RefCommodityCodeSchema.RH_Code));
			Guid defaultHVLVCommodityCode = defaultHVLVCommodityCodeObj != null ? (Guid)defaultHVLVCommodityCodeObj : Guid.Empty;
			TestRegistryItem(ItemSet.HVLVOuterPackageCommodityCode,
					"HVLVOuterPackageCommodityCode",
					"Freight/HVLV",
					"Commodity Code Default",
					"Use to default the Commodity Code on the HVLV Outer Package",
					RegistryStorageFlags.All,
					RegistryFindBoxCollection.RefCommodityCode,
					defaultHVLVCommodityCode);
		}

		#endregion

		#region Enable Security Filings For Non USA Companies

		public void TestEnableSecurityFilingsForNonUSACompanies()
		{
			TestRegistryItem(ItemSet.EnableSecurityFilingsForNonUSACompanies,
				"EnableSecurityFilingsForNonUSACompanies",
				"Freight/HVLV/Customs/United States of America",
				"Enable Security Filings for non USA based Companies",
				"When set to 'Yes' companies outside of the USA can create Security Filings from a HVL shipment that has a destination country of the USA.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region Use new 'Port/Carrier/Depot Selection' module

		public void TestPortCarrierDepotSelectionModule()
		{
			TestRegistryItem(ItemSet.PortCarrierDepotSelectionModule,
				"PortCarrierDepotSelectionModule",
				"Freight/HVLV",
				"Use new 'Port/Carrier/Depot Selection' module",
				"When set to 'Yes', the new 'Port/Carrier/Depot Selection' module will be used",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region HVLV Customs

		const string RemoveNonWesternEuropeanCharactersExpectedCaption = "Remove Non Western European Characters";

		#region Customs AU

		public void TestRemoveNonWesternEuropeanCharactersAUAirCargoReport()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersAUAirCargoReport,
				"RemoveNonWesternEuropeanCharactersAUAirCargoReport",
				"Freight/HVLV/Customs/Australia/Air Cargo Report",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Air Cargo Reports from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestRemoveNonWesternEuropeanCharactersAUExportSubManifest()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersAUExportSubManifest,
				"RemoveNonWesternEuropeanCharactersAUExportSubManifest",
				"Freight/HVLV/Customs/Australia/Export Sub Manifest (ESM)",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Export Sub Manifests from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestRemoveNonWesternEuropeanCharactersAUSeaCargoReport()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersAUSeaCargoReport,
				"RemoveNonWesternEuropeanCharactersAUSeaCargoReport",
				"Freight/HVLV/Customs/Australia/Sea Cargo Report",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Sea Cargo Reports from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestRemoveNonWesternEuropeanCharactersAUStandAloneDeclaration()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersAUStandAloneDeclaration,
				"RemoveNonWesternEuropeanCharactersAUStandAloneDeclaration",
				"Freight/HVLV/Customs/Australia/Stand Alone Declaration",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Stand Alone Declarations from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region Customs CA

		public void TestRemoveNonWesternEuropeanCharactersCAeManifest()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersCAeManifest,
				"RemoveNonWesternEuropeanCharactersCAeManifest",
				"Freight/HVLV/Customs/Canada/eManifest",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating eManifests from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestEnableHVLVCAeManifest()
		{
			TestRegistryItem(ItemSet.EnableHVLVCAeManifest,
				"EnableHVLVCAeManifest",
				"Freight/HVLV/Customs/Canada/eManifest",
				"Enable HVLV CA eManifest Forwarder Messaging",
				"This to enable eManifest Forwarder messaging for each HVLV Consignment. An eManifest message will not be sent for the shipment house bill when enabled.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region Customs NZ

		public void TestRemoveNonWesternEuropeanCharactersNZAirCargoReport()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersNZAirCargoReport,
				"RemoveNonWesternEuropeanCharactersNZAirCargoReport",
				"Freight/HVLV/Customs/New Zealand/ICR & CRE/Air Cargo ICR & CRE",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Air Cargo ICR/CREs from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestRemoveNonWesternEuropeanCharactersNZSeaCargoReport()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersNZSeaCargoReport,
				"RemoveNonWesternEuropeanCharactersNZSeaCargoReport",
				"Freight/HVLV/Customs/New Zealand/ICR & CRE/Sea Cargo ICR & CRE",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Sea Cargo ICR/CREs from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestRemoveNonWesternEuropeanCharactersNZStandAloneDeclaration()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersNZStandAloneDeclaration,
				"RemoveNonWesternEuropeanCharactersNZStandAloneDeclaration",
				"Freight/HVLV/Customs/New Zealand/Stand Alone Declaration",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Stand Alone Declarations from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestEnableHVLVMultiShipmentNZICRCRE()
		{
			var expectedHint = @"This registry enable add multiple shipments to same New Zealand ICR/CRE if they belong to same consol.

When set to ‘Yes’, shipments linked to same consol will be added to same ICR/CRE.

When set to ‘No’, separate ICR/CRE for each shipment.";

			TestRegistryItem(ItemSet.EnableHVLVMultiShipmentNZICRCRE,
				"EnableHVLVMultiShipmentNZICRCRE",
				"Freight/HVLV/Customs/New Zealand/ICR & CRE",
				"Enable Multi Shipment ICR/CRE",
				expectedHint,
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		#region Customs US

		public void TestRemoveNonWesternEuropeanCharactersUSACAS()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersUSACAS,
				"RemoveNonWesternEuropeanCharactersUSACAS",
				"Freight/HVLV/Customs/United States of America/ACAS",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating ACAS Reports from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestRemoveNonWesternEuropeanCharactersUSAirAMS()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersUSAirAMS,
				"RemoveNonWesternEuropeanCharactersUSAirAMS",
				"Freight/HVLV/Customs/United States of America/Air AMS",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Air AMS fillings from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#region Customs US eManifest

		public void TestRemoveNonWesternEuropeanCharactersUSeManifest()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersUSeManifest,
				"RemoveNonWesternEuropeanCharactersUSeManifest",
				"Freight/HVLV/Customs/United States of America/e-Manifest",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating e-Manifests from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestSkipeManifestShipmentValidation()
		{
			TestRegistryItem(ItemSet.SkipeManifestShipmentValidation,
				"SkipeManifestShipmentValidation",
				"Freight/HVLV/Customs/United States of America/e-Manifest",
				"Skip eManifest Shipment Validation",
				"Setting this registry to Yes will skip validation of e-Manifest Shipments when transmitting messages to Customs.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				false);
		}

		#endregion

		public void TestRemoveNonWesternEuropeanCharactersUSImporterSecurityFilling()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersUSImporterSecurityFilling,
				"RemoveNonWesternEuropeanCharactersUSImporterSecurityFilling",
				"Freight/HVLV/Customs/United States of America/Importer Security Filling",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Importer Security Fillings from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestRemoveNonWesternEuropeanCharactersUSSeaAMS()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersUSSeaAMS,
				"RemoveNonWesternEuropeanCharactersUSSeaAMS",
				"Freight/HVLV/Customs/United States of America/Sea AMS",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Sea AMS fillings from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestRemoveNonWesternEuropeanCharactersUSLowValueEntries()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersUSLowValueEntries,
				"RemoveNonWesternEuropeanCharactersUSLowValueEntries",
				"Freight/HVLV/Customs/United States of America/Low Value Entries",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Low Value Entries from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestRemoveNonWesternEuropeanCharactersUSStandAloneDeclaration()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersUSStandAloneDeclaration,
				"RemoveNonWesternEuropeanCharactersUSStandAloneDeclaration",
				"Freight/HVLV/Customs/United States of America/Stand Alone Declaration",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Stand Alone Declarations from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region Customs SG

		public void TestRemoveNonWesternEuropeanCharactersSGSGAccessImportManifest()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersSGSGAccessImportManifest,
				"RemoveNonWesternEuropeanCharactersSGSGAccessImportManifest",
				"Freight/HVLV/Customs/Singapore/SG Access Import Manifest",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating SG Access Import Manifests from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region Customs EU

		public void TestRemoveNonWesternEuropeanCharactersEUICS2Manifest()
		{
			TestRegistryItem(ItemSet.RemoveNonWesternEuropeanCharactersEUICS2Manifest,
				"RemoveNonWesternEuropeanCharactersEUICS2Manifest",
				"Freight/HVLV/Customs/EU/ICS2",
				RemoveNonWesternEuropeanCharactersExpectedCaption,
				"This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating EU ICS2 Manifests from HVLV Shipments.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedDefaultValue: false);
		}

		#endregion

		#endregion

		#region Threshold to Auto Load HVLV Consignments on Shipment Form

		public void TestThresholdToAutoLoadHVLVConsignmentsOnShipmentForm()
		{
			TestRegistryItem(ItemSet.ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm,
				"ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm",
				"Freight/HVLV",
				"HVLV Consignment Auto Load Threshold",
				"Specifies the number of HVLV Consignments on a HVL Shipment that will be loaded by default. When the number of HVLV Consignments exceeds the threshold, Consignments grid will not load any records and will be set to read-only.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				1000,
				0,
				10000);
		}

		#endregion

		#region Bulk Copy Batch Size

		public void TestBulkCopyBatchSize()
		{
			TestRegistryItem(
				item: ItemSet.HVLVDataBulkCopyBatchSize,
				expectedName: "HVLVDataBulkCopyBatchSize",
				expectedCategory: "Freight/HVLV",
				expectedCaption: "Bulk Copy Batch Size",
				expectedHint: "This registry setting controls the Batch Size of SQL Bulk Copy process when saving HVLV data.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: 100000,
				expectedMinValue: 100,
				expectedMaxValue: 100000);
		}

		#endregion

		#region Update HVL Shipments Weight, Volume and Inners Default

		public void TestHVLVShipmentWeightUpdateMethod()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair(Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromConsignmentsDetails, Constants.ShipmentWeightUpdateOptions.Description.ShowWarningFromConsignmentsDetails);
			list.AddPair(Constants.ShipmentWeightUpdateOptions.Code.AlwaysUpdateFromConsignmentsDetails, Constants.ShipmentWeightUpdateOptions.Description.AlwaysUpdateFromConsignmentsDetails);
			list.AddPair(Constants.ShipmentWeightUpdateOptions.Code.DoNotUpdate, Constants.ShipmentWeightUpdateOptions.Description.DoNotUpdate);
			list.AddPair(Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromPackingDetails, Constants.ShipmentWeightUpdateOptions.Description.ShowWarningFromPackingDetails);
			list.AddPair(Constants.ShipmentWeightUpdateOptions.Code.AlwaysUpdateFromPackingDetails, Constants.ShipmentWeightUpdateOptions.Description.AlwaysUpdateFromPackingDetails);

			TestRegistryItem(ItemSet.HVLVShipmentWeightUpdateMethod,
					"HVLVShipmentWeightUpdateMethod",
					"Freight/HVLV",
					"Update HVL Shipments Weight, Volume and Inners Default",
					"Defaults how HVL Shipment weight, volume and inners are updated when not matching with Items total weight, volume and counts upon saving the shipment.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					list,
					Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromConsignmentsDetails);
		}

		#endregion

		#region HVLV Denied Party Screening

		public void TestHVLVEnablePartyScreening()
		{
			TestGenericRegistryItem(
				ItemSet.HVLVEnablePartyScreening,
				"HVLVEnablePartyScreening",
				"Freight/HVLV/Denied Party Screening",
				"Enable HVLV Party Screening",
				@"Use this registry setting to enable Denied Party Screening for HVLV parties and enable new DPS result form.

When HVLV Party Screening is enabled, HVLV parties will be screened from Consol, Shipment and HVLV Booking Header.

When New DPS Result Form is enabled, potential matches will be displayed on the new form when DPS is run from a Consol that contains HVL Shipment(s), a HVL Shipment or a HVLV Booking Header.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);
		}

		#endregion

		#region Bulk Copy

		public void TestEnableHVLVDataBulkCopy()
		{
			TestRegistryItem(ItemSet.EnableHVLVDataBulkCopy,
				"EnableHVLVDataBulkCopy",
				"Freight/HVLV",
				"Enable Bulk Copy",
				"Enable SQL Bulk Copy for HVLV data.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: true);
		}

		#endregion
	}
}

