namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusContainerValidationTest : Customs.Business.Testing.CusContainerValidationTest<JobDeclaration>
	{
		public void TestParent()
		{
			var parent = Factory.New<CusContainer>();
			AssertEquals(parent.Validation.Parent, parent);
		}
	}
}
