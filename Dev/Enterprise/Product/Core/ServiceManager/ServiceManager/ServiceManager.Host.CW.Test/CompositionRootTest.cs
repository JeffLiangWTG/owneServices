using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing
{
	class CompositionRootTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			services = CreateServices();
		}

		IServiceCollection services;

		public void TestWrongParameters()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => CompositionRoot.AddRegistrations(null, Array.Empty<string>()));
			AssertEquals("services", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => CompositionRoot.AddRegistrations(Mock.Of<IServiceCollection>(), null));
			AssertEquals("args", result.ParamName);
		}

		public void TestServiceTaskResolution()
		{
			using (var provider = services.BuildServiceProvider())
			{
				AssertTypeResolutionCollection<IServiceManagerTask>(
					provider,
					new[]
					{
						typeof(Controller),
						typeof(LogFileArchiveCleanerTask),
						typeof(OldVersionsRemoverTask),
						typeof(HttpListenerTask),
						typeof(RequestQueueProcessor),
						typeof(QueueMonitorTask),
						typeof(RefreshRegistryTask),
					});
			}
		}

		public void TestRegistrations()
		{
			using (var provider = services.BuildServiceProvider())
			{
				services
					.Select(r => r.ServiceType)
					.Distinct()
					.ToList()
					.ForEach(t => AssertRegistration(provider, t));
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

			AssertContainsExactElementsInAnyOrder(Array.Empty<Type>(), duplicateSingletons);

			Assert(true);
		}

		public void TestUniqueRegistrationsWhereExpected()
		{
			var expectedMultipleRegistrations = new[] { typeof(IServiceManagerTask) };

			var unexpectedMultipleRegistrations = services
				.Where(r => !expectedMultipleRegistrations.Contains(r.ServiceType))
				.GroupBy(r => r.ServiceType)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();

			AssertContainsExactElementsInAnyOrder(Array.Empty<Type>(), unexpectedMultipleRegistrations);

			Assert(true);
		}

		[ExpectNoExceptions]
		public void TestSingletonsWithMultipleRegistrationsAreSafelyDisposable()
		{
			using (var provider = services.BuildServiceProvider())
			{
				var resolvedObjects = services
					.Where(r => r.ImplementationFactory != null && r.Lifetime == ServiceLifetime.Singleton)
					.Select(r => r.ImplementationFactory(provider))
					.ToList()
					.Distinct();

				foreach (var resolvedObject in resolvedObjects)
				{
					if (resolvedObject is RequestQueueProcessor queueProcessor)
					{
						queueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());
					}
					if (resolvedObject is IServiceManagerTask serviceTask)
					{
						if (serviceTask is QueueMonitorTask queueMonitorTask)
						{
							queueMonitorTask.ConfigureQueueMonitor(Mock.Of<ITaskStatusProvider>());
						}

						serviceTask.Initialise(CancellationToken.None);
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestHostExceptionHandlerIsExceptionHandler()
		{
			using (var provider = services.BuildServiceProvider())
			{
				var exceptionHandler = provider.GetService<IExceptionHandler>();

				AssertType<HostExceptionHandler>(exceptionHandler);
			}
		}

		void AssertRegistration<TService>(IServiceProvider provider) =>
		AssertRegistration(provider, typeof(TService));

		void AssertRegistration(IServiceProvider provider, Type serviceType)
		{
			var message = $"Registration {serviceType.Name}";
			var actual = provider.GetService(serviceType);

			AssertNotNull(message, actual);
		}

		void AssertInstanceResolution<TService>(IServiceProvider provider, TService expected) where TService : class
		{
			var message = $"Instance {nameof(TService)} -> {expected.GetType().Name}";
			var actual = provider.GetRequiredService<TService>();

			AssertNotNull(message, actual);
			AssertEquals(message, expected, actual);
		}

		void AssertTypeResolution<TService>(IServiceProvider provider) =>
			AssertTypeResolution(provider, typeof(TService));

		void AssertTypeResolution(IServiceProvider provider, Type serviceType)
		{
			var message = $"Resolve {serviceType.Name}";
			var actual = provider.GetService(serviceType);

			AssertNotNull(message, actual);
		}

		void AssertTypeResolutionCollection<TService>(IServiceProvider provider, IEnumerable<Type> expected)
		{
			var message = nameof(TService);
			var actual = provider.GetRequiredService<IEnumerable<TService>>();

			AssertNotNull(message, actual);
			AssertContainsExactElementsInAnyOrder(expected, actual.Select(a => a.GetType()).ToArray());
		}

		IServiceCollection CreateServices() =>
			new ServiceCollection()
				.AddRegistrations(new[] { "ServerName", "DatabaseName" })
				.RemoveAll<IHostRegistrySettings>()
				.AddSingleton(Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskHttpProcessorMaxThreads == 1));
	}
}
