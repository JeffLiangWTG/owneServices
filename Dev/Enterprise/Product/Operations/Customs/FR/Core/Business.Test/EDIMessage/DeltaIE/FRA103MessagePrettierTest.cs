using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA103;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class FRA103MessagePrettierTest : DeltaIEMessagePrettierTest<FRA103AType, FRA103MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=\"font-size: 120%\"><strong>LRN: </strong>WTLDFRFRM0000000001<br><strong>CRN: </strong>01FRAB1234567890R1<br><strong>MRN: </strong>01FRAB1234567890A1<br><strong>Customs request reference: </strong>FR456789<br><strong>Timer request instruction start date: </strong>2023-08-21T17:15:13<br><strong>Initial timer request instruction expiry date: </strong>2023-08-21T19:15:13<br><strong>New timer request instruction expiry date: </strong>2023-08-22T20:15:13<br><strong>Extension Information: </strong>extension info</p>";

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA103ResponseMessage.json");
	}
}
