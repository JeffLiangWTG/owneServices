using CargoWise.Customs.FR.MessageDefinitions.TP5.CC029C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC029CMessagePrettierTest : NCTSMessagePrettierTest<Cc029CType, CC029CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Released for Transit<br><strong>LRN: </strong>LRN1<br><strong>MRN: </strong>MRN1<br><strong>Release Date: </strong>01/05/2021</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC029CResponseMessage.xml");
	}
}
