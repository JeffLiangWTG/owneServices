using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business.Test
{
	class ServiceTaskFactoryProviderWrapperTest : BMSTestCaseWithFactory
	{
		public void TestFactoryProvider_ShouldRetainConnectionAndFactoryTypeFromInitialFactory_AndServices()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var connection = Db.NewExtraConnectionToMainDb();

			AssertNotEquals("Precondition: extra connection created is not the same as the default connection", Db.Connection, connection);

			var wrapper = new ServiceTaskFactoryProviderWrapperForTest(connection);

			var startingFactory = wrapper.Current;
			startingFactory.ServiceContainer.AddService(new ServiceTaskCodeService("ZZZ"));

			AssertType<SubclassedBusinessObjectFactory>(startingFactory);
			AssertEquals(connection, ((IDbConnected)startingFactory).Connection);
			AssertNotNull(startingFactory.ServiceContainer.GetService<ServiceTaskCodeService>());

			wrapper.CreateNewWithoutSave();

			var newFactory = wrapper.Current;

			AssertType<SubclassedBusinessObjectFactory>("Factory created through the provider should use the same type specified in CreateStartingFactory", newFactory);
			AssertEquals("Factory created through the provider should share the db connection provided through the factory created in CreateStartingFactory", connection, ((IDbConnected)newFactory).Connection);
			AssertNotNull(newFactory.ServiceContainer.GetService<ServiceTaskCodeService>());
		}
	}

	#region Test Classes

	class ServiceTaskFactoryProviderWrapperForTest : ServiceTaskFactoryProviderWrapper
	{
		public ServiceTaskFactoryProviderWrapperForTest(DbConnection connection)
			: base(new DummyLogger())
		{
			this.connection = connection;
		}

		readonly DbConnection connection;

		protected override BusinessObjectFactory CreateStartingFactory() => new SubclassedBusinessObjectFactory(connection);
	}

	class SubclassedBusinessObjectFactory : BusinessObjectFactory
	{
		public SubclassedBusinessObjectFactory(DbConnection connection)
			: base(connection)
		{
		}
	}

	#endregion
}
