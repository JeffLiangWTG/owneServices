using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC528C;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business.AES;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AESInboundEDIMessage))]
	class AESInboundEDIMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<AESInboundEDIMessage>();
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.IECustomsExport, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", Enterprise.Messaging.Business.EDIInterchange.Direction.Receive, message.EM_ReceiveTransmit);
			});
		}

		public void TestEM_MessageInterpretation()
		{
			var message = Factory.New<AESInboundEDIMessage>();
			AssertNoExceptionThrown("AESInboundEDIMessage EM_MessageInterpretation should not throw error when empty.", () => _ = message.EM_MessageInterpretation);

			message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE528;
			AssertNoExceptionThrown("AESInboundEDIMessage EM_MessageInterpretation should not throw error with empty EM_MessageText.", () => _ = message.EM_MessageInterpretation);

			AssertNullOrEmpty("To make sure EM_MessageInterpretation is empty before EM_MessageText is set.", message.EM_MessageInterpretation);
			message.EM_MessageText = "XXXXXX";
			AssertNoExceptionThrown("AESInboundEDIMessage EM_MessageInterpretation should not throw error even with invalid EM_MessageText.", () => _ = message.EM_MessageInterpretation);

			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cc528Text = AESInterchangeProcessorTestHelper.GetStandardCC528CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc528Text, includeResponseWrap: false);
			message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
			message.EM_LinkedObject = cusEntryHeader;
			var processor = new CC528CProcessor(new LoggingInformation(), typeof(Cc528C));
			processor.ProcessMessage(message);
			AssertNotNullOrEmpty("EM_MessageInterpretation should have a value after a valid EM_MessageText is set.", message.EM_MessageInterpretation);
		}
	}
}
