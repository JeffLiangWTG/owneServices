using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	#region Registry Item

	public class HVLVPreScreeningRuleRegistryItem : StronglyTypedRegistryItem<HVLVDetailsPreScreeningConfiguration>
	{
		public HVLVPreScreeningRuleRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions regOptions)
			: base(new RegistryItemImpl(name, category, caption, hint, new HVLVPreScreeningRuleRegistryDataType(), storage, regOptions))
		{
		}
	}

	#endregion

	#region Data Type

	[RegistryEditor("Enterprise.Registry.GUI.HVLVPreScreeningRuleRegistryItemEditor, Enterprise.Registry.GUI")]
	public class HVLVPreScreeningRuleRegistryDataType : NonPersistentBusinessObjectRegistryDataType<HVLVDetailsPreScreeningConfiguration>
	{
		public HVLVPreScreeningRuleRegistryDataType()
		{
		}
	}

	#endregion
}
