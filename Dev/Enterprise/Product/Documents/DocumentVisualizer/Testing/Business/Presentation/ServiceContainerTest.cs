using System;
using System.Linq;
using System.Reflection;
using Enterprise.DocumentVisualizer.Presentation;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class ServiceContainerTest : TestCase
	{
		#region TestRegisterAndResolve

		public void TestRegisterAndResolve()
		{
			IServiceContainer services = new ServiceContainer();

			var service = new TestService();

			services.Register<ITestService>(() => service);

			AssertEquals("Resolved service", service, services.Resolve<ITestService>());
		}

		#endregion

		#region TestResolveNullService

		public void TestResolveNullService()
		{
			IServiceContainer services = new ServiceContainer();

			var nullService = services.Resolve<ITestService>();

			AssertNotNull("Created null service", nullService);

			var baseService = nullService;

			AssertNotNull("Created null service", baseService);
		}

		#endregion

		#region TestProxiesForAllKnownServices

		public void TestProxiesForAllKnownServices()
		{
			var serviceTypes = typeof(ServiceContainer).Assembly.GetTypes()
				.Where(type => type.IsInterface && type.Name.EndsWith("Service"));

			var method = typeof(ServiceContainer).GetMethod("Resolve", BindingFlags.Instance | BindingFlags.Public);

			AssertNotNull("Prerequisite: resolve method found", method);

			var serviceContainer = new ServiceContainer();

			foreach (var serviceType in serviceTypes)
			{
				AssertProxy(serviceContainer, method, serviceType);
			}
		}

		void AssertProxy(ServiceContainer serviceContainer, MethodInfo resolveMethod, Type type)
		{
			var generic = resolveMethod.MakeGenericMethod(type);

			var proxy1 = generic.Invoke(serviceContainer, null);

			AssertNotNull(string.Format("Proxy for {0}", type.Name), proxy1);

			var proxy2 = generic.Invoke(serviceContainer, null);

			AssertEquals("Retrieved cached instance of proxy", proxy1, proxy2);
		}

		#endregion
	}

	public interface ITestService
	{
	}

	sealed class TestService : ITestService
	{
	}
}