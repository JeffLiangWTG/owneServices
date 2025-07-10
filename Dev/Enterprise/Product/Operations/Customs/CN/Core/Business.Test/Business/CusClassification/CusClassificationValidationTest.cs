namespace Enterprise.Customs.CN.Business.Testing
{
	class CusClassificationValidationTest : Customs.Business.Testing.CusClassificationValidationTest
	{
		public void TestParent()
		{
			var parent = Factory.New<CusClassification>();
			AssertEquals(parent.Validation.Parent, parent);
		}
	}
}
