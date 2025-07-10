using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class AdditionalHouseBillOfLadingTypeCollectionRegistryItem : StronglyTypedRegistryItem<AdditionalHouseBillOfLadingTypeCollection>
	{
		public AdditionalHouseBillOfLadingTypeCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, AdditionalHouseBillOfLadingTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AdditionalHouseBillOfLadingTypeCollectionRegistryDataType(), storage, defaultValue))
		{
		}

		public AdditionalHouseBillOfLadingTypeCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, AdditionalHouseBillOfLadingTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AdditionalHouseBillOfLadingTypeCollectionRegistryDataType(), storage, options, defaultValue))
		{
		}

		[RegistryEditor("Enterprise.Registry.GUI.AdditionalHouseBillOfLadingTypeCollectionRegistryItemEditor, Enterprise.Registry.GUI")]
#if DEBUG
		public
#endif
		sealed class AdditionalHouseBillOfLadingTypeCollectionRegistryDataType : FallbackMergedRegistryBusinessObjectCollectionDataType<AdditionalHouseBillOfLadingTypeCollection>
		{
			public AdditionalHouseBillOfLadingTypeCollectionRegistryDataType()
			{
			}
		}
	}
}
