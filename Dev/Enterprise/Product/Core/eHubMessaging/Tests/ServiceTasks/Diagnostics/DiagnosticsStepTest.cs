using System.Net;
using System.Net.Http;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.ServiceTasks.Diagnostics;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Diagnostics
{
	class DiagnosticsStepTest : TestCaseWithFactory
	{
		public void TestDiagnosticsStepRun_SuccessfulUrl()
		{
			var response = new HttpResponseMessage(HttpStatusCode.OK);

			var step = new Mock<DiagnosticsStep>("http://www.wisetechglobal.com/") { CallBase = true };
			step.Setup(m => m.GetResponse(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpClient>())).Returns(response);
			step.Object.AddResolutionAction("Resolution Action 1");
			step.Object.AddResolutionAction("Resolution Action 2");
			step.Object.AddResolutionAction("Resolution Action 3");
			step.Object.Run();
			AssertEquals("http://www.wisetechglobal.com/", step.Object.Url.AbsoluteUri);
			AssertEquals(true, step.Object.IsSuccessful);
			AssertEquals("Successfully connected to [http://www.wisetechglobal.com/].", step.Object.Result);
			AssertEquals(@"To Diagnose Further:
Resolution Action 1
Resolution Action 2
Resolution Action 3
", step.Object.ResolutionActions);
		}

		public void TestDiagnosticsStepRun_NotAccessibleUrl()
		{
			var url = "http://localhost/valarmorghulis";

			var step = new Mock<DiagnosticsStep>(url) { CallBase = true };
			step.Setup(m => m.TimeoutInMilliseconds).Returns(20000);
			step.Object.AddResolutionAction("Resolution Action 1");
			step.Object.AddResolutionAction("Resolution Action 2");
			step.Object.AddResolutionAction("Resolution Action 3");

			step.Object.Run();

			AssertEquals("http://localhost/valarmorghulis", step.Object.Url.AbsoluteUri);
			AssertEquals(false, step.Object.IsSuccessful);
			AssertContains("The default proxy has been setup for the HttpWebRequests, This test will fail if the default proxy is removed or changed.", @"Error connecting to [http://localhost/valarmorghulis].
Error Status: 404 - Not Found", step.Object.Result);
			AssertEquals(@"To Diagnose Further:
Resolution Action 1
Resolution Action 2
Resolution Action 3
", step.Object.ResolutionActions);

			step.VerifyAll();
		}

		public void TestDiagnosticsStepRun_NotAccessibleUrl_NotHaveTimeoutIssue()
		{
			var step = new DiagnosticsStep("http://blablattestnotexisrtornotavailable.bla");
			step.AddResolutionAction("Resolution Action 1");
			for (int i = 0; i < 10; i++)
			{
				step.Run();
				AssertNotContains(@"Error connecting to [http://blablattestnotexisrtornotavailable.bla].
Exception Status: Timeout
Exception Message: The operation has timed out", step.Result);
			}
		}
	}
}
