using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitConsignmentPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCusExitContainers()
		{
			(var pivot, _, _, var header) = CusExitConsignmentPivotTest.GetNewBusinessObject(Factory);
			AssertSame("CusExitContainers", header.CusExitContainers, pivot.Lookups.CusExitContainers);
		}
	}
}
