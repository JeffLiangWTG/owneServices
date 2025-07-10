using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZServiceContainerTest : TestCase
	{
		public void TestGetService()
		{
			var service = new TestService();
			ServiceContainer.AddService(service);
			AssertEquals("GetService() should get the service added by AddService()", service, ServiceContainer.GetService<TestService>());
		}

		public void TestRemoveService()
		{
			var service = new TestService();

			ServiceContainer.AddService(service);
			AssertNotNull("Service should be added", ServiceContainer.GetService<TestService>());
			ServiceContainer.RemoveService<TestService>();
			AssertNull("Service should be removed", ServiceContainer.GetService<TestService>());
		}

		public void TestAddService_ShouldAllowAdditionWithoutSpecifyingTypeParameterExplicitly()
		{
			foreach (var service in GetAllServicesToAdd())
			{
				ServiceContainer.AddService(service, service.GetType());
			}

			AssertNotNull(ServiceContainer.GetService<TestService>());
			AssertNotNull(ServiceContainer.GetService<TestService2>());
			AssertNotNull(ServiceContainer.GetService<TestService3>());
		}

		static IEnumerable<IService> GetAllServicesToAdd()
		{
			yield return new TestService();
			yield return new TestService2();
			yield return new TestService3();
		}

		#region Implementation

		readonly ZServiceContainer ServiceContainer = new ZServiceContainer();

		class TestService : IService
		{
		}

		class TestService2 : IService
		{
		}

		class TestService3 : IService
		{
		}

		#endregion
	}
}
