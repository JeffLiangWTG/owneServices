using System;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	abstract class CachedValueTestCase<T> : TestCase
			where T : CachedValue<int>
	{
		public void TestValueNotCachedOnException()
		{
			var cachedValue = new CachedValue<int>(() => { throw new NotImplementedException(); });

			AssertExceptionThrown<NotImplementedException>(() =>
			{
				int x = cachedValue.Value;
			});

			AssertExceptionThrown<NotImplementedException>(() =>
			{
				int x = cachedValue.Value;
			});
		}

		public void TestCachedValue()
		{
			int x = 0;
			CachedValue<int> intCache = new CachedValue<int>(() => x);

			x = 5;
			AssertEquals(5, intCache.Value);

			x = 10;
			AssertEquals(5, intCache.Value);
		}
	}
}
