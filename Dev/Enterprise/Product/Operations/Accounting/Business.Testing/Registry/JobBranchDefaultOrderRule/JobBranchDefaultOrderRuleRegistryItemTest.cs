using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobBranchDefaultOrderRuleRegistryItem))]
	class JobBranchDefaultOrderRuleRegistryItemTest : StronglyTypedRegistryItemTestCase<IJobBranchDefaultOrderRule, JobBranchDefaultOrderRule>
	{
		protected override StronglyTypedRegistryItem<IJobBranchDefaultOrderRule, JobBranchDefaultOrderRule> GetNewRegistryItem()
		{
			return new JobBranchDefaultOrderRuleRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
