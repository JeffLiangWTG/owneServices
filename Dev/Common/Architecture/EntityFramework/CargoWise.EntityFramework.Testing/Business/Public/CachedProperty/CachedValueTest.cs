using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class CachedValueTest : CachedValueTestCase<CachedValue<int>>
	{
		public void TestGetValue()
		{
			CachedValue<ZString> cachedValue = null;
			var i = 0;
			GetValueDelegate<ZString> getValue = () => (ZString)(++i).ToString();
			AssertEquals("1", CachedValueHelper.GetValue(ref cachedValue, getValue));
			AssertEquals("1", CachedValueHelper.GetValue(ref cachedValue, getValue));
			AssertNotNull(cachedValue);
			AssertEquals("1", cachedValue.Value);
			var cachedValue1 = cachedValue;
			cachedValue = null;
			AssertEquals("2", CachedValueHelper.GetValue(ref cachedValue, getValue));
			AssertEquals("2", CachedValueHelper.GetValue(ref cachedValue, getValue));
			AssertNotNull(cachedValue);
			AssertEquals("2", cachedValue.Value);
			AssertEquals(false, object.ReferenceEquals(cachedValue1, cachedValue));
		}
	}
}
