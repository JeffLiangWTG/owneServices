using CargoWise.Customs.FR.MessageDefinitions.TP5.CC140C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC140CMessagePrettierTest : NCTSMessagePrettierTest<Cc140CType, CC140CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Request on non-arrived movement<br><strong>MRN: </strong>MRN1<br><strong>Request Date: </strong>01/05/2021<br><strong>Limit Response Date: </strong>02/06/2021</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC140CResponseMessage.xml");
	}
}
