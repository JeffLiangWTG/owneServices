using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS030;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class IETS030MessagePrettierTest : PNTSMessagePrettierTest<Iets030, IETS030MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Presentation Notification Linked<br><strong>MRN: </strong>21BEPT00000000QGU7<br><strong>CRN: </strong>CRN21BETS00000000QFU2<br><strong>FRN: </strong>FRN21BEPN000000C3FLU1<br><strong>Notification Date: </strong>1/05/2021 12:34:56 PM<br><strong>Remarks: </strong>The remark</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS030_PLNResponseMessage.xml");
	}
}
