using System;
using System.Collections.Generic;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine
{
	public class ArchiveSystemCache : IArchiveSystemCache
	{
		#region IArchiveSystemCache Members

		public void Add<T>(T cacheObject)
			=> cache.Add(typeof(T), cacheObject);

		public T Retrieve<T>()
			=> cache.ContainsKey(typeof(T))
				? (T)cache[typeof(T)]
				: default;

		#endregion

		readonly Dictionary<Type, object> cache = new Dictionary<Type, object>();
	}
}
