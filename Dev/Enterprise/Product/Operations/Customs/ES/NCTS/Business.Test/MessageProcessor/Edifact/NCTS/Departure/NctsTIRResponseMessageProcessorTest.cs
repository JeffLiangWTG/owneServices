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
	public class NctsTIRResponseMessageProcessorTest : NctsGenericDepartureResponseMessageProcessorTest<NctsTIRResponseMessageProcessor>
	{
		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
		}

		protected override ZString GetExpectedProcessorFriendlyName() => "NCTS TIR Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.NctsTir };

		protected override NctsTIRResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new NctsTIRResponseMessageProcessor(logger);

		protected override ZString GetBM_CustomsStatus() => nctsHeader.MovementHeader.BM_CustomsStatus;

		protected override NctsTIRResponseMessageProcessor GetMockedProcessor(ICUSRESMessageProvider messageProvider)
		{
			var nctsArrivalDeclarationResponseMessageProcessorMock = new Mock<NctsTIRResponseMessageProcessorForMockTest>(logger) { CallBase = true };
			nctsArrivalDeclarationResponseMessageProcessorMock.Setup(c => c.GetMessageProviderForTest).Returns((INctsDepartureAndTIRResponseMessageProvider)messageProvider);
			return nctsArrivalDeclarationResponseMessageProcessorMock.Object;
		}

		public class NctsTIRResponseMessageProcessorForMockTest : NctsTIRResponseMessageProcessor
		{
			public NctsTIRResponseMessageProcessorForMockTest(LoggingInformation logger) : base(logger)
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
