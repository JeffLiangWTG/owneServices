using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	public class OIDCConfigFactoryTest : TestCaseWithFactory
	{
		OIDCConfig OIDCConfig;
		OIDCConfig WinzorOIDCConfig;

		public void TestGetOIDCConfigShouldReturnOIDCConfigWhenNotInWinzor()
		{
			using (Globals.SetIsWinzorForTest(false))
			{
				var oidcConfig = OIDCConfigFactory.GetOIDCConfig();
				AssertEquals(OIDCConfig, oidcConfig);
			}
		}

		public void TestGetOIDCConfigShouldReturnOIDCConfigWhenInWinzorButWinzorConfigIsDisabled()
		{
			using (Globals.SetIsWinzorForTest(true))
			{
				WinzorOIDCConfig.IsOIDCEnabled = false;
				WinzorOIDCConfig.IsVerified = true;
				SystemDataRegistry.Instance.WinzorOIDCConfig.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WinzorOIDCConfig);
				SystemDataRegistry.Instance.WinzorOIDCConfig.Inner.ClearCache();
				var oidcConfig = OIDCConfigFactory.GetOIDCConfig();
				AssertEquals(OIDCConfig, oidcConfig);
			}
		}

		public void TestGetOIDCConfigShouldReturnWinzorOIDCConfigWhenInWinzorAndWinzorConfigIsEnabled()
		{
			using (Globals.SetIsWinzorForTest(true))
			{
				var oidcConfig = OIDCConfigFactory.GetOIDCConfig();
				AssertEquals(WinzorOIDCConfig, oidcConfig);
			}
		}

		protected override void SetUp()
		{
			OIDCConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};
			OIDCConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim",
				Identifier = "GlbStaff.GS_LoginName"
			});
			OIDCConfig.IsVerified = true;
			SystemDataRegistry.Instance.OIDCConfig.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OIDCConfig);
			SystemDataRegistry.Instance.OIDCConfig.Inner.ClearCache();

			WinzorOIDCConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = "https://winzoronlyidentityprovider.com",
				ClientIdentifier = "SomeId",
			};
			WinzorOIDCConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim",
				Identifier = "GlbStaff.GS_LoginName"
			});
			WinzorOIDCConfig.IsVerified = true;
			SystemDataRegistry.Instance.WinzorOIDCConfig.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WinzorOIDCConfig);
			SystemDataRegistry.Instance.WinzorOIDCConfig.Inner.ClearCache();
		}
	}
}
