using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRSeaCargoSynchroniserTest : SeaCargoSynchroniserTest
	{
		public void TestSynchroniseShipmentDetailsCMR()
		{
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDevlieryAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignorPickupAddress.PK;
			var testSynchroniser = GetSeaCargoSynchroniser(consol);

			var houseBill = testSynchroniser.OceanBill.HouseBills.AddNew();
			testSynchroniser.SynchroniseHouse(houseBill, shipment);

			CombineAssertions(() =>
			{
				AssertEquals("Synchronise CA_CB", houseBill.CA_CB, testSynchroniser.OceanBill.PK);
				AssertEquals("Synchronise CA_HouseBill", shipment.JS_HouseBill, houseBill.CA_HouseBill);
				AssertEquals("Synchronise CA_IsMasterHouse", shipment.IsCoLoadMaster, houseBill.CA_IsMasterHouse);

				AssertEquals("Synchronise CA_RL_NK_PortOfOrigin", shipment.JS_RL_NKOrigin, houseBill.CA_RL_NK_PortOfOrigin);
				AssertEquals("Synchronise CA_RL_NK_PortOfDestination", shipment.JS_RL_NKDestination, houseBill.CA_RL_NK_PortOfDestination);
				AssertEquals("Synchronise CA_RN_NKGoodsOrigin", shipment.JS_RL_NKOrigin.SubstringSafe(0, 2), houseBill.CA_RN_NKGoodsOrigin);
				AssertEquals("Synchronise CA_PrepaidCollectOther", CMRMethodsOfPayment.Codes.Collect, houseBill.CA_PrepaidCollectOther);
				AssertEquals("Synchronise CA_OH_Consignor", shipment.Consignor.PK, houseBill.Consignor.PK);
				AssertEquals("TEST CONSIGNOR", houseBill.CA_ConsignorName);
				AssertEquals("Pickup addr1", houseBill.CA_ConsignorAddress1);
				AssertEquals("Pickup addr2", houseBill.CA_ConsignorAddress2);
				AssertEquals("4323", houseBill.CA_ConsignorPostcode);
				AssertEquals("", houseBill.CA_ConsignorState);
				AssertEquals("Pickup foocity NSW", houseBill.CA_ConsignorSuburb);

				AssertEquals("Synchronise CA_OH_Consignee", shipment.Consignee.PK, houseBill.Consignee.PK);
				AssertEquals("TEST CONSIGNEE", houseBill.CA_ConsigneeName);
				AssertEquals("Delivery 1addr", houseBill.CA_ConsigneeAddress1);
				AssertEquals("Delivery 2addr", houseBill.CA_ConsigneeAddress2);
				AssertEquals("666", houseBill.CA_ConsigneeFax);
				AssertEquals("444", houseBill.CA_ConsigneePhone);
				AssertEquals("8687", houseBill.CA_ConsigneePostcode);
				AssertEquals("", houseBill.CA_ConsigneeState);
				AssertEquals("Delivery barcity ACT", houseBill.CA_ConsigneeSuburb);

				AssertEquals("Synchronise CA_OH_Notify", shipment.NotifyPartyDocumentaryAddress.OrganisationPK, houseBill.CA_OH_Notify);
			});

			string defaultMoveUnderbondFromForOrg = "";
			AssertEquals("Synchronise CA_MoveUnderbondFrom", defaultMoveUnderbondFromForOrg, houseBill.CA_MoveUnderbondFrom);
			string defaultMoveUnderbondToForOrg = "";
			AssertEquals("Synchronise CA_MoveUnderbondTo", defaultMoveUnderbondToForOrg, houseBill.CA_MoveUnderbondTo);
			AssertEquals("Synchronise CA_ShipmentStatus", ExpectedNotSentState, houseBill.CA_ShipmentStatus);
			AssertEquals("Synchronise CA_UnderbondStatus", "", houseBill.CA_UnderbondStatus);
			AssertEquals("Synchronise CA_JS", shipment.PK, houseBill.CA_JS);
		}

		protected override ZString ExpectedNotSentState
		{
			get { return ZString.Empty; }
		}

		protected override ZString ExpectedPrincipalID(OrgHeader principalOrg)
		{
			return principalOrg.LocalBusinessRegNo.Replace(" ", "");
		}

		public void TestCoLoadMastersAreSynchronised()
		{
			CommonContainer addedContainer1 = consol.Containers.AddNew();
			shipment.OuterPackLines[0].SetContainer(consol, addedContainer1);
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			addedContainer1.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			CusSCAHouse houseBill = synchroniser.GetHouseBill(shipment);
			AssertNotNull("House Bill should not be null", houseBill);
		}

		public void TestHouseWithCoLoadMasterSynchronise()
		{
			CommonContainer addedContainer1 = consol.Containers.AddNew();
			shipment.OuterPackLines[0].SetContainer(consol, addedContainer1);
			CommonShipment shipmen2 = consol.Shipments.AddNew();
			shipmen2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipmen2.JS_HouseBill = "MASTERHOUSE";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_JS_ColoadMasterShipment = shipmen2.PK;
			addedContainer1.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.LCL;
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			CusSCAHouse houseBill = synchroniser.GetHouseBill(shipment);
			AssertNotNull("House Bill should not be null", houseBill);
			synchroniser.SynchroniseHouse(houseBill, shipment);
			AssertEquals("MASTERHOUSE", houseBill.CA_MasterHouseBill);
			shipmen2.JS_HouseBill = "MASTERHOUSE2";
			AssertEquals("MASTERHOUSE2", houseBill.CA_MasterHouseBill);
			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertEquals(ZString.Empty, houseBill.CA_MasterHouseBill);
			shipment.JS_JS_ColoadMasterShipment = shipmen2.PK;
			AssertEquals("MASTERHOUSE2", houseBill.CA_MasterHouseBill);
			CommonShipment shipmen3 = consol.Shipments.AddNew();
			shipmen3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipmen3.JS_HouseBill = "MASTERHOUSE3";
			shipment.JS_JS_ColoadMasterShipment = shipmen3.PK;
			AssertEquals("MASTERHOUSE3", houseBill.CA_MasterHouseBill);
			shipmen3.JS_HouseBill = "MASTERHOUSE4";
			AssertEquals("MASTERHOUSE4", houseBill.CA_MasterHouseBill);

			shipment.JS_GoodsDescription = "AMENDED DESCRIPTION";
			AssertEquals("Standard shipment description syncronises", "AMENDED DESCRIPTION", houseBill.Pivot[0].CV_GoodsDescription);

			shipmen3.JS_GoodsDescription = "SHIPMENT3 DESCRIPTION";
			CusSCAHouse shipment3House = synchroniser.GetHouseBill(shipmen3);
			AssertNotNull(shipment3House);
			AssertEquals("Co-load master desciption also syncronises", "SHIPMENT3 DESCRIPTION", shipment3House.Pivot[0].CV_GoodsDescription);
		}

		public void TestSynchOfHouseBillsOnRefresh()
		{
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Cuckoo Squeaker";
			CMRSeaCargoSynchroniser synchroniser = new CMRSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			synchroniser.SynchroniseOceanBill();
			AssertEquals(2, oceanBill.HouseBills.Count);
			AssertEquals(shipment.JS_HouseBill, oceanBill.HouseBills[1].CA_HouseBill);
		}

		public void TestDefaultOfPrepaidCollect()
		{
			CommonShipment shipment = consol.Shipments.AddNew();
			CMRSeaCargoSynchroniser synchroniser = new CMRSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			synchroniser.SynchroniseOceanBill();
			CusSCAHouse house = synchroniser.GetHouseBill(shipment);
			AssertEquals("Payment Type should be prepaid only", CMRMethodsOfPayment.Codes.PrepaidOnly, house.CA_PrepaidCollectOther);
		}

		public void TestSynchroniserCA_PrepaidCollectOther()
		{
			SeaCargoSynchroniser testSynchroniser = GetSeaCargoSynchroniser(consol);

			CusSCAHouse houseBill = testSynchroniser.OceanBill.HouseBills.AddNew();
			testSynchroniser.SynchroniseHouse(houseBill, shipment);

			AssertEquals("Synchronise CA_PrepaidCollectOther", CMRMethodsOfPayment.Codes.Collect, houseBill.CA_PrepaidCollectOther);
		}

		public void TestLCLShipmentToBBK_CMR()
		{
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.LCL;
			CommonContainer lCLContainer = consol.Containers.AddNew();
			lCLContainer.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.LCL;
			lCLContainer.JC_ContainerNum = "CHFU0303220";
			shipment.OuterPackLines[0].SetContainer(lCLContainer.PK);
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			houseBill = synchroniser.GetHouseBill(shipment);
			AssertEquals("LCL SCA Container", Enterprise.Core.Constants.ContainerModes.LCL, houseBill.Pivot[0].Container.CN_ContainerMode);
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			consol.Containers.RemoveAndDelete(lCLContainer);
			synchroniser.SynchroniseOceanBill();
			synchroniser.SynchroniseHouse(houseBill, shipment);
			AssertEquals("LCL SCA Container", CMRImportCargoTypes.Codes.BreakBulk, houseBill.Pivot[0].Container.CN_ContainerMode);
		}

		public void TestDefaultingCMRContainerModesGroupage()
		{
			consol = CreateGroupageConsol();
			consol.Containers.AddNew();
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			AssertEquals("should go to LCL", Enterprise.Core.Constants.ContainerModes.LCL, oceanBill.Containers[0].CN_ContainerMode);
			consol = CreateBuyersConsol();
			consol.Containers.AddNew();
			synchroniser = GetSeaCargoSynchroniser(consol);
			oceanBill = synchroniser.OceanBill;
			AssertEquals("should go to LCL", Enterprise.Core.Constants.ContainerModes.FCLMixedShipper, oceanBill.Containers[0].CN_ContainerMode);
		}

		public void TestSynchroniserSkipsOceanBillDetailsIfMessagesInProgressOrAcknowledged()
		{
			consol = CreateGroupageConsol();
			AssertEquals("Pre-Condition: Port of loading", "SGSIN", consol.JK_JX_JA_RL_NKPortOfLoading);
			AddContainerToConsol(consol, ContainerNumber1);
			CommonShipment shipment1 = AddShipmentToConsol(consol, HouseBillNumber1);
			CommonShipment shipment2 = AddShipmentToConsol(consol, HouseBillNumber2);
			CMRSeaCargoSynchroniser synchroniser = new CMRSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			CusSCAHouse house1 = synchroniser.GetHouseBill(shipment1);
			house1.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			house1.Messages.Add(Factory.New(typeof(CMRSEACRMessage)));

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "USLAX";
			synchroniser.SynchroniseOceanBill();
			AssertEquals("Port of loading should not have changed", "SGSIN", oceanBill.CB_RL_NKPortOfLoading);
			synchroniser.LoadHouseBills();
			AssertEquals("Port of loading should not have changed", "SGSIN", oceanBill.CB_RL_NKPortOfLoading);
		}

		public void TestOverideFreightDefaults()
		{
			consol = CreateGroupageConsol();
			AssertEquals("Pre-Condition: Port of loading", "SGSIN", consol.JK_JX_JA_RL_NKPortOfLoading);
			AddContainerToConsol(consol, ContainerNumber1);
			CommonShipment shipment1 = AddShipmentToConsol(consol, HouseBillNumber1);
			CMRSeaCargoSynchroniser synchroniser = new CMRSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			CusSCAHouse house1 = synchroniser.GetHouseBill(shipment1);
			AssertEquals("Port of loading", "SGSIN", oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Goods Origin", "SG", house1.CA_RN_NKGoodsOrigin);

			oceanBill.OverrideFreightDefaults = true;
			AssertEquals("Port of loading should not have changed", "SGSIN", oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Goods Origin should not have changed", "SG", house1.CA_RN_NKGoodsOrigin);

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "USLAX";
			shipment1.JS_RL_NKOrigin = "AUSYD";
			synchroniser.SynchroniseOceanBill();
			AssertEquals("Port of loading should not have changed", "SGSIN", oceanBill.CB_RL_NKPortOfLoading);
			synchroniser.LoadHouseBills();
			AssertEquals("Port of loading should not have changed", "SGSIN", oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Goods Origin should not have changed", "SG", house1.CA_RN_NKGoodsOrigin);

			oceanBill.OverrideFreightDefaults = false;
			AssertEquals("Port of loading should have been updated", "USLAX", oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Goods Origin should have been updated", "AU", house1.CA_RN_NKGoodsOrigin);

			transport.JW_RL_NKLoadPort = "NZAKL";
			shipment1.JS_RL_NKOrigin = "NZAKL";
			AssertEquals("Port of loading should have synchronised", "NZAKL", oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Goods Origin should have synchronised", "NZ", house1.CA_RN_NKGoodsOrigin);
		}

		public void TestSynchroniserLoadsExistingHouseBillPreservingChanges()
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Cuckoo Squeaker";
			var synchroniser = GetSeaCargoSynchroniser(consol);
			var oceanBill = synchroniser.OceanBill;
			var house = synchroniser.GetHouseBill(shipment);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var consolF2 = factory2.Load<CommonConsol>(consol.PK);
			var shipmentF2 = factory2.Load<CommonShipment>(shipment.PK);
			var oceanBillF2 = factory2.Load<CusSCAOceanBill>(oceanBill.PK);
			var houseF2 = factory2.Load<CusSCAHouse>(house.PK);
			houseF2.CA_HouseBill = "HouseInFactory2";

			var synchroniser2 = GetSeaCargoSynchroniser(consolF2);
			var oceanBillS2 = synchroniser2.OceanBill;
			AssertSame(oceanBillF2, oceanBillS2);
			var houseS2 = synchroniser2.GetHouseBill(shipmentF2);
			AssertSame(houseF2, houseS2);
			AssertEquals("House bill is loaded with changes intact", "HouseInFactory2", houseS2.CA_HouseBill);
		}

		public void TestSynchroniserCreatesHouseBillForNewShipment()
		{
			consol.Shipments.RemoveAndDeleteAll();
			var synchroniser = GetSeaCargoSynchroniser(consol);
			var oceanBill = synchroniser.OceanBill;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "New House";
			var house = oceanBill.HouseBills[0];
			AssertEquals("House bill is created by synchroniser", "NEW HOUSE", house.CA_HouseBill);
		}

		public void TestInnerSynchronisers()
		{
			var synchroniser = new CMRSeaCargoSynchroniserForTest(consol);
			_ = synchroniser.OceanBill;

			AssertEquals("OceanBillSynchroniser", 1, synchroniser.BusinessObjectSynchronisersExposed.OfType<CMROceanBillSynchroniser>().Count());
			AssertEquals("HouseBillSynchroniser", 1, synchroniser.BusinessObjectSynchronisersExposed.OfType<CMRHouseBillSynchroniser>().Count());
			AssertEquals("Total Synchronisers", 2, synchroniser.BusinessObjectSynchronisersExposed.Count);
		}

		public void TestCreateOceanBillWithSynchroniseConsolEnabled()
		{
			var synchroniser = new CMRSeaCargoSynchroniser(consol, synchroniseConsol: true);
			var synchroniserOceanBill = synchroniser.OceanBill;
			AssertEquals("House bills are created for existing Shipment(s) when the Ocean Bill is created", 1, synchroniserOceanBill.HouseBills.Count);

			var oldLoadPort = consol.JK_RL_NKLoadPort;
			consol.JK_RL_NKLoadPort = "NZAKL";
			AssertEquals("Ocean Bill is synchronised with Consol", "NZAKL", synchroniserOceanBill.CB_RL_NKPortOfLoading);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "New House";
			AssertEquals("House bill is created for new Shipment(s)", 2, synchroniserOceanBill.HouseBills.Count);
		}

		public void TestCreateOceanBillWithSynchroniseConsolDisabled()
		{
			var synchroniser = new CMRSeaCargoSynchroniser(consol, synchroniseConsol: false);
			var synchroniserOceanBill = synchroniser.OceanBill;
			AssertEquals("House bills are created for existing Shipment(s) when the Ocean Bill is created", 1, synchroniserOceanBill.HouseBills.Count);

			var oldLoadPort = consol.JK_RL_NKLoadPort;
			consol.JK_RL_NKLoadPort = "NZAKL";
			AssertEquals("Ocean Bill is not synchronised with Consol", oldLoadPort, synchroniserOceanBill.CB_RL_NKPortOfLoading);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "New House";
			AssertEquals("House bill is not created for new Shipment(s)", 1, synchroniserOceanBill.HouseBills.Count);
		}

		public void TestExistingOceanBillWithSynchroniseConsolEnabled()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var synchroniser = new CMRSeaCargoSynchroniser(consol, synchroniseConsol: true);
			var synchroniserOceanBill = synchroniser.OceanBill;
			AssertEquals("House bill is created for existing Shipment(s) even when Ocean Bill exists", 1, synchroniserOceanBill.HouseBills.Count);

			var oldLoadPort = consol.JK_RL_NKLoadPort;
			consol.JK_RL_NKLoadPort = "NZAKL";
			AssertEquals("Ocean Bill is synchronised with Consol", "NZAKL", synchroniserOceanBill.CB_RL_NKPortOfLoading);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "New House";
			AssertEquals("House bill is created for new Shipment(s)", 2, synchroniserOceanBill.HouseBills.Count);
		}

		public void TestExistingOceanBillWithSynchroniseConsolDisabled()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			oceanBill.CB_RL_NKPortOfLoading = consol.JK_RL_NKLoadPort;

			var synchroniser = new CMRSeaCargoSynchroniser(consol, synchroniseConsol: false);
			var synchroniserOceanBill = synchroniser.OceanBill;
			AssertEquals("House bill is not created for existing Shipment(s) when Ocean Bill exists", 0, synchroniserOceanBill.HouseBills.Count);

			var oldLoadPort = consol.JK_RL_NKLoadPort;
			consol.JK_RL_NKLoadPort = "NZAKL";
			AssertEquals("Ocean Bill is not synchronised with Consol", oldLoadPort, synchroniserOceanBill.CB_RL_NKPortOfLoading);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "New House";
			AssertEquals("House bill is not created for new Shipment(s)", 0, synchroniserOceanBill.HouseBills.Count);
		}

		public void TestCreateHouseInSameFactory()
		{
			var consol = CreateFCLConsol();
			consol.JK_MasterBillNum = TestMasterBillNumber;
			consol.JK_RL_NKLoadPort = TestPortOfLoading;
			consol.JK_RL_NKDischargePort = TestPortOfDischarge;
			var consolContainer = consol.Containers.AddNew();
			consolContainer.JC_ContainerNum = "CN1";

			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol, synchroniseConsol: true);
			var oceanBill = seaCargoSynchroniser.OceanBill;
			AssertEquals("Precondition - Initialise OceanBill HouseBills collection.", 0, oceanBill.HouseBills.Count);
			AssertEquals("Should have synched MasterBillNumber on the ocean bill", TestMasterBillNumber, oceanBill.CB_OceanBill);
			AssertEquals("Should have synched Port of Loading on the ocean bill", TestPortOfLoading, oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Should have synched Port of Discharge on the ocean bill", TestPortOfDischarge, oceanBill.CB_RL_NKPortOfDischarge);
			Factory.Save();

			AssertEquals("OceanBill HouseBills", 0, oceanBill.HouseBills.Count);
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB1";
			shipment.JS_GoodsDescription = "Goods";
			shipment.OuterPackLines[0].SetContainer(consol, consolContainer);
			AssertEquals("OceanBill HouseBills updated", 1, oceanBill.HouseBills.Count);

			var houseBill = oceanBill.HouseBills[0];
			AssertEquals("HB1", houseBill.CA_HouseBill);
			AssertEquals(false, houseBill.IsInDatabase);

			AssertEquals("OceanBill Containers updated", 1, oceanBill.Containers.Count);
			AssertEquals("CN1", oceanBill.Containers[0].CN_ContainerNumber);
		}

		public void TestCreateHouseInOtherFactory()
		{
			var consol = CreateFCLConsol();
			consol.JK_MasterBillNum = TestMasterBillNumber;
			consol.JK_RL_NKLoadPort = TestPortOfLoading;
			consol.JK_RL_NKDischargePort = TestPortOfDischarge;
			var consolContainer = consol.Containers.AddNew();
			consolContainer.JC_ContainerNum = "CN1";

			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol, synchroniseConsol: true);
			var oceanBill = seaCargoSynchroniser.OceanBill;
			AssertEquals("Precondition - Initialise OceanBill HouseBills collection.", 0, oceanBill.HouseBills.Count);
			AssertEquals("Should have synched MasterBillNumber on the ocean bill", TestMasterBillNumber, oceanBill.CB_OceanBill);
			AssertEquals("Should have synched Port of Loading on the ocean bill", TestPortOfLoading, oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Should have synched Port of Discharge on the ocean bill", TestPortOfDischarge, oceanBill.CB_RL_NKPortOfDischarge);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var consolF2 = factory2.Load<ForwardingConsol>(consol.PK);
			var shipmentF2 = factory2.New<ForwardingShipment>();
			shipmentF2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipmentF2.JS_HouseBill = "HB1";
			shipmentF2.JS_GoodsDescription = "Goods";
			shipmentF2.Consols.Add(consolF2);

			var containerF2 = consolF2.Containers[0];
			shipmentF2.OuterPackLines[0].SetContainer(consolF2, containerF2);

			AssertEquals("OceanBill HouseBills", 0, oceanBill.HouseBills.Count);
			factory2.Save();

			AssertEquals("Consol Shipments updated", 1, consol.Shipments.Count);
			AssertEquals("OceanBill HouseBills updated", 1, oceanBill.HouseBills.Count);
			var synchronisedHouseBill1 = oceanBill.HouseBills[0];
			AssertEquals("HB1", synchronisedHouseBill1.CA_HouseBill);
			AssertEquals(true, synchronisedHouseBill1.IsInDatabase);

			AssertEquals("OceanBill Containers updated", 1, oceanBill.Containers.Count);
			AssertEquals("CN1", oceanBill.Containers[0].CN_ContainerNumber);
		}

		public void TestRegenerateHouseInSameFactory()
		{
			var consol = CreateFCLConsol();
			consol.JK_MasterBillNum = TestMasterBillNumber;
			consol.JK_RL_NKLoadPort = TestPortOfLoading;
			consol.JK_RL_NKDischargePort = TestPortOfDischarge;

			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol, synchroniseConsol: true);
			var oceanBill = seaCargoSynchroniser.OceanBill;
			AssertEquals("Precondition - Initialise OceanBill HouseBills collection.", 0, oceanBill.HouseBills.Count);
			AssertEquals("Should have synched MasterBillNumber on the ocean bill", TestMasterBillNumber, oceanBill.CB_OceanBill);
			AssertEquals("Should have synched Port of Loading on the ocean bill", TestPortOfLoading, oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Should have synched Port of Discharge on the ocean bill", TestPortOfDischarge, oceanBill.CB_RL_NKPortOfDischarge);
			Factory.Save();

			var shipment = consol.Shipments.AddNew();
			AssertEquals("OceanBill HouseBills updated", 1, oceanBill.HouseBills.Count);

			var houseBill = oceanBill.HouseBills[0];
			AssertEquals(false, houseBill.IsInDatabase);
			var houseBillPk = houseBill.PK;
			houseBill.Delete();

			var house2 = seaCargoSynchroniser.GetHouseBill(shipment);
			AssertNotEquals("House is not the deleted house", houseBillPk, house2.PK);
		}

		const string TestPortOfLoading = "HKHKG";
		const string TestPortOfDischarge = "AUSYD";
		const string TestMasterBillNumber = "TESTMASTER123";

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}

		protected override SeaCargoSynchroniser GetSeaCargoSynchroniser(CommonConsol consol)
		{
			return new CMRSeaCargoSynchroniser(consol);
		}
	}

	public class CMRSeaCargoSynchroniserForTest : CMRSeaCargoSynchroniser
	{
		public CMRSeaCargoSynchroniserForTest(CommonConsol consol) : base(consol)
		{
		}

		public List<BusinessObjectSynchroniser> BusinessObjectSynchronisersExposed => base.BusinessObjectSynchronisers;
	}
}
