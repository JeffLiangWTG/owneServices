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
	[TestedType(typeof(CC522MessageInterpreter))]
	class CC522MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC522MessageInterpreter, CC522CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE522;

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000522";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc522Text = AESInterchangeProcessorTestHelper.GetStandardAESCC522CText("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc522Text, includeResponseWrap: false);
			return message;
		}

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Exit Release Rejection message has been received from Customs for Job B00000522 through the IE522 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Exit rejection motivation code</td><td>B</td></tr><tr><td>Exit rejection motivation</td><td>Exit Rejection Motivation B</td></tr></table>";

		protected override CC522CProvider GetProvider(TextReader reader) => new CC522CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC522C.Cc522C>(reader).Message);
	}
}
