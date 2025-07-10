using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsDepartureDeclarationResponseMessageProcessorTest : NctsGenericDepartureResponseMessageProcessorTest<NctsDepartureDeclarationResponseMessageProcessor>
	{
		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
		}

		protected override ZString GetExpectedProcessorFriendlyName() => "NCTS Departure Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.NctsDeparture };

		protected override NctsDepartureDeclarationResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new NctsDepartureDeclarationResponseMessageProcessor(logger);

		protected override ZString GetBM_CustomsStatus() => nctsHeader.MovementHeader.BM_CustomsStatus;

		protected override NctsDepartureDeclarationResponseMessageProcessor GetMockedProcessor(ICUSRESMessageProvider messageProvider)
		{
			var nctsArrivalDeclarationResponseMessageProcessorMock = new Mock<NctsDepartureDeclarationResponseMessageProcessorForMockTest>(logger) { CallBase = true };
			nctsArrivalDeclarationResponseMessageProcessorMock.Setup(c => c.GetMessageProviderForTest).Returns((INctsDepartureAndTIRResponseMessageProvider)messageProvider);
			return nctsArrivalDeclarationResponseMessageProcessorMock.Object;
		}

		public class NctsDepartureDeclarationResponseMessageProcessorForMockTest : NctsDepartureDeclarationResponseMessageProcessor
		{
			public NctsDepartureDeclarationResponseMessageProcessorForMockTest(LoggingInformation logger) : base(logger)
			{
			}

			public virtual INctsDepartureAndTIRResponseMessageProvider GetMessageProviderForTest
			{
				get;
			}
			protected override INctsDepartureAndTIRResponseMessageProvider GetMessageProviderCore(EDIMessage message) => GetMessageProviderForTest;
		}
	}
}
