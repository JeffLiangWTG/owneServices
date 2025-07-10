using System;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData
{
	class StatusHandlerFactory
	{
		public static StatusHandlerBase GetHandler(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData)
		{
			switch (ediTokenAuthOnBoardingData.TOD_Status)
			{
				case OnBoardingStatuses.Codes.Queued:
					return new QueuedStatusHandler(ediTokenAuthOnBoardingData);

				case OnBoardingStatuses.Codes.StagingMergedAndVerified:
					return new StagingMergedAndVerifiedStatusHandler(ediTokenAuthOnBoardingData);

				case OnBoardingStatuses.Codes.Revert:
					return new RevertStatusHandler(ediTokenAuthOnBoardingData);

				default:
					throw new InvalidOperationException($"Unsupported status {ediTokenAuthOnBoardingData.TOD_Status}");
			}
		}
	}
}
