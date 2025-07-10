using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX884;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(EX884MessageInterpreter))]
	class EX884MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, EX884MessageInterpreter, EX884Provider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX884;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"A Document Presentation Request Cancellation message has been received from Customs for Job B00000012 via the EX884 message canceling a previous request for presentation of documents.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Case Id</td><td>ID01</td></tr><tr><td>Document Presentation Request Cancellation Reason</td><td>Reason</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var ex884Text = AESInterchangeProcessorTestHelper.GetStandardEX884Text("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", ex884Text, includeResponseWrap: false);

			return message;
		}

		protected override EX884Provider GetProvider(TextReader reader) => new EX884Provider(new MailBoxItemProvider<Ex884>(reader).Message);
	}
}
