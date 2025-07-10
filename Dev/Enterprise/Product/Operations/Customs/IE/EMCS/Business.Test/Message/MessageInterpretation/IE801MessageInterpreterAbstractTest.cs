using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE801MessageInterpreter))]
	abstract class IE801MessageInterpreterAbstractTest : InboundMessageInterpreterAbstractTest<EMCSInboundEDIMessage, IE801MessageInterpreter, IIE801>
	{
		protected override ZString MessageType => EMCSIncomingMessageTypeList.Codes.IE704;

		protected abstract ZString MessageText { get; }

		protected override EMCSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_DeclarationReference = "E00000012";

			var message = Factory.New<EMCSInboundEDIMessage>();
			message.EM_LinkedObject = declaration;
			message.EM_MessageText = MessageText;

			return message;
		}

		protected override ZString GetExpectedInterpretation(EMCSInboundEDIMessage message) => "Electronic Administrative Document received.<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"></table><br />e-AD Number : MRN1234567 <br />";
	}
}
