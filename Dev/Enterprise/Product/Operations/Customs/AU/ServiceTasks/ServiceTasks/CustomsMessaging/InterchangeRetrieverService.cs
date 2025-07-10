using System.Linq;
using System.Threading;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	InterchangeRetrieverService.Code,
	InterchangeRetrieverService.Description,
	"AUC",
	typeof(InterchangeRetrieverService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Australia,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	InterchangeRetrieverService.Code,
	MailDBItemsSchema.Constants.TableName,
	new[] {
		MailDBItemsSchema.Constants.MI_Status + "=QUE",
		MailDBItemsSchema.Constants.MI_Direction + "=RCV",
		MailDBItemsSchema.Constants.MI_Application + "=AUI",
	},
	InterchangeRetrieverService.Description
)]

namespace Enterprise.Customs.AU.ServiceTasks
{
	public class InterchangeRetrieverService : ServiceProviderImpl
	{
		public InterchangeRetrieverService() : base() { }

		public override void RunTask(CancellationToken theTokenHasBeenReactedTo)
		{
			var branch = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.Australia).FirstOrDefault()?.FirstActiveBranch;
			if (branch != null)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				using (var retriever = CreateNewInterchangeRetrieverAndHookLoggingEvent())
				{
					retriever.ExecuteBatch(theTokenHasBeenReactedTo);
				}
			}
		}

		AUCInterchangeRetriever CreateNewInterchangeRetrieverAndHookLoggingEvent()
		{
			var result = CreateAUCInterchangeRetriever();
			result.Logger.OnLogInfoAdded += Logger_OnLogInfoAdded;
			return result;
		}

		protected virtual AUCInterchangeRetriever CreateAUCInterchangeRetriever()
		{
			return new AUCInterchangeRetriever();
		}

		void Logger_OnLogInfoAdded(string log, Integration.LogType logType)
		{
			ServiceLogger.Log(logType, log);
		}

		public const string Code = "AUI";
		public const string Description = "Australian Customs Interchange Retriever";
	}
}


