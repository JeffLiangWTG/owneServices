using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobClosureConfigurationRegistryItem : StronglyTypedRegistryItem<JobClosureConfigurationHeader>
	{
		public JobClosureConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, object defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new JobClosureConfigurationRegistryDataType(), storage, option, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.JobClosureConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class JobClosureConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JobClosureConfigurationHeader>
	{
		public JobClosureConfigurationRegistryDataType()
		{
		}
	}
}
