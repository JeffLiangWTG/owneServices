using System;
using System.Threading;
using Enterprise.AuditDataServices.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	[TestedType(typeof(GeneralLedgerDataSubscriberServiceTask))]
	class GeneralLedgerDataSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<GeneralLedgerDataSubscriberServiceTask>
	{
		public override GeneralLedgerDataSubscriberServiceTask GenerateServiceTask()
		{
			return new GeneralLedgerDataSubscriberServiceTask();
		}

		protected override bool IsClientSpecific => false;

		public override string ServiceTaskName()
		{
			return GeneralLedgerDataSubscriberServiceTask.Description;
		}

		public void TestTaskIsDisabled()
		{
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var serviceTask = GenerateServiceTask();
			serviceTask.ServiceLogger = new TestServiceLogger();
			serviceTask.RunTask(new CancellationToken(false));
			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			AssertNotContains($"Logs should not contain disabled message", $@"Debug|Related feature is not enabled. The General Ledger Data subscriber service task will not start.", logger.ToString(), true);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
