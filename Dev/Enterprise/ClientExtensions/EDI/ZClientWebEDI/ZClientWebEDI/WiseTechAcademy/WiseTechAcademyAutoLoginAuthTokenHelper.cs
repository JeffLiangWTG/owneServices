using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Microsoft.IdentityModel.Tokens;
using WTG.IdentitySecurity;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class WiseTechAcademyAutoLoginAuthTokenHelper
	{
		public static string GenerateAuthToken(EdiCustomerUserAccount userAccount, LicenceDatabase database, OrgContact loginContact, OrgHeader billingOrg, LicenceDatabase originalSystemLicenceDatabase)
		{
			if (userAccount == null || database == null || loginContact == null || billingOrg == null)
			{
				return string.Empty;
			}
			var privateKeyBytes = EDIDataRegistry.Instance.MyAccountSSOJWTTokenExchangePrivateKey.Value;
			if (privateKeyBytes == null || privateKeyBytes.Length == 0)
			{
				return string.Empty;
			}
			var privateKeyPem = Encoding.UTF8.GetString(privateKeyBytes);
			var privateKey = RSAKeyProvider.ImportPrivateKey(privateKeyPem);

			var securityKey = new RsaSecurityKey(privateKey)
			{
				CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
			};

			var billingOrgName = billingOrg.Addresses.DefaultAddressOfType(OrgAddressType.Receivables)?.CompanyName ?? billingOrg.OH_FullName;
			var personIds = string.Join(",", loginContact.Person?.PersonIDs.Select(x => x.ToString()) ?? Array.Empty<string>());
			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(WiseTechAcademyLoginClaims.TenantId, database.LD_TenantID),
					new Claim(WiseTechAcademyLoginClaims.Product, database.LD_Product),
					new Claim(WiseTechAcademyLoginClaims.OrgPk, loginContact.OC_OH.ToString()),
					new Claim(WiseTechAcademyLoginClaims.OrgName, loginContact.Header.OH_FullName),
					new Claim(WiseTechAcademyLoginClaims.ContactWorkingAddressOrgName, loginContact.WorkingAddressCompanyName),
					new Claim(WiseTechAcademyLoginClaims.ContactWorkingAddressCountry, WiseTechAcademyAutoLoginHelper.GetWorkingAddressCountryCode(loginContact)),
					new Claim(WiseTechAcademyLoginClaims.ContactLocation, loginContact.Location),
					new Claim(WiseTechAcademyLoginClaims.LicenceDatabaseMasterOrgCode, database.WebAccessOrg?.OH_Code ?? ZString.Empty),
					new Claim(WiseTechAcademyLoginClaims.LicenceDatabaseMasterOrgName, database.WebAccessOrg?.OH_FullName ?? ZString.Empty),
					new Claim(WiseTechAcademyLoginClaims.LicenceDatabaseBillingOrgCode, billingOrg.OH_Code),
					new Claim(WiseTechAcademyLoginClaims.LicenceDatabaseBillingOrgName, billingOrgName),
					new Claim(WiseTechAcademyLoginClaims.UserId, userAccount.EUA_UserID),
					new Claim(WiseTechAcademyLoginClaims.ContactPk, loginContact.PK.ToString()),
					new Claim(WiseTechAcademyLoginClaims.ContactName, loginContact.ContactNameWithoutNumberSuffix),
					new Claim(WiseTechAcademyLoginClaims.ContactEmail, loginContact.OC_Email),
					new Claim(WiseTechAcademyLoginClaims.PersonalEmail, loginContact.Person.PER_EmailAddress),
					new Claim(WiseTechAcademyLoginClaims.PersonIDs, personIds),
					new Claim(WiseTechAcademyLoginClaims.AutoLoginOriginProduct, originalSystemLicenceDatabase?.CurrentVersion?.HL_Product ?? string.Empty),
					new Claim(WiseTechAcademyLoginClaims.AutoLoginOriginProductVersion,  originalSystemLicenceDatabase?.CurrentVersion?.VersionNumber.ToString() ?? string.Empty),
				}),
				Audience = AudienceName,
				Issuer = IssuerName,
				Expires = ZDateTime.UtcNow.AddMinutes(AuthTokenLifeTime).ToDateTime(),
				SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256),
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		const int AuthTokenLifeTime = 5;
		const string IssuerName = "MyAccount";
		const string AudienceName = "WiseTechAcademy";
	}
}
