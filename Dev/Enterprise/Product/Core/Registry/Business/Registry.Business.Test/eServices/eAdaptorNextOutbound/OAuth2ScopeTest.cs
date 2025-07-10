using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OAuth2Scope))]
	sealed class OAuth2ScopeTest : RegistryBusinessObjectTemplateTestCase<OAuth2Scope>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override OAuth2Scope GetBusinessObjectToClone()
		{
			return new OAuth2Scope()
			{
				ScopeName = "SomeScope"
			};
		}

		protected override OAuth2Scope GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
