using System;

namespace CargoWise.Async
{
	public interface IMainThreadInvocationStrategy
	{
		void BeginInvoke(Action action);
		T Invoke<T>(Func<T> func);
	}
}
