using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OAuth2ScopesCollection))]
	sealed class OAuth2ScopeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OAuth2ScopesCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override OAuth2ScopesCollection GetCollectionToTest()
		{
			return new OAuth2ScopesCollection();
		}

		int _counter;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			_counter++;
			return new OAuth2Scope()
			{
				ScopeName = $"SomeScope{_counter}",
			};
		}
	}
}
