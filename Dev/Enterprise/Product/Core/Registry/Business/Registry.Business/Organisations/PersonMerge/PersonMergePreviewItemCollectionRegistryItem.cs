using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class PersonMergePreviewItemCollectionRegistryItem : StronglyTypedRegistryItem<PersonMergePreviewItemCollection>
	{
		public PersonMergePreviewItemCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, PersonMergePreviewItemCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new PersonMergePreviewItemRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
