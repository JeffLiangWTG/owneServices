namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	using System;
	using System.Net;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;
	using CargoWise.Application;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.TrustedMessaging.Intergration;
	using Enterprise.ZArchitecture.GUI;
	using NUnit.Framework;
	using WTG.TrustedMessaging.Models;
	using WTG.TrustedMessaging.MyAccount.Models;

	class SalesDiscoveryConductorUrlHelperTest : TestCaseWithFactory
	{
		string GetBase64HashCode(string text)
		{
			using (var sha256 = SHA256.Create())
			{
				var inputBytes = Encoding.UTF8.GetBytes(text);
				var base64String = Convert.ToBase64String(sha256.ComputeHash(inputBytes));
				return WebUtility.UrlEncode(base64String);
			}
		}

		public void TestGetSalesDiscoveryConductorUrl_Inquiry()
		{
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_LeadUniqueReference = "YaDunKnow";

			var queryString = FormattableString.Invariant($"{SalesDiscoveryConductorUrlHelper.InquiryIDQueryStringKey}={inquiry.O1_LeadUniqueReference}&");
			var hashCode = GetBase64HashCode(queryString);
			string expectedURL = FormattableString.Invariant($"https://go.goannas.go.go.goannas/en-us/Home/SalesConductor/Discovery.aspx?{queryString}{SalesDiscoveryConductorUrlHelper.HashCodeQueryStringKey}={hashCode}");
			AssertEquals(expectedURL, SalesDiscoveryConductorUrlHelper.GetSalesDiscoveryConductorUrl(inquiry).ToString());

			var linkedOrgHeader = Factory.New<OrgHeader>();
			linkedOrgHeader.OH_Code = "YDK";
			linkedOrgHeader.OH_FullName = "RoadmanShaq";
			inquiry.O1_OH_ConvertedToQualifiedLead = linkedOrgHeader.PK;

			queryString = FormattableString.Invariant($"{queryString}{SalesDiscoveryConductorUrlHelper.OrgCodeQueryStringKey}={inquiry.OrgCode}&{SalesDiscoveryConductorUrlHelper.OrgNameQueryStringKey}={linkedOrgHeader.OH_FullName}&");
			hashCode = GetBase64HashCode(queryString);
			expectedURL = FormattableString.Invariant($"https://go.goannas.go.go.goannas/en-us/Home/SalesConductor/Discovery.aspx?{queryString}{SalesDiscoveryConductorUrlHelper.HashCodeQueryStringKey}={hashCode}");
			AssertEquals(expectedURL, SalesDiscoveryConductorUrlHelper.GetSalesDiscoveryConductorUrl(inquiry).ToString());
		}

		public void TestGetSalesDiscoveryConductorUrl_InquiryDefaultsToCompanyNameWhenNoOrganizationIsLinked()
		{
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_LeadUniqueReference = "YaDunKnow";

			var queryString = FormattableString.Invariant($"{SalesDiscoveryConductorUrlHelper.InquiryIDQueryStringKey}={inquiry.O1_LeadUniqueReference}&");
			var hashCode = GetBase64HashCode(queryString);
			string expectedURL = FormattableString.Invariant($"https://go.goannas.go.go.goannas/en-us/Home/SalesConductor/Discovery.aspx?{queryString}{SalesDiscoveryConductorUrlHelper.HashCodeQueryStringKey}={hashCode}");
			AssertEquals(expectedURL, SalesDiscoveryConductorUrlHelper.GetSalesDiscoveryConductorUrl(inquiry).ToString());

			inquiry.CompanyName = "BigShaq";

			queryString = FormattableString.Invariant($"{queryString}{SalesDiscoveryConductorUrlHelper.OrgNameQueryStringKey}={inquiry.CompanyName}&");
			hashCode = GetBase64HashCode(queryString);
			expectedURL = FormattableString.Invariant($"https://go.goannas.go.go.goannas/en-us/Home/SalesConductor/Discovery.aspx?{queryString}{SalesDiscoveryConductorUrlHelper.HashCodeQueryStringKey}={hashCode}");
			AssertEquals(expectedURL, SalesDiscoveryConductorUrlHelper.GetSalesDiscoveryConductorUrl(inquiry).ToString());
		}

		public void TestGetSalesDiscoveryConductorUrl_Opportunity()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OpportunityID = "LynxEffect";

			var queryString = FormattableString.Invariant($"{SalesDiscoveryConductorUrlHelper.OpportunityIDQueryStringKey}={opportunity.P8_OpportunityID}&");
			var hashCode = GetBase64HashCode(queryString);
			string expectedUrl = FormattableString.Invariant($"https://go.goannas.go.go.goannas/en-us/Home/SalesConductor/Discovery.aspx?{queryString}{SalesDiscoveryConductorUrlHelper.HashCodeQueryStringKey}={hashCode}");
			AssertEquals(expectedUrl, SalesDiscoveryConductorUrlHelper.GetSalesDiscoveryConductorUrl(opportunity).ToString());

			var linkedInquiry = Factory.New<SalesEnquiry>();
			linkedInquiry.O1_LeadUniqueReference = "MansNotHot";
			opportunity.P8_O1_Enquiry = linkedInquiry.PK;

			queryString = FormattableString.Invariant($"{SalesDiscoveryConductorUrlHelper.InquiryIDQueryStringKey}={linkedInquiry.O1_LeadUniqueReference}&{SalesDiscoveryConductorUrlHelper.OpportunityIDQueryStringKey}={opportunity.P8_OpportunityID}&");
			hashCode = GetBase64HashCode(queryString);
			expectedUrl = FormattableString.Invariant($"https://go.goannas.go.go.goannas/en-us/Home/SalesConductor/Discovery.aspx?{queryString}{SalesDiscoveryConductorUrlHelper.HashCodeQueryStringKey}={hashCode}");
			AssertEquals(expectedUrl, SalesDiscoveryConductorUrlHelper.GetSalesDiscoveryConductorUrl(opportunity).ToString());

			var linkedOrgHeader = Factory.New<OrgHeader>();
			opportunity.P8_OH = linkedOrgHeader.PK;

			linkedOrgHeader.OH_Code = "BEB";
			linkedOrgHeader.OH_FullName = "BebSays TakeOffYour\"#%&+Jacket";
			var escapedName = "BebSays+TakeOffYour\"%23%25%26%2BJacket";

			var queryStringForHashCode = FormattableString.Invariant($"{queryString}{SalesDiscoveryConductorUrlHelper.OrgCodeQueryStringKey}={linkedOrgHeader.OH_Code}&{SalesDiscoveryConductorUrlHelper.OrgNameQueryStringKey}={linkedOrgHeader.OH_FullName}&");
			hashCode = GetBase64HashCode(queryStringForHashCode);
			var encodedQueryString = FormattableString.Invariant($"{queryString}{SalesDiscoveryConductorUrlHelper.OrgCodeQueryStringKey}={linkedOrgHeader.OH_Code}&{SalesDiscoveryConductorUrlHelper.OrgNameQueryStringKey}={escapedName}&");
			expectedUrl = FormattableString.Invariant($"https://go.goannas.go.go.goannas/en-us/Home/SalesConductor/Discovery.aspx?{encodedQueryString}{SalesDiscoveryConductorUrlHelper.HashCodeQueryStringKey}={hashCode}");
			var actualUrl = SalesDiscoveryConductorUrlHelper.GetSalesDiscoveryConductorUrl(opportunity);
			AssertEquals(expectedUrl, actualUrl.ToString());
		}

		public void TestNoSalesConductorDomainUrl()
		{
			EDIDataRegistry.Instance.SalesConductorDomainUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			var inquiry = Factory.New<SalesEnquiry>();

			AssertExceptionThrown(
				"Fail uri message",
				typeof(UriFormatException),
				"Can't create Sales Discovery Conductor link. Please verify value of the registry item 'SalesConductorDomainUrl'.",
				() => SalesDiscoveryConductorUrlHelper.GetSalesDiscoveryConductorUrl(inquiry));
		}

		[GuiTest]
		public void TestSalesMenuItem_UrlEncode()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.Header.OH_FullName = "TEST_ORG_\u4E2D\u6587";
			opportunity.Header.OH_Code = "TEST_ORG";
			opportunity.P8_OpportunityID = "O0000001";
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_OH_ConvertedToQualifiedLead = opportunity.Header.PK;
			enquiry.O1_LeadUniqueReference = "E0000001";
			Factory.Save();

			using (ObjectFactory.Substitute<ISystemUserAccountCollectionTermChecker>(new SystemUserAccountCollectionTermCheckerForTest() { TermAcknowledged = true }))
			{
				using (ObjectFactory.Substitute<IUserPortalClient>(new UserPortalClientForTest()))
				{
					SalesDiscoveryConductorUrlHelper.ClearLastUrlLaunched();
					var menuItem = SalesDiscoveryConductorUrlHelper.SalesMenuItem(opportunity).MenuItems.FindByText("&Sales Discovery");
					menuItem.PerformClick();
					AssertEquals("https://go.goannas.go.go.goannas/en-us/Home/SalesConductor/Discovery.aspx?OpportunityID=O0000001&OrgCode=TEST_ORG&OrgName=TEST_ORG_%E4%B8%AD%E6%96%87&HashCode=byaS9Ogp3EOhNr3lQ3pivnVTcIz3iwSyvq2kjJ8Cra4%3D", SalesDiscoveryConductorUrlHelper.LastUrlLaunched);

					SalesDiscoveryConductorUrlHelper.ClearLastUrlLaunched();
					menuItem = SalesDiscoveryConductorUrlHelper.SalesMenuItem(enquiry).MenuItems.FindByText("&Sales Discovery");
					menuItem.PerformClick();
					AssertEquals("https://go.goannas.go.go.goannas/en-us/Home/SalesConductor/Discovery.aspx?InquiryID=E0000001&OrgCode=TEST_ORG&OrgName=TEST_ORG_%E4%B8%AD%E6%96%87&HashCode=99ki5wutbTj1TS5dKWP1BGEUBOlPeLd5JIsya0FX3FE%3D", SalesDiscoveryConductorUrlHelper.LastUrlLaunched);

					SalesDiscoveryConductorUrlHelper.ClearLastUrlLaunched();
					menuItem = SalesDiscoveryConductorUrlHelper.SalesMenuItem(enquiry).MenuItems.FindByText("&Sales Content Play List Builder");
					menuItem.PerformClick();
					AssertEquals("https://myaccount.cargowise.com/Home/SalesConductor/SalesContentPlaylistBuilder.aspx", SalesDiscoveryConductorUrlHelper.LastUrlLaunched);
				}
			}
		}

		class SystemUserAccountCollectionTermCheckerForTest : ISystemUserAccountCollectionTermChecker
		{
			public bool TermAcknowledged { get; set; }

			public Task<bool> CheckTermAcknowledged() => Task.FromResult(TermAcknowledged);
		}

		class UserPortalClientForTest : IUserPortalClient
		{
			public Task<TrustedResponse<bool>> AcknowledgeAgreementAsync(string userAgreementType, bool shouldSendCopy)
			{
				return Task.FromResult(new TrustedResponse<bool>() { Success = true, Response = true });
			}

			public Task<TrustedResponse<bool>> SignAgreementAsync(IUserAgreementSignerDetails deets)
			{
				return Task.FromResult(new TrustedResponse<bool>() { Success = true, Response = true });
			}

			public Task<TrustedResponse<AutoLoginResponse>> ERequestPortalAutoLoginAsync(string landingPageId, string incidentNumber, string module, string subModule, string referenceId, string licenceCode)
				=> Task.FromResult(new TrustedResponse<AutoLoginResponse>() { Success = true, Response = new AutoLoginResponse(new Uri($"http://www.cw1.com/{landingPageId}/{incidentNumber}/{module}/{subModule}/{referenceId}/{licenceCode}")) });

			public Uri GetMyAccountAutoLoginUrl(Uri returnUrl)
			{
				return new Uri($"http://www.cw1.com/MyAccountAutoLoginUrl.aspx?return={WebUtility.UrlEncode(returnUrl.ToString())}");
			}

			public Task<TrustedResponse<OAuthLoginResponse>> OAuthAutoLoginAsync(Uri returnUrl)
			{
				return Task.FromResult(new TrustedResponse<OAuthLoginResponse>()
				{
					Success = true,
					Response = new OAuthLoginResponse() { RedirectUrl = new Uri(returnUrl + "?token=123"), Token = "123" }
				});
			}

			public Task<TrustedResponse<UserAgreementResponseData>> GetUserAgreementAsync(string userAgreementType)
				=> Task.FromResult(new TrustedResponse<UserAgreementResponseData>()
				{
					Success = true,
					Response = new UserAgreementResponseData()
					{
						Required = false,
						Title = "",
						Content = "",
						Level = "",
						VersionNumber = 0,
					}
				});

			public Task<TrustedResponse<EnterpriseAgreementResponseData>> GetEnterpriseAgreementUrlAsync(string userAgreementType)
			=> Task.FromResult(new TrustedResponse<EnterpriseAgreementResponseData>()
			{
				Success = true,
				Response = new EnterpriseAgreementResponseData()
				{
					Required = false,
					Url = string.Empty,
				}
			});

			public Task<TrustedResponse<AutoLoginResponse>> MyAccountAutoLoginAsync(Uri returnUrl)
			{
				var rsp = GetMyAccountAutoLoginUrl(returnUrl);
				return Task.FromResult(new TrustedResponse<AutoLoginResponse>() { Success = true, Response = new AutoLoginResponse() { AutoLoginUrl = rsp } });
			}

			public Task<TrustedResponse<GetAcceptancesResponse>> GetAcceptancesAsync(string userAgreementType)
			{
				return Task.FromResult(new TrustedResponse<GetAcceptancesResponse>() { Success = true, Response = new GetAcceptancesResponse() });
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			EDIDataRegistry.Instance.SalesConductorDomainUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://go.goannas.go.go.goannas/en-us/Home/SalesConductor/Discovery.aspx");
		}

		protected override void TearDown()
		{
			base.TearDown();
			SalesDiscoveryConductorUrlHelper.ClearLastUrlLaunched();
		}
	}
}
