using CargoWise.Customs.FR.MessageDefinitions.TP5.CC019C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC019CMessagePrettierTest : NCTSMessagePrettierTest<Cc019CType, CC019CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Discrepancies at Destination<br><strong>MRN: </strong>MRN1<br><strong>Discrepancies Notification Date: </strong>1/09/2024 12:00:00 AM<br><strong>Discrepancies Notification Text: </strong>discrepanciesNotificationText1</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC019CResponseMessage.xml");
	}
}
