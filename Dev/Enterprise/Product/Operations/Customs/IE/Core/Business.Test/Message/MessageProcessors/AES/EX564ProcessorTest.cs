using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX564;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(EX564Processor))]
	class EX564ProcessorTest : Business.Testing.EntryHeaderMessageProcessorTest<EX564Processor, AESInboundEDIMessage, AESOutboundEDIMessage, EX564Provider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Entry Status", AESEntryStatusList.Codes.CancellationRequestedByCustoms, entry.CH_EntryStatus);
			AssertEquals("Logical Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
		}

		protected override ZString MessageFriendlyName => "EX564: REQUEST DECLARATION INVALIDATION";

		protected override EX564Processor Processor => new EX564Processor(logger, typeof(Ex564));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX564;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardEX564Text("21IEDUB11A782454R2");
	}
}
