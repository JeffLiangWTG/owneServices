namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusContainerValidationTest : Customs.Business.Testing.CusContainerValidationTest<JobDeclaration>
	{
		public void TestParent()
		{
			CusContainer parent = Factory.New<CusContainer>();
			AssertEquals(parent.Validation.Parent, parent);
		}
	}
}
