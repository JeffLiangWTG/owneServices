using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData
{
	internal class StagingMergedAndVerifiedStatusHandler : QueuedStatusHandler
	{
		protected override string RepositorySubFolder => EdiTokenAuthOnBoardingData.Tenant.IDT_Name;
		protected override string TargetStatus => OnBoardingStatuses.Codes.ProductionPullRequest;

		public StagingMergedAndVerifiedStatusHandler(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData) : base(ediTokenAuthOnBoardingData)
		{
		}

		protected override void SetPRLink(string prLink)
		{
			EdiTokenAuthOnBoardingData.TOD_ProdPRLink = prLink;
		}
	}
}
