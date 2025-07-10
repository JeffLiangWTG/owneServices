using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class AesERRNCKMessageProcessorProviderTest : TestCaseWithFactory
	{
		public void TestGetProcessor_AesStatusRequest()
		{
			message.EM_MessageSubType = ExportMessageSubTypeList.Codes.EXQ;
			var processor = provider.GetProcessor(message);
			AssertType<AesStatusRequestERRNCKMessageProcessor>(processor);
		}

		public void TestGetProcessor_Export()
		{
			message.EM_MessageSubType = ExportMessageSubTypeList.Codes.EXP;
			var processor = provider.GetProcessor(message);
			AssertType<ExportERRNCKMessageProcessor>(processor);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var logger = new LoggingInformation();
			provider = new AesERRNCKMessageProcessorProvider(logger);
			message = Factory.New<AesInboundEDIMessage<IERRNCK>>();
		}
		AesERRNCKMessageProcessorProvider provider;
		AesInboundEDIMessage<IERRNCK> message;
	}
}
