using CargoWise.Customs.FR.MessageDefinitions.TP5.CC043C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC043CMessagePrettierTest : NCTSMessagePrettierTest<Cc043CType, CC043CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Unloading Permission<br><strong>MRN: </strong>MRN1<br><strong>Continue unloading: </strong>continueUnloading1</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC043CResponseMessage.xml");
	}
}
