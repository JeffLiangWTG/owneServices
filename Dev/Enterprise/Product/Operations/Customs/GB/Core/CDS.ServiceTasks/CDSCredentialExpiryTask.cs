using System;
using System.Threading;
using Enterprise.Customs.GB.CDS.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

	[assembly: HostedService(
	CDSCredentialExpiryTask.Code
	, CDSCredentialExpiryTask.FriendlyName
	, CDSCredentialExpiryTask.Category
	, typeof(CDSCredentialExpiryTask)
	, RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom
	, MinimumPeriod = "1Week"
	, CanRunInAnyBranch = true
	, DefaultScheduleRunEvery = "1week"
	, DefaultScheduleDaysOfWeek = [DayOfWeek.Sunday]
)]

namespace Enterprise.Customs.GB.CDS.ServiceTasks
{
	public class CDSCredentialExpiryTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = CDSServiceTaskConstants.CDSCredentialExpiryTaskCode;
		public const string FriendlyName = "GB CDS Credential Expiry Notification";
		public const string Category = CDSServiceTaskConstants.CDSMessageServiceTaskCategory;

		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.UnitedKingdom))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					new CDSExpiringTokenNotificationProcessor(branch.Factory).CheckForExpiringCredentials();
				}
			}
		}
	}
}
