using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF02C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CCF02CMessagePrettierTest : NCTSMessagePrettierTest<Ccf02CType, CCF02CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Object: </strong>Status Update Notification<br><strong>LRN: </strong>LRN1<br><strong>MRN: </strong>MRN1<br><strong>Date and Time: </strong>1/05/2024 12:34:56 PM<br><strong>Detailed status: </strong>Anticipée<br><strong>Comments: </strong>commentaire1<br><strong>Status: </strong>ANTICIPEE</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CCF02CResponseMessage_ANTICIPEE.xml");
	}
}
