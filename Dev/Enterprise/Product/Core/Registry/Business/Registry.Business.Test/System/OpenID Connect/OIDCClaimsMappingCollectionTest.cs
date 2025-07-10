using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OIDCClaimsMappingCollection))]
	sealed class OIDCClaimsMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OIDCClaimsMappingCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override OIDCClaimsMappingCollection GetCollectionToTest()
		{
			return new OIDCClaimsMappingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OIDCClaimsMapping()
			{
				ClaimName = "someclaim",
				Identifier = "GlbStaff.GS_LoginName",
			};
		}
	}
}
