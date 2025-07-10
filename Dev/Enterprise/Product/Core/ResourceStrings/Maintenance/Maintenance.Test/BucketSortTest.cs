using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Maintenance.Test
{
	sealed class BucketSortTest : TestCase
	{
		public void TestBucketSort()
		{
			var random = new Random();
			const int totalItems = 1534;
			using (var sort = new BucketSort<TestItem>(new TestItemSerailizer(), new TestItemComparer(), 100))
			{
				for (int i = 0; i < totalItems; i++)
				{
					var item = new TestItem();
					item.key = random.Next();
					item.value = item.key.ToString();
					sort.Add(item);
				}
				int lastKey = -1;
				int count = 0;
				foreach (var item in sort)
				{
					Assert(lastKey < item.key);
					AssertEquals(item.key.ToString(), item.value);
					lastKey = item.key;
					count++;
				}
				AssertEquals(totalItems, count);
			}
		}

		class TestItem
		{
			public int key;
			public string value;
		}

		class TestItemSerailizer : BucketSort<TestItem>.ISerializer
		{
			public TestItem Read(TextReader reader)
			{
				TestItem item = null;
				string line = reader.ReadLine();
				if (line != null)
				{
					string[] parts = line.Split('\t');
					item = new TestItem();
					item.key = int.Parse(parts[0]);
					item.value = parts[1];
				}
				return item;
			}

			public void Write(TestItem item, TextWriter writer)
			{
				writer.WriteLine(item.key.ToString() + "\t" + item.value);
			}
		}

		class TestItemComparer : IComparer<TestItem>
		{
			public int Compare(TestItem x, TestItem y)
			{
				return x.key.CompareTo(y.key);
			}
		}
	}
}
