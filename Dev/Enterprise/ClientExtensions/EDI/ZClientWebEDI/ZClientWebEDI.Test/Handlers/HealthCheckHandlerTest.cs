using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using CargoWise.EntityFramework.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class HealthCheckHandlerTest : TestCaseWithFactory
	{
		public void TestHealthCheck_Get()
		{
			AssertCallWithValidMethod(HttpMethod.Get.Method);
		}

		public void TestHealthCheck_Head()
		{
			AssertCallWithValidMethod(HttpMethod.Head.Method);
		}

		void AssertCallWithValidMethod(string method)
		{
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			try
			{
				using (var stream = new MemoryStream())
				{
					SetupContext();
					request.Setup(r => r.HttpMethod).Returns(method);
					response.Setup(r => r.OutputStream).Returns(stream);
					var handler = new HealthCheckHandler();
					handler.ProcessRequest(context.Object);
					var responseBody = Encoding.UTF8.GetString(stream.ToArray());
					CombineAssertions(() =>
					{
						AssertEquals((int)HttpStatusCode.OK, response.Object.StatusCode);
						AssertContains("INFO(Database): OK", responseBody);
					});
				}
			}
			finally
			{
				EnvProxy.Instance.Registry.ExpectedClientDLL = string.Empty;
			}
		}

		public void TestHealthCheck_InvalidMethods()
		{
			AssertCallWithInvalidMethod(HttpMethod.Post.Method);
			AssertCallWithInvalidMethod(HttpMethod.Put.Method);
			AssertCallWithInvalidMethod(HttpMethod.Delete.Method);
			AssertCallWithInvalidMethod(HttpMethod.Trace.Method);
			AssertCallWithInvalidMethod(HttpMethod.Options.Method);
		}

		void AssertCallWithInvalidMethod(string method)
		{
			SetupContext();
			request.Setup(r => r.HttpMethod).Returns(method);
			var handler = new HealthCheckHandler();
			handler.ProcessRequest(context.Object);
			AssertEquals((int)HttpStatusCode.MethodNotAllowed, response.Object.StatusCode);
		}

		public void TestHealthCheck_InvalidDbVersion()
		{
			var schemaMajorVersion = SchemaVersion.Application.Major;
			Env.Registry.DatabaseMajorSchemaVersion = schemaMajorVersion - 1;
			try
			{
				using (var stream = new MemoryStream())
				{
					SetupContext();
					request.Setup(r => r.HttpMethod).Returns(HttpMethod.Head.Method);
					response.Setup(r => r.OutputStream).Returns(stream);
					var handler = new HealthCheckHandler();
					handler.ProcessRequest(context.Object);
					var responseBody = Encoding.UTF8.GetString(stream.ToArray());
					CombineAssertions(() =>
					{
						AssertEquals((int)HttpStatusCode.ServiceUnavailable, response.Object.StatusCode);
						AssertContains("ERROR(Database): Database version mismatch", responseBody);
					});
				}
			}
			finally
			{
				Env.Registry.DatabaseMajorSchemaVersion = schemaMajorVersion;
			}
		}

		void SetupContext()
		{
			request = new Mock<HttpRequestBase>();
			response = new Mock<HttpResponseBase>();
			context = new Mock<HttpContextBase>();
			context.SetupAllProperties();
			context.Setup(x => x.Request).Returns(request.Object);
			context.Setup(x => x.Response).Returns(response.Object);
			request.SetupAllProperties();
			response.SetupAllProperties();
		}

		Mock<HttpContextBase> context;
		Mock<HttpRequestBase> request;
		Mock<HttpResponseBase> response;
	}
}