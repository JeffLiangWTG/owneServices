using System;
using System.Linq;
using Enterprise.ServiceManager.Runner;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;
using WTG.ApplicationLogging.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test
{
	class CompositionRootTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			services = CompositionRoot
				.AddRegistrations(new ServiceCollection(), Mock.Of<IApplicationLoggerFactory>());
		}

		public void TestWrongParameters()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => CompositionRoot.AddRegistrations(null, Mock.Of<IApplicationLoggerFactory>()));
			AssertEquals("services", result.ParamName);
		}

		public void TestResolveTaskRunners()
		{
			CombineAssertions(() =>
			{
				Test<StdInputRunner>(p => CompositionRoot.ResolveInteractiveTaskRunner(p));
				Test<GrpcRunner>(p => CompositionRoot.ResolveGrpcRunner(p, string.Empty));
			});

			void Test<T>(Func<IServiceProvider, ITaskRunner> resolve)
			{
				using (var provider = services.BuildServiceProvider())
				{
					var result = resolve(provider);
					AssertNotNull(result);
					AssertType<T>(result);
				}
			}
		}

		public void TestRegistrations()
		{
			var services = CompositionRoot
				.AddRegistrations(new ServiceCollection(), Mock.Of<IApplicationLoggerFactory>());

			using (var provider = services.BuildServiceProvider())
			{
				services
					.Select(r => r.ServiceType)
					.Distinct()
					.ToList()
					.ForEach(t => AssertRegistration(provider, t));
			}

			void AssertRegistration(IServiceProvider provider, Type serviceType)
			{
				var message = $"Registration {serviceType.Name}";
				var actual = provider.GetService(serviceType);
				AssertNotNull(message, actual);
			}
		}

		public void TestSingletonsAreUniquelyRegistered()
		{
			var duplicateSingletons = services
				.Where(r => r.Lifetime == ServiceLifetime.Singleton && r.ImplementationType != null)
				.GroupBy(r => r.ImplementationType)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();

			AssertEquals(duplicateSingletons.Count, 0);

			Assert(true);
		}

		public void TestExpectedMultipleRegistrations()
		{
			// Arrange
			foreach (var registration in expectedMultipleRegistrations)
			{
				// Act
				var actualCount = services
					.Count(r => r.ServiceType == registration.Item1);

				// Assert
				AssertEquals(registration.Item2, actualCount);
			}
		}

		public void TestNoUnexpectedMultipleRegistrations()
		{
			// Arrange, Act
			var unexpectedMultipleRegistrations = services
				.Where(r => expectedMultipleRegistrations.All(e => e.Item1 != r.ServiceType))
				.GroupBy(r => r.ServiceType)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();

			// Assert
			AssertEquals(unexpectedMultipleRegistrations.Count, 0);
			Assert(true);
		}

		public void TestDbConnectionDisposerCheckerIsRegisteredBeforeTransactionChecker()
		{
			// Checkers appear in the order they are registered when resolved via IEnumerable<IEnvironmentChecker> in EnvironmentCheckerStrategy.
			// DbConnectionDisposerCheckerTransactionChecker needs to do its check before TransactionChecker
			// So DbConnectionDisposerCheckerTransactionChecker needs to be registered before TransactionChecker

			// Arrange
			var environmentCheckerRegistrations = services
				.Where(r => new[] { typeof(IEnvironmentChecker) }.Contains(r.ServiceType))
				.Select(r => r.ImplementationType.Name)
				.ToArray();

			// Act
			var dbConnectionDisposerCheckerIndex = Array.IndexOf(environmentCheckerRegistrations, "DbConnectionDisposerChecker");
			var transactionCheckerIndex = Array.IndexOf(environmentCheckerRegistrations, "TransactionChecker");

			// Assert
			AssertGreaterThan(transactionCheckerIndex, dbConnectionDisposerCheckerIndex);
		}

		public void TestUserContextLeakCheckerIsTheLastRegisteredChecker()
		{
			// Arrange
			var environmentCheckerRegistrations = services
				.Where(r => new[] { typeof(IEnvironmentChecker) }.Contains(r.ServiceType))
				.Select(r => r.ImplementationType.Name)
				.ToArray();

			// Act
			var result = environmentCheckerRegistrations.Last();

			// Assert
			AssertEquals("UserContextLeakChecker", result);
		}

		readonly (Type, int)[] expectedMultipleRegistrations = new (Type, int)[] { (typeof(IEnvironmentChecker), 5) };
		protected IServiceCollection services;
	}
}
