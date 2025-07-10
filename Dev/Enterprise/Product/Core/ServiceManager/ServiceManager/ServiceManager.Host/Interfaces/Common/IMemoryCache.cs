using System;

namespace Enterprise.ServiceManager.Host
{
	public interface IMemoryCache
	{
		void Set<T>(string key, T value, TimeSpan expiration);
		T Get<T>(string key);
		T AddOrGetExisting<T>(string key, Func<T> function, TimeSpan expiration);
		T AddOrGetExisting<T>(string key, Func<T> function, Func<TimeSpan> expiration);
	}
}
