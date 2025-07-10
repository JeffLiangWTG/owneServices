using System;
using System.Threading;

namespace CargoWise.Async.Test
{
	public class MainThreadInvocationStrategyWithCallback : DefaultMainThreadInvocationStrategy
	{
		public Action CallbackOnMessagePosted { get; set; }

		protected override T Invoke<T>(Func<T> func)
		{
			T result = default(T);
			using (var funcExecuted = new AutoResetEvent(false))
			{
				base.BeginInvoke(() =>
				{
					result = func();
					funcExecuted.Set();
				});
				CallbackOnMessagePosted?.Invoke(); // allow to execute external code such as processing the message pump (Application.DoEvents()) precisely after puting the method call into the pump
				funcExecuted.WaitOne();
				return result;
			}
		}
	}
}
