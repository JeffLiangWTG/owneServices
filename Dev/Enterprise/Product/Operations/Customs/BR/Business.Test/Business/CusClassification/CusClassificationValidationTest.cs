namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusClassificationValidationTest : Customs.Business.Testing.CusClassificationValidationTest
	{
		public void TestParent()
		{
			CusClassification parent = Factory.New<CusClassification>();
			AssertEquals(parent.Validation.Parent, parent);
		}
	}
}
