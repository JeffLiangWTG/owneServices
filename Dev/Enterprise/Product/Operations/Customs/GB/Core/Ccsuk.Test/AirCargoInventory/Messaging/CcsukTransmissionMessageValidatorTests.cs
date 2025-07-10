using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using ccsukBO = Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	public class CcsukTransmissionMessageValidatorTests : TestCaseWithFactory
	{
		public void TestSendCuscarMawb_FCS()
		{
			var mawb = CreateMawbForTest(false, Factory, true);
			var splits = GetSplitsForTest(Factory);
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FCS(splits));
			AssertContains("Cargo Terminal Operator Airport And Shed:", shutUp.ContinueWithActionMessage);
			AssertContains("Weight UQ:", shutUp.ContinueWithActionMessage);
			AssertContains("Weight:", shutUp.ContinueWithActionMessage);
			mawb = CreateMawbForTest(false, Factory, false);
			mawb.CM_MAWB = "76523456791";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FCS(splits));
			AssertEquals(null, shutUp.ContinueWithActionMessage);
		}

		public void TestSendCuscarHawb_FCS()
		{
			var mawb = CreateMawbForTest(true, Factory, true);
			var splits = GetSplitsForTest(Factory);
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb.ChildBills[0], shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FCS(splits));
			AssertContains("HAWB:", shutUp.ContinueWithActionMessage);
			AssertContains("Weight UQ:", shutUp.ContinueWithActionMessage);
			AssertContains("Weight:", shutUp.ContinueWithActionMessage);
			mawb = CreateMawbForTest(true, Factory, false);
			mawb.CM_MAWB = "67812345600";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb.ChildBills[0], shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FCS(splits));
			AssertEquals(null, shutUp.ContinueWithActionMessage);
		}

		public void TestSendCuscarMawb_FRX()
		{
			var mawb = CreateMawbForTest(false, Factory, true);
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRX());
			AssertContains("Cargo Terminal Operator Airport And Shed:", shutUp.ContinueWithActionMessage);
			mawb = CreateMawbForTest(false, Factory, false);
			mawb.CM_MAWB = "76523456791";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRX());
			AssertEquals(null, shutUp.ContinueWithActionMessage);
		}

		public void TestSendCuscarMawb_FRXWhenHawbsStillActive()
		{
			var mawb = CreateMawbForTest(true, Factory, false);
			var hawb = mawb.ChildBills[0];
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRX());
			AssertContains("This MAWB contains child house bills that are still active on CCS-UK", shutUp.ContinueWithActionMessage);
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDbDeleted;
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRX());
			AssertNull(shutUp.ContinueWithActionMessage);
		}

		public void TestSendCuscarMawb_FRXWhenBasicIsSplit()
		{
			var basic = CreateMawbForTest(false, Factory, false);
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(basic, shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRX());
			AssertNotContains("This basic AWB is split", shutUp.ContinueWithActionMessage);
			basic.Splits.AddNew();
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(basic, shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRX());
			AssertContains("This basic AWB is split", shutUp.ContinueWithActionMessage);
		}

		public void TestSendCuscarHawb_FRX()
		{
			var mawb = CreateMawbForTest(true, Factory, true);
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb.ChildBills[0], shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRX());
			AssertContains("HAWB:", shutUp.ContinueWithActionMessage);
			AssertContains("Cargo Terminal Operator Airport And Shed:", shutUp.ContinueWithActionMessage);
			mawb = CreateMawbForTest(true, Factory, false);
			mawb.CM_MAWB = "67812345600";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb.ChildBills[0], shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRX());
			AssertEquals(null, shutUp.ContinueWithActionMessage);
		}

		public void TestSendCuscarMawb_FRI()
		{
			var mawb = CreateMawbForTest(false, Factory, true);
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRI());
			AssertContains("Cargo Terminal Operator Airport And Shed:", shutUp.ContinueWithActionMessage);
			AssertContains("Weight UQ:", shutUp.ContinueWithActionMessage);
			AssertContains("Weight:", shutUp.ContinueWithActionMessage);
			AssertContains("Goods Description:", shutUp.ContinueWithActionMessage);
			AssertContains("Discharge Port:", shutUp.ContinueWithActionMessage);
			mawb = CreateMawbForTest(false, Factory, false, true);
			mawb.CM_MAWB = "67812345600";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRI());
			AssertEquals(null, shutUp.ContinueWithActionMessage);
		}

		public void TestSendCuscarHawb_FRI()
		{
			var mawb = CreateMawbForTest(true, Factory, true);
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb.ChildBills[0], shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRI());
			AssertContains("HAWB:", shutUp.ContinueWithActionMessage);
			AssertContains("Cargo Terminal Operator Airport And Shed:", shutUp.ContinueWithActionMessage);
			AssertContains("Weight UQ:", shutUp.ContinueWithActionMessage);
			AssertContains("Weight:", shutUp.ContinueWithActionMessage);
			AssertContains("Goods Description:", shutUp.ContinueWithActionMessage);
			mawb = CreateMawbForTest(true, Factory, false);
			mawb.CM_MAWB = "67812345600";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb.ChildBills[0], shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRI());
			AssertEquals(null, shutUp.ContinueWithActionMessage);
		}

		public void TestSendCIMMawb()
		{
			var mawb = CreateMawbForTest(false, Factory, true);
			mawb.CargoTerminalOperator = "";
			mawb.CargoTerminalOperatorAirport = "";
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CIM.FRN("ZZZ"));
			AssertContains("Cargo Terminal Operator Airport And Shed:", shutUp.ContinueWithActionMessage);
			mawb = CreateMawbForTest(false, Factory, false);
			mawb.CM_MAWB = "76523456791";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CIM.FRN("ZZZ"));
			AssertEquals(null, shutUp.ContinueWithActionMessage);
		}

		public void TestSendCIMHawb()
		{
			var mawb = CreateMawbForTest(true, Factory, true);
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb.ChildBills[0], shutUp, new CcsukTransmissionMessageFunction.CIM.FRN("ZZZ"));
			AssertContains("Cargo Terminal Operator Airport And Shed:", shutUp.ContinueWithActionMessage);
			mawb = CreateMawbForTest(true, Factory, false);
			mawb.CM_MAWB = "76523456791";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var hawb = mawb.ChildBills[0];
			hawb.CS_PiecesManifested = 15;
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(hawb, shutUp, new CcsukTransmissionMessageFunction.CIM.FRN("ZZZ"));
			AssertEquals(null, shutUp.ContinueWithActionMessage);
		}

		public void TestSendCusdecMawb()
		{
			var mawb = CreateMawbForTest(false, Factory, true);
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CUSDEC.ISR());
			AssertContains("Cargo Terminal Operator Airport And Shed:", shutUp.ContinueWithActionMessage);
			AssertContains("Discharge Port:", shutUp.ContinueWithActionMessage);
			mawb = CreateMawbForTest(false, Factory, false);
			mawb.CM_MAWB = "76523456791";
			mawb.AirportOfArrival = "LHR";
			mawb.AirportOfDestination = "LHR";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var isr = mawb.ISRs.AddNew();
			isr.NewShedId = "BAC";
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(isr, shutUp, new CcsukTransmissionMessageFunction.CUSDEC.ISR());
			AssertEquals(null, shutUp.ContinueWithActionMessage);
			mawb.AgentBadge = "";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(isr, shutUp, new CcsukTransmissionMessageFunction.CUSDEC.ISR());
			AssertContains("badge", shutUp.ContinueWithActionMessage);
		}

		public void TestSendCusdecHawb()
		{
			var mawb = CreateMawbForTest(true, Factory, true);
			var hawb = mawb.ChildBills[0];
			hawb.AgentBadge = "";
			hawb.CargoTerminalOperator = "";
			var tsr = hawb.TSRs.AddNew();
			tsr.LicenseRestrictionInd = "N";
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(tsr, shutUp, new CcsukTransmissionMessageFunction.CUSDEC.TSR());
			AssertContains("Cargo Terminal Operator Airport And Shed:", shutUp.ContinueWithActionMessage);
			mawb = CreateMawbForTest(true, Factory, false);
			mawb.CM_MAWB = "98712345675";
			hawb = mawb.ChildBills[0];
			tsr = hawb.TSRs.AddNew();
			hawb.AirportOfDestination = "LAX";
			tsr.OnwardMode = "40"; // air
			tsr.AirportOrCountryOfDestination = "USLAX";
			tsr.PortOfShipment = "LHR";
			tsr.LicenseRestrictionInd = "Y";
			hawb.ShipmentDescriptionCode = "E";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(tsr, shutUp, new CcsukTransmissionMessageFunction.CUSDEC.ISR());
			var msg = tsr.Notifications.GetMessageErrors().FirstOrDefault(x => x.Message.Contains("Removals for consignments with SDC=C or E are not allowed")).Message;
			AssertContains("SDC=C or E", msg);
			hawb.AirportOfDestination = "LHR";
			hawb.ShipmentDescriptionCode = "T";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(tsr, shutUp, new CcsukTransmissionMessageFunction.CUSDEC.TSR());
			AssertEquals(null, shutUp.ContinueWithActionMessage);
			hawb.CS_RL_NKDischargePort = "GBMAN";
			var isr = hawb.ISRs.AddNew();
			isr.NewShedId = "BAC";
			hawb.AgentBadge = "";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(isr, shutUp, new CcsukTransmissionMessageFunction.CUSDEC.ISR());
			AssertContains("badge", shutUp.ContinueWithActionMessage);
			hawb.AgentBadge = "LXA";
			hawb.AirportOfArrival = "LHR";
			hawb.AirportOfDestination = "LHR";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(isr, shutUp, new CcsukTransmissionMessageFunction.CUSDEC.ISR());
			AssertEquals(null, shutUp.ContinueWithActionMessage);
		}

		public void TestSendCusdecSplit()
		{
			var mawb = CreateMawbForTest(true, Factory, true);
			mawb.CargoTerminalOperatorAirportAndShed = "LHRBAC";
			var hawb = mawb.ChildBills[0];
			hawb.CS_HAWB = "12345678";
			hawb.CS_WarehouseLocation = "LHRBAC";
			var split = hawb.Splits.AddNew();
			split.SplitReference = "69";
			var tsr = hawb.TSRs.AddNew();
			tsr.SplitReferenceToWhichThisRemovalPertains = "68";
			tsr.LicenseRestrictionInd = "N";
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(tsr, shutUp, new CcsukTransmissionMessageFunction.CUSDEC.TSR());
			AssertContains("Please select a split from the list", shutUp.ContinueWithActionMessage);
			tsr.SplitReferenceToWhichThisRemovalPertains = "69";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(tsr, shutUp, new CcsukTransmissionMessageFunction.CUSDEC.TSR());
			AssertNotContains("Please select a split from the list", shutUp.ContinueWithActionMessage);
			var isr = split.ISRs.AddNew();
			isr.NewShedId = "BAC";
			isr.SplitReferenceToWhichThisRemovalPertains = "69";
			isr.C4_PiecesManifested = 1;
			split.AgentBadge = "";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(isr, shutUp, new CcsukTransmissionMessageFunction.CUSDEC.ISR());
			AssertContains("badge", shutUp.ContinueWithActionMessage);
			split.AgentBadge = "LXA";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(isr, shutUp, new CcsukTransmissionMessageFunction.CUSDEC.ISR());
			AssertEquals(null, shutUp.ContinueWithActionMessage);
		}

		public void TestSendFSRMawb()
		{
			var mawb = CreateMawbForTest(false, Factory, true);
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CUKFSR.FSA());
			mawb = CreateMawbForTest(false, Factory, false);
			mawb.CM_MAWB = "76523456791";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb, shutUp, new CcsukTransmissionMessageFunction.CUKFSR.FSA());
			AssertEquals(null, shutUp.ContinueWithActionMessage);
		}

		public void TestSendFSRHawb()
		{
			var mawb = CreateMawbForTest(true, Factory, true);
			var sender = new CcsukInventoryICusAwbMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb.ChildBills[0], shutUp, new CcsukTransmissionMessageFunction.CUKFSR.FSA());
			AssertContains("HAWB:", shutUp.ContinueWithActionMessage);
			mawb = CreateMawbForTest(true, Factory, false);
			mawb.CM_MAWB = "67812345600";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.ValidateAndShowUserAnyWarningsOrErrorsExposed(mawb.ChildBills[0], shutUp, new CcsukTransmissionMessageFunction.CUKFSR.FSA());
			AssertEquals(null, shutUp.ContinueWithActionMessage);
		}

		ccsukBO.CusMAWB CreateMawbForTest(bool createHawbToo, BusinessObjectFactory factory, bool withMissingDataButStillAbleToSave, bool shedBadge = false)
		{
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			var mawb = factory.New<ccsukBO.CusMAWB>();
			mawb.Profile = "CUKFFW98000LXA";
			mawb.CM_MAWB = "80112345678";
			mawb.CM_RL_NKLoadPort = "USLAX";
			mawb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.TotalConsignmentManifested;
			mawb.NumberOfPiecesExpected = 15;
			if (withMissingDataButStillAbleToSave)
			{
				mawb.WeightCode = "";
				if (createHawbToo)
				{
					var hawb = mawb.ChildBills.AddNew();
					hawb.CS_HAWB = "111222333";
					hawb.CS_RL_NKLoadPort = "USLAX";
					hawb.CS_PiecesManifested = 15;
				}
			}
			else
			{
				if (!shedBadge)
				{
					mawb.CM_ArrivalDate = ZDateTime.Empty;
					mawb.NumberOfPiecesReceived = 0;
				}
				else
				{
					mawb.CM_ArrivalDate = ZDateTime.Now;
					mawb.NumberOfPiecesReceived = 68;
				}
				mawb.CargoTerminalOperator = "BAC";
				mawb.CargoTerminalOperatorAirport = "LHR";
				mawb.CM_RL_NKFirstArrivalPort = "GBMAN";
				mawb.CM_RL_NKLoadPort = "USLAX";
				mawb.CM_RL_NKDischargePort = "GBLHR";

				mawb.WeightCode = "KG";
				mawb.Weight = 3000m;
				mawb.DescriptionOfGoods = "COLUMBIAN FLOUR";
				mawb.CM_FlightNo = "BA112";
				mawb.AgentBadge = "LXA";

				if (createHawbToo)
				{
					var hawb = mawb.ChildBills.AddNew();
					hawb.CS_HAWB = "87654321";
					hawb.CS_RL_NKLoadPort = "USLAX";
					hawb.CS_WeightUQ = "KG";
					hawb.CS_Weight = 100;
					hawb.CS_PiecesLanded = 3;
					hawb.CS_PiecesManifested = 4;
					hawb.CS_GoodsDescription = "Stuff";
					hawb.CargoTerminalOperator = "BAC";
					hawb.CargoTerminalOperatorAirport = "LHR";
				}
			}
			return mawb;
		}

		NonPersistentSplitLineCollection GetSplitsForTest(BusinessObjectFactory factory)
		{
			var splits = new NonPersistentSplitLineCollection(factory);
			splits.Add(new NonPersistentSplitLine("01", "K", 2, 33m, "", "", factory));
			splits.Add(new NonPersistentSplitLine("02", "KGM", 2, 12345.67m, "Handle with care", "", factory));
			splits.Add(new NonPersistentSplitLine("03", "KG", 1, 10m, "Daniel", "", factory));
			return splits;
		}

		protected override void SetUp()
		{
			base.SetUp();
			AirCargoInventory.Testing.LicencingAndShedRestrictionsTests.EnsureAgentLxa();
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H", portName: "Heathrow");
			Factory.Save();
		}
	}

	class CcsukInventoryICusAwbMessageSenderForTest : CcsukInventoryICusAwbMessageSender
	{
		public void ValidateAndShowUserAnyWarningsOrErrorsExposed(IBusiness bizO, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction how)
		{
			base.ValidateAndShowUserAnyWarningsOrErrors(bizO, sendMessagesToCustoms, how);
		}
	}
}
