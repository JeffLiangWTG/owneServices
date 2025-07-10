using System;
using System.Collections.Generic;
using Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.ServiceTask.Testing
{
	[TestedType(typeof(IssueManagerProcessorServiceTask))]
	class SqlAlertProcessingTaskTest : ServiceTaskTestCase<SqlAlertProcessingTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestCheckWiseGridReportingConnectionString_HostServiceRequirementIsDefined()
		{
			var methodInfo = typeof(SqlAlertProcessingTask).GetMethod(nameof(SqlAlertProcessingTask.CheckWiseGridReportingConnectionString));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		public void TestCheckWiseGridReportingConnectionString_ShouldReturnNoErrorMsg_WhenRegistryValueIsSet()
		{
			using (EDIDataRegistry.Instance.ExternalMonitoringWiseGridReportingConnectionString.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "db connection string"))
			{
				var message = SqlAlertProcessingTask.CheckWiseGridReportingConnectionString();
				AssertEquals(message, string.Empty);
			}
		}

		public void TestCheckWiseGridReportingConnectionString_ShouldReturnErrorMsg_WhenRegistryValueIsNotSet()
		{
			using (EDIDataRegistry.Instance.ExternalMonitoringWiseGridReportingConnectionString.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var message = SqlAlertProcessingTask.CheckWiseGridReportingConnectionString();
				AssertEquals($"The registry setting '{EDIDataRegistry.Instance.ExternalMonitoringWiseGridReportingConnectionString.GetLocationInEnglish()}' has not been configured.", message);
			}
		}
	}
}
