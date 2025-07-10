namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
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
