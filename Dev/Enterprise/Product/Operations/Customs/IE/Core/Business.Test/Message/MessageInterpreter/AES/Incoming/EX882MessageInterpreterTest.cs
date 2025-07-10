using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX882;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(EX882MessageInterpreter))]
	class EX882MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, EX882MessageInterpreter, EX882Provider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX882;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"A Document Upload Request Cancellation message has been received from Customs for Job B00000012 through the EX882 message stating that Revenue have now decided to cancel the uploading document request which was sent before through EX582 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Case Id</td><td>ID01</td></tr><tr><td>Document Upload Request Cancellation Reason</td><td>Reason</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var ex882Text = AESInterchangeProcessorTestHelper.GetStandardEX882Text("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", ex882Text, includeResponseWrap: false);

			return message;
		}

		protected override EX882Provider GetProvider(TextReader reader) => new EX882Provider(new MailBoxItemProvider<Ex882>(reader).Message);
	}
}
