using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test
{
	public class TopListTestCase : TestCase
	{
		public void TestTryAddShouldReturnTrueWhenTopListIsUnderfilled()
		{
			var random = new Random(Seed: (int)(DateTime.UtcNow.Ticks % int.MaxValue));
			var topList = new TopList<int>(10, Comparer<int>.Default);
			for (var i = 0; i < 10; ++i)
			{
				var v = random.Next(0, 100);
				Assert(topList.TryAdd(v));
			}
			AssertLessThanOrEqualTo(topList.Count, 10);
		}

		public void TestTryAddShouldReturnFalseWhenTopListIsFilledAndValueIsTooLarge()
		{
			var topList = new TopList<int>(10, Comparer<int>.Default);
			for (var i = 0; i < 10; ++i)
			{
				Assert(topList.TryAdd(0));
			}

			Assert(!topList.TryAdd(0));
			Assert(!topList.TryAdd(1));
			AssertLessThanOrEqualTo(topList.Count, 10);
		}

		public void TestTryAddShouldReturnTrueWhenTopListIsFilledAndValueIsSmallEnough()
		{
			var topList = new TopList<int>(10, Comparer<int>.Default);
			for (var i = 0; i < 10; ++i)
			{
				Assert(topList.TryAdd(0));
			}

			Assert(topList.TryAdd(-1));
			Assert(topList.TryAdd(-2));
			Assert(topList.TryAdd(-3));
			Assert(topList.TryAdd(-4));
			AssertLessThanOrEqualTo(topList.Count, 10);
		}

		public void TestTopListContainsTopValues()
		{
			var values = Enumerable.Range(0, 100).Concat(Enumerable.Range(0, 100)).ToArray();

			// shuffle
			var random = new Random(Seed: (int)(DateTime.UtcNow.Ticks % int.MaxValue));
			for (var i = 0; i < 100; ++i)
			{
				var j = random.Next(0, values.Length - 1);
				var k = random.Next(0, values.Length - 1);

				if (i != j)
				{
					(values[j], values[i]) = (values[i], values[j]);
				}
			}

			var topList = new TopList<int>(10, (a, b) => b.CompareTo(a));  // prioritise larger values
			foreach (var x in values)
			{
				_ = topList.TryAdd(x);
			}

			var topListValues = topList.ToArray();
			Array.Sort(topListValues, (a, b) => b.CompareTo(a));
			AssertEquals(10, topListValues.Length);
			topListValues[0] = 99;
			topListValues[1] = 99;
			topListValues[2] = 98;
			topListValues[3] = 98;
			topListValues[4] = 97;
			topListValues[5] = 97;
			topListValues[6] = 96;
			topListValues[7] = 96;
			topListValues[8] = 95;
			topListValues[9] = 95;
		}
	}
}