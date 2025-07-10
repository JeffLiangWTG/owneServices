using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.DataTransfer.Testing
{
	[TestedType(typeof(AccountingDataCSVExportProcessTask))]
	public class AccountingDataCSVExportProcessTaskTest : ServiceTaskTestCase<AccountingDataCSVExportProcessTask>
	{
		public void TestRunTask()
		{
			var task = new AccountingDataCSVExportProcessTask();
			InitialiseAndRunTaskSchedule(task);
			RunTaskSchedule(task);
		}

		public void TestRunTaskNonErrorRecord()
		{
			var task = new AccountingDataCSVExportProcessTask();
			InitialiseTaskSchedule(task);

			using (SystemDataRegistry.Instance.EnableAutomaticGLTransactionsCSVExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (Env.Instance.TemporaryServiceTaskContext(AccountingDataCSVExportProcessTask.Code, canRunInAnyBranch: true))
			{
				task.RunTask();
			}
			AssertEquals("Error Report's count should be zero!", 0, ErrorReporter.TotalErrorCount);
		}

		// No nudging: no queue table.
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestHasNoQueueProvider()
		{
			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode("ACE");
			AssertNull("No queue table for this service task.", queueProvider);
		}
	}
}
