using System.Collections.Generic;
using System.Security.Claims;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.Testing
{
	public class OidcLoginClaimDataTest : TestCase
	{
		public void TestClaimValueIsNullWhenError()
		{
			var oidcLoginClaimData = new OidcLoginClaimData("error", "error description");
			AssertNull(oidcLoginClaimData["any_claim_type"]);
		}

		public void TestClaimValueIsCorrect()
		{
			var mockOidcConfig = new Mock<IOIDCConfig>();
			mockOidcConfig.Setup(config => config.ClaimsMappings).Returns(new OIDCClaimsMappingCollection()
			{
				new OIDCClaimsMapping
					{
						ClaimName = "user_email",
						Identifier = "GlbStaff.GS_EmailAddress"
					}
			});
			var claims = new List<Claim>
			{
				new("user_email", "testemail@123.com"),
				new("company_code", "WTG"),
				new("original_user_name", "Hunter"),
			};
			var oidcLoginClaimData = new OidcLoginClaimData(claims, mockOidcConfig.Object);
			AssertEquals("testemail@123.com", oidcLoginClaimData["user_email"]);
			AssertEquals("WTG", oidcLoginClaimData["company_code"]);
			AssertEquals("Hunter", oidcLoginClaimData["original_user_name"]);
			AssertNull(oidcLoginClaimData["non_existent_claim"]);
		}
	}
}
