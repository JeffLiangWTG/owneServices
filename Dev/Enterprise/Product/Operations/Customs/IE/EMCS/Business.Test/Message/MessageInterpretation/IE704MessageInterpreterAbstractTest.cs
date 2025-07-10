using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE704MessageInterpreter))]
	abstract class IE704MessageInterpreterAbstractTest : InboundMessageInterpreterAbstractTest<EMCSInboundEDIMessage, IE704MessageInterpreter, IIE704>
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

		protected override ZString GetExpectedInterpretation(EMCSInboundEDIMessage message) => @"A Generic Refusal (IE704) message has been received for Job E00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Administrative Reference Code</td><td>MRN98761234</td></tr><tr><td>LRN</td><td>B000222547896254786321</td></tr></table><br />
<br />Functional Error 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Type</td><td>12</td></tr><tr><td>Error Reason</td><td>reason1</td></tr><tr><td>Error Location</td><td>location1</td></tr><tr><td>Original Attribute Value</td><td>original value 1</td></tr></table><br />
<br />Functional Error 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Type</td><td>15</td></tr><tr><td>Error Reason</td><td>reason2</td></tr><tr><td>Error Location</td><td>location2</td></tr><tr><td>Original Attribute Value</td><td>original value 2</td></tr></table>";
	}
}
