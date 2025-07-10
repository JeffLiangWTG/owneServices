using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEntryLine()
		{
			CusEntryLine parent = Factory.New<CusEntryLine>();
			AssertEquals(parent.Validation.EntryLine, parent);
		}
	}
}
