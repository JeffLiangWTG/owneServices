using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(EnglishSpellingRegistryItem))]
	sealed class EnglishSpellingRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new EnglishSpellingRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System);
		}
	}
}
