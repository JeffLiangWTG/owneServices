using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.Testing
{
	class EdiTokenAuthOnBoardingDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOnBoardingStatusCodes()
		{
			var onBoardingStatusList = NewDummyLookups().OnBoardingStatusList;

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					OnBoardingStatuses.Codes.New,
					OnBoardingStatuses.Codes.Queued,
					OnBoardingStatuses.Codes.StagingPullRequest,
					OnBoardingStatuses.Codes.StagingMergedAndVerified,
					OnBoardingStatuses.Codes.ProductionPullRequest,
					OnBoardingStatuses.Codes.Verified,
					OnBoardingStatuses.Codes.CustomerTestCompleted,
					OnBoardingStatuses.Codes.Completed,
					OnBoardingStatuses.Codes.Revert,
					OnBoardingStatuses.Codes.Error,
				},
				onBoardingStatusList.GetAllCodes());
			AssertContainsExactElementsInAnyOrder(onBoardingStatusList, new OnBoardingStatuses());
		}

		public void TestOnBoardingStatusDescription()
		{
			var onBoardingStatusList = NewDummyLookups().OnBoardingStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("COM",
					onBoardingStatusList.GetCodeFromDescription(nameof(OnBoardingStatuses.Codes.Completed)));
				AssertEquals("ERR",
					onBoardingStatusList.GetCodeFromDescription(nameof(OnBoardingStatuses.Codes.Error)));
				AssertEquals("NEW",
					onBoardingStatusList.GetCodeFromDescription(nameof(OnBoardingStatuses.Codes.New)));
				AssertEquals("QUE",
					onBoardingStatusList.GetCodeFromDescription(nameof(OnBoardingStatuses.Codes.Queued)));
				AssertEquals("VER",
					onBoardingStatusList.GetCodeFromDescription(nameof(OnBoardingStatuses.Codes.Verified)));
				AssertEquals("REV",
					onBoardingStatusList.GetCodeFromDescription(nameof(OnBoardingStatuses.Codes.Revert)));
				// special case for OnBoardingStatuses.ManualVerification, which contains an extra space.
				AssertNull(onBoardingStatusList.GetCodeFromDescription(nameof(OnBoardingStatuses.Codes.StagingPullRequest)));
				AssertEquals("SPR", onBoardingStatusList.GetCodeFromDescription("Staging Pull Request"));
				AssertNull(onBoardingStatusList.GetCodeFromDescription(nameof(OnBoardingStatuses.Codes.StagingMergedAndVerified)));
				AssertEquals("SMV", onBoardingStatusList.GetCodeFromDescription("Staging Merged and Verified"));
				AssertNull(onBoardingStatusList.GetCodeFromDescription(nameof(OnBoardingStatuses.Codes.ProductionPullRequest)));
				AssertEquals("PPR", onBoardingStatusList.GetCodeFromDescription("Production Pull Request"));
				AssertNull(onBoardingStatusList.GetCodeFromDescription(nameof(OnBoardingStatuses.Codes.CustomerTestCompleted)));
				AssertEquals("CTC", onBoardingStatusList.GetCodeFromDescription("Customer Test Completed"));
			});
			// if the below test is failing, add a check for the new status description.
			AssertEquals("The test need to be updated with new values", 10, onBoardingStatusList.Count);
		}

		public void TestClaimMappingIdentifiersList()
		{
			AssertContainsExactElementsInAnyOrder(new OIDCClaimMappingIdentifiers(), NewDummyLookups().ClaimMappingIdentifiersList);
		}

		public void TestOIDCServerTypesList()
		{
			AssertContainsExactElementsInAnyOrder(new OIDCServerTypesList(), NewDummyLookups().OIDCServerTypesList);
		}

		public void TestOnBoardingStatusList()
		{
			AssertContainsExactElementsInAnyOrder(new OnBoardingStatuses(), NewDummyLookups().OnBoardingStatusList);
		}

		public void TestTenants()
		{
			var tenant1 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant1.IDT_Name = "QD1";
			tenant1.IDT_Onboarding = true;
			Factory.Save();
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			AssertContainsExactElementsInAnyOrder(new[] { tenant1 }, ediTokenAuthOnBoardingData.Lookups.Tenants);
		}

		public void TestRelatedIncidents()
		{
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			var otherIncident = Factory.NewWithValidTestData<SupportIncident>();
			AssertNotNull(ediTokenAuthOnBoardingData.Lookups);
			var supportIncidentCollection = ediTokenAuthOnBoardingData.Lookups.RelatedIncidents;
			Assert($"Expected collection to not be loaded but it contains {supportIncidentCollection.Count} elements", !supportIncidentCollection.IsLoaded);
			supportIncidentCollection.Load();
			Assert("Collection should be loaded after Load is called", supportIncidentCollection.IsLoaded);

			AssertContainsExactElementsInAnyOrder(new [] { ediTokenAuthOnBoardingData.Incident, otherIncident }, supportIncidentCollection);
		}

		public void TestVerificationResult()
		{
			AssertEquals("Failed", EdiTokenAuthOnBoardingDataLookups.VerificationResult.Failed);
			AssertEquals("Not Verified", EdiTokenAuthOnBoardingDataLookups.VerificationResult.NotVerified);
			AssertEquals("Success", EdiTokenAuthOnBoardingDataLookups.VerificationResult.Success);
		}

		static EdiTokenAuthOnBoardingDataLookups NewDummyLookups()
		{
			return new EdiTokenAuthOnBoardingDataLookups(null);
		}
	}
}
