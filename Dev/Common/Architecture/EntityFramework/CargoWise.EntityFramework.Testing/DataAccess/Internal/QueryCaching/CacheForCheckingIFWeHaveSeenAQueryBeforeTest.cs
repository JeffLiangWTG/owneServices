using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DiamondCacheTestCase : TestCase
	{
		CacheForCheckingIFWeHaveSeenAQueryBefore Cache { get; } = new CacheForCheckingIFWeHaveSeenAQueryBefore();

		public void TestAdd()
		{
			Cache.AddQueryToCache("Harry");
			Assert(Cache.Contains("Harry"));
			Assert(!Cache.Contains("Steve"));
			Assert(!Cache.Contains(""));
		}

		public void TestCaseInsensitive()
		{
			Cache.AddQueryToCache("Harry");
			Assert(Cache.Contains("harRY"));
		}

		public void TestWithBlob()
		{
			var blob1 = "BLOB1";
			var blob2 = "BLOB2";

			Cache.AddQueryToCache("Harry", blob1, blob2);
			Assert(Cache.Contains("Harry"));
			Assert(Cache.Contains("Harry", blob1));
			Assert(Cache.Contains("Harry", blob2));
			Assert(Cache.Contains("Harry", blob1, blob2));
			Assert(Cache.Contains("Harry", blob2, blob1));

			Assert(!Cache.Contains("Steve", blob2, blob1));
			Assert(!Cache.Contains("", blob2, blob1));
		}

		public void TestEmptyMeansMatchEveryThing()
		{
			var blob1 = "Im a robot";
			var blob2 = "So is pat";
			Cache.AddQueryToCache(string.Empty);

			Assert(Cache.Contains("Harry"));
			Assert(Cache.Contains("Virginia"));
			Assert(!Cache.Contains("Virginia", blob1));
			Assert(!Cache.Contains("Virginia", blob2));
			Assert(!Cache.Contains("Virginia", blob2, blob1));
		}

		public void TestEmptyWithBlob()
		{
			var blob1 = "Im a robot";
			var blob2 = "So is pat";
			Cache.AddQueryToCache(string.Empty, blob1);

			Assert(Cache.Contains("Harry"));
			Assert(Cache.Contains("Virginia"));
			Assert(Cache.Contains("Virginia", blob1));
			Assert(!Cache.Contains("Virginia", blob2));
			Assert(!Cache.Contains("Virginia", blob2, blob1));
		}

		public void TestEmptyWithBlob_Multiple()
		{
			var blob1 = "Im a robot";
			var blob2 = "So is pat";
			Cache.AddQueryToCache(string.Empty, blob1, blob2);

			Assert(Cache.Contains("Harry"));
			Assert(Cache.Contains("Virginia"));
			Assert(Cache.Contains("Virginia", blob1));
			Assert(Cache.Contains("Virginia", blob2));
			Assert(Cache.Contains("Virginia", blob2, blob1));
		}

		public void TestMatchesWithBlobSplit()
		{
			var blob1 = "Im a robot";
			var blob2 = "So is pat";
			var blob3 = "So mych blub";
			Cache.AddQueryToCache(string.Empty, blob1);
			Cache.AddQueryToCache("Granny", blob2);

			Assert(Cache.Contains("Granny"));
			Assert(Cache.Contains("Granny", blob1));
			Assert(Cache.Contains("Granny", blob2));
			Assert(!Cache.Contains("Granny", blob3));
			Assert(Cache.Contains("Tim", blob1));
			Assert(!Cache.Contains("Tim", blob2));
		}
	}
}
