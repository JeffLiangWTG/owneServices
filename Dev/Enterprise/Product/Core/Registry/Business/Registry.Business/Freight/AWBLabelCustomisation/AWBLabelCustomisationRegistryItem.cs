using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AWBLabelCustomisationRegistryItem : StronglyTypedRegistryItem<AWBLabelCustomisation>
	{
		public AWBLabelCustomisationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, AWBLabelCustomisation defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AWBLabelCustomisationRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.AWBLabelCustomisationRegistryItemEditor, Enterprise.Registry.GUI")]
	class AWBLabelCustomisationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AWBLabelCustomisation>
	{
		public AWBLabelCustomisationRegistryDataType()
		{
		}
	}
}
