using System;
using System.Threading;
using CargoWise.Common;

namespace CargoWise.Async
{
	public static class MainThreadRunner
	{
		public static void RunOnMainThread(Action action)
		{
			Argument.NotNull(action, nameof(action));
			Argument.NotNull(ApplicationDispatcher.Current, nameof(ApplicationDispatcher.Current));

			if (Thread.CurrentThread.IsBackground)
			{
				GetStrategy().BeginInvoke(action);
			}
			else
			{
				action();
			}
		}

		public static T RunOnMainThreadSync<T>(Func<T> func)
		{
			Argument.NotNull(func, nameof(func));
			Argument.NotNull(ApplicationDispatcher.Current, nameof(ApplicationDispatcher.Current));

			if (Thread.CurrentThread.IsBackground)
			{
				return GetStrategy().Invoke(func);
			}
			else
			{
				return func();
			}
		}

		static IMainThreadInvocationStrategy GetStrategy()
		{
			var strategy = InvocationStrategy.Value
				?? throw new InvalidOperationException("Why is this being overridden to null?");

			return strategy;
		}

		public static IDisposable OverrideInvocationStrategy(IMainThreadInvocationStrategy strategy)
		{
			InvocationStrategy.Value = strategy;

			return new DisposableAction(() => InvocationStrategy.ResetValue());
		}

		public static readonly LazyOverridable<IMainThreadInvocationStrategy> InvocationStrategy = new LazyOverridable<IMainThreadInvocationStrategy>(() => new DefaultMainThreadInvocationStrategy());

		public static void RequireMainThread(this Thread thread)
		{
			if (thread != ApplicationDispatcher.MainThread)
			{
				throw new CrossThreadAccessException("This code path must be accessed on the main thread only");
			}
		}
	}
}
