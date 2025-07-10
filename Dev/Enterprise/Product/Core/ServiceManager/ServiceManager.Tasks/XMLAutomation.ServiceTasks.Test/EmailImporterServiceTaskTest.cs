using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.ServiceTasks.Test
{
	[TestedType(typeof(EmailImporterServiceTask))]
	sealed class EmailImporterServiceTaskTest : ServiceTaskTestCase<EmailImporterServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "EMI", hostedServiceAttribute.Code);
				AssertEquals("Description", "Email Importer Task", hostedServiceAttribute.Description);
				AssertEquals("Category", "BP", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", true, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", false, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
			});
		}

		public void TestTaskShouldBeEnabled()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(2) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			AssertEquals("HasInterfaceConnector", true, eHubMessagingRegistry.Instance.HasInterfaceConnector);

			var methodInfo = typeof(EmailImporterServiceTask).GetMethod(nameof(EmailImporterServiceTask.ShouldRun));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
			AssertEquals("ShouldRun returns empty string", string.Empty, EmailImporterServiceTask.ShouldRun());
		}

		public void TestTaskShouldBeDisabled()
		{
			AssertEquals("HasInterfaceConnector", false, eHubMessagingRegistry.Instance.HasInterfaceConnector);
			var methodInfo = typeof(EmailImporterServiceTask).GetMethod(nameof(EmailImporterServiceTask.ShouldRun));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
			AssertNotEquals("ShouldRun should return a non-empty string", string.Empty, EmailImporterServiceTask.ShouldRun());
			AssertEquals("ShouldRun returns correct message", FormattableString.Invariant($"You need the Interface Connector license to activate the {EmailImporterServiceTask.ServiceTaskDescription}"), EmailImporterServiceTask.ShouldRun());
		}

		public void TestEmailImporterDirectorRun()
		{
			var serviceTask = new EmailImporterServiceTaskForTest { ServiceLogger = new TestServiceLogger() };
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			AssertEquals(1, log.Count);
			var logString = log[0];
			Assert("EmailImportBatchDirector", logString.IndexOf("EmailImportBatchDirector", System.StringComparison.Ordinal) > -1);
		}

		public void TestEventIsPrintedOut()
		{
			var serviceTask = new EmailImporterServiceTaskForTest { ServiceLogger = new TestServiceLogger() };
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			AssertEquals(1, log.Count);
			Assert("EmailImportBatchDirector", log[0].IndexOf("EmailImportBatchDirector", System.StringComparison.Ordinal) > -1);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Test Classes

		class EmailImporterServiceTaskForTest : EmailImporterServiceTask
		{
			protected override EmailImportBatchDirector GetEmailImportBatchDirector()
			{
				return new EmailImportBatchDirectorForTest(this);
			}
		}

		class EmailImportBatchDirectorForTest : EmailImportBatchDirector
		{
			public EmailImportBatchDirectorForTest(INotifications notify) : base(notify) { }
			protected override void RunCore(CancellationToken token)
			{
				Notify.Notify(new WarningNotification("EmailImportBatchDirector"));
			}
		}

		#endregion
	}
}
