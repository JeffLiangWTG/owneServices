using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(BillingSystemChargeCodeMappingRegistryItem))]
	class BillingSystemChargeCodeMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<BillingSystemChargeCodeMappingCollection>
	{
		protected override StronglyTypedRegistryItem<BillingSystemChargeCodeMappingCollection, BillingSystemChargeCodeMappingCollection> GetNewRegistryItem()
		{
			return new BillingSystemChargeCodeMappingRegistryItem("BillingSystemChargeCodeMappings", (NoResString)"Dummy Category", (NoResString)"Dummy Caption", null);
		}
	}
}
