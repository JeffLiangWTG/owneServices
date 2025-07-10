using System;
using System.Web;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI.Services.Testing
{
	class SubscriptionPreferenceServiceTest : TestCaseWithFactory
	{
		public void TestGetSubscriptionPreferenceUrl()
		{
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://learning.org/");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.org";
			Factory.Save();

			var service = new SubscriptionPreferenceServiceForTest();
			var url = service.GetSubscriptionPreferenceUrl(contact.PK.ToGuid(), "ABC");

			var uri = new Uri(url);
			var encryptedData = HttpUtility.ParseQueryString(uri.Query).Get(UnsubscribeUrlHelper.VoteExamSurveyQueryStringKey);
			var queryStringDecode = new SecureQueryString(encryptedData);
			var pkValue = queryStringDecode[OrgContactSchema.Constants.PK];
			var listCode = queryStringDecode[UnsubscribeUrlHelper.PublishedListCodeKey];
			AssertEquals("/subscribepreference.aspx", uri.AbsolutePath);
			AssertEquals("learning.org", uri.Host);
			AssertEquals(contact.PK.ToString(), pkValue);
			AssertEquals("ABC", listCode);
		}

		public void TestGetSubscriptionPreferenceUrl_NullEnvironment()
		{
			var user = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, User.WebUserName);
			var context = new UserContext(user, DataRegistry.Instance.WebBranch, DataRegistry.Instance.WebDepartment);
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(context.Company.PK, Guid.Empty, Guid.Empty, "http://learning.org/");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.org";
			Factory.Save();

			EnvProvider providerToSave = Env.GetCurrentProvider();
			var nullEnvironment = new NullEnvProvider();
			try
			{
				nullEnvironment.Enable();
				AssertEquals("Precondition: Env.CurrentUser", null, Env.CurrentUser);
				var service = new SubscriptionPreferenceServiceForTest();
				var url = service.GetSubscriptionPreferenceUrl(contact.PK.ToGuid(), "ABC");

				var uri = new Uri(url);
				var encryptedData = HttpUtility.ParseQueryString(uri.Query).Get(UnsubscribeUrlHelper.VoteExamSurveyQueryStringKey);
				var queryStringDecode = new SecureQueryString(encryptedData);
				var pkValue = queryStringDecode[OrgContactSchema.Constants.PK];
				var listCode = queryStringDecode[UnsubscribeUrlHelper.PublishedListCodeKey];
				AssertEquals("/subscribepreference.aspx", uri.AbsolutePath);
				AssertEquals("learning.org", uri.Host);
				AssertEquals(contact.PK.ToString(), pkValue);
				AssertEquals("ABC", listCode);
			}
			finally
			{
				providerToSave.Enable();
				nullEnvironment.Dispose();
			}
		}

		class SubscriptionPreferenceServiceForTest : SubscriptionPreferenceService
		{
			protected override void ValidateRequestIpAddress()
			{
			}
		}
	}
}
