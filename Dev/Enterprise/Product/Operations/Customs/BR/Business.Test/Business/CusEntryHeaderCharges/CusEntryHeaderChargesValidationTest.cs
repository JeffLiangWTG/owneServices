using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusEntryHeaderChargesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEntryHeaderCharges()
		{
			CusEntryHeaderCharges parent = Factory.New<CusEntryHeaderCharges>();
			AssertEquals(parent.Validation.EntryHeaderCharges, parent);
		}
	}
}
