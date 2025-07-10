using CargoWise.EntityFramework.Testing;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class UniversalEventMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestGetFormattedMessageText()
		{
			var universalEvent = new UniversalEventWrapper(UniversalEventTestDataHelper.CreateUniversalEventXml(Events.InterchangeRejectedCode, MessageTypeList.Codes.XER, "Unauthorized", "ErrorMessage", "...."));
			var messagePrettyFormatter = new UniversalEventMessagePrettyFormatter(universalEvent);

			AssertMultilineASCIIEquals(@"<H3>Service Error</H3>" +
						"<table border=\"1\" cellpadding=\"2\" cellspacing=\"0\" class=\"table\" style=\"white-space:pre\" width=\"100%\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						@"<tr><td>Unauthorized</td><td>ErrorMessage</td></tr>" +
						"</table>", messagePrettyFormatter.GetFormattedMessageText());
		}
	}
}
