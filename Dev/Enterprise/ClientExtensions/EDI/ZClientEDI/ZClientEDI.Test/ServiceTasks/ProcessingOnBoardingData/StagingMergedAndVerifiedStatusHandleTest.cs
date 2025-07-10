using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using NUnit.Framework;

namespace ZClientEDI.Test.ServiceTasks.ProcessingOnBoardingData
{
	[TestedType(typeof(StagingMergedAndVerifiedStatusHandler))]
	internal class StagingMergedAndVerifiedStatusHandleTest : TestCaseWithFactory
	{
		public void TestHandle()
		{
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.StagingMergedAndVerified;
			ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = "ABC";
			ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = "1234567";

			Factory.Save();

			AssertEquals(OnBoardingStatuses.Codes.StagingMergedAndVerified, ediTokenAuthOnBoardingData.TOD_Status);
			AssertEquals(ZString.Empty, ediTokenAuthOnBoardingData.TOD_StagingPRLink);

			new StagingMergedAndVerifiedStatusHandlerForTesting(ediTokenAuthOnBoardingData).Handle();

			AssertEquals(OnBoardingStatuses.Codes.ProductionPullRequest, ediTokenAuthOnBoardingData.TOD_Status);
			AssertNotNull(ediTokenAuthOnBoardingData.TOD_StagingPRLink);
		}

		public void TestRepositorySubFolder()
		{
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_Name = "Test";
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.StagingMergedAndVerified;
			ediTokenAuthOnBoardingData.TOD_IDT = tenant.PK;
			var handler = new StagingMergedAndVerifiedStatusHandlerForTesting(ediTokenAuthOnBoardingData);
			AssertEquals(tenant.IDT_Name, handler.RepositorySubFolderForTest);
		}

		public void TestHandleWhenCreatePRFailed()
		{
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_Name = "Test";
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.StagingMergedAndVerified;
			ediTokenAuthOnBoardingData.TOD_IDT = tenant.PK;
			var message = AssertExceptionThrown<InvalidOperationException>(() => new StagingMergedAndVerifiedStatusHandlerForTestingFailed(ediTokenAuthOnBoardingData).Handle()).Message;
			AssertEquals("Create PR failed.", message);
			AssertEquals(OnBoardingStatuses.Codes.StagingMergedAndVerified, ediTokenAuthOnBoardingData.TOD_Status);
			AssertEquals(string.Empty, ediTokenAuthOnBoardingData.TOD_StagingPRLink);
		}

		class StagingMergedAndVerifiedStatusHandlerForTesting : StagingMergedAndVerifiedStatusHandler
		{
			public StagingMergedAndVerifiedStatusHandlerForTesting(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData) : base(ediTokenAuthOnBoardingData)
			{
			}

			protected override string CreatePRAndMerge(string subFolder, bool isRemove = false)
			{
				return "http://crikey.wtg.zone/";
			}
		}

		class StagingMergedAndVerifiedStatusHandlerForTestingFailed : StagingMergedAndVerifiedStatusHandlerForTesting
		{
			public StagingMergedAndVerifiedStatusHandlerForTestingFailed(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData) : base(ediTokenAuthOnBoardingData)
			{
			}

			protected override string CreatePRAndMerge(string subFolder, bool isRemove = false)
			{
				throw new InvalidOperationException("Create PR failed.");
			}
		}
	}
}
