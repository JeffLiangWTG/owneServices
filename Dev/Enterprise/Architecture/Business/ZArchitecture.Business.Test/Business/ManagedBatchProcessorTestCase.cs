using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ManagedBatchProcessorTestCase<TM, T> : TestCaseWithFactory
			where TM : ManagedBatchProcessor<T>
			where T : BusinessObject
	{
		#region Impl

		public enum QueueStatus
		{
			Failed,
			Succeeded,
			Queued
		}

		protected abstract T AddToQueue();

		protected abstract TM GetProcessor();

		protected abstract QueueStatus GetStatus(T row);

		public IList<T> SetupQueue(int size)
		{
			var items = new List<T>(size);
			for (var i = 0; i < size; i++)
			{
				items.Add(AddToQueue());
			}
			Factory.Save();
			return items;
		}

		public NotificationCollection Notifications => notifications.Value;
		readonly LazyOverridable<NotificationCollection> notifications = new LazyOverridable<NotificationCollection>(() => new NotificationCollection());

		#endregion

		public void TestLoad()
		{
			SetupQueue(10);
			AssertEquals("The query and the setup should be in sync", 10, GetProcessor().LoadBatch(Factory).Count);
		}

		public void TestDoAction()
		{
			AssertAllMessagesGetProcessed(10);
		}

		public void AssertAllMessagesGetProcessed(int numberOfMessages)
		{
			var queue = SetupQueue(numberOfMessages);
			AssertEquals("The setup items are initially queued", Math.Min(numberOfMessages, GetProcessor().BatchSize), GetProcessor().LoadBatch(Factory).Count(l => GetStatus(l) == QueueStatus.Queued));
			GetProcessor().Process(Notifications);
			queue.ForEach(q => q.Reload());
			AssertEquals("The setup items are all done", numberOfMessages, queue.Count(l => GetStatus(l) == QueueStatus.Succeeded));
		}

		public void TestYield()
		{
			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				var queue = SetupQueue(10);
				cancellationTokenSource.Cancel();
				AssertExceptionThrown<OperationCanceledException>(() => GetProcessor().Process(Notifications, cancellationTokenSource.Token));
				queue.ForEach(q => q.Reload());
				AssertEquals("Since it was cancelled, we do nothing.", 10, queue.Count(l => GetStatus(l) == QueueStatus.Queued));
			}
		}
	}
}
