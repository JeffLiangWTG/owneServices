using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC557C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC557CProcessor))]
	class CC557CProcessorTest : Business.Testing.EntryHeaderMessageProcessorTest<CC557CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC557CProvider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Not change Entry Status", string.Empty, entry.CH_EntryStatus);
			AssertEquals("Not change Logical Status", LogicalStatusList.Codes.Invalid, entry.CH_Status);
		}

		protected override ZString MessageFriendlyName => "CC557C: REJECTION FROM OFFICE OF EXIT";

		protected override CC557CProcessor Processor => new CC557CProcessor(logger, typeof(Cc557C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE557;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardAESCC557CText("LRN123456789", "21IEDUB11A782454R2");
	}
}
