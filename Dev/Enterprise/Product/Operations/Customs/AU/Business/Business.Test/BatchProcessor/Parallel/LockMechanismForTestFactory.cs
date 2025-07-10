using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class LockMechanismForTestFactory
	{
		readonly Dictionary<string, string> lockOwnersByKey = new Dictionary<string, string>();
		readonly object innerLock = new object();
		readonly Action<string> logger;

		public LockMechanismForTestFactory() : this(s => { })
		{
		}

		public LockMechanismForTestFactory(Action<string> logger)
		{
			this.logger = logger;
		}

		public ILockMechanism Create(string owner)
		{
			return new LockMechanismForTest(this, owner);
		}

		public delegate void BeforeLockEventHandler(string owner, string key);

		public event BeforeLockEventHandler OnBeforeLock;

		bool Lock(string key, string owner)
		{
			OnBeforeLock?.Invoke(owner, key);

			lock (innerLock)
			{
				if (lockOwnersByKey.TryGetValue(key, out var currentOwner))
				{
					if (currentOwner != owner)
					{
						logger(FormattableString.Invariant($"Lock rejected: key='{key}', owner='{owner}', currentOwner='{currentOwner}'."));
						return false;
					}

					throw new InvalidOperationException(FormattableString.Invariant(
						$"Avoid locking same key multiple times): key='{key}', owner='{owner}'."
					));
				}

				logger(FormattableString.Invariant($"Lock acquired: key='{key}', owner='{owner}'."));
				lockOwnersByKey.Add(key, owner);

				return true;
			}
		}

		void Release(string key, string owner)
		{
			lock (innerLock)
			{
				if (!lockOwnersByKey.TryGetValue(key, out var currentOwner) || currentOwner != owner)
				{
					throw new InvalidOperationException(FormattableString.Invariant(
						$"Cannot release: key='{key}', owner='{owner}'."
					));
				}

				logger(FormattableString.Invariant($"Lock released: key='{key}', owner='{owner}'."));
				lockOwnersByKey.Remove(key);
			}
		}

		sealed class LockMechanismForTest : ILockMechanism
		{
			readonly LockMechanismForTestFactory factory;
			readonly string owner;

			public LockMechanismForTest(LockMechanismForTestFactory factory, string owner)
			{
				this.factory = factory;
				this.owner = owner;
			}

			public bool TryGetLock(string key, out IDisposable appLock)
			{
				if (factory.Lock(key, owner))
				{
					appLock = new DisposableAction(() => factory.Release(key, owner));
					return true;
				}

				appLock = null;
				return false;
			}
		}
	}
}
