using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSMessageProcessorTests : TestCaseWithFactory
	{
		public void TestApplicationCode()
		{
			var processor = new CDSMessageProcessorForTest(new LoggingInformation());
			AssertEquals(EDIMessage.ApplicationCodes.GbCustomsDeclarationServices, processor.ApplicationCode);
		}
	}

	class CDSMessageProcessorForTest : CDSMessageProcessor<MessageDataObj>
	{
		public CDSMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString ProcessMessageCore(MessageDataObj cdsEDIMessage, BusinessObjectFactory factory)
		{
			return EDIMessageStatusList.Codes.Discarded;
		}

		protected override ZString MessageType => "XX";

		protected override string MessageFriendlyNameCore => "Hello";
	}

	class MessageDataObj : CDSEDIMessage
	{
		public MessageDataObj(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public string MessageCode { get; set; }
		public string ActionCode { get; set; }
	}
}
