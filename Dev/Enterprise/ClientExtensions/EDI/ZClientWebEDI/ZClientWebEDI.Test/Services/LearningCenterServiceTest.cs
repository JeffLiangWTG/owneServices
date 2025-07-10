using System;
using System.Net;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI.Services.Testing
{
	sealed class LearningCenterServiceTest : TestCaseWithFactory
	{
		public void TestGetLearningCenterExamUrl()
		{
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://learning.org/");
			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			campaign.G0_CampaignID = "CRT000001";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.org";
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "sam@test.org";
			Factory.Save();
			SecureQueryString queryString = new SecureQueryString();
			queryString[GlbCompanyCampaignSchema.Constants.PK] = campaign.PK.ToString();
			queryString[RefCountrySchema.Constants.Prefix] = "AU";
			queryString[VoteExamSurveyUrlHelper.RecipientIDQueryStringKey] = contact.PK.ToString();
			queryString[VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey] = OrgContactSchema.Constants.Prefix;
			var service = new LearningCenterServiceForTest();
			var url = service.GetLearningCenterExamUrl("CRT000001", contact.PK.ToGuid(), "AU");
			AssertEquals(string.Empty, url);
			url = service.GetLearningCenterExamUrl("CRT000002", contact.PK.ToGuid(), "AU");
			AssertEquals("", url);
			url = service.GetLearningCenterExamUrl("CRT000001", Factory.New<OrgContact>().PK.ToGuid(), "AU");
			AssertEquals("", url);
		}

		public void TestGetLearningCenterExamUrl_WithJobSkill()
		{
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://learning.org/");
			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			campaign.G0_CampaignID = "CRT000001";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.org";
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "sam@test.org";
			Factory.Save();
			SecureQueryString queryString = new SecureQueryString();
			queryString[GlbCompanyCampaignSchema.Constants.PK] = campaign.PK.ToString();
			queryString[RefCountrySchema.Constants.Prefix] = "AU";
			queryString[VoteExamSurveyUrlHelper.RecipientIDQueryStringKey] = contact.PK.ToString();
			queryString[VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey] = OrgContactSchema.Constants.Prefix;
			queryString[VoteExamSurveyUrlHelper.JobSkillCodeStringKey] = "ZZZ";
			string expectedUrl = string.Empty;
			var service = new LearningCenterServiceForTest();
			var url = service.GetLearningCenterExamUrl("CRT000001", contact.PK.ToGuid(), "AU", jobSkillCode: "ZZZ");
			AssertEquals(expectedUrl, url);
			url = service.GetLearningCenterExamUrl("CRT000001", contact.PK.ToGuid(), "AU");
			AssertEquals(expectedUrl, url);
		}

		public void TestGetLearningCenterExamUrl_WithLanguage()
		{
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://learning.org/");
			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			campaign.G0_CampaignID = "CRT000001";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.org";
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "sam@test.org";
			Factory.Save();
			var queryString = new SecureQueryString();
			queryString[GlbCompanyCampaignSchema.Constants.PK] = campaign.PK.ToString();
			queryString[RefCountrySchema.Constants.Prefix] = "AU";
			queryString[VoteExamSurveyUrlHelper.RecipientIDQueryStringKey] = contact.PK.ToString();
			queryString[VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey] = OrgContactSchema.Constants.Prefix;
			queryString[VoteExamSurveyUrlHelper.LanguageStringKey] = "GRM";
			string expectedUrl = string.Format("http://learning.org/login.aspx?{0}={1}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			var service = new LearningCenterServiceForTest();
			var url = service.GetLearningCenterExamUrl("CRT000001", contact.PK.ToGuid(), "AU", jobSkillCode: "", language: "GRM");
			AssertNotEquals(expectedUrl, url);
			url = service.GetLearningCenterExamUrl("CRT000001", contact.PK.ToGuid(), "AU", jobSkillCode: "", language: "");
			AssertNotEquals(expectedUrl, url);
			var queryString2 = new SecureQueryString();
			queryString2[GlbCompanyCampaignSchema.Constants.PK] = campaign.PK.ToString();
			queryString2[RefCountrySchema.Constants.Prefix] = "AU";
			queryString2[VoteExamSurveyUrlHelper.RecipientIDQueryStringKey] = contact.PK.ToString();
			queryString2[VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey] = OrgContactSchema.Constants.Prefix;
			queryString2[VoteExamSurveyUrlHelper.JobSkillCodeStringKey] = "ZZZ";
			queryString2[VoteExamSurveyUrlHelper.LanguageStringKey] = "GRM";
			expectedUrl = string.Format("http://learning.org/login.aspx?{0}={1}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString2.ToString()));
			url = service.GetLearningCenterExamUrl("CRT000001", contact.PK.ToGuid(), "AU", jobSkillCode: "ZZZ", language: "GRM");
			AssertNotEquals(expectedUrl, url); //Should not generate a jobSkillCode string if there is no job skill with that code
			url = service.GetLearningCenterExamUrl("CRT000001", contact.PK.ToGuid(), "AU", jobSkillCode: "XXX", language: "GRM");
			AssertNotEquals(expectedUrl, url);
			url = service.GetLearningCenterExamUrl("CRT000001", contact.PK.ToGuid(), "AU", language: "GRM");
			AssertNotEquals(expectedUrl, url);
		}

		public void TestGetLearningCenterExamResult()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Email = "steve@test.org";
			HRJobApplicant applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "steve@test.org";
			LearningCentreCampaign campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			campaign.G0_CampaignID = "CRT000001";
			campaign.G0_DefaultAnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			LearningCentreQuestion question1 = campaign.Questions.AddNew();
			question1.HY_ExamCorrectAnswer = "1";
			LearningCentreQuestion question2 = campaign.Questions.AddNew();
			question2.HY_ExamCorrectAnswer = "1";
			LearningCentreQuestion question3 = campaign.Questions.AddNew();
			question3.HY_ExamCorrectAnswer = "1";
			LearningCentreQuestion question4 = campaign.Questions.AddNew();
			question4.HY_ExamCorrectAnswer = "1";
			LearningCentreQuestion question5 = campaign.Questions.AddNew();
			question5.HY_ExamCorrectAnswer = "1";
			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = applicant.PK;
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			campaignItem.PersistedAnswers.LoadOrCreateNew(question1).HZ_Answer = "1";
			campaignItem.PersistedAnswers.LoadOrCreateNew(question2).HZ_Answer = "1";
			campaignItem.PersistedAnswers.LoadOrCreateNew(question3).HZ_Answer = "2";
			campaignItem.PersistedAnswers.LoadOrCreateNew(question4).HZ_Answer = "";
			campaignItem.PersistedAnswers.LoadOrCreateNew(question5).HZ_Answer = "";
			Factory.Save();
			string expectedResult = campaignItem.ExamScoreAsString;
			var service = new LearningCenterServiceForTest();
			string result = service.GetLearningCenterExamResult("CRT000001", contact.PK.ToGuid());
			AssertEquals(expectedResult, result);
			result = service.GetLearningCenterExamResult("CRT000002", contact.PK.ToGuid());
			AssertEquals("", result);
			result = service.GetLearningCenterExamResult("CRT000001", Factory.New<OrgContact>().PK.ToGuid());
			AssertEquals("", result);
		}

		public void TestGetNextScheduledServiceTaskDateTime()
		{
			var service = new LearningCenterServiceForTest();
			var systemUpgradeServiceTask = new BusinessObjectFactory().New<StmScheduleTask>();
			systemUpgradeServiceTask.S5_IsActive = false;
			var nextScheduledSystemUpgradeDateTime = service.GetNextScheduledServiceTaskDateTime(systemUpgradeServiceTask);
			AssertEquals("Service Task is not active", ZDateTime.Empty, nextScheduledSystemUpgradeDateTime);
			systemUpgradeServiceTask.S5_IsActive = true;
			nextScheduledSystemUpgradeDateTime = service.GetNextScheduledServiceTaskDateTime(systemUpgradeServiceTask);
			AssertEquals("Service Task is active with S5_NextScheduledPrintRunTimeUtc not set", ZDateTime.Empty, nextScheduledSystemUpgradeDateTime);
			systemUpgradeServiceTask.S5_IsActive = true;
			systemUpgradeServiceTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
			nextScheduledSystemUpgradeDateTime = service.GetNextScheduledServiceTaskDateTime(systemUpgradeServiceTask);
			AssertEquals("Service Task is active with S5_NextScheduledPrintRunTimeUtc set", systemUpgradeServiceTask.S5_NextScheduledPrintRunTimeUtc, nextScheduledSystemUpgradeDateTime);
		}

		public void TestShouldShowPersonalEmailRequestNotification()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "jeff1";
			contact1.OC_Email = "jeff1@test.org";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "jeff2";
			contact2.OC_Email = "jeff2@test.org";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "jeff3";
			contact3.OC_Email = "jeff3@test.org";
			var contact4 = org.Contacts.AddNew();
			contact4.OC_ContactName = "jeff4";
			contact4.OC_Email = "jeff4@test.org";
			var applicant1 = Factory.New<HRJobApplicant>();
			applicant1.HA_EmailAddress = "jeff1@test.org";
			applicant1.HA_FullName = "name1";
			var applicant2 = Factory.New<HRJobApplicant>();
			applicant2.HA_EmailAddress = "jeff2@test.org";
			applicant2.HA_FullName = "name2";
			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			campaign.G0_CampaignID = "CRT000001";
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientID = applicant1.PK;
			campaignItem1.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			var completedResults = new ExamAttemptCollection(campaignItem1).AddNew();
			completedResults.EXA_TestCommencedUtc = ZDateTime.BrettsBirthday.AddMinutes(-20);
			completedResults.EXA_TestCompletedUtc = ZDateTime.BrettsBirthday.AddMinutes(-10);
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientID = applicant2.PK;
			campaignItem2.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			completedResults = new ExamAttemptCollection(campaignItem2).AddNew();
			completedResults.EXA_TestCommencedUtc = ZDateTime.BrettsBirthday.AddMinutes(-20);
			completedResults.EXA_TestCompletedUtc = ZDateTime.BrettsBirthday.AddMinutes(-10);
			Factory.Save();
			contact1.Person.PER_EmailAddress = "jeff@gmail.com";
			contact3.Person.PER_EmailAddress = "jeff@gmail.com";
			Factory.Save();
			var service = new LearningCenterServiceForTest();
			Assert("contact1 should return false as with personal email address entered", !service.ShouldShowPersonalEmailRequestNotification(contact1.PK.ToGuid()));
			Assert("contact2 should return true as doesn't enter personal email address", service.ShouldShowPersonalEmailRequestNotification(contact2.PK.ToGuid()));
			Assert("contact3 should return false as doesn't complete any exams", !service.ShouldShowPersonalEmailRequestNotification(contact3.PK.ToGuid()));
			Assert("contact4 should return false as doesn't complete any exams and without personal email address", !service.ShouldShowPersonalEmailRequestNotification(contact4.PK.ToGuid()));
		}

		#region Implementation
		class LearningCenterServiceForTest : LearningCenterService
		{
			protected override void HandleError(string errorMessage, string errorDetail = "")
			{
				LastErrorMessage = errorMessage;
				LastErrorDetail = errorDetail;
			}

			public string LastErrorMessage { get; private set; }

			public string LastErrorDetail { get; private set; }

			protected override void ValidateRequestIpAddress()
			{
			}
		}
		#endregion
	}
}
