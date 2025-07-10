using CargoWise.Customs.FR.MessageDefinitions.TP5.CC028C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC028CMessagePrettierTest : NCTSMessagePrettierTest<Cc028CType, CC028CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>MRN Allocated<br><strong>LRN: </strong>LRN28<br><strong>MRN: </strong>MRN28<br><strong>Acceptance Date Time: </strong>1/05/2024 12:00:00 AM</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC028CResponseMessage.xml");
	}
}
