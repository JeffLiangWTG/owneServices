using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEntryLine()
		{
			var parent = Factory.New<CusEntryLine>();
			AssertEquals(parent.Validation.EntryLine, parent);
		}
	}
}
