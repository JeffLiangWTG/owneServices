using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	class DurationTrackerTest : TestCase
	{
		public void TestSpans()
		{
			AssertSpan(expectedSum: 0, expectedUnits: 0, expectedSpans: 0);
			AssertSpan(expectedSum: 1, expectedUnits: 1, expectedSpans: 1, tuples: new[] { new Span<int>(1, 2) });
			AssertSpan(expectedSum: 10, expectedUnits: 1, expectedSpans: 1, tuples: new[] { new Span<int>(1, 11) });
			AssertSpan(expectedSum: 1, expectedUnits: 1, expectedSpans: 2, tuples: new[] { new Span<int>(1, 2), new Span<int>(1, 2) });
			AssertSpan(expectedSum: 1, expectedUnits: 1, expectedSpans: 2, tuples: new[] { new Span<int>(1, 2), new Span<int>(1, 2) });
			AssertSpan(expectedSum: 2, expectedUnits: 2, expectedSpans: 4, tuples: new[] { new Span<int>(1, 2), new Span<int>(1, 2), new Span<int>(2, 3), new Span<int>(2, 3), });
			AssertSpan(expectedSum: 2, expectedUnits: 2, expectedSpans: 2, tuples: new[] { new Span<int>(1, 2), new Span<int>(3, 4), });
			AssertSpan(expectedSum: 10, expectedUnits: 3, expectedSpans: 4, tuples: new[] { new Span<int>(0, 10), new Span<int>(3, 4), });
		}

		public void AssertSpan(int expectedSum, int expectedUnits, int expectedSpans, params Span<int>[] tuples)
		{
			AssertEquals(expectedSum, tuples.MapOverlap<int, Span<int>, int>((t1, t2, l) => t2 - t1).Sum());
			AssertEquals(expectedUnits, tuples.MapOverlap<int, Span<int>, int>((t1, t2, l) => l.Count()).Count());
			AssertEquals(expectedSpans, tuples.MapOverlap<int, Span<int>, int>((t1, t2, l) => l.Count()).Sum());
		}

		public class Span<T> : ISpan<T> where T : IComparable<T>
		{
			public Span(T start, T end)
			{
				Start = start;
				End = end;
			}

			public T Start { get; }

			public T End { get; }
		}
	}
}