using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE917MessageInterpreter))]
	abstract class IE917MessageInterpreterAbstractTest : InboundMessageInterpreterAbstractTest<EMCSInboundEDIMessage, IE917MessageInterpreter, IIE917>
	{
		protected override ZString MessageType => EMCSIncomingMessageTypeList.Codes.IE917;

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

		protected override ZString GetExpectedInterpretation(EMCSInboundEDIMessage message) => ExpectedInterpretation;

		internal static ZString ExpectedInterpretation => @"A Negative Acknowledgement of XML Receipt message has been received.<br />
			<br />Your submission has been rejected for the following reasons:<br />
				<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""></table><br />
				<br />Error 1:<br />
				<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>Error Line Number</td><td>1</td></tr>
					<tr><td>Error Column Number</td><td>368</td></tr>
					<tr><td>Error Reason</td><td>cvc-elt.1: Cannot find the declaration of element ie:IE815.</td></tr>
					<tr><td>Error Location</td><td>Location</td></tr>
					<tr><td>Original Attribute Value</td><td>Original Value</td></tr>
				</table><br />
				<br />Error 2:<br />
				<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>Error Line Number</td><td>2</td></tr>
					<tr><td>Error Column Number</td><td>12</td></tr>
					<tr><td>Error Reason</td><td>cvc-elt.2: Cannot find the declaration of element ie:IE815.</td></tr>
					<tr><td>Error Location</td><td>Location 2</td></tr>
					<tr><td>Original Attribute Value</td><td>Original Value 2</td></tr>
				</table>";
	}
}
