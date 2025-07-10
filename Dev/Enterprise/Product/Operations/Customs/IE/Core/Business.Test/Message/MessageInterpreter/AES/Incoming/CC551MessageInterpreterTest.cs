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
	[TestedType(typeof(CC551MessageInterpreter))]
	class CC551MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC551MessageInterpreter, CC551CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE551;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Export No Release message has been received from Customs for Job B00000012 through the IE551 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Export Release Rejected (REJ)</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Issues Reported</td><td>Test Report</td></tr><tr><td>Result Date</td><td>18-Sep-71</td></tr><tr><td>Result Text</td><td>Text of a control result</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc551Text = AESInterchangeProcessorTestHelper.GetStandardCC551CText("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc551Text, includeResponseWrap: false);

			return message;
		}

		protected override CC551CProvider GetProvider(TextReader reader) => new CC551CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC551C.Cc551C>(reader).Message);
	}
}
