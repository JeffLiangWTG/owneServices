using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OIDCScopesCollection))]
	sealed class OIDCScopeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OIDCScopesCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override OIDCScopesCollection GetCollectionToTest()
		{
			return new OIDCScopesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OIDCScope()
			{
				ScopeName = "SomeScope",
			};
		}
	}
}
