using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CountryDefaultLanguageBusinessObjectCollectionRegistryItem))]
	sealed class CountryDefaultLanguageBusinessObjectCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<CountryDefaultLanguageBusinessObjectCollection>
	{
		protected override StronglyTypedRegistryItem<CountryDefaultLanguageBusinessObjectCollection, CountryDefaultLanguageBusinessObjectCollection> GetNewRegistryItem()
		{
			return new CountryDefaultLanguageBusinessObjectCollectionRegistryItem("1", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, RegistryOptions.Default, new CountryDefaultLanguageBusinessObjectCollection());
		}
	}
}
