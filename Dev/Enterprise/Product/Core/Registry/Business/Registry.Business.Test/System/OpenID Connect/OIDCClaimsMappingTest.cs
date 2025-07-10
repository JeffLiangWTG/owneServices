using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OIDCClaimsMapping))]
	sealed class OIDCClaimsMappingTest : RegistryBusinessObjectTemplateTestCase<OIDCClaimsMapping>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override OIDCClaimsMapping GetBusinessObjectToClone()
		{
			return new OIDCClaimsMapping()
			{
				ClaimName = "someclaim",
				Identifier = "GlbStaff.GS_LoginName",
			};
		}

		protected override OIDCClaimsMapping GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
