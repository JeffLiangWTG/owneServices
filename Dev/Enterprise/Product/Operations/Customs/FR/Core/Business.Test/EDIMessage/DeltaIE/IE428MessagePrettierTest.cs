using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IE428MessagePrettierTest : DeltaIEMessagePrettierTest<CC428BType, IE428MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>ACCEPTE<br><strong>Status Date: </strong>2023-05-11T11:54:37<br><strong>Acceptance Date Time: </strong>2023-05-10T11:54:37<br><strong>LRN: </strong>WTLDFRFRM0000000001<br><strong>CRN: </strong>23FRD0000001228CR8<br><strong>MRN: </strong>23FRD2300001228MR1</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE428ResponseMessage.json");
	}
}
