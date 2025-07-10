using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusEntryHeaderChargesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEntryHeaderCharges()
		{
			var parent = Factory.New<CusEntryHeaderCharges>();
			AssertEquals(parent.Validation.EntryHeaderCharges, parent);
		}
	}
}
