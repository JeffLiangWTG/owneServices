using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class StaffColumnToGroupDescriptionScimMappingRegistryItem : StronglyTypedRegistryItem<StaffColumnToGroupDescriptionScimMappingCollection>
	{
		public StaffColumnToGroupDescriptionScimMappingRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new StaffColumnToGroupDescriptionScimMappingRegistryDataType(), storage, options))
		{
		}
	}
}
