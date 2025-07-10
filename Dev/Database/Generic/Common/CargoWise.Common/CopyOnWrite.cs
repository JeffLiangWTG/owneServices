using System;
using System.Threading;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Common
{
	[ThreadSafe]
	public class CopyOnWrite<T> where T : class
	{
		public CopyOnWrite()
			: this(default(T))
		{
		}

		public CopyOnWrite(T value)
		{
			this.value = value;
		}

		public T GetValue()
		{
			return value;
		}

		public void ResetUnsafe(T resetValue)
		{
			this.value = resetValue;
		}

		public void UpdateValue(Func<T, T> operation)
		{
			Argument.NotNull(operation, nameof(operation));
			var original = this.value;
			if (Interlocked.CompareExchange(ref value, operation(original), original) != original)
			{
				var spinner = new SpinWait();
				do
				{
					spinner.SpinOnce();
					original = this.value;
				}
				while (Interlocked.CompareExchange(ref value, operation(original), original) != original);
			}
		}

		T value;
	}
}
