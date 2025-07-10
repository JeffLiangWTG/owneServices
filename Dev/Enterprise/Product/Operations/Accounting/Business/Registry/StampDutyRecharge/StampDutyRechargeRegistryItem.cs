using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class StampDutyRechargeRegistryItem : StronglyTypedRegistryItem<StampDutyRecharge>
	{
		public StampDutyRechargeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new StampDutyRechargeRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.StampDutyRechargeRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class StampDutyRechargeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<StampDutyRecharge>
	{
	}
}
