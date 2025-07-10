using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(DiscountSuspensionPolicyRegistryItem))]
	class DiscountSuspensionPolicyRegistryItemTest : StronglyTypedRegistryItemTestCase<DiscountSuspensionPolicyCollection>
	{
		protected override StronglyTypedRegistryItem<DiscountSuspensionPolicyCollection, DiscountSuspensionPolicyCollection> GetNewRegistryItem()
		{
			return new DiscountSuspensionPolicyRegistryItem("DiscountSuspensionPolicies", (NoResString)"Dummy Category", (NoResString)"Dummy Caption", null, RegistryStorageFlags.Company, new DiscountSuspensionPolicyCollection());
		}
	}
}
