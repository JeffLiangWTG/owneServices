using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.ServiceTasks.Diagnostics;
using Enterprise.ZArchitecture;
using Moq;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Diagnostics
{
	class EHubCommunicationDiagnosterTest : TestCaseWithFactory
	{
		public void TestInboundServiceTask_CommunicationExceptions()
		{
			var testAddress = "ehub-ausyd.cargowise.net";
			var serviceTask = new Mock<InboundServiceTask>(new GatewayAdaptorFactory(), CreateMockDiagnosterFactory(testAddress)) { CallBase = true };
			TestServiceTask_InboundCommunicationExceptions(serviceTask, testAddress);
		}

		public void TestEHubOutboundServiceTask_CommunicationExceptions()
		{
			var testAddress = "ehub-ausyd.cargowise.net";
			var serviceTask = new Mock<eHubOutboundServiceTask>(new GatewayAdaptorFactory(), CreateMockDiagnosterFactory(testAddress)) { CallBase = true };
			TestServiceTask_OutboundCommunicationExceptions(serviceTask, testAddress);
		}

		static IEHubCommunicationDiagnosterFactory CreateMockDiagnosterFactory(string testAddress)
		{
			var responseSuccess = new HttpResponseMessage(HttpStatusCode.OK);
			var responseNotFound = new HttpResponseMessage(HttpStatusCode.NotFound);

			var testUrl = $"https://{testAddress}/eHubGateway/eHubStreamedService.svc?wsdl";
			var eHubCommunicationDiagnoster = new Mock<EHubCommunicationDiagnoster>(testUrl) { CallBase = true };

			var diagnosticsStepSuccess = new Mock<DiagnosticsStep>("http://www.wisetechglobal.com/") { CallBase = true };
			var diagnosticsStepNotFound = new Mock<DiagnosticsStep>(testUrl) { CallBase = true };
			diagnosticsStepSuccess.Setup(m => m.GetResponse(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpClient>())).Returns(responseSuccess);
			diagnosticsStepNotFound.Setup(m => m.GetResponse(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpClient>())).Returns(responseNotFound);
			var diagnosterFactory = new Mock<IEHubCommunicationDiagnosterFactory>(MockBehavior.Strict);
			diagnosterFactory.Setup(m => m.Create(testUrl)).Returns(eHubCommunicationDiagnoster.Object);
			eHubCommunicationDiagnoster.SetupSequence(x => x.CreateNewDiagnosticsStep(It.IsAny<string>()))
				.Returns(diagnosticsStepSuccess.Object).Returns(diagnosticsStepNotFound.Object);
			return diagnosterFactory.Object;
		}

		void TestServiceTask_InboundCommunicationExceptions(Mock<InboundServiceTask> mockServiceTask, string testAddress)
		{
			var notifier = new NotificationBuffer();
			var job = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);
			job.Setup(m => m.Execute(It.IsAny<CancellationToken>()))
				.Throws(new CommunicationException("timeout", new WebException("The underlying connection was closed")));
			job.Setup(m => m.NextExecuteIterationIsScheduled).Returns(false);
			mockServiceTask.Setup(x => x.GetJobs()).Returns(new[] { job.Object });
			mockServiceTask.Setup(x => x.Notifier).Returns(notifier);
			mockServiceTask.Setup(x => x.IsEHubTestingEnabled).Returns(true);
			mockServiceTask.Setup(x => x.DefaultServerAddress).Returns(testAddress);

			mockServiceTask.Object.RunTask();

			var errorMessage = $@"Communication with the server has timed out - The service task will reattempt the transfer on the next run. You do not need to take action unless this timeout has occurred frequently over an extended period of time. The problem may have occurred because of insufficient network bandwidth or eHub was under load and took too long to respond.
Running Diagnostics to check Network Status:

Error connecting to [https://{testAddress}/eHubGateway/eHubStreamedService.svc?wsdl].
Error Status: 404 - Not Found 
To Diagnose Further:
Open a browser and try to access the site [https://{testAddress}/eHubGateway/eHubStreamedService.svc?wsdl].

Successfully connected to [http://www.wisetechglobal.com/].
";
			Assert(notifier.Events.Any(e => e.Message.Contains(errorMessage)));
		}

		void TestServiceTask_OutboundCommunicationExceptions(Mock<eHubOutboundServiceTask> mockServiceTask, string testAddress)
		{
			var notifier = new NotificationBuffer();
			var job = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);
			job.Setup(m => m.Execute(It.IsAny<CancellationToken>()))
				.Throws(new CommunicationException("timeout", new WebException("The underlying connection was closed")));
			job.Setup(m => m.NextExecuteIterationIsScheduled).Returns(false);
			mockServiceTask.Setup(x => x.GetJobs()).Returns(new[] { job.Object });
			mockServiceTask.Setup(x => x.Notifier).Returns(notifier);
			mockServiceTask.Setup(x => x.IsEHubTestingEnabled).Returns(true);
			mockServiceTask.Setup(x => x.DefaultServerAddress).Returns(testAddress);

			mockServiceTask.Object.RunTask();

			var errorMessage = $@"Communication with the server has timed out - The service task will reattempt the transfer on the next run. You do not need to take action unless this timeout has occurred frequently over an extended period of time. The problem may have occurred because of insufficient network bandwidth or eHub was under load and took too long to respond.
Running Diagnostics to check Network Status:

Error connecting to [https://{testAddress}/eHubGateway/eHubStreamedService.svc?wsdl].
Error Status: 404 - Not Found 
To Diagnose Further:
Open a browser and try to access the site [https://{testAddress}/eHubGateway/eHubStreamedService.svc?wsdl].

Successfully connected to [http://www.wisetechglobal.com/].
";
			Assert(notifier.Events.Any(e => e.Message.Contains(errorMessage)));
		}

		public void TestRunEHubCommunicationDiagnostics_RunForEHO_Failure()
		{
			var steps = new List<DiagnosticsStep>();

			var step1 = new DiagnosticsStepMock("http://step1.bla.cw");
			step1.AddResolutionAction("Resolution Action 1");
			step1.AddResolutionAction("Resolution Action 2");
			step1.AddResolutionAction("Resolution Action 3");
			step1.SetIsSuccessful(true);
			step1.SetResult("Connect to http://step1.bla.cw success");
			steps.Add(step1);

			var step2 = new DiagnosticsStepMock("http://step2.bla.cw");
			step2.AddResolutionAction("Resolution Action 1");
			step2.SetIsSuccessful(false);
			step2.SetResult("Failure to connect to http://step2.bla.cw");
			steps.Add(step2);

			var step3 = new DiagnosticsStepMock("http://step3.bla.cw");
			step3.AddResolutionAction("Resolution Action 3");
			step3.SetIsSuccessful(false);
			step3.SetResult("Failure to connect to http://step3.bla.cw success");
			steps.Add(step3);

			var diagnoster = new EHubCommunicationDiagnosterMock("ehubgatewayserver.test.com", steps);
			AssertEquals(@"Running Diagnostics to check Network Status:

Failure to connect to http://step3.bla.cw success
To Diagnose Further:
Resolution Action 3

Failure to connect to http://step2.bla.cw
To Diagnose Further:
Resolution Action 1

Connect to http://step1.bla.cw success
", diagnoster.Run());
		}

		public void TestRunEHubCommunicationDiagnostics_RunForEHO_FailureOnFirstStep()
		{
			var steps = new List<DiagnosticsStep>();

			var step1 = new DiagnosticsStepMock("http://step1.bla.cw");
			step1.AddResolutionAction("Resolution Action 1");
			step1.AddResolutionAction("Resolution Action 2");
			step1.AddResolutionAction("Resolution Action 3");
			step1.SetIsSuccessful(false);
			step1.SetResult("Failure to connect to http://step1.bla.cw");
			steps.Add(step1);

			var step2 = new DiagnosticsStepMock("http://step2.bla.cw");
			step2.AddResolutionAction("Resolution Action 1");
			step2.SetIsSuccessful(false);
			step2.SetResult("Failure to connect to http://step2.bla.cw");
			steps.Add(step2);

			var step3 = new DiagnosticsStepMock("http://step3.bla.cw");
			step3.AddResolutionAction("Resolution Action 3");
			step3.SetIsSuccessful(false);
			step3.SetResult("Failure to connect to http://step3.bla.cw success");
			steps.Add(step3);

			var diagnoster = new EHubCommunicationDiagnosterMock("ehubgatewayserver.test.com", steps);

			AssertEquals(@"Running Diagnostics to check Network Status:

Failure to connect to http://step3.bla.cw success
To Diagnose Further:
Resolution Action 3

Failure to connect to http://step2.bla.cw
To Diagnose Further:
Resolution Action 1

Failure to connect to http://step1.bla.cw
To Diagnose Further:
Resolution Action 1
Resolution Action 2
Resolution Action 3

", diagnoster.Run());
		}

		class EHubCommunicationDiagnosterMock : EHubCommunicationDiagnoster
		{
			public EHubCommunicationDiagnosterMock(string eHubGatewayServerAddress, List<DiagnosticsStep> steps)
				: base(eHubGatewayServerAddress)
			{
				Steps = steps;
			}

			protected override void RunDiagnostics()
			{
			}

			protected override void InitializeDiagnosticsStep()
			{
			}
		}

		class DiagnosticsStepMock : DiagnosticsStep
		{
			public DiagnosticsStepMock(string url) : base(url)
			{
			}

			public void SetIsSuccessful(bool isSuccessful)
			{
				IsSuccessful = isSuccessful;
			}

			public void SetResult(string result)
			{
				Result = result;
			}
		}
	}
}
