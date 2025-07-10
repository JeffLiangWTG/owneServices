using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.UserManagement.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class WiseTechAcademyAutoLoginAuthTokenHelperTest : TestCaseWithFactory
	{
		public void TestGenerateAuthToken()
		{
			EDIDataRegistry.Instance.MyAccountSSOJWTTokenExchangePrivateKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Encoding.UTF8.GetBytes(@"-----BEGIN RSA PRIVATE KEY-----
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
-----END RSA PRIVATE KEY-----"));

			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			database.LD_Product = "CW1";
			database.LD_TenantID = "AUSYDEDI";

			var org = licence.Database.LicEnterprise.Organisation;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact1";
			contact.OC_Email = "contact1@cw1.com";
			Factory.Save();
			contact.Person.PER_EmailAddress = "walawala@123.com";
			Factory.Save();
			var (userAccount, billingOrg) = WiseTechAcademyAutoLoginHelper.GetOrCreateCustomerUserAccount(org, contact);

			AssertNullOrEmpty(WiseTechAcademyAutoLoginAuthTokenHelper.GenerateAuthToken(userAccount, null, null, null, null));
			AssertNullOrEmpty(WiseTechAcademyAutoLoginAuthTokenHelper.GenerateAuthToken(null, database, null, null, null));
			AssertNullOrEmpty(WiseTechAcademyAutoLoginAuthTokenHelper.GenerateAuthToken(null, null, contact, null, null));
			AssertNullOrEmpty(WiseTechAcademyAutoLoginAuthTokenHelper.GenerateAuthToken(null, null, null, org, null));

			var emptyContactResult = WiseTechAcademyAutoLoginAuthTokenHelper.GenerateAuthToken(userAccount, licence.Database, contact, org, null);
			AssertNotNullOrEmpty(emptyContactResult);
			var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(emptyContactResult);

			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.Product).Value);
			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.OrgPk).Value);
			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.OrgName).Value);
			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.ContactWorkingAddressOrgName).Value);
			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.ContactWorkingAddressCountry).Value);
			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.LicenceDatabaseMasterOrgCode).Value);
			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.LicenceDatabaseBillingOrgCode).Value);
			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.UserId).Value);
			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.ContactName).Value);
			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.ContactPk).Value);
			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.ContactEmail).Value);
			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.PersonalEmail).Value);
			AssertNotNullOrEmpty(jwtToken.Claims.FirstOrDefault(x => x.Type == WiseTechAcademyLoginClaims.PersonIDs).Value);
		}
	}
}
