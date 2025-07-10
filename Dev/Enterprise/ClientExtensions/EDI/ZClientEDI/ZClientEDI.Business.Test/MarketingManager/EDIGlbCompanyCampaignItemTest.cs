using System;
using System.Net;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.Business.Test
{
	[TestedType(typeof(EDIGlbCompanyCampaignItem))]
	public class EDIGlbCompanyCampaignItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDeciders()
		{
			GlbCompanyCampaign surveyCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItem surveyItem = surveyCampaign.CampaignsItemsSent.AddNew();
			surveyItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			surveyItem.G8_RecipientID = ZGuid.NewZGuid();
			LearningCentreCampaign examCampaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			LearningCentreCampaignItem examItem = examCampaign.CampaignsItemsSent.AddNew();
			examItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			examItem.G8_RecipientID = ZGuid.NewZGuid();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertEquals(typeof(EDIGlbCompanyCampaignItem), newFactory.Load<GlbCompanyCampaignItem>(surveyItem.PK).GetType());
			AssertEquals(typeof(LearningCentreCampaignItem), newFactory.Load<GlbCompanyCampaignItem>(examItem.PK).GetType());
		}

		public void TestCanAutoStartVoteExamSurvey()
		{
			EDIGlbCompanyCampaign campaign = Factory.New<EDIGlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = EDICampaignTypeList.Codes.ProductConsultantSurvey;
			EDIGlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			Assert("ProductConsultantSurvey can auto start", ((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).CanAutoStartVoteExamSurvey);
		}

		public void TestGetMyAccountUserAgreementUrl()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "AA";
			contact.OC_Email = "a@test.com";
			var campaign = Factory.NewWithValidTestData<EDIGlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "AA";
			staff.GS_EmailAddress = "a@test.com";
			var staffCampaign = Factory.NewWithValidTestData<EDIGlbCompanyCampaign>();
			var staffCampaignItem = staffCampaign.CampaignsItemsSent.AddNew();
			staffCampaignItem.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			staffCampaignItem.G8_RecipientID = staff.PK;

			Factory.Save();

			EDIDataRegistry.Instance.MyAccountSiteRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://meh.xx/myaccount");
			var url = campaignItem.GetMyAccountUserAgreementUrl("CWN", true);
			AssertStartsWith("The result should start with registry value", "https://meh.xx/myaccount/Admin/UserAgreement.aspx?data=", url);

			AssertEquals("The url should be contact only", "RECIPIENT-IS-NOT-CONTACT", staffCampaignItem.GetMyAccountUserAgreementUrl("CWN", true));

			var token = WebUtility.UrlDecode(url.Split('=')[1]);
			var tokenControl = (ITokenizedAccessControl)new TokenizedAccessControl();
			Assert(tokenControl.TryConsume(token, AccessTokenTypes.MyAccountUserAgreement, out var tokenInfo));
			AssertEquals(campaignItem.PK, tokenInfo.ParentId);
			AssertEquals(campaignItem.TableCode, tokenInfo.ParentTableCode);

			var tokenJsonObj = JObject.Parse(tokenInfo.Scope);
			AssertEquals("Campaign", tokenJsonObj["Source"].Value<string>());
			AssertEquals("VER", tokenJsonObj["Type"].Value<string>());
			AssertEquals(contact.PK.ToString(), tokenJsonObj["FromContact"].Value<string>());
			AssertEquals("CWN", tokenJsonObj["AgreementType"].Value<string>());
			Assert(tokenJsonObj["SendAgreementCopy"].Value<bool>());
		}

		public void TestGetMyAccountUserAgreementUrlWhenInvalidAgreementType()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "AA";
			contact.OC_Email = "a@test.com";
			var campaign = Factory.NewWithValidTestData<EDIGlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "AA";
			staff.GS_EmailAddress = "a@test.com";
			var staffCampaign = Factory.NewWithValidTestData<EDIGlbCompanyCampaign>();
			var staffCampaignItem = staffCampaign.CampaignsItemsSent.AddNew();
			staffCampaignItem.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			staffCampaignItem.G8_RecipientID = staff.PK;

			Factory.Save();

			EDIDataRegistry.Instance.MyAccountSiteRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://meh.xx/myaccount");
			// Assuming ZZZ doesn't exist as an agreementType
			AssertEquals("Should not generate URL if agreement type is invalid for Contact", "INVALID-AGREEMENT-TYPE", campaignItem.GetMyAccountUserAgreementUrl("ZZZ", true));
			AssertEquals("Staff error should override agreement type error", "RECIPIENT-IS-NOT-CONTACT", staffCampaignItem.GetMyAccountUserAgreementUrl("ZZZ", true));
		}

		[ExpectNoExceptions]
		public void TestSubmitAnswerSetNullException()
		{
			EDIGlbCompanyCampaignItem campaignItem = Factory.New<EDIGlbCompanyCampaignItem>();
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).SubmitAnswerSet();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var campaignItem = (EDIGlbCompanyCampaignItem)base.GetNewBusinessObjectForDeleteTest(factory);
			campaignItem.G8_G0 = factory.NewWithValidTestData<EDIGlbCompanyCampaign>().PK;
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			return campaignItem;
		}
	}
}
