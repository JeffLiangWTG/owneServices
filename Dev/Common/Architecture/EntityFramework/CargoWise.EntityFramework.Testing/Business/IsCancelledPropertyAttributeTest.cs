namespace CargoWise.EntityFramework.Testing
{
	sealed class IsCancelledPropertyAttributeTest : TestCaseWithFactory
	{
		public void TestIsActiveAttribute()
		{
			AssertEquals("Z0_Bool", IsCancelledPropertyAttribute.IsCancelledPropertyName(typeof(DummyBusinessObjectWithCancelledProperty)));
		}
	}
}
