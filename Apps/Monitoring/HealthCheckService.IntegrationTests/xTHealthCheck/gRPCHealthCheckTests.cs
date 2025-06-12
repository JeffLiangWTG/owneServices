using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using XH.Framework.GrpcClient;
using XH.Framework.Logging;
using XH.Framework.Logging.Sinks;
using XH.XT.Monitoring.HealthCheckService.HealthChecks;

namespace XH.XT.Monitoring.HealthCheckService.IntegrationTests.xTHealthCheck
{
	[TestFixture]
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	public class gRPCHealthCheckTests : TestBase<GrpcHealthCheck>
	{
		protected override Action<IApplicationBuilder> CustomAppConfigureAction
		{
			get
			{
				return (app) =>
				{
					app.UsePathBase(PathBase);
				};
			}
		}

		protected override object[] HealthCheckParams
		{
			get
			{
				var logger = LoggerFactory.GetLogger();

				var path = new PathString("/grpc/wtg/status");
				var request = new Mock<HttpRequest>();
				request.SetupGet(x => x.Path).Returns(path);
				var httpContext = new Mock<HttpContext>();
				httpContext.SetupGet(x => x.Request).Returns(request.Object);
				var httpContextAccessor = new Mock<IHttpContextAccessor>();
				httpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext.Object);
				var xTClient = new Mock<IXtClient>();

				var mockedSection = new Mock<IConfigurationSection>();
				mockedSection.Setup(x => x.Value)
					.Returns("false");
				var mockedSection2 = new Mock<IConfigurationSection>();
				mockedSection2.Setup(x => x.Value)
					.Returns("5000");
				var configuration = new Mock<IConfiguration>();
				configuration.Setup(x => x.GetSection("HealthCheckSettings:SkipCheck"))
					.Returns(mockedSection.Object);

				return new object[] { logger, httpContextAccessor.Object, xTClient.Object, configuration.Object };
			}
		}

		protected override bool UsingHealthCheckMock => true;

		[Test]
		public void xTHealthCheck_gRPC_Test_WtgStatus()
		{
			HealthCheckMock.Setup(x => x.CheckConnections())
				.Returns(HealthCheckResult.Healthy(data: new Dictionary<string, object>
				{
					{ "gRPC", "Healthy" }
				}));

			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var expectedResult = $"""
				{ExpectedHeathCheckUrlRow}
				{ExpectedHeathCheckServerHostName}
				INFO(gRPC): Healthy
				""";
			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);
		}

		[Test]
		public void xTHealthCheck_gRPC_Test_WtgStatus_Exception_Unavailable()
		{
			using (var log = new UnitTestLoggingScope())
			{
				HealthCheckMock.Setup(x => x.CheckConnections()).Throws(new Grpc.Core.RpcException(new Grpc.Core.Status(Grpc.Core.StatusCode.Unavailable, string.Empty)));

				var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
				Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

				var expectedResult = $"""
					{ExpectedHeathCheckUrlRow}
					{ExpectedHeathCheckServerHostName}
					ERROR(TestProduct): Grpc.Core.RpcException: Status(StatusCode="Unavailable", Detail="")
					""";
				var actual = response.Content.ReadAsStringAsync().Result;
				Assert.That(actual, Is.EqualTo(expectedResult));

				Assert.That(log.Events.Any(x => x.Exception is Grpc.Core.RpcException && x.Properties["HealthCheckName"].ToString() == "\"grpc\""));
			}
		}
	}
}
