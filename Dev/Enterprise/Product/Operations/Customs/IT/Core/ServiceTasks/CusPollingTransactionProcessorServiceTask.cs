using System.Threading;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.ServiceTasks;
using Enterprise.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		ServiceTaskCodeList.Codes.CusPollingTransactionProcessor,
		ServiceTaskCodeList.Descriptions.CusPollingTransactionProcessor,
		MessagingServiceTask.MessageServiceTaskCategory,
		typeof(CusPollingTransactionProcessorServiceTask),
		RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Italy,
		CanRunInAnyBranch = true,
		MinimumPeriod = "1day",
		DefaultScheduleRunEvery = "1day",
		/// <summary>
		/// UTC start time equivalent to 10-11pm in Italy.
		/// </summary>
		DefaultScheduleStartAtUtc = "21hours",
		ActiveByDefault = true
	)]

namespace Enterprise.Customs.IT.ServiceTasks;

public sealed class CusPollingTransactionProcessorServiceTask : MessagingServiceTask
{
	protected override void RunTaskCore(CancellationToken token)
	{
		foreach (var companyCode in DisposableEnvironment.GetActiveCompanies(Core.Constants.CountryCodes.Italy))
		{
			token.ThrowIfCancellationRequested();

			using (DisposableEnvironment.ForCompany(companyCode))
			{
				RunTaskHandleEmailSendFailure(() => new CusPollingTransactionProcessor(Logger).ExecuteBatch(token));
			}
		}
	}
}
