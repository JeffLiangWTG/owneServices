using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC917C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC917CMessageInterpreter))]
	sealed class CC917CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC917CMessageInterpreter, CC917CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE917;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var cc182Text = InterchangeProcessorTestHelper.GetStandardCC917CText("LRNCC060C0123456789012", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc182Text, includeResponseWrap: false);
			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"An Syntax Error Notification (IE917) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRNCC060C0123456789012</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Error Line Number</td><td>1</td></tr><tr><td>Error Column Number</td><td>1</td></tr><tr><td>Error Pointer</td><td>Pointer1</td></tr><tr><td>Error Code</td><td>12</td></tr><tr><td>Error Text</td><td>ErrorText1</td></tr><tr><td>Error Line Number</td><td>2</td></tr><tr><td>Error Column Number</td><td>1</td></tr><tr><td>Error Pointer</td><td>Pointer2</td></tr><tr><td>Error Code</td><td>13</td></tr><tr><td>Error Text</td><td>ErrorText2</td></tr></table>";

		protected override CC917CProvider GetProvider(TextReader reader) => new CC917CProvider(new MailBoxItemProvider<Cc917CType>(reader).Message);
	}
}
