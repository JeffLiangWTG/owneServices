using System.Threading;
using System.Windows.Threading;

namespace Enterprise.ZArchitecture.GUI
{
	// When Dispatcher.Invoke is called with an Action/Func exceptions are thrown out of the method
	// However when called with a Delegate exceptions are caught and raised on the Dispatcher.UnhandledException event
	// This means when changing CW1 Dispatcher.Invoke calls to use the DispatcherSynchronizationContext.Send, exceptions that used to be thrown are instead caught and raised
	// To keep the old behaviour, use ZDispatcherSynchronizationContext to call Invoke with an Action from Send instead of DispatcherSynchronizationContext that passes a Delegate
	public class ZDispatcherSynchronizationContext : SynchronizationContext
	{
		public ZDispatcherSynchronizationContext(Dispatcher dispatcher)
		{
			this.dispatcher = dispatcher;
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			dispatcher.Invoke(() => d(state));
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			dispatcher.BeginInvoke(d, state);
		}

		readonly Dispatcher dispatcher;
	}
}
