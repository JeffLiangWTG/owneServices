using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE410;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IE410MessagePrettierTest : DeltaIEMessagePrettierTest<CC410BType, IE410MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>LRN: </strong>WTLDFRFRM0000000001<br><strong>CRN: </strong>CRN099999999<br><strong>MRN: </strong>MRN099999999<br><strong>Operator request reference: </strong>987654321<br><strong>Customs request reference: </strong>123456789<br><strong>Invalidation decision date and time: </strong>2022-12-25T11:11:11<br><strong>Invalidation request date and time: </strong>2022-12-25T11:11:11<br><strong>Invalidation initiated by customs: </strong>0<br><strong>Invalidation motivation: </strong>invalidationMotivation<br><strong>Invalidation justification: </strong>invalidationJustification</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE410ResponseMessage.json");
	}
}
