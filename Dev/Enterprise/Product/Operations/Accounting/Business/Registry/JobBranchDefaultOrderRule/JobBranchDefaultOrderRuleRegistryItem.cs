using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobBranchDefaultOrderRuleRegistryItem : StronglyTypedRegistryItem<IJobBranchDefaultOrderRule, JobBranchDefaultOrderRule>
	{
		public JobBranchDefaultOrderRuleRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new JobBranchDefaultOrderRuleRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.JobBranchDefaultOrderRuleRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class JobBranchDefaultOrderRuleRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JobBranchDefaultOrderRule>
	{
		public JobBranchDefaultOrderRuleRegistryDataType()
		{
		}
	}
}
