using System;
using CargoWise.Pipes;

namespace Enterprise.VisualBoards.Business
{
	public class BoardReloadOperation : IBoardRefreshContext
	{
		public BoardReloadOperation(ISubscribableDispatcher dispatcher)
		{
			Dispatcher = dispatcher;
		}

		ISubscribableDispatcher Dispatcher { get; }

		public BoardRefreshType RefreshType
		{
			get { return BoardRefreshType.Reload; }
		}

		public void AddDisposableSubscription(string key, Func<IDispatcher, IDisposable> addSubscription)
		{
			Dispatcher.TryAdd(key, () => addSubscription(Dispatcher));
		}
	}
}
