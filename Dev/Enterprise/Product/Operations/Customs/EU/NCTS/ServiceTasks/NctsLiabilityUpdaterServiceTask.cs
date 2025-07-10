using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.ServiceTasks;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	NctsLiabilityUpdaterServiceTask.Code,
	NctsLiabilityUpdaterServiceTask.FriendlyName,
	"EUC",
	typeof(NctsLiabilityUpdaterServiceTask),
	ActiveByDefault = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1day")
]

namespace Enterprise.Customs.EU.NCTS.ServiceTasks
{
	public class NctsLiabilityUpdaterServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "NLU";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string FriendlyName = "NCTS Liability Updater Task";

		protected override void RunTaskCore(CancellationToken token)
		{
			var factory = new BusinessObjectFactory();
			new NctsLiabilityUpdater(factory).Run(ServiceLogger, token);
			DisableServiceTask();
		}

		void DisableServiceTask()
		{
			ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskIsActive(Code, isActive: false);
		}
	}
}
