using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AWBRoundingRegistryItem : StronglyTypedRegistryItem<AWBRoundingCollection>
	{
		public AWBRoundingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, AWBRoundingCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AWBRoundingRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.AWBRoundingRegistryItemEditor, Enterprise.Registry.GUI")]
	class AWBRoundingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AWBRoundingCollection>
	{
		public AWBRoundingRegistryDataType()
		{
		}
	}
}
