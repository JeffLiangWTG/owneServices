using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class HouseBillSynchroniserTest : SynchroniserTestCase
	{
		public void TestColoadMasterShipment_ChangedWithDeletedBusinessObject()
		{
			var chouse = Factory.NewWithValidTestData<CusHouseForTest>();
			Factory.Save();
			var rowFactory = new RowFactory(Db.DatabaseName);
			var row = rowFactory.LoadFromPK(CusSCAHouseSchema.Constants.TableName, chouse.PK);
			rowFactory.Save();
			chouse.FillWithValidTestData();
			Factory.Save();

			var consol = CreateFCLConsol();
			var sCASynchroniser = GetSeaCargoSynchroniser(consol);
			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();

			var masterShipment = consol.Shipments.AddNew();
			masterShipment.FillWithValidTestData();
			masterShipment.JS_HouseBill = "wot";

			AssertNotEquals("MasterShipment's Housebill is empty - ColoadMasterShipment will not reach set method!", "", masterShipment.JS_HouseBill);

			var masterGUID = masterShipment.PK;
			Factory.Save();

			shipment.JS_JS_ColoadMasterShipment = masterGUID;

			AssertNotNull("Master Shipment is null! ColoadMasterShipment was not changed!", shipment.CoLoadMasterShipment);

			var synchroniser = new CMRHouseBillSynchroniser(chouse, shipment);
			chouse.DeleteRowForTest();
			synchroniser.ColoadMasterShipment_Changed(null, null);
		}

		public void TestSynchroniser()
		{
			var consol = CreateFCLConsol();
			var sCASynchroniser = GetSeaCargoSynchroniser(consol);
			var shipment = consol.Shipments.AddNew();

			var oceanBill = sCASynchroniser.OceanBill;
			var houseBill = oceanBill.HouseBills.AddNew();

			var synchroniser = GetHouseBillSynchroniser(houseBill, shipment);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			shipment.JS_HouseBill = TestHouseBillNumber;
			AssertEquals("HouseBill HouseBillNumber", TestHouseBillNumber, houseBill.CA_HouseBill);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("HouseBill MasterHouse", true, houseBill.CA_IsMasterHouse);

			shipment.JS_RL_NKOrigin = TestRL_NK_PortOfOrigin;
			AssertEquals("HouseBill CA_RL_NKPortOfOrigin", TestRL_NK_PortOfOrigin, houseBill.CA_RL_NK_PortOfOrigin);
			AssertEquals("HouseBill CA_RN_NKGoodsOrigin", TestRL_NK_PortOfOrigin.Substring(0, 2), houseBill.CA_RN_NKGoodsOrigin);

			shipment.JS_RL_NKDestination = TestRL_NK_PortOfDestination;
			AssertEquals("HouseBill CA_RL_NKPortOfOrigin", TestRL_NK_PortOfDestination, houseBill.CA_RL_NK_PortOfDestination);

			shipment.ConsigneePK = TestOH_Consignee;
			AssertEquals(TestOH_Consignee, houseBill.Consignee.PK);
			AssertEquals("Synchroniser Consignee", houseBill.CA_ConsigneeName);
			AssertEquals("1addr", houseBill.CA_ConsigneeAddress1);
			AssertEquals("2addr", houseBill.CA_ConsigneeAddress2);
			AssertEquals("555", houseBill.CA_ConsigneeFax);
			AssertEquals("333", houseBill.CA_ConsigneePhone);
			AssertEquals("6768", houseBill.CA_ConsigneePostcode);
			AssertEquals("barcity HEH", houseBill.CA_ConsigneeSuburb);

			shipment.ConsignorPK = TestOH_Consignor;
			AssertEquals(TestOH_Consignor, houseBill.Consignor.PK);
			AssertEquals("Synchroniser Consignor", houseBill.CA_ConsignorName);
			AssertEquals("addr1", houseBill.CA_ConsignorAddress1);
			AssertEquals("addr2", houseBill.CA_ConsignorAddress2);
			AssertEquals("3234", houseBill.CA_ConsignorPostcode);
			AssertEquals("foocity BOO", houseBill.CA_ConsignorSuburb);

			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = TestOH_NotifyParty;
			AssertEquals("HouseBill Notify Party", TestOH_NotifyParty, houseBill.CA_OH_Notify);

			var deliverTo = CreateOrgHeader("Deliver To");
			deliverTo.MainAddress.OA_Address1 = "DLV 1addr";
			deliverTo.MainAddress.OA_Address2 = "DLV 2addr";
			deliverTo.MainAddress.OA_Fax = "444";
			deliverTo.MainAddress.OA_Phone = "222";
			deliverTo.MainAddress.OA_PostCode = "1212";
			deliverTo.MainAddress.OA_City = "DLV barcity";
			deliverTo.MainAddress.OA_State = "HEH";

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliverTo.MainAddress.PK;
			AssertEquals(deliverTo.PK, houseBill.Consignee.PK);
			AssertEquals("Deliver To", houseBill.CA_ConsigneeName);
			AssertEquals("DLV 1addr", houseBill.CA_ConsigneeAddress1);
			AssertEquals("DLV 2addr", houseBill.CA_ConsigneeAddress2);
			AssertEquals("444", houseBill.CA_ConsigneeFax);
			AssertEquals("222", houseBill.CA_ConsigneePhone);
			AssertEquals("1212", houseBill.CA_ConsigneePostcode);
			AssertEquals("DLV barcity HEH", houseBill.CA_ConsigneeSuburb);
		}

		public void TestSynchroniserWithJobDocAddresses()
		{
			var consol = CreateFCLConsol();
			var sCASynchroniser = GetSeaCargoSynchroniser(consol);
			var shipment = consol.Shipments.AddNew();

			var oceanBill = sCASynchroniser.OceanBill;
			var houseBill = oceanBill.HouseBills.AddNew();

			var synchroniser = GetHouseBillSynchroniser(houseBill, shipment);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = TestOH_NotifyParty;
			AssertEquals("HouseBill Notify Party", TestOH_NotifyParty, houseBill.CA_OH_Notify);

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "Doc Consignee";
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "CNEE Address 1";
			shipment.ConsigneeDocumentaryAddress.E2_Address2 = "CNEE Address 2";
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			shipment.ConsigneeDocumentaryAddress.E2_Fax = "1212";
			shipment.ConsigneeDocumentaryAddress.E2_Phone = "2323";
			shipment.ConsigneeDocumentaryAddress.E2_Postcode = "3434";
			shipment.ConsigneeDocumentaryAddress.E2_State = "CNEEState";
			shipment.ConsigneeDocumentaryAddress.E2_City = "CNEECity";
			AssertEquals("HouseBill Consignee", ZGuid.Empty, houseBill.CA_OH_Consignee);
			AssertEquals("HouseBill Consignee", "Doc Consignee", houseBill.CA_ConsigneeName);
			AssertEquals("HouseBill Consignee", "CNEE Address 1", houseBill.CA_ConsigneeAddress1);
			AssertEquals("HouseBill Consignee", "CNEE Address 2", houseBill.CA_ConsigneeAddress2);
			AssertEquals("HouseBill Consignee", "AU", houseBill.CA_RN_NKConsigneeCountryCode);
			AssertEquals("HouseBill Consignee", "1212", houseBill.CA_ConsigneeFax);
			AssertEquals("HouseBill Consignee", "2323", houseBill.CA_ConsigneePhone);
			AssertEquals("HouseBill Consignee", "3434", houseBill.CA_ConsigneePostcode);
			AssertEquals("HouseBill Consignee", "", houseBill.CA_ConsigneeState);
			AssertEquals("HouseBill Consignee", "CNEECity CNEEState", houseBill.CA_ConsigneeSuburb);

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "Doc Consignor";
			shipment.ConsignorDocumentaryAddress.E2_Address1 = "CNOR Address 1";
			shipment.ConsignorDocumentaryAddress.E2_Address2 = "CNOR Address 2";
			shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_Postcode = "4545";
			shipment.ConsignorDocumentaryAddress.E2_State = "CNORState";
			shipment.ConsignorDocumentaryAddress.E2_City = "CNORCity";
			AssertEquals("HouseBill Consignor", ZGuid.Empty, houseBill.CA_OH_Consignor);
			AssertEquals("HouseBill Consignor", "Doc Consignor", houseBill.CA_ConsignorName);
			AssertEquals("HouseBill Consignor", "CNOR Address 1", houseBill.CA_ConsignorAddress1);
			AssertEquals("HouseBill Consignor", "CNOR Address 2", houseBill.CA_ConsignorAddress2);
			AssertEquals("HouseBill Consignor", "AU", houseBill.CA_RN_NKConsignorCountryCode);
			AssertEquals("HouseBill Consignor", "4545", houseBill.CA_ConsignorPostcode);
			AssertEquals("HouseBill Consignor", "", houseBill.CA_ConsignorState);
			AssertEquals("HouseBill Consignor", "CNORCity CNORState", houseBill.CA_ConsignorSuburb);

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_CompanyName = "Doc NotifyParty";
			shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "NP Address 1";
			shipment.NotifyPartyDocumentaryAddress.E2_Address2 = "NP Address 2";
			shipment.NotifyPartyDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			shipment.NotifyPartyDocumentaryAddress.E2_Fax = "5656";
			shipment.NotifyPartyDocumentaryAddress.E2_Phone = "6767";
			shipment.NotifyPartyDocumentaryAddress.E2_Postcode = "7878";
			shipment.NotifyPartyDocumentaryAddress.E2_State = "NPState";
			shipment.NotifyPartyDocumentaryAddress.E2_City = "NPCity";
			AssertEquals("HouseBill NotifyParty", ZGuid.Empty, houseBill.CA_OH_Notify);
			AssertEquals("HouseBill NotifyParty", "Doc NotifyParty", houseBill.CA_NotifyName);
			AssertEquals("HouseBill NotifyParty", "NP Address 1", houseBill.CA_NotifyAddress1);
			AssertEquals("HouseBill NotifyParty", "NP Address 2", houseBill.CA_NotifyAddress2);
			AssertEquals("HouseBill NotifyParty", "AU", houseBill.CA_RN_NKNotifyCountryCode);
			AssertEquals("HouseBill NotifyParty", "5656", houseBill.CA_NotifyFax);
			AssertEquals("HouseBill NotifyParty", "6767", houseBill.CA_NotifyPhone);
			AssertEquals("HouseBill NotifyParty", "7878", houseBill.CA_NotifyPostcode);
			AssertEquals("HouseBill NotifyParty", "", houseBill.CA_NotifyState);
			AssertEquals("HouseBill NotifyParty", "NPCity NPState", houseBill.CA_NotifySuburb);

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = false;
			AssertEquals("HouseBill Notify Party", TestOH_NotifyParty, houseBill.CA_OH_Notify);
		}

		public void TestSynchroniseConsigneeWithSystemDefinedOrganisation()
		{
			ZGuid unmatchedOrg = Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;

			ForwardingConsol consol = CreateFCLConsol();
			SeaCargoSynchroniser sCASynchroniser = GetSeaCargoSynchroniser(consol);
			CommonShipment shipment = consol.Shipments.AddNew();

			CusSCAOceanBill oceanBill = sCASynchroniser.OceanBill;
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();

			HouseBillSynchroniser synchroniser = GetHouseBillSynchroniser(houseBill, shipment);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			shipment.ConsigneePK = unmatchedOrg;

			AssertEquals(unmatchedOrg, houseBill.Consignee.PK);
		}

		public void TestSynchroniseConsignorWithSystemDefinedOrganisation()
		{
			ZGuid unmatchedOrg = Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;

			ForwardingConsol consol = CreateFCLConsol();
			SeaCargoSynchroniser sCASynchroniser = GetSeaCargoSynchroniser(consol);
			CommonShipment shipment = consol.Shipments.AddNew();

			CusSCAOceanBill oceanBill = sCASynchroniser.OceanBill;
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();

			HouseBillSynchroniser synchroniser = GetHouseBillSynchroniser(houseBill, shipment);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			shipment.ConsignorPK = unmatchedOrg;

			AssertEquals(unmatchedOrg, houseBill.Consignor.PK);
		}

		public void TestHouseBillPackLines()
		{
			ForwardingConsol consol = CreateFCLConsol();
			SeaCargoSynchroniser sCASynchroniser = GetSeaCargoSynchroniser(consol);
			CommonShipment shipment = consol.Shipments.AddNew();
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CNTU12837287";
			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "FTCU23736473";

			var consol2 = CreateFCLConsol();
			var container3 = consol2.Containers.AddNew();
			container3.JC_ContainerNum = "XXXU1231230";
			consol2.Shipments.Add(shipment);

			CusSCAOceanBill oceanBill = sCASynchroniser.OceanBill;
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();

			HouseBillSynchroniser synchroniser = GetHouseBillSynchroniser(houseBill, shipment);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			PackLine newPackLine1 = shipment.OuterPackLines.AddNew();
			newPackLine1.SetContainer(consol, container1);
			AssertEquals("HouseBill Pivot Count", 1, houseBill.Pivot.Count);

			PackLine newPackLine2 = shipment.OuterPackLines.AddNew();
			newPackLine2.SetContainer(consol, container2);
			AssertEquals("HouseBill Pivot Count", 2, houseBill.Pivot.Count);

			shipment.OuterPackLines.Remove(newPackLine2);
			AssertEquals("HouseBill Pivot Count", 1, houseBill.Pivot.Count);
		}

		public void TestRemovePivotSynchronisationStillWorks()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer container1 = consol.Containers.AddNew();
			SeaCargoSynchroniser sCASynchroniser = GetSeaCargoSynchroniser(consol);
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 0;
			shipment.OuterPackLines.RemoveAndDeleteAll();
			AssertEquals("Pre-Condition: Outer Packline Count", 0, shipment.OuterPackLines.Count);

			var oceanBill = sCASynchroniser.OceanBill;
			var houseBill = oceanBill.HouseBills.AddNew();

			var synchroniser = GetHouseBillSynchroniser(houseBill, shipment);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			houseBill.Pivot.RemoveAndDeleteAll();
			AssertEquals("When Binding a fake row is added and then deleted and fires the Count Changed Event", 0, houseBill.Pivot.Count);

			PackLine newPackLine1 = shipment.OuterPackLines.AddNew();
			AssertEquals("HouseBill Pivot Count", 1, houseBill.Pivot.Count);
		}

		public void TestDestinationDeletedEvent()
		{
			ForwardingShipment shipment = (ForwardingShipment)CreateGRPConsol().Shipments.Find(new ZQuery(JobShipmentSchema.JS_HouseBill, Ultimate1HouseBill))[0];
			shipment.JS_OuterPacks = 20;
			Assert("PRE-Condition Shipment should have at least one packline", shipment.OuterPackLines.Count > 0);

			CusSCAHouse house = Factory.New<CusSCAHouse>();
			house.CA_JS = shipment.PK;
			house.Pivot.AddNew();
			HouseBillSynchroniser testSynchroniser = GetHouseBillSynchroniser(house, shipment);
			testSynchroniser.DestinationDeleted += new EventHandler(TestSynchroniser_DestinationDeleted);
			testSynchroniser.SetEnabled(true, false);
			AssertEquals("House bill should have pivot count equals to synchronised shipment packline count", shipment.OuterPackLines.Count, house.Pivot.Count);
			destinationDeletedCalled = false;
			house.Pivot.RemoveAndDeleteAll();
			AssertEquals("Only Pivots removed, event should not have been called yet.", false, destinationDeletedCalled);
			house.Delete();
			AssertEquals("Failed to call destination Deleted Event", true, destinationDeletedCalled);
		}

		public void TestColoadMasterShipment_Changed()
		{
			ForwardingConsol result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Groupage;
			result.JK_RL_NKLoadPort = "SGSIN";
			result.JK_RL_NKDischargePort = "AUSYD";

			CommonContainer gRPContainer = result.Containers.AddNew();
			gRPContainer.JC_ContainerNum = "FRCU3399209";

			CommonShipment coLoadMaster = result.Shipments.AddNew();
			coLoadMaster.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			coLoadMaster.ConsigneePK = CreateForwarder(AlternateForwarderClientID).PK;
			coLoadMaster.JS_HouseBill = CoLoadMasterHouseBill;

			SeaCargoSynchroniser sCASynchroniser = GetSeaCargoSynchroniser(result);
			CusSCAOceanBill oceanBill = sCASynchroniser.OceanBill;
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			HouseBillSynchroniser testSynchroniser = GetHouseBillSynchroniser(houseBill, coLoadMaster);
			testSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			testSynchroniser.Destination.Delete();
			AssertNoExceptionThrown("Should not deleted row information cannot be accessed through the row.", () => testSynchroniser.ColoadMasterShipment_Changed(testSynchroniser, new EventArgs()));
		}

		public void TestSynchroniserReadOnlyState()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment = consol.Shipments.AddNew();
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAHouse house = synchroniser.GetHouseBill(shipment);

			Assert(!house.OceanBill.OverrideFreightDefaults);
			AssertEquals("ReadOnly CA_HouseBillInfo", true, house.CA_HouseBillInfo.ReadOnly);
			AssertEquals("ReadOnly CA_RL_NK_PortOfOriginInfo", true, house.CA_RL_NK_PortOfOriginInfo.ReadOnly);
			AssertEquals("ReadOnly CA_RN_NKGoodsOriginInfo", false, house.CA_RN_NKGoodsOriginInfo.ReadOnly);
			AssertEquals("ReadOnly CA_RL_NK_PortOfDestinationInfo", true, house.CA_RL_NK_PortOfDestinationInfo.ReadOnly);

			house.OceanBill.OverrideFreightDefaults = true;
			AssertEquals("ReadOnly CA_HouseBillInfo", false, house.CA_HouseBillInfo.ReadOnly);
			AssertEquals("ReadOnly CA_RL_NK_PortOfOriginInfo", false, house.CA_RL_NK_PortOfOriginInfo.ReadOnly);
			AssertEquals("ReadOnly CA_RN_NKGoodsOriginInfo", false, house.CA_RN_NKGoodsOriginInfo.ReadOnly);
			AssertEquals("ReadOnly CA_RL_NK_PortOfDestinationInfo", false, house.CA_RL_NK_PortOfDestinationInfo.ReadOnly);
		}

		public void TestPivotSynchroniserDoesNotDuplicateContainer()
		{
			CFSDataRegistry.Instance.AutopackContainers.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer container = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST0000001";
			container2.JC_ContainerNum = "FRCU0044930";
			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.SetContainer(consol, container);
			line1.JL_PackageCount = 10;
			line1.JL_F3_NKPackType = "BOX";

			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAHouse house = synchroniser.GetHouseBill(shipment);
			AssertEquals("House Pivot Package Count", 10, house.Pivot[0].CV_PackageCount);
			AssertEquals("House Bill Total Pivot Count", 1, house.Pivot.Count);
			PackLine line2 = (PackLine)Factory.New(line1.GetType());
			line2.JL_PackageCount = 8;
			line2.JL_F3_NKPackType = "BOX";
			line2.JL_FreightMode = FreightConstants.OuterPackType;
			line2.SetContainer(consol, container2);
			shipment.OuterPackLines.Add(line2);
			AssertEquals("Pivot Count", 2, house.Pivot.Count);
		}

		public void TestMultiplePackLinesPackedIntoSameContainerSamePackageType()
		{
			ForwardingConsol consol = CreateGroupageConsol();

			Transport transport = consol.Transports[0];
			transport.JW_JX = ImportSailing.PK;

			CommonContainer groupageContainer = consol.Containers.AddNew();
			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 20;
			packLine1.JL_F3_NKPackType = "BOX";

			packLine2.JL_PackageCount = 40;
			packLine2.JL_F3_NKPackType = "BOX";

			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAHouse houseBill = synchroniser.GetHouseBill(shipment);
			AssertEquals("Pivot Count", 1, houseBill.Pivot.Count);
			AssertEquals("Pivot Package Count", 60, houseBill.Pivot[0].CV_PackageCount);
			AssertEquals("Pivot Package Type", "BX", houseBill.Pivot[0].CV_PackageType);
		}

		public void TestMultiplePackLinesPackIntoSameContainerDifferentPackageTypes()
		{
			ForwardingConsol consol = CreateGroupageConsol();

			Transport transport = consol.Transports[0];
			transport.JW_JX = ImportSailing.PK;

			CommonContainer groupageContainer = consol.Containers.AddNew();
			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 20;
			packLine1.JL_F3_NKPackType = "PKG";

			packLine2.JL_PackageCount = 40;
			packLine2.JL_F3_NKPackType = "BOX";

			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAHouse houseBill = synchroniser.GetHouseBill(shipment);
			synchroniser.SynchroniseHouse(houseBill, shipment);
			AssertEquals("Pivot Count", 1, houseBill.Pivot.Count);
			AssertEquals("Pivot Package Count", 60, houseBill.Pivot[0].CV_PackageCount);
			AssertEquals("Pivot Package Type - Mixed package types goes to NE - Packed or Unpacked", expectedUnpackedOrPackedCode, houseBill.Pivot[0].CV_PackageType);
		}

		public void TestIssue00880176()
		{
			ErrorReporter.Clear();

			var consol = CreateFCLConsol();
			var sCASynchroniser = GetSeaCargoSynchroniser(consol);
			var shipment = consol.Shipments.AddNew();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CNTU12837287";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualVolume = 10m;
			packLine1.SetContainer(consol, container1);

			var oceanBill = sCASynchroniser.OceanBill;
			var houseBill = oceanBill.HouseBills.AddNew();

			var synchroniser = GetHouseBillSynchroniser(houseBill, shipment);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(1, consol.RelatedPackLines.Count);
			AssertEquals(10m, consol.RelatedPackLines[0].JL_ActualVolume);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualVolume = 20m;
			packLine2.SetContainer(consol, container1);

			packLine2.Delete();

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualVolume = 30m;
			packLine3.SetContainer(consol, container1);

			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("No exception thrown.", ZString.Empty, ErrorReporter.LastMessageReported);
			AssertEquals(2, consol.RelatedPackLines.Count);
			AssertEquals(10m, consol.RelatedPackLines[0].JL_ActualVolume);
			AssertEquals(30m, consol.RelatedPackLines[1].JL_ActualVolume);
		}

		public void TestDeletedPacklineIsRemovedFromLoosePacklines()
		{
			var consol = CreateFCLConsol();
			var sCASynchroniser = GetSeaCargoSynchroniser(consol);
			var shipment = consol.Shipments.AddNew();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CNTU12837287";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualVolume = 10m;
			packLine1.SetContainer(consol, container1);

			var oceanBill = sCASynchroniser.OceanBill;
			var houseBill = oceanBill.HouseBills.AddNew();

			var houseSynchroniser = GetHouseBillSynchroniser(houseBill, shipment);
			houseSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			var pivotSynchroniser = houseSynchroniser.PivotSynchronisers.FirstOrDefault();

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualVolume = 20m;
			packLine2.SetContainer(consol, container1);

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualVolume = 30m;
			packLine3.JL_Calc_ContainerNumber = "";

			AssertEquals("Watched Lines", 1, pivotSynchroniser.AdditionalPackLinesToWatch.Count);
			AssertEquals("Loose lines", 1, houseSynchroniser.LoosePackLines.Count);

			packLine2.Delete();
			AssertEquals("Watched Lines", 0, pivotSynchroniser.AdditionalPackLinesToWatch.Count);
			AssertEquals("Deleted watched line does not end up in loose lines", 1, houseSynchroniser.LoosePackLines.Count);

			packLine3.Delete();
			AssertEquals("Watched Lines", 0, pivotSynchroniser.AdditionalPackLinesToWatch.Count);
			AssertEquals("Deleted loose line is removed from loose lines", 0, houseSynchroniser.LoosePackLines.Count);
		}

		#region Implementation

		protected const string CoLoadMasterHouseBill = "SGNSYD3300229";
		protected const string Ultimate1HouseBill = "HGK33994";
		protected const string Ultimate2HouseBill = "847762";
		protected const string Normal1HouseBill = "SGNSYD3300291";
		protected const string Normal2HouseBill = "SGNSYD3300293";
		protected const string ConsolForwarderClientID = "C006214584";
		protected const string AlternateForwarderClientID = "C997150143";
		protected string expectedUnpackedOrPackedCode = "NE";

		bool destinationDeletedCalled;
		void TestSynchroniser_DestinationDeleted(object sender, EventArgs e)
		{
			destinationDeletedCalled = true;
		}

		protected abstract SeaCargoSynchroniser GetSeaCargoSynchroniser(ForwardingConsol consol);

		protected abstract HouseBillSynchroniser GetHouseBillSynchroniser(CusSCAHouse house, CommonShipment shipment);

		protected void AssertOrgClientID(string message, OrgHeader forwarder, ZString clientID)
		{
			Assert("Client ID empty " + message, !clientID.IsEmpty);
			ZString code = forwarder.LocalManifestID;
			AssertEquals("Client IDs do not match " + message, code, clientID);
		}

		protected ForwardingConsol CreateGRPConsol()
		{
			ForwardingConsol result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Groupage;
			result.JK_RL_NKLoadPort = "SGSIN";
			result.JK_RL_NKDischargePort = "AUSYD";

			Transport transport = result.Transports[0];
			transport.JW_VoyageFlight = TestVoyage;
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.NotEqual, ZString.Empty)).RV_Code;

			result.SetDefaultReceivingForwarderAddress(CreateForwarder(ConsolForwarderClientID));
			CommonContainer gRPContainer = result.Containers.AddNew();
			gRPContainer.JC_ContainerNum = "FRCU3399209";

			CommonShipment coLoadMaster = result.Shipments.AddNew();
			coLoadMaster.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			coLoadMaster.ConsigneePK = CreateForwarder(AlternateForwarderClientID).PK;
			coLoadMaster.JS_HouseBill = CoLoadMasterHouseBill;
			CommonShipment ultimate1 = result.Shipments.AddNew();
			ultimate1.JS_JS_ColoadMasterShipment = coLoadMaster.PK;
			ultimate1.JS_HouseBill = Ultimate1HouseBill;
			ultimate1.ConsigneePK = CreateConsignee("Ultimate 1 Consignee").PK;
			CommonShipment ultimate2 = result.Shipments.AddNew();
			ultimate2.JS_JS_ColoadMasterShipment = coLoadMaster.PK;
			ultimate2.JS_HouseBill = Ultimate2HouseBill;
			ultimate2.ConsigneePK = CreateConsignee("Ultimate 2 Consignee").PK;
			CommonShipment normal1 = result.Shipments.AddNew();
			normal1.JS_HouseBill = Normal1HouseBill;
			normal1.ConsigneePK = CreateConsignee("Normal 1 Consingee").PK;
			CommonShipment normal2 = result.Shipments.AddNew();
			normal2.JS_HouseBill = Normal1HouseBill;
			normal2.ConsigneePK = CreateConsignee("Normal 2 Consingee").PK;
			return result;
		}

		protected OrgHeader CreateForwarder(ZString clientID)
		{
			OrgHeader result = CreateOrgHeader("Forwader " + clientID);
			result.OH_IsForwarder = true;
			result.LocalManifestID = clientID;
			return result;
		}

		protected OrgHeader CreateConsignee(ZString name)
		{
			OrgHeader result = CreateOrgHeader(name);
			result.OH_IsConsignee = true;
			return result;
		}

		protected OrgHeader CreateConsignor(ZString name)
		{
			OrgHeader result = CreateOrgHeader(name);
			result.OH_IsConsignor = true;
			return result;
		}

		protected OrgHeader CreateOrgHeader(ZString name)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsConsignee = true;
			result.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			result.MainAddress.OA_Address1 = "Address 1 " + name;
			result.OH_FullName = name;
			return result;
		}

		#endregion
	}
}
