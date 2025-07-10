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
	[TestedType(typeof(DataExporterServiceTask))]
	sealed class DataExporterServiceTaskTest : ServiceTaskTestCase<DataExporterServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "XME", hostedServiceAttribute.Code);
				AssertEquals("Description", "Data Exporter Task", hostedServiceAttribute.Description);
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

			var methodInfo = typeof(DataExporterServiceTask).GetMethod(nameof(DataExporterServiceTask.ShouldRun));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
			AssertEquals("ShouldRun returns empty string", string.Empty, DataExporterServiceTask.ShouldRun());
		}

		public void TestTaskShouldBeDisabled()
		{
			AssertEquals("HasInterfaceConnector", false, eHubMessagingRegistry.Instance.HasInterfaceConnector);
			var methodInfo = typeof(DataExporterServiceTask).GetMethod(nameof(DataExporterServiceTask.ShouldRun));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
			AssertNotEquals("ShouldRun should return a non-empty string", string.Empty, DataExporterServiceTask.ShouldRun());
			AssertEquals("ShouldRun returns correct message", FormattableString.Invariant($"You need the Interface Connector license to activate the {DataExporterServiceTask.ServiceTaskDescription}"), DataExporterServiceTask.ShouldRun());
		}

		public void TestDataExporterDirectorRun()
		{
			var serviceTask = new DataExporterServiceTaskForTest { ServiceLogger = new TestServiceLogger() };
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			AssertEquals(1, log.Count);
			var logString = log[0];
			Assert("BatchExportDirector", logString.IndexOf("BatchExportDirector", System.StringComparison.Ordinal) > -1);
		}

		public void TestEventIsPrintedOut()
		{
			var serviceTask = new DataExporterServiceTaskForTest { ServiceLogger = new TestServiceLogger() };
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			AssertEquals(1, log.Count);
			Assert("BatchExportDirector", log[0].IndexOf("BatchExportDirector", System.StringComparison.Ordinal) > -1);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Test Classes

		class DataExporterServiceTaskForTest : DataExporterServiceTask
		{
			protected override BatchExportDirector GetDataExportDirector()
			{
				return new BatchExportDirectorForTest(this);
			}
		}

		class BatchExportDirectorForTest : BatchExportDirector
		{
			public BatchExportDirectorForTest(INotifications notify) : base(notify) { }
			protected override void RunCore(CancellationToken token)
			{
				Notify.Notify(new WarningNotification("BatchExportDirector"));
			}
		}

		#endregion
	}
}
