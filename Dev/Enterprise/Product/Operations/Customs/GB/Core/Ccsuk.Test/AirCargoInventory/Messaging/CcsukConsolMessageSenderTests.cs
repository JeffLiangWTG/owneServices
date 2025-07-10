using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	class CcsukConsolMessageSenderTests : TestCaseWithFactory
	{
		public void TestSendFsrFromExportConsol()
		{
			CreateBadgeForTest();
			var consolSender = new CcsukConsolMessageSender();
			wrapper.MawbExportHelper.ME_Profile = "BAC";
			wrapper.MawbExportHelper.CalculateMUCR();
			consolSender.SendToCCSUK(wrapper, shutup, new CcsukTransmissionMessageFunction.CUKFSR.FsaForExport());
			var message = consol.Messages[0];
			AssertEquals("FSR", message.EM_MessageType);
			AssertEquals("EXP", message.EM_MessageSubType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("CUKSYS98COMMDB", message.EM_ApplicationReference);
			AssertEquals("QUE", message.EM_Status);
			AssertContains("A:11122222222 at LHRBAC", message.EM_MessageInterpretation);
			AssertContains(@"CUKFSR:1:912:BT+", message.EM_MessageText);
			AssertContains(@"BGM++11122222222+++++EXP'LOC+11:LHR:145:3::BAC:129:ZZZ'UNT", message.EM_MessageText);
		}

		public void TestSendFsrNoShedFromExportConsol()
		{
			CreateBadgeForTest();
			var consolSender = new CcsukConsolMessageSender();
			wrapper.MawbExportHelper.ME_Profile = "BAC";
			wrapper.MawbExportHelper.CalculateMUCR();
			consolSender.SendToCCSUK(wrapper, shutup, new CcsukTransmissionMessageFunction.CUKFSR.FsaForExportWithoutShed());
			var message = consol.Messages[0];
			AssertEquals("FSR", message.EM_MessageType);
			AssertEquals("EXP", message.EM_MessageSubType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("QUE", message.EM_Status);
			AssertContains(">A:11122222222<", message.EM_MessageInterpretation);
			AssertContains(@"CUKFSR:1:912:BT+", message.EM_MessageText);
			AssertContains(@"BGM++11122222222+++++EXP'UNT", message.EM_MessageText);
		}

		public void TestSendG2gFromExportConsolType2()
		{
			CreateBadgesForLicencingAndShedRestrictions(AgentTypeForExportFallbackList.Codes.Type2);
			wrapper.MawbExportHelper.ME_Profile = "LXA"; // type 2 
			wrapper.MawbExportHelper.CalculateMUCR();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "GBLHR";
			var consolSender = new CcsukConsolMessageSender();
			consolSender.SendToCCSUK(wrapper, shutup, new CcsukTransmissionMessageFunction.CUKG2G());
			var message = consol.Messages[0];
			AssertEquals("G2G", message.EM_MessageType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("QUE", message.EM_Status);
			AssertEquals("CUKSYS98COMMDB", message.EM_ApplicationReference);
			AssertContains("A:11122222222<", message.EM_MessageInterpretation);
			AssertContains(@"CUKG2G:A:04A:BT", message.EM_MessageText);
		}

		public void TestSendG2gFromExportConsolType1()
		{
			CreateBadgesForLicencingAndShedRestrictions(AgentTypeForExportFallbackList.Codes.Type1);
			wrapper.MawbExportHelper.ME_Profile = "LXA"; // type 1
			wrapper.MawbExportHelper.CalculateMUCR();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "GBLHR";
			var consolSender = new CcsukConsolMessageSender();
			consolSender.SendToCCSUK(wrapper, shutup, new CcsukTransmissionMessageFunction.CUKG2G());
			AssertContains("Type 1 agents cannot send G2G without a consol- or shipment-level customs authorisaton reference", shutup.LastErrorsAsString);
			var car = consol.Numbers.AddNew();
			car.CE_EntryType = "CAR";
			car.CE_EntryNum = "Anything";
			consolSender = new CcsukConsolMessageSender();
			shutup = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			consolSender.SendToCCSUK(wrapper, shutup, new CcsukTransmissionMessageFunction.CUKG2G());
			AssertEquals("", shutup.LastErrorsAsString);
		}

		public void TestSendG2gFromExportConsolTypeNothing()
		{
			CreateBadgeForTest();
			wrapper.MawbExportHelper.ME_Profile = "LXA"; // type nothing
			wrapper.MawbExportHelper.CalculateMUCR();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "GBLHR";
			var consolSender = new CcsukConsolMessageSender();
			consolSender.SendToCCSUK(wrapper, shutup, new CcsukTransmissionMessageFunction.CUKG2G());
			AssertContains("Only Type 1 or Type 2 agents may send G2G messages", shutup.LastErrorsAsString);
		}

		public void TestProfileValidation()
		{
			CreateBadgeForTest();
			wrapper.MawbExportHelper.ME_Profile = "";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "GBLHR";
			var consolSender = new CcsukConsolMessageSender();
			consolSender.SendToCCSUK(wrapper, shutup, new CcsukTransmissionMessageFunction.CUKG2G());
			AssertContains("Profile", shutup.LastErrorsAsString);
		}

		void CreateBadgesForLicencingAndShedRestrictions(ZString fallBackAgentType)
		{
			base.SetUp();
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false, false, fallBackAgentType, direction: "EXP", mucrGenerationStyle: Registry.MucrGenerationStyles.Codes.Air);
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "11122222222";
			consol.JK_RL_NKLoadPort = "GBLHR";
			shutup = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			wrapper = new CustomsExportConsolIntegrationWrapper(consol, shutup);
			var helper = wrapper.MawbExportHelper;
			helper.ME_ExportShed = "BAC";
			helper.ME_ExportLocation = "LHR";
		}

		void CreateBadgeForTest()
		{
			base.SetUp();
			MawbTestHelper.MakeBadge("BAC", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", true, false, BadgeDirectionList.Codes.EXP, "GBLHR", Registry.MucrGenerationStyles.Codes.Air);
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "11122222222";
			consol.JK_RL_NKLoadPort = "GBLHR";
			shutup = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			wrapper = new CustomsExportConsolIntegrationWrapper(consol, shutup);
			var helper = wrapper.MawbExportHelper;
			helper.ME_ExportShed = "BAC";
			helper.ME_ExportLocation = "LHR";
		}

		ForwardingConsol consol;
		CustomsExportConsolIntegrationWrapper wrapper;
		Customs.Business.SendsMessagesToCustomsShutterUpperer shutup;
	}
}
