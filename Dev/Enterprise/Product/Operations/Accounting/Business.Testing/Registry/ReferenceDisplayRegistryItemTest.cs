using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ReferenceDisplayRegistryItem))]
	class ReferenceDisplayRegistryItemTest : StronglyTypedRegistryItemTestCase<bool>
	{
		protected override StronglyTypedRegistryItem<bool, bool> GetNewRegistryItem()
		{
			return new ReferenceDisplayRegistryItem(string.Empty, null, null);
		}
	}
}
