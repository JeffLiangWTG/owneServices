#if !WINZOR
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.AirCargoInventory.BusinessObjects.Testing
{
	class CcsukOperationalActionApplicatorRunnerTest : TestCaseWithFactory
	{
		public void TestDetachFConsolAndShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var mawbLinkedToConsol = Factory.New<CusMAWB>();
			mawbLinkedToConsol.CM_MAWB = "11111111111";
			var mawbNotLinked = Factory.New<CusMAWB>();
			mawbNotLinked.CM_MAWB = "22222222222";
			mawbLinkedToConsol.CM_JK = consol.PK;
			var hawbLinkedToShipment = mawbLinkedToConsol.ChildBills.AddNew();
			hawbLinkedToShipment.CS_HAWB = "33333333";
			var hawbNotLinked = Factory.New<CusHAWB>();
			hawbNotLinked.CS_HAWB = "44444444";
			hawbLinkedToShipment.CS_JS = shipment.PK;

			var log = new DummyOperationalActionSectionLog();
			var applicator = new DetachFromForwardingApplicator();
			applicator.Apply(log, new BusinessObject[] { mawbLinkedToConsol, mawbNotLinked });

			AssertEquals(ZGuid.Empty, mawbLinkedToConsol.CM_JK);
			AssertEquals(shipment.PK, hawbLinkedToShipment.CS_JS);

			applicator.Apply(log, new BusinessObject[] { hawbLinkedToShipment, hawbNotLinked });
			AssertEquals(ZGuid.Empty, hawbLinkedToShipment.CS_JS);

			AssertMultilineASCIIEquals("Bad log", @"INFO: Detached MAWB from forwarding for 111-11111111
WARNING: Skipping 222-22222222 because MAWB is not linked to Forwarding
INFO: Detached HAWB from forwarding for 111-11111111-33333333
WARNING: Skipping 44444444 because HAWB is not linked to Forwarding", log.MessagesString());
		}

		public void TestRenominate()
		{
			var basicGood = Factory.New<CusMAWB>();
			basicGood.CM_MAWB = "basicGood00";
			basicGood.NumberOfPiecesExpected = 10;
			basicGood.Profile = shedPima;
			basicGood.CM_FlightNo = "BA123";
			var basicGoodWithSplits = Factory.New<CusMAWB>();
			basicGoodWithSplits.NumberOfPiecesExpected = 10;
			basicGoodWithSplits.CM_MAWB = "bGWSplits00";
			basicGoodWithSplits.Profile = shedPima;
			var basicGoodWithSplits_S1 = basicGoodWithSplits.Splits.AddNew();
			var basicGoodWithSplits_S2 = basicGoodWithSplits.Splits.AddNew();
			basicGoodWithSplits_S1.SplitReference = "01";
			basicGoodWithSplits_S2.SplitReference = "02";
			basicGoodWithSplits_S1.NumberOfPiecesExpected = 6;
			basicGoodWithSplits_S2.NumberOfPiecesExpected = 4;
			var basicBadIsAgent = Factory.New<CusMAWB>();
			basicBadIsAgent.CM_MAWB = "bBadIsAgent";
			basicBadIsAgent.Profile = agentPima;
			basicBadIsAgent.AgentBadge = "OLD";
			basicBadIsAgent.NumberOfPiecesExpected = 10;
			var basicBadIsStatus3 = Factory.New<CusMAWB>();
			basicBadIsStatus3.CM_MAWB = "bSt30000000";
			basicBadIsStatus3.AgentBadge = "OLD";
			basicBadIsStatus3.Profile = shedPima;
			basicBadIsStatus3.NumberOfPiecesExpected = 10;
			basicBadIsStatus3.SetCustomsActionCode("CW", ZDateTime.BrettsBirthday);
			var basicBadCannotMessage = Factory.New<CusMAWB>();
			basicBadCannotMessage.CM_MAWB = "bBadNoMsg00";
			basicBadCannotMessage.Profile = shedPima;
			basicBadCannotMessage.AgentBadge = "OLD";

			var mawb = Factory.New<CusMAWB>();  // Consolidation without splits should not explode when we look at its splits
			mawb.CM_MAWB = "CONSOL";
			mawb.Profile = agentPima;
			mawb.NumberOfPiecesExpected = 10;
			var house = mawb.ChildBills.AddNew();

			var log = new DummyOperationalActionSectionLog();
			var applicator = new RenominateApplicator(Factory);
			applicator.NewAgent = "DJC";
			applicator.Apply(log, new BusinessObject[] { basicGood, basicGoodWithSplits, basicBadIsAgent, basicBadIsStatus3, basicBadCannotMessage, mawb });

			AssertEquals("DJC", basicGood.AgentBadge);
			AssertEquals("The agent field on an awb with splits is not editable so is unchanged", "", basicGoodWithSplits.AgentBadge);
			AssertEquals("The agent field on a split is not editable so is unchanged", "", basicGoodWithSplits_S1.AgentBadge);
			AssertEquals("The agent field on a split is not editable so is unchanged", "", basicGoodWithSplits_S2.AgentBadge);
			AssertEquals("OLD", basicBadIsAgent.AgentBadge);
			AssertEquals("OLD", basicBadIsStatus3.AgentBadge);
			AssertEquals("OLD", basicBadCannotMessage.AgentBadge);

			AssertEquals("FRC", basicGood.Messages[0].EM_MessageSubType);
			AssertEquals(0, basicGoodWithSplits.Messages.Count);
			AssertEquals(0, basicGoodWithSplits_S1.Messages.Count);
			AssertEquals(0, basicGoodWithSplits_S2.Messages.Count);
			AssertEquals(0, basicBadIsAgent.Messages.Count);
			AssertEquals(0, basicBadIsStatus3.Messages.Count);

			AssertMultilineASCIIEquals("Bad log", @"INFO: New agent set on bas-icGood00 and FRC message queued
WARNING: Skipping bGW-Splits00 because the agent field is locked
WARNING: Skipping bGW-Splits00/01 because the agent field is locked
WARNING: Skipping bGW-Splits00/02 because the agent field is locked
WARNING: Skipping bBa-dIsAgent because the agent field is locked
WARNING: Skipping bSt-30000000 because the agent field is locked
WARNING: FRC message for bBa-dNoMsg00 could not be queued so agent has been reverted. It is not permitted to send this message using your ETSF profile when the bill lacks a flight number. This sending will be aborted. Supply a flight number and re-try the sending, or if you do not have the flight number you may consider using your agent profile to create a pre-arrival record.  In doing the latter, it should be remembered that agent pre-arrival records are expunged (archived) if not arrived within 4 days.
WARNING: Skipping CON-CONSOL because the agent field is locked", log.MessagesString());
		}

		public void TestQueryFsrWithUpdate()
		{
			var basicGood = Factory.New<CusMAWB>();
			basicGood.CM_MAWB = "basicGood00";
			basicGood.NumberOfPiecesExpected = 10;
			basicGood.Profile = shedPima;
			var basicGoodWithSplits = Factory.New<CusMAWB>();
			basicGoodWithSplits.NumberOfPiecesExpected = 10;
			basicGoodWithSplits.CM_MAWB = "bGWSplits00";
			basicGoodWithSplits.Profile = shedPima;
			var basicGoodWithSplits_S1 = basicGoodWithSplits.Splits.AddNew();
			var basicGoodWithSplits_S2 = basicGoodWithSplits.Splits.AddNew();
			basicGoodWithSplits_S1.SplitReference = "01";
			basicGoodWithSplits_S2.SplitReference = "02";
			basicGoodWithSplits_S1.NumberOfPiecesExpected = 6;
			basicGoodWithSplits_S2.NumberOfPiecesExpected = 4;

			var log = new DummyOperationalActionSectionLog();
			var applicator = new QueryWithUpdateApplicator();
			applicator.Apply(log, new BusinessObject[] { basicGood, basicGoodWithSplits });
			AssertEquals(1, basicGood.Messages.Count);
			AssertContains("'BGM++BASICGOOD00+++++FSA'", basicGood.Messages[0].EM_MessageText);
			AssertEquals("This AWB is split but we do not explictly create FSR messages for those splits.  That'll happen upon processing the response.", 1, basicGoodWithSplits.Messages.Count);
			AssertContains("'BGM++BGWSPLITS00+++++FSA'", basicGoodWithSplits.Messages[0].EM_MessageText);
		}

		readonly string shedPima = "CUKAIR98LHRXXX";
		readonly string agentPima = "CUKFFW98000YYY";
	}

	[TestedType(typeof(QueryWithUpdateApplicator))]
	class QueryWithUpdateApplicatorTest : OperationalActionMethodApplicatorTest
	{
	}

	[TestedType(typeof(DetachFromForwardingApplicator))]
	class DetachFromForwardingTest : OperationalActionMethodApplicatorTest
	{
	}

	[TestedType(typeof(RenominateApplicator))]
	class RenominateApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestLookups()
		{
			LicencingAndShedRestrictionsTests.EnsureAgentLxa();
			var agents = new RenominateApplicator(Factory).AgentsList;
			Assert("Agents list", agents.ContainsCode("LXA"));
		}
	}

	[TestedType(typeof(CcsukOperationalActionSupporterHawb))]
	class CcsukOperationalActionSupporterHawbTest : OperationalActionSupporterTest<CcsukOperationalActionSupporterHawb>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukAirInventoryHouse; }
		}
	}

	[TestedType(typeof(CcsukOperationalActionSupporterMawb))]
	class CcsukOperationalActionSupporterMawbTest : OperationalActionSupporterTest<CcsukOperationalActionSupporterMawb>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukAirInventory; }
		}
	}

	class PickUpDropOffMessageTests : TestCaseWithFactory
	{
		public void TestMakeDRP_TwoSimpleDeclarations()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			PrepareDecBasically(declaration1, 1);
			declaration1.JE_TotalNoOfPacks = 1;
			declaration1.JE_TotalWeight = 10;
			var declaration2 = Factory.New<JobDeclaration>();
			PrepareDecBasically(declaration2, 2);
			declaration2.JE_TotalNoOfPacks = 2;
			declaration2.JE_TotalWeight = 20;
			Factory.Save();
			var log = new DummyOperationalActionSectionLog();
			var applicator = new GbDeclarationDropOffMessageActionMethodApplicator();
			applicator.ETD = ZDateTime.BrettsBirthday;
			applicator.ETA = ZDateTime.BrettsBirthday.AddHours(2);
			applicator.Vehicle = "MT10ABC";
			applicator.Apply(log, new BusinessObject[] { declaration1, declaration2 });
			var message = declaration1.Messages[0];
			AssertContains(@"FTX+CIM+++
									DRP/8:
									1/ZZ0001/18SEP0000/CAR/MT10ABC/GB/18SEP0200/LHR:
									PVS:125-87654321LHRATL/T1K10MC5/WIDGETS1:
									125-87654322LHRATL/T2K20MC5/WIDGETS2'
							FTX+CIM+++
									LAST
							'UNT+4+1'".TidySpace(), message.EM_MessageText);
		}

		public void TestMakeDRP_Containerised()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			PrepareDecBasically(declaration1, 1);
			declaration1.JE_ContainerMode = "ULD";
			var cont11 = declaration1.CusContainers.AddNew();
			cont11.CO_ContainerNumber = "CONT11";
			cont11.CO_Weight = 11;
			declaration1.JE_TotalNoOfPacks = 1;
			declaration1.JE_TotalWeight = 10;
			var declaration2 = Factory.New<JobDeclaration>();
			PrepareDecBasically(declaration2, 2);
			declaration2.JE_ContainerMode = "ULD";
			declaration2.JE_TotalNoOfPacks = 2;
			declaration2.JE_TotalWeight = 100;
			var cont21 = declaration2.CusContainers.AddNew();
			cont21.CO_ContainerNumber = "CONT21";
			cont21.CO_Weight = 21;
			var cont22 = declaration2.CusContainers.AddNew();
			cont22.CO_ContainerNumber = "CONT22";
			cont22.CO_Weight = 22;
			Factory.Save();
			var log = new DummyOperationalActionSectionLog();
			var applicator = new GbDeclarationDropOffMessageActionMethodApplicator();
			applicator.ETD = ZDateTime.BrettsBirthday;
			applicator.ETA = ZDateTime.BrettsBirthday.AddHours(2);
			applicator.Vehicle = "MT10ABC";
			applicator.Apply(log, new BusinessObject[] { declaration1, declaration2 });
			var message = declaration1.Messages[0];
			AssertContains(@"FTX+CIM+++
									DRP/8:
									1/ZZ0001/18SEP0000/CAR/MT10ABC/GB/18SEP0200/LHR:
									PVS:ULD/CONT11:
									125-87654321LHRATL/T1K11MC5.5/WIDGETS1'
								FTX+CIM+++
									ULD/CONT21:
									125-87654322LHRATL/T2K21MC1.05/WIDGETS2:
									ULD/CONT22:
									125-87654322LHRATL/T2K22MC1.1/WIDGETS2:
									LAST'
								UNT+4+1'".TidySpace(), message.EM_MessageText);
		}

		public void TestMakeDRP_ContainerisedWithShipmentpackLines()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C4321";
			consol.JK_RL_NKDischargePort = "USATL";
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "AIR";
			consol.JK_MasterBillNum = "1258765432" + "1";
			var containerA = consol.Containers.AddNew();
			containerA.JC_ContainerNum = "AAAA1234567";
			var containerB = consol.Containers.AddNew();
			containerB.JC_ContainerNum = "BBBB1234567";
			var containerC = consol.Containers.AddNew();
			containerC.JC_ContainerNum = "CCCC1234567";
			var shipmentOne = consol.Shipments.AddNew();
			shipmentOne.JS_HouseBill = "HOUSEONE";
			shipmentOne.JS_RL_NKOrigin = "GBDTE";
			shipmentOne.JS_RL_NKDestination = "USATL";
			shipmentOne.JS_OuterPacks = 26;
			shipmentOne.JS_F3_NKPackType = "PKG";
			shipmentOne.JS_ActualWeight = 35m;
			shipmentOne.JS_UnitOfWeight = "KG";
			var shipmentTwo = consol.Shipments.AddNew();
			shipmentTwo.JS_HouseBill = "HOUSETWO";
			shipmentOne.OuterPackLines.RemoveAndDeleteAll(); // gets rid of the automatically-created one that appears when you set the pieces on the shipment
			var pivot1A = shipmentOne.OuterPackLines.AddNew();
			pivot1A.JL_JC = containerA.PK;
			pivot1A.JL_PackageCount = 69;
			pivot1A.JL_ActualVolume = 100;
			shipmentTwo.OuterPackLines.RemoveAndDeleteAll(); // gets rid of the automatically-created one that appears when you set the pieces on the shipment
			var pivot2B = shipmentTwo.OuterPackLines.AddNew();
			pivot2B.JL_JC = containerB.PK;
			pivot2B.JL_PackageCount = 70;
			pivot2B.JL_ActualVolume = 200;
			var pivot2C = shipmentTwo.OuterPackLines.AddNew();
			pivot2C.JL_JC = containerC.PK;
			pivot2C.JL_PackageCount = 71;
			pivot2C.JL_ActualVolume = 300;

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_JS = shipmentOne.PK;
			PrepareDecBasically(declaration1, 1);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_JS = shipmentTwo.PK;
			PrepareDecBasically(declaration2, 1);  // same consol

			var cont11 = declaration1.CusContainers.AddNew();
			cont11.CO_ContainerNumber = containerA.JC_ContainerNum;
			cont11.CO_Weight = 11;
			declaration1.JE_TotalNoOfPacks = 1;
			declaration1.JE_TotalWeight = 10;
			declaration2.JE_ContainerMode = "ULD";
			declaration2.JE_TotalNoOfPacks = 2;
			declaration2.JE_TotalWeight = 100;
			var cont21 = declaration2.CusContainers.AddNew();
			cont21.CO_ContainerNumber = containerB.JC_ContainerNum;
			cont21.CO_Weight = 21;
			var cont22 = declaration2.CusContainers.AddNew();
			cont22.CO_ContainerNumber = containerC.JC_ContainerNum;
			cont22.CO_Weight = 22;
			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var applicator = new GbDeclarationDropOffMessageActionMethodApplicator();
			applicator.ETD = ZDateTime.BrettsBirthday;
			applicator.ETA = ZDateTime.BrettsBirthday.AddHours(2);
			applicator.Vehicle = "MT10ABC";
			applicator.Apply(log, new BusinessObject[] { declaration1, declaration2 });
			var message = declaration1.Messages[0];
			AssertContains(@"FTX+CIM+++
									DRP/8:
									1/ZZ0001/18SEP0000/CAR/MT10ABC/GB/18SEP0200/LHR:
									PVS:
									125-87654321LHRATL/T1K10MC5/WIDGETS1:
									ULD/BBBB1234567'
								FTX+CIM+++
									125-87654321LHRATL/S70K21T2MC200/WIDGETS1:
									ULD/CCCC1234567:
									125-87654321LHRATL/S71K22T2MC300/WIDGETS1:
									LAST'
								UNT+4+1'".TidySpace(), message.EM_MessageText);
		}

		static void PrepareDecBasically(JobDeclaration dec, int suffix)
		{
			dec.JE_MessageType = "EXP";
			dec.JE_TransportMode = "AIR";
			dec.JE_RL_NKPortOfLoading = "GBLHR";
			dec.JE_RL_NKPortOfArrival = "USATL";
			dec.JE_CustomsProfile = "CAR";
			dec.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			dec.SubLocation = "PVS";
			dec.JE_LocationOfGoods = "LHR";
			dec.JE_MasterBill = "1258765432" + suffix;
			dec.JE_GoodsDescription = "WIDGETS" + suffix.ToString();
			dec.JE_TotalVolume = 5;
		}
	}

	[TestedType(typeof(GbDeclarationDropOffMessageActionMethodApplicator))]
	class PickupDropOffApplicatorTest : OperationalActionMethodApplicatorTest
	{
	}

	class GbDeclarationDropOffMessageActionMethodApplicatorValidationTest : TestCaseWithFactory
	{
		public void TestAllValidation()
		{
			var bizO = new GbDeclarationDropOffMessageActionMethodApplicator();
			bizO.Validation.ValidateAll();
			AssertHasErrorContaining(bizO.ETAInfo, "enter");
			AssertHasErrorContaining(bizO.ETDInfo, "enter");
			AssertHasErrorContaining(bizO.VehicleInfo, "enter");
			bizO.ETA = ZDateTime.BrettsBirthday;
			bizO.ETD = ZDateTime.BrettsBirthday;
			bizO.Vehicle = "X";
			AssertNoErrorContaining(bizO.ETAInfo, "enter");
			AssertNoErrorContaining(bizO.ETDInfo, "enter");
			AssertNoErrorContaining(bizO.VehicleInfo, "enter");
		}
	}

	static class StringExtensions
	{
		public static string TidySpace(this string dirty)
		{
			return dirty.Replace(" ", "").Replace("	", "").Replace("\r\n", "");
		}
	}
}
#endif
