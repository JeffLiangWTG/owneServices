using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common
{
	public class OccurrenceTracker
	{
		readonly TimeSpan trackingSpan;
		readonly Func<DateTime> currentTimeFunc;
		readonly ConcurrentQueue<DateTime> occurrences = new ConcurrentQueue<DateTime>();
		readonly object dropLock = new object();

		public OccurrenceTracker(TimeSpan trackingSpan, Func<DateTime> currentTimeFunc)
		{
			Argument.NotNull(currentTimeFunc, nameof(currentTimeFunc));

			this.trackingSpan = trackingSpan;
			this.currentTimeFunc = currentTimeFunc;
		}

		public void Add()
		{
			var currentTime = currentTimeFunc();
			occurrences.Enqueue(currentTime);
			Latest = currentTime;
			DropExpired();
		}

		public int Count
		{
			get
			{
				DropExpired();
				return occurrences.Count;
			}
		}

		public DateTime? Latest { get; private set; }

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Needs to be done as dequeue is conditional, have protected with a lock")]
		void DropExpired()
		{
			var currentTime = currentTimeFunc();

			lock (dropLock)
			{
				while (occurrences.TryPeek(out var oldest) && oldest < currentTime.Subtract(trackingSpan))
				{
					occurrences.TryDequeue(out oldest);
				}
			}
		}
	}
}