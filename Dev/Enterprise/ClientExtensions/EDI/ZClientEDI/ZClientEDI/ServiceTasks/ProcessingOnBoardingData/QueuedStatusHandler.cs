using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData
{
	internal class QueuedStatusHandler : StatusHandlerBase
	{
		protected virtual string RepositorySubFolder => "CargoWiseB2CTest01";
		protected virtual string TargetStatus => OnBoardingStatuses.Codes.StagingPullRequest;
		internal string RepositorySubFolderForTest => RepositorySubFolder;

		public QueuedStatusHandler(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData) : base(ediTokenAuthOnBoardingData)
		{ }

		public override void Handle()
		{
			SetPRLink(CreatePRAndMerge(RepositorySubFolder));
			EdiTokenAuthOnBoardingData.TOD_Status = TargetStatus;
		}

		protected virtual void SetPRLink(string prLink)
		{
			EdiTokenAuthOnBoardingData.TOD_StagingPRLink = prLink;
		}
	}
}
