using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(BillingDbUsageCodesRegistryItem))]
	class BillingDbUsageCodesRegistryItemTest : StronglyTypedRegistryItemTestCase<BillingDbUsageCodesCollection>
	{
		protected override StronglyTypedRegistryItem<BillingDbUsageCodesCollection, BillingDbUsageCodesCollection> GetNewRegistryItem()
		{
			return new BillingDbUsageCodesRegistryItem("BillingDbUsageCodes", (NoResString)"Dummy Category", (NoResString)"Dummy Caption", null);
		}
	}
}
