using System;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.Business.Test
{
	[TestedType(typeof(EDIGlbCompanyCampaign))]
	sealed class EDIGlbCompanyCampaignTest : GlbCompanyCampaignTest
	{
		protected override ModuleIdentifier ExpectedModuleID
		{
			get
			{
				return ModuleIDs.DripMarketingFilterRuleEDI;
			}
		}

		public void TestLookups()
		{
			EDIGlbCompanyCampaign campaign = (EDIGlbCompanyCampaign)CachedBusinessObject;
			AssertEquals(typeof(EDIGlbCompanyCampaignLookups), campaign.Lookups.GetType());
		}

		public void TestCampaignsItemsSentOfCorrectType()
		{
			EDIGlbCompanyCampaign campaign = Factory.New<EDIGlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = EDICampaignTypeList.Codes.WiseServicePartnerSurvey;

			AssertEquals(typeof(EDIGlbCompanyCampaignItemCollection), campaign.CampaignsItemsSent.GetType());
		}

		public void TestTypeDeciders()
		{
			GlbCompanyCampaign surveyCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			LearningCentreCampaign examCampaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertEquals(typeof(EDIGlbCompanyCampaign), newFactory.Load<GlbCompanyCampaign>(surveyCampaign.PK).GetType());
			AssertEquals(typeof(LearningCentreCampaign), newFactory.Load<GlbCompanyCampaign>(examCampaign.PK).GetType());
		}

		public void TestFlags()
		{
			EDIGlbCompanyCampaign campaign = Factory.New<EDIGlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = EDICampaignTypeList.Codes.Voting;
			Assert(campaign.IsVoteCampaign);
			Assert(!campaign.IsProductConsultantSurvey);
			Assert(!campaign.IsWiseServicePartnerSurvey);
			Assert(!campaign.IsSurveyCampaign);

			campaign.G0_BroadcastVoteSurveyExam = EDICampaignTypeList.Codes.ProductConsultantSurvey;
			Assert(!campaign.IsVoteCampaign);
			Assert(campaign.IsProductConsultantSurvey);
			Assert(!campaign.IsWiseServicePartnerSurvey);
			Assert(campaign.IsSurveyCampaign);

			campaign.G0_BroadcastVoteSurveyExam = EDICampaignTypeList.Codes.WiseServicePartnerSurvey;
			Assert(!campaign.IsVoteCampaign);
			Assert(!campaign.IsProductConsultantSurvey);
			Assert(campaign.IsWiseServicePartnerSurvey);
			Assert(campaign.IsSurveyCampaign);
		}

		protected override Type ExpectedGlbCompanyCampaignType
		{
			get { return typeof(EDIGlbCompanyCampaign); }
		}
	}
}
