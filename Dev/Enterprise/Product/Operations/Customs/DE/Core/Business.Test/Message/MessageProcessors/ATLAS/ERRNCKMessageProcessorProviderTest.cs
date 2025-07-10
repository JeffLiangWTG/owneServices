using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ERRNCKMessageProcessorProviderTest : TestCaseWithFactory
	{
		public void TestGetProcessor_TemporaryStorage()
		{
			CombineAssertions(() =>
			{
				foreach (ICodeDescription pair in new TemporaryStorageMessageSubTypeList())
				{
					var messageSubType = pair.Code;
					message.EM_MessageSubType = messageSubType;
					var processor = provider.GetProcessor(message);
					AssertType<TemporaryStorageERRNCKMessageProcessor>(messageSubType, processor);
				}
			});
		}

		public void TestGetProcessor_Ncts()
		{
			CombineAssertions(() =>
			{
				foreach (var subType in new NctsMessageSubTypeList().GetAllCodes().Except(NctsMessageSubTypeList.Codes.StatusRequestMessage))
				{
					message.EM_MessageSubType = subType;
					var processor = provider.GetProcessor(message);
					AssertEquals(subType, ObjectFactory.GetType("DENCTSERRNCKMessageProcessor"), processor.GetType());
				}
			});
		}

		public void TestGetProcessor_NctsStatusRequest()
		{
			message.EM_MessageSubType = NctsMessageSubTypeList.Codes.StatusRequestMessage;
			var processor = provider.GetProcessor(message);
			AssertType<NctsStatusRequestERRNCKMessageProcessor>(processor);
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
					AssertType<MonthlyClosingERRNCKMessageProcessor>(messageSubType, processor);
				}
			});
		}

		public void TestGetProcessor_Import()
		{
			CombineAssertions(() =>
			{
				foreach (ICodeDescription pair in new ImportMessageSubTypeList())
				{
					var messageSubType = pair.Code;
					message.EM_MessageSubType = messageSubType;
					var processor = provider.GetProcessor(message);
					AssertType<ImportERRNCKMessageProcessor>(messageSubType, processor);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var logger = new LoggingInformation();
			provider = new ERRNCKMessageProcessorProvider(logger);
			message = Factory.New<AtlasInboundEDIMessage<IERRNCK>>();
		}
		ERRNCKMessageProcessorProvider provider;
		AtlasInboundEDIMessage<IERRNCK> message;
	}
}
