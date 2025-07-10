using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;

namespace Enterprise.GraphEngine.ServiceTasks
{
	public sealed class MessageStatusChangeNotifier<TQueueState> : Disposable, INotifiedDisposable
		where TQueueState : class, IQueueState, IEquatable<TQueueState>
	{
		public MessageStatusChangeNotifier(TQueueState[] messages, GrEngineDequeuer<TQueueState> dequeuer, IDisposable disposable)
		{
			Argument.NotNull(messages, nameof(messages));
			Argument.NotNull(dequeuer, nameof(dequeuer));

			this.messages = messages;
			this.dequeuer = dequeuer;
			this.disposable = disposable;
		}

		readonly IDisposable disposable;
		readonly GrEngineDequeuer<TQueueState> dequeuer;
		readonly TQueueState[] messages;

		bool notified;

		public void Notify()
		{
			notified = true;
		}

		#region Disposable

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (notified)
				{
					var updatedMessages = messages.Select(s => new QueueStateResult<TQueueState>(s, QueueStateResultType.Processed));
					dequeuer.Notify(updatedMessages);
				}

				// Must release the locks last, or there is a race
				disposable?.Dispose();
			}
		}

		#endregion
	}
}
