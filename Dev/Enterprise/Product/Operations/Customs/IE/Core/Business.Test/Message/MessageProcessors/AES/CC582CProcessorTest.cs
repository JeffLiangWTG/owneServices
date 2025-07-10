using System;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC582C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC582CProcessor))]
	class CC582CProcessorTest : EntryHeaderMessageProcessorTest<CC582CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC582CProvider>
	{
		public void TestMessageInterpreter()
		{
			AssertEquals("MessageInterpreter", typeof(CC582MessageInterpreter), new CC582CProcessorForTest(logger, typeof(Cc582C)).MessageInterpreterType);
		}

		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Entry Status set to Requested", AESEntryStatusList.Codes.Requested, entry.CH_EntryStatus);
			AssertEquals("Logical Status set to Accepted", LogicalStatusList.Codes.Accepted, entry.CH_Status);

			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Request on Non-exited Export message has been received from Customs for Job B00001000 through the IE582 message." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC582C: REQUEST ON NON-EXITED EXPORT";

		protected override CC582CProcessor Processor => new CC582CProcessor(logger, typeof(Cc582C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE582;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardAESCC582CText("LRN123456789", "21IEDUB11A782454R2");

		class CC582CProcessorForTest : CC582CProcessor
		{
			public CC582CProcessorForTest(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
			{
			}

			public new Type MessageInterpreterType => base.MessageInterpreterType;
		}
	}
}
