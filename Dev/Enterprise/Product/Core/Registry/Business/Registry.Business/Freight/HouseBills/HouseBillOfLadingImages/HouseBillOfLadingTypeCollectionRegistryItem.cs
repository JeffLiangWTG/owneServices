using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class HouseBillOfLadingTypeCollectionRegistryItem : StronglyTypedRegistryItem<HouseBillOfLadingTypeCollection>
	{
		public HouseBillOfLadingTypeCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, HouseBillOfLadingTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new HouseBillOfLadingTypeCollectionRegistryDataType(), storage, defaultValue))
		{
		}

		public HouseBillOfLadingTypeCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, HouseBillOfLadingTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new HouseBillOfLadingTypeCollectionRegistryDataType(), storage, options, defaultValue))
		{
		}

		[RegistryEditor("Enterprise.Registry.GUI.HouseBillOfLadingTypeCollectionRegistryItemEditor, Enterprise.Registry.GUI")]
		internal sealed class HouseBillOfLadingTypeCollectionRegistryDataType : FallbackMergedRegistryBusinessObjectCollectionDataType<HouseBillOfLadingTypeCollection>
		{
			public HouseBillOfLadingTypeCollectionRegistryDataType()
			{
			}
		}
	}
}
