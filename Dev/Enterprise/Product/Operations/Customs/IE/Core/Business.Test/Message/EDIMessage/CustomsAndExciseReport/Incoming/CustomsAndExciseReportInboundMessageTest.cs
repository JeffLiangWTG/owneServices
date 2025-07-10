using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CustomsAndExciseReportInboundMessage))]
	public class CustomsAndExciseReportInboundMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		public void TestGetDataProvider() => CombineAssertions(() =>
		{
			var message = Factory.New<CustomsAndExciseReportInboundMessage>();
			message.EM_MessageType = CustomsAndExciseReportTypeList.Codes.PSR;
			var incomingJson = CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePSRText();
			message.EM_MessageText = incomingJson;
			var psrProvider = message.GetDataProvider<PSRProvider>(typeof(PSRMessage));
			AssertEquals("Timestamp", "1660211772103", psrProvider.Timestamp);
			AssertEquals("Eori", "IE1234567A", psrProvider.Eori);
			AssertEquals("Period", new ZDateTime(2022, 08, 01), psrProvider.Period);
			AssertEquals("TaxTotal", 400.0M, psrProvider.TaxTotal);
			AssertEquals("TaxBreakdowns", 2, psrProvider.TaxBreakdowns.Count);
			AssertEquals("DailyBreakdowns", 2, psrProvider.DailyBreakdowns.Count);
		});

		public void TestSetDefaultValues()
		{
			var message = Factory.New<CustomsAndExciseReportInboundMessage>();
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.IECustomsAndExcise, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", Enterprise.Messaging.Business.EDIInterchange.Direction.Receive, message.EM_ReceiveTransmit);
			});
		}
	}
}
