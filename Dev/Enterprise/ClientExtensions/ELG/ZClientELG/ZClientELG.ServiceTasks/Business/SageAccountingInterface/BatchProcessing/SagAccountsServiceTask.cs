using System.Globalization;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.ELG.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	SagAccountsServiceTask.Code,
	"Sage Accounting Interface",
	"CSP",
	typeof(SagAccountsServiceTask),
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true)
]

namespace Enterprise.Client.ELG.ServiceTasks
{
	class SagAccountsServiceTask : ServiceProviderImpl
	{
		#region Service Task overrides

		public override void RunTask(CancellationToken token)
		{
			Buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber());

			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany())
			{
				token.ThrowIfCancellationRequested();
				using (branch.SetAsTemporaryContext())
				{
					if (ELGDataRegistry.Instance.EnableSagDataExport && IsEnvironmentDataValid())
					{
						Buffer.Notify(new InfoNotification(string.Format(CultureInfo.InvariantCulture, "==========  Sage accounts data export started for Company '{0}' ==========", branch.Company.GC_Code)));
						ExportDirector.Execute();
						Buffer.Notify(new InfoNotification(string.Format(CultureInfo.InvariantCulture, "==========  Sage accounts data export finished for Company '{0}' ==========", branch.Company.GC_Code)));
					}
				}
			}
		}

		#endregion

		#region Implementation

		bool IsEnvironmentDataValid()
		{
			ZStringBuilder message = ELGDataRegistry.GetIsExportEnvironmentValid();

			if (!message.IsEmpty)
			{
				message.Prepend("==========================================================");
				message.Prepend(string.Format(CultureInfo.InvariantCulture, "Sage Data Export Settings for Company '{0}' are not set or are invalid", GlbCompany.CurrentCompany.GC_Code));
				Buffer.Notify(new ErrorNotification(ErrorType.Error, message.ToStringWithNewLineBetweenAppends()));
			}
			return message.IsEmpty;
		}

		SagAccountsExportDirector ExportDirector
		{
			get { return exportDirector ?? (exportDirector = new SagBatchExportDirector(Factory, Buffer)); }
		}
		SagAccountsExportDirector exportDirector;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		NotificationBuffer Buffer;

		#endregion

#if DEBUG
		public NotificationBuffer GetBuffer()
		{
			return Buffer;
		}
#endif

		public const string Code = "ZE1";
	}
}
