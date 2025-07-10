using System;
using System.Collections;
using System.Collections.Generic;

namespace CargoWise.Common.Collections
{
	/// <summary>
	/// Simple fixed length ring buffer
	/// </summary>
	/// <typeparam name="T">Element type</typeparam>
	public class RingBuffer<T> : IEnumerable<T>, IReadOnlyList<T>
	{
		readonly T[] buffer;
		long head;

		/// <summary>
		/// Number of elements inserted in the buffer
		/// </summary>
		public int Count { get; private set; }

		/// <summary>
		/// Length of the ring buffer 
		/// </summary>
		public int Length => this.buffer.Length;

		/// <summary>
		/// Create a new ring buffer of set size
		/// </summary>
		/// <param name="size">Number of elements the buffer will hold before wrapping</param>
		public RingBuffer(int size)
		{
			if (size < 1)
			{
				throw new ArgumentException("Invalid argument.", nameof(size));
			}

			this.buffer = new T[size];
		}

		/// <summary>
		/// Add an element to the buffer, overwriting the oldest if full
		/// </summary>
		/// <param name="value">Element to add</param>
		public void Add(T value)
		{
			this.head = (this.head + 1) % this.buffer.Length;
			this.buffer[this.head] = value;

			if (this.Count < this.buffer.Length)
			{
				++this.Count;
			}
		}

		/// <summary>
		/// Retreive the current element (last inserted)
		/// </summary>
		/// <returns>Last inserted element</returns>
		public T Current()
		{
			return this.buffer[this.head];
		}

		/// <summary>
		/// Retrieve the previous element (1 before current)
		/// </summary>
		/// <returns>The previous element</returns>
		public T Previous()
		{
			return Previous(1);
		}

		/// <summary>
		/// Retrieve the previous element based on an offset
		/// </summary>
		/// <param name="offset">How many elements prior to current to walk back</param>
		/// <returns>The element at the specified offset</returns>
		public T Previous(int offset)
		{
			return this.buffer[(this.head + this.buffer.Length - (offset % this.buffer.Length)) % this.buffer.Length];
		}

		/// <summary>
		/// Clear the buffer (resets count to 0)
		/// </summary>
		public void Clear()
		{
			this.Count = 0;
		}

		/// <summary>
		/// Access ringbuffer by index (0 being oldest value inserted)
		/// </summary>
		/// <param name="index">Offset from oldest value inserted</param>
		/// <returns>Value at index</returns>
		public T this[int index]
		{
			get
			{
				return this.buffer[(this.head + this.buffer.Length - this.Count + index + 1) % this.buffer.Length];
			}

			set
			{
				this.buffer[(this.head + this.buffer.Length - this.Count + index + 1) % this.buffer.Length] = value;
			}
		}

		/// <summary>
		/// Enumerate the buffer in FIFO order
		/// </summary>
		/// <returns>An enumerator to enumerate the buffer in FIFO order</returns>
		public IEnumerator<T> GetEnumerator()
		{
			var currentCount = this.Count;
			var tail = (this.head + this.buffer.Length - this.Count + 1) % this.buffer.Length;

			for (int i = 0; i < currentCount; ++i)
			{
				yield return this.buffer[(tail + i) % this.buffer.Length];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}