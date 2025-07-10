namespace CargoWise.EntityFramework.Testing
{
	sealed class RecalculableCachedValueTest : CachedValueTestCase<RecalculableCachedValue<int>>
	{
		public void TestNeedRecals()
		{
			int x = 0;
			RecalculableCachedValue<int> intCache = new RecalculableCachedValue<int>(() => ++x);

			AssertEquals(1, intCache.Value);
			AssertEquals(1, intCache.Value);
			AssertEquals(1, intCache.Value);

			intCache.InvalidateCache();

			AssertEquals(2, intCache.Value);
			AssertEquals(2, intCache.Value);
			AssertEquals(2, intCache.Value);
		}
	}
}
