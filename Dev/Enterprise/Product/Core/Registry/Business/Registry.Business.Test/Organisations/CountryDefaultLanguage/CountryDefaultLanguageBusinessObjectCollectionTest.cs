using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CountryDefaultLanguageBusinessObjectCollection))]
	sealed class CountryDefaultLanguageBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CountryDefaultLanguageBusinessObjectCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => false;

		protected override CountryDefaultLanguageBusinessObjectCollection GetCollectionToTest()
		{
			return new CountryDefaultLanguageBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CountryDefaultLanguageBusinessObject();
		}
	}
}
