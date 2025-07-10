using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX582;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(EX582MessageInterpreter))]
	class EX582MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, EX582MessageInterpreter, EX582Provider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX582;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"A Document Request message has been received from Customs for Job B00000012 through the EX582 message. <br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Request Date</td><td>22-Dec-20 00:00</td></tr><tr><td>Provide by Date</td><td>29-Jul-22 00:00</td></tr><tr><td>Document Type</td><td>Z740</td></tr><tr><td>Document Information</td><td>Info123</td></tr><tr><td>Document Type</td><td>Z750</td></tr><tr><td>Document Information</td><td>Info456</td></tr></table>
";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var ex582Text = AESInterchangeProcessorTestHelper.GetStandardEX582Text("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", ex582Text, includeResponseWrap: false);

			return message;
		}

		protected override EX582Provider GetProvider(TextReader reader) => new EX582Provider(new MailBoxItemProvider<Ex582>(reader).Message);
	}
}
