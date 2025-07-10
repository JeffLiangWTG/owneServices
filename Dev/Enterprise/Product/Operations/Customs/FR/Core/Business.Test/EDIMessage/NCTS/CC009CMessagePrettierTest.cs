using CargoWise.Customs.FR.MessageDefinitions.TP5.CC009C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC009CMessagePrettierTest : NCTSMessagePrettierTest<Cc009CType, CC009CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Invalidation Decision<br><strong>LRN: </strong>LRN1<br><strong>MRN: </strong>MRN1<br><strong>Decision Date Time: </strong>1/01/2024 10:01:01 AM<br><strong>Initiated By Customs: </strong>0</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC009CResponseMessage.xml");
	}
}
