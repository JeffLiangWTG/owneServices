using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class WatermarkRegistryItem : StronglyTypedRegistryItem<Watermark>
	{
		public WatermarkRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new WatermarkRegistryDataType(), storage))
		{
		}

		public WatermarkRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new WatermarkRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.WatermarkRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
	class WatermarkRegistryDataType : NonPersistentBusinessObjectRegistryDataType<Watermark>
	{
		public WatermarkRegistryDataType()
		{
		}
	}
}
