using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.EConversation.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Mail.ServiceTasks.Test
{
	[TestedType(typeof(ImplementationEmailProcessorServiceTask))]
	class ImplementationEmailProcessorServiceTaskTest : EdiEmailProcessorServiceProviderTestCase<ImplementationEmailProcessorServiceTask>
	{
		[TestDate(2005, 11, 1)]
		public void TestRunTask()
		{
			SetupTestDataForTestRunTask();

			var emailReaderFactory = new EmailReaderFactoryForTest(PopulateEmailReaderForTestRunTask);
			CreateServiceTask(emailReaderFactory).RunTask();

			AssertLogsAndProcessedMailForTestRunTask();
		}

		public void TestServiceTaskCanRunInAnyBranch()
		{
			ErrorReporter.Clear();
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var serviceTask = new ImplementationEmailProcessorServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			AssertEquals($"Precondition failed, errors in error reporter:\n{string.Join("\n\n", ErrorReporter.ExceptionsThrown)}", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => serviceTask.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestServiceTaskCanRunInAnyBranch_IEP()
		{
			ErrorReporter.Clear();
			SetupTestDataForTestRunTask();
			var emailReaderFactory = new EmailReaderFactoryForTest(PopulateEmailReaderForTestRunTask);
			var serviceTask = new ImplementationEmailProcessorServiceTask(emailReaderFactory) { ServiceLogger = TestLogger };
			AssertEquals($"Precondition failed, errors in error reporter:\n{string.Join("\n\n", ErrorReporter.ExceptionsThrown)}", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => serviceTask.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			AssertLogsAndProcessedMailForTestRunTask();
			ErrorReporter.Clear();
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			EDIDataRegistry.Instance.ImplementationMailBox.Server = "TestMailServer";
			EDIDataRegistry.Instance.ImplementationMailBox.UserName = "Implementation";
		}

		protected override ImplementationEmailProcessorServiceTask CreateServiceTaskCore(IEmailReaderFactory emailReaderFactory)
		{
			return new ImplementationEmailProcessorServiceTask(emailReaderFactory);
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

		void PopulateEmailReaderForTestRunTask(EmailReaderForTest reader)
		{
			reader.AddEmailBundle(
				new EmailBuilderForTesting().From("test@cargowise.com").Subject("attach this to PRJ00000101").GetEmail(),
				new EmailBuilderForTesting().From("test@cargowise.com").Subject("please create a new project").GetEmail());
		}

		void AssertLogsAndProcessedMailForTestRunTask()
		{
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Information, "Email:1 Attachments:0 From:<test@cargowise.com> Subject:attach this to PRJ00000101", null),
				new LogForTest(LogType.Information, "Email attached to: PRJ00000101", null),
				new LogForTest(LogType.Information, "Email:2 Attachments:0 From:<test@cargowise.com> Subject:please create a new project", null),
				new LogForTest(LogType.Information, "Processed 2 emails", null),
				new LogForTest(LogType.Information, "2 emails read, processed and deleted", null));
			ZQuery mailItemQuery = new ZQuery(MailDBItemsSchema.MI_Application, EDIMailApplication.Implementation);
			MailItem[] createdMailItems = Factory.Load<MailItem>(mailItemQuery);
			AssertEquals(2, createdMailItems.Length);
			Assert("Should be created and processed (attached)", createdMailItems.Any(m => m.MI_Subject == "attach this to PRJ00000101" && m.MI_Status == MailStatus.Processed));
			Assert("Should be created and unprocessed (unattached)", createdMailItems.Any(m => m.MI_Subject == "please create a new project" && m.MI_Status == MailStatus.Unprocessed));
			EDIProject[] existingProjects = new BusinessObjectFactory().Load<EDIProject>(new ZQuery());
			AssertEquals(1, existingProjects.Length);
			AssertEquals("PRJ00000101", existingProjects[0].WKP_ProjectNumber);
			AssertEquals(1, ((IDocManagerSupport)existingProjects[0]).DocManagerInfo.AllEDocs.Count);
			AssertEquals("attach this to PRJ00000101.eml", ((IDocManagerSupport)existingProjects[0]).DocManagerInfo.AllEDocs[0].FileName);
		}

		void SetupTestDataForTestRunTask()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "projectmanager@cargowise.com";
			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			project.WKP_ProjectNumber = "PRJ00000101";
			project.WKP_GS_NKProjectManager = staff.GS_Code;
			Factory.Save();
		}
	}
}
