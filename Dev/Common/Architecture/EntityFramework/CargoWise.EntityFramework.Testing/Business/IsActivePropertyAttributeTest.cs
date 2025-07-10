namespace CargoWise.EntityFramework.Testing
{
	sealed class IsActivePropertyAttributeTest : TestCaseWithFactory
	{
		public void TestIsActiveAttribute()
		{
			AssertEquals("Z0_Bool", IsActivePropertyAttribute.IsActivePropertyName(typeof(DummyBusinessObjectWithActiveProperty)));
		}
	}
}
