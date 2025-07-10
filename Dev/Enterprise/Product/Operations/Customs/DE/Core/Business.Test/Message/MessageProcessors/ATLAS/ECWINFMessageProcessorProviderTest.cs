using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Import;
using Enterprise.Customs.DE.Messaging;
using Moq;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ECWINFMessageProcessorProviderTest : TestCaseWithFactory
	{
		public void TestGetProcessor_Import()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATC996151771020016389";

			var dataProviderMock = new Mock<IECWINF>();
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATC996151771020016389");

			var messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IECWINF>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			var processor = provider.GetProcessor(messageMock.Object);
			AssertType<ImportECWINFMessageProcessor>(processor);
		}

		public void TestGetProcessor_Ncts()
		{
			var nctsHeader = Factory.New<Integration.Customs.DE.ICusInBondHeader>();

			var mrnEntryNumber = CusEntryNumber.LoadOrCreate((BusinessObject)nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATC996151771020016388";

			var dataProviderMock = new Mock<IECWINF>();
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATC996151771020016388");

			var messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IECWINF>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			var expectedReturnType = ObjectFactory.GetType("DENCTSECWINFMessageProcessor");

			var processor = provider.GetProcessor(messageMock.Object);

			AssertType(expectedReturnType, processor);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var logger = new LoggingInformation();
			provider = new ECWINFMessageProcessorProvider(logger);
		}

		ECWINFMessageProcessorProvider provider;
	}
}
