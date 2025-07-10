using CargoWise.Customs.FR.MessageDefinitions.TP5.CC045C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC045CMessagePrettierTest : NCTSMessagePrettierTest<Cc045CType, CC045CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Goods Written Off/Closed<br><strong>MRN: </strong>MRN1<br><strong>Write-off Date: </strong>01/05/2021</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC045CResponseMessage.xml");
	}
}

