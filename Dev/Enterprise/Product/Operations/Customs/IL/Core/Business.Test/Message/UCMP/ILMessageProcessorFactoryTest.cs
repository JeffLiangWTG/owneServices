using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILMessageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestGetMessageProcessor()
		{
			AssertProcessor("DEC", string.Empty, "Enterprise.Customs.IL.Business.MessageProcessors.Message274Processor");
			AssertProcessor("DLO", string.Empty, "Enterprise.Customs.IL.Business.MessageProcessors.Message1220Processor");
			AssertProcessor("DOC", "276", "Enterprise.Customs.IL.Manifest.Business.Message2716Processor");
			AssertProcessor("DOC", "828", "Enterprise.Customs.IL.Manifest.Business.Message828Processor");
			AssertProcessor("GPM", string.Empty, "Enterprise.Customs.IL.Business.MessageProcessors.Message1035Processor");
			AssertProcessor("MAN", "171", "Enterprise.Customs.IL.Manifest.Business.Message1171Processor");
			AssertProcessor("MAN", "821", "Enterprise.Customs.IL.Manifest.Business.Message8241Processor");
			AssertProcessor("XER", string.Empty, "Enterprise.Customs.IL.Business.MessageProcessors.XTERRMessageProcessor");
			AssertProcessor("", string.Empty, "Enterprise.Customs.IL.Business.UnsupportedTypeMessageProcessor");
		}

		void AssertProcessor(string messageType, string messageSubType, string expectedType)
		{
			var processor = ILMessageProcessorFactory.GetMessageProcessor(messageType, messageSubType, new BatchProcessor.LoggingInformation());
			AssertNotNull("Processor", processor);
			AssertEquals("Processor Type", expectedType, processor.GetType().FullName);
		}
	}
}
