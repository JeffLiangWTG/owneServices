using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF03C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing;

sealed class CCF03CMessagePrettierTest : NCTSMessagePrettierTest<Ccf03CType, CCF03CMessageDataObject>
{
	protected override ZString GetExpectedMessageInterpretation() => @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Positive Acknowledge<br><strong>MRN: </strong>MRN1<br><strong>Acceptance Date Time : </strong>18/09/2024 12:00:00 AM</p>";

	protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CCF03CResponseMessage.xml");
}
