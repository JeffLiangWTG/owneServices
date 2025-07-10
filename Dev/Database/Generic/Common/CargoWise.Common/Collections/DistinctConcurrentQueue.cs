using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace CargoWise.Common.Collections
{
	public class DistinctConcurrentQueue<T> : IProducerConsumerCollection<T>, IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
	{
		public DistinctConcurrentQueue()
		{
			queuedItems = new HashSet<T>();
			spinLock = new SpinLock(false);
			concurrentQueue = new ConcurrentQueue<T>();
		}

		public DistinctConcurrentQueue(IEqualityComparer<T> comparer)
		{
			queuedItems = new HashSet<T>(comparer);
			spinLock = new SpinLock(false);
			concurrentQueue = new ConcurrentQueue<T>();
		}

		SpinLock spinLock; // This is a struct, it must never be readonly
		readonly HashSet<T> queuedItems;
		readonly ConcurrentQueue<T> concurrentQueue;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "SpinLock used to manage access to ConcurrentQueue")]
		public ICollection<T> Clear()
		{
			bool lockTaken = false;
			var list = new List<T>(concurrentQueue.Count);
			try
			{
				spinLock.TryEnter(-1, ref lockTaken);

				queuedItems.Clear();
				while (concurrentQueue.TryDequeue(out var item))
				{
					list.Add(item);
				}
			}
			finally
			{
				if (lockTaken)
				{
					spinLock.Exit();
				}
			}
			return list;
		}

		public bool TryEnqueue(T item)
		{
			bool lockTaken = false;
			try
			{
				spinLock.TryEnter(-1, ref lockTaken);

				var result = queuedItems.Add(item);
				if (result)
				{
					concurrentQueue.Enqueue(item);
				}
				return result;
			}
			finally
			{
				if (lockTaken)
				{
					spinLock.Exit();
				}
			}
		}

		public bool TryDequeue(out T result)
		{
			bool lockTaken = false;
			try
			{
				spinLock.TryEnter(-1, ref lockTaken);

				if (concurrentQueue.TryDequeue(out result))
				{
					queuedItems.Remove(result);
					return true;
				}
				return false;
			}
			finally
			{
				if (lockTaken)
				{
					spinLock.Exit();
				}
			}
		}

		public bool TryPeek(out T result)
		{
			return concurrentQueue.TryPeek(out result);
		}

		public void CopyTo(T[] array, int index) => concurrentQueue.CopyTo(array, index);

		public IEnumerator<T> GetEnumerator() => concurrentQueue.GetEnumerator();

		public T[] ToArray() => concurrentQueue.ToArray();

		public int Count => concurrentQueue.Count;

		public bool IsEmpty => concurrentQueue.IsEmpty;

		#region IProducerConsumerCollection<T>

		bool IProducerConsumerCollection<T>.TryAdd(T item) => TryEnqueue(item);
		bool IProducerConsumerCollection<T>.TryTake(out T item) => TryDequeue(out item);

		#endregion

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion

		#region ICollection

		object ICollection.SyncRoot => ((ICollection)concurrentQueue).SyncRoot;
		bool ICollection.IsSynchronized => ((ICollection)concurrentQueue).IsSynchronized;
		void ICollection.CopyTo(Array array, int index) => ((ICollection)concurrentQueue).CopyTo(array, index);

		#endregion
	}
}
