using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;

namespace CargoWise.Async
{
	public class DefaultMainThreadInvocationStrategy : IMainThreadInvocationStrategy
	{
		[DebuggerStepThrough]
		void IMainThreadInvocationStrategy.BeginInvoke(Action action) => BeginInvoke(action);

		[DebuggerStepThrough]
		T IMainThreadInvocationStrategy.Invoke<T>(Func<T> func) => Invoke(func);

		[SuppressMessage("Microsoft.Contracts", "Nonnull-11-0")]
		protected void BeginInvoke(Action action) => ApplicationDispatcher.Current.BeginInvoke(action);

		[SuppressMessage("Microsoft.Contracts", "Nonnull-6-0")]
		protected virtual T Invoke<T>(Func<T> func) => ApplicationDispatcher.Current.Invoke(func);
	}
}
