using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class ALandingPageProcessorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCreateWebEnquiry()
		{
			var page = GetPageForTest();
			FillPageRequest(page);
			page.DoPageLoad();
			var enquiry = Factory.LoadTop1<EDIWebSalesInquiry>(new ZQuery());
			AssertNotNull(enquiry);
		}

		public void TestLeadSource()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();
			collection.Add("WEB", (NoResString)"WEB", true);
			collection.Add("XXX", (NoResString)"XXX", true);
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var page = GetPageForTest();
			FillPageRequest(page);
			page.Request.QueryString.Add("LeadSource", "XXX");
			page.DoPageLoad();
			var enquiry = Factory.LoadTop1<EDIWebSalesInquiry>(new ZQuery());
			AssertEquals("LeadSource should be preserved", "XXX", enquiry.O1_LeadSource);
		}

		public void TestShouldSetupSession()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();
			collection.Add("WEB", (NoResString)"WEB", true);
			collection.Add("XXX", (NoResString)"XXX", true);
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var page = GetPageForTest();
			FillPageRequest(page);
			Env.ClearUserContext();
			page.Request.QueryString.Add("LeadSource", "XXX");
			page.DoPageLoad();
			var enquiry = Factory.LoadTop1<EDIWebSalesInquiry>(new ZQuery());
			AssertEquals("Should be no errors", false, enquiry.HasErrors);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
		}

		public void TestLeadSource_DefaultValue()
		{
			var page = GetPageForTest();
			FillPageRequest(page);
			page.DoPageLoad();
			var enquiry = Factory.LoadTop1<EDIWebSalesInquiry>(new ZQuery());
			AssertEquals($"LeadSource should default to {Constants.Sales.LeadType.Website} when not provided", Constants.Sales.LeadType.Website, enquiry.O1_LeadSource);
		}

		public void TestLeadSource_InvalidValue()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();
			collection.Add("WEB", (NoResString)"WEB", true);
			collection.Add("XXX", (NoResString)"XXX", true);
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var page = GetPageForTest();
			FillPageRequest(page);
			page.Request.QueryString.Add("LeadSource", "YYY");
			page.DoPageLoad();
			var enquiry = Factory.LoadTop1<EDIWebSalesInquiry>(new ZQuery());
			AssertNull("No enquiry should be created when Lead Source is invalid", enquiry);
		}

		void FillPageRequest(LandingPageProcessor page)
		{
			page.Request.QueryString.Add("CompanyName", "Company AAA");
			page.Request.QueryString.Add("FullName", "Contact One");
			page.Request.QueryString.Add("Email", "contact.one@company.aaa.com");
			page.Request.QueryString.Add("WorkPhone", "0287479999");
			page.Request.QueryString.Add("City", "Sydney");
			page.Request.QueryString.Add("State", "NSW");
			page.Request.QueryString.Add("WorkPhoneNationalCode", "1123812");
			page.Request.QueryString.Add("WorkPhoneExtension", "99997777");
			page.Request.QueryString.Add("JobTitle", new string('t', 51));
			page.Request.QueryString.Add("JobRole", new string('r', 51));
			page.Request.QueryString.Add("CompanySize", new string('s', 51));
			page.Request.QueryString.Add("TypeOfBusiness", new string('b', 301));
			page.Request.QueryString.Add("ReasonForRequestingAccess", new string('a', 301));
			page.Request.QueryString.Add("AdditionalInfo", new string('i', 5001));
		}

		protected override void SetUp()
		{
			base.SetUp();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
		}

		LandingPageProcessorForTest GetPageForTest()
		{
			var page = new LandingPageProcessorForTest();
			MethodInfo method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		class LandingPageProcessorForTest : LandingPageProcessor
		{
			public void DoPageLoad()
			{
				base.Page_Load(null, EventArgs.Empty);
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			class GlobalForTest : Global
			{
				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}
			}
		}
	}
}
