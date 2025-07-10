using System;

namespace CargoWise.Async.Test
{
	public class NonBlockingMainThreadInvocationStrategy : DefaultMainThreadInvocationStrategy
	{
		protected override T Invoke<T>(Func<T> func)
		{
			base.BeginInvoke(() => func());
			return default(T);
		}
	}
}
