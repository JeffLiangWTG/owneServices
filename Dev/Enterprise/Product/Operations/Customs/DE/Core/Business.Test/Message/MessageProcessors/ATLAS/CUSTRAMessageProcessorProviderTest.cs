using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CUSTRAMessageProcessorProviderTest : TestCaseWithFactory
	{
		public void TestGetProcessor_Import()
		{
			CombineAssertions(() =>
			{
				foreach (ICodeDescription pair in new ImportMessageSubTypeList())
				{
					var messageSubType = pair.Code;
					message.EM_MessageSubType = messageSubType;
					var processor = provider.GetProcessor(message);
					AssertType<ImportCUSTRAMessageProcessor>(messageSubType, processor);
				}
			});
		}

		public void TestGetProcessor_MonthlyClosing()
		{
			CombineAssertions(() =>
			{
				foreach (ICodeDescription pair in new MonthlyClosingMessageSubTypeList())
				{
					var messageSubType = pair.Code;
					message.EM_MessageSubType = messageSubType;
					var processor = provider.GetProcessor(message);
					AssertType<MonthlyClosingCUSTRAMessageProcessor>(messageSubType, processor);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var logger = new LoggingInformation();
			provider = new CUSTRAMessageProcessorProvider(logger);
			message = Factory.New<AtlasInboundEDIMessage<ICUSTRA>>();
		}
		CUSTRAMessageProcessorProvider provider;
		AtlasInboundEDIMessage<ICUSTRA> message;
	}
}
