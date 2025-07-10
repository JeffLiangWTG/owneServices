using System.IO;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC917MessageInterpreter))]
	class CC917MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC917MessageInterpreter, CC917CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE917;

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000917";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			message.EM_MessageText = AESInterchangeProcessorTestHelper.GetStandardAESCC917CMailboxItemText("TransactionID", includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => $@"A Syntax Error Response message has been received from Customs for Job B00000917 through the IE917 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Error Line Number</td><td>1</td></tr><tr><td>Error Column Number</td><td>1</td></tr><tr><td>Error Pointer</td><td>p1</td></tr><tr><td>Error Code</td><td>12 - Incorrect enumeration</td></tr><tr><td>Error Text</td><td>Error Text 1</td></tr><tr><td>Error Line Number</td><td>2</td></tr><tr><td>Error Column Number</td><td>2</td></tr><tr><td>Error Pointer</td><td>p2</td></tr><tr><td>Error Code</td><td>50 - Invalid Value for the specific type</td></tr><tr><td>Error Text</td><td>Error Text 2</td></tr><tr><td>Error Line Number</td><td>3</td></tr><tr><td>Error Column Number</td><td>3</td></tr><tr><td>Error Text</td><td>Error Text 3</td></tr></table>";

		protected override CC917CProvider GetProvider(TextReader reader) => new CC917CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC917C.Cc917C>(reader).Message);
	}
}
