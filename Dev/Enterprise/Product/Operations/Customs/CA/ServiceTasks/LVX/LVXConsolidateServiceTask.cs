using System.Linq;
using System.Threading;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.ServiceTasks;
using Enterprise.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	LVXConsolidateServiceTask.Code,
	LVXConsolidateServiceTask.FriendlyName,
	MessageProcessorServiceTask.MessageServiceTaskCategory,
	typeof(LVXConsolidateServiceTask),
	MinimumPeriod = "1day",
	MaximumPeriod = "1month",
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Canada,
	DefaultScheduleRunEvery = "1day"
)]

namespace Enterprise.Customs.CA.ServiceTasks
{
	public class LVXConsolidateServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "LCP";
		public const string FriendlyName = "LVX Consolidate Processor";

		protected override void RunTaskCore(CancellationToken token)
		{
			var lvxJobsReadyForConsolidate = LVXJobsConsolidateRunner.SelectLVXJobPKsReadyForConsolidation();
			if (lvxJobsReadyForConsolidate.Any())
			{
				var copProcessor = new LVXConsolidateProcessor(ServiceLogger);
				foreach (var jobs in lvxJobsReadyForConsolidate.GroupBy(x => x.BranchPK, (y => y.DeclarationPK)))
				{
					token.ThrowIfCancellationRequested();
					using (DisposableEnvironment.ForBranch(jobs.Key.ToGuid()))
					{
						copProcessor.Process(jobs);
					}
				}
			}
		}
	}
}
