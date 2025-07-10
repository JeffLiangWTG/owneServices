using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.JP.Business.Testing
{
	sealed class GroupInvoiceChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeDistributeBy()
		{
			var parent = Factory.New<GroupInvoiceCharge>();

			Assert(parent.Lookups.ChargeDistributionBy.ContainsCode(ChargeDistributeByList.Codes.Value));
			Assert(parent.Lookups.ChargeDistributionBy.ContainsCode(ChargeDistributeByList.Codes.Quantity));

			parent.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;

			Assert(!parent.Lookups.ChargeDistributionBy.ContainsCode(ChargeDistributeByList.Codes.Value));
			Assert(!parent.Lookups.ChargeDistributionBy.ContainsCode(ChargeDistributeByList.Codes.Quantity));
		}
	}
}
