using System.Threading;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.ServiceTasks;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	InterchangeProcessorServiceTask.InterchangeProcessorServiceCode,
	InterchangeProcessorServiceTask.InterchangeProcessorServiceName,
	MessageProcessorServiceTask.MessageServiceTaskCategory,
	typeof(InterchangeProcessorServiceTask),
	MinimumPeriod = "5Minutes ",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "5minutes"
)]

namespace Enterprise.Customs.CA.ServiceTasks
{
	/// <summary>
	/// Canadian Customs Interchange Processor, processes interchanges from in box
	/// </summary>
	public sealed class InterchangeProcessorServiceTask : ServiceProviderImplUnderDefaultBranch
	{
		public const string InterchangeProcessorServiceCode = "CAI";
		public const string InterchangeProcessorServiceName = "Canadian Customs Interchange Processor";

		[HostedServiceRequirement]
		public static string CheckCompanyInCanadaOrAppliesAllCountries() => BatchProcessorUtilities.CheckCompanyInCanadaOrAppliesAllCountries();

		protected override void RunTaskMain(CancellationToken token)
		{
			using (var retriever = new Retriever())
			{
				retriever.Logger.OnLogInfoAdded += (log, logType) => ServiceLogger.Log(logType, log);
				retriever.ExecuteBatch(token);
			}
		}
	}
}
