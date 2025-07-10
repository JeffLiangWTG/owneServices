using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CodeSelectionCollectionRegistryItem : StronglyTypedRegistryItem<CodeSelectionCollection>
	{
		public CodeSelectionCollectionRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			CodeDescriptionPairListProvider codesProvider)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeSelectionCollectionRegistryDataType(codesProvider), storage))
		{
		}

		public CodeSelectionCollectionRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			CodeDescriptionPairListProvider codesProvider,
			RegistryOptions options,
			CodeSelectionCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeSelectionCollectionRegistryDataType(codesProvider), storage, options, defaultValue))
		{
		}
	}
}
