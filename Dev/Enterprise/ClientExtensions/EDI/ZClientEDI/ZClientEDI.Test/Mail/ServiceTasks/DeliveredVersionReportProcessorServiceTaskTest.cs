using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Client.EDI.VersionReporting.BatchProcessor;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.EConversation.Testing;
using Enterprise.Environment;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using MimeKit;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Mail.ServiceTasks.Test
{
	[TestedType(typeof(DeliveredVersionReportProcessorServiceTask))]
	class DeliveredVersionReportProcessorServiceTaskTest : EdiEmailProcessorServiceProviderTestCase<DeliveredVersionReportProcessorServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var serviceTask = new DeliveredVersionReportProcessorServiceTask();
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

		class TaskForTest : DeliveredVersionReportProcessorServiceTask
		{
			public IEmailAttachmentProcessor GetAttachmentProcessor_Exposed(Email email)
			{
				return base.GetAttachmentProcessor(email);
			}
		}

		public void TestGetAttachmentProcessor()
		{
			var task = new TaskForTest();
			var processor = task.GetAttachmentProcessor_Exposed(new Email(new MimeMessage().GetData()));
			AssertType<DeliveredVersionReportProcessor>(processor);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			EDIDataRegistry.Instance.DeliveredVersionReportMailBox.Server = EmailReaderForTest.TestServer;
			EDIDataRegistry.Instance.DeliveredVersionReportMailBox.UserName = EmailReaderForTest.TestMailbox;
		}

		protected override DeliveredVersionReportProcessorServiceTask CreateServiceTaskCore(IEmailReaderFactory emailReaderFactory)
		{
			return new DeliveredVersionReportProcessorServiceTask(emailReaderFactory);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
