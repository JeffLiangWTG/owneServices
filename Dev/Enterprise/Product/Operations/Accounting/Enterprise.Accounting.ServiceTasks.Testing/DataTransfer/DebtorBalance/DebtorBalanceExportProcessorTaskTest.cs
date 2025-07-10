using System;
using System.Collections.Generic;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.DataTransfer.DebtorBalanceExport.Testing
{
	[TestedType(typeof(DebtorBalanceExportProcessorTask))]
	class DebtorBalanceExportProcessorTaskTest : ServiceTaskTestCase<DebtorBalanceExportProcessorTask>
	{
		[ExpectNoExceptions]
		public void TestRunTask()
		{
			DebtorBalanceExportProcessorTask serviceTask = new DebtorBalanceExportProcessorTask();
			InitialiseAndRunTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);

			using (Env.Instance.TemporaryServiceTaskContext(DebtorBalanceExportProcessorTask.Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}
		}

		// No nudging: no queue table; service task exports current state on regular schedule.
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestHasNoQueueProvider()
		{
			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode("DBE");
			AssertNull("No queue table for this service task; exports current state on regular schedule.", queueProvider);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			SystemDataRegistry.Instance.DebtorOutstandingBalancesExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Env.TempPath);
			TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory);
		}
	}
}
