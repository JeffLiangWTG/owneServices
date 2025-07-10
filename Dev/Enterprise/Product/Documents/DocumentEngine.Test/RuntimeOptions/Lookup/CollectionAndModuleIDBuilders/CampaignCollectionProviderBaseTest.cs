using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestsSubclassesOf(typeof(CampaignCollectionProvider), typeof(TestExcludeCollectionProviderAllHaveTestCase))]
	public abstract class CampaignCollectionProviderBaseTest : CollectionProviderBaseTest
	{
		public IGlbCompanyCampaign CreateCampaign(ZString campaignType)
		{
			IGlbCompanyCampaign result = Factory.New<IGlbCompanyCampaign>();
			result.G0_BroadcastVoteSurveyExam = campaignType;
			return result;
		}

		protected override Type ExpectedCollectionType => typeof(GlbCompanyCampaignCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.GlbCompanyCampaign;
	}
}
