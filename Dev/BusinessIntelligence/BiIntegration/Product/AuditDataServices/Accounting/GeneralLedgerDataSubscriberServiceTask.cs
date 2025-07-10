using System.Threading;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.Accounting.GeneralLedgerDataSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.Accounting.GeneralLedgerDataSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.Accounting.GeneralLedgerDataSubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "5minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.Accounting
{
	public class GeneralLedgerDataSubscriberServiceTask : AuditSubscriberTask
	{
		public const string Code = "GLS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public const string Description = "General Ledger Data subscriber service task";

		public override string ServiceTaskCode => Code;
		public override string ServiceTaskDescription => Description;
		public override string SubscriberCode => SubscriberLoader.AccountingSubscriberCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Assembly name")]
		public override string AssemblyName => base.AssemblyName + "Accounting";

		public override void RunTask(CancellationToken token)
		{
			if (!AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value)
			{
				return;
			}

			base.RunTask(token);
		}
	}
}
