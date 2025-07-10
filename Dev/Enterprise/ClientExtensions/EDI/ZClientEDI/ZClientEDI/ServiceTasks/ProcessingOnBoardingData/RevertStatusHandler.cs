using CargoWise.Types;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData
{
	internal class RevertStatusHandler : StatusHandlerBase
	{
		public RevertStatusHandler(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData) : base(ediTokenAuthOnBoardingData)
		{ }

		const string StagingRepositorySubFolder = "CargoWiseB2CTest01";

		public override void Handle()
		{
			if (!string.IsNullOrEmpty(EdiTokenAuthOnBoardingData.TOD_StagingPRLink))
			{
				EdiTokenAuthOnBoardingData.TOD_StagingPRLink = ZString.Empty;
				CreatePRAndMerge(StagingRepositorySubFolder, true);
			}

			if (!string.IsNullOrEmpty(EdiTokenAuthOnBoardingData.TOD_ProdPRLink))
			{
				EdiTokenAuthOnBoardingData.TOD_ProdPRLink = ZString.Empty;
				CreatePRAndMerge(EdiTokenAuthOnBoardingData.Tenant.IDT_Name, true);
			}

			EdiTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.New;
		}
	}
}
