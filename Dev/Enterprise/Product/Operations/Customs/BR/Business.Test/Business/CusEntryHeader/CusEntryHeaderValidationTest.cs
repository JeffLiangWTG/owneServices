namespace Enterprise.Customs.BR.Business.Testing
{
	partial class CusEntryHeaderValidationTest : Customs.Business.Testing.CusEntryHeaderValidationTest
	{
		public void TestParent()
		{
			CusEntryHeader parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Validation.Parent, parent);
		}
	}
}
