using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS095;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class IETS095MessagePrettierTest : PNTSMessagePrettierTest<Iets095, IETS095MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Measures Required<br><strong>Status Reason: </strong>Irregularity at timer expiration<br><strong>MRN: </strong>21BEPT00000000QGU7<br><strong>CRN: </strong>CRN21BETS00000000QFU2<br><strong>Notification Date: </strong>1/05/2021 12:34:56 PM<br><strong>Timer For Temporary Storage: </strong>30/07/2021 11:59:59 PM<br><strong>Remarks: </strong>The remarks</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS095_TMRResponseMessage.xml");
	}
}
