using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using NUnit.Framework;

namespace ZClientEDI.Test.ServiceTasks.ProcessingOnBoardingData
{
	[TestedType(typeof(StatusHandlerFactory))]
	internal class StatusHandlerFactoryTest : TestCaseWithFactory
	{
		public void TestStatusHandlerFactory()
		{
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();

			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Queued;
			var statusHandleBase = StatusHandlerFactory.GetHandler(ediTokenAuthOnBoardingData);
			AssertType(typeof(QueuedStatusHandler), statusHandleBase);

			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.StagingMergedAndVerified;
			statusHandleBase = StatusHandlerFactory.GetHandler(ediTokenAuthOnBoardingData);
			AssertType(typeof(StagingMergedAndVerifiedStatusHandler), statusHandleBase);

			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Revert;
			statusHandleBase = StatusHandlerFactory.GetHandler(ediTokenAuthOnBoardingData);
			AssertType(typeof(RevertStatusHandler), statusHandleBase);

			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Completed;
			AssertExceptionThrown<InvalidOperationException>(() => { StatusHandlerFactory.GetHandler(ediTokenAuthOnBoardingData); });
		}
	}
}
