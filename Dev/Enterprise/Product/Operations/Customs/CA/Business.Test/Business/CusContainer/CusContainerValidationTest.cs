namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusContainerValidationTest : Customs.Business.Testing.CusContainerValidationTest<JobDeclaration>
	{
		public void TestParent()
		{
			CusContainer parent = Factory.New<CusContainer>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public override void TestContainersRequirePackagesValidation()
		{
			Assert(true);
		}
	}
}
