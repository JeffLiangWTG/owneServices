using System;
using System.Collections.Generic;
using System.Threading;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Common
{
	[ThreadSafe]
	public abstract class Overridable
	{
		private protected Overridable()
		{
		}

		public abstract void ResetValue();

		int registered; // using int intead of bool to so we can Interlocked.Exchange()

		protected void RegisterOverridden(Overridable item)
		{
			if (Interlocked.Exchange(ref registered, 1) == 0)
			{
				overridden.UpdateValue((Overridable[] current) => ArrayUtil.Append(current, item));
			}
		}

		public static void ResetAll()
		{
			overridden.GetValue().ForEach((Overridable item) =>
				{
					item.registered = 0;
					item.ResetValue();
				});
			overridden.ResetUnsafe(Empty);
		}

		static Overridable[] Empty => Array.Empty<Overridable>();
		protected static readonly CopyOnWrite<Overridable[]> overridden = new CopyOnWrite<Overridable[]>(Empty);
	}

	[ThreadSafe]
	public abstract class OverridableBase<T> : Overridable
	{
		private protected OverridableBase()
		{
			valueLock = new SpinLock();
		}

		T currentValue;

		// Do not mark this field as readonly. If you do, the compiler will take a copy of it when it attempts to mutate the struct
		// (i.e. take the lock, release the lock), and since everyone trying to take the lock is locking on their own private copy,
		// the lock becomes completely ineffective.
		// See:
		// > It is also important to note that SpinLock is a value type, for performance reasons. For this reason, you must be very
		// > careful not to accidentally copy a SpinLock instance, as the two instances (the original and the copy) would then be
		// > completely independent of one another, which would likely lead to erroneous behavior of the application. If a SpinLock
		// > instance must be passed around, it should be passed by reference rather than by value.
		// > Do not store SpinLock instances in readonly fields."
		// - https://docs.microsoft.com/en-us/dotnet/api/system.threading.spinlock#remarks
		SpinLock valueLock;

		protected T ReadCurrentValue()
		{
			var lockTaken = false;
			valueLock.Enter(ref lockTaken);
			try
			{
				return currentValue;
			}
			finally
			{
				if (lockTaken)
				{
					valueLock.Exit();
				}
			}
		}

		protected void WriteCurrentValue(T newValue)
		{
			var lockTaken = false;
			valueLock.Enter(ref lockTaken);
			try
			{
				currentValue = newValue;
			}
			finally
			{
				if (lockTaken)
				{
					valueLock.Exit();
				}
			}
		}

		protected T ExchangeCurrentValue(T newValue)
		{
			var lockTaken = false;
			valueLock.Enter(ref lockTaken);
			try
			{
				var oldValue = currentValue;
				currentValue = newValue;
				return oldValue;
			}
			finally
			{
				if (lockTaken)
				{
					valueLock.Exit();
				}
			}
		}
	}

	[ThreadSafe]
	public sealed class Overridable<T> : OverridableBase<T>
	{
		public Overridable()
			: this(default(T))
		{
		}

		public Overridable(T defaultValue)
		{
			this.defaultValue = defaultValue;
			WriteCurrentValue(defaultValue);
		}

		public bool DisposeIfIDisposable { get; set; } = true;

		public T Value
		{
			get => ReadCurrentValue();
			set
			{
				RegisterOverridden(this);
				WriteCurrentValue(value);
			}
		}

		public bool IsOverriden => !EqualityComparer<T>.Default.Equals(ReadCurrentValue(), defaultValue);

		public override void ResetValue()
		{
			if (IsOverriden && DisposeIfIDisposable)
			{
				var prevValue = ExchangeCurrentValue(defaultValue);
				if (prevValue is IDisposable disposable && !ReferenceEquals(prevValue, defaultValue))
				{
					disposable.Dispose();
				}
			}
			else
			{
				WriteCurrentValue(defaultValue);
			}
		}

		readonly T defaultValue;
	}
}
