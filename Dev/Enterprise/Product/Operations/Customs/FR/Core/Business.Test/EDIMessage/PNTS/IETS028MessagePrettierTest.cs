using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS028;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public sealed class IETS028MessagePrettierTest : PNTSMessagePrettierTest<Iets028, IETS028MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Proof of Union Status Presented<br><strong>FRN: </strong>FRN21BEPN000000C3FMU4<br><strong>Notification Date: </strong>1/05/2021 12:34:56 PM</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS028ResponseMessageForIETS007.xml");
	}
}
