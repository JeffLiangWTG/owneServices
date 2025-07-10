using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS928;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class IETS928MessagePrettierTest : PNTSMessagePrettierTest<Iets928, IETS928MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Message was successfully received in customs<br><strong>Correlation ID: </strong>142857</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS928ResponseMessage.xml");
	}
}
