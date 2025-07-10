using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.Common.Enumeration.Testing
{
	class MultipleEnumeratorTest : TestCase
	{
		public void HitsAllElements()
		{
			var mock = new MockEnumerable<int>(new[] { 1, 2, 3, 4, 5 }, new[] { 6, 7, 8, 9, 10 });
			AssertEquals(10, mock.Count());
			AssertEquals(10, mock.Count());
			AssertEquals(10, mock.Count());
			AssertArrayEqualsByElements(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, mock.ToArray());
			AssertEquals("Math!", 6 * 5, mock.Sum());
		}

		class MockEnumerable<T> : IEnumerable<T>
		{
			public MockEnumerable(params IEnumerable<T>[] enumerables)
			{
				this.enumerables = enumerables;
			}

			readonly IEnumerable<T>[] enumerables;
			public IEnumerator<T> GetEnumerator() => new MultipleEnumerator<T>(enumerables);
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}