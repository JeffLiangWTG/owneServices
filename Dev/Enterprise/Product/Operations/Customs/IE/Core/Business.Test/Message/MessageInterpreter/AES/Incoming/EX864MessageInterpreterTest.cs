using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX864;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(EX864MessageInterpreter))]
	class EX864MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, EX864MessageInterpreter, EX864Provider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX864;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"A Declaration Invalidation Request Cancellation message has been received from Customs for Job B00000012 through the EX864 message stating that Revenue have now decided to cancel the Invalidation request which was earlier sent through EX564 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Case Id</td><td>CASEID</td></tr><tr><td>Invalidation Request Cancellation Reason</td><td>REASON</td></tr></table>
";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var ex864Text = AESInterchangeProcessorTestHelper.GetStandardEX864Text("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", ex864Text, includeResponseWrap: false);

			return message;
		}

		protected override EX864Provider GetProvider(TextReader reader) => new EX864Provider(new MailBoxItemProvider<Ex864>(reader).Message);
	}
}
