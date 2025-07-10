using CargoWise.Common;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.JP.Common.Testing
{
	sealed class NACCSFactoryServiceTest : TestCaseWithFactory
	{
		public void TestGetInboundMessageParser()
		{
			CombineAssertions(() =>
			{
				var oldParser = NACCSFactoryService.GetInboundMessageParser(Factory);
				AssertType<JPInboundMessageParser>("Default service", oldParser);
				var newParser = new JPInboundMessageParser();
				using (NACCSFactoryServiceTestHelper.SetInboundMessageParser(Factory, newParser))
				{
					AssertEquals("New service can be set", newParser, NACCSFactoryService.GetInboundMessageParser(Factory));
				}
				AssertEquals("Old service can be recovered", oldParser, NACCSFactoryService.GetInboundMessageParser(Factory));
			});
		}

		public void TestGetMessageFlatParser()
		{
			CombineAssertions(() =>
			{
				var oldParser = NACCSFactoryService.GetMessageFlatParser(Factory);
				AssertType<JPInboundMessageParser>("Default service", oldParser);
				var newParser = new JPInboundMessageParser();
				using (NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, newParser))
				{
					AssertEquals("New service can be set", newParser, NACCSFactoryService.GetMessageFlatParser(Factory));
				}
				AssertEquals("Old service can be recovered", oldParser, NACCSFactoryService.GetMessageFlatParser(Factory));
			});
		}

		public void TestGetOutboundMessageWriter()
		{
			CombineAssertions(() =>
			{
				var oldWriter = NACCSFactoryService.GetOutboundMessageWriter(Factory);
				AssertType<JPOutboundMessageWriter>("Default service", NACCSFactoryService.GetOutboundMessageWriter(Factory));
				var newWriter = new JPOutboundMessageWriter();
				using (NACCSFactoryServiceTestHelper.SetOutboundMessageWriter(Factory, newWriter))
				{
					AssertEquals("New service can be set", newWriter, NACCSFactoryService.GetOutboundMessageWriter(Factory));
				}
				AssertEquals("Old service can be recovered", oldWriter, NACCSFactoryService.GetOutboundMessageWriter(Factory));
			});
		}
	}

	public static class NACCSFactoryServiceTestHelper
	{
		public static DisposableAction SetInboundMessageParser(BusinessObjectFactory factory, IJPInboundMessageParser newParser)
		{
			return SetService(factory, newParser, NACCSFactoryService.InboundMessageParserKey);
		}

		public static DisposableAction SetMessageFlatParser(BusinessObjectFactory factory, IJPMessageFlatParser newParser)
		{
			return SetService(factory, newParser, NACCSFactoryService.MessageFlatParserKey);
		}

		public static DisposableAction SetOutboundMessageWriter(BusinessObjectFactory factory, IJPOutboundMessageWriter newWriter)
		{
			return SetService(factory, newWriter, NACCSFactoryService.OutboundMessageWriterKey);
		}

		static DisposableAction SetService<T>(BusinessObjectFactory factory, T newService, string key) where T : class
		{
			var originalService = factory.GetCachedValue<T>(key, () => null);
			factory.ClearCachedValue<T>(key);
			factory.GetCachedValue(key, () => newService);
			return new DisposableAction(() =>
			{
				factory.ClearCachedValue<T>(key);
				if (originalService != null)
				{
					factory.GetCachedValue(key, () => originalService);
				}
			});
		}
	}
}
