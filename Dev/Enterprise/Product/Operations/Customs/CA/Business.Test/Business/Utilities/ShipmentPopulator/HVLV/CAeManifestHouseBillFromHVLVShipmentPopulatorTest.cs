using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CA.Business.Testing
{
	class CAeManifestHouseBillFromHVLVShipmentPopulatorTest : TestCaseWithFactory
	{
		public void TestCusCAeMHHouseMappings()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;
			var master = GetTestMaster(consol.PK);

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);
			master.Factory.Save();

			var house1 = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 1");
			var house2 = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 2");

			CombineAssertions(() =>
			{
				AssertEquals("1st BW_MovementType", "24", house1.BW_MovementType);
				AssertEquals("1st BW_CBSAReleasePort", "0021", house1.BW_CBSAReleasePort);
				AssertEquals("1st BW_CBSAReleaseSubLocation", "0022", house1.BW_CBSAReleaseSubLocation);
				AssertEquals("1st BW_HouseCCN", "000020000", house1.BW_HouseCCN);
				AssertEquals("1st BW_Weight", 140m, house1.BW_Weight);
				AssertEquals("1st BW_WeightUQ", "KGM", house1.BW_WeightUQ);
				AssertEquals("1st BW_Volume", 60m, house1.BW_Volume);
				AssertEquals("1st BW_VolumeUQ", "MTQ", house1.BW_VolumeUQ);
				AssertEquals("1st BW_HandlingInstructions", "Leave at front", house1.BW_HandlingInstructions);

				AssertEquals("2nd BW_MovementType", "24", house2.BW_MovementType);
				AssertEquals("2nd BW_CBSAReleasePort", "0021", house2.BW_CBSAReleasePort);
				AssertEquals("2nd BW_CBSAReleaseSubLocation", "0022", house2.BW_CBSAReleaseSubLocation);
				AssertEquals("2nd BW_HouseCCN", "000020001", house2.BW_HouseCCN);
				AssertEquals("2nd BW_Weight", 40m, house2.BW_Weight);
				AssertEquals("2nd BW_WeightUQ", "KGM", house2.BW_WeightUQ);
				AssertEquals("2nd BW_Volume", 40m, house2.BW_Volume);
				AssertEquals("2nd BW_VolumeUQ", "MTQ", house2.BW_VolumeUQ);
				AssertEquals("2nd BW_HandlingInstructions", "Leave at back", house2.BW_HandlingInstructions);
			});
		}

		public void TestCusCAeMHHouseCCNGenerator_WhenShipmentCCNExists()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;
			var master = GetTestMaster(consol.PK);

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);
			master.Factory.Save();

			var house1 = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 1");
			var house2 = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 2");

			AssertEquals("when Shipment CCN exists", "000020000", house1.BW_HouseCCN);
			AssertEquals("when Shipment CCN exists", "000020001", house2.BW_HouseCCN);
		}

		public void TestCusCAeMHHouseCNNGenerator_WhenNoShipmentCNN_UsesMasterPrimaryCCN()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;
			var shipmentCCNtoRemove = shipment.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN);
			shipment.Numbers.RemoveAndDelete(shipmentCCNtoRemove);
			var emptyShipmentCCN = shipment.Numbers.AddNew();
			emptyShipmentCCN.CE_EntryType = "CCN";
			emptyShipmentCCN.CE_EntryNum = "";
			emptyShipmentCCN.CE_RN_NKCountryCode = "CA";
			var master = GetTestMaster(consol.PK);
			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);
			master.Factory.Save();

			var house1 = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 1");
			var house2 = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 2");

			AssertEquals("when Shipment CCN does not exist, but Master Primary CCN does", "000000000", house1.BW_HouseCCN);
			AssertEquals("when Shipment CCN does not exist, but Master Primary CCN does", "000000001", house2.BW_HouseCCN);
		}

		public void TestCusCAeMHHouseCNNGenerator_WhenNoShipmentAndMasterPrimaryCCN_UsesSuffixOnly()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;
			var shipmentCCNtoRemove = shipment.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN);
			var consolCCNtoRemove = consol.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN);

			shipment.Numbers.RemoveAndDelete(shipmentCCNtoRemove);
			consol.Numbers.RemoveAndDelete(consolCCNtoRemove);

			var emptyShipmentCCN = shipment.Numbers.AddNew();
			emptyShipmentCCN.CE_EntryType = "CCN";
			emptyShipmentCCN.CE_EntryNum = "";
			emptyShipmentCCN.CE_RN_NKCountryCode = "CA";

			var emptyConsolCCN = consol.Numbers.AddNew();
			emptyConsolCCN.CE_EntryType = "CCN";
			emptyConsolCCN.CE_EntryNum = "";
			emptyConsolCCN.CE_RN_NKCountryCode = "CA";

			var master = GetTestMaster(consol.PK);
			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);
			master.Factory.Save();

			var house1 = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 1");
			var house2 = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 2");

			AssertEquals("when Shipment CCN and Primay Master CCN does not exist", "0000", house1.BW_HouseCCN);
			AssertEquals("when Shipment CCN and Primay Master CCN does not exist", "0001", house2.BW_HouseCCN);
		}

		public void TestCusCAeMHItemMappings()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;
			var master = GetTestMaster(consol.PK);

			var consignment1 = shipment.HVLVConsignments.First(c => c.HVC_WaybillNumber == "Test Waybill 1");
			var line1a = consignment1.Items.OfType<IHVLVItemForDocument>().SelectMany(item => item.Lines.OfType<IHVLVItemLine>()).First(x => x.HVS_GoodsDescription == "Test Item Line 1a");
			line1a.HVS_Quantity = 13;
			line1a.HVS_FormattedDestinationTariff = "111.222.33";

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);

			var house1 = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 1");
			AssertEquals(5, house1.Items.Count);

			var item1a = house1.Items.Single(x => x.BX_Description == "Test Item Line 1a");

			CombineAssertions(() =>
			{
				AssertEquals("BX_Quantity", (decimal)line1a.HVS_Quantity, item1a.BX_Quantity);
				AssertEquals("BX_HSCode", line1a.HVS_FormattedOriginTariff, item1a.BX_HSCode);
				AssertEquals("BX_Description", line1a.HVS_GoodsDescription, item1a.BX_Description);
			});

			var house2 = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 2");
			AssertEquals(1, house2.Items.Count);

			var item4 = house2.Items.Single(x => x.BX_Description == "Test Item Line 4");
			var consignment2 = shipment.HVLVConsignments.First(c => c.HVC_WaybillNumber == "Test Waybill 2");
			var line4 = consignment2.Items.OfType<IHVLVItemForDocument>().SelectMany(item => item.Lines.OfType<IHVLVItemLine>()).Single();

			CombineAssertions(() =>
			{
				AssertEquals("BX_Quantity", (decimal)line4.HVS_Quantity, item4.BX_Quantity);
				AssertEquals("BX_HSCode", line4.HVS_FormattedOriginTariff, item4.BX_HSCode);
				AssertEquals("BX_Description", line4.HVS_GoodsDescription, item4.BX_Description);
			});
		}

		public void TestCusCAeMHContainerMappings()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;
			var master = GetTestMaster(consol.PK);

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);

			var container1 = master.Containers.Single(x => x.BQ_ContainerNumber == "TESTCONTAINER 1");
			var container2 = master.Containers.Single(x => x.BQ_ContainerNumber == "TESTCONTAINER 2");

			CombineAssertions(() =>
			{
				AssertEquals("1st BQ_RC_NKContainerType", "20XX", container1.BQ_RC_NKContainerType);
				AssertEquals("1st BQ_Seal1", "TestSeal", container1.BQ_Seal1);

				AssertEquals("2nd BQ_RC_NKContainerType", "20XY", container2.BQ_RC_NKContainerType);
				AssertEquals("2nd BQ_Seal1", "TestSeal", container2.BQ_Seal1);
			});
		}

		public void TestCAeMHDocAddressMappings_WhenShipperIsOrganization()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;
			var master = GetTestMaster(consol.PK);

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = "TSTSH";
			shipper.OH_FullName = "Test Shipper";

			var address = shipper.Addresses.AddNew();
			address.OA_Address1 = "No.123 abc street";

			var contact = shipper.Contacts.AddNew();
			contact.OC_OA_OrgAddress = address.PK;
			contact.OC_ContactName = "Test Contact";

			var consignment = shipment.HVLVConsignments.First();
			consignment.HVC_OA_ShipperAddress = address.PK;
			consignment.HVC_ShipperContact = contact.OC_ContactName;

			Factory.Save();

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);

			var house = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 1");
			AssertNotNull(house);

			var houseShipper = house.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			CombineAssertions(() =>
			{
				AssertEquals("No.123 abc street", houseShipper.Address1);
				AssertEquals("Test Contact", houseShipper.E2_Contact);
			});
		}

		public void TestCAeMHDocAddressMappings_WhenShipperIsNotOrganization()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;
			var master = GetTestMaster(consol.PK);

			var consignment = shipment.HVLVConsignments.First();
			consignment.HVC_ShipperAddress1 = "No.123 abc street";
			consignment.HVC_ShipperContact = "Test Contact";

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);

			var house = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 1");
			AssertNotNull(house);

			var houseShipper = house.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			CombineAssertions(() =>
			{
				AssertEquals("No.123 abc street", houseShipper.Address1);
				AssertEquals("Test Contact", houseShipper.E2_Contact);
			});
		}

		public void TestCAeMHDocAddressMappings_WhenConsigneeIsOrganization()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;
			var master = GetTestMaster(consol.PK);

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TSTCN";
			consignee.OH_FullName = "Test Consignee";

			var address = consignee.Addresses.AddNew();
			address.OA_Address1 = "No.123 abc street";

			var contact = consignee.Contacts.AddNew();
			contact.OC_OA_OrgAddress = address.PK;
			contact.OC_ContactName = "Test Contact";

			var consignment = shipment.HVLVConsignments.First();
			consignment.HVC_OA_ConsigneeAddress = address.PK;
			consignment.HVC_ConsigneeContact = contact.OC_ContactName;

			Factory.Save();

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);

			var house = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 1");
			AssertNotNull(house);

			var houseConsignee = house.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			CombineAssertions(() =>
			{
				AssertEquals("No.123 abc street", houseConsignee.Address1);
				AssertEquals("Test Contact", houseConsignee.E2_Contact);
			});
		}

		public void TestCAeMHDocAddressMappings_WhenConsigneeIsNotOrganization()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;
			var master = GetTestMaster(consol.PK);

			var consignment = shipment.HVLVConsignments.First();
			consignment.HVC_ConsigneeAddress1 = "No.123 abc street";
			consignment.HVC_ConsigneeContact = "Test Contact";

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);

			var house = master.HouseBills.First(x => x.BW_HouseBill == "Test Waybill 1");
			AssertNotNull(house);

			var houseConsignee = house.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			CombineAssertions(() =>
			{
				AssertEquals("No.123 abc street", houseConsignee.Address1);
				AssertEquals("Test Contact", houseConsignee.E2_Contact);
			});
		}

		public void TestCusCAeMHMasterMappings_CreateTRFLogsForMasterAndShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment1.JS_UniqueConsignRef = "S00001000";
			var shipmentTRFLog = shipment1.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode);
			Assert("Precondition : no TRF log on shipment", !shipmentTRFLog.Any());

			var master = GetTestMaster(consol.PK);
			var masterTRFLogs = master.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode);
			Assert("Precondition : no TRF log on master", !masterTRFLogs.Any());

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment1);
			Factory.Save();

			AssertCreateTRFLogForMasterAndShipment("add log for master and shipment", new ForwardingShipment[] { shipment1 });

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_UniqueConsignRef = "S00002000";

			populator.Populate(shipment1);
			populator.Populate(shipment2);
			Factory.Save();

			AssertCreateTRFLogForMasterAndShipment("add log for new added shipment but won't add duplicate log for existing shipment", new ForwardingShipment[] { shipment1, shipment2 });

			var newPopulator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			newPopulator.Populate(shipment1);
			newPopulator.Populate(shipment2);
			Factory.Save();

			AssertCreateTRFLogForMasterAndShipment("differnet populator won't add duplicate log", new ForwardingShipment[] { shipment1, shipment2 });

			void AssertCreateTRFLogForMasterAndShipment(string message, IEnumerable<ForwardingShipment> shipments)
			{
				masterTRFLogs = master.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode);

				CombineAssertions(message, () =>
				{
					AssertEquals("should add log for each shipment", shipments.Count(), masterTRFLogs.Count());

					foreach (var shipment in shipments)
					{
						Assert(masterTRFLogs.Any(log => log.Parameters.Count == 2 &&
											log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] == "HVL" &&
											log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber] == shipment.JobNumber));

						shipmentTRFLog = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode);
						AssertEquals(1, shipmentTRFLog.Count());

						var trfLog = shipmentTRFLog.First();
						AssertEquals(2, trfLog.Parameters.Count);
						AssertEquals("MAN", trfLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
						AssertEquals(master.BP_MessageReference, trfLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber]);
					}
				});
			}
		}

		public void TestCusCAeMHMasterMappings_CreateHousebill()
		{
			var shipment1 = GetTestHVLVShipment();
			var consol = shipment1.Consols.FirstOrDefault() as ForwardingConsol;
			var master = GetTestMaster(consol.PK);

			AssertEquals("precondtion: there are no house bills on master.", 0, master.HouseBills.Count);

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment1);

			AssertEquals("2 house bills have been created (as the test HVLV shipment makes 2 consignments).", 2, master.HouseBills.Count);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment2.PK;

			Factory.Save();

			populator.Populate(shipment1);
			populator.Populate(shipment2);
			AssertEquals("add housebill for shipment2 but won't add duplicate housebill for shipment1.", 3, master.HouseBills.Count);

			Factory.Save();

			populator.Populate(shipment1);
			populator.Populate(shipment2);
			AssertEquals("Would not create duplicate housebill after saving data.", 3, master.HouseBills.Count);

			var newPopulator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			newPopulator.Populate(shipment1);
			newPopulator.Populate(shipment2);
			AssertEquals("different populator won't create duplicate housebill.", 3, master.HouseBills.Count);
		}

		public void TestCusCAeMHMasterMappings_AddMappingIfConvertedToMaster_TRFLogOnMaster()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;
			var master = GetTestMaster(consol.PK);

			AssertEquals("Precondtion: there are no house bills on master.", 0, master.HouseBills.Count);

			master.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, shipment.JobNumber),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, ShipmentTypes.HighVolumeLowValue)
			});

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);

			master.Factory.Save();
			AssertEquals("No house bills are created as shipment is mapped when a TRF log exists on master.", 0, master.HouseBills.Count);
		}

		public void TestCAeManifestFromHVLVShipmentCreator_WhenFactorySavedSuccessful_ThenRefreshEnabledIsTrue()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;
			var master = GetTestMaster(consol.PK);
			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);

			populator.factory_SavingForTest();
			Assert("RefreshEnabled should be false", !shipment.Factory.RefreshEnabled);
			populator.factory_SavedForTest();
			Assert("RefreshEnabled should be true", shipment.Factory.RefreshEnabled);
		}

		public void TestPopulate_ShouldPerformNoMatchingForExistingHouseBill()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.First();
			var master = GetTestMaster(consol.PK);
			var consignment = shipment.HVLVConsignments.First();
			var existingHouse = master.HouseBills.AddNew();
			existingHouse.BW_ParentID = consignment.PK;

			AssertEquals("pre-condition", 1, master.HouseBills.Count);
			AssertEquals("pre-condition", 2, shipment.HVLVConsignments.Count());

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);

			AssertEquals("Should have a total of 3 house bills (1 existing + 2 new)", 3, master.HouseBills.Count);
			AssertEquals("Should have 2 house bills linked to same consignment", 2, master.HouseBills.Count(x => x.BW_ParentID == consignment.PK));
		}

		public void TestPopulate_WhenPopulatingAddressCollection_ShouldNotCallRefreshBindingOnHouseBill()
		{
			var shipment = GetTestHVLVShipment();
			var consol = shipment.Consols.First();
			var master = GetTestMaster(consol.PK);

			var populator = new CAeManifestHouseBillFromHVLVShipmentPopulator(master);
			populator.Populate(shipment);

			AssertEquals("Populating address collection should not call refresh binding on house bill", false, populator.RefreshBindingCalledOnHouseByAddress);
		}

		#region Implementations
		ForwardingShipment GetTestHVLVShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "123456";
			consol.JK_RL_NKDischargePort = "CA001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2021, 10, 3);
			consol.JK_OA_UnpackDepotAddress = orgCFS.MainAddress.PK;

			var number = consol.Numbers.AddNew();
			number.CE_EntryType = "CCN";
			number.CE_EntryNum = "00000";
			number.CE_RN_NKCountryCode = "CA";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.JC_ContainerNum = "TestContainer 1";
			container1.JC_SealNum = "TestSeal";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.JC_ContainerNum = "TestContainer 2";
			container2.JC_SealNum = "TestSeal";

			var refContainer1 = container1.RefContainer_List.AddNew();
			refContainer1.RC_Code = "20XX";
			container1.JC_RC = refContainer1.PK;

			var refContainer2 = container2.RefContainer_List.AddNew();
			refContainer2.RC_Code = "20XY";
			container2.JC_RC = refContainer2.PK;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "CA001";
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ATA = ZDateTime.Today.AddDays(1);
			transport.JW_VoyageFlight = "TestVoyage";
			transport.JW_OA_CarrierAddress = carrierAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_HouseBill = "01234";

			var shipmentCCN = shipment.Numbers.AddNew();
			shipmentCCN.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			shipmentCCN.CE_EntryNum = "00002";

			var consignment1 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "Test Waybill 1";
			consignment1.HVC_ManifestedWeight = 10;
			consignment1.HVC_WeightUQ = "KG";
			consignment1.HVC_ManifestedVolume = 10;
			consignment1.HVC_VolumeUQ = "M3";
			consignment1.HVC_ConsigneeInstructions = "Leave at front";

			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_WaybillNumber = "Test Waybill 2";
			consignment2.HVC_ManifestedWeight = 20;
			consignment2.HVC_WeightUQ = "KG";
			consignment2.HVC_ManifestedVolume = 20;
			consignment2.HVC_VolumeUQ = "M3";
			consignment2.HVC_ConsigneeInstructions = "Leave at back";

			var item1 = (IHVLVItemForDocument)consignment1.Items.AddNew();
			item1.HVI_CurrentBarcode = "TestItem1";
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			item1.HVI_F3_NKPackType = PkgUnit.Pallet;
			item1.HVI_ManifestedWeight = 10;
			item1.HVI_ManifestedVolume = 10;
			item1.HVI_ContainerNumber = "TestContainer 1";

			var item2 = (IHVLVItemForDocument)consignment1.Items.AddNew();
			item2.HVI_CurrentBarcode = "TestItem2";
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_F3_NKPackType = PkgUnit.Tube;
			item2.HVI_ManifestedWeight = 20;
			item2.HVI_ManifestedVolume = 20;
			item2.HVI_ContainerNumber = "TestContainer 1";

			var item3 = (IHVLVItemForDocument)consignment1.Items.AddNew();
			item3.HVI_CurrentBarcode = "TestItem3";
			item3.HVI_JS_LoadedOnShipment = shipment.PK;
			item3.HVI_F3_NKPackType = PkgUnit.Package;
			item3.HVI_ManifestedWeight = 30;
			item3.HVI_ManifestedVolume = 30;
			item3.HVI_ContainerNumber = "TestContainer 2";

			var item4 = (IHVLVItemForDocument)consignment2.Items.AddNew();
			item4.HVI_CurrentBarcode = "TestItem4";
			item4.HVI_JS_LoadedOnShipment = shipment.PK;
			item4.HVI_F3_NKPackType = PkgUnit.Reel;
			item4.HVI_ManifestedWeight = 40;
			item4.HVI_ManifestedVolume = 40;
			item4.HVI_ContainerNumber = "TestContainer 2";

			var itemLine1a = (IHVLVItemLine)item1.Lines.AddNew();
			itemLine1a.HVS_FormattedDestinationTariff = "1234.56";
			itemLine1a.HVS_CustomsValue = 1.1m;
			itemLine1a.HVS_GrossWeight = 10m;
			itemLine1a.HVS_WeightUnit = "KG";
			itemLine1a.HVS_Quantity = 1;
			itemLine1a.HVS_GoodsDescription = "Test Item Line 1a";
			itemLine1a.HVS_RN_NKOriginCountryCode = "AU";

			var itemLine1b = (IHVLVItemLine)item1.Lines.AddNew();
			itemLine1b.HVS_FormattedDestinationTariff = "1234.56";
			itemLine1b.HVS_CustomsValue = 1.5m;
			itemLine1b.HVS_GrossWeight = 20m;
			itemLine1b.HVS_WeightUnit = "KG";
			itemLine1b.HVS_Quantity = 2;
			itemLine1b.HVS_GoodsDescription = "Test Item Line 1b";
			itemLine1b.HVS_RN_NKOriginCountryCode = "AU";

			var itemLine2 = (IHVLVItemLine)item2.Lines.AddNew();
			itemLine2.HVS_FormattedDestinationTariff = "6543.21";
			itemLine2.HVS_CustomsValue = 2.9m;
			itemLine2.HVS_GrossWeight = 20m;
			itemLine2.HVS_WeightUnit = "KG";
			itemLine2.HVS_Quantity = 1;
			itemLine2.HVS_GoodsDescription = "Test Item Line 2";
			itemLine2.HVS_RN_NKOriginCountryCode = "AU";

			var itemLine3a = (IHVLVItemLine)item3.Lines.AddNew();
			itemLine3a.HVS_FormattedDestinationTariff = "1357.99";
			itemLine3a.HVS_CustomsValue = 2.5m;
			itemLine3a.HVS_GrossWeight = 30m;
			itemLine3a.HVS_WeightUnit = "KG";
			itemLine3a.HVS_Quantity = 1;
			itemLine3a.HVS_GoodsDescription = "Test Item Line 3a";
			itemLine3a.HVS_RN_NKOriginCountryCode = "AU";

			var itemLine3b = (IHVLVItemLine)item3.Lines.AddNew();
			itemLine3b.HVS_FormattedDestinationTariff = "1357.99";
			itemLine3b.HVS_CustomsValue = 6m;
			itemLine3b.HVS_GrossWeight = 60m;
			itemLine3b.HVS_WeightUnit = "KG";
			itemLine3b.HVS_Quantity = 2;
			itemLine3b.HVS_GoodsDescription = "Test Item Line 3b";
			itemLine3b.HVS_RN_NKOriginCountryCode = "AU";

			var itemLine4 = (IHVLVItemLine)item4.Lines.AddNew();
			itemLine4.HVS_FormattedDestinationTariff = "2468.00";
			itemLine4.HVS_CustomsValue = 4m;
			itemLine4.HVS_GrossWeight = 40m;
			itemLine4.HVS_WeightUnit = "KG";
			itemLine4.HVS_Quantity = 1;
			itemLine4.HVS_GoodsDescription = "Test Item Line 4";
			itemLine4.HVS_RN_NKOriginCountryCode = "AU";

			Factory.Save();

			return shipment;
		}

		CusCAeMHMaster GetTestMaster(ZGuid consolPK)
		{
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_ParentID = consolPK;
			master.BP_ParentTableCode = JobConsolSchema.Constants.Prefix;
			master.BP_ModeOfTransport = "AIR";
			master.BP_MasterBill = "123456";
			master.BP_MasterHouseBill = "01234";
			master.BP_PrimaryCCN = "00000";
			master.BP_RL_NKDiscPort = "CA001";
			master.BP_CBSACarrierCode = "8888";
			master.BP_CBSADischargePort = "0041";
			master.BP_ATA = ZDateTime.Today.AddDays(1);
			master.BP_CBSADischargeSubLocation = "0042";

			return master;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var branch = GlbBranch.CurrentBranch;
			branch.GB_RL_NKHomePort = "AUBNE";

			var canada = Factory.Load<RefCountry>(CountryGuids.Canada);
			shpCFS = Factory.NewWithValidTestData<OrgHeader>();
			var ctoCusCOC = shpCFS.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0011", canada);
			ctoCusCOC.OK_OA_PremisesAddress = shpCFS.MainAddress.PK;
			var ctoCusCCP = shpCFS.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0012", canada);
			ctoCusCCP.OK_OA_PremisesAddress = shpCFS.MainAddress.PK;

			orgCFS = Factory.NewWithValidTestData<OrgHeader>();
			var cfsCusCOC = orgCFS.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0021", canada);
			cfsCusCOC.OK_OA_PremisesAddress = orgCFS.MainAddress.PK;
			var cfsCusCCP = orgCFS.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0022", canada);
			cfsCusCCP.OK_OA_PremisesAddress = orgCFS.MainAddress.PK;

			var arrivalAt = Factory.NewWithValidTestData<OrgHeader>();
			arrivalAtAddress = arrivalAt.MainAddress;
			var arrCusCOC = arrivalAt.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0031", canada);
			arrCusCOC.OK_OA_PremisesAddress = arrivalAtAddress.PK;
			var arrCusCCP = arrivalAt.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0032", canada);
			arrCusCCP.OK_OA_PremisesAddress = arrivalAtAddress.PK;

			var officeHeader = Factory.NewWithValidTestData<OrgHeader>();
			carrierAddress = officeHeader.MainAddress;
			var cusCOCOffice = officeHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "0041", canada);
			cusCOCOffice.OK_OA_PremisesAddress = carrierAddress.PK;
			var cusCCPOffice = officeHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "0042", canada);
			cusCCPOffice.OK_OA_PremisesAddress = carrierAddress.PK;
			var airCTO = officeHeader.CarrierAppointedAgentPorts_AirCTO.AddNew();
			airCTO.O5_OA_AgentOfficeAddress = officeHeader.MainAddress.PK;
			airCTO.O5_PortOrCountry = "CA";

			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.RL_Code = "CA001";

			var locoMapAir = Factory.New<RefLocoMap>();
			locoMapAir.RY_LocalPortCode = "1111";
			locoMapAir.RY_RL_NKLocoPort = "CA001";
			locoMapAir.RY_RN = CountryGuids.Canada;
			locoMapAir.RY_SystemUsage = LocoMapSystemUsageList.Codes.Air;
			var locoMapSea = Factory.New<RefLocoMap>();
			locoMapSea.RY_LocalPortCode = "2222";
			locoMapSea.RY_RL_NKLocoPort = "CA001";
			locoMapSea.RY_RN = CountryGuids.Canada;
			locoMapSea.RY_SystemUsage = LocoMapSystemUsageList.Codes.Sea;
			var locoMapSub = Factory.New<RefLocoMap>();
			locoMapSub.RY_LocalPortCode = "SUB1";
			locoMapSub.RY_RL_NKLocoPort = "CA001";
			locoMapSub.RY_RN = CountryGuids.Canada;
			locoMapSub.RY_SystemUsage = LocoMapSystemUsageList.Codes.Sub;

			var unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_Code = "CA002";
			var locoMapAir2 = Factory.New<RefLocoMap>();
			locoMapAir2.RY_LocalPortCode = "3333";
			locoMapAir2.RY_RL_NKLocoPort = "CA002";
			locoMapAir2.RY_RN = CountryGuids.Canada;
			locoMapAir2.RY_SystemUsage = LocoMapSystemUsageList.Codes.Air;
			var locoMapSea2 = Factory.New<RefLocoMap>();
			locoMapSea2.RY_LocalPortCode = "4444";
			locoMapSea2.RY_RL_NKLocoPort = "CA002";
			locoMapSea2.RY_RN = CountryGuids.Canada;
			locoMapSea2.RY_SystemUsage = LocoMapSystemUsageList.Codes.Sea;
			Factory.Save();
		}

		OrgHeader shpCFS;
		OrgHeader orgCFS;
		OrgAddress carrierAddress;
		OrgAddress arrivalAtAddress;

		#endregion
	}
}
