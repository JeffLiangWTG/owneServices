namespace CargoWise.EntityFramework.Testing
{
	[DoNotCacheRowFilterComparer]
	sealed class DoNotCacheRowFilterComparerAttributeTest : TestCaseWithFactory
	{
		public void TestHasAttribute()
		{
			AssertEquals(true, DoNotCacheRowFilterComparerAttribute.HasAttribute(typeof(DoNotCacheRowFilterComparerAttributeTest)));
			AssertEquals(false, DoNotCacheRowFilterComparerAttribute.HasAttribute(typeof(object)));
		}
	}
}
