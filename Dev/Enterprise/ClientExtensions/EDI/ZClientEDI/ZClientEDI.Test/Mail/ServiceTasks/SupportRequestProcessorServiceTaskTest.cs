using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.EConversation.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Mail.ServiceTasks.Test
{
	[TestedType(typeof(SupportRequestProcessorServiceTask))]
	class SupportRequestProcessorServiceTaskTest : EdiEmailProcessorServiceProviderTestCase<SupportRequestProcessorServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var serviceTask = new SupportRequestProcessorServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => serviceTask.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		[TestDate(2005, 11, 1)]
		public void TestRunTask()
		{
			var email = new EmailBuilderForTesting().To("", "TestSupportRequest@edi.net.au").WithAttachment("somefile.xml", @"
<?xml version=""1.0"" encoding=""utf-16""?>
<CustomerServiceRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <LicenceCode>ZUBRAKLOL</LicenceCode>
  <ReportingStaffMemberName>Zarn Bou</ReportingStaffMemberName>
  <Criticality>CR2</Criticality>
  <IncidentSummary>My Incident Summary</IncidentSummary>
  <IncidentDetails>My Incident Details</IncidentDetails>
  <Staff>
    <Staff>
      <Name>Zarn Bou</Name>
      <EmailAddress>zarn@test.com</EmailAddress>
      <Sequence>1</Sequence>
    </Staff>
    <Staff>
      <Name>Someone</Name>
      <Sequence>2</Sequence>
    </Staff>
  </Staff>
  <ApprovingUser>Zarn Bou</ApprovingUser>
</CustomerServiceRequest>".Trim()).GetEmail();
			var emailReaderFactory = new EmailReaderFactoryForTest(r => r.AddEmailBundle(email));

			CreateServiceTask(emailReaderFactory).RunTask();

			SupportIncident[] createdIncidents = Factory.Load<SupportIncident>(new ZQuery());
			AssertEquals(1, createdIncidents.Length);
			AssertEquals("My Incident Summary", createdIncidents[0].IM_Description);
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Information, "Email:1 Attachments:1 From: Subject:", null),
				new LogForTest(LogType.Information, "Request: , Incident: " + createdIncidents[0].IM_IncidentNumber + ", Licence: ZUBRAKLOL, Approving User: \"Zarn Bou\" <>, Status: Incident created.", null),
				new LogForTest(LogType.Information, "Processed 1 emails", null),
				new LogForTest(LogType.Information, "1 emails read, processed and deleted", null));
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			EDIDataRegistry.Instance.SupportRequestMailBox.Server = EmailReaderForTest.TestServer;
			EDIDataRegistry.Instance.SupportRequestMailBox.UserName = EmailReaderForTest.TestMailbox;
		}

		protected override SupportRequestProcessorServiceTask CreateServiceTaskCore(IEmailReaderFactory emailReaderFactory)
		{
			return new SupportRequestProcessorServiceTask(emailReaderFactory);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
