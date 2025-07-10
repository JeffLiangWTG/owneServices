using System;
using CargoWise.Pipes;

namespace Enterprise.VisualBoards.Business
{
	public interface ISubscribableDispatcher : IDispatcher
	{
		bool TryAdd(string key, Func<IDisposable> subscription);
	}
}
