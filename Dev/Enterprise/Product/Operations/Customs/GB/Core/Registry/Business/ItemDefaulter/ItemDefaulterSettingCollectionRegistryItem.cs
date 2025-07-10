using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
namespace Enterprise.Customs.GB.Registry
{
	public class ItemDefaulterSettingCollectionRegistryItem : StronglyTypedRegistryItem<ItemDefaulterSettingCollection>
	{
		public ItemDefaulterSettingCollectionRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, (NoResString)caption, (NoResString)hint, new ItemDefaulterRegistryDataType(), storage))
		{
		}
	}
}
