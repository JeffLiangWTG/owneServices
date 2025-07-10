using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS410;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class IETS410MessagePrettierTest : PNTSMessagePrettierTest<Iets410, IETS410MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>TSD Invalidated<br><strong>CRN: </strong>CRN21BETS00000000QFU2<br><strong>Notification Date: </strong>1/05/2021 12:34:56 PM<br><strong>Invalidation Initiated By Customs: </strong>False</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS410ResponseMessage.xml");
	}
}
