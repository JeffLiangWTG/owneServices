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
	[TestedType(typeof(CC504MessageInterpreter))]
	class CC504MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC504MessageInterpreter, CC504CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE504;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Amendment Acceptance message has been received from Customs for Job B00000012 through the IE504 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Amendment Submission Date and Time</td><td>18-Sep-71 00:00</td></tr><tr><td>Amendment Acceptance Date and Time</td><td>01-Jul-22 00:00</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc556Text = AESInterchangeProcessorTestHelper.GetStandardCC504CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc556Text, includeResponseWrap: false);

			return message;
		}

		protected override CC504CProvider GetProvider(TextReader reader) => new CC504CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC504C.Cc504C>(reader).Message);
	}
}
