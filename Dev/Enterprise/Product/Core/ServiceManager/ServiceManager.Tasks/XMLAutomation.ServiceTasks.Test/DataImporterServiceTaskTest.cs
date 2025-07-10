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
	[TestedType(typeof(DataImporterServiceTask))]
	sealed class DataImporterServiceTaskTest : ServiceTaskTestCase<DataImporterServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "XMI", hostedServiceAttribute.Code);
				AssertEquals("Description", "Data Importer Task", hostedServiceAttribute.Description);
				AssertEquals("Category", "BP", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", false, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
			});
		}

		public void TestTaskShouldBeEnabled()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(2) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			AssertEquals("HasInterfaceConnector", true, eHubMessagingRegistry.Instance.HasInterfaceConnector);

			var methodInfo = typeof(DataImporterServiceTask).GetMethod(nameof(DataImporterServiceTask.ShouldRun));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
			AssertEquals("ShouldRun returns empty string", string.Empty, DataImporterServiceTask.ShouldRun());
		}

		public void TestTaskShouldBeDisabled()
		{
			AssertEquals("HasInterfaceConnector", false, eHubMessagingRegistry.Instance.HasInterfaceConnector);
			var methodInfo = typeof(DataImporterServiceTask).GetMethod(nameof(DataImporterServiceTask.ShouldRun));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
			AssertNotEquals("ShouldRun should return a non-empty string", string.Empty, DataImporterServiceTask.ShouldRun());
			AssertEquals("ShouldRun returns correct message", FormattableString.Invariant($"You need the Interface Connector license to activate the {DataImporterServiceTask.ServiceTaskDescription}"), DataImporterServiceTask.ShouldRun());
		}

		public void TestDataImporterDirectorRun()
		{
			var serviceTask = new DataImporterServiceTaskForTest { ServiceLogger = new TestServiceLogger() };
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			AssertEquals(3, log.Count);
			var logString = log[1];
			Assert("BatchImportDirector", logString.IndexOf("BatchImportDirector", System.StringComparison.Ordinal) > -1);
		}

		public void TestEventIsPrintedOut()
		{
			var serviceTask = new DataImporterServiceTaskForTest { ServiceLogger = new TestServiceLogger() };
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			AssertEquals(3, log.Count);
			Assert("BatchImportDirector", log[1].IndexOf("BatchImportDirector", System.StringComparison.Ordinal) > -1);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Test Classes

		class DataImporterServiceTaskForTest : DataImporterServiceTask
		{
			protected override BatchImportDirector GetDataImportDirector()
			{
				return new BatchImportDirectorForTest(this);
			}
		}

		class BatchImportDirectorForTest : BatchImportDirector
		{
			public BatchImportDirectorForTest(INotifications notify) : base(notify) { }
			protected override void RunCore(CancellationToken token)
			{
				Notify.Notify(new WarningNotification("BatchImportDirector"));
			}
		}

		#endregion
	}
}
