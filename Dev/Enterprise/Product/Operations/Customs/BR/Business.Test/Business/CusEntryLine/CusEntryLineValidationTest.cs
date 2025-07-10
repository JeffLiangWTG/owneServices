using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEntryLine()
		{
			CusEntryLine parent = Factory.New<CusEntryLine>();
			AssertEquals(parent.Validation.EntryLine, parent);
		}
	}
}
