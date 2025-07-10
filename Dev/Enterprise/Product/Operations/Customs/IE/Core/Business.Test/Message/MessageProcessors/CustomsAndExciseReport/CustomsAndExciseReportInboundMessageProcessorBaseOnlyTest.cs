using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Integration;

namespace Enterprise.Customs.IE.Business.Testing
{
	class CustomsAndExciseReportInboundMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetEmailGroupRegistryItem()
		{
			var processor = new CustomsAndExciseReportInboundMessageProcessorForTest(new LoggingInformation());
			AssertSame(IECustomsDataRegistry.Instance.SendCustomsAndExciseReportAcknowledgements, processor.EmailGroupRegistryItem);
		}
	}

	class CustomsAndExciseReportInboundMessageProcessorForTest : CustomsAndExciseReportInboundMessageProcessor<PSRProvider>
	{
		public CustomsAndExciseReportInboundMessageProcessorForTest(LoggingInformation logger) : base(logger, typeof(PSRMessage)) { }

		public IRegistryItem EmailGroupRegistryItem => GetEmailGroupRegistryItem();

		protected override string MessageFriendlyNameCore => "HELLO WORLD";
	}
}
