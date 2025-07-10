using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA102;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class FRA102MessagePrettierTest : DeltaIEMessagePrettierTest<FRA102AType, FRA102MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>LRN: </strong>WTLDFRFRM0000000001<br><strong>CRN: </strong>01FRAB1234567890R1<br><strong>MRN: </strong>01FRAB1234567890A1<br><strong>Operator request reference: </strong>ABC123<br><strong>Customs request reference: </strong>FR456789<br><strong>Request Registration date and time: </strong>2023-06-21T12:15:30</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA102ResponseMessage.json");
	}
}
