using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE802MessageInterpreter))]
	abstract class IE802MessageInterpreterAbstractTest : InboundMessageInterpreterAbstractTest<EMCSInboundEDIMessage, IE802MessageInterpreter, IIE802>
	{
		protected override ZString MessageType => EMCSIncomingMessageTypeList.Codes.IE802;

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

		protected override ZString GetExpectedInterpretation(EMCSInboundEDIMessage message) => @"A reminder message IE802 has been received for the job E00000012.<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Message Type</td><td>1</td></tr><tr><td>Date and Time of Issuance of Reminder</td><td>04-Oct-21 14:15</td></tr><tr><td>Limit Date and Time</td><td>03-Sep-22 13:16</td></tr><tr><td>Reminder Information</td><td>You received this reminder information.</td></tr></table>";
	}
}
