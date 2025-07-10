using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(DocumentImageTypeRegistryItem))]
	class DocumentImageTypeRegistryItemTest : StronglyTypedRegistryItemTestCase<DocumentImageTypeCollection>
	{
		public void TestConstructor()
		{
			AssertEquals("Test", Item.Name);
			AssertEquals("Category", Item.Category);
			AssertEquals("Caption", Item.Caption);
			AssertEquals("Hint", Item.Hint);
			AssertEquals(typeof(DocumentImageTypeCollection), Item.DataType.DataType);
			AssertEquals(RegistryStorageFlags.System, Item.Storage);
		}

		protected override StronglyTypedRegistryItem<DocumentImageTypeCollection, DocumentImageTypeCollection> GetNewRegistryItem()
		{
			return new DocumentImageTypeRegistryItem("Test", "Category", "Caption", "Hint");
		}
	}
}
