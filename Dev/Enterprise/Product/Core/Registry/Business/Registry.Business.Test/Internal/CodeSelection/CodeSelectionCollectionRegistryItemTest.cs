using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeSelectionCollectionRegistryItem))]
	sealed class CodeSelectionCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeSelectionCollection>
	{
		protected override StronglyTypedRegistryItem<CodeSelectionCollection, CodeSelectionCollection> GetNewRegistryItem()
		{
			return new CodeSelectionCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, CodeSelectionTest.GetCodesProviderForTesting());
		}
	}
}
