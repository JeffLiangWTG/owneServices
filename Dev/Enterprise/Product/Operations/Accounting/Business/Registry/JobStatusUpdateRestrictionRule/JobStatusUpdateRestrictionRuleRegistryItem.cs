using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobStatusUpdateRestrictionRuleRegistryItem : StronglyTypedRegistryItem<JobStatusUpdateRestrictionRuleCollection>
	{
		public JobStatusUpdateRestrictionRuleRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, JobStatusUpdateRestrictionRuleCollection defaultValue, RegistryOptions options = RegistryOptions.Default)
			: base(new RegistryItemImpl(name, category, caption, hint, new JobStatusUpdateRestrictionRuleRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.JobStatusUpdateRestrictionRuleRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class JobStatusUpdateRestrictionRuleRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JobStatusUpdateRestrictionRuleCollection>
	{
		public JobStatusUpdateRestrictionRuleRegistryDataType()
		{
		}
	}
}
