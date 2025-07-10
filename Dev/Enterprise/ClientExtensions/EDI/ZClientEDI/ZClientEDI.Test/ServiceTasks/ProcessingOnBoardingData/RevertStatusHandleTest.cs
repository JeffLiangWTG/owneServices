using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using NUnit.Framework;

namespace ZClientEDI.Test.ServiceTasks.ProcessingOnBoardingData
{
	[TestedType(typeof(RevertStatusHandler))]
	internal class RevertStatusHandleTest : TestCaseWithFactory
	{
		public void TestHandle()
		{
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Revert;
			ediTokenAuthOnBoardingData.TOD_StagingPRLink = "https://baidu.com";
			ediTokenAuthOnBoardingData.TOD_ProdPRLink = "https://google.com";

			ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = "ABC";
			ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = "1234567";

			Factory.Save();

			AssertEquals(OnBoardingStatuses.Codes.Revert, ediTokenAuthOnBoardingData.TOD_Status);
			AssertEquals("https://baidu.com", ediTokenAuthOnBoardingData.TOD_StagingPRLink);
			AssertEquals("https://google.com", ediTokenAuthOnBoardingData.TOD_ProdPRLink);

			new RevertStatusHandlerForTesting(ediTokenAuthOnBoardingData).Handle();

			AssertEquals(OnBoardingStatuses.Codes.New, ediTokenAuthOnBoardingData.TOD_Status);
			AssertEquals(ZString.Empty, ediTokenAuthOnBoardingData.TOD_StagingPRLink);
			AssertEquals(ZString.Empty, ediTokenAuthOnBoardingData.TOD_ProdPRLink);
		}

		public void TestHandleWhenCreatePRFailed()
		{
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_Name = "Test";
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_IDT = tenant.PK;
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Revert;
			ediTokenAuthOnBoardingData.TOD_ProdPRLink = "https://google.com";

			var message = AssertExceptionThrown<InvalidOperationException>(() => new RevertStatusHandlerForTestingFailed(ediTokenAuthOnBoardingData).Handle()).Message;
			AssertEquals("Create PR failed.", message);
			AssertEquals(OnBoardingStatuses.Codes.Revert, ediTokenAuthOnBoardingData.TOD_Status);
			AssertEquals(string.Empty, ediTokenAuthOnBoardingData.TOD_ProdPRLink);
		}

		class RevertStatusHandlerForTesting : RevertStatusHandler
		{
			public RevertStatusHandlerForTesting(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData) : base(ediTokenAuthOnBoardingData)
			{
			}

			protected override string CreatePRAndMerge(string subFolder, bool isRemove = false)
			{
				return "http://crikey.wtg.zone/";
			}
		}

		class RevertStatusHandlerForTestingFailed : RevertStatusHandler
		{
			public RevertStatusHandlerForTestingFailed(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData) : base(ediTokenAuthOnBoardingData)
			{
			}

			protected override string CreatePRAndMerge(string subFolder, bool isRemove = false)
			{
				throw new InvalidOperationException("Create PR failed.");
			}
		}
	}
}
