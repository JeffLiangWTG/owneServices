using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using NUnit.Framework;
using XH.XT.Monitoring.HealthCheckService.Tests;

namespace XH.XT.Monitoring.HealthCheckService.IntegrationTests
{
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	public abstract class TestBase<T> where T : class, IHealthCheck
	{
		protected virtual Action<IApplicationBuilder> CustomAppConfigureAction => (app) => { };
		protected virtual bool UsingHealthCheckMock => false;
		protected virtual bool UsingServiceMock => false;
		protected virtual string PathBase => $"/{AppName}";
		protected virtual string AppName => "TestProduct";
		protected virtual string HealthCheckName => UsingHealthCheckMock ? typeof(T).Name + "Test" : typeof(T).Name;
		protected Mock<T> HealthCheckMock { get; private set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "An array is appropriate in test context.")]
		protected virtual object[] HealthCheckParams { get; }
		Dictionary<string, string> healthCheckOptions;
		protected Dictionary<string, string> HealthCheckOptions
		{
			get { return healthCheckOptions ??= new Dictionary<string, string> { { $"Options:{PathBase}/wtg/status:0", HealthCheckName } }; }
			set => healthCheckOptions = value;
		}

		protected const string ExpectedHeathCheckUrlRow = "INFO(Health Check Url): http://localhost/TestProduct/wtg/status";
		protected static readonly string ExpectedHeathCheckServerHostName = $"INFO(Health Check Server HostName): {Environment.MachineName}";

		public ServicesMock ServicesMock { get; } = new();

		[SetUp]
		public void Initialize()
		{
			HealthCheckMock = CreateHealthCheckMock();
		}

		protected virtual Mock<T> CreateHealthCheckMock() => new(HealthCheckParams) { CallBase = true };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Object is used outside scope")]
		public HttpClient GetClient(string extraConfigurationJsonFile = "")
		{
			var serviceMocks = new List<(Type, object)>();

			if (UsingServiceMock)
			{
				serviceMocks.AddRange(ServicesMock.GetMocks());
			}

			if (UsingHealthCheckMock)
			{
				serviceMocks.Add(new(typeof(T), HealthCheckMock.Object));
			}

			var customWebApplicationFactory = new CustomWebApplicationFactory(serviceMocks, CustomAppConfigureAction, HealthCheckOptions) { ExtraConfigurationJsonFile = extraConfigurationJsonFile };
			return customWebApplicationFactory.CreateClient();
		}

		protected static Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new FileNotFoundException($"Could not locate embedded resource '{fullResourceName}'");
			}
			return resource;
		}

		protected static string GetEmbeddedResourceAsString(string resourceName)
		{
			using var stream = GetEmbeddedResource(resourceName);
			using var streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd();
		}
	}
}
