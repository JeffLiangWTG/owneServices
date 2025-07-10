using Enterprise.Integration;

namespace Enterprise.Registry.Business.Testing
{
	sealed class TranslatableRegistryBusinessItemCollectionRegistryItemForTest : TranslatableRegistryBusinessItemCollectionRegistryItem<CodeDescriptionBoolCollection, CodeDescriptionBoolCollection>
	{
		public TranslatableRegistryBusinessItemCollectionRegistryItemForTest(IRegistryItem inner)
			: base(inner) { }

		public override int MaxLength { get { return 256; } }
	}
}
