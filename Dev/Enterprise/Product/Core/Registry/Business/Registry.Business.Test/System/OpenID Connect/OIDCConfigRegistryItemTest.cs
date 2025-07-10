using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OIDCConfigRegistryItem))]
	sealed class OIDCConfigRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<OIDCConfig>
	{
		protected override StronglyTypedRegistryItem<OIDCConfig, OIDCConfig> GetNewRegistryItem()
		{
			return new OIDCConfigRegistryItem(
						"OIDCConfig",
						null,
						null,
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						OIDCConfig.DefaultValue);
		}

		protected override OIDCConfig ValidValue
		{
			get
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

				oidcConfig.IsVerified = true;
				return oidcConfig;
			}
		}
	}
}
