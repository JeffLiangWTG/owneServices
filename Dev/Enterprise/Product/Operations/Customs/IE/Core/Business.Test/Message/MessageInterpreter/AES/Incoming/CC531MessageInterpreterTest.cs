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
	[TestedType(typeof(CC531MessageInterpreter))]
	class CC531MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC531MessageInterpreter, CC531Provider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE531;

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc531Text = AESInterchangeProcessorTestHelper.GetStandardAESCC531CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc531Text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message)
		{
			return $@"An Expiry Of Timer message has been received for supplementary declaration from Customs for Job B00000012 through the IE531 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Lodgement of Supplementary Declaration start date</td><td>18-Sep-71</td></tr><tr><td>Lodgement of Supplementary Declaration expiry date</td><td>19-Sep-71</td></tr><tr><td>Timer Expiry Information</td><td>Test Information 123</td></tr></table>";
		}

		protected override CC531Provider GetProvider(TextReader reader) => new CC531Provider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC531C.Cc531C>(reader).Message);
	}
}
