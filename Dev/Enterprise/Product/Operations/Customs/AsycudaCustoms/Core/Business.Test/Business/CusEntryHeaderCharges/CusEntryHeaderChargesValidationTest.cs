using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
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
