using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.WiseTechAcademy;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class WiseTechAcademyAutoLoginTest : TestCaseWithFactory
	{
		[HttpContextEnabledTest]
		public void TestLogin()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(ClientOverride.Instance))
			{
				using (EDIDataRegistry.Instance.MyAccountSSOJWTTokenExchangePrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null))
				using (EDIDataRegistry.Instance.EnableAcademyJWTAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (EDIDataRegistry.Instance.WiseTechAcademyAutoLoginUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://wisetechacademy.com"))
				{
					var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
					var org = licence.Database.LicEnterprise.Organisation;
					org.OH_Code = "SCWAAASYD";
					var contact = org.Contacts.AddNew();
					contact.OC_ContactName = "sam";
					contact.OC_Email = "sam@test.com";
					contact.OC_WebAccessEnabled = true;
					contact.SetHashedPassword("1234");
					Factory.Save();
					contact.Person.PER_EmailAddress = "sam@gmail.com";
					var otherPerson1 = Factory.NewWithValidTestData<GlbPerson>();
					var otherPerson2 = Factory.NewWithValidTestData<GlbPerson>();
					var mergedPersons1 = Factory.New<GlbMergedPerson>();
					mergedPersons1.GMP_PER_Person = contact.Person.PK;
					mergedPersons1.GMP_MergedPerson = otherPerson1.PK;
					var mergedPersons2 = Factory.New<GlbMergedPerson>();
					mergedPersons2.GMP_PER_Person = contact.Person.PK;
					mergedPersons2.GMP_MergedPerson = otherPerson2.PK;
					AssertEquals("Precondition", true, contact.Person.PersonIDs.Contains(otherPerson1.PK));
					AssertEquals("Precondition", true, contact.Person.PersonIDs.Contains(otherPerson2.PK));
					Factory.Save();
					var page = GetPageForTest();
					page.SiteUser.LoginForTest(org.OH_Code, contact.Email, "1234");
					page.Request.QueryString.Add("path", "product");
					page.Request.QueryString.Add("target", "cargowise");
					page.Request.QueryString.Add("quickstartid", "AAA");
					page.Request.QueryString.Add("searchText", "COR1301");
					page.Request.QueryString.Add("courseid", "285");
					page.Request.QueryString.Add("type", "technical%20advisory%20note");
					page.Request.QueryString.Add("programid", "83318d42-d497-443e-8211-943b8d200000");
					page.DoPageLoad();
					var accessToken = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK));
					AssertNotNull(accessToken);
					AssertEquals("WTA", accessToken.SAT_Type);
					AssertEquals(1, accessToken.SAT_RemainingUseCount);
					AssertEquals(200, page.Response.StatusCode);
					var hfs = page.LoginForm_Exposed.Controls.OfType<HiddenField>().ToArray();
					void AssertHiddenField(string id, string valueExpected)
					{
						AssertEquals(id, valueExpected, hfs.Single(x => x.ID == id).Value);
					}

					CombineAssertions(() =>
					{
						AssertHiddenField("TenantId", "101");
						AssertHiddenField("Product", "WTA");
						AssertHiddenField("OrgPk", org.PK.ToString());
						AssertHiddenField("OrgName", "ENTCOM Company");
						AssertHiddenField("ContactWorkingAddressOrgName", "ENTCOM Company");
						AssertHiddenField("ContactWorkingAddressCountry", "AU");
						AssertHiddenField("ContactLocation", "AUSYD");
						AssertHiddenField("LicenceDatabaseMasterOrgCode", "SCWAAASYD");
						AssertHiddenField("LicenceDatabaseMasterOrgName", "ENTCOM Company");
						AssertHiddenField("LicenceDatabaseBillingOrgCode", "SCWAAASYD");
						AssertHiddenField("LicenceDatabaseBillingOrgName", "ENTCOM Company");
						AssertHiddenField("ContactPk", contact.PK.ToString());
						AssertHiddenField("ContactName", "sam");
						AssertHiddenField("ContactEmail", "sam@test.com");
						AssertHiddenField("PersonalEmail", "sam@gmail.com");
						AssertHiddenField("Path", "product");
						AssertHiddenField("Target", "cargowise");
						AssertHiddenField("QuickstartId", "AAA");
						AssertHiddenField("SearchText", "COR1301");
						AssertHiddenField("CourseId", "285");
						AssertHiddenField("Type", "technical%20advisory%20note");
						AssertHiddenField("ProgramId", "83318d42-d497-443e-8211-943b8d200000");
						AssertHiddenField("AutoLoginOriginProduct", string.Empty);
						AssertHiddenField("AutoLoginOriginProductVersion", string.Empty);
						AssertContains(otherPerson1.PK.ToString(), hfs.Single(x => x.ID == "PersonIDs").Value);
						AssertContains(otherPerson2.PK.ToString(), hfs.Single(x => x.ID == "PersonIDs").Value);
						AssertContains(contact.Person.PK.ToString(), hfs.Single(x => x.ID == "PersonIDs").Value);
						var userAccount = new BusinessObjectFactory().Load<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_Email, "sam@test.com")).Single();
						AssertEquals(contact.PK, userAccount.EUA_OC_WebAccessContact);
						AssertHiddenField("UserId", userAccount.PK.ToString());
					});
				}
			}
		}

		[HttpContextEnabledTest]
		public void TestLogin_WithAutoLoginOriginProductVersion()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(ClientOverride.Instance))
			{
				using (EDIDataRegistry.Instance.MyAccountSSOJWTTokenExchangePrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null))
				using (EDIDataRegistry.Instance.EnableAcademyJWTAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (EDIDataRegistry.Instance.WiseTechAcademyAutoLoginUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://wisetechacademy.com"))
				{
					var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
					var org = licence.Database.LicEnterprise.Organisation;
					licence.Database.LD_Product = "WTA";
					var build1611101 = CreateNewReleaseBuild(16, 1, 1, 101, false, "GP1");
					licence.Database.LD_HL_CurrentRunningVersion = build1611101.PK;
					org.OH_Code = "SCWAAASYD";
					var contact = org.Contacts.AddNew();
					contact.OC_ContactName = "sam";
					contact.OC_Email = "sam@test.com";
					contact.OC_WebAccessEnabled = true;
					contact.SetHashedPassword("1234");
					Factory.Save();

					var page = GetPageForTest();
					HttpContext.Current.Session["DatabaseNumber"] = licence.Database.LD_DatabaseNumber;
					page.SiteUser.LoginForTest(org.OH_Code, contact.Email, "1234");
					page.DoPageLoad();

					var hfs = page.LoginForm_Exposed.Controls.OfType<HiddenField>().ToArray();
					void AssertHiddenField(string id, string valueExpected)
					{
						AssertEquals(id, valueExpected, hfs.Single(x => x.ID == id).Value);
					}

					AssertHiddenField("AutoLoginOriginProduct", "CW1");
					AssertHiddenField("AutoLoginOriginProductVersion", "16.1.1.101");
				}
			}
		}

		[HttpContextEnabledTest]
		public void TestLogin_WithAutoLoginOriginProductVersion_WhenDatabaseNumberIsNull()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(ClientOverride.Instance))
			{
				using (EDIDataRegistry.Instance.MyAccountSSOJWTTokenExchangePrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null))
				using (EDIDataRegistry.Instance.EnableAcademyJWTAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (EDIDataRegistry.Instance.WiseTechAcademyAutoLoginUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://wisetechacademy.com"))
				{
					var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
					var org = licence.Database.LicEnterprise.Organisation;
					licence.Database.LD_Product = "WTA";
					var build1611101 = CreateNewReleaseBuild(16, 1, 1, 101, false, "GP1");
					licence.Database.LD_HL_CurrentRunningVersion = build1611101.PK;
					org.OH_Code = "SCWAAASYD";
					var contact = org.Contacts.AddNew();
					contact.OC_ContactName = "sam";
					contact.OC_Email = "sam@test.com";
					contact.OC_WebAccessEnabled = true;
					contact.SetHashedPassword("1234");
					Factory.Save();

					var page = GetPageForTest();
					HttpContext.Current.Session["DatabaseNumber"] = null;
					page.SiteUser.LoginForTest(org.OH_Code, contact.Email, "1234");
					page.DoPageLoad();

					var hfs = page.LoginForm_Exposed.Controls.OfType<HiddenField>().ToArray();
					void AssertHiddenField(string id, string valueExpected)
					{
						AssertEquals(id, valueExpected, hfs.Single(x => x.ID == id).Value);
					}

					AssertHiddenField("AutoLoginOriginProduct", string.Empty);
					AssertHiddenField("AutoLoginOriginProductVersion", string.Empty);
				}
			}
		}

		[HttpContextEnabledTest]
		public void TestLogin_WithAutoLoginOriginProductVersion_WhenDatabaseNumberIsWrong()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(ClientOverride.Instance))
			{
				using (EDIDataRegistry.Instance.MyAccountSSOJWTTokenExchangePrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null))
				using (EDIDataRegistry.Instance.EnableAcademyJWTAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (EDIDataRegistry.Instance.WiseTechAcademyAutoLoginUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://wisetechacademy.com"))
				{
					var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
					var org = licence.Database.LicEnterprise.Organisation;
					licence.Database.LD_Product = "WTA";
					var build1611101 = CreateNewReleaseBuild(16, 1, 1, 101, false, "GP1");
					licence.Database.LD_HL_CurrentRunningVersion = build1611101.PK;
					org.OH_Code = "SCWAAASYD";
					var contact = org.Contacts.AddNew();
					contact.OC_ContactName = "sam";
					contact.OC_Email = "sam@test.com";
					contact.OC_WebAccessEnabled = true;
					contact.SetHashedPassword("1234");
					Factory.Save();

					var page = GetPageForTest();
					HttpContext.Current.Session["DatabaseNumber"] = new Random().Next();
					page.SiteUser.LoginForTest(org.OH_Code, contact.Email, "1234");
					page.DoPageLoad();

					var hfs = page.LoginForm_Exposed.Controls.OfType<HiddenField>().ToArray();
					void AssertHiddenField(string id, string valueExpected)
					{
						AssertEquals(id, valueExpected, hfs.Single(x => x.ID == id).Value);
					}

					AssertHiddenField("AutoLoginOriginProduct", string.Empty);
					AssertHiddenField("AutoLoginOriginProductVersion", string.Empty);
				}
			}
		}

		[HttpContextEnabledTest]
		public void TestLogin_NoEnv()
		{
			using (EDIDataRegistry.Instance.WiseTechAcademyAutoLoginUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://wisetechacademy.com"))
			{
				var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
				var org = licence.Database.LicEnterprise.Organisation;
				org.OH_Code = "SCWAAASYD";
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "sam";
				contact.OC_Email = "sam@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				Factory.Save();
				contact.Person.PER_EmailAddress = "sam@gmail.com";
				Factory.Save();
				var page = GetPageForTest();
				page.SiteUser.LoginForTest(org.OH_Code, contact.Email, "1234");
				Env.ClearUserContext();
				EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
				page.DoPageLoad();
				var accessToken = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK));
				AssertNull(accessToken);
			}
		}

		[HttpContextEnabledTest]
		public void TestLogin_SupportLogin()
		{
			using (EDIDataRegistry.Instance.WiseTechAcademyAutoLoginUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://wisetechacademy.com"))
			{
				var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
				var org = licence.Database.LicEnterprise.Organisation;
				org.OH_Code = "SCWAAASYD";
				Factory.Save();
				var page = GetPageForTest();
				page.SiteUser.LoginSupportForTest(org.OH_Code);
				page.DoPageLoad();
				var accessToken = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Type, WiseTechAcademyAccessTokenData.TokenType));
				AssertNull(accessToken);
			}
		}

		[HttpContextEnabledTest]
		public void TestLogin_NullCheck()
		{
			using (EDIDataRegistry.Instance.WiseTechAcademyAutoLoginUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://wisetechacademy.com"))
			{
				var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
				var org = licence.Database.LicEnterprise.Organisation;
				org.OH_Code = "SCWAAASYD";
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "sam";
				contact.OC_Email = "sam@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				Factory.Save();

				var page = GetPageForTest();
				page.SiteUser.LoginForTest(org.OH_Code, contact.Email, User.WebTransientPassword);
				TestConnection.ExecuteNonQuery($"DELETE dbo.Orgcontact WHERE OC_PK = '{contact.PK}'; ");
				TestConnection.ExecuteNonQuery($"DELETE dbo.GlbPersonPrimaryRelationship WHERE PPR_PER = '{contact.OC_PER}'; ");
				TestConnection.ExecuteNonQuery($"DELETE dbo.GlbPerson WHERE PER_PK = '{contact.OC_PER}'; ");
				AssertNull(new BusinessObjectFactory().Load<OrgContact>(contact.PK));
				AssertNull(page.SiteUser.LoggedInUser.Person);

				EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
				page.DoPageLoad();
				var accessToken = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK));
				AssertNull(accessToken);
			}
		}

		[HttpContextEnabledTest]
		public void TestLogin_JWT()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence.Database.LicEnterprise.Organisation;
			var build1611101 = CreateNewReleaseBuild(16, 1, 1, 101, false, "GP1");
			licence.Database.LD_HL_CurrentRunningVersion = build1611101.PK;
			org.OH_Code = "SCWAAASYD";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ntt";
			contact.OC_Email = "ntt@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			contact.Person.PER_EmailAddress = "ntt@gmail.com";
			var otherPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			var otherPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			var mergedPersons1 = Factory.New<GlbMergedPerson>();
			mergedPersons1.GMP_PER_Person = contact.Person.PK;
			mergedPersons1.GMP_MergedPerson = otherPerson1.PK;
			var mergedPersons2 = Factory.New<GlbMergedPerson>();
			mergedPersons2.GMP_PER_Person = contact.Person.PK;
			mergedPersons2.GMP_MergedPerson = otherPerson2.PK;
			AssertEquals("Precondition", true, contact.Person.PersonIDs.Contains(otherPerson1.PK));
			AssertEquals("Precondition", true, contact.Person.PersonIDs.Contains(otherPerson2.PK));
			Factory.Save();

			var key = Encoding.UTF8.GetBytes(@"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEAzC3PmqAVO8x+AcloQ5O9RlA+pjAdUxERZTDt/qgPI64uTPXG
PADAnD8+fLHZvrV0zV8O6LWJDuayYvaCxTG4B8gFfT0X6X8DfxJIGOwS2MitcKWU
B3umrk7qgTOCcAWAzWGhBUta5CZTgEGXi3hawX+pRTY+pzBP9ZMqRLdMiZUVWEQr
kWpLhu15CWAYg1JWazuSPXbvX990JYGRoXej4SxRQe13ArcyPs2zmYbZ9xreCpdx
Y/0oHSna51EfxC57Zn0LET5GNAra9K83F3cg+6yiJxwLGm4sanlKSp5Aqd8vQoWU
HbZLDvA4tMRFeg0m77xjSzkTnDEGIyQpoqZ0jQIDAQABAoIBAQCHfF6mXFO6updi
2CM3tHGElvr7jDHpTQod+7nxodNp+cr/hpdkeZtWEyGD3QCAbCh1nv5lrRClsq/s
u2dLMxLLFw+Na1zStFW9nIP7Bav77i4o8baowIR6ZiN2WJfVfdFad85BlR9bBZOj
J+NHyTVv8SaBpt0sVAK7EkyaDIfdQsDSP2qJJVJS0/Zwm55Y/ptw/YxBHOFy6HX9
iipT1+LQa0d2dYDKxLbqpnZQzc4YCrQT5rPP44M5a79vMQMAKMVCPu6lzAVThEfx
fiYzuyvVz4JVJ1tSIf1WLf/F3ypC2K/2MhaRSzM/Nmy1Ib5tpdXwwf40+Z8Mwvbp
QslzksgBAoGBAOgwbW/jnwTmNKHbovc7aS1TzTMV9RyE7Uajx7JOaztZOso2F+kQ
kuICw0OmtQXmWHwXLJ10TXyYuRk3CUum9HlhB4SpAeieLDAKgTXba44V7qtN7F0N
8G3YCIGjKU92vrB4SqcoLeSTlj5R2HPAxSz4oedQ0xLNWjtO14YirlU9AoGBAOEe
Cr5EBeMGN/A0mjFcFa9Pa1+6MlCYlIDrx5PMWSGnUaVdOsNBCb4nhgE8cxU1WKIx
zLyiIJTuN+jmb3fl9rn/aA0RDNRzt7NzqB60ODiRAFFtwZKVpKWQfXbIbbBjJRgJ
43HuXTxpxpl+huySEwWzJeMSXrohVbwQbX5ybbGRAoGAYuGs2Yewgx+eroehAXUV
t64Gp4jkV/7sJbc+JltrI10+wjsDN8hNJV9T1Q277gVJDZ+46l1LWpKX0Xs0xDkX
yFFgKEjpfS1PWC5BFLSbO2lvuRh4XrC/AaiNBth7kVHap8Cy2jksQjnwNB4a9kDU
N/Cy0pYDLfCySquq8X73i2kCgYAVKM25tIsZG6yGV2tm2FDxeXWOOeIg0TakJ4VK
zxpRn3h9IpYzZBmWVgCyfQwUIj+Cf0vPLy4A0aNPsNkpW+Qk92zATan3DilmJKjY
uffO2VI+VSKstIQVS89/KrekrKz/5W4Ld2wsEYUpSEtGUTSYhI47Ga7tr9RvKNwh
1n+ZAQKBgQDdHtmBMpwMcNZwdCP4WfRORPuGjncWoRdGjCgazUmLL+zsBAbsO6VL
7wHq1Y/DGTLwg2thSdxJpCEr5hCq5kyyV+BcTXQJ0N/P/LD/ZHL6AVaSOnxjcd5c
Dx7PPxXUzdGioltaSQyawR5qIaj5E17PxJQesFpXEpmU1g2EnAVP3g==
-----END RSA PRIVATE KEY-----");

			using (EDIDataRegistry.Instance.WiseTechAcademyAutoLoginUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://wisetechacademy.com"))
			using (EDIDataRegistry.Instance.MyAccountSSOJWTTokenExchangePrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, key))
			using (EDIDataRegistry.Instance.EnableAcademyJWTAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var page = GetPageForTest();
				page.SiteUser.LoginForTest(org.OH_Code, contact.Email, "1234");
				page.Request.QueryString.Add("path", "product");
				page.Request.QueryString.Add("target", "cargowise");
				page.Request.QueryString.Add("quickstartid", "AAA");
				page.Request.QueryString.Add("searchText", "COR1301");
				page.Request.QueryString.Add("courseid", "285");
				page.Request.QueryString.Add("type", "technical%20advisory%20note");
				HttpContext.Current.Session["DatabaseNumber"] = licence.Database.LD_DatabaseNumber;

				page.DoPageInit();
				page.DoPageLoad();

				var hfs = page.LoginForm_Exposed.Controls.OfType<HiddenField>().ToArray();
				void AssertHiddenField(string id, string valueExpected)
				{
					AssertEquals(id, valueExpected, hfs.Single(x => x.ID == id).Value);
				}

				var token = new JwtSecurityTokenHandler().ReadJwtToken(hfs.Single(x => x.ID == "Token").Value);
				void AssertTokenField(string controlId, string valueExpected, bool hasFormValue = false)
				{
					var claim = typeof(WiseTechAcademyLoginClaims).GetField(controlId, BindingFlags.Public | BindingFlags.Static).GetValue(null) as string;
					AssertNotNullOrEmpty($"{controlId} missed token claim", claim);
					AssertEquals($"{controlId} is not equal with the value in the token", token.Claims.FirstOrDefault(x => x.Type == claim).Value, valueExpected);
					Assert($"({controlId})These fields should be in token", hasFormValue || !hfs.Any(x => x.ID == controlId));
				}

				CombineAssertions(() =>
				{
					AssertTokenField("TenantId", "101");
					AssertTokenField("Product", "WTA");
					AssertTokenField("OrgName", "ENTCOM Company");
					AssertTokenField("ContactWorkingAddressOrgName", "ENTCOM Company");
					AssertTokenField("ContactWorkingAddressCountry", "AU");
					AssertTokenField("ContactLocation", "AUSYD");
					AssertTokenField("LicenceDatabaseMasterOrgCode", "SCWAAASYD");
					AssertTokenField("LicenceDatabaseMasterOrgName", "ENTCOM Company");
					AssertTokenField("LicenceDatabaseBillingOrgCode", "SCWAAASYD");
					AssertTokenField("LicenceDatabaseBillingOrgName", "ENTCOM Company");
					AssertTokenField("ContactPk", contact.PK.ToString());
					AssertTokenField("ContactName", "ntt");
					AssertTokenField("ContactEmail", "ntt@test.com");
					AssertTokenField("PersonalEmail", "ntt@gmail.com");
					AssertTokenField("AutoLoginOriginProduct", "CW1", true);
					AssertTokenField("AutoLoginOriginProductVersion", "16.1.1.101", true);

					var tokenPersonID = token.Claims.First(x => x.Type == WiseTechAcademyLoginClaims.PersonIDs).Value;
					AssertContains(otherPerson1.PK.ToString(), tokenPersonID);
					AssertContains(otherPerson2.PK.ToString(), tokenPersonID);
					AssertContains(contact.Person.PK.ToString(), tokenPersonID);
					var userAccount = new BusinessObjectFactory().Load<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_Email, "ntt@test.com")).Single();
					AssertEquals(contact.PK, userAccount.EUA_OC_WebAccessContact);
					AssertTokenField("UserId", userAccount.PK.ToString());
				});

				CombineAssertions("Constant Form Fields", () =>
				{
					AssertHiddenField("Path", "product");
					AssertHiddenField("Target", "cargowise");
					AssertHiddenField("QuickstartId", "AAA");
					AssertHiddenField("SearchText", "COR1301");
					AssertHiddenField("CourseId", "285");
					AssertHiddenField("Type", "technical%20advisory%20note");

					AssertHiddenField("AutoLoginOriginProduct", "CW1");
					AssertHiddenField("AutoLoginOriginProductVersion", "16.1.1.101");
				});
			}
		}

		internal static WiseTechAcademyAutoLoginForTest GetPageForTest()
		{
			var page = new WiseTechAcademyAutoLoginForTest();
			MethodInfo method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		ReleaseBuild CreateNewReleaseBuild(int majorVersion, int minorVersion, int release, int patch, bool superceded, string releaseStatus)
		{
			var releaseBuild = ReleaseBuild.NewForTesting(Factory, releaseStatus, superceded);
			releaseBuild.HL_MajorVersion = majorVersion;
			releaseBuild.HL_MinorVersion = minorVersion;
			releaseBuild.HL_Release = release;
			releaseBuild.HL_Patch = patch;
			releaseBuild.HL_Product = ProductTypes.Codes.CargoWiseOne;
			return releaseBuild;
		}

		internal class WiseTechAcademyAutoLoginForTest : WiseTechAcademyAutoLogin
		{
			public WiseTechAcademyAutoLoginForTest()
			{
				LoginForm = new HtmlForm();
				Token = new HiddenField()
				{ ID = "Token" };
				TenantId = new HiddenField()
				{ ID = "TenantId" };
				Product = new HiddenField()
				{ ID = "Product" };
				OrgPk = new HiddenField()
				{ ID = "OrgPk" };
				OrgName = new HiddenField()
				{ ID = "OrgName" };
				ContactWorkingAddressOrgName = new HiddenField()
				{ ID = "ContactWorkingAddressOrgName" };
				ContactWorkingAddressCountry = new HiddenField()
				{ ID = "ContactWorkingAddressCountry" };
				ContactLocation = new HiddenField()
				{ ID = "ContactLocation" };
				LicenceDatabaseMasterOrgCode = new HiddenField()
				{ ID = "LicenceDatabaseMasterOrgCode" };
				LicenceDatabaseMasterOrgName = new HiddenField()
				{ ID = "LicenceDatabaseMasterOrgName" };
				LicenceDatabaseBillingOrgCode = new HiddenField()
				{ ID = "LicenceDatabaseBillingOrgCode" };
				LicenceDatabaseBillingOrgName = new HiddenField()
				{ ID = "LicenceDatabaseBillingOrgName" };
				ContactPk = new HiddenField()
				{ ID = "ContactPk" };
				ContactName = new HiddenField()
				{ ID = "ContactName" };
				ContactEmail = new HiddenField()
				{ ID = "ContactEmail" };
				PersonalEmail = new HiddenField()
				{ ID = "PersonalEmail" };
				PersonIDs = new HiddenField()
				{ ID = "PersonIDs" };
				Path = new HiddenField()
				{ ID = "Path" };
				Target = new HiddenField()
				{ ID = "Target" };
				QuickstartId = new HiddenField()
				{ ID = "QuickstartId" };
				SearchText = new HiddenField()
				{ ID = "SearchText" };
				UserId = new HiddenField()
				{ ID = "UserId" };
				CourseId = new HiddenField()
				{ ID = "CourseId" };
				Type = new HiddenField()
				{ ID = "Type" };
				ProgramId = new HiddenField()
				{ ID = "ProgramId" };
				AutoLoginOriginProduct = new HiddenField()
				{ ID = "AutoLoginOriginProduct" };
				AutoLoginOriginProductVersion = new HiddenField()
				{ ID = "AutoLoginOriginProductVersion" };
				DefaultBody = new HtmlGenericControl();
				LoginForm.Controls.Add(Token);
				LoginForm.Controls.Add(TenantId);
				LoginForm.Controls.Add(Product);
				LoginForm.Controls.Add(OrgPk);
				LoginForm.Controls.Add(OrgName);
				LoginForm.Controls.Add(ContactWorkingAddressOrgName);
				LoginForm.Controls.Add(ContactWorkingAddressCountry);
				LoginForm.Controls.Add(ContactLocation);
				LoginForm.Controls.Add(LicenceDatabaseMasterOrgCode);
				LoginForm.Controls.Add(LicenceDatabaseMasterOrgName);
				LoginForm.Controls.Add(LicenceDatabaseBillingOrgCode);
				LoginForm.Controls.Add(LicenceDatabaseBillingOrgName);
				LoginForm.Controls.Add(ContactPk);
				LoginForm.Controls.Add(ContactName);
				LoginForm.Controls.Add(ContactEmail);
				LoginForm.Controls.Add(PersonalEmail);
				LoginForm.Controls.Add(PersonIDs);
				LoginForm.Controls.Add(UserId);
				LoginForm.Controls.Add(Path);
				LoginForm.Controls.Add(Target);
				LoginForm.Controls.Add(QuickstartId);
				LoginForm.Controls.Add(SearchText);
				LoginForm.Controls.Add(CourseId);
				LoginForm.Controls.Add(Type);
				LoginForm.Controls.Add(ProgramId);
				LoginForm.Controls.Add(AutoLoginOriginProduct);
				LoginForm.Controls.Add(AutoLoginOriginProductVersion);
			}

			public void DoPageLoad()
			{
				base.Page_Load(null, EventArgs.Empty);
			}

			public void DoPageInit()
			{
				base.Page_Init(this, EventArgs.Empty);
			}

			public HtmlForm LoginForm_Exposed => LoginForm;
		}
	}
}
