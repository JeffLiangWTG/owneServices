using System.Collections.Concurrent;

namespace Enterprise.Builder.Generator.Cache
{
	public abstract class GenericConcurrentCache<T>
	{
		protected GenericConcurrentCache()
		{
			Cache = new ConcurrentDictionary<string, T>();
		}

		protected T Get(string id)
		{
			return Cache.GetOrAdd(id, s => GeneratorFunc(id));
		}

		protected abstract T GeneratorFunc(string id);

		protected ConcurrentDictionary<string, T> Cache { get; private set; }
	}
}
