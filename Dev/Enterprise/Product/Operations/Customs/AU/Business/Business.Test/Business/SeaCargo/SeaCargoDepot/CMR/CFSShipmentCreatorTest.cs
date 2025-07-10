using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CFSShipmentCreatorTest : CFSRecordCreatorTest
	{
		public void TestOuterPacksPopulated()
		{
			var header = CreateOutturnHeader();
			var seiMessage = (CMRSEIMessage)header.Messages.AddNew(typeof(CMRSEIMessage));
			seiMessage.EM_MessageText = sEIMessage;
			seiMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			seiMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;
			DepotCusOutturn outturn1 = header.Outturns.AddNew();
			DepotCusOutturn outturn2 = header.Outturns.AddNew();
			AssertEquals(2, header.Outturns.Count);
			header.C6_LloydsIMO = "7654321";
			header.C6_OutturningPremiseID = "9914N";
			header.C6_VoyageNum = "999S";

			outturn1.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			outturn2.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;

			const string containerNumber = "AAAA1111113";
			outturn1.C5_ContainerNumber = containerNumber;
			outturn2.C5_ContainerNumber = containerNumber;

			const string houseNumber = "HOUSE100";
			outturn2.C5_HouseBill = houseNumber;

			const string oceanNumber = "OCEAN100";
			outturn2.C5_MasterBill = oceanNumber;
			outturn2.C5_GoodsDescription = "1234567890123456789012345678901234567890";

			outturn1.C5_OuterPacks = 1;
			outturn1.C5_OuterPackUnits = CMRPackageTypes.Codes.UnpackedOrPacked;

			outturn2.C5_OuterPacks = 7;
			outturn2.C5_OuterPackUnits = CMRPackageTypes.Codes.Package;

			CFSLoadListConsolCreator loadlistCreator = new CFSLoadListConsolCreator(outturn1, Factory);
			CFSShipmentCreator shipmentCreator = new CFSShipmentCreator(outturn2, loadlistCreator.Consol, Factory);

			CFSShipment shipment = shipmentCreator.Shipment;

			AssertEquals(7, shipment.JS_OuterPacks);
			AssertEquals("PKG", shipment.JS_F3_NKPackType);
			AssertEquals("Volume", 7m, shipment.JS_ActualVolume);
			AssertEquals("Volume Units", "M3", shipment.JS_UnitOfVolume);
			AssertEquals("Weight", 7000m, shipment.JS_ActualWeight);
			AssertEquals("Weight Units", "KG", shipment.JS_UnitOfWeight);
			AssertEquals("Description is 35 characters", "12345678901234567890123456789012345", shipment.JS_GoodsDescription);
		}
		const string sEIMessage = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEI+1G79 7IAF 71F9:1++11'DTM+9:20090317154645464587:ZZZ'TDT+20+6990++11++++7654321::11'" +
"TDT+1++ROA'NAD+MR+AAA374M::95'NAD+VW+41065894724::95'RFF+ABO:O00000007/DAT8::8'DOC+1'PAC+++FCL:67:95'PAC+100++BX:185:95'RFF+MB:OBLDPT001'RFF+AAQ:OCLU8911239'" +
"FTX+AAA+++CONSOLIDATED CARGO'GIS+FFO:109:95'MEA+AAE+G+KG:0000000023000.00'MEA+AAE+AAL+KG:0000000020000.00'MEA+AAE+ABJ+CM:0000000000020.00'NAD+CN++LOCAL FORWARDER'" +
"DOC+1'PAC+++LCL:67:95'PAC+20++BX:185:95'RFF+MB:OBLDPT001'RFF+BH:HBL001'RFF+AAQ:OCLU8911239'FTX+AAA+++STUFF TYPE 1'MEA+AAE+G+KG:0000000005000.00'" +
"MEA+AAE+AAL+KG:0000000005000.00'MEA+AAE+ABJ+CM:0000000000005.00'NAD+CN++CONSIGNEE'DOC+1'PAC+++LCL:67:95'PAC+30++BX:185:95'RFF+MB:OCEAN100'RFF+BH:HOUSE100'" +
"RFF+AAQ:AAAA1111113'PCI+28+MARKS1:MARKS2:MARKS3:MARKS4:MARKS5:MARKS6:MARKS7:MARKS8:MARKS9'FTX+AAA+++STUFF TYPE 2'MEA+AAE+G+KG:0000000007000.00'" +
"MEA+AAE+AAL+KG:0000000007001.00'MEA+AAE+ABJ+CU:0000000000007.00'NAD+CN++CONSIGNEE 2'UNT+43+000001'";

		public void TestCreateNewContainerFromOutturn()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn = AddContainerLine(header, underbond, TestContainerNumber1);

			CFSContainerCreator creator = new CFSContainerCreator(outturn, Factory);
			AssertNotNull("Failed to create Consol", creator.Consol);
			AssertNotNull("Failed to create Consol", creator.Container);

			AssertEquals("Container Number", TestContainerNumber1, creator.Container.JC_ContainerNum);
			AssertEquals("Parent ID", creator.Container.PK, outturn.C5_ParentID);
			AssertEquals("Parent ID", creator.Container.PK, underbond.C4_ParentID);
		}

		public void TestCreatingNewContainerOnExistingLoadList()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill1);

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();

			Transport transport = consol.Transports[0];
			transport.JW_Vessel = VesselFromLloyds(TestLloydsNum);
			transport.JW_VoyageFlight = TestVoyageNum;

			CFSShipmentCreator creator = new CFSShipmentCreator(outturn, consol, Factory);
			AssertEquals("Containers consol", consol, creator.Consol);

			AssertEquals("Shipment should have OuterPackLine packed into Container", consol.Containers[0], consol.Shipments[0].OuterPackLines[0].GetContainer(consol));

			AssertEquals("Parent ID", creator.Shipment.PK, outturn.C5_ParentID);
			AssertEquals("Parent ID", creator.Container.PK, underbond.C4_ParentID);
		}

		public void TestCreateNewBreakBulkShipmentFromOutturn()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn = AddBreakBulkLine(header, underbond, TestHouseBill1, TestOceanBill1);

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();

			Transport transport = consol.Transports[0];
			transport.JW_Vessel = VesselFromLloyds(TestLloydsNum);
			transport.JW_VoyageFlight = TestVoyageNum;

			CFSShipmentCreator creator = new CFSShipmentCreator(outturn, consol, Factory);
			AssertEquals("Containers consol", consol, creator.Consol);

			AssertEquals("Shipment should have 1 OuterPackLine not packed into any Container", null, consol.Shipments[0].OuterPackLines[0].GetContainer(consol));

			AssertEquals("Parent ID", creator.Shipment.PK, outturn.C5_ParentID);
			AssertEquals("Parent ID", creator.Shipment.PK, underbond.C4_ParentID);
			AssertEquals("Goods description comes across", "FOO", creator.Shipment.JS_GoodsDescription);
			AssertEquals("Marks and Numbers comes across", "BAR", creator.Shipment.JS_MarksAndNumbers);
		}

		public void TestNoNotLinkToWrongContainerFromOutturn()
		{
			var header = CreateOutturnHeader();
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			var outturn = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill1);

			var consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_MasterBillNum = TestOceanBill1;
			var transport1 = consol1.Transports[0];
			transport1.JW_Vessel = VesselFromLloyds(TestLloydsNum);
			transport1.JW_VoyageFlight = TestVoyageNum;

			var consol2 = Factory.New<CFSLoadListConsol>();
			consol2.JK_MasterBillNum = TestOceanBill1;
			var transport2 = consol2.Transports[0];
			transport2.JW_Vessel = VesselFromLloyds(TestLloydsNum);
			transport2.JW_VoyageFlight = TestVoyageNum;
			var con = consol2.Containers.AddNew();
			con.JC_ContainerNum = TestContainerNumber1;

			var creator = new CFSShipmentCreator(outturn, Factory);
			AssertEquals("Ocean bill consol", consol1, creator.Consol);
			AssertEquals("New container on consol1", 1, consol1.Containers.Count);
			AssertEquals("Did use new container on consol1", consol1.Containers[0].PK, creator.Container.PK);
			AssertNotEquals("Did not use container on other consol2", con.PK, creator.Container.PK);
		}
	}
}
