using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement.Test
{
	internal class OIDCConfigHelper
	{
		public static OIDCConfig GetOIDCConfig()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Azure,
				AuthorityURL = "https://test.com",
				ClientIdentifier = "testid",
			};
			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "testname", Identifier = "GlbStaff.GS_LoginName" });
			oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
			oidcConfig.IsVerified = true;
			return oidcConfig;
		}
	}
}
