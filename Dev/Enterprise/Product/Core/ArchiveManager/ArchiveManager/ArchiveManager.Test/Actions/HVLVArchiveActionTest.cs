using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions;
using Enterprise.ArchiveManager.Business.SystemDescriptors;
using Enterprise.ArchiveManager.Engine;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Actions
{
	[UseSnapshotProtection]
	class HVLVArchiveActionTest : TestCaseWithFactory
	{
		static readonly int OnOrBeforeMinimumValue = RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.HAR);

		#region PopulateHVLVShipmentHistory

		public void TestPopulateShipmentHistory_WhenShipmentIsArrangedToBeArchived()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = header.PK;

			Factory.Save();

			AssertHVLVShipmentHistoryRowCount(0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVShipmentHistoryRowCount(1);
		}

		public void TestPopulateShipmentHistory_WhenShipmentIsNotArrangedToBeArchived()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue - 1));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = header.PK;

			Factory.Save();

			AssertHVLVShipmentHistoryRowCount(0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVShipmentHistoryRowCount(0);
		}

		public void TestPopulateShipmentHistory_GivenOneShipmentToArchive_WhenJS_E_ARVOlderThan10Years_ThenCreateOneHSHRow()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.BrettsBirthday;

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = header.PK;

			Factory.Save();

			AssertHVLVShipmentHistoryRowCount(0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);
			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVShipmentHistoryRowCount(1);
		}

		public void TestPopulateShipmentHistory_ProductTariff_Origin_Destination_ProductCode_CC_Lookup_CountsAreAllPopulated()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = header.PK;
			var item = consignment.Items.AddNew();
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;
			var classification = GetNewClassification("IMP");
			itemLine.HVS_CC_Lookup = classification.PK;
			itemLine.HVS_OriginTariff = "ORGN";
			itemLine.HVS_RN_NKOriginCountryCode = "AU";
			itemLine.HVS_DestinationTariff = "DEST";

			Factory.Save();
			var targetCode = "PROD1234";
			var sql = $"update dbo.HVLVItemLine set HVS_PRODUCTCODE = '{targetCode}'";
			_ = TestConnection.ExecuteScalar(sql);
			AssertHVLVShipmentHistoryRowCount(0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVShipmentHistoryRowCount(1);
			AssertHVLVShipmentHistoryColumns(new[] {
					new ColumnAssertion("HSH_ItemLineDetailsProvidedCount", 1),
					new ColumnAssertion("HSH_ItemLineClassificationLookupUsedCount", 1),
					new ColumnAssertion("HSH_ItemLineProductCodeUsedCount", 1),
					new ColumnAssertion("HSH_ItemLineOrigINTariffProvidedCount", 1),
					new ColumnAssertion("HSH_ItemLineDestinationTariffProvidedCount", 1)
				});
		}

		BaseCusClassification GetNewClassification(string type)
		{
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = type;
			classification.CC_TariffNum = "1234.56.78";
			classification.CC_Description = type + " DESC";
			classification.CC_LookupCode = "TEST";

			return classification;
		}

		void AssertHVLVShipmentHistoryColumns(IEnumerable<ColumnAssertion> assertions)
		{
			var dataTable = GetResultSet("SELECT * FROM dbo.HVLVShipmentHistory");
			assertions.ForEach(assertion => assertion.Assert(dataTable));
		}

		DataTable GetResultSet(string sql)
			=> DataUtils.GetDataTableFromQuery(Db.Connection, sql);

		sealed class ColumnAssertion
		{
			string Column { get; }
			object Value { get; }
			int RecordNo { get; }

			public ColumnAssertion(string column, object value, int recordNo = 0)
			{
				Column = column;
				Value = value;
				RecordNo = recordNo;
			}

			public void Assert(DataTable data)
			{
				AssertEquals($"Row {RecordNo}: {Column} did not equal {Value}, but was {data.Rows[RecordNo][Column]}", Value, data.Rows[RecordNo][Column]);
			}
		}

		public void TestPopulateShipmentHistory_ProductTariff_Origin_Destination_ProductCode_CC_Lookup_CountsAreAllPopulatedWithZero()
		{
			AssertHVLVShipmentHistoryRowCount(0);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = header.PK;
			var item = consignment.Items.AddNew();
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;
			itemLine.HVS_ProductCode = "";
			itemLine.HVS_OriginTariff = "";
			itemLine.HVS_RN_NKOriginCountryCode = "";
			itemLine.HVS_DestinationTariff = "";

			Factory.Save();

			AssertHVLVShipmentHistoryRowCount(0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVShipmentHistoryRowCount(1);
			AssertHVLVShipmentHistoryColumns(new[] { new ColumnAssertion("HSH_ItemLineDetailsProvidedCount", 1), new ColumnAssertion("HSH_ItemLineClassificationLookupUsedCount", 0), new ColumnAssertion("HSH_ItemLineProductCodeUsedCount", 0), new ColumnAssertion("HSH_ItemLineOrigINTariffProvidedCount", 0), new ColumnAssertion("HSH_ItemLineDestinationTariffProvidedCount", 0) });
		}

		public void TestPopulateShipmentHistory_HSHConsignmentAggregate()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001000";
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();

			consignment1.HVC_HCH_Header = header.PK;
			consignment2.HVC_HCH_Header = header.PK;
			consignment3.HVC_HCH_Header = header.PK;

			consignment1.HVC_UndgClass = "6";
			consignment2.HVC_UndgClass = "6";

			consignment2.HVC_IsTaxPrePaid = true;
			consignment3.HVC_IsTaxPrePaid = true;

			consignment1.HVC_ConsigneeInstructions = string.Empty;
			consignment2.HVC_ConsigneeInstructions = string.Empty;
			consignment3.HVC_ConsigneeInstructions = string.Empty;

			consignment1.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;
			consignment3.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;

			consignment1.HVC_IsHazardous = true;
			consignment2.HVC_IsHazardous = true;
			consignment3.HVC_IsHazardous = true;

			consignment2.HVC_RequiresFumigation = true;

			consignment1.HVC_IsPersonalEffects = false;

			consignment2.HVC_IsTimber = true;
			consignment3.HVC_IsTimber = true;

			consignment1.HVC_IsPerishable = true;
			consignment2.HVC_IsPerishable = true;
			consignment3.HVC_IsPerishable = true;

			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_DeclarationReference = "Test Import Ref";
			declaration2.JE_DeclarationReference = "Test Export Ref";
			consignment1.HVC_JE_ImportDeclaration = declaration1.PK;
			consignment1.HVC_JE_ExportDeclaration = declaration2.PK;

			consignment1.HVC_IsActive = true;
			consignment2.HVC_IsActive = true;

			consignment1.HVC_WeightUQ = Core.Constants.Weight.Pounds;
			consignment2.HVC_WeightUQ = Core.Constants.Weight.Kilograms;

			consignment1.HVC_VolumeUQ = Core.Constants.Volume.Litre;
			consignment2.HVC_VolumeUQ = Core.Constants.Volume.CubicFeet;

			consignment1.HVC_IsSignatureRequired = true;
			consignment2.HVC_IsSignatureRequired = true;
			consignment3.HVC_IsSignatureRequired = true;

			consignment1.HVC_AuthorityToLeave = true;
			consignment2.HVC_AuthorityToLeave = true;
			consignment3.HVC_AuthorityToLeave = false;

			consignment1.HVC_IsSelfBooked = true;
			consignment2.HVC_IsSelfBooked = true;
			consignment3.HVC_IsSelfBooked = true;

			consignment1.HVC_OA_DestinationDepot = ZGuid.Empty;
			consignment2.HVC_OA_DestinationDepot = ZGuid.Empty;
			consignment3.HVC_OA_DestinationDepot = ZGuid.Empty;

			consignment1.HVC_VendorIdentifier = "9827341";
			consignment2.HVC_VendorIdentifier = "9827341";
			consignment3.HVC_VendorIdentifier = "";

			consignment1.HVC_INCO = "FOB";
			consignment2.HVC_INCO = "FOB";
			consignment3.HVC_INCO = "CIF";

			consignment1.HVC_OH_LastMileCarrierBookingAgent = ZGuid.Empty;
			consignment2.HVC_OH_LastMileCarrierBookingAgent = ZGuid.Empty;
			consignment3.HVC_OH_LastMileCarrierBookingAgent = ZGuid.Empty;

			consignment1.HVC_CarrierAccountNumber = "";
			consignment2.HVC_CarrierAccountNumber = "";
			consignment3.HVC_CarrierAccountNumber = "";

			consignment1.HVC_ConsigneeAddressValidationStatus = "INV";
			consignment2.HVC_ConsigneeAddressValidationStatus = "NYV";
			consignment3.HVC_ConsigneeAddressValidationStatus = "NYV";

			consignment1.HVC_OA_ConsigneeAddress = ZGuid.Empty;
			consignment2.HVC_OA_ConsigneeAddress = ZGuid.Empty;
			consignment3.HVC_OA_ConsigneeAddress = ZGuid.Empty;

			consignment1.HVC_ImportReleaseStatus = "NON";
			consignment1.HVC_ExportReleaseStatus = "NON";

			consignment2.HVC_ImportReleaseStatus = "CLR";
			consignment2.HVC_ExportReleaseStatus = "NON";

			consignment3.HVC_ImportReleaseStatus = "NON";
			consignment3.HVC_ExportReleaseStatus = "CLR";

			consignment1.HVC_ACASStatus = "SR";
			consignment2.HVC_ACASStatus = "SR";
			consignment3.HVC_ACASStatus = "";

			consignment1.HVC_ItemCount = 1;
			var item1a = consignment2.Items.AddNew();
			item1a.HVI_ManifestedWeight = 24;
			item1a.HVI_ItemId = "ITEM1A";
			item1a.HVI_ActualWeight = 2882;

			consignment2.HVC_ItemCount = 2;
			var item2a = consignment2.Items.AddNew();
			item2a.HVI_ManifestedWeight = 1;
			item2a.HVI_ItemId = "ITEM2A";
			item2a.HVI_ActualWeight = 5432;
			var item2b = consignment2.Items.AddNew();
			item2b.HVI_ManifestedWeight = 1;
			item2b.HVI_ItemId = "ITEM2B";
			item2b.HVI_ActualWeight = 1234;
			item2b.HVI_ActualVolume = 1351;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reloadedConsignment3 = factory2.Load<HVLVConsignment>(consignment3.PK);
			reloadedConsignment3.HVC_IsActive = false;
			factory2.Save();

			AssertHVLVShipmentHistoryRowCount(0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			var sql = $"SELECT * FROM dbo.HVLVShipmentHistory";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Expected number of rows ", 1, dataTable.Rows.Count);

			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentIsDangerousGoodsCount"], 2);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentIsTaxPrePaidCount"], 2);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentIsPreScreenedCount"], 2);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentIsHazardousCount"], 3);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentRequiresFumigationCount"], 1);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentIsPersonalEffectsCount"], 0);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentIsTimberCount"], 2);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentIsPerishableCount"], 3);
			AssertEquals(dataTable.Rows[0]["HSH_StandAloneDeclarationCount"], 2);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentCount"], 3);
			AssertEquals(dataTable.Rows[0]["HSH_ActiveConsignmentCount"], 2);
			AssertEquals(dataTable.Rows[0]["HSH_MultiItemConsignmentCount"], 1);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentSignatureRequiredCount"], 3);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentAuthorityToLeaveCount"], 2);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentHasConsigneeInstructionCount"], 0);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentSelfBookedCount"], 3);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentHasDestinationDepotCount"], 0);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentHasVendorIDCount"], 2);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentINCOIsFOBCount"], 2);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentINCOIsCIFCount"], 1);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentLMCAgentUsedCount"], 0);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentCarrierAccountNumberProvidedCount"], 0);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentAddressValidatedCount"], 1);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentConsigneeIsOrgCount"], 0);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentNonCustomsClearedCount"], 1);
			AssertEquals(dataTable.Rows[0]["HSH_ConsignmentACASMessageUsedCount"], 2);
			AssertEquals(dataTable.Rows[0]["HSH_MaximumWeight"], Convert.ToDecimal(9548.0));
			AssertEquals(dataTable.Rows[0]["HSH_AverageWeight"], Convert.ToDecimal(3182.667));
			AssertEquals(dataTable.Rows[0]["HSH_MaximumVolume"], Convert.ToDecimal(38.256));
			AssertEquals(dataTable.Rows[0]["HSH_AverageVolume"], Convert.ToDecimal(12.752));
		}

		public void TestPopulateShipmentHistory_HSHAggregateItemCounts()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001000";
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = header.PK;

			var clearedItem = consignment.Items.AddNew();
			clearedItem.HVI_ReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;

			var heldItem = consignment.Items.AddNew();
			heldItem.HVI_ReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;

			var nonItem = consignment.Items.AddNew();
			nonItem.HVI_ReleaseStatus = HVLVReleaseStatus.ShortVersion.None;

			var clearedScannedItem = consignment.Items.AddNew();
			clearedScannedItem.HVI_ReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;
			clearedScannedItem.HVI_Status = HVLVItemStatus.Codes.DispatchedToLastMileCarrier;

			var heldScannedItem = consignment.Items.AddNew();
			heldScannedItem.HVI_ReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;
			heldScannedItem.HVI_Status = HVLVItemStatus.Codes.DispatchedToLastMileCarrier;

			var nonScannedItem = consignment.Items.AddNew();
			nonScannedItem.HVI_ReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
			nonScannedItem.HVI_Status = HVLVItemStatus.Codes.DispatchedToLastMileCarrier;

			var surplusItem = consignment.Items.AddNew();
			surplusItem.HVI_Status = HVLVItemStatus.Codes.SurplusAtDestinationDepot;

			var interceptedItem = consignment.Items.AddNew();
			interceptedItem.HVI_Status = HVLVItemStatus.Codes.ReceivedAtDestinationDepotWithException;

			var seizedItem = consignment.Items.AddNew();
			seizedItem.HVI_Status = HVLVItemStatus.Codes.SeizedByCustoms;

			var discardedItem = consignment.Items.AddNew();
			discardedItem.HVI_Status = HVLVItemStatus.Codes.DiscardedAtDestinationDepot;

			var deliveredItem = consignment.Items.AddNew();
			deliveredItem.HVI_Status = HVLVItemStatus.Codes.Delivered;

			var proofOfDeliveryItem = consignment.Items.AddNew();
			proofOfDeliveryItem.HVI_Status = HVLVItemStatus.Codes.ProofOfDeliveryReceived;

			var damagedItem = consignment.Items.AddNew();
			damagedItem.HVI_IsDamaged = true;

			var ullagedItem = consignment.Items.AddNew();
			ullagedItem.HVI_IsUllaged = true;

			var pillagedItem = consignment.Items.AddNew();
			pillagedItem.HVI_IsPillaged = true;

			var inactiveItem = consignment.Items.AddNew();
			inactiveItem.HVI_IsActive = false;

			var transportBooking = Factory.NewWithValidTestData<DtbBooking>();
			var itemOnTransportBooking = consignment.Items.AddNew();
			itemOnTransportBooking.HVI_KM_LastMileTransportBooking = transportBooking.PK;

			var itemWithContainerNumber = consignment.Items.AddNew();
			itemWithContainerNumber.HVI_ContainerNumber = "LTT0001";

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var itemOnOuterPackage = consignment.Items.AddNew();
			itemOnOuterPackage.HVI_HVO_OuterPackage = outerPackage.PK;

			var itemUsedForSecurityFiling = consignment.Items.AddNew();
			itemUsedForSecurityFiling.HVI_SecurityFilingFirstUsageTimeUtc = ZDateTime.UtcNow;

			foreach (HVLVItem item in consignment.Items)
			{
				item.HVI_JS_LoadedOnShipment = shipment.PK;
			}

			Factory.Save();

			AssertHVLVShipmentHistoryRowCount(0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			var sql = $"SELECT * FROM dbo.HVLVShipmentHistory";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Expected number of rows ", 1, dataTable.Rows.Count);

			AssertEquals(20, dataTable.Rows[0]["HSH_ItemCount"]);
			AssertEquals(2, dataTable.Rows[0]["HSH_ClearedItemCount"]);
			AssertEquals(2, dataTable.Rows[0]["HSH_HeldItemCount"]);
			AssertEquals(16, dataTable.Rows[0]["HSH_NoneReportedItemCount"]);
			AssertEquals(5, dataTable.Rows[0]["HSH_ScannedItemCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_ScannedClearedItemCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_ScannedHeldItemCount"]);
			AssertEquals(3, dataTable.Rows[0]["HSH_ScannedNoneReportedItemCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_SurplusItemCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_InterceptedItemCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_SeizedItemCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_DiscardedItemCount"]);
			AssertEquals(18, dataTable.Rows[0]["HSH_NonDeliveredItemCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_TransportBookingBookedItemCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_DamagedItemCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_PillagedItemCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_UllagedItemCount"]);
			AssertEquals(19, dataTable.Rows[0]["HSH_ActiveItemCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_ItemHasContainerNumberCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_ItemHasOuterPackageCount"]);
			AssertEquals(1, dataTable.Rows[0]["HSH_ItemHasSecurityFiledCount"]);
		}

		void AssertHVLVShipmentHistoryRowCount(int expectedRowCount)
		{
			var sql = $"SELECT COUNT (*) FROM dbo.HVLVShipmentHistory";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected this number of rows in HVLVShipmentHistory", expectedRowCount, result);
		}

		#endregion

		#region HVLVUsageHistory

		public void TestPopulateHVLVUsageHistory()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001000";
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = header.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			var sqlText = string.Format(@"
INSERT dbo.HVLVUsage(HXU_PK, HXU_UsageTimeUtc, HXU_GC_NKCompany, HXU_BranchCode, HXU_HVI_ParentItem, HXU_Category, HXU_Code, HXU_GS_NKUser) VALUES
					(NEWID(), '2023-12-08', 'WTG', 'DEM', '{0}', 'CW1', 'AAA', '~BP'),
					(NEWID(), '2023-12-08', 'WTG', 'DEM', '{0}', 'CW1', 'AAB', '~BP'),
					(NEWID(), '2023-12-08', 'WTG', 'DEM', '{0}', 'LVD', 'BBB', '~BP'),
					(NEWID(), '2023-12-08', 'WTG', 'DEM', '{0}', 'SEC', 'CCC', '~BP'),
					(NEWID(), '2023-12-08', 'WTG', 'DEM', '{0}', 'HVD', 'DDD', '~BP'),
					(NEWID(), '2023-12-08', 'WTG', 'DEM', '{0}', 'WEB', 'EEE', '~BP'),
					(NEWID(), '2023-12-08', 'WTG', 'DEM', '{0}', 'CW1', 'FFF', '~BP')", item.PK);

			_ = TestConnection.ExecuteNonQuery(sqlText);

			AssertTableRowCount("HVLVUsageHistory", 0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertTableRowCount("HVLVUsageHistory", 5);
			var dataTable = GetResultSet("SELECT * FROM dbo.HVLVUsageHistory");
			var expectResult = new List<(string category, int count)> { ("CW1", 3), ("LVD", 1), ("SEC", 1), ("HVD", 1), ("WEB", 1) };

			var i = 0;
			while (i < dataTable.Rows.Count)
			{
				Assert(expectResult.Contains(((string)dataTable.Rows[i][2], (int)dataTable.Rows[i][3])));
				i++;
			}
		}

		#endregion

		#region SetHVLVDataToArchived

		public void TestSetHVLVDataToArchived()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment.JS_UniqueConsignRef = "S00001500";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			consignmentHeader.HCH_JS_Shipment = shipment.PK;

			Factory.Save();

			var logger = new TestArchiveLogger();
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var archiveSet = GetArchiveSet(shipment, config);

			Assert("ConsignmentHeader should not be archived", !consignmentHeader.HCH_IsArchived);

			var archiveAction = new HVLVArchiveAction(logger, archiveSet, config);
			archiveAction.Execute();

			var newFactory = new BusinessObjectFactory();
			consignmentHeader = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader.PK);

			Assert("ConsignmentHeader should be archived", consignmentHeader.HCH_IsArchived);
		}

		#endregion

		#region PopulateHVLVDeliveryByArea

		public void TestPopulatesDeliveryByArea_HasWeightCapForConsignment()
		{
			const decimal weightCap = 999999.999m;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header.PK;
			consignment1.HVC_ActualWeight = weightCap;
			consignment1.HVC_OH_LastMileCarrier = carrier.PK;
			consignment1.HVC_WeightUQ = Core.Constants.Weight.Kilograms;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_HCH_Header = header.PK;
			consignment2.HVC_ActualWeight = weightCap;
			consignment2.HVC_OH_LastMileCarrier = carrier.PK;
			consignment2.HVC_WeightUQ = Core.Constants.Weight.Kilograms;

			var item1 = consignment1.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			item1.HVI_ActualWeight = weightCap;

			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_ActualWeight = weightCap;

			Factory.Save();

			AssertHVLVDeliveryByAreaRowCount(0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(1);

			var sql = $"SELECT HDB_ConsignmentWeightSumInKG FROM dbo.HVLVDeliveryByArea";
			var resultDecimal = (decimal)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_ConsignmentWeightSumInKG to be 999999.999 KG", weightCap, resultDecimal);
		}

		public void TestPopulateDeliveryByArea_HasVolumeCapForConsignment()
		{
			const decimal volumeCap = 999999.999m;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header.PK;
			consignment1.HVC_ActualVolume = volumeCap;
			consignment1.HVC_OH_LastMileCarrier = carrier.PK;
			consignment1.HVC_VolumeUQ = Core.Constants.Volume.CubicMetres;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_HCH_Header = header.PK;
			consignment2.HVC_ActualVolume = volumeCap;
			consignment2.HVC_OH_LastMileCarrier = carrier.PK;
			consignment2.HVC_VolumeUQ = Core.Constants.Volume.CubicMetres;

			var item1 = consignment1.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			item1.HVI_ActualVolume = volumeCap;

			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_ActualVolume = volumeCap;

			Factory.Save();

			AssertHVLVDeliveryByAreaRowCount(0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(1);

			var sql = $"SELECT HDB_ConsignmentVolumeSumInM3 FROM dbo.HVLVDeliveryByArea";
			var resultDecimal = (decimal)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_ConsignmentVolumeSumInM3 to be 999999.999 M3", volumeCap, resultDecimal);
		}

		public void TestPopulateDeliveryByArea_WhenShipmentIsArrangedToBeArchived()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header.PK;

			Factory.Save();

			AssertHVLVDeliveryByAreaRowCount(0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(1);
		}

		public void TestPopulateDeliveryByArea_GivenOneShipmentToArchive_WhenRunningHARTwice_ThenNoDuplicateHDBRowsAreCreatedForShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header.PK;

			Factory.Save();

			AssertHVLVDeliveryByAreaRowCount(0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);
			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(1);
		}

		public void TestPopulateDeliveryByArea_WhenShipmentIsNotArrangedToBeArchived()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue - 1));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header.PK;

			Factory.Save();

			AssertHVLVDeliveryByAreaRowCount(0);

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(0);
		}

		[TestDate(2022, 05, 15)]
		public void TestPopulateDeliveryByArea_CreatedMonth()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var header2 = shipment2.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment5 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header1.PK;
			consignment4.HVC_HCH_Header = header2.PK;
			consignment5.HVC_HCH_Header = header2.PK;
			consignment1.HVC_SystemCreateTimeUtc = ZDateTime.Now;
			consignment2.HVC_SystemCreateTimeUtc = ZDateTime.Now;
			consignment3.HVC_SystemCreateTimeUtc = ZDateTime.Now;
			consignment4.HVC_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(1);
			consignment5.HVC_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(1);

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(2);

			var sql = $"SELECT COUNT (*) FROM dbo.HVLVDeliveryByArea WHERE HDB_CreatedMonth = 5";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HVLVDeliveryByArea to have 1 rows where HDB_CreatedMonth = 5 (Or May)", 1, result);

			sql = $"SELECT COUNT (*) FROM dbo.HVLVDeliveryByArea WHERE HDB_CreatedMonth = 6";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HVLVDeliveryByArea to have 1 rows where HDB_CreatedMonth = 6 (Or June)", 1, result);
		}

		[TestDate(2020, 01, 01)]
		public void TestPopulateDeliveryByArea_CreatedYear()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var header2 = shipment2.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header1.PK;
			consignment4.HVC_HCH_Header = header2.PK;
			consignment1.HVC_SystemCreateTimeUtc = ZDateTime.Now;
			consignment2.HVC_SystemCreateTimeUtc = ZDateTime.Now;
			consignment3.HVC_SystemCreateTimeUtc = ZDateTime.Now.AddYears(1);
			consignment4.HVC_SystemCreateTimeUtc = ZDateTime.Now.AddYears(1);

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(3);

			var sql = $"SELECT COUNT (*) FROM dbo.HVLVDeliveryByArea WHERE HDB_CreatedYear = 2020";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HVLVDeliveryByArea to have 1 rows where HDB_CreatedYear = 2020", 1, result);

			sql = $"SELECT COUNT (*) FROM dbo.HVLVDeliveryByArea WHERE HDB_CreatedYear = 2021";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HVLVDeliveryByArea to have 2 rows where HDB_CreatedMonth = 2021", 2, result);
		}

		public void TestPopulateDeliveryByArea_ConsigneeCountry()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var header2 = shipment2.GetOrCreateHVLVConsignmentHeader();
			var header3 = shipment2.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment3.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header2.PK;
			consignment3.HVC_HCH_Header = header3.PK;
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment2.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment3.HVC_RN_NKConsigneeCountryCode = "NL";
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment3.HVC_OH_LastMileCarrier = carrier1.PK;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(3);

			var sql = $"SELECT COUNT(*) FROM dbo.HVLVDeliveryByArea WHERE HDB_RN_NKConsigneeCountry = 'AU'";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HVLVDeliveryByArea where HDB_RN_NKConsigneCountry to be 2", 2, result);

			sql = $"SELECT COUNT(*) FROM dbo.HVLVDeliveryByArea WHERE HDB_RN_NKConsigneeCountry = 'NL'";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HVLVDeliveryByArea where HDB_RN_NKConsigneeCountry to be 1", 1, result);
		}

		public void TestPopulateDeliveryByArea_ConsigneePostcode()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment5 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment6 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header1.PK;
			consignment4.HVC_HCH_Header = header1.PK;
			consignment5.HVC_HCH_Header = header1.PK;
			consignment6.HVC_HCH_Header = header1.PK;
			consignment1.HVC_ConsigneePostcode = "2000";
			consignment2.HVC_ConsigneePostcode = "2000";
			consignment3.HVC_ConsigneePostcode = "2000";
			consignment4.HVC_ConsigneePostcode = "2200";
			consignment5.HVC_ConsigneePostcode = "2000";
			consignment6.HVC_ConsigneePostcode = ZString.Empty;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(3);

			var sql = $"SELECT COUNT(*) FROM dbo.HVLVDeliveryByArea WHERE HDB_ConsigneePostcode = '2200'";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected there to be one row in HVLVDeliveryByArea with 2200 postcode", 1, result);

			sql = $"SELECT COUNT(*) FROM dbo.HVLVDeliveryByArea WHERE HDB_ConsigneePostcode = '2000'";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected there to be one row in HVLVDeliveryByArea with 2000 postcode", 1, result);

			sql = $"SELECT COUNT(*) FROM dbo.HVLVDeliveryByArea WHERE HDB_ConsigneePostcode = ''";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected there to be one row in HVLVDeliveryByArea with empty postcode", 1, result);
		}

		public void TestPopulateDeliveryByArea_LastMileCarrier()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var header2 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var header3 = shipment1.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment3.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment5 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment6 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header2.PK;
			consignment4.HVC_HCH_Header = header2.PK;
			consignment5.HVC_HCH_Header = header3.PK;
			consignment6.HVC_HCH_Header = header3.PK;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment3.HVC_OH_LastMileCarrier = carrier2.PK;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(3);

			var sql = $"SELECT COUNT (*) FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("ExpectedHVLVDeliveryByArea should have 1 row for carrier1", 1, result);

			sql = $"SELECT COUNT (*) FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier2.PK}'";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HVLVDeliveryByArea to have 1 row for carrier2", 1, result);

			sql = $"SELECT COUNT (*) FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier IS NULL";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HVLVDeliveryByArea to have 1 row when HDB_OH_LastMileCarrier is null", 1, result);
		}

		public void TestPopulateDeliveryByArea_ConsignmentCount()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var header2 = shipment1.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment5 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment6 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header1.PK;
			consignment4.HVC_HCH_Header = header2.PK;
			consignment5.HVC_HCH_Header = header2.PK;
			consignment6.HVC_HCH_Header = header2.PK;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier2.PK;
			consignment3.HVC_OH_LastMileCarrier = carrier2.PK;
			consignment4.HVC_OH_LastMileCarrier = carrier3.PK;
			consignment5.HVC_OH_LastMileCarrier = carrier3.PK;
			consignment6.HVC_OH_LastMileCarrier = carrier3.PK;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(3);

			var sql = $"SELECT HDB_ConsignmentCount FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_ConsignmentCount for carrier1 to be 1", 1, result);

			sql = $"SELECT HDB_ConsignmentCount FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier2.PK}'";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_ConsignmentCount for carrier2 to be 2", 2, result);

			sql = $"SELECT HDB_ConsignmentCount FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier3.PK}'";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_ConsignmentCount for carrier3 to be 3", 3, result);
		}

		public void TestPopulateDeliveryByArea_MultiItemCount()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header.PK;
			consignment2.HVC_HCH_Header = header.PK;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier2.PK;

			var item1 = consignment1.Items.AddNew();
			var item2 = consignment1.Items.AddNew();
			var item3 = consignment2.Items.AddNew();
			var item4 = consignment2.Items.AddNew();
			var item5 = consignment2.Items.AddNew();

			item1.HVI_HVC_Consignment = consignment1.PK;
			item2.HVI_HVC_Consignment = consignment1.PK;
			item3.HVI_HVC_Consignment = consignment2.PK;
			item4.HVI_HVC_Consignment = consignment2.PK;
			item5.HVI_HVC_Consignment = consignment2.PK;
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			item3.HVI_JS_LoadedOnShipment = shipment.PK;
			item4.HVI_JS_LoadedOnShipment = shipment.PK;
			item5.HVI_JS_LoadedOnShipment = shipment.PK;

			var line1 = item1.Lines.AddNew();
			var line2 = item2.Lines.AddNew();
			var line3 = item3.Lines.AddNew();
			var line4 = item4.Lines.AddNew();
			var line5 = item5.Lines.AddNew();
			line1.HVS_Quantity = 1;
			line2.HVS_Quantity = 1;
			line3.HVS_Quantity = 1;
			line4.HVS_Quantity = 1;
			line5.HVS_Quantity = 1;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(2);

			var sql = $"SELECT HDB_MultiItemCount FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_MultiItemCount for carrier1 to be 2", 2, result);

			sql = $"SELECT HDB_MultiItemCount FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier2.PK}'";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_MultiItemCount for carrier2 to be 3", 3, result);
		}

		public void TestPopulateDeliveryByArea_ConsignmentVolumeSumInM3()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment5 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment6 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header1.PK;
			consignment4.HVC_HCH_Header = header1.PK;
			consignment5.HVC_HCH_Header = header1.PK;
			consignment6.HVC_HCH_Header = header1.PK;
			consignment1.HVC_ActualVolume = 10000;
			consignment2.HVC_ActualVolume = 1000;
			consignment3.HVC_ActualVolume = 4000;
			consignment4.HVC_ActualVolume = 30;
			consignment5.HVC_ActualVolume = 15;
			consignment6.HVC_ActualVolume = 2;
			consignment1.HVC_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			consignment2.HVC_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			consignment3.HVC_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			consignment4.HVC_VolumeUQ = Core.Constants.Volume.CubicMetres;
			consignment5.HVC_VolumeUQ = Core.Constants.Volume.CubicMetres;
			consignment6.HVC_VolumeUQ = Core.Constants.Volume.CubicMetres;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment3.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment4.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment5.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment6.HVC_OH_LastMileCarrier = carrier1.PK;

			var item1 = consignment1.Items.AddNew();
			var item2 = consignment2.Items.AddNew();
			var item3 = consignment3.Items.AddNew();
			var item4 = consignment4.Items.AddNew();
			var item5 = consignment5.Items.AddNew();
			var item6 = consignment6.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment1.PK;
			item2.HVI_JS_LoadedOnShipment = shipment1.PK;
			item3.HVI_JS_LoadedOnShipment = shipment1.PK;
			item4.HVI_JS_LoadedOnShipment = shipment1.PK;
			item5.HVI_JS_LoadedOnShipment = shipment1.PK;
			item6.HVI_JS_LoadedOnShipment = shipment1.PK;
			item1.HVI_ActualVolume = 10000;
			item2.HVI_ActualVolume = 1000;
			item3.HVI_ActualVolume = 4000;
			item4.HVI_ActualVolume = 30;
			item5.HVI_ActualVolume = 15;
			item6.HVI_ActualVolume = 2;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(1);

			var sql = $"SELECT HDB_ConsignmentVolumeSumInM3 FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var resultDecimal = (decimal)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_ConsignmentVolumeSumInM3 for carrier1 to be 62 M3", (decimal)62, resultDecimal);
		}

		public void TestPopulateDeliveryByArea_ConsignmentVolumeSumInM3_Fallback()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment5 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment6 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header1.PK;
			consignment4.HVC_HCH_Header = header1.PK;
			consignment5.HVC_HCH_Header = header1.PK;
			consignment6.HVC_HCH_Header = header1.PK;
			consignment1.HVC_ActualVolume = 0;
			consignment2.HVC_ActualVolume = 1000;
			consignment3.HVC_ActualVolume = 4000;
			consignment4.HVC_ActualVolume = 0;
			consignment5.HVC_ActualVolume = 15;
			consignment6.HVC_ActualVolume = 2;
			consignment1.HVC_ManifestedVolume = 3000;
			consignment2.HVC_ManifestedVolume = 0;
			consignment3.HVC_ManifestedVolume = 0;
			consignment4.HVC_ManifestedVolume = 10;
			consignment5.HVC_ManifestedVolume = 0;
			consignment6.HVC_ManifestedVolume = 0;
			consignment1.HVC_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			consignment2.HVC_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			consignment3.HVC_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			consignment4.HVC_VolumeUQ = Core.Constants.Volume.CubicMetres;
			consignment5.HVC_VolumeUQ = Core.Constants.Volume.CubicMetres;
			consignment6.HVC_VolumeUQ = Core.Constants.Volume.CubicMetres;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment3.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment4.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment5.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment6.HVC_OH_LastMileCarrier = carrier1.PK;

			var item1 = consignment1.Items.AddNew();
			var item2 = consignment2.Items.AddNew();
			var item3 = consignment3.Items.AddNew();
			var item4 = consignment4.Items.AddNew();
			var item5 = consignment5.Items.AddNew();
			var item6 = consignment6.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment1.PK;
			item2.HVI_JS_LoadedOnShipment = shipment1.PK;
			item3.HVI_JS_LoadedOnShipment = shipment1.PK;
			item4.HVI_JS_LoadedOnShipment = shipment1.PK;
			item5.HVI_JS_LoadedOnShipment = shipment1.PK;
			item6.HVI_JS_LoadedOnShipment = shipment1.PK;
			item1.HVI_ActualVolume = 0;
			item2.HVI_ActualVolume = 1000;
			item3.HVI_ActualVolume = 4000;
			item4.HVI_ActualVolume = 0;
			item5.HVI_ActualVolume = 15;
			item6.HVI_ActualVolume = 2;
			item1.HVI_ManifestedVolume = 3000;
			item2.HVI_ManifestedVolume = 0;
			item3.HVI_ManifestedVolume = 0;
			item4.HVI_ManifestedVolume = 10;
			item5.HVI_ManifestedVolume = 0;
			item6.HVI_ManifestedVolume = 0;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(1);

			var sql = $"SELECT HDB_ConsignmentVolumeSumInM3 FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var resultDecimal = (decimal)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_ConsignmentVolumeSumInM3 for carrier1 to be 35.000 M3", (decimal)35.000, resultDecimal);
		}

		public void TestPopulateDeliveryByArea_ConsignmentWeightSumInKG()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header1.PK;
			consignment4.HVC_HCH_Header = header1.PK;
			consignment1.HVC_ActualWeight = 15;
			consignment2.HVC_ActualWeight = 25;
			consignment3.HVC_ActualWeight = 50;
			consignment4.HVC_ActualWeight = 8000;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment3.HVC_OH_LastMileCarrier = carrier2.PK;
			consignment4.HVC_OH_LastMileCarrier = carrier2.PK;
			consignment1.HVC_WeightUQ = Core.Constants.Weight.Kilograms;
			consignment2.HVC_WeightUQ = Core.Constants.Weight.Kilograms;
			consignment3.HVC_WeightUQ = Core.Constants.Weight.Kilograms;
			consignment4.HVC_WeightUQ = Core.Constants.Weight.Grams;

			var item1 = consignment1.Items.AddNew();
			var item2 = consignment2.Items.AddNew();
			var item3 = consignment3.Items.AddNew();
			var item4 = consignment4.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment1.PK;
			item2.HVI_JS_LoadedOnShipment = shipment1.PK;
			item3.HVI_JS_LoadedOnShipment = shipment1.PK;
			item4.HVI_JS_LoadedOnShipment = shipment1.PK;
			item1.HVI_ActualWeight = 15;
			item2.HVI_ActualWeight = 25.85;
			item3.HVI_ActualWeight = 50;
			item4.HVI_ActualWeight = 8500;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(2);

			var sql = $"SELECT HDB_ConsignmentWeightSumInKG FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var resultDecimal = (decimal)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_ConsignmentWeightSumInKG for carrier1 to be 40.850 KG", (decimal)40.850, resultDecimal);

			sql = $"SELECT HDB_ConsignmentWeightSumInKG FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier2.PK}'";
			resultDecimal = (decimal)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_ConsignmentWeightSumInKG for carrier2 to be 58.5 KG", (decimal)58.5, resultDecimal);
		}

		public void TestPopulateDeliveryByArea_ConsignmentWeightSumInKG_Fallback()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header1.PK;
			consignment4.HVC_HCH_Header = header1.PK;
			consignment1.HVC_ActualWeight = 0;
			consignment2.HVC_ActualWeight = 10;
			consignment3.HVC_ActualWeight = 0;
			consignment4.HVC_ActualWeight = 0;
			consignment1.HVC_ManifestedWeight = 33;
			consignment2.HVC_ManifestedWeight = 100;
			consignment3.HVC_ManifestedWeight = 12.1;
			consignment4.HVC_ManifestedWeight = 1250;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment3.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment4.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment1.HVC_WeightUQ = Core.Constants.Weight.Kilograms;
			consignment2.HVC_WeightUQ = Core.Constants.Weight.Kilograms;
			consignment3.HVC_WeightUQ = Core.Constants.Weight.Kilograms;
			consignment4.HVC_WeightUQ = Core.Constants.Weight.Grams;

			var item1 = consignment1.Items.AddNew();
			var item2 = consignment2.Items.AddNew();
			var item3 = consignment3.Items.AddNew();
			var item4 = consignment4.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment1.PK;
			item2.HVI_JS_LoadedOnShipment = shipment1.PK;
			item3.HVI_JS_LoadedOnShipment = shipment1.PK;
			item4.HVI_JS_LoadedOnShipment = shipment1.PK;
			item1.HVI_ActualWeight = 0;
			item2.HVI_ActualWeight = 10;
			item3.HVI_ActualWeight = 0;
			item4.HVI_ActualWeight = 1250;
			item1.HVI_ManifestedWeight = 33;
			item2.HVI_ManifestedWeight = 100;
			item3.HVI_ManifestedWeight = 12.1;
			item4.HVI_ManifestedWeight = 1250;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(1);

			var sql = $"SELECT HDB_ConsignmentWeightSumInKG FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var resultDecimal = (decimal)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_ConsignmentWeightSumInKG for carrer1 should be 56.350 KG", (decimal)56.350, resultDecimal);
		}

		public void TestPopulateDeliveryByArea_IsDangerousCount()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var header2 = shipment2.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment5 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment6 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header1.PK;
			consignment4.HVC_HCH_Header = header2.PK;
			consignment5.HVC_HCH_Header = header2.PK;
			consignment6.HVC_HCH_Header = header2.PK;
			consignment1.HVC_UndgClass = "1.1D";
			consignment2.HVC_UndgClass = "1.1D";
			consignment3.HVC_UndgClass = "1.1D";
			consignment4.HVC_UndgClass = "1.1D";
			consignment5.HVC_UndgClass = ZString.Empty;
			consignment6.HVC_UndgClass = ZString.Empty;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment3.HVC_OH_LastMileCarrier = carrier1.PK;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(2);

			var sql = $"SELECT HDB_IsDangerousCount FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_IsDangerousCount for carrier1 to be 3", 3, result);

			sql = $"SELECT HDB_IsDangerousCount FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier IS NULL";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_IsDangerousCount where HDB_OH_LastMileCarrier to be 0", 1, result);
		}

		public void TestPopulateDeliveryByArea_AuthorityToLeaveCount()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var header2 = shipment2.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header2.PK;
			consignment4.HVC_HCH_Header = header2.PK;
			consignment1.HVC_AuthorityToLeave = ZBool.True;
			consignment2.HVC_AuthorityToLeave = ZBool.True;
			consignment3.HVC_AuthorityToLeave = ZBool.False;
			consignment4.HVC_AuthorityToLeave = ZBool.False;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier1.PK;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(2);

			var sql = $"SELECT HDB_AuthorityToLeaveCount FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_AuthorityToLeaveCount for carrier1 should be 2", 2, result);

			sql = $"SELECT HDB_AuthorityToLeaveCount FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier IS NULL";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_AuthorityToLeaveCount where HDB_OH_LastMileCarrier is null to be 0", 0, result);
		}

		public void TestPopulateDeliveryByArea_SignatureRequiredCount()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var header2 = shipment2.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment5 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header1.PK;
			consignment4.HVC_HCH_Header = header2.PK;
			consignment5.HVC_HCH_Header = header2.PK;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment3.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment1.HVC_IsSignatureRequired = ZBool.True;
			consignment2.HVC_IsSignatureRequired = ZBool.True;
			consignment3.HVC_IsSignatureRequired = ZBool.False;
			consignment4.HVC_IsSignatureRequired = ZBool.True;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(2);

			var sql = $"SELECT HDB_SignatureRequiredCount FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_SignatureRequiredCount for carrier1 to be 2", 2, result);

			sql = $"SELECT HDB_SignatureRequiredCount FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier IS NULL";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_SignatureRequiredCount where HDB_OH_LastMileCarrier os null to be 1", 1, result);
		}

		public void TestPopulateDeliveryByArea_SelfBookedCount()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header1.PK;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment3.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment1.HVC_IsSelfBooked = ZBool.False;
			consignment2.HVC_IsSelfBooked = ZBool.True;
			consignment3.HVC_IsSelfBooked = ZBool.True;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(1);

			var sql = $"SELECT HDB_SelfBookedCount FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_SelfBookedCount for carrier1 to be 2", 2, result);
		}

		public void TestPopulateDeliveryByArea_HasConsigneeInstructionCount()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment4.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var header2 = shipment2.GetOrCreateHVLVConsignmentHeader();
			var header3 = shipment3.GetOrCreateHVLVConsignmentHeader();
			var header4 = shipment4.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment3.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment4.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsigneeInstructions = "Has instructions";
			consignment2.HVC_ConsigneeInstructions = "Has instructions";
			consignment3.HVC_ConsigneeInstructions = "Has instructions";
			consignment4.HVC_ConsigneeInstructions = ZString.Empty;
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header2.PK;
			consignment3.HVC_HCH_Header = header3.PK;
			consignment4.HVC_HCH_Header = header4.PK;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment4.HVC_OH_LastMileCarrier = carrier2.PK;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(4);

			var sql = $"SELECT SUM(HDB_HasConsigneeInstructionCount) FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected sum of HDB_HasConsigneeInstructionCount where HDB_OH_LastMileCarrier carrier1 to be 2", 2, result);

			sql = $"SELECT SUM(HDB_HasConsigneeInstructionCount) FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier2.PK}'";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected sum of HDB_HasConsigneeInstructionCount where HDB_OH_LastMileCarrier carrier2 to be 0", 0, result);

			sql = $"SELECT SUM(HDB_HasConsigneeInstructionCount) FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier IS NULL";
			result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected HDB_HasConsigneeInstructionCount where HDB_OH_LastMileCarrier to be null", 1, result);
		}

		public void TestPopulateDeliveryByArea_ConsigneeAddressValidatedCount()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var header2 = shipment2.GetOrCreateHVLVConsignmentHeader();
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = header1.PK;
			consignment2.HVC_HCH_Header = header1.PK;
			consignment3.HVC_HCH_Header = header2.PK;
			consignment4.HVC_HCH_Header = header2.PK;
			consignment1.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment2.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment3.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment4.HVC_OH_LastMileCarrier = carrier1.PK;
			consignment1.HVC_ConsigneeAddressValidationStatus = "NYV";
			consignment2.HVC_ConsigneeAddressValidationStatus = "INV";
			consignment3.HVC_ConsigneeAddressValidationStatus = "VAD";

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			AssertHVLVDeliveryByAreaRowCount(2);

			var sql = $"SELECT SUM(HDB_ConsigneeAddressValidatedCount) FROM dbo.HVLVDeliveryByArea WHERE HDB_OH_LastMileCarrier = '{carrier1.PK}'";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected sum of HDB_ConsigneeAddressValidatedCount for carrier1 to be 2. We count only rows where HVC_ConsigneeAddressValidationStatus != NYV", 2, result);
		}

		void AssertHVLVDeliveryByAreaRowCount(int expectedRowCount)
		{
			var sql = $"SELECT COUNT (*) FROM dbo.HVLVDeliveryByArea";
			var result = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals("Expected this number of rows in HVLVDeliveryByArea", expectedRowCount, result);
		}

		#endregion

		#region MoveArchivedHVLVDataIntoEDocs

		public void TestMoveArchivedHVLVDataIntoEDocs_ThenArchiveConsignmentHeaderAndAllChildrenAsXML_eDocOnJobShipment()
		{
			using (SystemDataRegistry.Instance.ArchivingFileFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.FileFormats.XML))
			{
				var shipment = SetupShipmentAndRelatedDataForArchiving("S00001500");
				CreateRelatedHVLVData(shipment.GetOrCreateHVLVConsignmentHeader());

				Factory.Save();

				var logger = new TestArchiveLogger();
				var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
				var archiveSet = GetArchiveSet(shipment, config);

				AssertEquals("Expected shipment to have zero eDocs", 0, shipment.DocManagerInfo.AllEDocs.Count);

				var archiveAction = new HVLVArchiveAction(logger, archiveSet, config);
				archiveAction.Execute();

				var newFactory = new BusinessObjectFactory();
				shipment = newFactory.Load<ForwardingShipment>(shipment.PK);

				AssertEquals("Expected shipment to have one eDoc", 1, shipment.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Expected shipment to have a .xml.gz file in eDocs", "S00001500 - HVLV Archived Data.xml.gz", shipment.DocManagerInfo.AllEDocs.Cast<IeDoc>().Select(n => n.FileName).Single());
			}
		}

		public void TestMoveArchivedHVLVDataIntoEDocs_WhenArchivingMultipleShipments_ThenCreateOneXMLeDocPerShipment()
		{
			using (SystemDataRegistry.Instance.ArchivingFileFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.FileFormats.XML))
			{
				var shipment1 = SetupShipmentAndRelatedDataForArchiving("S00001550");
				var shipment2 = SetupShipmentAndRelatedDataForArchiving("S00001650");

				Factory.Save();

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("Expected shipment1 to have zero eDocs", 0, shipment1.DocManagerInfo.AllEDocs.Count);
					AssertEquals("Expected shipment2 to have zero eDocs", 0, shipment2.DocManagerInfo.AllEDocs.Count);
				});

				var logger = new TestArchiveLogger();
				var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
				var archiveSet1 = GetArchiveSet(shipment1, config);
				var archiveSet2 = GetArchiveSet(shipment2, config);

				var archiveAction1 = new HVLVArchiveAction(logger, archiveSet1, config);
				var archiveAction2 = new HVLVArchiveAction(logger, archiveSet2, config);

				archiveAction1.Execute();
				archiveAction2.Execute();

				var newFactory = new BusinessObjectFactory();
				shipment1 = newFactory.Load<ForwardingShipment>(shipment1.PK);
				shipment2 = newFactory.Load<ForwardingShipment>(shipment2.PK);

				AssertEquals("Expected shipment1 to have one eDoc", 1, shipment1.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Expected shipment2 to have one eDoc", 1, shipment2.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Expected shipment1 to have a .xml.gz file in eDocs", "S00001550 - HVLV Archived Data.xml.gz", shipment1.DocManagerInfo.AllEDocs.Cast<IeDoc>().Select(n => n.FileName).Single());
				AssertEquals("Expected shipment2 to have a .xml.gz file in eDocs", "S00001650 - HVLV Archived Data.xml.gz", shipment2.DocManagerInfo.AllEDocs.Cast<IeDoc>().Select(n => n.FileName).Single());
			}
		}

		public void TestMoveArchivedHVLVDataIntoEDocs_ThenArchiveConsignmentHeaderAndAllChildrenAsCSV_eDocOnJobShipment()
		{
			using (SystemDataRegistry.Instance.ArchivingFileFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.FileFormats.CSV))
			{
				var shipment = SetupShipmentAndRelatedDataForArchiving("S00001500");
				CreateRelatedHVLVData(shipment.GetOrCreateHVLVConsignmentHeader());

				Factory.Save();

				var logger = new TestArchiveLogger();
				var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
				var archiveSet = GetArchiveSet(shipment, config);

				AssertEquals("Shipment expected to have zero eDocs", 0, shipment.DocManagerInfo.AllEDocs.Count);

				var archiveAction = new HVLVArchiveAction(logger, archiveSet, config);
				archiveAction.Execute();

				var newFactory = new BusinessObjectFactory();
				shipment = newFactory.Load<ForwardingShipment>(shipment.PK);

				AssertEquals("Shipment expected to have one eDoc", 1, shipment.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Shipment should have a .csv.gz file in eDocs", "S00001500 - HVLV Archived Data.csv.gz", shipment.DocManagerInfo.AllEDocs.Cast<IeDoc>().Select(n => n.FileName).Single());
			}
		}

		public void TestMoveArchivedHVLVDataIntoEDocs_WhenArchivingMultipleShipments_ThenCreateOneCSVeDocPerShipment()
		{
			using (SystemDataRegistry.Instance.ArchivingFileFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.FileFormats.CSV))
			{
				var shipment1 = SetupShipmentAndRelatedDataForArchiving("S00001200");
				var shipment2 = SetupShipmentAndRelatedDataForArchiving("S00001300");

				Factory.Save();

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("Shipment1 expected to have zero eDocs", 0, shipment1.DocManagerInfo.AllEDocs.Count);
					AssertEquals("Shipment2 expected to have zero eDocs", 0, shipment2.DocManagerInfo.AllEDocs.Count);
				});

				var logger = new TestArchiveLogger();
				var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
				var archiveSet1 = GetArchiveSet(shipment1, config);
				var archiveSet2 = GetArchiveSet(shipment2, config);

				var archiveAction1 = new HVLVArchiveAction(logger, archiveSet1, config);
				var archiveAction2 = new HVLVArchiveAction(logger, archiveSet2, config);

				archiveAction1.Execute();
				archiveAction2.Execute();

				var newFactory = new BusinessObjectFactory();
				shipment1 = newFactory.Load<ForwardingShipment>(shipment1.PK);
				shipment2 = newFactory.Load<ForwardingShipment>(shipment2.PK);

				AssertEquals("Shipment1 expected to have one eDoc", 1, shipment1.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Shipment2 expected to have one eDoc", 1, shipment2.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Shipment1 should have a .csv.gz file in eDocs", "S00001200 - HVLV Archived Data.csv.gz", shipment1.DocManagerInfo.AllEDocs.Cast<IeDoc>().Select(n => n.FileName).Single());
				AssertEquals("Shipment2 should have a .csv.gz file in eDocs", "S00001300 - HVLV Archived Data.csv.gz", shipment2.DocManagerInfo.AllEDocs.Cast<IeDoc>().Select(n => n.FileName).Single());
			}
		}

		#endregion

		ForwardingShipment SetupShipmentAndRelatedDataForArchiving(string uniqueConsignRef)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment.JS_UniqueConsignRef = uniqueConsignRef;

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			consignmentHeader.HCH_JS_Shipment = shipment.PK;

			CreateRelatedHVLVData(consignmentHeader);

			return shipment;
		}

		void CreateRelatedHVLVData(HVLVConsignmentHeader consignmentHeader)
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_HVC_Consignment = consignment.PK;

			var line = item.Lines.AddNew();
			line.HVS_Quantity = 1;
		}

		ArchiveSet GetArchiveSet(ForwardingShipment shipment, ArchiveConfiguration config)
		{
			var systemDescriptor = new HARArchiveSystemDescriptor();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();

			var mainArchiveItem = new ArchiveItem(shipment.PKSchemaColumn, shipment.PK.ToGuid(), null, Guid.Empty, false, shipment.TablePrefix);
			var mainArchiveableType = new ArchiveableType(stageDescriptor.MainArchivePKColumn, stageDescriptor.MainArchiveNKColumn);
			var mainArchiveableTypeFilter = stageDescriptor.GetMainArchiveableFilter(config);

			return new ArchiveSet(systemDescriptor, stageDescriptor.Name, Guid.Empty, mainArchiveItem, mainArchiveableType, mainArchiveableTypeFilter, stageDescriptor.MainArchiveNKColumn.ToString());
		}
	}
}
