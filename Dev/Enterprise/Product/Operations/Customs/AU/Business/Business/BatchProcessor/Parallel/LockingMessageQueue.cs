using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class LockingMessageQueue : IDisposable
	{
		readonly ILockMechanism lockMechanism;
		readonly List<IDisposable> appLocks = new List<IDisposable>();

		readonly HashSet<string> keysAffectedByUs = new HashSet<string>();
		readonly HashSet<string> keysAffectedByOthers = new HashSet<string>();

		bool disposed;

		public LockingMessageQueue(ILockMechanism lockMechanism)
		{
			this.lockMechanism = lockMechanism;
		}

		public bool Enqueue(MessageKeySet messageKeys)
		{
			if (disposed)
			{
				throw new InvalidOperationException(FormattableString.Invariant(
					$"Cannot enqueue messages into a {nameof(LockingMessageQueue)} after it was disposed."
				));
			}

			// All the keys, that are affected by previous messages, are:
			// - either in keysAffectedByUs and locked by us;
			// - or in keysAffectedByOthers and locked by another consumer.

			if (messageKeys.DependsOn.Any(k => keysAffectedByOthers.Contains(k)))
			{
				keysAffectedByOthers.UnionWith(messageKeys.Affects);
				return false;
			}

			if (!Lock(messageKeys.Affects))
			{
				keysAffectedByOthers.UnionWith(messageKeys.Affects);
				return false;
			}

			keysAffectedByUs.UnionWith(messageKeys.Affects);
			return true;
		}

		public void Dispose()
		{
			if (disposed)
			{
				return;
			}

			disposed = true;

			foreach (var appLock in appLocks)
			{
				appLock.Dispose();
			}
		}

		bool Lock(IEnumerable<string> keys)
		{
			// It is important to always lock multiple keys in same order to avoid deadlocks.
			var keysToLock = new SortedSet<string>(keys.Where(k => !keysAffectedByUs.Contains(k)));

			var newAppLocks = new List<IDisposable>();

			try
			{
				foreach (var key in keysToLock)
				{
					if (lockMechanism.TryGetLock(key, out var appLock))
					{
						newAppLocks.Add(appLock);
					}
					else
					{
						return false;
					}
				}

				appLocks.AddRange(newAppLocks);
				newAppLocks = null;
				return true;
			}
			finally
			{
				if (newAppLocks != null)
				{
					foreach (var appLock in newAppLocks)
					{
						appLock.Dispose();
					}
				}
			}
		}
	}
}
