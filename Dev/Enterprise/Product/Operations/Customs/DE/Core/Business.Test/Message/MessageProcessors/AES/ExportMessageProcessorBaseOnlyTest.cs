using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ExportMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			new ExportMessageProcessorForTest(new LoggingInformation());
		}
	}

	class ExportMessageProcessorForTest : ExportMessageProcessor<AesInboundEDIMessage<IAesDataProvider>, IAesDataProvider>
	{
		internal ExportMessageProcessorForTest(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => ZString.Empty;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IAesDataProvider> message)
		{
		}

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IAesDataProvider> message) => null;
	}
}
