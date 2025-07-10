namespace CargoWise.EntityFramework.Testing
{
	[SingleObjectAroundARow]
	sealed class SingleObjectAroundARowTest : TestCaseWithFactory
	{
		public void TestHasAttribute()
		{
			AssertEquals(true, SingleObjectAroundARow.HasAttribute(typeof(SingleObjectAroundARowTest)));
			AssertEquals(false, SingleObjectAroundARow.HasAttribute(typeof(object)));
		}
	}
}
