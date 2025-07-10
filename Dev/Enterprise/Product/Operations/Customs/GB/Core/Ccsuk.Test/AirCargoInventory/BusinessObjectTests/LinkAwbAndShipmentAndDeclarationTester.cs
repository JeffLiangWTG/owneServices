using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class LinkAwbAndShipmentAndDeclarationTester : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			GBCustomsDataRegistry.Instance.AllowLocationOfGoodsCalculationForNotArrivedGoods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		public void TestLinkingHawbToShipmentAndDeclaration_SetShipmentOnHawbThenOnDeclaration()
		{
			SetupConsolMawbDecAndShipButDoNotLinkDecToShipment(Enterprise.Core.Constants.AgentType.Agent);
			mawb.CM_MAWB = "12387654321";
			hawb = mawb.ChildBills.AddNew();
			hawb.CargoTerminalOperatorAirport = "LHR";
			hawb.CargoTerminalOperator = "BAC";
			hawb.AgentBadge = "DVG";
			hawb.CS_HAWB = "HOUSE001";

			hawb.CS_JS = shipment.PK;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals(declaration.PK, hawb.CS_JE_CustomsFormalEntry);
			AssertEquals("LHR", declaration.JE_LocationOfGoods);
			AssertEquals("BAC", declaration.SubLocation);
			AssertEquals("DVG", declaration.JE_CustomsProfile);

			hawb.CargoTerminalOperatorAirport = "STN";
			declaration.JE_JS = Guid.Empty;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("LSA", declaration.JE_LocationOfGoods);
		}

		public void TestLinkingHawbToShipmentAndDeclaration_SetShipmentOnDeclarationThenOnHawb()
		{
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("GHI");
			SetupConsolMawbDecAndShipButDoNotLinkDecToShipment(Enterprise.Core.Constants.AgentType.Agent);
			hawb = mawb.ChildBills.AddNew();
			hawb.CargoTerminalOperatorAirport = "LHR";
			hawb.CargoTerminalOperator = "BAC";
			hawb.AgentBadge = "DVG";

			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);
			hawb.CS_JS = shipment.PK;

			AssertEquals(declaration.PK, hawb.CS_JE_CustomsFormalEntry);
			AssertEquals("Dec not updated because at the time of linking ship & dec the ship had no hawb", "ABC", declaration.JE_LocationOfGoods);
			AssertEquals("Dec not updated because at the time of linking ship & dec the ship had no hawb", "DEF", declaration.SubLocation);
			AssertEquals("Dec not updated because at the time of linking ship & dec the ship had no hawb, and the value is set again from the registry based on port", "GHI", declaration.JE_CustomsProfile);
		}

		public void TestLinkingBasicToShipmentAndDeclaration()
		{
			SetupConsolMawbDecAndShipButDoNotLinkDecToShipment(Enterprise.Core.Constants.AgentType.Direct);
			var basic = mawb;
			basic.CM_MAWB = "12387654321";
			basic.CargoTerminalOperatorAirport = "LHR";
			basic.CargoTerminalOperator = "BAC";
			basic.AgentBadge = "DVG";

			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals(declaration.PK, mawb.MasterLevelHouseHelper.CS_JE_CustomsFormalEntry);
			AssertEquals("LHR", declaration.JE_LocationOfGoods);
			AssertEquals("BAC", declaration.SubLocation);
			AssertEquals("DVG", declaration.JE_CustomsProfile);
		}

		void SetupConsolMawbDecAndShipButDoNotLinkDecToShipment(string agentType)
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = agentType;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBMAN";
			mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			mawb.AgentBadge = "XYZ";
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_LocationOfGoods = "ABC";
			declaration.SubLocation = "DEF";
			declaration.JE_CustomsProfile = "GHI";
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			shipment = consol.Shipments.AddNew();

			var aussieMawbThatWeWantToIgnore = Factory.New<Customs.Business.CusMAWB>();
			aussieMawbThatWeWantToIgnore.CM_ApplicationCode = "CMR";
			var aussieHawbThatWeWantToIgnore = aussieMawbThatWeWantToIgnore.ChildBills.AddNew();
			aussieHawbThatWeWantToIgnore.CS_JS = shipment.PK;
		}

		public void TestConsolWithMultipleBasics()
		{
			SetupConsolMawbDecAndShipButDoNotLinkDecToShipment(Enterprise.Core.Constants.AgentType.Direct);
			var basicOne_ISR = mawb;
			basicOne_ISR.CM_MAWB = "12387654321";
			basicOne_ISR.CargoTerminalOperatorAirport = "LHR";
			basicOne_ISR.CargoTerminalOperator = "BAC";
			basicOne_ISR.AgentBadge = "ONE";
			var basicTwo_Open = CheapClone(basicOne_ISR);
			basicTwo_Open.CargoTerminalOperator = "DAN";
			basicTwo_Open.AgentBadge = "TWO";
			basicTwo_Open.CargoTerminalOperatorAirport = "LGW";
			var basicThree_IAR = CheapClone(basicOne_ISR);
			basicThree_IAR.AgentBadge = "THR";
			basicThree_IAR.CargoTerminalOperator = "DJC";
			basicThree_IAR.CargoTerminalOperatorAirport = "MAN";
			basicThree_IAR.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval, ZDateTime.BrettsBirthday);
			basicOne_ISR.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterShedRemoval, ZDateTime.BrettsBirthday);

			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals(declaration.PK, basicTwo_Open.MasterLevelHouseHelper.CS_JE_CustomsFormalEntry);
			AssertEquals("Declaration is synched to 'basicTwo_Open', the only one with no CAC", "DAN", declaration.SubLocation);
			AssertEquals("Declaration is synched to 'basicTwo_Open', the only one with no CAC", "LGW", declaration.JE_LocationOfGoods);
			AssertEquals("TWO", declaration.JE_CustomsProfile);
		}

		public void TestConsolWithMultipleBasics_MultipleOpen()
		{
			SetupConsolMawbDecAndShipButDoNotLinkDecToShipment(Enterprise.Core.Constants.AgentType.Direct);
			var basicOne = mawb;
			basicOne.CM_MAWB = "12387654321";
			basicOne.CargoTerminalOperatorAirport = "LHR";
			basicOne.CargoTerminalOperator = "BAC";
			basicOne.AgentBadge = "ONE";
			var basicTwo = CheapClone(basicOne);
			basicTwo.CM_MAWB = "12387654321";
			basicTwo.CargoTerminalOperatorAirport = "LHR";
			basicTwo.CargoTerminalOperator = "BAC";
			basicTwo.AgentBadge = "TWO";
			basicTwo.CargoTerminalOperator = "DAN";

			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertNull("No log about synching", declaration.Logs.MostRecentLog);
			AssertEquals("No badge found in rego, gets wiped", "", declaration.JE_CustomsProfile);
		}

		public void TestShipmentWithMultipleHouses()
		{
			SetupConsolMawbDecAndShipButDoNotLinkDecToShipment(Enterprise.Core.Constants.AgentType.Agent);
			var mawbOne_ISR = mawb;
			mawbOne_ISR.CM_MAWB = "12387654321";
			mawbOne_ISR.CargoTerminalOperatorAirport = "LHR";
			mawbOne_ISR.CargoTerminalOperator = "BAC";
			mawbOne_ISR.AgentBadge = "DVG";
			var hawb1_ISR = mawbOne_ISR.ChildBills.AddNew();
			hawb1_ISR.CS_JS = shipment.PK;
			hawb1_ISR.CS_HAWB = "HOUSEISR";
			hawb1_ISR.CS_WarehouseLocation = "MANSLS";
			var mawbTwo_Open = CheapClone(mawbOne_ISR);
			mawbTwo_Open.CargoTerminalOperator = "DAN";
			var hawb2Open = mawbTwo_Open.ChildBills.AddNew();
			hawb2Open.CS_JS = shipment.PK;
			hawb2Open.CS_HAWB = "HOUSE001";
			hawb2Open.CS_WarehouseLocation = "LGWBAC";
			var mawbThree_IAR = CheapClone(mawbOne_ISR);
			mawbThree_IAR.CargoTerminalOperator = "DJC";
			var hawb3Iar = mawbThree_IAR.ChildBills.AddNew();
			hawb3Iar.CS_JS = shipment.PK;
			hawb3Iar.CS_HAWB = "HOUSEIAR";
			hawb3Iar.CS_WarehouseLocation = "STNCAX";
			hawb3Iar.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval, ZDateTime.BrettsBirthday);
			hawb1_ISR.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterShedRemoval, ZDateTime.BrettsBirthday);

			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals(declaration.PK, hawb2Open.CS_JE_CustomsFormalEntry);
			AssertEquals("Declaration is synched to 'hawb2Open', the only one with no CAC", "BAC", declaration.SubLocation);
			AssertEquals("Declaration is synched to 'hawb2Open', the only one with no CAC", "LGW", declaration.JE_LocationOfGoods);
			AssertEquals("DVG", declaration.JE_CustomsProfile);
		}

		CusMAWB CheapClone(CusMAWB source)
		{
			var target = Factory.New<CusMAWB>();
			target.CM_MAWB = source.CM_MAWB;
			target.CargoTerminalOperator = source.CargoTerminalOperator;
			target.CargoTerminalOperatorAirport = source.CargoTerminalOperatorAirport;
			target.AgentBadge = source.AgentBadge;
			target.CM_JK = source.CM_JK;
			return target;
		}

		JobDeclaration declaration;
		ForwardingShipment shipment;
		CusHAWB hawb;
		CusMAWB mawb;
		ForwardingConsol consol;
	}
}
