using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WarehouseDataRegistry))]
	sealed class WarehouseDataRegistryTest : RegistryItemSetTestCaseWithFactory<WarehouseDataRegistry>
	{
		#region TestCreateNewAddressOnUnmatchedAddressForOrders

		public void TestCreateNewAddressOnUnmatchedAddressForOrders()
		{
			TestRegistryItem(ItemSet.CreateNewAddressOnUnmatchedAddressForOrders,
				"CreateNewAddressOnUnmatchedAddressForOrders",
				WarehouseDataRegistry.Categories.Warehouse_Orders,
				"Create new Organization Address on Unmatched Address",
				"When this item is enabled, during Legacy XML Import of a Warehouse Order, a new Organization Address will be automatically created when an Organization is matched but the Address does not match.",
				RegistryStorageFlags.System,
				true);
		}

		#endregion

		#region TestAdjustmentReasonCodes

		public void TestAdjustmentReasonCodes()
		{
			TestRegistryItemWithDefaultCode(
				ItemSet.AdjustmentReasonCodes,
				"AdjustmentReasonCodes",
				WarehouseDataRegistry.Categories.Warehouse_Adjustments,
				"Adjustment Reason Codes",
				"Adjustment Reason Codes",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, 3, 6);

			AssertEquals("Stocktake Adjustment", ItemSet.AdjustmentReasonCodes.DefaultValue.GetDescriptionFromCode("STA"));
			AssertEquals("Damaged Stock", ItemSet.AdjustmentReasonCodes.DefaultValue.GetDescriptionFromCode("DAM"));
			AssertEquals("Client Instructed", ItemSet.AdjustmentReasonCodes.DefaultValue.GetDescriptionFromCode("CLI"));
			AssertEquals("Shrinkage", ItemSet.AdjustmentReasonCodes.DefaultValue.GetDescriptionFromCode("SHR"));
			AssertEquals("Customs Amendment", ItemSet.AdjustmentReasonCodes.DefaultValue.GetDescriptionFromCode("AMD"));
			AssertEquals("Change Ownership", ItemSet.AdjustmentReasonCodes.DefaultValue.GetDescriptionFromCode("OCH"));
			AssertEquals("STA", ItemSet.AdjustmentReasonCodes.DefaultValue.DefaultCode);
		}

		#endregion

		#region ABCAnalysis

		#region TestABCAnalysisPeriod

		public void TestABCAnalysisPeriod()
		{
			CodeDescriptionPairList list = ItemSet.ABCAnalysisPeriodList;
			TestRegistryItem(
				ItemSet.ABCAnalysisPeriod,
				"WarehouseABCAnalysisPeriod",
				WarehouseDataRegistry.Categories.Warehouse_ABCAnalysis,
				"ABC Analysis Period",
				"The ABC Analysis Period determines the time range of transactions that will be used when calculating the ABC category of a product. The registry setting can be changed to suit the business requirements.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				list,
				"WKY");
		}

		#endregion

		#region TestABCAnalysisMethod

		public void TestABCAnalysisMethod()
		{
			CodeDescriptionPairList list = ItemSet.ABCAnalysisMethodList;
			TestRegistryItem(
				ItemSet.ABCAnalysisMethod,
				"WarehouseABCAnalysisMethod",
				WarehouseDataRegistry.Categories.Warehouse_ABCAnalysis,
				"ABC Analysis Method",
				"The ABC Analysis Method determines how the system will use the analysis period's transaction data to calculate the category.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				list,
				"VLC");
		}

		#endregion

		#region TestABCAnalysisCategories

		public void TestABCAnalysisCategories()
		{
			TestGenericRegistryItem(
				ItemSet.ABCAnalysisCategories,
				"WarehouseABCAnalysisCategories",
				WarehouseDataRegistry.Categories.Warehouse_ABCAnalysis,
				"ABC Analysis Categories",
				"The ABC Analysis Category determines how the period and method data are categorized and displayed to users of the Warehouse module.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
		}

		#endregion

		#endregion

		#region Charegable

		#region TestWarehouseChargeableFactorStorage

		public void TestWarehouseChargeableFactorStorage()
		{
			TestGenericRegistryItem(
				ItemSet.WarehouseChargeableFactorStorage,
				"WarehouseChargeableFactorStorage",
				WarehouseDataRegistry.Categories.Warehouse_Chargeable,
				"Chargeable Factor for Warehouse Storage",
				@"The chargeable factor will be used to convert the actual volume into a chargeable weight.

The chargeable weight is the greater of the actual weight or actual volume divided by the chargeable factor.

Max of [ weight(KG) , volume(CM3) / factor ] = KG

Default chargeable factor is 6000 CC/KG.

Max of [ weight(LB) , volume(CI) / factor ] = LB

Default chargeable factor is 194 CI/LB.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic));
		}

		#endregion

		#region TestWarehouseChargeableFactorHandling

		public void TestWarehouseChargeableFactorHandling()
		{
			TestGenericRegistryItem(
				ItemSet.WarehouseChargeableFactorHandling,
				"WarehouseChargeableFactorHandling",
				WarehouseDataRegistry.Categories.Warehouse_Chargeable,
				"Chargeable Factor for Warehouse Handling",
				@"The chargeable factor will be used to convert the actual volume into a chargeable weight.

The chargeable weight is the greater of the actual weight or actual volume divided by the chargeable factor.

Max of [ weight(KG) , volume(CM3) / factor ] = KG

Default chargeable factor is 6000 CC/KG.

Max of [ weight(LB) , volume(CI) / factor ] = LB

Default chargeable factor is 194 CI/LB.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic));
		}

		#endregion

		#endregion

		#region TestIFSOrgProxy

		public void TestIFSOrgProxy()
		{
			TestRegistryItem(
				ItemSet.IFSOrgProxy,
				"IFSOrgProxy",
				WarehouseDataRegistry.Categories.Warehouse_Release,
				"IFS Organization",
				"You should create an IFS Organization and then setup IFS Code Mappings on the Organization's 'EDI Code Mapping' tab.",
				RegistryStorageFlags.System,
				RegistryFindBoxCollection.OrgHeader,
				Guid.Empty);
		}

		#endregion

		#region Freight Location Is True WHS Location

		public void TestFreightLocationIsTrueWHSLocation()
		{
			TestRegistryItem(
				ItemSet.FreightLocationIsTrueWHSLocation,
				"FreightLocationIsTrueWHSLocation",
				WarehouseDataRegistry.Categories.Warehouse_Freight,
				"Freight Location Is True WHS Location",
				"By setting this, the Freight Location will become a true Warehouse location and be validated as such.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				false);
		}

		#endregion

		#region UOMPackType

		public void TestUOMPackType()
		{
			TestGenericRegistryItem(
				ItemSet.UOMPackType,
				"UOMPackType",
				WarehouseDataRegistry.Categories.Warehouse_Picking,
				"UOM Pack Types",
				"A list of Warehouse UOM pack types.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);

			var packTypes = ItemSet.UOMPackType.Value.Cast<UOMPackType>().OrderBy(l => l.Code).ToArray();
			AssertUOMPackType(packTypes[0], UOMPackTypesList.Codes.Case, UOMPackTypesList.Descriptions.Case, 1);
			AssertUOMPackType(packTypes[1], UOMPackTypesList.Codes.Pallet, UOMPackTypesList.Descriptions.Pallet, 1);
			AssertUOMPackType(packTypes[2], UOMPackTypesList.Codes.SplitCase, UOMPackTypesList.Descriptions.SplitCase, 1);
		}

		void AssertUOMPackType(UOMPackType packType, string expectedCode, string expectedDescription, int expectedNumberOfLabels)
		{
			AssertEquals(expectedCode, packType.Code);
			AssertEquals(expectedDescription, packType.Description);
			AssertEquals(expectedNumberOfLabels, packType.NumberOfLabels);
		}

		#endregion

		#region Transit Warehouse

		public void TestTransitWarehouseBranchesThatAllowMixedSecurityFreight()
		{
			TestGenericRegistryItem(
				ItemSet.TransitWarehouseBranchesThatAllowMixedSecurityFreight,
				"TransitWarehouseBranchesThatAllowMixedSecurityFreight",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Branches that allow Secure and Non-Secure freight on vehicles",
				"Add Branches for Transit Warehouses that treat Secure Freight mixed with Non-Secure Freight (on Vehicles) as secure.",
				RegistryStorageFlags.System);
		}

		public void TestEnableImportingCoLoadMastersWithoutSubs()
		{
			TestGenericRegistryItem(
				ItemSet.EnableImportingCoLoadMastersWithoutSubs,
				"EnableImportingCoLoadMastersWithoutSubs",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Enable Importing Co-Load Masters Without Subs",
				"Will enable Importing Co-Load Masters without Sub-shipments when importing UXML shipment from Forwarding.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnablePackageSealNumbers()
		{
			TestGenericRegistryItem(
				ItemSet.EnablePackageSealNumbers,
				"EnablePackageSealNumbers",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Enable Package Seal Numbers",
				"Will enable Seal Numbers on Package.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableEcommercePortal()
		{
			TestGenericRegistryItem(
				ItemSet.EnableEcommercePortal,
				"EnableEcommercePortal",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Enable Ecommerce Portal",
				"Enabling this creates a link on the Desktop Portal to the Ecommerce Module.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#region DefaultPhotoDocumentType

		public void TestDefaultPhotoDocumentType()
		{
			TestGenericRegistryItem(
				ItemSet.DefaultPhotoDocumentType,
				"DefaultPhotoDocumentType",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Default Photo Document Type",
				"The selected document type will be defaulted when taking/uploading photos to the eDocs from mobile devices.",
				RegistryStorageFlags.System,
				"MCF"
			);
		}

		#endregion

		#region TestApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation

		public void TestApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation()
		{
			TestRegistryItem(ItemSet.ApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation, "ApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation", WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Apply package quantity counting algorithm for customs status calculation", "Apply package quantity counting algorithm when calculating customs status of receive consignment and packages.", RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, false);
		}

		public void TestApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation_OnUpdate_ChangeToFalse() => TestApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation_OnUpdate_Core(false);
		public void TestApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation_OnUpdate_ChangeToTrue() => TestApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation_OnUpdate_Core(true);

		void TestApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation_OnUpdate_Core(bool newStatus)
		{
			var registryItem = ItemSet.ApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation;

			var checkProcedureName = "UpdatePackageStateAndRCNCustomStatus";
			var mockedCheckProcedure = "\r\nALTER PROC " + checkProcedureName + " (@CompanyBranchPK UNIQUEIDENTIFIER, @SystemLastEditUser VARCHAR(3), @CurrentUTC DATETIME, @WarehouseConfigCustomControlled BIT, @WarehouseConfigPortControlled BIT, @IsApplyPackageQuantityCountingAlgorithm BIT, @PackageStatePKs dbo.TVP_uniqueidentifier READONLY) AS\r\nBEGIN\r\n\tDECLARE @result varchar(max) = CONCAT(CONVERT(NVARCHAR(36), @CompanyBranchPK), ',',  @SystemLastEditUser, ',', @CurrentUTC, ',', @WarehouseConfigCustomControlled, ',', @WarehouseConfigPortControlled, ',', @IsApplyPackageQuantityCountingAlgorithm, ',', (SELECT COUNT(*) FROM @PackageStatePKs))\r\n\tRAISERROR(@result, 18, 1)\r\n\tRETURN 1\r\nEND";

			Db.Connection.ExecuteNonQuery(mockedCheckProcedure);
			var guid = Guid.NewGuid();
			registryItem.OnUpdateAction(Guid.Empty, guid, Guid.Empty, newStatus);
			registryItem.OnAllValuesSaved();

			AssertEquals("Error Reported", guid.ToString().ToUpper() + "," + ((IGlbStaff)Env.CurrentUser).GS_Code + "," + "," + "," + "," + Convert.ToInt32(newStatus) + "," + 0, ErrorReporter.LastMessageReported.ToUpper());
			ErrorReporter.Clear();
		}

		#endregion

		#region TestPackageScreeningMethods

		public void TestPackageScreeningMethod()
		{
			TestRegistryItemWithDefaultCode(ItemSet.PackageScreeningMethods, "PackageScreeningMethods", WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse, "Package Screening Methods", "Add Package Screening Methods used to determine if a freight can be released.", RegistryStorageFlags.System, 3, 18);
		}

		public void TestDefaultPackageScreeningMethodTypes()
		{
			var defaultValue = ItemSet.PackageScreeningMethods.DefaultValue;
			AssertEquals("DefaultValue.Count", 18, defaultValue.Count);
			AssertEquals(ScreeningMethods.Descriptions.PhysicalInspectionAndHandSearch, defaultValue.GetDescriptionFromCode(ScreeningMethods.Codes.PhysicalInspectionAndHandSearch));
			AssertEquals(ScreeningMethods.Descriptions.VisualCheck, defaultValue.GetDescriptionFromCode(ScreeningMethods.Codes.VisualCheck));
			AssertEquals(ScreeningMethods.Descriptions.XRayEquipment, defaultValue.GetDescriptionFromCode(ScreeningMethods.Codes.XRayEquipment));
			AssertEquals(ScreeningMethods.Descriptions.ExplosiveDetectionSystem, defaultValue.GetDescriptionFromCode(ScreeningMethods.Codes.ExplosiveDetectionSystem));
			AssertEquals(ScreeningMethods.Descriptions.SubjectedToAnyOtherMeans, defaultValue.GetDescriptionFromCode(ScreeningMethods.Codes.SubjectedToAnyOtherMeans));
			AssertEquals(ScreeningMethods.Descriptions.ExplosiveDetectionDogs, defaultValue.GetDescriptionFromCode(ScreeningMethods.Codes.ExplosiveDetectionDogs));
			AssertEquals(ScreeningMethods.Descriptions.ExplosivesTraceDetectionEquipment, defaultValue.GetDescriptionFromCode(ScreeningMethods.Codes.ExplosivesTraceDetectionEquipment));
			AssertEquals(ScreeningMethods.Descriptions.CargoMetalDetection, defaultValue.GetDescriptionFromCode(ScreeningMethods.Codes.CargoMetalDetection));
			AssertEquals(ScreeningMethods.Descriptions.ExplosivesVaporDetection, defaultValue.GetDescriptionFromCode(ScreeningMethods.Codes.ExplosivesVaporDetection));
			AssertEquals(ExemptionCodes.Descriptions.SmallUndersizedShipments, defaultValue.GetDescriptionFromCode(ExemptionCodes.Codes.SmallUndersizedShipments));
			AssertEquals(ExemptionCodes.Descriptions.Mail, defaultValue.GetDescriptionFromCode(ExemptionCodes.Codes.Mail));
			AssertEquals(ExemptionCodes.Descriptions.BiomedicalSamples, defaultValue.GetDescriptionFromCode(ExemptionCodes.Codes.BiomedicalSamples));
			AssertEquals(ExemptionCodes.Descriptions.DiplomaticBagsOrDiplomaticMail, defaultValue.GetDescriptionFromCode(ExemptionCodes.Codes.DiplomaticBagsOrDiplomaticMail));
			AssertEquals(ExemptionCodes.Descriptions.LifeSavingMaterials, defaultValue.GetDescriptionFromCode(ExemptionCodes.Codes.LifeSavingMaterials));
			AssertEquals(ExemptionCodes.Descriptions.NuclearMaterial, defaultValue.GetDescriptionFromCode(ExemptionCodes.Codes.NuclearMaterial));
			AssertEquals(ExemptionCodes.Descriptions.TransferOrTransshipment, defaultValue.GetDescriptionFromCode(ExemptionCodes.Codes.TransferOrTransshipment));
			AssertEquals(ExemptionCodes.Descriptions.GovernmentApprovedReliableOrganization, defaultValue.GetDescriptionFromCode(ExemptionCodes.Codes.GovernmentApprovedReliableOrganization));
			AssertEquals(ExemptionCodes.Descriptions.AdHocMovementsOfCargo, defaultValue.GetDescriptionFromCode(ExemptionCodes.Codes.AdHocMovementsOfCargo));
			AssertEquals(string.Empty, ItemSet.PackageScreeningMethods.DefaultValue.DefaultCode);
		}

		#endregion

		#region TestMandatoryPackageScreeningForAirAndUnknownTransportMode

		public void TestMandatoryPackageScreeningForAirAndUnknownTransportMode()
		{
			TestRegistryItem(ItemSet.MandatoryPackageScreeningForAirAndUnknownTransportMode, "MandatoryPackageScreeningForAirAndUnknownTransportMode", WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Package must pass its latest screening before it can be loaded", "Package must pass its latest screening before it can be loaded if transport mode is Air or Unknown.", RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, true);
		}

		public void TestMandatoryPackageScreeningForAirAndUnknownTransportMode_OnUpdate_ChangeToFalse() => TestMandatoryPackageScreeningForAirAndUnknownTransportMode_OnUpdate_Core(false);
		public void TestMandatoryPackageScreeningForAirAndUnknownTransportMode_OnUpdate_ChangeToTrue() => TestMandatoryPackageScreeningForAirAndUnknownTransportMode_OnUpdate_Core(true);

		void TestMandatoryPackageScreeningForAirAndUnknownTransportMode_OnUpdate_Core(bool newStatus)
		{
			var registryItem = ItemSet.MandatoryPackageScreeningForAirAndUnknownTransportMode;

			var checkProcedureName = "UpdatePackageStateSecurityStatus";
			var mockedCheckProcedure = "\r\nALTER PROC " + checkProcedureName + " (@CompanyBranchPK UNIQUEIDENTIFIER, @SystemLastEditUser VARCHAR(3), @RegistryValue BIT, @WarehouseConfigurationValue BIT, @PackageStatePKs dbo.TVP_uniqueidentifier READONLY) AS\r\nBEGIN\r\n\tDECLARE @result varchar(max) = CONCAT(CONVERT(NVARCHAR(36), @CompanyBranchPK), ',',  @SystemLastEditUser, ',', @RegistryValue, ',', @WarehouseConfigurationValue, ',', (SELECT COUNT(*) FROM @PackageStatePKs))\r\n\tRAISERROR(@result, 18, 1)\r\n\tRETURN 1\r\nEND";

			Db.Connection.ExecuteNonQuery(mockedCheckProcedure);
			var guid = Guid.NewGuid();
			registryItem.OnUpdateAction(Guid.Empty, guid, Guid.Empty, newStatus);
			registryItem.OnAllValuesSaved();

			AssertEquals("Error Reported", guid.ToString().ToUpper() + "," + ((IGlbStaff)Env.CurrentUser).GS_Code + "," + Convert.ToInt32(newStatus) + "," + ",0", ErrorReporter.LastMessageReported.ToUpper());
			ErrorReporter.Clear();
		}

		#endregion

		#region Unload

		#region DefaultTransportationType

		public void TestDefaultTransportationType()
		{
			CodeDescriptionPairList list = ItemSet.DefaultTransportationTypeList;
			TestRegistryItem(
				ItemSet.DefaultTransportationType,
				"DefaultTransportationType",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Default Transportation Type",
				"Set default Transportation Type when starting a new unload on mobile.",
				RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				list,
				"NON");
		}

		#endregion

		#region AutoCreationOfPutawayTransferOnUnloadCompletion

		public void TestAutoCreationOfPutawayTransferOnUnloadCompletion()
		{
			TestRegistryItem(ItemSet.AutoCreationOfPutawayTransferOnUnloadCompletion,
				"AutoCreationOfPutawayTransferOnUnloadCompletion",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Auto-Creation of Putaway Transfer on Unload Completion",
				"When enabled, a putaway transfer will be automatically created when the user does not elect to putaway packages immediately after unload process.\r\nWhen disabled, when the user elects not to putaway packages immediately after unload, no putaway transfer will be created. User will need to create an ad-hoc transfer or putaway by scanning the package to putaway.",
				RegistryStorageFlags.System,
				true);
		}

		#endregion

		#region PromptForASN

		public void TestPromptForASN()
		{
			TestRegistryItem(ItemSet.PromptForASN, "PromptForASN", WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Prompt for ASN", "Select ASN prompt shown after entering or resuming an RTU. If set to NO, the Select ASN prompt is not shown and the unload will be treated as a blind unload i.e., no active consignment and no remaining packages are shown.\r\n\r\nPlease note, this setting is ignored if an ASN is found attached to an RTU.", RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, true);
		}

		#endregion

		#region RTUTransportCompanyIsMandatoryDuringUnload

		public void TestRTUTransportCompanyIsMandatoryDuringUnload()
		{
			TestRegistryItem(
				ItemSet.RTUTransportCompanyIsMandatoryDuringUnload,
				"RTUTransportCompanyIsMandatoryDuringUnload",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"RTU Transport Company is Mandatory for Vehicle during Unload",
				"Receive Transport Unit Transport Company is mandatory during the unload process for Vehicle Load Types.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region DefaultUnloadMode

		public void TestDefaultUnloadMode()
		{
			TestRegistryItem(
				ItemSet.DefaultUnloadMode,
				"DefaultUnloadMode",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Default Unload Mode",
				"Set default Unload Mode when starting a new unload on mobile.\r\n\r\nPlease note, if APM is selected, the mobile user will be prompted between the Full and Rapid Unload modes.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				new WhsUnloadModeCodeList(),
				WhsUnloadModeCodeList.Codes.FullUnload);
		}

		#endregion

		#region DefaultDimsOrVolumeBehaviour

		public void TestDefaultDimsOrVolumeBehaviour()
		{
			TestRegistryItem(
				ItemSet.DefaultDimsOrVolumeBehaviour,
				"DefaultDimsOrVolumeBehaviour",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Dimensions Or Volume Behavior Mode",
				"This item controls whether dimensions and/or volume fields are read-only when unloading packages on the mobile device.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				new DimsOrVolumeBehaviourModesCodeList(),
				DimsOrVolumeBehaviourModesCodeList.Codes.DIMorVOL);
		}

		#endregion

		#region DefaultReceiveConsignmentDirection

		public void TestDefaultReceiveConsignmentDirection()
		{
			TestRegistryItem(
				ItemSet.DefaultReceiveConsignmentDirection,
				"DefaultReceiveConsignmentDirection",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Default Receive Consignment Direction",
				"Set default transport direction when creating a new receive consignment.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				new DefaultConsignmentDirectionCodeList(),
				DefaultConsignmentDirectionCodeList.Codes.Empty);
		}

		#endregion

		#region DefaultDispatchConsignmentDirection

		public void TestDefaultDispatchConsignmentDirection()
		{
			TestRegistryItem(
				ItemSet.DefaultDispatchConsignmentDirection,
				"DefaultDispatchConsignmentDirection",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Load,
				"Default Dispatch Consignment Direction",
				"Set default transport direction when creating a new dispatch consignment.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				new DefaultConsignmentDirectionCodeList(),
				DefaultConsignmentDirectionCodeList.Codes.Empty);
		}

		#endregion

		#region DriverSecurityCertificationCheckingActivated

		public void TestDriverSecurityCertificationCheckingActivated()
		{
			var itemSetDriverSecurityCertificationCheckingActivated = ItemSet.DriverSecurityCertificationCheckingActivated;
			TestRegistryItem(
				itemSetDriverSecurityCertificationCheckingActivated,
				"DriverSecurityCertificationCheckingActivated",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Driver Security Certification Checking Activated",
				"When registry is set to Yes, Driver Security Certification will be checked, and included in the 'Is Secure' calculation in accordance with EU and UK supply chain security regime compliance requirements.\r\nWhen registry is set to No, the 'Is Secure' calculation will exclude checking of Driver Security Certification irrespective of the contact having certification on their contact record.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				true);
		}

		#endregion

		#region DriverSignature

		#region PromptForDriverSignatureAfterUnloadingVehicle

		public void TestPromptForDriverSignatureAfterUnloadingVehicle()
		{
			TestRegistryItem(
				ItemSet.PromptForDriverSignatureAfterUnloadingVehicle,
				"PromptForDriverSignatureAfterUnloadingVehicle",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload_DriverSignature,
				"Prompt for Driver Signature after unloading vehicle",
				"Whether or not RTU unload complete prompts for vehicle signature.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		#endregion

		#region PromptForDriverSignatureAfterUnloadingContainer

		public void TestPromptForDriverSignatureAfterUnloadingContainer()
		{
			TestRegistryItem(
				ItemSet.PromptForDriverSignatureAfterUnloadingContainer,
				"PromptForDriverSignatureAfterUnloadingContainer",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload_DriverSignature,
				"Prompt for Driver Signature after unloading sea container",
				"Whether or not RTU unload complete prompts for sea container signature.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		#endregion

		#region PromptForDriverSignatureAfterUnloadingAirContainer

		public void TestPromptForDriverSignatureAfterUnloadingAirContainer()
		{
			TestRegistryItem(
				ItemSet.PromptForDriverSignatureAfterUnloadingAirContainer,
				"PromptForDriverSignatureAfterUnloadingAirContainer",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload_DriverSignature,
				"Prompt for Driver Signature after unloading air container",
				"Whether or not RTU unload complete prompts for air container signature.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		#endregion

		#endregion

		#region PackageDetailsAreMandatory

		public void TestPackageDetailsAreMandatory()
		{
			TestRegistryItem(
				ItemSet.PackageDetailsAreMandatory,
				"PackageDetailsAreMandatory",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Package Details are Mandatory",
				"Packages require dimensions, volume and weight to be recorded before they are Putaway, Cross-Docked or Loaded.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region TestAllowPackageHeightModificationWhileLocked

		public void TestAllowPackageHeightModificationWhileLocked()
		{
			TestGenericRegistryItem(
				ItemSet.AllowPackageHeightModificationWhileLocked,
				"AllowPackageHeightModificationWhileLocked",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Allow Package Height Modification While Locked",
				"Will allow editing of package height while packages are locked, for example, for pallets with variable height.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region TestActionsTakenToRCNsWhenFinishUnloading

		public void TestActionsTakenToRCNSWhenFinishUnloadingTypeList()
		{
			AssertEquals(3, WarehouseDataRegistry.Instance.ActionsTakenToRCNSWhenFinishUnloadingTypeList.Count);
			AssertEquals("PMT", WarehouseDataRegistry.Instance.ActionsTakenToRCNSWhenFinishUnloadingTypeList.DefaultCode);
			Assert(WarehouseDataRegistry.Instance.ActionsTakenToRCNSWhenFinishUnloadingTypeList.ContainsCode("PMT"));
			Assert(WarehouseDataRegistry.Instance.ActionsTakenToRCNSWhenFinishUnloadingTypeList.ContainsCode("CLS"));
			Assert(WarehouseDataRegistry.Instance.ActionsTakenToRCNSWhenFinishUnloadingTypeList.ContainsCode("OPN"));
		}

		public void TestActionsTakenToRCNSWhenFinishUnloading()
		{
			CodeDescriptionPairList list = ItemSet.ActionsTakenToRCNSWhenFinishUnloadingTypeList;
			TestRegistryItem(
				ItemSet.ActionsTakenToRCNSWhenFinishUnloading,
				"ActionsTakenToRCNSWhenFinishUnloading",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Actions Taken To RCNs When Finish Unloading",
				"What action should be taken when an RTU is Finished Unloading and RCNs unloaded are received short?",
				RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				list,
				"PMT");
		}

		#endregion

		#endregion

		#region Load

		#region DTUTransportCompanyIsMandatoryDuringLoad

		public void TestDTUTransportCompanyIsMandatoryDuringLoad()
		{
			TestRegistryItem(
				ItemSet.DTUTransportCompanyIsMandatoryDuringLoad,
				"DTUTransportCompanyIsMandatoryDuringLoad",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Load,
				"DTU Transport Company is Mandatory for Vehicle during Load",
				"Dispatch Transport Unit Transport Company is mandatory during the load process for Vehicle Load Types.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region DriverSignature

		#region PromptForDriverSignatureAfterUnloadingVehicle

		public void TestPromptForDriverSignatureAfterLoadingVehicle()
		{
			TestRegistryItem(
				ItemSet.PromptForDriverSignatureAfterLoadingVehicle,
				"PromptForDriverSignatureAfterLoadingVehicle",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Load_DriverSignature,
				"Prompt for Driver Signature after loading vehicle",
				"Whether or not DTU load complete prompts for vehicle signature.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		#endregion

		#region PromptForDriverSignatureAfterLoadingContainer

		public void TestPromptForDriverSignatureAfterLoadingContainer()
		{
			TestRegistryItem(
				ItemSet.PromptForDriverSignatureAfterLoadingContainer,
				"PromptForDriverSignatureAfterLoadingContainer",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Load_DriverSignature,
				"Prompt for Driver Signature after loading sea container",
				"Whether or not DTU load complete prompts for sea container signature.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		#endregion

		#region PromptForDriverSignatureAfterLoadingAirContainer

		public void TestPromptForDriverSignatureAfterLoadingAirContainer()
		{
			TestRegistryItem(
				ItemSet.PromptForDriverSignatureAfterLoadingAirContainer,
				"PromptForDriverSignatureAfterLoadingAirContainer",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Load_DriverSignature,
				"Prompt for Driver Signature after loading air container",
				"Whether or not DTU load complete prompts for air container signature.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		#endregion

		#endregion

		#endregion

		#region Data Capture Defaults

		#region CaptureRCNCustomsDetails

		public void TestCaptureRCNCustomsDetails()
		{
			TestRegistryItem(
				ItemSet.CaptureRCNCustomsDetails,
				"CaptureRCNCustomsDetails",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Capture RCN Customs Details",
				"When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Customs Details if not already entered.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region CaptureRCNConsignor

		public void TestCaptureRCNConsignor()
		{
			TestRegistryItem(
				ItemSet.CaptureRCNConsignor,
				"CaptureRCNConsignor",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Capture RCN Consignor",
				"When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Consignor if not already entered.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region CaptureRCNConsignee

		public void TestCaptureRCNConsignee()
		{
			TestRegistryItem(
				ItemSet.CaptureRCNConsignee,
				"CaptureRCNConsignee",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Capture RCN Consignee",
				"When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Consignee if not already entered.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region CaptureRCNBookingParty

		public void TestCaptureRCNBookingParty()
		{
			TestRegistryItem(
				ItemSet.CaptureRCNBookingParty,
				"CaptureRCNBookingParty",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Capture RCN Booking Party",
				"When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Booking Party if not already entered.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region CaptureRCNBillingClient

		public void TestCaptureRCNBillingClient()
		{
			TestRegistryItem(
				ItemSet.CaptureRCNBillingClient,
				"CaptureRCNBillingClient",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Capture RCN Billing Client",
				"When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Billing Client if not already entered.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region DefaultBilltoPartyForTWConsignment

		public void TestDefaultBilltoPartyForTWConsignment()
		{
			TestRegistryItem(
				ItemSet.DefaultBilltoPartyForTWConsignment,
				"DefaultBilltoPartyForTWConsignment",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Default Bill to Party For TW Consignment",
				"Override the default value in the Registry to configure the Organization to be used as the Bill to Party in Transit Warehouse",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				new DefaultBilltoPartyForTWConsignmentCodeList(),
				DefaultBilltoPartyForTWConsignmentCodeList.Codes.Non);
		}

		#endregion

		#region CaptureRCNServiceLevel

		public void TestCaptureRCNServiceLevel()
		{
			TestRegistryItem(
				ItemSet.CaptureRCNServiceLevel,
				"CaptureRCNServiceLevel",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Capture RCN Service Level",
				"When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Service Level if not already entered.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region CaptureRCNNextDischargePort

		public void TestCaptureRCNNextDischargePort()
		{
			TestRegistryItem(
				ItemSet.CaptureRCNNextDischargePort,
				"CaptureRCNNextDischargePort",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Capture RCN Next Discharge Port",
				"When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Next Discharge Port if not already entered.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region CaptureRCNExpectedDispatch

		public void TestCaptureRCNExpectedDispatch()
		{
			TestRegistryItem(
				ItemSet.CaptureRCNExpectedDispatch,
				"CaptureRCNExpectedDispatch",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Capture RCN Expected Dispatch",
				"When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Expected Dispatch if not already entered.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region CapturePackageInformationScan

		public void TestCapturePackageInformationScan()
		{
			TestRegistryItem(
				ItemSet.CapturePackageInformationScan,
				"CapturePackageInformationScan",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Capture Package Information Scan",
				"When unloading a Received Transportation Unit, prompt to scan a barcode containing package information such as weight or dimensions.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#endregion

		#region TestLabelValidationField

		public void TestLabelValidationFieldList()
		{
			AssertEquals(3, WarehouseDataRegistry.Instance.LabelValidationFieldList.Count);
			AssertEquals("", WarehouseDataRegistry.Instance.LabelValidationFieldList.DefaultCode);
			Assert(WarehouseDataRegistry.Instance.LabelValidationFieldList.ContainsCode("HSB"));
			Assert(WarehouseDataRegistry.Instance.LabelValidationFieldList.ContainsCode("MAB"));
			Assert(WarehouseDataRegistry.Instance.LabelValidationFieldList.ContainsCode("FSH"));
		}

		public void TestLabelValidationField()
		{
			TestRegistryItem(ItemSet.LabelValidationField,
				"LabelValidationField",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Default Label Validation Number",
				"Default Label Validation Number you would like to validate against.",
				RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue,
				ItemSet.LabelValidationFieldList,
				"");
		}

		#endregion

		#region TestAdjustOutReasons

		public void TestAdjustOutReasons()
		{
			TestRegistryItemWithDefaultCode(
				ItemSet.AdjustOutReasons,
				"AdjustOutReasons",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Adjust Out Reasons",
				"Adjust Out Reasons",
				RegistryStorageFlags.System,
				3,
				4);
			AssertEquals(true, ItemSet.AdjustOutReasons.DefaultValue.ContainsCode("OTH"));
			AssertEquals("Other", ItemSet.AdjustOutReasons.DefaultValue.GetDescriptionFromCode("OTH"));
			AssertEquals(true, ItemSet.AdjustOutReasons.DefaultValue.ContainsCode("LCC"));
			AssertEquals("Lost in Cycle Count", ItemSet.AdjustOutReasons.DefaultValue.GetDescriptionFromCode("LCC"));
			AssertEquals(true, ItemSet.AdjustOutReasons.DefaultValue.ContainsCode("BDS"));
			AssertEquals("Broken Down via Service", ItemSet.AdjustOutReasons.DefaultValue.GetDescriptionFromCode("BDS"));
			AssertEquals(true, ItemSet.AdjustOutReasons.DefaultValue.ContainsCode("CTP"));
			AssertEquals("Converted to Packline", ItemSet.AdjustOutReasons.DefaultValue.GetDescriptionFromCode("CTP"));
		}

		#endregion

		#region EnablePackageIDMatchingFromPreviousTransitWarehouses

		public void TestEnablePackageIDMatchingFromPreviousTransitWarehouses()
		{
			TestRegistryItem(ItemSet.EnablePackageIDMatchingFromPreviousTransitWarehouses, "EnablePackageIDMatchingFromPreviousTransitWarehouses",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Enable Package ID Matching from Previous Transit Warehouses",
				"When enabled, an package ID scanned during unload that is recognized from a previous transit warehouse will have its details imported to the current warehouse. Handling Unit IDs will have their child packages imported.",
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		#region EnableUNDGValidationWhenRTUGateIn

		public void TestEnableUNDGValidationWhenRTUGateIn()
		{
			TestRegistryItem(ItemSet.EnableUNDGValidationWhenRTUGateIn, "EnableUNDGValidationWhenRTUGateIn",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Enable UNDG Validation when RTU Gate In",
				"When enabled, UNDG validation rule will be used to validate RTU Planned Packages.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region Gate Management

		public void TestGateBookingMessagingeHubID()
		{
			TestRegistryItem(ItemSet.GateBookingMessagingeHubID,
				"GateBookingMessagingeHubID",
				WarehouseDataRegistry.Categories.Warehouse_GateManagement,
				"Gate Booking Messaging eHub ID",
				"Gate Booking Messaging eHub ID",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				TextEditorType.TextBox,
				"");
		}

		public void TestEnableFacilitiesGateWebService()
		{
			TestRegistryItem(ItemSet.EnableFacilitiesGateWebService,
				"EnableFacilitiesGateWebService",
				WarehouseDataRegistry.Categories.Warehouse_GateManagement,
				"Enable Facilities Gate Web Service",
				"Enables Facilities Gate Web Service",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region EnableAutoCreateReceiveConsignmentMatchingFromPreviousTransitWarehouses

		public void TestEnableAutoCreateReceiveConsignmentMatchingFromPreviousTransitWarehouses()
		{
			TestRegistryItem(ItemSet.EnableAutoCreateReceiveConsignmentMatchingFromPreviousTransitWarehouses, "EnableAutoCreateReceiveConsignmentMatchingFromPreviousTransitWarehouses",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Enable Auto Create Receive Consignment",
				"When enabled, Unload Package with ID Matching from Previous Warehouse should create Receive Consignment automatically.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestEnableTransitPutawayRulesEngine

		public void TestEnableTransitPutawayRulesEngine()
		{
			TestRegistryItem(ItemSet.EnableTransitPutawayRulesEngine, "EnableTransitPutawayRulesEngine",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Enable Directed Putaway Rules",
				"Use the Transit Warehouse Directed Putaway Rules Engine in the Putaway processes and enable \"Suggest Another Location\" option on Putaway and Manual Transfers.",
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		#region TestShowErrorWhenDuplicateVehicleInFacility

		public void TestShowErrorWhenDuplicateVehicleInFacility()
		{
			TestRegistryItem(ItemSet.ShowErrorWhenDuplicateVehicleInFacility, "ShowErrorWhenDuplicateVehicleInFacility",
				WarehouseDataRegistry.Categories.Warehouse_GateManagement,
				"Show Error When Duplicate Vehicle in Facility",
				"By default, the system displays a warning when user creates a gate in with a vehicle registration matching a vehicle that is already in the facility. By setting this, the system displays an error instead, and user is not able to save the gate in until this is rectified.",
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		#region TestEnablePalletizePackline

		public void TestEnablePalletizePackline()
		{
			TestRegistryItem(ItemSet.EnablePalletizePackline, "EnablePalletizePackline",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Enable Palletize Packline",
				"Enable Palletize Packline in Mobile Application.",
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		#region TestEnableHandlingUnitSkipScanMode

		public void TestEnableHandlingUnitSkipScanMode()
		{
			TestRegistryItem(ItemSet.EnableHandlingUnitSkipScanMode, "EnableHandlingUnitSkipScanMode",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Enable Handling Unit Skip Scan Mode",
				"Enable Skip Scan Mode for Handling Unit.",
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		#region TestAllowServiceCompletionOnBookedPackages

		public void TestAllowServiceCompletionOnBookedPackages()
		{
			TestRegistryItem(ItemSet.AllowServiceCompletionOnBookedPackages, "AllowServiceCompletionOnBookedPackages",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Allow Service Completion on Booked Packages",
				"When set to Yes, Services can be completed on Booked Packages.\r\nWhen set to No, Services cannot be completed on Booked Packages.",
				RegistryStorageFlags.Branch,
				true);
		}

		#endregion

		#region TestDepartedPackageAutoFinalizationDelay

		public void TestDepartedPackageAutoFinalizationDelay()
		{
			TestRegistryItem(
				ItemSet.DepartedPackageAutoFinalizationDelay,
				"DepartedPackageAutoFinalizationDelay",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Departed Package Auto-Finalization Delay",
				@"Packages that have departed the warehouse are automatically finalized after the specified delay (in hours).

Note: Finalized packages no longer count toward DG limits.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				24,
				0,
				2400);
		}

		#endregion

		#region TestEnableCoLoadAndAssemblyMasterSubConsignmentsCreation

		public void TestEnableCoLoadAndAssemblyMasterSubConsignmentsCreation()
		{
			TestRegistryItem(ItemSet.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation, "EnableCoLoadAndAssemblyMasterSubConsignmentsCreation",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Enable Consignment Creation for Sub-Shipments",
				"Enable Consignment Creation for CoLoads, Blind CoLoads and Assembly Master Sub-Shipments.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		#endregion

		#region Test Require Loading onto Vehicle for Dispatch

		#region TestULDNeedLoadingOntoVehicle

		public void TestULDNeedLoadingOntoVehicle()
		{
			TestRegistryItem(ItemSet.ULDNeedLoadingOntoVehicle, "ULDNeedLoadingOntoVehicle",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Load,
				"ULDs Require Loading onto Vehicle for Dispatch",
				"When Registry Date is in the past, ULDs are required to be loaded onto a Vehicle. However, non-cleared and/or non-authorized packages can be picked and packed/loaded into a ULD.\r\n \r\nWhen Registry Date is Empty or in the future, ULDs can be gated out without loading them onto a Vehicle. However, non-cleared and non-authorized packages can not be picked or packed/loaded into a ULD.",
				ZDateTimePickerFormat.Short,
				RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				DateTime.MinValue);

			AssertType(typeof(DateWithinOneYearRegistryDataType), ItemSet.ULDNeedLoadingOntoVehicle.DataType);
		}

		#endregion

		#region TestCNTNeedLoadingOntoVehicle

		public void TestCNTNeedLoadingOntoVehicle()
		{
			TestRegistryItem(ItemSet.CNTNeedLoadingOntoVehicle, "CNTNeedLoadingOntoVehicle",
						WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Load,
						"Sea Containers Require Loading onto Vehicle for Dispatch",
						"When Registry is Yes, Sea Containers are required to be loaded onto a Vehicle. However, non-cleared and/or non-authorized packages can be picked and packed/loaded into a Sea Container.\r\n \r\nWhen Registry is No, Sea Containers can be gated out without loading them onto a Vehicle. However, non-cleared and non-authorized packages can not be picked or packed/loaded into a Sea Container.",
						RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						false);
		}

		#endregion

		#endregion

		#region TestTransitReferenceMapping

		public void TestTransitReferenceMapping()
		{
			TestGenericRegistryItem(ItemSet.TransitReferenceMapping, "TransitReferenceMapping",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Transit Warehouse Reference Mapping",
				"Please setup the reference mapping for transit warehouse.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch);
		}

		#endregion

		#region Transit Charegable

		#region TransitChargeableFactorForAir

		public void TestTransitChargeableFactorForAir()
		{
			TestGenericRegistryItem(
				ItemSet.TransitChargeableFactorForAir,
				"TransitChargeableFactorForAir",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_ChargeableForTransportationUnit,
				"Chargeable Factor for Air",
				@"The chargeable factor will be used to convert the actual volume into a chargeable weight.

The chargeable weight is the greater of the actual weight or actual volume divided by the chargeable factor.

Max of [ weight(KG) , volume(CM3) / factor ] = KG

Default chargeable factor is 6000 CC/KG.

Max of [ weight(LB) , volume(CI) / factor ] = LB

Default chargeable factor is 194 CI/LB.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic));
		}

		#endregion

		#region TestTransitChargeableFactorForSea

		public void TestTransitChargeableFactorForSea()
		{
			TestGenericRegistryItem(
				ItemSet.TransitChargeableFactorForSea,
				"TransitChargeableFactorForSea",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_ChargeableForTransportationUnit,
				"Chargeable Factor for Sea",
				@"The chargeable factor will be used to convert the actual volume into a chargeable weight.

The chargeable weight is the greater of the actual weight or actual volume divided by the chargeable factor.

Max of [ weight(KG) , volume(CM3) / factor ] = KG

Default chargeable factor is 1000 KG/M3.

Max of [ weight(LB) , volume(CI) / factor ] = LB

Default chargeable factor is 194 CI/LB.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Sea, ConversionFactor.Standard.Imperial.Domestic));
		}

		#endregion

		#region TestTransitChargeableFactorForRoad

		public void TestTransitChargeableFactorForRoad()
		{
			TestGenericRegistryItem(
				ItemSet.TransitChargeableFactorForRoad,
				"TransitChargeableFactorForRoad",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_ChargeableForTransportationUnit,
				"Chargeable Factor for Road",
				@"The chargeable factor will be used to convert the actual volume into a chargeable weight.

The chargeable weight is the greater of the actual weight or actual volume divided by the chargeable factor.

Max of [ weight(KG) , volume(CM3) / factor ] = KG

Default chargeable factor is 333 KG/M3.

Max of [ weight(LB) , volume(CI) / factor ] = LB

Default chargeable factor is 194 CI/LB.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				new ChargeableFactor(ConversionFactor.Standard.Metric.Road, ConversionFactor.Standard.Imperial.Domestic));
		}

		#endregion

		#endregion

		#region TestUnloadCompleteDateForRatingOfStorage

		public void TestUnloadCompleteDateForRatingOfStorage()
		{
			TestRegistryItem(
				ItemSet.UnloadCompleteDateForRatingOfStorage,
				"UnloadCompleteDateForRatingOfStorage",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Unload Complete Date for Rating of Storage",
				"Select the date autorating uses to calculate storage for packages.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				new UnloadCompleteDateForRatingOfStorageList(),
				UnloadCompleteDateForRatingOfStorageList.Codes.EachUnloadCompleteDate);
		}

		#endregion

		#region TestDefaultActiveRCNDuringUnloading

		public void TestDefaultActiveRCNDuringUnloading()
		{
			TestRegistryItem(ItemSet.DefaultActiveRCNDuringUnloading, "DefaultActiveRCNDuringUnloading",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Unload,
				"Default Active RCN When Multiple Expected",
				"When an ASN is selected to unload, if there are multiple RCNs expected on the ASN, default the Active Consignment to the First RCN found on the ASN.\r\n\r\nNote: If the ASN is only expecting one RCN, it will always default as the Active Consignment, regardless of the option chosen below.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				true);
		}

		#endregion

		#region TestCreateSingleASNForAllContainers

		public void TestCreateSingleASNForAllContainers()
		{
			TestRegistryItem(ItemSet.CreateSingleASNForAllContainers,
				"CreateSingleASNForAllContainers",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Create Single ASN for All Containers",
				@"Always create an ASN per Forwarding Consol (via UXML), regardless of planned packing of Container.

Set to True to always create a single ASN on UXML read.
Set to False to create an ASN per Container when the TWH is an Import/Arrival TWH and the UXML indicates what packages to pack in each Container.

Please note, this registry does not control ASN creation for Export/Departure TWH.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region TestCreateSingleDLLForAllContainers

		public void TestCreateSingleDLLForAllContainers()
		{
			TestRegistryItem(ItemSet.CreateSingleDLLForAllContainers, "CreateSingleDLLForAllContainers",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Create Single DLL For All Containers",
				@"Always create a Load List per Forwarding Consol (via UXML), regardless of planned packing of Container.

Set to True to always create a single Load List on UXML read.
Set to False to create a Load List per Container when the TWH is an Export/Departure TWH the UXML indicates what packages to pack in each Container.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region EnableTransitWarehouseDashboard

		public void TestEnableTransitWarehouseDashboard()
		{
			TestRegistryItem(ItemSet.EnableTransitWarehouseDashboard, "EnableTransitWarehouseDashboard",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Enable Dashboard",
				"Enable Dashboard - DO NOT USE THIS ON PERSISTED TESTING SYSTEMS LIKE UAT ALPHA.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region EnableCycleCountLastPackageScanCancellation

		public void TestEnableCycleCountLastPackageScanCancellation()
		{
			TestRegistryItem(ItemSet.EnableCycleCountLastPackageScanCancellation, "EnableCycleCountLastPackageScanCancellation",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Enable Cycle Count Last Package Scan Cancellation",
				"When enabled, an unexpected package scan can be canceled during a cycle count.",
				RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#endregion

		#region PackageStateAdditionalReferenceType

		public void TestPackageAdditionalReferenceType()
		{
			TestRegistryItemWithDefaultCode(ItemSet.PackageAdditionalReferenceType, "PackageAdditionalReferenceType", WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse, "Package Additional Reference Types", "The system defines default Additional Reference Types. Using this registry, you can define Package Additional Reference Types.", RegistryStorageFlags.System, 3, 4);

			var defaultValue = ItemSet.PackageAdditionalReferenceType.DefaultValue;
			AssertEquals("DefaultValue.Count", 4, defaultValue.Count);
			AssertEquals(TransitWarehousePackageAdditionalReferenceTypes.Descriptions.InternetOfThings, defaultValue.GetDescriptionFromCode(TransitWarehousePackageAdditionalReferenceTypes.Codes.InternetOfThings));
			AssertEquals(TransitWarehousePackageAdditionalReferenceTypes.Descriptions.ConsignmentOrDeliveryNoteNumber, defaultValue.GetDescriptionFromCode(TransitWarehousePackageAdditionalReferenceTypes.Codes.ConsignmentOrDeliveryNoteNumber));
			AssertEquals(TransitWarehousePackageAdditionalReferenceTypes.Descriptions.ShipmentNumber, defaultValue.GetDescriptionFromCode(TransitWarehousePackageAdditionalReferenceTypes.Codes.ShipmentNumber));
			AssertEquals(TransitWarehousePackageAdditionalReferenceTypes.Descriptions.CargoTrackingNumber, defaultValue.GetDescriptionFromCode(TransitWarehousePackageAdditionalReferenceTypes.Codes.CargoTrackingNumber));
			AssertEquals(string.Empty, ItemSet.PackageAdditionalReferenceType.DefaultValue.DefaultCode);
		}

		#endregion

		#region Packing Slip Title

		public void TestPackingSlipTitles()
		{
			TestRegistryItem(
				ItemSet.PackingSlipTitles,
				"PackingSlipTitles",
				WarehouseDataRegistry.Categories.Warehouse_PackingSlipTitle,
				"Packing Slip Title",
				"The title which will appear on the Packing Slip.",
				RegistryStorageFlags.Branch,
				TextEditorType.TextBox,
				RegistryOptions.Default,
				"Packing Slip");
		}

		#endregion

		#region Picking

		#region TestPriorityIncrementForShortPick

		public void TestPriorityIncrementForShortPick()
		{
			TestRegistryItem(ItemSet.ShortPickingPriorityIncrement, "ShortPickingPriorityIncrement", WarehouseDataRegistry.Categories.Warehouse_Picking,
				"Back Order Priority Increase.", "This item allows the Priority that is assigned to Back Orders to be increased by taking the Priority from the original order, increasing that by a given value and assigning that higher Priority to the Back Order. Default value is increment by 1..", RegistryStorageFlags.System, 1);
		}

		#endregion

		#region TestPickGroups

		public void TestPickGroups()
		{
			TestGenericRegistryItem(
				ItemSet.PickGroups,
				"PickGroups",
				WarehouseDataRegistry.Categories.Warehouse_Picking,
				"Pick Groups",
				"The Pick Group enables automatic or manual grouping of Products for directed picking based on Product characteristics (for example Weight). " +
				"Such characteristics may impact the sequence in which products need to be picked on an Order.",
				RegistryStorageFlags.System);
		}

		#endregion

		#region TestPickMethod

		public void TestPickMethod()
		{
			TestRegistryItemWithDefaultCode(ItemSet.PickMethod, "PickMethod", WarehouseDataRegistry.Categories.Warehouse_Picking, "Pick Methods", "Pick Methods are used to control the use of different equipment by location. For example you may have extra high locations that require a special hoist to access. If these Pick Methods are assigned to locations, when Putaway Sheets or Picking Slips or Stocktake Sheets are printed, they will have a page break by Pick Method so that different store-men using different equipment can work on the same job at the same time.", RegistryStorageFlags.System, 3, 1);
			AssertEquals(true, ItemSet.PickMethod.Value.ContainsCode("ANY"));
		}

		#endregion

		#region TestPickingSequence

		public void TestPickingSequence()
		{
			TestGenericRegistryItem(ItemSet.PickingSequence, "WAREHOUSE_PICKING_SEQUENCE_READONLY",
			WarehouseDataRegistry.Categories.Warehouse_Picking,
			"Picking Sequence",
			@"Pick Sequences define the order of execution for Pick Algorithms. System wide Pick Sequences are defined here. These can then be overridden per organization.

***Legacy Allocation Rules are being replaced with the Production Rules Engine. Allocation rules can be setup in the GLOW PRE portal, hyperlinks have been added in the warehouse configuration for organization.***",
			RegistryStorageFlags.System,
			RegistryOptions.PreserveTestValue | RegistryOptions.IsReadOnly);
		}

		#endregion

		#region TestCartonGroupSequence

		public void TestCartonGroupSequence()
		{
			TestGenericRegistryItem(ItemSet.CartonGroupSequence, "CartonGroupSequence", WarehouseDataRegistry.Categories.Warehouse_Picking,
				"Carton Group Sequence", "Carton Group Sequence defines the fallback sequence for the Carton Group used in the Cartonization process during Allocate Package Labels.",
				RegistryStorageFlags.Branch);
		}

		#endregion

		#region TestPickByBiggestType

		public void TestPickByBiggestType()
		{
			TestRegistryItem(ItemSet.PickByBiggestType, "PickByBiggestType",
					WarehouseDataRegistry.Categories.Warehouse_Picking,
					"Pick by Biggest Pack Type",
					"When this registry setting is enabled, both the Pick Slip Document and RF Picking will let the user pick by Pack Type (as opposed to only units). The largest possible pack types are used, based on the Product Unit Conversions.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, true);
		}

		#endregion

		#region TestPickFaceReplenishmentSingleLineLimiter

		public void TestPickFaceReplenishmentSingleLineLimiter()
		{
			TestRegistryItem(ItemSet.SingleLineAutoPickFaceReplenishmentTransfers, "SingleLineAutoPickFaceReplenishmentTransfers",
				WarehouseDataRegistry.Categories.Warehouse_Transfers,
				"Single-Line Fixed Pick Face Replenishment Transfers",
				"When this registry setting is enabled, the system will limit auto-replenishments for fixed pick faces to a single line per transfer.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
		}

		#endregion

		#region TestWaveCreationRulesFailureNotificationGroup

		public void TestWaveCreationRulesFailureNotificationGroup()
		{
			TestGenericRegistryItem(ItemSet.WaveCreationRulesFailureNotificationGroup,
				"WaveCreationRulesFailureNotificationGroup",
				WarehouseDataRegistry.Categories.Warehouse_Picking,
				"Wave Creation Rules Failure Notification Group",
				"This group will be sent an email notification each time Wave Creation Rules are deactivated due to failures.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.NotCached | RegistryOptions.IsValueOptional,
				Core.Constants.Groups.PostMastersGroupPK);

			var newGroupPK = new Guid();
			ItemSet.WaveCreationRulesFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGroupPK);
			AssertEquals("WaveCreationRulesFailureNotificationGroup.Value", newGroupPK, ItemSet.WaveCreationRulesFailureNotificationGroup.Value);
		}

		#endregion

		#region TestDeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks

		public void TestDeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks()
		{
			TestRegistryItem(ItemSet.DeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks, "DeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks",
				WarehouseDataRegistry.Categories.Warehouse_Picking,
				"Deallocate Lower Priority Picks When Allocating Waiting Replenishment Picks",
				"When this registry setting is enabled, the system will deallocate lower priority picks first before allocating picks that are waiting replenishment as a lower priority pick might be withholding stock from a higher priority pick.",
				RegistryStorageFlags.System, false);
		}

		#endregion

		#region TestGroupOrderedInventoryByCustomAttributesRegistry

		public void TestGroupOrderedInventoryByCustomAttributesRegistry()
		{
			TestRegistryItem(
				ItemSet.GroupOrderedInventoryByCustomAttributes,
				"GroupOrderedInventoryByCustomAttributes",
				WarehouseDataRegistry.Categories.Warehouse_Orders,
				"Group Ordered Inventory By Custom Attributes",
				"Enables displaying and grouping Warehouse Pick Ordered Inventory By Custom Attributes.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				expectedDefaultValue: false);
		}

		#endregion

		#endregion

		#region TestEnablePickLineLoaderBuildingStatusFilter

		public void TestEnablePickLineLoaderBuildingStatusFilter()
		{
			TestRegistryItem(ItemSet.EnablePickLineLoaderBuildingStatusFilter, "EnablePickLineLoaderBuildingStatusFilter",
				WarehouseDataRegistry.Categories.Warehouse_Picking,
				"Enables RF Get Next Pick and Tote Picking To Filter Out Picks With Building Status",
				"A Pick is created with a Building Status when at least one Order on the pick has their fulfillment rules not met.",
				RegistryStorageFlags.System,
				expectedDefaultValue: true);
		}

		#endregion

		#region InventoryAccuracyManagement

		#region TestEnableCycleCountingAutomationFailureNotificationGroup

		public void TestCycleCountingAutomationFailureNotificationGroup()
		{
			TestGenericRegistryItem(ItemSet.CycleCountingAutomationFailureNotificationGroup,
				"CycleCountingAutomationFailureNotificationGroup",
				WarehouseDataRegistry.Categories.Warehouse_InventoryAccuracyManagement,
				"Cycle Count Task Creation Failure Notification Group",
				"This group will be sent Cycle Count Task Creation failure notifications.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.NotCached | RegistryOptions.IsValueOptional,
				Core.Constants.Groups.PostMastersGroupPK);

			var newGroupPK = new Guid();
			ItemSet.CycleCountingAutomationFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGroupPK);
			AssertEquals("CycleCountingAutomationFailureNotificationGroup.Value", newGroupPK, ItemSet.CycleCountingAutomationFailureNotificationGroup.Value);
		}

		#endregion

		#region TestInventoryAccuracyManagementMethod

		public void TestInventoryAccuracyManagementMethod()
		{
			TestRegistryItem(
				ItemSet.InventoryAccuracyManagementMethod,
				"InventoryAccuracyManagementMethod",
				WarehouseDataRegistry.Categories.Warehouse_InventoryAccuracyManagement,
				"Inventory Accuracy Management Method",
				"The Inventory Accuracy Management Method determines what process the system will use to check inventory accuracy.",
				RegistryStorageFlags.System,
				ItemSet.InventoryAccuracyManagementMethodList,
				"WCC");
		}

		public void TestInventoryAccuracyManagementMethod_IsUsingLegacyStocktake()
		{
			AssertEquals("WCC", ItemSet.InventoryAccuracyManagementMethod.Value);
			AssertEquals(false, ItemSet.IsUsingLegacyStocktake);
			using (ItemSet.InventoryAccuracyManagementMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "WST"))
			{
				Assert(ItemSet.IsUsingLegacyStocktake);
			}
		}

		#endregion

		#region StocktakeCycle

		public void TestStocktakeCycle()
		{
			TestGenericRegistryItem(ItemSet.StocktakeCycle, "WarehouseStocktakeCycle", WarehouseDataRegistry.Categories.Warehouse_InventoryAccuracyManagement_LegacyStocktake,
				"Stocktake Cycle",
				"Stocktake Cycles can be used to organize cyclic stocktakes. When a stocktake/cycle count is created, one of these stocktake cycles can be selected to determine which stock is to be included in the stocktake cycle count. Example cycle codes may be days of the week / month, or types of product etc.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue);
			AssertEquals(0, ItemSet.StocktakeCycle.Value.Count);
		}

		#endregion

		#region StocktakeType

		public void TestStocktakeTypes()
		{
			TestRegistryItemWithDefaultCode
			(
				ItemSet.StocktakeTypes,
				"WarehouseStocktakeType",
				WarehouseDataRegistry.Categories.Warehouse_InventoryAccuracyManagement_LegacyStocktake,
				"Stocktake Type",
				"Stocktake Types be used to define customized stocktakes. The Stocktake Type on new Stocktakes are defaulted to the Stocktake Type marked below as 'Default'. Note: ATC and AZC can not be manually entered by a user, so can not be chosen as defaults.",
				RegistryStorageFlags.System, 3, 3
			);

			AssertCodeAndDefaultColumn((CodeDescriptionBoolDefaultReadonly)ItemSet.StocktakeTypes.Value[0], StocktakeTypeCodeList.Codes.Standard, false);
			AssertCodeAndDefaultColumn((CodeDescriptionBoolDefaultReadonly)ItemSet.StocktakeTypes.Value[1], StocktakeTypeCodeList.Codes.AutomaticTouchCount, true);
			AssertCodeAndDefaultColumn((CodeDescriptionBoolDefaultReadonly)ItemSet.StocktakeTypes.Value[2], StocktakeTypeCodeList.Codes.AutomaticZeroConfirmation, true);
		}

		void AssertCodeAndDefaultColumn(CodeDescriptionBoolDefaultReadonly codeDescriptionBool, string code, bool defaultColumnReadonly)
		{
			AssertEquals(code, codeDescriptionBool.Code);
			AssertEquals(defaultColumnReadonly, codeDescriptionBool.DefaultColumnReadOnly);
		}

		#endregion

		#endregion

		#region AdditionalReferenceType

		public void TestDocketReferenceType()
		{
			AssertEquals(WarehouseDocketReferenceTypeRegistry.RegistryItemKey, ItemSet.AdditionalReferenceType.Name);
			AssertEquals("Additional Reference Types", ItemSet.AdditionalReferenceType.Caption);
			AssertEquals("The system defines default Additional Reference Types. Using this registry, you can define custom Reference Types.", ItemSet.AdditionalReferenceType.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.AdditionalReferenceType.Storage);
			AssertEquals(typeof(CodeDescriptionPairListWithDefaultCodeRegistryDataType), ItemSet.AdditionalReferenceType.DataType.GetType());
			AssertEquals(typeof(CodeDescriptionBoolRegistryEditorInfo), ItemSet.AdditionalReferenceType.EditorInfo.GetType());
			AssertContainsExactElementsInAnyOrder(new[] { RawDataRegistry.Categories.Warehouse_General, RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport }, ItemSet.AdditionalReferenceType.Categories);

			var editorInfo = (CodeDescriptionBoolRegistryEditorInfo)ItemSet.AdditionalReferenceType.EditorInfo;
			AssertEquals("Default column should be invisible", false, editorInfo.IsBoolColumnVisible);
			AssertEquals("Default Column", "Default", editorInfo.BoolColumnCaption);

			var warehouseDocketReferenceTypesFromDefinition = WarehouseDocketReferenceTypeRegistry.GetWarehouseAdditionalReferenceTypeKeyValuePairs();
			var registryItemDefaultList = ((CodeDescriptionPairListWithDefaultCodeRegistryDataType)ItemSet.AdditionalReferenceType.DataType).SystemDefinedList;
			var registryItemDefaultListKeyValuePairs = new List<KeyValuePair<string, string>>();
			foreach (CodeDescriptionPair item in registryItemDefaultList)
			{
				registryItemDefaultListKeyValuePairs.Add(new KeyValuePair<string, string>(item.Code, item.Description));
			}

			CombineAssertions("Default additional reference types should be kept In-Sync with CargoWise Definitions and Glow.", () =>
			{
				AssertEquals(warehouseDocketReferenceTypesFromDefinition.Count(), registryItemDefaultList.Count);
				AssertNotNull(registryItemDefaultList);
				AssertContainsExactElementsInAnyOrder("All Additional Reference Codes and Descriptions Should Match.", warehouseDocketReferenceTypesFromDefinition, registryItemDefaultListKeyValuePairs);
			});
		}

		#endregion

		#region Job Import

		#region Warehouse Job Notification Staff Roles

		public void TestWarehouseJobNotificationStaffRoles()
		{
			var newRoles = new CodeDescriptionBoolDisallowNewCollection();
			CodeDescriptionBool role = newRoles.AddNew();
			role.Code = "TST";
			role.Bool = true;

			ItemSet.WarehouseJobNotificationStaffRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRoles);

			AssertNotNull("WarehouseJobNotificationStaffRoles.Value should not be null", ItemSet.WarehouseJobNotificationStaffRoles.Value);
			AssertEquals("Should be one element", 1, ItemSet.WarehouseJobNotificationStaffRoles.Value.Count);
			AssertEquals("The element's Code should be TST", "TST", ItemSet.WarehouseJobNotificationStaffRoles.Value[0].Code);
			AssertEquals("The element's Bool should be True", true, ItemSet.WarehouseJobNotificationStaffRoles.Value[0].Bool);
		}

		public void TestWarehouseJobNotificationStaffRolesDefaultValue()
		{
			AssertNotNull("WarehouseJobNotificationStaffRoles.Value should not be null", ItemSet.WarehouseJobNotificationStaffRoles.Value);
			StaffRolesNotificationTestHelper.AssertDefaultRoles(ItemSet.WarehouseJobNotificationStaffRoles.Value);

			StaffRolesNotificationTestHelper.AssertBoolValue(ItemSet.WarehouseJobNotificationStaffRoles.Value, true, true, StaffAssignmentRoles.Codes.CartageCoordinator);
			StaffRolesNotificationTestHelper.AssertBoolValue(ItemSet.WarehouseJobNotificationStaffRoles.Value, false, false, StaffAssignmentRoles.Codes.CartageCoordinator);
		}

		#endregion

		#endregion

		#region SOHLocationWarning

		public void TestSOHLocationWarning()
		{
			TestRegistryItem(ItemSet.SOHLocationWarning, "SOHLocationWarning", WarehouseDataRegistry.Categories.Warehouse_Receive, "Stock On Hand Location Warning", "Warn operator if Stock On Hand already exists in the location in which they are attempting to place the product.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, false);
		}

		#endregion

		#region TotalUnitsValidation

		public void TestTotalUnitsValidation()
		{
			TestRegistryItem(ItemSet.TotalUnitsValidation,
				"TotalUnitsValidation",
				WarehouseDataRegistry.Categories.Warehouse_Receive,
				"Total Units Validation",
				"Enabling this setting will cause a receipt to display an error when the receipt is finalized if the Total Units field is not equal to the sum of all the line quantities. The same validation applies to work orders if this setting is enabled as work orders creates a receipt on finalization.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region TestTotalPalletsValidation

		public void TestTotalPalletsValidation()
		{
			TestRegistryItem(ItemSet.TotalPalletsValidation,
				"TotalPalletsValidation",
				WarehouseDataRegistry.Categories.Warehouse_Receive,
				"Total Pallets Validation",
				"Enabling this setting will display an error when the total number of received Pallets is not equal to the expected number of Pallets in the Total Pallets field.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region TestReceiveCategories

		public void TestReceiveCategories()
		{
			const string receiveCategories = nameof(ItemSet.ReceiveCategories);
			TestGenericRegistryItem(ItemSet.ReceiveCategories, receiveCategories,
				WarehouseDataRegistry.Categories.Warehouse_Receive,
				"Receive Categories",
				"A list of Warehouse Receive Categories",
				RegistryStorageFlags.System,
				RegistryOptions.Default);
		}

		#endregion

		#region TestBreakSystemDefinedPickSlipByArea

		public void TestBreakSystemDefinedPickSlipByArea()
		{
			TestRegistryItem(
				ItemSet.BreakSystemDefinedPickSlipByArea,
				"BreakSystemDefinedPickSlipByArea",
				WarehouseDataRegistry.Categories.Warehouse_Picking,
				"Break Pick Slip by Area",
				"When enabled, Picking Slip Documents will show Pick Area and Group by Area.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region TestUnsupportedPackTypeError

		public void TestPackTypesValidation()
		{
			TestRegistryItem(
				ItemSet.PackTypesValidation,
				"PackTypesValidation",
				WarehouseDataRegistry.Categories.Warehouse_General,
				"Pack Types Validation",
				"Enabling this setting will display an error when the entered Pack Type is not defined for the Product.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region TestEnableWarehouseRFVolcam

		public void TestEnableWarehouseRFVolcam()
		{
			TestRegistryItem(ItemSet.EnableRFVolcam, "EnableRFVolcam",
				WarehouseDataRegistry.Categories.Warehouse_VolCam,
				"Enable Warehouse RF Vol-Cam",
				"Enable Vol-Cam for Warehouse RF devices and the related functionality.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: false);
		}

		#endregion

		#region TestValidateConcurrentRFLogins

		public void TestValidateConcurrentRFLogins()
		{
			TestRegistryItem(ItemSet.ValidateConcurrentRFLogins, "ValidateConcurrentRFLogins",
				WarehouseDataRegistry.Categories.Warehouse_General,
				"Validate Concurrent RF Logins",
				"Enable concurrent login checking on the Warehouse RF web service.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: true);
		}

		#endregion

		#region TestCheckAndroidDeviceVersion

		public void TestCheckAndroidDeviceVersion()
		{
			TestRegistryItem(ItemSet.CheckAndroidDeviceVersion, "CheckAndroidDeviceVersion",
				WarehouseDataRegistry.Categories.Warehouse_General,
				"Validate Android Device Version",
				"This registry controls whether there is a need to validate the Warehouse RF Android device version in the warehouse webservice or not. By default, this registry will be set to true. THIS VALUE SHOULD ONLY BE OVERRIDDEN IN A TEST ENVIRONMENT.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				expectedDefaultValue: true);
		}

		#endregion

		#region TestWinCEDeprecationDate

		public void TestWinCEDeprecationDate()
		{
			TestRegistryItem(ItemSet.WinCEApplicationUsageEnabledUntil, nameof(ItemSet.WinCEApplicationUsageEnabledUntil),
				WarehouseDataRegistry.Categories.Warehouse_General,
				"WinCE Application Usage Enabled Until",
				"This registry controls the deprecation date for WinCE devices to be used. This value should only be overriden in exceptional circumstances.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: new DateTime(2026, 01, 31, 00, 00, 00, DateTimeKind.Utc));
		}

		#endregion

		#region TestVirtualWarehouseOnRFEnabledUntil

		public void TestVirtualWarehouseOnRFEnabledUntil()
		{
			TestRegistryItem(
				ItemSet.VirtualWarehouseOnRFEnabledUntil,
				"VirtualWarehouseOnRFEnabledUntil",
				WarehouseDataRegistry.Categories.Warehouse_General,
				"Virtual Warehouse On RF Enabled Until",
				"The UTC date that we support Virtual Warehouses on RF until.",
				ZDateTimePickerFormat.Short,
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				DateTime.MinValue);

			AssertType(typeof(DateWithinOneYearRegistryDataType), ItemSet.VirtualWarehouseOnRFEnabledUntil.DataType);
		}

		#endregion

		#region TestValidateDPSRestrictionsOnFinalisation

		public void TestValidateDPSRestrictionsOnFinalisation()
		{
			TestRegistryItem(ItemSet.ValidateDPSRestrictionsOnFinalisation, "ValidateDPSRestrictionsOnFinalisation",
				WarehouseDataRegistry.Categories.Warehouse_General,
				"Check the Denied Party Screening (DPS) Status on Finalization",
				"Enable DPS-specific restrictions on finalizing Warehouse Orders and Receipts.",
				RegistryStorageFlags.System,
				expectedDefaultValue: true);
		}

		#endregion

		#region AutoFinalizePick

		public void TestAutoFinalizePick()
		{
			TestRegistryItem(ItemSet.AutoFinalizePick, "AutoFinalizePick", WarehouseDataRegistry.Categories.Warehouse_Release, "Auto Finalize Pick", "The system will automatically finalize the Pick when all the Orders are finalized.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
		}

		#endregion

		#region AutoFinalizeOrdrers

		public void TestAutoFinalizeOrders()
		{
			TestRegistryItem(ItemSet.AutoFinalizeOrders, "AutoFinalizeOrders", WarehouseDataRegistry.Categories.Warehouse_Release, "Auto Finalize Orders", "Automatically finalize orders during pick finalization.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
		}

		#endregion

		#region DisablePrintingLabelsOnAutoPack

		public void TestDisablePrintingLabelsOnAutoPack()
		{
			TestRegistryItem(ItemSet.DisablePrintingLabelsOnAutoPack,
				"DisablePrintingLabelsOnAutoPack",
				WarehouseDataRegistry.Categories.Warehouse_Release,
				"Disable Printing Labels on Auto-Pack",
				"When ticked, Labels will no longer print after Auto-Pack operational action is used for Warehouse Orders.",
				RegistryStorageFlags.System,
				expectedDefaultValue: false);
		}

		#endregion

		#region TestTransportCoDefaultingRules

		public void TestTransportCoDefaultingRules()
		{
			TestGenericRegistryItem(ItemSet.TransportCoDefaultingRules, "TransportCoDefaultingRules", WarehouseDataRegistry.Categories.Warehouse_Release,
				"Transport Co Defaulting Rules", "Transport Co Defaulting Rules define the order of precedence when searching for a Default Transport Company.",
				 RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
		}

		#endregion

		#region WarnFinalizePick

		public void TestWarnFinalizePick()
		{
			TestRegistryItem(ItemSet.WarnFinalizePick, "WarnFinalizePick", WarehouseDataRegistry.Categories.Warehouse_Release, "Finalize Pick Warning", "If all Orders are finalized but the Pick is not finalized, the system will warn you when the Release screen is closed.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
		}

		#endregion

		#region TestAllowOrderToBeFinalisedWithoutCustomsClearance

		public void TestAllowOrderToBeFinalisedWithoutCustomsClearance()
		{
			TestRegistryItem(
				ItemSet.AllowOrderToBeFinalisedWithoutCustomsClearance,
				"AllowOrderToBeFinalisedWithoutCustomsClearance",
				WarehouseDataRegistry.Categories.Warehouse_Release,
				"Allow Orders To Be Finalized Without Customs Clearance",
				"Allow Warehouse Orders to be released even if the Order is waiting on clearance from Customs.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestSupervisorOverrideForPartialOrderLoading

		public void TestSupervisorOverrideForPartialOrderLoading()
		{
			TestRegistryItem(
				ItemSet.SupervisorOverrideForPartialOrderLoading,
				"SupervisorOverrideForPartialOrderLoading",
				WarehouseDataRegistry.Categories.Warehouse_Release,
				"Supervisor Override For Partial Order Loading",
				"The supervisor override code to allow shipping partially loaded orders on RF devices. The override code is \"allow\" by default.",
				RegistryStorageFlags.Branch,
				TextEditorType.Password,
				RegistryOptions.IsValueMandatory,
				"allow");
		}

		#endregion

		#region TestAddPalletWeightToOrder

		public void TestAddPalletWeightToOrder()
		{
			TestRegistryItem(ItemSet.AddPalletWeightToOrder,
				"AddPalletWeightToOrder",
				WarehouseDataRegistry.Categories.Warehouse_Release,
				"Add Pallet Weight to Order when not using Packing",
				"If the Order is not using Packing (Packages on the Packing Tab), add the weight of the total number of pallets specified on the Order Release to the total weight of the order by default (defaulted setting can be overridden on release). Otherwise this registry has no effect.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region PackingSlipOrderBy

		public void TestPackingSlipOrderBy()
		{
			CodeDescriptionPairList list = (CodeDescriptionPairList)ObjectFactory.Get<IMasterFilesListProvider>().PackingSlipOrderByList();
			TestRegistryItem(ItemSet.PackingSlipOrderBy, "PackingSlipOrderBy", WarehouseDataRegistry.Categories.Warehouse_Release, "Packing Slip Order By", "Specify the default Sort Order By for Lines on Order Copy/Packing Slip Documents.", RegistryStorageFlags.System | RegistryStorageFlags.Company, list, "PRC");
		}

		#endregion

		#region EnforceUniqueSerialNumbers

		public void TestSerialUniquenessTypeList()
		{
			AssertEquals(2, WarehouseDataRegistry.Instance.SerialUniquenessTypeList.Count);
			AssertEquals("PRO", WarehouseDataRegistry.Instance.SerialUniquenessTypeList.DefaultCode);
			Assert(WarehouseDataRegistry.Instance.SerialUniquenessTypeList.ContainsCode("PRO"));
			Assert(WarehouseDataRegistry.Instance.SerialUniquenessTypeList.ContainsCode("CLI"));
		}

		public void TestEnforceUniqueSerialNumbers()
		{
			TestRegistryItem(ItemSet.EnforceSerialUniqueness, "EnforceUniqueSerialNumbers", WarehouseDataRegistry.Categories.Warehouse_General, "Enforce Unique Serial Numbers", "By default the system will ensure serial numbers are unique by product. This means the same product cannot have more than one unit of stock for the same serial number, but different products can share the same serial number. Using this registry, you can change this default to ensure serial numbers are unique by client, which means the same client cannot have more than one unit of stock for the same serial number, regardless of the product.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, ItemSet.SerialUniquenessTypeList, "PRO");
		}

		#endregion

		#region InvoiceDetailGroupBy

		public void TestInvoiceDetailGroupBy()
		{
			Assert("incomplete test", true);
			//TestRegistryItem(ItemSet.SOHLocationWarning, "InvoiceDetailGroupBy", "Warehouse/Invoicing", "Invoice Detail Group By's", "Specify the grouping for the Invoice Detail report. You can specify upto 3 nested groups. Leave as blank to specify the group is not used. Each group will have a sub-total on the report.", RegistryStorageFlags.System, null);
		}

		#endregion

		#region CrossDockPendingOrdersBufferInDays

		public void TestCrossDockPendingOrdersBufferInDays()
		{
			TestRegistryItem(
				ItemSet.CrossDockPendingOrdersBufferInDays,
				"CrossDockPendingOrdersBufferInDays",
				WarehouseDataRegistry.Categories.Warehouse_Receive,
				"Cross-Dock Pending Orders Buffer in Days",
				"Specifies the minimum number of days to ignore orders for the cross docket pending order check. E.g. If you enter a value of 7, then all pending orders that are required within one week will be included in the warning, and all orders required more than a week in the future will be ignored.\r\n\r\nEnter a value of zero to disable this feature.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				7);
		}

		#endregion

		#region MaxSplitCountForPalletizeLines

		public void TestMaxSplitCountForPalletizeLines()
		{
			TestRegistryItem(
				ItemSet.MaxSplitCountForPalletizeLines,
				"MaxSplitCountForPalletizeLines",
				WarehouseDataRegistry.Categories.Warehouse_Receive,
				"Max Split Count For Palletize Lines",
				"Specifies the maximum number of lines that can be created from a single Palletize Lines action. Creating a very large number of pallets at once is likely indicative of a data entry or setup issue and may perform poorly.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				1000);
		}

		#endregion

		#region InvoiceBillingPeriodDay

		public void TestInvoiceBillingPeriodDay()
		{
			TestRegistryItem(
				ItemSet.InvoiceBillingPeriodDay,
				"InvoiceBillingPeriodDay",
				WarehouseDataRegistry.Categories.Warehouse_Invoicing,
				"Invoice Billing Period Day",
				"Specifies the default day for billing periods. If weekly billing, then day of the week - if monthly billing, then day of month. This is used for Split Period Billing to determine whether Order level Storage should or should not be charged.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				1);
		}

		#endregion

		#region TestJobServices

		public void TestJobServices()
		{
			TestGenericRegistryItem
			(
				ItemSet.JobServices,
				"WarehouseJobServices",
				WarehouseDataRegistry.Categories.Warehouse_General,
				"Job Services",
				"Services that can be performed on a a Warehouse Job.", RegistryStorageFlags.System | RegistryStorageFlags.Company
			);

			var defaultList = (CodeDescriptionPairList)ObjectFactory.Get<IFreightServiceTypes>();
			AssertEquals("Ensure we inherit all freight JobServices.", defaultList.Count, ItemSet.JobServices.Value.Count);
		}

		#endregion

		#region TestWarehouseWebServerURL

		public void TestWarehouseWebServerURL()
		{
			TestRegistryItem(
			ItemSet.WarehouseWebServerURL,
			"WarehouseWebServerURL",
			WarehouseDataRegistry.Categories.Warehouse_General,
			"Warehouse Web Server URL",
			"The URL used to connect to your warehouse web service.",
			RegistryStorageFlags.System,
			TextEditorType.Url,
			RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
			"http://edidat.sand.wtg.zone/");
		}

		#endregion

		#region Scanning

		#region TestApplicationIdentifiers

		public void TestApplicationIdentifiers()
		{
			var value = ItemSet.ApplicationIdentifiers.Value;
			AssertEquals(83, value.Count);
		}

		#endregion

		#region TestDisplayPickStatus

		public void TestDisplayPickStatus()
		{
			TestRegistryItem(ItemSet.DisplayPickStatus,
				"DisplayPieChart",
				WarehouseDataRegistry.Categories.Warehouse_Scanning,
				"Display Pie Chart",
				"The system will display the percentage of locations that have been picked.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		#endregion

		#region TestWarehouseRFJobFinalizeFailNotificationEmailGroup

		public void TestWarehouseRFJobFinalizeFailNotificationEmailGroup()
		{
			TestGenericRegistryItem(ItemSet.WarehouseRFJobFinalizationFailureNotificationGroup,
				"WarehouseRFJobFinalizationFailureNotificationGroup",
				WarehouseDataRegistry.Categories.Warehouse_Scanning,
				"Warehouse RF Job Finalization Failure Notification Group",
				"This group will be sent an email notification each time a Finalization failure occurs via RF.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.NotCached | RegistryOptions.IsValueOptional,
				RegistryFactory.Instance.GetGroupPK("ALL"));

			Guid newGroupPK = new Guid();
			ItemSet.WarehouseRFJobFinalizationFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGroupPK);
			AssertEquals("WarehouseRFJobFinalizationFailureNotificationGroup.Value", newGroupPK, ItemSet.WarehouseRFJobFinalizationFailureNotificationGroup.Value);
		}

		#endregion

		#region TestWarnWhenDuplicatePalletIdScanned

		public void TestWarnWhenDuplicatePalletIdScanned()
		{
			TestRegistryItem(ItemSet.WarnWhenDuplicatePalletIdScanned,
				"WarnWhenDuplicatePalletIdScanned",
				WarehouseDataRegistry.Categories.Warehouse_Scanning,
				"Warn When Duplicate Pallet ID Scanned in RF",
				"A warning pop-up will be displayed when duplicate Pallet ID is scanned in RF during Unload.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		#endregion

		#endregion

		#region TaskManagement

		public void TestExposeWarehouseTaskManagement()
		{
			TestRegistryItem(ItemSet.ExposeWarehouseTaskManagement, "ExposeWarehouseTaskManagement",
				WarehouseDataRegistry.Categories.Warehouse_TaskManagement,
				"Expose Warehouse Task Management",
				"Expose Warehouse Task Management functionality - DO NOT USE THIS ON PERSISTED TESTING SYSTEMS LIKE UAT ALPHA",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestAutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning()
		{
			TestRegistryItem(ItemSet.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning, "AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning",
				WarehouseDataRegistry.Categories.Warehouse_TaskManagement,
				"Automatically Set Receive Task Planning Status To Ready For Planning",
				"Automatically set a job to Ready For Planning when the receive has started receiving",
				RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestAutomaticallySetPickTaskPlanningStatusToReadyForPlanning()
		{
			TestRegistryItem(ItemSet.AutomaticallySetPickTaskPlanningStatusToReadyForPlanning, "AutomaticallySetPickTaskPlanningStatusToReadyForPlanning",
				WarehouseDataRegistry.Categories.Warehouse_TaskManagement,
				"Automatically Set Pick Task Planning Status To Ready For Planning",
				@"Automatically set a pick job to Ready For Planning when the following conditions are met:
1. Fulfillment Rules Met
2. Not Awaiting Replenishment
3. Cartonization / Pick By Label Flags",
				RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestAutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning()
		{
			TestRegistryItem(ItemSet.AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning, "AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning",
				WarehouseDataRegistry.Categories.Warehouse_TaskManagement,
				"Automatically Set Replenishment Transfer Task Planning Status To Ready For Planning",
				"Automatically set Replenishment Transfer to Ready For Planning on saving.",
				RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestAutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning()
		{
			TestRegistryItem(ItemSet.AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning, "AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning",
				WarehouseDataRegistry.Categories.Warehouse_TaskManagement,
				"Automatically Set Transfer Task Planning Status To Ready For Planning",
				"Automatically set Transfer (excluding Replenishment) to Ready For Planning on saving.",
				RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		#endregion

		#region Locations

		#region TestMaxNumberOfLocationsPerRow

		public void TestMaxNumberOfLocationsPerRow()
		{
			TestRegistryItem(ItemSet.MaxNumberOfLocationsPerRow, "MaxNumberOfLocationsPerRow",
				WarehouseDataRegistry.Categories.Warehouse_Locations,
				"Max Number of Locations per Row",
				"This item allows Warehouses to increase the number of Locations per Row.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				10000,
				1,
				50000);
		}

		#endregion

		#region TestEnableRowNamePrefixValidationForFixedWidthWarehouses

		public void TestEnableRowNamePrefixValidationForFixedWidthWarehouses()
		{
			TestRegistryItem(ItemSet.EnableRowNamePrefixValidationForFixedWidthWarehouses, "EnableRowNamePrefixValidationForFixedWidthWarehouses",
				WarehouseDataRegistry.Categories.Warehouse_Locations,
				"Enable Row Name Prefix Validation for Fixed Width Warehouses",
				"This validation will ensure that row names used in a Fixed Width warehouse will not be a prefix (starts with) for another row name in that warehouse. This is used to prevent potential location barcode conflicts.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				expectedDefaultValue: true);
		}

		#endregion

		#region TestEnableSchemaRedesignChanges

		public void TestEnableSchemaRedesignChanges()
		{
			TestRegistryItem(ItemSet.EnableSchemaRedesignChanges, "EnableSchemaRedesignChanges",
				WarehouseDataRegistry.Categories.Warehouse_General,
				"Enable Schema Redesign Changes",
				"Enable Schema Redesign Changes - DO NOT USE THIS ON PERSISTED TESTING SYSTEMS LIKE UAT ALPHA",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestAllowFinalisationDateUpTo30DaysInTheFuture

		public void TestAllowFinalisationDateUpTo30DaysInTheFuture()
		{
			TestRegistryItem(ItemSet.AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture, "AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture",
				WarehouseDataRegistry.Categories.Warehouse_General,
				"Allow Warehouse Order Finalization Dates Up To 30 Days In The Future",
				"Allow finalization dates up to 30 days in the future on Warehouse Orders via the 'Use Required Date For Outwards Finalized Date' Warehouse option.",
				RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#endregion

		#region TestRFInactivityPeriod

		public void TestRFInactivityPeriod()
		{
			TestRegistryItem(ItemSet.RFInactivityPeriod, "RFInactivityPeriod",
				WarehouseDataRegistry.Categories.Warehouse_General,
				"Android RF Inactivity Period",
				"Specify a time (in minutes) after which the Warehouse RF application (Android only) will automatically log the user out if they have been idle. Changes will take effect on next login to the Warehouse RF application.",
				RegistryStorageFlags.System,
				0);
		}

		#endregion

		#region TestPerformanceReporting

		public void TestPerformanceReporting()
		{
			TestGenericRegistryItem(
				ItemSet.PerformanceReporting,
				"WarehousePerformanceReporting",
				WarehouseDataRegistry.Categories.Warehouse_PerformanceReporting,
				"Performance Metrics",
				"The Performance Reporting Category determines how the performance data is categorized per metric.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
		}

		#endregion

		#region ConditionallyVisibleRegistryItems

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				return new List<string>(base.ConditionallyVisibleRegistryItems)
				{
					nameof(WarehouseDataRegistry.MaxNumberOfLocationsPerRow),
					nameof(WarehouseDataRegistry.EnableRFVolcam),
					nameof(WarehouseDataRegistry.ValidateConcurrentRFLogins),
					nameof(WarehouseDataRegistry.AllowOrderToBeFinalisedWithoutCustomsClearance),
					nameof(WarehouseDataRegistry.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation),
					nameof(WarehouseDataRegistry.EnableImportingCoLoadMastersWithoutSubs),
					nameof(WarehouseDataRegistry.EnablePackageSealNumbers),
					nameof(WarehouseDataRegistry.EnableEcommercePortal),
					nameof(WarehouseDataRegistry.EnablePackageIDMatchingFromPreviousTransitWarehouses),
					nameof(WarehouseDataRegistry.EnableAutoCreateReceiveConsignmentMatchingFromPreviousTransitWarehouses),
					nameof(WarehouseDataRegistry.DefaultUnloadMode),
					nameof(WarehouseDataRegistry.PromptForDriverSignatureAfterUnloadingAirContainer),
					nameof(WarehouseDataRegistry.PromptForDriverSignatureAfterLoadingAirContainer),
					nameof(WarehouseDataRegistry.IgnoreDCNOnUXMLImportWhenPackagesCantBeFoundToAttach),
					nameof(WarehouseDataRegistry.VirtualWarehouseOnRFEnabledUntil),
					nameof(WarehouseDataRegistry.MHEServiceEndpoint),
					nameof(WarehouseDataRegistry.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning),
					nameof(WarehouseDataRegistry.AutomaticallySetPickTaskPlanningStatusToReadyForPlanning),
					nameof(WarehouseDataRegistry.AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning),
					nameof(WarehouseDataRegistry.AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning),
				};
			}
		}

		#endregion

		#region TestDisableOrderLinesUpdateWhenOrderIsPicking

		public void TestPreventOrderLinesUpdateWhenOrderIsInPicking()
		{
			TestRegistryItem(ItemSet.PreventOrderLinesUpdateWhenOrderIsInPicking,
				"PreventOrderLinesUpdateWhenOrderIsInPicking",
				WarehouseDataRegistry.Categories.Warehouse_Orders,
				"Prevent Updates to Orders After Pick is Created",
				"When this item is enabled, updates to order lines will not be allowed once the order is in picking.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#endregion

		#region TestPickFaceReplenishmentLastRun

		public void TestPickFaceReplenishmentLastRun()
		{
			TestGenericRegistryItem(
				ItemSet.PickFaceReplenishmentLastRunUTC,
				"PickFaceReplenishmentLastRunUTC",
				WarehouseDataRegistry.Categories.Warehouse_Picking,
				"Pick Face Replenishment Last Run",
				"Time (in UTC) of the last time the pick face replenishment service task was run.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden,
				new DateTime(1900, 1, 1));
		}

		#endregion

		#region TestRCNPackageCountMatchingType

		public void TestDispatchInstructionRCNPackageCountMatchingType()
		{
			TestRegistryItem(ItemSet.DispatchInstructionRCNPackageCountMatchingType,
				"DispatchInstructionRCNPackageCountMatchingType",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Dispatch Instruction RCN Package Count Matching Type",
				"When matching RCNs for Dispatching via UXML Import, use the following matching criteria.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				ItemSet.DispatchInstructionRCNPackageCountMatchingTypeList,
				"TYP");
		}

		#endregion

		#region TestIgnoreDCNOnUXMLImportWhenPackagesCantBeFoundToAttach

		public void TestIgnoreDCNOnUXMLImportWhenPackagesCantBeFoundToAttach()
		{
			TestRegistryItem(ItemSet.IgnoreDCNOnUXMLImportWhenPackagesCantBeFoundToAttach,
				"IgnoreDCNOnUXMLImportWhenPackagesCantBeFoundToAttach",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Ignore DCN on UXML Import when packages can't be found to attach",
				@"This registry setting controls whether Dispatch Instructions, imported via UXML, are created when packages can't be found to attach to a DCN.

When set to No, the whole import will be rejected.

When set to Yes, the affected DCN will be ignored and the DLL will be created.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestEnableImprovedStorageOfCustomsData

		public void TestEnableImprovedStorageOfCustomsData()
		{
			TestRegistryItem(ItemSet.EnableImprovedStorageOfCustomsData, "enableImprovedStorageOfCustomsData",
				WarehouseDataRegistry.Categories.Warehouse_General,
				"Enable Improved Storage Of Customs Data",
				"Enable Improved Storage Of Customs Data - DO NOT USE THIS ON PERSISTED TESTING SYSTEMS LIKE UAT ALPHA",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestEnableDynamicWorkOrder

		public void TestEnableDynamicWorkOrder()
		{
			TestRegistryItem(ItemSet.EnableDynamicWorkOrder, "enableDynamicWorkOrder",
				WarehouseDataRegistry.Categories.Warehouse_General,
				"Enable Dynamic Work Order Functionality",
				"Enable Dynamic Work Order Functionality - DO NOT USE THIS ON PERSISTED TESTING SYSTEMS LIKE UAT ALPHA",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestUseStaticHomePages

		public void TestUseStaticHomePages()
		{
			TestRegistryItem(ItemSet.UseStaticHomePages,
				"UseStaticHomePages",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Use Static Home Pages",
				"Removes most dynamic data from desktop and mobile home pages. Improves system performance by reducing server load, especially if there are many idle users logged in.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region TestAutoSetDamagedPackageToHeld

		public void TestAutoSetDamagedPackageToHeld()
		{
			TestRegistryItem(ItemSet.AutoSetDamagedPackageToHeld, "AutoSetDamagedPackageToHeld",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Auto Set Damaged Package to Held",
				"When enabled, damaged packages will be set to held automatically.",
				RegistryStorageFlags.Branch,
				true);
		}

		#endregion

		#region TestEnableContainerYard

		public void TestEnableContainerYard()
		{
			TestRegistryItem(ItemSet.EnableContainerYard,
				"EnableContainerYard",
				WarehouseDataRegistry.Categories.Warehouse_ContainerYard,
				"Enables Container Yard Module Section",
				"If yes, container yard module section is visible.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestDefaultDimensionUnitForContainerYard

		public void TestDimensionUnitForContainerYardList()
		{
			AssertEquals(1, WarehouseDataRegistry.Instance.DefaultDimensionUnitListForContainerYard.Count);
			AssertEquals("CM", WarehouseDataRegistry.Instance.DefaultDimensionUnitListForContainerYard.DefaultCode);
			Assert(WarehouseDataRegistry.Instance.DefaultDimensionUnitListForContainerYard.ContainsCode("CM"));
		}

		public void TestDefaultDimensionUnitForContainerYard()
		{
			TestRegistryItem(ItemSet.DefaultDimensionUnitForContainerYard,
				"DefaultDimensionUnitForContainerYard",
				WarehouseDataRegistry.Categories.Warehouse_ContainerYard,
				"Default Dimension Unit",
				"The default unit of dimension to use.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				ItemSet.DefaultDimensionUnitListForContainerYard,
				"CM");
		}

		#endregion

		#region EnableContainerYardUnloadRules

		public void TestEnableContainerYardUnloadRules()
		{
			TestRegistryItem(ItemSet.EnableContainerYardUnloadRules,
				"EnableContainerYardUnloadRules",
				WarehouseDataRegistry.Categories.Warehouse_ContainerYard,
				"Enables Container Yard Unload Rules in PRE",
				"If yes, Unload tile is visible in PRE.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestEnableContainerYardDeliveryAndPickupInstructions

		public void TestEnableContainerYardDeliveryAndPickupInstructions()
		{
			TestRegistryItem(ItemSet.EnableContainerYardDeliveryAndPickupInstructions,
				"EnableContainerYardDeliveryAndPickupInstructions",
				WarehouseDataRegistry.Categories.Warehouse_ContainerYard,
				"Enable Container Yard Delivery and Pick up Instructions",
				"If yes, Container Yard Delivery and Pick up Instructions is visiable.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestEnableMNRCombinedEstimates

		public void TestEnableMNRCombinedEstimates()
		{
			TestRegistryItem(ItemSet.EnableMNRCombinedEstimates,
				"EnableMNRCombinedEstimates",
				WarehouseDataRegistry.Categories.Warehouse_ContainerYard,
				"Enable combined estimates",
				"If yes, the structural and machinery M&R estimates for a unit will be combined into a single estimate.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestEnableSendingCIN750Message

		public void TestEnableSendingCIN750Message()
		{
			TestRegistryItem(ItemSet.EnableSendingCIN750Message,
				"EnableSendingCIN750Message",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse,
				"Enable Sending CIN 750 Message",
				"If yes, Transit Modules would display Message Menu and show CIN 750 Notification Forms in the Document Menu.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region TestEnableWhsPeriodicInvoiceXUT

		public void TestEnableWhsPeriodicInvoiceXUT()
		{
			TestRegistryItem(ItemSet.EnableWhsPeriodicInvoiceXUT,
				"EnableWhsPeriodicInvoiceXUT",
				WarehouseDataRegistry.Categories.Warehouse_Invoicing,
				"Enable Warehouse Periodic Invoice XUT",
				"If yes, Periodic Billings can be imported through Universal Transaction (XUT).",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestConsignmentPackageLabelsMandatoryOnLoad

		public void TestEnableInnerLoadModeOnDispatch()
		{
			TestRegistryItem(ItemSet.ConsignmentPackageLabelsMandatoryOnLoad,
				"ConsignmentPackageLabelsMandatoryOnLoad",
				WarehouseDataRegistry.Categories.Warehouse_TransitWarehouse_Load,
				"Consignment Package Labels Mandatory on Load",
				"Enable this setting to make it mandatory for Packlines packed onto Handling Units to be labeled on load.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestEnableHeldGoodsForOrders

		public void TestEnableHeldGoodsForOrders()
		{
			TestRegistryItem(
				ItemSet.EnableHeldGoodsForOrders,
				"EnableHeldGoodsForOrders",
				WarehouseDataRegistry.Categories.Warehouse_Orders,
				"Enable Held Goods entry on Order Line",
				"When this registry setting is enabled, the entry field for Held Goods will be enabled on Order Lines.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region NumberCustomisation

		#region TestWarehouseNumberCustomisation_Order

		public void TestWarehouseNumberCustomisation_Order()
		{
			AssertEquals("StorageFlags is System and Company", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.WarehouseNumberCustomisation_Order.Storage);
			AssertEquals("Caption is set correctly", "Order Number Customization", ItemSet.WarehouseNumberCustomisation_Order.Caption);

			var customisations = new BillOfLadingNumberCustomisationsByServiceLevel();

			customisations.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix = true;
			customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";

			ItemSet.WarehouseNumberCustomisation_Order.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisations);

			var value = ItemSet.WarehouseNumberCustomisation_Order.Value;

			AssertEquals("Value.RemoveSPrefix", true, value.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.WarehouseJob | NumberCustomisationElementCategories.WarehouseOrder, value.Categories);
			AssertEquals("Value.AllowNonAlphanumericCharacters", true, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", true, value.EnableMacroInsertion);

			var dataType = ItemSet.WarehouseNumberCustomisation_Order.DataType as BillCustomisationByServiceLevelRegistryDataType;
			AssertEquals("DataType.MacroType", ObjectFactory.GetType<IWhsOrder>(), dataType.MacroType);
		}

		public void TestWarehouseNumberCustomisation_OrderIsAddedToRegistry()
		{
			AssertVisible(ItemSet.WarehouseNumberCustomisation_Order);
		}

		#endregion

		#region TestWarehouseNumberCustomisation_Receive

		public void TestWarehouseNumberCustomisation_Receive()
		{
			AssertEquals("StorageFlags is System and Company", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.WarehouseNumberCustomisation_Receive.Storage);
			AssertEquals("Caption is set correctly", "Receive Number Customization", ItemSet.WarehouseNumberCustomisation_Receive.Caption);

			var customisations = new BillOfLadingNumberCustomisationsByServiceLevel();

			customisations.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix = true;
			customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";

			ItemSet.WarehouseNumberCustomisation_Receive.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisations);

			var value = ItemSet.WarehouseNumberCustomisation_Receive.Value;

			AssertEquals("Value.RemoveSPrefix", true, value.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.WarehouseJob | NumberCustomisationElementCategories.WarehouseReceive, value.Categories);
			AssertEquals("Value.AllowNonAlphanumericCharacters", true, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", true, value.EnableMacroInsertion);

			var dataType = ItemSet.WarehouseNumberCustomisation_Receive.DataType as BillCustomisationByServiceLevelRegistryDataType;
			AssertEquals("DataType.MacroType", ObjectFactory.GetType<IWhsReceive>(), dataType.MacroType);
		}

		public void TestWarehouseNumberCustomisation_ReceiveIsAddedToRegistry()
		{
			AssertVisible(ItemSet.WarehouseNumberCustomisation_Receive);
		}

		#endregion

		#endregion

		#region Gate Management

		public void TestEnableGateRegistry()
		{
			TestRegistryItem(ItemSet.EnableGateManagement,
				expectedName: "EnableGateManagement",
				expectedCategory: WarehouseDataRegistry.Categories.Warehouse_GateManagement,
				expectedCaption: "Enable Gate Management",
				expectedHint: "Enables the 'Gate Management' section under the Warehouse module in CW1",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: false
				);
		}

		public void TestAllowGateBookingCreationMenu()
		{
			TestRegistryItem(
				ItemSet.AllowGateBookingCreationMenu,
				"AllowGateBookingCreationMenu",
				WarehouseDataRegistry.Categories.Warehouse_GateManagement,
				"Allow creation of Gate Bookings in the portal",
				"If yes, 'Gate Booking' and 'New Gate Booking' menu items are visible.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestDisallowTimeslotDateLaterthanCurrentDateGateIn()
		{
			TestRegistryItem(ItemSet.DisallowTimeslotDateLaterthanCurrentDateGateIn,
				"DisallowTimeslotDateLaterthanCurrentDateGateIn",
				WarehouseDataRegistry.Categories.Warehouse_GateManagement,
				"Disallow Time-slot Date Later than Current Date in Gate-In",
				"Disallows Time-slot Date Later than Current Date in Gate-In",
				RegistryStorageFlags.System,
				true);
		}

		public void TestDisallowTWHGateInWithoutBooking()
		{
			TestRegistryItem(
				ItemSet.DisallowTWHGateInWithoutBooking,
				"DisallowTWHGateInWithoutBooking",
				WarehouseDataRegistry.Categories.Warehouse_GateManagement,
				"Disallow Gate In Without Booking for Transit Warehouse Facilities",
				"If yes, 'Gate In Without Booking' will not be shown for Transit Warehouse branches and a Transit Warehouse cannot be chosen for a gate in without booking.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestEnableGateInValidation()
		{
			TestRegistryItem(ItemSet.EnableGateInValidation,
				"EnableGateInValidation",
				WarehouseDataRegistry.Categories.Warehouse_GateManagement,
				"Enables gate-in validation at the facility",
				"Enables gate-in validation at the facility",
				RegistryStorageFlags.System,
				true);
		}

		public void TestWarehouseRegistry_GateManagementInboundOAuthAuthorityUrls()
		{
			Assert("Registry Item GateManagementInboundOAuthAuthorityUrls should have RegistryOptions.PreserveTestValue",
				WarehouseDataRegistry.Instance.GateManagementInboundOAuthAuthorityUrls.HasOption(RegistryOptions.PreserveTestValue));
		}

		public void TestWarehouseRegistry_GateManagementInboundOAuthClientIDs()
		{
			Assert("Registry Item GateManagementInboundOAuthClientIDs should have RegistryOptions.PreserveTestValue",
				WarehouseDataRegistry.Instance.GateManagementInboundOAuthClientIDs.HasOption(RegistryOptions.PreserveTestValue));
		}

		public void TestWarehouseRegistry_EnableUniversalIntegrationBetweenGateAndContainerYard()
		{
			TestRegistryItem(ItemSet.EnableUniversalIntegrationBetweenGateAndContainerYard,
				"EnableUniversalIntegrationBetweenGateAndContainerYard",
				WarehouseDataRegistry.Categories.Warehouse_GateManagement,
				"Enable universal integration between gate and container yard",
				"Enables the logic that allows gate and container yard to communicate through UXML using universal job links",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestWarehouseRegistry_EnableUniversalIntegrationBetweenGateAndTransitWarehouse()
		{
			TestRegistryItem(ItemSet.EnableUniversalIntegrationBetweenGateAndTransitWarehouse,
				"EnableUniversalIntegrationBetweenGateAndTransitWarehouse",
				WarehouseDataRegistry.Categories.Warehouse_GateManagement,
				"Enable universal integration between gate and transit warehouse",
				"Enables the logic that allows gate and transit warehouse to communicate through UXML using universal job links",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region EnableMHEServiceTestingMode

		public void TestEnableMHEServiceTestingMode_Core()
		{
			TestRegistryItem(
				ItemSet.EnableMHEServiceTestingMode,
				"EnableMHEServiceTestingMode",
				WarehouseDataRegistry.Categories.Warehouse_MHE,
				"Enable MHE Service Testing Mode",
				"Uses staging client ID for communication with MHE service.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region SaveMHEFilesToEDocs

		public void TestSaveMHEFilesToEDocs_Core()
		{
			TestRegistryItem(
				ItemSet.SaveMHEFilesToEDocs,
				"SaveMHEFilesToEDocs",
				WarehouseDataRegistry.Categories.Warehouse_MHE,
				"Save MHE Files To eDocs",
				"If set to True, MHE files will be saved to eDocs. If set to False, a URL linking to the MHE files will be saved in the database.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region TestMHEServiceEndpoint

		public void TestMHEServiceEndpoint_Core()
		{
			TestRegistryItem(
				ItemSet.MHEServiceEndpoint,
				"MHEServiceEndpoint",
				WarehouseDataRegistry.Categories.Warehouse_MHE,
				"MHE Service Endpoint",
				"The URL for dimensioner middleware service.",
				RegistryStorageFlags.System,
				TextEditorType.Url,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				"https://mheservice.wisegrid.net/");
		}

		#endregion
	}
}
