using System;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	abstract class ServiceProviderTest : TestCase
	{
		public void TestAddAndGetService()
		{
			AssertNull("GetAfterOnSavingService returns null if service is not in a container.", ServiceContainer.GetService<TestService1>());

			ServiceContainer.AddService(service1);
			ServiceContainer.AddService(service2);
			AssertEquals("GetAfterOnSavingService() should get the service added by AddService()", service1, ServiceContainer.GetService<TestService1>());
			AssertEquals("GetAfterOnSavingService() should get the service added by AddService()", service2, ServiceContainer.GetService<TestService2>());
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestAddService_ThrowExceptionIfServiceAlreadyExists()
		{
			ServiceContainer.AddService(service1);
			ServiceContainer.AddService(service1);
		}

		[ExpectNoExceptions]
		public void TestRemoveServiceTwice()
		{
			ServiceContainer.AddService(service1);
			ServiceContainer.RemoveService<TestService1>();
			ServiceContainer.RemoveService<TestService1>();
		}

		public void TestRemoveService()
		{
			ServiceContainer.AddService(service1);
			ServiceContainer.AddService(service2);
			AssertNotNull("Service1 should be added", ServiceContainer.GetService<TestService1>());
			AssertNotNull("Service2 should be added", ServiceContainer.GetService<TestService2>());

			ServiceContainer.RemoveService<TestService1>();
			AssertNull("Service1 should be removed", ServiceContainer.GetService<TestService1>());
			AssertNotNull("Service2 should not be removed", ServiceContainer.GetService<TestService2>());

			ServiceContainer.RemoveService<TestService2>();
			AssertNull("Service1 should be removed", ServiceContainer.GetService<TestService1>());
			AssertNull("Service2 should be removed", ServiceContainer.GetService<TestService2>());
		}

		public void TestIsServiceCollectionEmpty()
		{
			ServiceContainer.AddService(service1);
			ServiceContainer.AddService(service2);
			AssertEquals(false, ServiceContainer.IsServiceCollectionEmpty);

			ServiceContainer.RemoveService<TestService1>();
			AssertEquals(false, ServiceContainer.IsServiceCollectionEmpty);

			ServiceContainer.RemoveService<TestService2>();
			AssertEquals(true, ServiceContainer.IsServiceCollectionEmpty);
		}

		#region Implementation

		protected abstract ServiceProviderBase ServiceContainer { get; }

		readonly TestService1 service1 = new TestService1();
		readonly TestService2 service2 = new TestService2();

		class TestService1 : IService
		{
		}

		class TestService2 : IService
		{
		}

		#endregion
	}
}
