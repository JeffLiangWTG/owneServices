namespace CargoWise.Common.Collections.Testing
{
	using System;
	using System.Linq;
	using NUnit.Framework;

	public class RingBufferTests : TestCase
	{
		public void TestCtorThrowsOnInvalid()
		{
			AssertExceptionThrown<ArgumentException>(() => new RingBuffer<int>(0));
		}

		public void TestCount()
		{
			var buffer = new RingBuffer<int>(3);
			AssertEquals(0, buffer.Count);

			buffer.Add(1);
			AssertEquals(1, buffer.Count);

			buffer.Add(2);
			AssertEquals(2, buffer.Count);

			buffer.Add(3);
			AssertEquals(3, buffer.Count);

			buffer.Add(4);
			// Should wrap
			AssertEquals(3, buffer.Count);
		}

		public void TestPeekCurrent()
		{
			var buffer = new RingBuffer<int>(3);
			// Default value if empty
			AssertEquals(0, buffer.Current());

			buffer.Add(1);
			AssertEquals(1, buffer.Current());

			buffer.Add(2);
			AssertEquals(2, buffer.Current());

			buffer.Add(3);
			AssertEquals(3, buffer.Current());

			buffer.Add(4);
			AssertEquals(4, buffer.Current());
		}

		public void TestPeekPrevious()
		{
			var buffer = new RingBuffer<int>(3);
			// Default value if empty
			AssertEquals(0, buffer.Previous());

			buffer.Add(1);
			AssertEquals(0, buffer.Previous());

			buffer.Add(2);
			AssertEquals(1, buffer.Previous());

			buffer.Add(3);
			AssertEquals(2, buffer.Previous());

			buffer.Add(4);
			AssertEquals(3, buffer.Previous());
		}

		public void TestPeekPreviousByOffset()
		{
			var buffer = new RingBuffer<int>(3);
			// Default value if empty
			AssertEquals(0, buffer.Previous(2));

			buffer.Add(1);
			buffer.Add(2);
			buffer.Add(3);
			buffer.Add(4);
			buffer.Add(5);
			AssertEquals(4, buffer.Previous(1));
			AssertEquals(3, buffer.Previous(2));

			// Should wrap
			AssertEquals(5, buffer.Previous(3));
			AssertEquals(5, buffer.Previous(3 * 100));
		}

		public void TestEnumeration()
		{
			var buffer = new RingBuffer<int>(3);
			AssertEquals(0, buffer.ToArray().Length);

			buffer.Add(1);
			buffer.Add(2);
			buffer.Add(3);
			buffer.Add(4);
			buffer.Add(5);
			AssertArrayEqualsByElements(new int[] { 3, 4, 5 }, buffer.ToArray());
		}

		public void TestIndexerAccess()
		{
			var buffer = new RingBuffer<int>(3);
			AssertEquals(0, buffer[5]);

			buffer.Add(1);
			buffer.Add(2);
			buffer.Add(3);
			buffer.Add(4);
			buffer.Add(5);

			AssertEquals(3, buffer[0]);
			AssertEquals(4, buffer[1]);
			AssertEquals(5, buffer[2]);

			// Expect wrap behaviour
			AssertEquals(3, buffer[3]);
			AssertEquals(3, buffer[3 * 100]);

			buffer[0] = 10;
			buffer[1] = 11;
			buffer[2] = 12;

			AssertArrayEqualsByElements(new int[] { 10, 11, 12 }, buffer.ToArray());

			// Expect wrap behaviour
			buffer[3] = 13;
			AssertArrayEqualsByElements(new int[] { 13, 11, 12 }, buffer.ToArray());
		}
	}
}
