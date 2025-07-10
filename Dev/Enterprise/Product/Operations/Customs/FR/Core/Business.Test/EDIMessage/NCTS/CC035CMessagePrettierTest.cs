using CargoWise.Customs.FR.MessageDefinitions.TP5.CC035C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC035CMessagePrettierTest : NCTSMessagePrettierTest<Cc035CType, CC035CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Recovery Notification<br><strong>MRN: </strong>MRN1<br><strong>Recovery Notification Date: </strong>01/08/2024<br><strong>Recovery Notification Text: </strong>RecoveryNotification1<br><strong>Amount Claimed And Currency: </strong>100 AUD</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC035CResponseMessage.xml");
	}
}
