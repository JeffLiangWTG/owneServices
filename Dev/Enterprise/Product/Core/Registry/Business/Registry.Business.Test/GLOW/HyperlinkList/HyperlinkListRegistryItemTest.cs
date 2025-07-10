using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HyperlinkListRegistryItem))]
	sealed class HyperlinkListRegistryItemTest : StronglyTypedRegistryItemTestCase<HyperlinkCollection>
	{
		protected override StronglyTypedRegistryItem<HyperlinkCollection, HyperlinkCollection> GetNewRegistryItem()
		{
			var defaultCollection = new HyperlinkCollection();
			return new HyperlinkListRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.Default);
		}
	}
}
