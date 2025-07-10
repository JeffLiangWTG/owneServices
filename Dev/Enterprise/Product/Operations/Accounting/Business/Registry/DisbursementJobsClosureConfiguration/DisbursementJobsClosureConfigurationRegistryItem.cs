using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class DisbursementJobsClosureConfigurationRegistryItem : StronglyTypedRegistryItem<IDisbursementJobsClosureConfiguration, DisbursementJobsClosureConfiguration>
	{
		public DisbursementJobsClosureConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new RegistryItemImpl(name, category, caption, hint, new DisbursementJobsClosureConfigurationRegistryDataType(), storage, option))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.DisbursementJobsClosureConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class DisbursementJobsClosureConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DisbursementJobsClosureConfiguration>
	{
	}
}
