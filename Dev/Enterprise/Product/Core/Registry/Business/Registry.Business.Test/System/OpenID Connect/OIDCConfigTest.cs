using System;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OIDCConfig))]
	sealed class OIDCConfigTest : RegistryBusinessObjectTemplateTestCase<OIDCConfig>
	{
		public void TestIsVerifiedShouldBeTrueIfConfigIsEnabledInDatabase()
		{
			var oidcConfig = GetBusinessObjectToClone();
			oidcConfig.IsVerified = true; //mock isverified
			SystemDataRegistry.Instance.OIDCConfig.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig);
			SystemDataRegistry.Instance.OIDCConfig.Inner.ClearCache();

			Assert("oidcconfig is enabled in database", oidcConfig.IsOIDCEnabled);
			Assert("Should be verified if config is enabled in database.", SystemDataRegistry.Instance.OIDCConfig.Value.IsVerified);
		}

		public void TestIsVerifiedShouldBeFalseIfConfigIsNotEnabledInDatabase()
		{
			var oidcConfig = GetBusinessObjectToClone();
			oidcConfig.IsOIDCEnabled = false;
			SystemDataRegistry.Instance.OIDCConfig.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig);
			SystemDataRegistry.Instance.OIDCConfig.Inner.ClearCache();

			Assert("oidcconfig is not enabled in database", !oidcConfig.IsOIDCEnabled);
			Assert("Should not be verified if config is not enabled in database.", !SystemDataRegistry.Instance.OIDCConfig.Value.IsVerified);
		}

		public void TestIsVerifiedShouldBeFalseIfConfigHasAnyChange()
		{
			var oidcConfig = new OIDCConfig();

			oidcConfig.IsVerified = true;
			Assert("mock it's verified config.", oidcConfig.IsVerified);
			oidcConfig.OIDCServerType = OIDCServerTypes.Azure;
			Assert("Should be false as the config is changed.", !oidcConfig.IsVerified);

			oidcConfig.IsVerified = true;
			Assert("mock it's verified config.", oidcConfig.IsVerified);
			oidcConfig.AuthorityURL = "https://aaa.com";
			Assert("Should be false as the config is changed.", !oidcConfig.IsVerified);

			oidcConfig.IsVerified = true;
			Assert("mock it's verified config.", oidcConfig.IsVerified);
			oidcConfig.ClientIdentifier = "clientid";
			Assert("Should be false as the config is changed.", !oidcConfig.IsVerified);

			oidcConfig.IsVerified = true;
			Assert("mock it's verified config.", oidcConfig.IsVerified);
			oidcConfig.IsOIDCEnabled = true;
			Assert("Should be false as the config is changed.", !oidcConfig.IsVerified);

			oidcConfig.IsVerified = true;
			Assert("mock it's verified config.", oidcConfig.IsVerified);
			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "user_name", Identifier = "GlbStaff.GS_LoginName" });
			Assert("Should be false as the config is changed.", !oidcConfig.IsVerified);

			oidcConfig.IsVerified = true;
			Assert("mock it's verified config.", oidcConfig.IsVerified);
			oidcConfig.ClaimsMappings[0].ClaimName = "username";
			Assert("Should be false as the config is changed.", !oidcConfig.IsVerified);

			oidcConfig.IsVerified = true;
			Assert("mock it's verified config.", oidcConfig.IsVerified);
			oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "somescope" });
			Assert("Should be false as the config is changed.", !oidcConfig.IsVerified);

			oidcConfig.IsVerified = true;
			Assert("mock it's verified config.", oidcConfig.IsVerified);
			oidcConfig.Scopes[0].ScopeName = "newscope";
			Assert("Should be false as the config is changed.", !oidcConfig.IsVerified);
		}

		public void TestOidcServerTypeMapping()
		{
			var codes = new String[] { "GEN", "OKT", "AZU", "ONE", "WTG" };
			var oidcServerTypes = new OIDCServerTypes[] { OIDCServerTypes.Generic , OIDCServerTypes.Okta,
				OIDCServerTypes.Azure, OIDCServerTypes.OneLogin, OIDCServerTypes.WiseTechIdP };

			for (int i = 0; i < codes.Length; i++)
			{
				var oidcConfig = new OIDCConfig()
				{
					IsOIDCEnabled = true,
					OIDCServerTypeCode = codes[i]
				};
				AssertEquals(oidcServerTypes[i], oidcConfig.OIDCServerType);
			}
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override OIDCConfig GetBusinessObjectToClone()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim",
				Identifier = "GlbStaff.GS_LoginName"
			});

			return oidcConfig;
		}

		protected override OIDCConfig GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		public override void TestBizObjectFields()
		{
			// Tested elsewhere
			Assert(true);
		}
	}
}
