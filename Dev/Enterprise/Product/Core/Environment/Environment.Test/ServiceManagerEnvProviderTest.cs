using System;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	abstract class ServiceManagerEnvProviderBaseTest : TestCase
	{
		public void TestInstanceIsSingleton()
		{
			// Arrange
			// Act
			var result = Env.Instance;
			var results = Enumerable.Range(1, 10).Select(i => Env.Instance).ToArray();

			// Assert
			CombineAssertions(() =>
			{
				AssertNotNull(result);
				Assert(results.All(environment => environment.GetType() == typeof(ServiceTaskEnvironment)));
				foreach (var environment in results)
				{
					AssertEquals(result, environment);
				}
			});
		}

		public void TestEnvironmentTypes()
		{
			CombineAssertions(() =>
			{
				AssertType(typeof(ServiceManagerEnvProvider), Env.GetCurrentProvider());
				AssertType(typeof(ServiceTaskEnvironment), Env.Instance);
				AssertType(DbEnvironment, DbEnv.Instance);
			});
		}

		protected abstract Type DbEnvironment { get; }
	}

	class ServiceManagerEnvProviderTestWithPooledConnections : ServiceManagerEnvProviderBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			envProvider = Env.GetCurrentProvider();
			serviceManagerEnvProvider = new ServiceManagerEnvProvider(true);
			serviceManagerEnvProvider.Enable();
		}

		protected override void TearDown()
		{
			envProvider.Enable();
			serviceManagerEnvProvider.Dispose();
			base.TearDown();
		}

		EnvProvider envProvider;
		ServiceManagerEnvProvider serviceManagerEnvProvider;

		protected override Type DbEnvironment { get; } = typeof(ServiceManagerDbEnvironmentPooled);
	}

	class ServiceManagerEnvProviderTestWithNoPooledConnections : ServiceManagerEnvProviderBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			envProvider = Env.GetCurrentProvider();
			serviceManagerEnvProvider = new ServiceManagerEnvProvider(false);
			serviceManagerEnvProvider.Enable();
		}

		protected override void TearDown()
		{
			envProvider.Enable();
			serviceManagerEnvProvider.Dispose();
			base.TearDown();
		}

		EnvProvider envProvider;
		ServiceManagerEnvProvider serviceManagerEnvProvider;

		protected override Type DbEnvironment { get; } = typeof(ServiceManagerDbEnvironmentUnpooled);
	}
}