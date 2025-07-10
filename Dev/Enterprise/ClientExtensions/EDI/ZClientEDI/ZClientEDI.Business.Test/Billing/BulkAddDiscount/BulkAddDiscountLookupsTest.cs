using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class BulkAddDiscountLookupsTest : TestCaseWithFactory
	{
		public void TestLookups()
		{
			var newDiscount = new BulkAddDiscount();
			var lookups = new BulkAddDiscountLookups(newDiscount, Factory);

			AssertNotNull(lookups.SystemCodes);
			AssertNotNull(lookups.SubCodes);
			AssertNotNull(lookups.DiscountTypes);
			AssertNotNull(lookups.ModuleCodes);
			AssertNotNull(lookups.BreakUnits);
		}
	}
}