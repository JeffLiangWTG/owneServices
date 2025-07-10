using CargoWise.Customs.FR.MessageDefinitions.TP5.CC928C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC928CMessagePrettierTest : NCTSMessagePrettierTest<Cc928CType, CC928CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Positive Acknowledge<br><strong>LRN: </strong>LRN1</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC928CResponseMessage.xml");
	}
}
