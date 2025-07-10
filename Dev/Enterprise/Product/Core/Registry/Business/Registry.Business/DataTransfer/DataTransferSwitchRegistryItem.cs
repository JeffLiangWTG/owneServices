using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class DataTransferSwitchRegistryItem : AutomaticProcessRegistryItemBase<DataTransferSwitchRegistryBusinessObject>
	{
		public DataTransferSwitchRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new DataTransferSwitchRegistryDataType(), storage, options)
		{
		}

		[RegistryEditor("Enterprise.Registry.GUI.DataTransferSwitchRegistryItemEditor, Enterprise.Registry.GUI")]
		internal class DataTransferSwitchRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DataTransferSwitchRegistryBusinessObject>
		{
			public DataTransferSwitchRegistryDataType()
			{
			}
		}
	}
}
