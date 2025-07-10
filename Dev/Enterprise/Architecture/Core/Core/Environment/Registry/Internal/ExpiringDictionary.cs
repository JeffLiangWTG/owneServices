using System;
using System.Collections.Concurrent;
using CargoWise.Application;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Environment
{
	class ExpiringConcurrentDictionary<TKey, TValue>
	{
		public ExpiringConcurrentDictionary(TimeSpan itemExpiry)
		{
			inner = new ConcurrentDictionary<TKey, TValue>();
			this.itemExpiry = itemExpiry;

#if DEBUG
			stopwatch = ObjectFactory.Get<IStopwatch>();
#else   // SuppressCodeSmell Reason = Evil but needed for performance                                                                
			stopwatch = new UberWatch();
#endif
			stopwatch.Start();
		}
		readonly ConcurrentDictionary<TKey, TValue> inner;
		readonly TimeSpan itemExpiry;
		readonly IStopwatch stopwatch;

		public void Add(TKey key, TValue value)
		{
			inner.AddOrUpdate(key, _ => value, (_, __) => value);
		}

		public void Clear()
		{
			inner.Clear();
		}

		public bool Contains(TKey key)
		{
			return TryGetValue(key, out _);
		}

		public void Remove(TKey key)
		{
			inner.TryRemove(key, out _);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			if (inner.TryGetValue(key, out value))
			{
				if (stopwatch.ElapsedMilliseconds < itemExpiry.TotalMilliseconds)
				{
					return true;
				}
				else
				{
					Clear();
					stopwatch.Restart();
				}
			}

			value = default;
			return false;
		}
	}
}
