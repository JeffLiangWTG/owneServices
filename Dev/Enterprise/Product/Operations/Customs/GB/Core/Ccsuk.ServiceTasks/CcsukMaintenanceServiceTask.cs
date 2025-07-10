using System.Threading;
using Enterprise.Customs.ServiceTasks;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.GB.Ccsuk.ServiceTasks.CcsukMaintenanceServiceTask.Code,
	"UK CCSUK Maintenance Task",
	"GBC",
	typeof(Enterprise.Customs.GB.Ccsuk.ServiceTasks.CcsukMaintenanceServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom,
	MinimumPeriod = "24Hours",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "24hours"
	)]

namespace Enterprise.Customs.GB.Ccsuk.ServiceTasks
{
	//This task doesn't need to be nudged, it's a daily housekeeping task

	public class CcsukMaintenanceServiceTask : CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			// Does not create jobs or messages, branch not relevant to finding records. Accesses enterprise-level registry for email address. 
			new CcsukMaintenanceServiceTaskRunner(ServiceLogger).DoEverything(token);
		}

		public const string Code = "GCM"; // GB CCSUK Maint. task
	}
}
