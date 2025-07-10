using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(StampDutyRechargeRegistryItem))]
	class StampDutyRechargeRegistryItemTest : StronglyTypedRegistryItemTestCase<StampDutyRecharge>
	{
		protected override StronglyTypedRegistryItem<StampDutyRecharge, StampDutyRecharge> GetNewRegistryItem()
		{
			return new StampDutyRechargeRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
