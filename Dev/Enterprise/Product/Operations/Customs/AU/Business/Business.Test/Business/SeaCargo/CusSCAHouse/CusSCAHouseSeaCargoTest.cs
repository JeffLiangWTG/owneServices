using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusSCAHouseSeaCargoTest : SeaCargoTestCase
	{
		public void TestMovementUnderbondFromAddress()
		{
			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			CusSCAHouse houseBill = oceanBill.HouseBills[0];

			AssertEquals("Pre-Condition", false, houseBill.CA_MoveUnderbondFromInfo.ReadOnly);

			OrgAddress underbondFromOrgAddress = CreateControlledPremiseIDAddress();
			houseBill.CA_OA_UnderbondFrom = underbondFromOrgAddress.PK;
			AssertEquals("Premise Code", TestControlledPremiseID, houseBill.CA_MoveUnderbondFrom);
			AssertEquals("Premise Code Read Only", true, houseBill.CA_MoveUnderbondFromInfo.ReadOnly);

			houseBill.CA_OA_UnderbondFrom = ZGuid.Empty;
			AssertEquals("Premise Code", "", houseBill.CA_MoveUnderbondFrom);
			AssertEquals("Premise Code Read Only", false, houseBill.CA_MoveUnderbondFromInfo.ReadOnly);
		}

		public void TestMovementUnderbondToAddress()
		{
			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			CusSCAHouse houseBill = oceanBill.HouseBills[0];

			AssertEquals("Pre-Condition", false, houseBill.CA_MoveUnderbondToInfo.ReadOnly);

			OrgAddress underbondToOrgAddress = CreateControlledPremiseIDAddress();
			houseBill.CA_OA_UnderbondTo = underbondToOrgAddress.PK;
			AssertEquals("Premise Code", TestControlledPremiseID, houseBill.CA_MoveUnderbondTo);
			AssertEquals("Premise Code Read Only", true, houseBill.CA_MoveUnderbondToInfo.ReadOnly);

			houseBill.CA_OA_UnderbondTo = ZGuid.Empty;
			AssertEquals("Premise Code", "", houseBill.CA_MoveUnderbondTo);
			AssertEquals("Premise Code Read Only", false, houseBill.CA_MoveUnderbondToInfo.ReadOnly);
		}

		public void TestMovementUnderbondMissingCusCode()
		{
			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			CusSCAHouse houseBill = oceanBill.HouseBills[0];

			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress address = header.Addresses.AddNew();
			houseBill.CA_OA_UnderbondTo = address.PK;
			houseBill.CA_OA_UnderbondFrom = address.PK;

			AssertEquals("Underbond From", "", houseBill.CA_MoveUnderbondFrom);
			AssertEquals("Underbond To", "", houseBill.CA_MoveUnderbondTo);

			houseBill.CA_MoveUnderbondTo = "FAKE1";
			houseBill.CA_MoveUnderbondFrom = "FAKE2";

			AssertEquals("Underbond Address From", ZGuid.Empty, houseBill.CA_OA_UnderbondFrom);
			AssertEquals("Underbond Address To", ZGuid.Empty, houseBill.CA_OA_UnderbondTo);

			CreateControlledPremiseIDAddress();
			houseBill.CA_MoveUnderbondTo = TestControlledPremiseID;
			houseBill.CA_MoveUnderbondFrom = TestControlledPremiseID;

			AssertEquals("Underbond Address From", TestOrgHeaderName, houseBill.UnderbondFrom.Header.OH_FullName);
			AssertEquals("Underbond Address To", TestOrgHeaderName, houseBill.UnderbondTo.Header.OH_FullName);
		}

		public void TestUnderbondMovementAddresses()
		{
			OrgHeader testDepot = CreateDepot();
			OrgAddress unpackPremiseWest = testDepot.Addresses.AddNew();
			unpackPremiseWest.OA_Address1 = "West Shed";
			unpackPremiseWest.LocalControlledPremisesID = WestShedPremiseID;

			OrgAddress unpackPremiseEast = testDepot.Addresses.AddNew();
			unpackPremiseEast.OA_Address1 = "East Shed";
			unpackPremiseEast.LocalControlledPremisesID = EastShedPremiseID;

			OrgAddress unpackPremiseSouth = testDepot.Addresses.AddNew();
			unpackPremiseSouth.OA_Address1 = "South Shed";
			unpackPremiseSouth.LocalControlledPremisesID = SouthShedPremiseID;

			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			CusSCAHouse houseBill = oceanBill.HouseBills[0];

			houseBill.CA_OA_UnderbondFrom = unpackPremiseWest.PK;
			houseBill.CA_OA_UnderbondTo = unpackPremiseSouth.PK;

			AssertEquals("From Premise ID From Address", WestShedPremiseID, houseBill.CA_MoveUnderbondFrom);
			AssertEquals("To Premise ID From Address", SouthShedPremiseID, houseBill.CA_MoveUnderbondTo);

			houseBill.CA_OA_UnderbondFrom = ZGuid.Empty;
			houseBill.CA_OA_UnderbondTo = ZGuid.Empty;
			houseBill.CA_MoveUnderbondFrom = ZString.Empty;
			houseBill.CA_MoveUnderbondTo = ZString.Empty;

			houseBill.CA_MoveUnderbondFrom = SouthShedPremiseID;
			houseBill.CA_MoveUnderbondTo = WestShedPremiseID;

			AssertEquals("From Premise Address From Code", unpackPremiseSouth.PK, houseBill.CA_OA_UnderbondFrom);
			AssertEquals("To Premise Address From Code", unpackPremiseWest.PK, houseBill.CA_OA_UnderbondTo);
		}

		public void TestShipment()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			CusSCAHouse houseBill = oceanBill.HouseBills[0];
			houseBill.CA_JS = shipment.PK;
			AssertNotNull(houseBill.Shipment);
		}

		public void TestWeGetShipmentsReference()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			CusSCAHouse houseBill = oceanBill.HouseBills[0];
			houseBill.CA_JS = shipment.PK;
			Factory.Save();
			AssertEquals(shipment.JS_UniqueConsignRef, houseBill.CA_BGMReference);
		}

		public void TestWeGetShipmentsReferenceEvenIfWereAlreadyInDB()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			CusSCAHouse houseBill = oceanBill.HouseBills[0];
			houseBill.CA_JS = shipment.PK;
			Factory.Save();
			houseBill.CA_BGMReference = ZString.Empty;
			Factory.Save();

			AssertEquals(shipment.JS_UniqueConsignRef, houseBill.CA_BGMReference);

			AssertNotNull("ErrorReported", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestWeDontGetReferenceIfWeBlowUpWhilstSaving()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			CusSCAHouse houseBill = oceanBill.HouseBills[0];
			houseBill.CA_JS = shipment.PK;
			houseBill.CA_OH_Consignee = Guid.NewGuid();
			bool exceptionThrown = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				exceptionThrown = true;
			}
			Assert(exceptionThrown);
			AssertEquals(ZString.Empty, houseBill.CA_BGMReference);
		}

		public void TestDelete()
		{
			CommonConsol consol = CreateGroupageConsol();
			CommonContainer container = AddContainerToConsol(consol, ContainerNumber1);
			CommonShipment shipment = AddShipmentToConsol(consol, HouseBillNumber1);

			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			AssertEquals(1, synchroniser.OceanBill.HouseBills.Count);
			CusSCAHouse houseBill1 = synchroniser.GetHouseBill(shipment);
			houseBill1.Deleted += new EventHandler(HouseBill1_Deleted);
			ZGuid housePK = houseBill1.PK;
			AssertEquals("OceanBill should have 1 house bill", 1, synchroniser.OceanBill.HouseBills.Count);
			AssertNotNull("Pivot should exist been removed", Factory.LoadTop1<CusSCAPivot>(new ZQuery(CusSCAPivotSchema.CV_CA, housePK)));
			houseBill1.Delete();
			AssertNull("Should remove connection to shipment when deleted", Factory.LoadTop1<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_JS, shipment.PK)));
			AssertEquals("House Bill should have been removed from Ocean Bills", 0, synchroniser.OceanBill.HouseBills.Count);
			AssertNull("Pivot should have been removed", Factory.LoadTop1<CusSCAPivot>(new ZQuery(CusSCAPivotSchema.CV_CA, housePK)));
			AssertNotNull("A new housebill should be retrieved if the previous one was deleted", synchroniser.GetHouseBill(shipment));
			Assert("Call Deleted EventHandler", deletedCalled);
		}

		bool deletedCalled;
		void HouseBill1_Deleted(object sender, EventArgs e)
		{
			deletedCalled = true;
		}

		public void TestDeletedFromDataRefresh()
		{
			AssertDeletedEvent((houseBill) =>
			{
				var newFactory = NewFactory();

				var houseBillInNewFactory = newFactory.Load<CusSCAHouse>(houseBill.PK);
				houseBillInNewFactory.Delete();

				newFactory.Save();
			});
		}

		void AssertDeletedEvent(Action<CusSCAHouse> deleteAction)
		{
			var consol = CreateGroupageConsol();
			var container = AddContainerToConsol(consol, ContainerNumber1);
			var shipment = AddShipmentToConsol(consol, HouseBillNumber1);

			var synchroniser = GetSeaCargoSynchroniser(consol);
			var houseBill = synchroniser.GetHouseBill(shipment);

			var isDeletedCalled = false;

			Factory.Save();

			houseBill.Deleted += (s, e) => isDeletedCalled = true;

			var housePK = houseBill.PK;
			AssertEquals("OceanBill should have 1 house bill", 1, synchroniser.OceanBill.HouseBills.Count);
			AssertNotNull("Pivot should exist been removed", Factory.LoadTop1<CusSCAPivot>(new ZQuery(CusSCAPivotSchema.CV_CA, housePK)));

			Assert("Precondition.", !houseBill.IsDeleted);
			Assert("Precondition.", !isDeletedCalled);

			deleteAction(houseBill);

			Assert("Should be deleted from data refresh.", houseBill.IsDeleted);

			AssertNull("Should remove connection to shipment when deleted from data refresh", Factory.LoadTop1<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_JS, shipment.PK)));
			AssertEquals("House Bill should have been removed from Ocean Bills", 0, synchroniser.OceanBill.HouseBills.Count);

			AssertNull("Pivot should have been removed", Factory.LoadTop1<CusSCAPivot>(new ZQuery(CusSCAPivotSchema.CV_CA, housePK)));
			AssertNotEquals("A new housebill should be retrieved if the previous one was deleted", houseBill.PK, synchroniser.GetHouseBill(shipment).PK);

			Assert("Call Deleted EventHandler", isDeletedCalled);
		}

		public void TestNew()
		{
			AssertNotNull(CusSCAHouse.New(Factory));
		}

		public void TestWeDontCareAboutTheMessageErrorsOfAnotherShipmentOrContainer()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container1 = cMROceanBill.Containers.AddNew();
			CusSCAContainer container2 = cMROceanBill.Containers.AddNew();
			CusSCAHouse house1 = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot1 = house1.Pivot.AddNew();
			pivot1.CV_CN = container1.PK;
			CusSCAHouse house2 = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot2 = house2.Pivot.AddNew();
			cMROceanBill.CB_PrincipalID = ABNNumberWithOutSpaces;
			cMROceanBill.CB_OceanBill = "OCEANBILL";
			cMROceanBill.CB_RL_NKPortOfLoading = "USLAX";
			cMROceanBill.CB_ResponsiblePartyID = ABNNumberWithOutSpaces;
			cMROceanBill.CB_Voyage = "34A";
			cMROceanBill.CB_LloydsIMO = "8610033";
			pivot2.CV_CN = container1.PK;
			AssertNotNull(house1.OceanBill);
			AssertNotNull(house2.OceanBill);
			container1.CN_ContainerNumber = ContainerNumber1;
			container2.CN_ContainerNumber = ContainerNumber2;
			container1.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.LCL;
			container2.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.LCL;
			container1.CN_RC_NKContainerType = "40GP";
			container2.CN_RC_NKContainerType = "40GP";
			house1.CA_HouseBill = HouseBillNumber1;
			house2.CA_HouseBill = HouseBillNumber2;
			house1.CA_OA_ConsigneeAddress = CreateConsignee("AUSYDCEE").MainAddress.PK;
			house1.CA_OA_ConsignorAddress = CreateConsignee("AUSYDCOR").MainAddress.PK;
			house2.CA_OA_ConsigneeAddress = CreateConsignee("A2SYDCEE").MainAddress.PK;
			house2.CA_OA_ConsignorAddress = CreateConsignee("A2SYDCOR").MainAddress.PK;
			house1.CA_RL_NK_PortOfOrigin = "USLAX";
			house2.CA_RL_NK_PortOfOrigin = "USLAX";
			house1.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.PrepaidOnly;
			house2.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.PrepaidOnly;
			house1.CA_RN_NKGoodsOrigin = "US";
			house2.CA_RN_NKGoodsOrigin = "US";
			pivot1.CV_PackageCount = 10;
			pivot1.CV_PackageType = CMRPackageTypes.Codes.Box;
			pivot1.CV_Weight = 1000m;
			pivot1.CV_Volume = 1m;
			pivot1.CV_GoodsDescription = "Description";
			pivot1.CV_MarksAndNumbers = "MARKS";
			pivot2.CV_PackageCount = 20;
			pivot2.CV_PackageType = CMRPackageTypes.Codes.Box;
			pivot2.CV_Weight = 2000m;
			pivot2.CV_Volume = 2m;
			pivot2.CV_GoodsDescription = "Description";
			pivot2.CV_MarksAndNumbers = "MARKS";
			cMROceanBill.RunPreSaveValidation();
			house1.RunPreSaveValidation();
			house2.RunPreSaveValidation();
			container1.RunPreSaveValidation();
			container2.RunPreSaveValidation();
			pivot1.RunPreSaveValidation();
			pivot2.RunPreSaveValidation();
			AssertNoMessageErrors("Container Should not have an message Errors", container1);
			if (cMROceanBill.CB_PrincipalIDInfo.HasMessageErrors())
			{
				cMROceanBill.CB_PrincipalID = "C123456789";
			}

			AssertNoMessageErrors("Ocean Bill Should not have an message Errors", cMROceanBill);
			if (house1.HasMessageErrors)
			{
				house1.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.PrepaidOnly;
				house2.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.PrepaidOnly;
			}
			AssertNoMessageErrors("House1 Should not have an message Errors", house1);
			AssertNoMessageErrors("House2 Should not have an message Errors", house2);
			AssertNoMessageErrors("Pivot1 Should not have an message Errors", pivot1);
			AssertNoMessageErrors("Pivot2 Should not have an message Errors", pivot2);

			house1.CA_HouseBill = "";
			AssertHasMessageErrors("MessageErrors", house1.CA_HouseBillInfo);
			AssertNoMessageErrors("NoMessageErrors", house2);
			container2.CN_ContainerNumber = "";
			AssertHasMessageErrors("HasMessageErrors", house1.CA_HouseBillInfo);
			AssertNoMessageErrors("NoMessageErrors", house2);
		}

		public void TestBGMReferenceIsPopulatedForStandAloneHouseBills()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			AssertEquals("CA_BGMReference.IsEmpty", true, house.CA_BGMReference.IsEmpty);
			Factory.Save();
			AssertEquals("CA_BGMReference.StartsWith(L)", true, house.CA_BGMReference.StartsWith("L"));
		}

		public void TestLoadFromICusSCAHouseInfoProvider()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB123";
			oceanBill.CB_Voyage = " 039sX";

			oceanBill.CB_VesselName = "ADMIRALENGRACHT";
			var house = oceanBill.HouseBills.AddNew();
			house.CA_HouseBill = "HB321";

			var info = new TestHelperCusSCAHouseInfoProvider();
			info.HouseBillNumber = "HB322";
			info.OceanBillNumber = "OB123";
			info.VoyageNumber = "0 39Sx";
			info.LloydsNumber = "8811924";

			AssertEquals("Result", null, CusSCAHouse.Load(Factory, info));
			info.HouseBillNumber = "HB321";

			AssertEquals("Result", house, CusSCAHouse.Load(Factory, info));
			info.OceanBillNumber = "OB124";
			AssertEquals("Result", null, CusSCAHouse.Load(Factory, info));
		}

		public void TestDefaultValues()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_RL_NKPortOfLoading = "SGSIN";
			oceanBill.CB_RL_NKPortOfDischarge = "AUMEL";

			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			AssertEquals("House Bill Origin Default", "SGSIN", house.CA_RL_NK_PortOfOrigin);
			AssertEquals("House Bill Destination Default", "AUMEL", house.CA_RL_NK_PortOfDestination);
		}

		public void TestAggregatedMasterHouseBill()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			AssertEquals("AggregatedMasterHouseBill", ZString.Empty, house.AggregatedMasterHouseBill);
			oceanBill.CB_MasterHouseBill = "123";
			AssertEquals("AggregatedMasterHouseBill", "123", house.AggregatedMasterHouseBill);
			house.CA_MasterHouseBill = "321";
			AssertEquals("AggregatedMasterHouseBill", "321", house.AggregatedMasterHouseBill);
		}

		public void TestPartyDetailsUpdateWhenChangingOrganisationOnAcknowledgedContainer()
		{
			const string Consignee1Name = "RONALD FRENT";
			const string Consignee2Name = "BUBBA GUMP";
			const string Consignor1Name = "Fred";
			const string Consignor2Name = "Barney";
			const string NotifyParty1Name = "Purple";
			const string NotifyParty2Name = "Blue";
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			house.CA_OA_ConsigneeAddress = CreateConsignee(Consignee1Name).MainAddress.PK;
			house.CA_OH_Notify = CreateConsignee(NotifyParty1Name).PK;
			house.CA_OA_ConsignorAddress = CreateConsignor(Consignor1Name).MainAddress.PK;
			Assert("Address line should still contail details for consignee", house.CA_ConsigneeAddress1.Contains(Consignee1Name));
			Assert("Address line should still contail details for consignor", house.CA_ConsignorAddress1.Contains(Consignor1Name));
			Assert("Address line should still contail details for notify", house.CA_NotifyAddress1.Contains(NotifyParty1Name));

			house.CA_OA_ConsigneeAddress = CreateConsignee(Consignee2Name).MainAddress.PK;
			house.CA_OH_Notify = CreateConsignee(NotifyParty2Name).PK;
			house.CA_OA_ConsignorAddress = CreateConsignor(Consignor2Name).MainAddress.PK;
			Assert("Address line should contail details for consignee 2", house.CA_ConsigneeAddress1.Contains(Consignee2Name));
			Assert("Address line should contail details for consignor 2", house.CA_ConsignorAddress1.Contains(Consignor2Name));
			Assert("Address line should contail details for notify 2", house.CA_NotifyAddress1.Contains(NotifyParty2Name));
		}

		public void TestConsigneeConsignorLists()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			AssertEquals("Consignee List", typeof(ConsigneeCollection), houseBill.CA_OH_Consignee_List.GetType());
			AssertEquals("Consignor List", typeof(ConsignorCollection), houseBill.CA_OH_Consignor_List.GetType());
			houseBill.CA_IsMasterHouse = true;
			AssertEquals("Consignee Forwarder List", typeof(ForwarderCollection), houseBill.CA_OH_Consignee_List.GetType());
			AssertEquals("Consignor Forwarder List", typeof(ForwarderCollection), houseBill.CA_OH_Consignor_List.GetType());
		}

		public void TestStatusFieldsReadOnly()
		{
			CusSCAHouse house = Factory.New<CusSCAHouse>();
			AssertEquals("Custums Status - CA_ShipmentStatus ReadOnly", true, house.CA_ShipmentStatusInfo.ReadOnly);
			AssertEquals("Messaging Status - CA_MessageStatus ReadOnly", true, house.CA_MessageStatusInfo.ReadOnly);
		}

		const string ABNNumberWithSpaces = "75 006 687 958";
		const string ABNNumberWithOutSpaces = "75006687958";
		public void TestABNSpacesStripped()
		{
			CusSCAHouse house = Factory.New<CusSCAHouse>();
			house.CA_ResponsiblePartyID = ABNNumberWithSpaces;
			AssertEquals("Responsible Party ID", ABNNumberWithOutSpaces, house.CA_ResponsiblePartyID);
		}

		[ExpectNoExceptions()]
		public void TestIssue9927_DeleteTwice()
		{
			CusSCAHouse house = Factory.New<CusSCAHouse>();
			house.Delete();
			house.Delete();
		}

		public void TestIScanHouseBillProvider()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "XXX";
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OC1";
			oceanBill.CB_VesselName = "XXX";
			oceanBill.CB_Voyage = "123";
			var house = oceanBill.HouseBills.AddNew();
			house.CA_HouseBill = "HB1";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.StandardHouse;
			house.CA_JS = shipment.PK;
			house.CA_HouseBill = "HB1";
			house.CA_ConsigneeName = "CONSIGNEE NAME";

			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "CN1";
			var pivot = container.Pivots.AddNew();
			pivot.CV_CA = house.PK;
			pivot.CV_PackageCount = 4;
			pivot.CV_PackageType = "XX";
			pivot.CV_GoodsDescription = "GOODS DESCRIPTION";
			pivot.CV_MarksAndNumbers = "MARKS AND NUMBERS";
			pivot.CV_CargoStatus = "CLR";
			container.Pivots.AddNew().CV_CA = house.PK;
			var underbond = container.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "12345";

			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_VesselName = "XXX";
			outturnHeader.C6_OutturningPremiseID = "12345";
			outturnHeader.C6_VoyageNum = "123";
			var line1 = outturnHeader.Outturns.AddNew();
			line1.C5_MasterBill = "OC1";
			line1.C5_HouseBill = "HB1";
			line1.C5_ContainerNumber = "CN1";
			line1.C5_PackagesOutturned = 1;
			var line2 = outturnHeader.Outturns.AddNew();
			line2.C5_MasterBill = "OC1";
			line2.C5_HouseBill = "HB1";
			line2.C5_ContainerNumber = "CN1";
			line2.C5_PackagesOutturned = 2;

			var iScan = (IScanHouseBillProvider)house;
			AssertEquals(Enterprise.Core.Constants.ShipmentTypes.StandardHouse, iScan.ShipmentType);
			AssertEquals("HB1", iScan.HouseBill);
			AssertEquals("CONSIGNEE NAME", iScan.ConsigneeName);

			var manifestInfo = iScan.GetManifestInformation(underbond);
			AssertEquals(4, manifestInfo.Quantity);
			AssertEquals("XX", manifestInfo.UQ);
			AssertEquals("GOODS DESCRIPTION", manifestInfo.GoodsDescription);
			AssertEquals("MARKS AND NUMBERS", manifestInfo.MarksAndNumbers);
			AssertEquals("CLR", manifestInfo.CustomsStatus);
			AssertEquals(pivot.PK, manifestInfo.PK);
			AssertEquals(CusSCAPivotSchema.Constants.Prefix, manifestInfo.TablePrefix);
		}

		public void TestIMessageManageableBizObj()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			Customs.Business.IMessageManageableBizObj bizObj = house;

			AssertEquals("MessageManager", typeof(CusSCAHouseMessageManager), bizObj.GetMessageManagerForAmendmentDetection().GetType());
		}

		public void TestIsForAirCargo()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			AssertEquals(false, ((ICusUnderbondUnionCollectionParent)house).IsForAirCargo);
		}

		public void TestCMRStartingStatus()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			AssertEquals("CA_MessageStatus", CMRBaseStatuses.Codes.NotSent, house.CA_MessageStatus);
			Assert("CA_ShipmentStatus", house.CA_ShipmentStatus.IsEmpty);
			Assert("InitialShipmentStatus", house.ShipmentStatus.IsEmpty);
		}

		public void TestCA_MessageStatusForCMR()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			house.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals(CMRBaseStatuses.Codes.OriginalAccepted, house.CA_MessageStatus);
		}

		public void TestMessageStatusForCMR()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			house.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals(CMRBaseStatuses.Descriptions.OriginalAccepted, house.MessageStatus);
		}

		public void TestDetails()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			CusSCAHouse houseBill = oceanBill.HouseBills[0];
			string expectedResult = @"OCEAN BILL DETAILS:
