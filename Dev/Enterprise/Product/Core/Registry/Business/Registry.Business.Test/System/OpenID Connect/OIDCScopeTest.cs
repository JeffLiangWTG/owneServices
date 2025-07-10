using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OIDCScope))]
	sealed class OIDCScopeTest : RegistryBusinessObjectTemplateTestCase<OIDCScope>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override OIDCScope GetBusinessObjectToClone()
		{
			return new OIDCScope()
			{
				ScopeName = "SomeScope"
			};
		}

		protected override OIDCScope GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
