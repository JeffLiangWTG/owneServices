using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class DataTransferRegistryItem : AutomaticProcessRegistryItemBase<DataTransferRegistryBusinessObject>
	{
		public DataTransferRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(name, category, caption, hint, new DataTransferRegistryDataType(), storage)
		{
		}

		public DataTransferRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new DataTransferRegistryDataType(), storage, options)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.DataTransferRegistryItemEditor, Enterprise.Registry.GUI")]
	class DataTransferRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DataTransferRegistryBusinessObject>
	{
		public DataTransferRegistryDataType()
		{
		}
	}
}
