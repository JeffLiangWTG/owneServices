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
	[TestedType(typeof(CustomerServiceEmailProcessorServiceTask))]
	class CustomerServiceEmailProcessorServiceTaskTest : EdiEmailProcessorServiceProviderTestCase<CustomerServiceEmailProcessorServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			SetupTestDataForTestRunTask();

			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var serviceTask = new CustomerServiceEmailProcessorServiceTask();
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

		public void TestServiceTaskCanRunInAnyBranch_CSE()
		{
			SetupTestDataForTestRunTask();

			ErrorReporter.Clear();
			var emailReaderFactory = new EmailReaderFactoryForTest(PopulateEmailReader);
			var serviceTask = new CustomerServiceEmailProcessorServiceTask(emailReaderFactory) { ServiceLogger = TestLogger };
			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => serviceTask.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			AssertLogsAndProcessedMail();
			ErrorReporter.Clear();
		}

		[TestDate(2005, 11, 1)]
		public void TestRunTask()
		{
			SetupTestDataForTestRunTask();

			var emailReaderFactory = new EmailReaderFactoryForTest(PopulateEmailReader);
			CreateServiceTask(emailReaderFactory).RunTask();

			AssertLogsAndProcessedMail();
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			EDIDataRegistry.Instance.CustomerServiceMailBox.Server = "TestMailServer";
			EDIDataRegistry.Instance.CustomerServiceMailBox.UserName = "CustomerService";
		}

		protected override CustomerServiceEmailProcessorServiceTask CreateServiceTaskCore(IEmailReaderFactory emailReaderFactory)
		{
			return new CustomerServiceEmailProcessorServiceTask(emailReaderFactory);
		}

		void PopulateEmailReader(EmailReaderForTest reader)
		{
			var uniqueEmailId = CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_IncidentNumber, IncidentNumberForTest)));
			var generateUniqueEmailId = uniqueEmailId;
			var mark = string.Format(IncidentConstants.MarkContent, generateUniqueEmailId);
			var email1 = new EmailBuilderForTesting().From("test@cargowise.com").Subject("attach this to CS00000101").Body(mark).GetEmail();
			var email2 = new EmailBuilderForTesting().From("test@cargowise.com").Subject("please create a new incident").GetEmail();

			reader.AddEmailBundle(email1, email2);
		}

		void AssertLogsAndProcessedMail()
		{
			var incident = Factory.LoadFromNaturalKey<SupportIncident>(IncidentMainSchema.IM_IncidentNumber, IncidentNumberForTest);
			var incidentUniqueId = CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(incident);
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Information, "Email:1 Attachments:0 From:<test@cargowise.com> Subject:attach this to CS00000101", null),
				new LogForTest(LogType.Information, $"Sender email address test@cargowise.com is not in the valid list.\r\nEmail ID {incidentUniqueId} matches original ID {incidentUniqueId}.\r\n", null),
				new LogForTest(LogType.Information, "Email attached to: CS00000101", null),
				new LogForTest(LogType.Information, "Email:2 Attachments:0 From:<test@cargowise.com> Subject:please create a new incident", null),
				new LogForTest(LogType.Information, "Processed 2 emails", null),
				new LogForTest(LogType.Information, "2 emails read, processed and deleted", null));
			ZQuery mailItemQuery = new ZQuery(MailDBItemsSchema.MI_Application, EDIMailApplication.CustomerService);
			MailItem[] createdMailItems = Factory.Load<MailItem>(mailItemQuery);
			AssertEquals(2, createdMailItems.Length);
			Assert("Should be created and processed (attached)", createdMailItems.Any(m => m.MI_Subject == "attach this to CS00000101" && m.MI_Status == MailStatus.Processed));
			Assert("Should be created and unprocessed (unattached)", createdMailItems.Any(m => m.MI_Subject == "please create a new incident" && m.MI_Status == MailStatus.Unprocessed));
			SupportIncident[] existingIncidents = new BusinessObjectFactory().Load<SupportIncident>(new ZQuery());
			AssertEquals(1, existingIncidents.Length);
			AssertEquals("CS00000101", existingIncidents[0].IM_IncidentNumber);
			AssertEquals(1, ((IDocManagerSupport)existingIncidents[0]).DocManagerInfo.AllEDocs.Count);
			AssertEquals("attach this to CS00000101.eml", ((IDocManagerSupport)existingIncidents[0]).DocManagerInfo.AllEDocs[0].FileName);
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

		void SetupTestDataForTestRunTask()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = IncidentNumberForTest;
			Factory.Save();
		}

		string IncidentNumberForTest => "CS00000101";
	}
}
