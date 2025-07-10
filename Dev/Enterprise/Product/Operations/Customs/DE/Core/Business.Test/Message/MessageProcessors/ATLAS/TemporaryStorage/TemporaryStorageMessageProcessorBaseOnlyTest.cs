using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class TemporaryStorageMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetLinkedObjectFromReference()
		{
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "ATB150000620520195875";
			Factory.Save();

			AssertEquals(regHeader, processor.GetLinkedObjectFromReferenceExposed(Factory, "ATB150000620520195875", "24DE123050554788M5"));
		}

		public void GetLinkedObjectFromReference_MRNFallback()
		{
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "24DE123050554788M5";
			Factory.Save();

			AssertEquals(regHeader, processor.GetLinkedObjectFromReferenceExposed(Factory, "ATB150000620520195875", "24DE123050554788M5"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new TemporaryStorageMessageProcessorForTest(new LoggingInformation());
		}
		TemporaryStorageMessageProcessorForTest processor;
	}

	class TemporaryStorageMessageProcessorForTest : TemporaryStorageMessageProcessor<AtlasInboundEDIMessage<IDataProvider>, IDataProvider>
	{
		public TemporaryStorageMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => ZString.Empty;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IDataProvider> message)
		{
		}

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IDataProvider> message) => null;

		public BusinessObject GetLinkedObjectFromReferenceExposed(BusinessObjectFactory factory, string referenceNumber, string movementReferenceNumber = "") => base.GetLinkedObjectFromReference(factory, referenceNumber, movementReferenceNumber);
	}
}
