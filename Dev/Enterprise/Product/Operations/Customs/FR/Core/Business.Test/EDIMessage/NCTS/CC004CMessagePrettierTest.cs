using CargoWise.Customs.FR.MessageDefinitions.TP5.CC004C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC004CMessagePrettierTest : NCTSMessagePrettierTest<Cc004CType, CC004CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>LRN: </strong>LRN1<br><strong>MRN: </strong>MRN1<br><strong>Acceptance Date Time: </strong>1/05/2021 12:34:56 PM</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC004CResponseMessage.xml");
	}
}
