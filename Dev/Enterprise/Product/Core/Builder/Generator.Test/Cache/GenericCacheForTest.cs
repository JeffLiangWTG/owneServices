using System.Collections.Concurrent;

namespace Enterprise.Builder.Generator.Cache.Testing
{
	sealed class GenericCacheForTest : GenericConcurrentCache<string>
	{
		public string GetSomethingForTest(string id)
		{
			return Get(id);
		}

		protected override string GeneratorFunc(string id)
		{
			return id;
		}

		public ConcurrentDictionary<string, string> ExposedCache
		{
			get
			{
				return Cache;
			}
		}
	}
}
