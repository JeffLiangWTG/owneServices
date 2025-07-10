using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE404;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IE404MessagePrettierTest : DeltaIEMessagePrettierTest<CC404BType, IE404MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=\"font-size: 120%\"><strong>Customs request reference: </strong>REF123456789<br><strong>Operator request reference: </strong>OPREF123456789<br><strong>MRN: </strong>22FR1234567890123R1<br><strong>Amendment date and time: </strong>2023-10-01T12:34:56<br><strong>Amendment acceptance date and time: </strong>2023-10-01T12:34:56<br><strong>Amendment justification: </strong>This is a justification for the amendment.</p><p style=\"font-size: 120%\"><strong>Remarks</strong><br><strong>Remarks code and Reason: </strong>REMARK001This is remark 1.<br><strong>Remarks code and Reason: </strong>REMARK002This is remark 2.</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE404ResponseMessage.json");
	}
}
