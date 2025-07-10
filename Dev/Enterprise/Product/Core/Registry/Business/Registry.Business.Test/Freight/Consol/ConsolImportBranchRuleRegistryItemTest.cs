using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ConsolImportBranchRuleRegistryItem))]
	sealed class ConsolImportBranchRuleRegistryItemTest : StronglyTypedRegistryItemTestCase<ImportBranchRule>
	{
		protected override StronglyTypedRegistryItem<ImportBranchRule, ImportBranchRule> GetNewRegistryItem()
		{
			return new ConsolImportBranchRuleRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new ImportBranchRule());
		}
	}
}
