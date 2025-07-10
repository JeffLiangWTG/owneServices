using CargoWise.Data.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CargoWise.Data.Test.Services
{
	public class DbServiceLocatorTest : TestCase
	{
		DbServiceLocator GlobalServiceLocator { get; } = CreateServiceLocatorForTest();
		public void TestFetchingDefaultServices()
		{
			var someService = GlobalServiceLocator.GetService<ISomeService>();
			Assert(someService is SomeService);

			var secondService = GlobalServiceLocator.GetService<ISecondService>();
			Assert(secondService is SecondService);
		}

		public interface ISomeService { }
		public class SomeService : ISomeService { }

		public interface ISecondService { }
		public class SecondService : ISecondService { }

		// Creating a service locator for testing, so these tests don't need to depend
		// on the global service locator.
		static DbServiceLocator CreateServiceLocatorForTest()
		{
			var services = new DbServiceLocator((services) =>
			{
				services.AddSingleton<ISomeService, SomeService>();
				services.AddSingleton<ISecondService, SecondService>();
			});

			return services;
		}
	}
}
