using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.H7.Module.Testing
{
	sealed class GBH7BillFilterBusinessObjectLookupsTest : TestCaseWithFactory
	{
		public void TestCustomsStatusList()
		{
			var parent = new GBH7BillFilterBusinessObject();
			var lookups = new GBH7BillFilterBusinessObjectLookups(parent);

			AssertEquals("ACC, RCV, CTL, DOC, TAX, CLR, CAN", lookups.CustomsStatusList.CodesAsString);
		}
	}
}
