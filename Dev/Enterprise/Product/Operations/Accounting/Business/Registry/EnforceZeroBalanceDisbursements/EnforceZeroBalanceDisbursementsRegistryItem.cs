using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class EnforceZeroBalanceDisbursementsRegistryItem : StronglyTypedRegistryItem<EnforceZeroBalanceDisbursementsConfiguration>
	{
		public EnforceZeroBalanceDisbursementsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new RegistryItemImpl(name, category, caption, hint, new EnforceZeroBalanceDisbursementsRegistryDataType(), storage, option))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.EnforceZeroBalanceDisbursementsRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class EnforceZeroBalanceDisbursementsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EnforceZeroBalanceDisbursementsConfiguration>
	{
		public EnforceZeroBalanceDisbursementsRegistryDataType()
		{
		}
	}
}