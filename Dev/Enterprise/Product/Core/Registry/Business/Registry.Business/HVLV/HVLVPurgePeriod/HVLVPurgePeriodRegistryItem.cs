using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class HVLVPurgePeriodRegistryItem : StronglyTypedRegistryItem<HVLVPurgePeriod>
	{
		public HVLVPurgePeriodRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new HVLVPurgePeriodRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.HVLVPurgePeriodRegistryItemEditor, Enterprise.Registry.GUI")]
	public class HVLVPurgePeriodRegistryDataType : NonPersistentBusinessObjectRegistryDataType<HVLVPurgePeriod> { }
}
