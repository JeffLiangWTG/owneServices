using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using XH.Framework.Logging;
using XH.XT.Monitoring.HealthCheckService.HealthChecks;

namespace XH.XT.Monitoring.HealthCheckService.IntegrationTests.xTHealthCheck
{
	[TestFixture]
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	public class ServiceHealthCheckTests : TestBase<ServiceHealthCheck>
	{
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

				return new object[] { logger, httpContextAccessor.Object, new Mock<IConfiguration>().Object };
			}
		}

		[Test]
		public void xTHealthCheckService_Test_WtgStatus()
		{
			var response = GetClient().GetAsync("/wtg/status").Result;
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var expectedResult = $"""
				INFO(Health Check Url): http://localhost/wtg/status
				{ExpectedHeathCheckServerHostName}
				INFO(xTHealthCheckService): Service is alive
				""";

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);
		}
	}
}
