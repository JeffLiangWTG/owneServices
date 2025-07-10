using System;
using System.Linq;
using System.Threading;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.MailFilters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.GB.Pentant.ServiceTasks.PentantDownloaderServiceTask.Code,
	Enterprise.Customs.GB.Pentant.ServiceTasks.PentantDownloaderServiceTask.Description,
	"GBC",
	typeof(Enterprise.Customs.GB.Pentant.ServiceTasks.PentantDownloaderServiceTask),
	AllowsMultipleInstances = false,
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom,
	MinimumPeriod = "600Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.GB.Pentant.ServiceTasks.PentantDownloaderServiceTask.Code,
	MailDBItemsSchema.Constants.TableName,
	new[]
	{
		MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.GbPentantEmails,
		MailDBItemsSchema.Constants.MI_Status      + "=" + MailManager.StatusCodeList.Codes.Queued,
		MailDBItemsSchema.Constants.MI_Direction   + "=" + MailManager.DirectionList.Codes.Receive
	},
	"UK Customs Pentant mail downloader")]

namespace Enterprise.Customs.GB.Pentant.ServiceTasks
{
	public class PentantDownloaderServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			ServiceTaskHelper.LogTaskStarting(ServiceLogger, Code, Description);
			RunTaskMain(token);
			SetupManualNudgeIfRequired(ServiceLogger);
			ServiceTaskHelper.LogTaskFinished(ServiceLogger, Code, Description);
		}

		internal static void SetupManualNudgeIfRequired(ILogger serviceLogger)
		{
			var nudgedRunTime = GBCustomsDataRegistry.Instance.PentantNudgedDateTime.Value;
			if (nudgedRunTime != DateTime.MinValue)
			{
				ServiceTaskHelper.NudgeServiceTaskTime(serviceLogger, Code, nudgedRunTime, true);
			}
			GBCustomsDataRegistry.Instance.PentantNudgedDateTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);
		}

		void RunTaskMain(CancellationToken token)
		{
			var pentantLockoutThreshold = GBCustomsDataRegistry.Instance.PentantWebServiceLockoutThreshold.Value;
			foreach (var company in GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.UnitedKingdom))
			{
				ServiceLogger.Log(Integration.LogType.Information, "Commencing Pentant downloader task for " + company.GC_Code);
				var credentials = CredentialsSetting.GetAllCredentials(company.PK);
				token.ThrowIfCancellationRequested();
				var branches = company.Branches.Where(x => x.GB_IsActive).Select(x => x.PK.ToGuid());

				foreach (var branchPK in branches)
				{
					token.ThrowIfCancellationRequested();
					using (DisposableEnvironment.ForBranch(branchPK))
					{
						RunTaskHandleEmailSendFailure(() =>
						{
							token.ThrowIfCancellationRequested();
							ServiceLogger.Log(Integration.LogType.Information, "About to poll emails for branch " + GlbBranch.CurrentBranch.GB_Code);
							using (var pentantEmailsToMessagesPoller = new PentantEmailsToMessagesPoller(ServiceLogger))
							{
								pentantEmailsToMessagesPoller.ExecuteBatch(token);
							}

							token.ThrowIfCancellationRequested();
							// Look in EdiMessages table and process each waiting message
							using (var pentantEmailProcessor = new PentantProcessor(ServiceLogger))
							{
								pentantEmailProcessor.ExecuteBatch(token);
							}
							ServiceLogger.Log(Integration.LogType.Information, "Finished emails for branch " + GlbBranch.CurrentBranch.GB_Code);
						});
					}
				}
			}
		}

		protected virtual bool ShouldConnectViaFtp(CredentialsSettingCollection credentials)
			=> credentials.Cast<CredentialsSetting>().Any(x => x.IsPentant && !x.IsCDS);
			
		public const string Code = "PDD";
		public const string Description = "Pentant downloader";
	}
}
