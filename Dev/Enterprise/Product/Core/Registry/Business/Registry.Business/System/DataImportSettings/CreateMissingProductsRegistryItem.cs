using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CreateMissingProductsRegistryItem : StronglyTypedRegistryItem<CreateMissingProductsInfo>
	{
		public CreateMissingProductsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CreateMissingProductsInfo defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CreateMissingProductsRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.CreateMissingProductsItemEditor, Enterprise.Registry.GUI")]
	public class CreateMissingProductsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CreateMissingProductsInfo>
	{
	}
}
