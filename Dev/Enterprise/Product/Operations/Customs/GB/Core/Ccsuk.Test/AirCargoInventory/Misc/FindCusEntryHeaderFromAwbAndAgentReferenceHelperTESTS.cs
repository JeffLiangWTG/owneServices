using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class FindCusEntryHeaderFromAwbAndAgentReferenceHelper_ForeignKeyTests : TestCaseWithFactory
	{
		public void TestShipmentHawbDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_JS = shipment.PK;
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("PreReq: CS and JE are now linked by FK", declaration, hawb.Declaration);
			var helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(hawb);
			AssertEquals("Helper finds entry by hawb-dec-ceh FK links", ceh, helper.GetEntryFromAwbAsBestWeCan());

			hawb.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			AssertEquals("Helper finds entry by hawb-shipment-dec-ceh FK links", ceh, helper.GetEntryFromAwbAsBestWeCan());
		}

		public void TestConsolShipmentBasicDeclaration()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			var workerShipmentForDeclaration = consol.Shipments.AddNew();
			var basic = Factory.New<CusMAWB>();
			basic.CM_JK = consol.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = workerShipmentForDeclaration.PK;
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			basic.MasterLevelHouseHelper.CS_JE_CustomsFormalEntry = declaration.PK;
			var helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(basic);
			AssertEquals("Helper finds entry by basic-workerHawb-shipment-dec-ceh FK links", ceh, helper.GetEntryFromAwbAsBestWeCan());
			basic.MasterLevelHouseHelper.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			AssertEquals("Helper finds entry by basic-consol-workerShipment-dec-ceh FK links", ceh, helper.GetEntryFromAwbAsBestWeCan());
		}

		public void TestSplitBasicOwnDeclaration()
		{
			var basic = Factory.New<CusMAWB>();
			var split = (SplitBasic)basic.Splits.AddNew();
			RunSplitOwnDeclarationTest(split);
		}

		public void TestSplitHouseOwnDeclaration()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var split = (SplitHouse)hawb.Splits.AddNew();
			RunSplitOwnDeclarationTest(split);
		}

		void RunSplitOwnDeclarationTest(SplitConsignment split)
		{
			var declaration = Factory.New<JobDeclaration>();
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = split.PK;
			pivot.XX_Relation2ID = declaration.PK;
			AssertEquals("PreReq", declaration, split.OwnDeclaration);
			var helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(split);
			AssertEquals("Helper finds entry by split-genPivot-dec-ceh FK links", ceh, helper.GetEntryFromAwbAsBestWeCan());
		}

		public void TestShipmentHawbSplitsOneDeclarationMultipleEntries()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			var split01 = (SplitHouse)hawb.Splits.AddNew();
			var split02 = (SplitHouse)hawb.Splits.AddNew();
			RunMultiEntryHeaderSplitTest(shipment, split01, split02);
		}

		public void TestConsolShipmentBasicSplitsOneDeclarationMultipleEntries()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_AgentType = Constants.AgentType.Direct;
			var workerShipment = consol.Shipments.AddNew();
			var basic = Factory.New<CusMAWB>();
			basic.CM_JK = consol.PK;
			var split01 = (SplitBasic)basic.Splits.AddNew();
			var split02 = (SplitBasic)basic.Splits.AddNew();
			split01.SplitReference = "01";
			split02.SplitReference = "02";
			RunMultiEntryHeaderSplitTest(workerShipment, split01, split02);
		}

		void RunMultiEntryHeaderSplitTest(ForwardingShipment shipmentOrWorkerShipment, SplitConsignment split01, SplitConsignment split02)
		{
			split01.SplitReference = "01";
			split02.SplitReference = "02";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_JS = shipmentOrWorkerShipment.PK; // this links hawb directly to declaration via CS_JE
			var invoice01 = declaration.Invoices.AddNew();
			var invoice02 = declaration.Invoices.AddNew();
			invoice01.ZG_HouseSplitReference = "01";
			invoice02.ZG_HouseSplitReference = "02";
			var line1 = invoice01.InvoiceLines.AddNew();
			line1.JI_Tariff = "123";
			var line2 = invoice02.InvoiceLines.AddNew();
			line2.JI_Tariff = "123";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			AssertEquals("PreReq, 2 entries", 2, declaration.ActiveEntryHeaders.Count);

			var ceh01 = (from CusEntryHeader ceh in declaration.CustomsEntryHeaders where ceh.RandomHeader.ZG_HouseSplitReference == "01" select ceh).FirstOrDefault();
			var ceh02 = (from CusEntryHeader ceh in declaration.CustomsEntryHeaders where ceh.RandomHeader.ZG_HouseSplitReference == "02" select ceh).FirstOrDefault();

			var helper01 = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(split01);
			AssertEquals("Helper finds entry by split-hawb-(shipment-)declaration by FK links and then CEH from split number", ceh01, helper01.GetEntryFromAwbAsBestWeCan());
			var helper02 = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(split02);
			AssertEquals("Helper finds entry by split-hawb-(shipment-)declaration by FK links and then CEH from split number", ceh02, helper02.GetEntryFromAwbAsBestWeCan());
		}
	}

	class FindCusEntryHeaderFromAwbAndAgentReferenceHelper_NaturalKeyTests : TestCaseWithFactory
	{
		public void TestHawbDeclaration()
		{
			CreateIrrelevantDeclaration();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "11122222222";
			mawb.CargoTerminalOperator = "BAC";
			mawb.CargoTerminalOperatorAirport = "LHR";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "33333333";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterUCR = "HBAC1112222222233333333";
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			var helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(hawb);
			AssertEquals("Helper finds entry by MUCR", ceh, helper.GetEntryFromAwbAsBestWeCan());
		}

		void CreateIrrelevantDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MasterUCR = "YXXX66677777778888888899";
			var ceh = dec.CustomsEntryHeaders.AddNew();
		}

		public void TestBasicDeclaration()
		{
			CreateIrrelevantDeclaration();
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "11122222222";
			basic.CargoTerminalOperator = "BAC";
			basic.CargoTerminalOperatorAirport = "LHR";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterUCR = "HBAC11122222222";
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			var helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(basic);
			AssertEquals("Helper finds entry by MUCR", ceh, helper.GetEntryFromAwbAsBestWeCan());
		}

		public void TestSplitHawbDeclaration()
		{
			CreateIrrelevantDeclaration();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "11122222222";
			mawb.CargoTerminalOperator = "BAC";
			mawb.CargoTerminalOperatorAirport = "LHR";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "33333333";
			var split = hawb.Splits.AddNew();
			split.SplitReference = "44";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterUCR = "HBAC111222222223333333344";
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			var helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(split);
			AssertEquals("Helper finds entry by MUCR", ceh, helper.GetEntryFromAwbAsBestWeCan());
		}

		public void TestSplitBasicDeclaration()
		{
			CreateIrrelevantDeclaration();
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "11122222222";
			basic.CargoTerminalOperator = "BAC";
			basic.CargoTerminalOperatorAirport = "LHR";
			var split = basic.Splits.AddNew();
			split.SplitReference = "44";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterUCR = "HBAC11122222222        44";
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			var helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(split);
			AssertEquals("Helper finds entry by MUCR", ceh, helper.GetEntryFromAwbAsBestWeCan());
		}

		public void TestAgentReferenceMatch()
		{
			var dunstableBranch = CuscarInboundParserTests.GetDunstableBranchPkForTest(Factory);
			using (DisposableEnvironment.ForBranch(dunstableBranch.PK.ToGuid()))
			{
				CreateIrrelevantDeclaration();
				var basic = Factory.New<CusMAWB>();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_DeclarationReference = "SLHR001000";
				var ceh = declaration.CustomsEntryHeaders.AddNew();
				var helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(basic, "B0001234");
				AssertEquals("No dec even close, find nothing", null, helper.GetEntryFromAwbAsBestWeCan());

				helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(basic, "HR001000");
				AssertEquals("Match with RIGHTMOST 8 chars", ceh, helper.GetEntryFromAwbAsBestWeCan());

				helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(basic, "SLHR00100");
				AssertEquals("No exact match no rightmost match", null, helper.GetEntryFromAwbAsBestWeCan());

				var anotherDeclaration = Factory.New<JobDeclaration>();
				anotherDeclaration.JE_DeclarationReference = "SXHR001000";  // similar 
				var anotherCeh = anotherDeclaration.CustomsEntryHeaders.AddNew();
				AssertEquals("Two jobs now match with rightmost chars, so no hit returned", null, helper.GetEntryFromAwbAsBestWeCan());

				var yetAnotherDeclaration = Factory.New<JobDeclaration>();
				yetAnotherDeclaration.JE_DeclarationReference = "SLHR001001";
				var yetAnotherCeh = yetAnotherDeclaration.CustomsEntryHeaders.AddNew();
				helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(basic, "SLHR001001");
				AssertEquals("Exact match using unrealistic 9 chars back from CCSUK", yetAnotherCeh, helper.GetEntryFromAwbAsBestWeCan());

				helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(basic, "SLHR00100");
				AssertEquals("No hits, no exact match and get two results for rightmost match", null, helper.GetEntryFromAwbAsBestWeCan());

				helper = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(basic, "LHR001001");
				AssertEquals("Match on rightmost chars", yetAnotherCeh, helper.GetEntryFromAwbAsBestWeCan());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();
		}
	}
}
