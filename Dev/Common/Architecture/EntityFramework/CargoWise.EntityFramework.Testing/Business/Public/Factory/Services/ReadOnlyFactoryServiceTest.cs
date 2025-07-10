using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ReadOnlyFactoryServiceTest : TestCase
	{
		public void TestConstructor()
		{
			ReadOnlyFactoryService service = new ReadOnlyFactoryService();
			Assert("Should implement IService", service is IService);
			AssertNotNull("ReadOnlyFactory", service.ReadOnlyFactory);
		}

		public void TestResetFactory()
		{
			ReadOnlyFactoryService service = new ReadOnlyFactoryService();
			BusinessObjectFactory saveFactory = service.ReadOnlyFactory;
			AssertEquals("Should be same Factory", saveFactory, service.ReadOnlyFactory);
			service.ResetReadOnlyFactory();
			AssertNotEquals("Should be different Factory", saveFactory, service.ReadOnlyFactory);
		}
	}
}
