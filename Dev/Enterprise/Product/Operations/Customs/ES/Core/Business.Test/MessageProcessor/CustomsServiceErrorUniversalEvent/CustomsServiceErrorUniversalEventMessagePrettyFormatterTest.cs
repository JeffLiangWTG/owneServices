using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class CustomsServiceErrorUniversalEventMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new CustomsServiceErrorUniversalEventMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted()
		{
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			AssertEquals(ZString.Empty, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertEquals("<H3>Universal Event Service Error</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Event Type</strong></td><td><strong>Message Type</strong></td><td><strong>Reason</strong></td></tr>" +
						"<tr><td>IRJ</td><td>NPI</td><td>The MRN 21ES00999930MXRZN5 exceeded the retry count of 5 and still receives no update</td></tr>" +
						"</table>", messageInterpretationText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			universalEventWrapper = new UniversalEventWrapper($@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Event>
	<EventTime>2022-11-08T07:08:36</EventTime>
	<EventType>{Events.InterchangeRejectedCode}</EventType>
	<EventParameters>
	  <MessageType>NPI</MessageType>
	  <Type>Test</Type>
	  <Reason>The MRN 21ES00999930MXRZN5 exceeded the retry count of 5 and still receives no update</Reason>
	</EventParameters>
  </Event>
</UniversalEvent>");
			messagePrettyFormatter = new CustomsServiceErrorUniversalEventMessagePrettyFormatter(universalEventWrapper);
		}

		CustomsServiceErrorUniversalEventMessagePrettyFormatter messagePrettyFormatter;
		UniversalEventWrapper universalEventWrapper;
	}
}
