using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing;

[TestedType(typeof(PBNInboundEDIMessage))]
sealed class PBNInboundEDIMessageTest : EDIMessageTest
{
	public void TestSetDefaultValues()
	{
		var message = Factory.New<PBNInboundEDIMessage>();
		AssertEquals(EDIMessage.ApplicationCodes.IECustomsPBN, message.EM_ApplicationCode);
	}

	public void TestLookups()
	{
		var message = Factory.New<PBNInboundEDIMessage>();
		AssertType("Lookups should be of type PBNInboundEDIMessageLookups", typeof(PBNInboundEDIMessageLookups), message.Lookups);
	}

	public void TestGetDataProvider_EmptyMessageText()
	{
		var message = Factory.New<PBNInboundEDIMessage>();
		var provider = message.GetDataProvider<PSRProvider>(typeof(PSRMessage));
		AssertNull(provider);
		AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
	}

	public void TestGetDataProvider()
	{
		var message = Factory.New<PBNInboundEDIMessage>();
		message.EM_Status = EDIMessage.Status.Queued;
		var incomingJson = CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePSRText();
		message.EM_MessageText = incomingJson;
		var psrProvider = message.GetDataProvider<PSRProvider>(typeof(PSRMessage));
		AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
		AssertEquals("Timestamp", "1660211772103", psrProvider.Timestamp);
		AssertEquals("Eori", "IE1234567A", psrProvider.Eori);
		AssertEquals("Period", new ZDateTime(2022, 08, 01), psrProvider.Period);
		AssertEquals("TaxTotal", 400.0M, psrProvider.TaxTotal);
		AssertEquals("TaxBreakdowns", 2, psrProvider.TaxBreakdowns.Count);
		AssertEquals("DailyBreakdowns", 2, psrProvider.DailyBreakdowns.Count);
	}
}