Ocean Bill: SUDU400014772110
Load Port: NZAKL
Discharge Port: AUSYD
Vessel: AUSTRALIAN STAR
Voyage: 442

HOUSE BILL DETAILS:
House Bill: HB00001
Origin: NZAKL
Destination: AUBNE";
			AssertMultilineASCIIEquals("Details", expectedResult, houseBill.Details);
		}

		public void TestShortDescription()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			CusSCAHouse houseBill = oceanBill.HouseBills[0];
			AssertEquals("ShortDescription", "Ocean Bill: SUDU400014772110", houseBill.ShortDescription);
		}

		public void TestOceanBillIsNullAfterWeSetForeignKeyToEmpty()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			AssertNotNull(house.OceanBill);
			house.CA_CB = ZGuid.Empty;
			AssertNull(house.OceanBill);
		}

		public void TestAllUnderbonds()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			AssertNotNull(((ICusUnderbondUnionCollectionParent)house).AllUnderbonds);
		}

		public void TestGetAllPossibleUnderbondCollectionProvider()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot = house.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			ICusUnderbondDependentCollectionParent[] result = ((ICusUnderbondUnionCollectionParent)house).GetAllPossibleCollectionProviders();
			AssertEquals("Length", 2, result.Length);
			AssertEquals("Result[0]", pivot, result[0]);
			AssertEquals("Result[1]", container, result[1]);
		}

		public void TestStatusNeedsRecalculation()
		{
			var newFactory = new BusinessObjectFactory();
			var oceanBill = newFactory.New<CusSCAOceanBill>();
			oceanBill.HouseBills.AddNew();
			newFactory.Save();

			var cMROceanBill = Factory.Load<CusSCAOceanBill>(oceanBill.PK);
			var house = cMROceanBill.HouseBills[0];
			Factory.Save();
			AssertEquals("StatusNeedsRecalculation", false, house.StatusNeedsRecalculation);
			AssertEquals("Do not hit the DB to check StatusNeedsRecalculation if we haven't touched the Messages collection.", 0, Factory.GetTableHitCount(EDIMessageSchema.Constants.TableName));
			house.Messages.AddNew();
			house.Messages[0].HasChanges = true;
			AssertEquals("StatusNeedsRecalculation", true, house.StatusNeedsRecalculation);
		}

		public void TestUserFriendlyStatuses()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			AssertNotNull(house.UserFriendlyStatuses);
			AssertEquals(typeof(ZPropertyInfoString), house.UserFriendlyStatusesInfo.GetType());
		}

		public void TestLoadFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			house.CA_JS = shipment.PK;
			//Factory.Save();
			AssertEquals("LoadedHouse", house, CusSCAHouse.LoadFromShipment(shipment));
		}

		public void TestShipmentStatusForCMRIncludesCargoStatuses()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			house.Pivot.AddNew().CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			AssertEquals(CMRConsolidatedCargoStatuses.Descriptions.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, house.ShipmentStatus);
		}

		public void TestLookups()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			AssertNotNull("nullness", house.Lookups);
			AssertEquals("type", typeof(CusSCAHouseLookups), house.Lookups.GetType());
		}

		public void TestDefaultCMRValues()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_RL_NKPortOfLoading = "SGSIN";
			oceanBill.CB_RL_NKPortOfDischarge = "AUMEL";

			Env.Registry.ConsolPaymentTerm = Enterprise.Core.Constants.PaymentType.Prepaid;
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			AssertEquals("House Bill Pre-paid/Collect", CMRMethodsOfPayment.Codes.PrepaidOnly, house.CA_PrepaidCollectOther);

			Env.Registry.ConsolPaymentTerm = Enterprise.Core.Constants.PaymentType.Collect;
			house = oceanBill.HouseBills.AddNew();
			AssertEquals("House Bill Pre-paid/Collect", CMRMethodsOfPayment.Codes.Collect, house.CA_PrepaidCollectOther);
		}

		public void TestCanBeDeletedBasedOnMessageStatus()
		{
			var house = CusSCAHouse.New(Factory);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.WithdrawalAccepted;
			Assert(house.CanDelete);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.WithdrawalRejected;
			AssertEquals(false, house.CanDelete);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalRejected;
			Assert(house.CanDelete);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals(false, house.CanDelete);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.NotSent;
			Assert(house.CanDelete);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
			AssertEquals(false, house.CanDelete);

			house.CA_MessageStatus = ZString.Empty;
			Assert(house.CanDelete);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals(false, house.CanDelete);

			house.CA_MessageStatus = "WTD";
			Assert(house.CanDelete);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			AssertEquals(false, house.CanDelete);
		}

		public void TestCanBeDeleted()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			CusSCAContainer container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "CONTAINER1";
			Factory.Save();
			AssertEquals("HouseBill CanDelete", true, house.CanDelete);
			var pivot = house.Pivot.AddNew();
			pivot.CV_CN = container1.PK;
			Factory.Save();
			AssertEquals("Pivot CanDelete", true, pivot.CanDelete);
			AssertEquals("HouseBill CanDelete", true, pivot.CanDelete);

			CusUnderbond underbond = pivot.Underbonds.AddNew();
			Factory.Save();
			AssertEquals("Underbond CanDelete", true, underbond.CanDelete);
			AssertEquals("Pivot CanDelete", true, pivot.CanDelete);
			AssertEquals("HouseBill CanDelete", true, house.CanDelete);

			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals("Underbond CanDelete", false, underbond.CanDelete);
			AssertEquals("Pivot CanDelete", false, pivot.CanDelete);
			AssertEquals("HouseBill CanDelete", false, house.CanDelete);

			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;
			AssertEquals("Underbond CanDelete", true, underbond.CanDelete);
			AssertEquals("Pivot CanDelete", true, pivot.CanDelete);
			AssertEquals("HouseBill CanDelete", true, house.CanDelete);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals("Underbond CanDelete", true, underbond.CanDelete);
			AssertEquals("Pivot CanDelete - Can not delete Pivot when House is Awaiting Original", false, pivot.CanDelete);
			AssertEquals("HouseBill CanDelete", false, house.CanDelete);
		}

		public void TestCanBeDeletedExceptionNotThrown()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();
			AssertEquals("Pivot CanDelete", true, pivot.CanDelete);
			AssertEquals("HouseBill CanDelete", true, house.CanDelete);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Assert(!house.CanDelete);
			AssertEquals("You cannot delete this HouseBill as there are sent messages", house.ReasonForNotAbleToDelete);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var houseLoaded = factory2.Load<CusSCAHouse>(house.PK);
			houseLoaded.CA_MessageStatus = CMRBaseStatuses.Codes.NotSent;

			houseLoaded.Delete();
			factory2.Save();

			Assert("House should have been deleted", houseLoaded.IsDeleted);
		}

		#region TestHelperCusSCAHouseInfoProvider

		class TestHelperCusSCAHouseInfoProvider : ICusSCAHouseInfoProvider
		{
			ZString fLloydsNumber;
			public ZString LloydsNumber
			{
				get
				{
					return fLloydsNumber;
				}
				set
				{
					fLloydsNumber = value;
				}
			}

			ZString fVoyageNumber;
			public ZString VoyageNumber
			{
				get
				{
					return fVoyageNumber;
				}
				set
				{
					fVoyageNumber = value;
				}
			}

			ZString fOceanBillNumber;
			public ZString OceanBillNumber
			{
				get
				{
					return fOceanBillNumber;
				}
				set
				{
					fOceanBillNumber = value;
				}
			}

			ZString fHouseBillNumber;
			public ZString HouseBillNumber
			{
				get
				{
					return fHouseBillNumber;
				}
				set
				{
					fHouseBillNumber = value;
				}
			}
		}

		#endregion

		#region Implementation

		const string WestShedPremiseID = "F304J";
		const string EastShedPremiseID = "F239D";
		const string SouthShedPremiseID = "H483D";
		const string TestControlledPremiseID = "FH36A";
		const string TestOrgHeaderName = "Controlled Premise ID Organisation Header";

		OrgAddress CreateControlledPremiseIDAddress()
		{
			//OrgHeader
			OrgHeader cPIDHeader = Factory.New<OrgHeader>();
			cPIDHeader.OH_Code = "CPIDHEADER";
			cPIDHeader.OH_FullName = TestOrgHeaderName;
			cPIDHeader.OH_IsUnpackDepot = true;
			cPIDHeader.OH_RL_NKClosestPort = "AUSYD";
			//OrgAddress
			OrgAddress unpackDepotAddress = cPIDHeader.Addresses.AddNew();
			unpackDepotAddress.OA_Address1 = "The Docks";
			unpackDepotAddress.OA_PostCode = "2000";
			//OrgCusCode
			unpackDepotAddress.LocalControlledPremisesID = TestControlledPremiseID;
			return unpackDepotAddress;
		}

		OrgHeader CreateDepot()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "Test DEPOT";
			result.OH_IsUnpackDepot = true;
			result.OH_RL_NKClosestPort = "AUSYD";
			result.MainAddress.OA_Address1 = "Depot land";
			return result;
		}

		protected SeaCargoSynchroniser GetSeaCargoSynchroniser(CommonConsol consol)
		{
			return new CMRSeaCargoSynchroniser(consol);
		}

		#endregion
	}
}
