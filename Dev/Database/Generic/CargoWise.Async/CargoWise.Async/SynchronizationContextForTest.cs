#if DEBUG
using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;

namespace CargoWise.Async
{
	public class SynchronizationContextForTest : SynchronizationContext, IDisposable
	{
		public static SynchronizationContextForTest Enable()
		{
			return new SynchronizationContextForTest();
		}

		SynchronizationContextForTest()
		{
			isMainThread = ApplicationDispatcher.MainThread == Thread.CurrentThread;
			this.previousContext = SynchronizationContext.Current;
			SynchronizationContext.SetSynchronizationContext(this);
			if (isMainThread)
			{
				previousDispatcher = ApplicationDispatcher.Current;
				ApplicationDispatcher.Current = this;
			}
		}

		public bool WaitAndExecuteCallbacks(TimeSpan timeout)
		{
			var result = postedMessageEvent.WaitOne(timeout);
			while (postedMessages.Count > 0)
			{
				var message = postedMessages.Dequeue();
				message.Callback.Invoke(message.State);
			}
			return result;
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			postedMessages.Enqueue(new Message(d, state));
			postedMessageEvent.Set();
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			d.Invoke(state);
		}

		public void Dispose()
		{
			SynchronizationContext.SetSynchronizationContext(previousContext);
			if (isMainThread)
			{
				ApplicationDispatcher.Current = previousDispatcher;
			}
		}

		readonly Queue<Message> postedMessages = new Queue<Message>();
		readonly AutoResetEvent postedMessageEvent = new AutoResetEvent(false);
		readonly SynchronizationContext previousContext;
		readonly SynchronizationContext previousDispatcher;
		readonly bool isMainThread;

		class Message
		{
			public Message(SendOrPostCallback callback, object state)
			{
				Callback = callback;
				State = state;
			}

			public SendOrPostCallback Callback { get; }
			public object State { get; }
		}
	}
}
#endif
