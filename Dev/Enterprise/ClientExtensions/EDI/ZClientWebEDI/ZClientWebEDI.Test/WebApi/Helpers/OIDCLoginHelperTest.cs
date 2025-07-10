using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using WTG.OAuth2.Token.TestFramework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class OIDCLoginHelperTest : TestCaseWithFactory
	{
		public void TestIsOIDCReady()
		{
			var domains = Array.Empty<string>();
			var organisations = Array.Empty<Guid>();
			EDIDataRegistry.Instance.RedirectedEmailDomains.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, domains);
			EDIDataRegistry.Instance.RedirectedOrganisations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, organisations);
			Assert("OIDC should not be ready", !OIDCLoginHelper.IsOIDCReady());

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			SetupDomainAndOrganisationRegistry(header);
			Assert("OIDC should be ready", OIDCLoginHelper.IsOIDCReady());
		}

		public void TestIsMatchDomain()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			SetupDomainAndOrganisationRegistry(header);

			Assert("userCredential should not match domain", !OIDCLoginHelper.IsMatchDomain("pyn@test1.com"));

			Assert("userCredential should match domain", OIDCLoginHelper.IsMatchDomain("pyn@test.com"));
		}

		public void TestIsMatchOrganisation()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			SetupDomainAndOrganisationRegistry(header);

			Assert("userCredential should not match organisation", !OIDCLoginHelper.IsMatchOrganisation(Factory, "AAAAAAAAA"));
			Assert("userCredential should match organisation", OIDCLoginHelper.IsMatchOrganisation(Factory, header.OH_Code));
		}

		public void TestGetCodeUri()
		{
			var logger = new NLogWrapperForTest(GetType());
			using (EDIDataRegistry.Instance.MyAccountSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://dummyy-account.local"))
			using (var server = SetupMockServer())
			{
				var oidcConfig = new OIDCConfig()
				{
					IsOIDCEnabled = false,
					OIDCServerType = OIDCServerTypes.Azure,
					AuthorityURL = $"https://{MockIdentityServerBase.HostName}:{server.Port}",
					ClientIdentifier = "testid"
				};
				oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "unique_name", Identifier = "GlbStaff.GS_LoginName" });
				oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
				oidcConfig.IsVerified = true;

				var returnUrl = "https://DummySite.com/main?path=product&target=cargowise&type=update%20note";
				var expectedUri = $"{oidcConfig.AuthorityURL}/connect/authorize?client_id=&scope=openid+&response_type=code&redirect_uri=https%3a%2f%2fdummyy-account.local%2fLogin%2fOIDCLoginComplete.aspx&domain_hint=Azure&response_mode=query&a=a&stats={WebUtility.UrlEncode(returnUrl)}";
				using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
				{
					var codeUri = OIDCLoginHelper.GetOIDCCodeUrl(new Dictionary<string, string> {
						{ "a", "a" },
						{ "stats", returnUrl }
					}, logger, CancellationToken.None);
					Assert(expectedUri.Equals(codeUri.AbsoluteUri, StringComparison.OrdinalIgnoreCase));
				}
			}
		}

		public void TestShouldRedirectToIDP()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			SetupDomainAndOrganisationRegistry(header);

			Assert("The method should return false if the email domain is not OIDC enabled and org code is not provided", !OIDCLoginHelper.ShouldRedirectToIDP(Factory, "123@123.com"));

			Assert("The method should return true if the email domain is not OIDC enabled and org code is OIDC enabled", OIDCLoginHelper.ShouldRedirectToIDP(Factory, "123@123.com", header.OH_Code));

			Assert("The method should return true if the email domain is OIDC enabled and org code is not provided", OIDCLoginHelper.ShouldRedirectToIDP(Factory, "123@test.com"));

			Assert("The method should return true if the email domain is OIDC enabled and org code is OIDC enabled", OIDCLoginHelper.ShouldRedirectToIDP(Factory, "123@test.com", header.OH_Code));

			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			header2.OH_Code = "DummyOrg";
			Factory.Save();

			AssertNotEquals(header.OH_Code, header2.OH_Code);
			Assert("The method should return false if the email domain is OIDC enabled and org code is not OIDC enabled", !OIDCLoginHelper.ShouldRedirectToIDP(Factory, "123@test.com", header2.OH_Code));
		}

		public void TestGetTokenUri()
		{
			var logger = new NLogWrapperForTest(GetType());
			using (var server = SetupMockServer())
			{
				var oidcConfig = new OIDCConfig()
				{
					IsOIDCEnabled = false,
					OIDCServerType = OIDCServerTypes.Azure,
					AuthorityURL = $"https://{MockIdentityServerBase.HostName}:{server.Port}",
					ClientIdentifier = "testid"
				};
				oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "unique_name", Identifier = "GlbStaff.GS_LoginName" });
				oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
				oidcConfig.IsVerified = true;

				var expectedUri = $"{oidcConfig.AuthorityURL}/connect/token?code=DummyCode&grant_type=authorization_code&a=a";
				using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
				{
					var codeUri = OIDCLoginHelper.GetOIDCTokenUri("DummyCode", new Dictionary<string, string> { { "a", "a" } }, logger, CancellationToken.None);
					AssertEquals(expectedUri, codeUri);
				}
			}
		}

		public void TestGenerateCodeChallenge()
		{
			var verifier = "0ZyO0YgYpr7wFMM3wkHh3wTGasG5kE0tCWV4eOIX1WQ";
			var expectedChallenge = "8kBAZ7iOizVThk6D_bdNgVjgjGiX_plxOrp1aKGKxf8";

			AssertEquals(expectedChallenge, OIDCLoginHelper.GenerateCodeChallenge(verifier));
		}

		public void TestGetAllContactsViaUserEmailAndPersonPK()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "orgCode";

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_Code = "orgCode2";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = nameof(contact1);
			contact1.OC_Email = "1@abc.com";
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;

			Factory.Save();
			AssertNotNull(contact1.Person);

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = nameof(contact2);
			contact2.OC_Email = "1@abc.com";
			contact2.OC_IsActive = false;
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_PER = contact1.OC_PER;

			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = nameof(contact3);
			contact3.OC_Email = "1@abc.com";
			contact3.OC_IsActive = true;
			contact3.OC_WebAccessEnabled = false;
			contact3.OC_PER = contact1.OC_PER;

			var contact4 = org2.Contacts.AddNew();
			contact4.OC_ContactName = nameof(contact4);
			contact4.OC_Email = "1@abc.com";
			contact4.OC_IsActive = true;
			contact4.OC_WebAccessEnabled = true;
			contact4.OC_PER = contact1.OC_PER;

			Factory.Save();

			AssertEquals(contact1.OC_PER, contact2.OC_PER);
			AssertEquals(contact1.OC_PER, contact3.OC_PER);
			AssertEquals(contact1.OC_PER, contact4.OC_PER);

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var query1Result = OIDCLoginHelper.GetAllContactsViaUserEmailAndPersonPK(factory1, "1@abc.com", contact1.OC_PER);
			AssertEquals(2, query1Result.Length);
			Assert(query1Result.Any(x => x.PK == contact1.PK));
			Assert(query1Result.Any(x => x.PK == contact4.PK));

			var query2Result = OIDCLoginHelper.GetAllContactsViaUserEmailAndPersonPK(factory2, "1@abc.com", contact1.OC_PER, org.OH_Code);
			AssertEquals(1, query2Result.Length);
			Assert(query2Result.First().PK == contact1.PK);
		}

		public static MockOpenIDIdentityServer SetupMockServer()
		{
			var mockServer = new MockOpenIDIdentityServer
			{
				ClientIdentifier = "MyAccount",
				Claims = new List<Claim>
				{
					new Claim("claim1", "claim1Value"),
				},
			};

			return mockServer;
		}

		void SetupDomainAndOrganisationRegistry(OrgHeader header)
		{
			var domains = new string[] { "@test.com" };
			var organisations = new Guid[] { header.PK.ToGuid() };
			EDIDataRegistry.Instance.RedirectedEmailDomains.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, domains);
			EDIDataRegistry.Instance.RedirectedOrganisations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, organisations);
		}
	}
}
