using System;
using NUnit.Framework;
using WTG.Statistics;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StatisticsCacheManagerTest : TransactionedTestCase
	{
		public void TestNonExistent()
		{
			var manager = new StatisticsCacheManager();
			manager.TryRetrieve("dbo", "blah", out var histograms);
			Assert(histograms.Length == 0);
		}

		public void TestRoundTrip()
		{
			var manager = new StatisticsCacheManager();
			var histogram = new SqlHistogram("Schema", "Table", "Column", new[] { new SqlHistogramStep("HiKey", 100, 10000), });
			var histograms = new SqlHistogram[] { histogram };

			manager.TryRetrieve("dbo", "Statistics:dbo.BPMTaskTmpl", out var testHistograms);

			manager.Save("Schema", "Table", histograms);
			manager.TryRetrieve("Schema", "Table", out var histograms2);
			AssertEquals(1, histograms2.Length);
			AssertEquals("Schema", histograms2[0].SchemaName);
			AssertEquals("Column", histograms2[0].ColumnName);
			AssertEquals("Table", histograms2[0].TableName);
		}

		public void TestSerializeDeserializeUnexpectedObject()
		{
			var manager = new StatisticsCacheManager();
			var histogram = new SqlHistogram("Schema", "Table", "Column", new[] { new SqlHistogramStep(new object(), 100, 10000), });
			var histograms = new SqlHistogram[] { histogram };

			AssertExceptionThrown<ArgumentException>("Type 'Object' is not allowed to deserialize by StatisticsCacheSerializer", () =>
			{
				manager.Save("Schema", "Table", histograms);
				manager.TryRetrieve("Schema", "Table", out var histograms2);
			});
		}

		public void TestSerializeDeserialize_GuidKey()
		{
			var manager = new StatisticsCacheManager();
			var histogram = new SqlHistogram("Schema", "Table", "Column", new[] { new SqlHistogramStep(Guid.Parse("AAAAAAAA-0000-0000-0000-000000000000"), 100, 10000), });
			var histograms = new SqlHistogram[] { histogram };

			var field = typeof(SqlHistogram).GetField("range_Hi_Keys", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			manager.Save("Schema", "Table", histograms);
			manager.TryRetrieve("Schema", "Table", out var histograms2);

			var keys1 = (object[])field.GetValue(histograms[0]);
			var keys2 = (object[])field.GetValue(histograms2[0]);

			AssertEquals(typeof(Guid), keys1[0].GetType());
			AssertEquals(typeof(Guid), keys2[0].GetType());
		}

		public void TestSerializeDeserialize_DecimalKey()
		{
			var manager = new StatisticsCacheManager();
			var histogram = new SqlHistogram("Schema", "Table", "Column", new[] { new SqlHistogramStep(12.5M, 100, 10000), });
			var histograms = new SqlHistogram[] { histogram };

			var field = typeof(SqlHistogram).GetField("range_Hi_Keys", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			manager.Save("Schema", "Table", histograms);
			manager.TryRetrieve("Schema", "Table", out var histograms2);

			var keys1 = (object[])field.GetValue(histograms[0]);
			var keys2 = (object[])field.GetValue(histograms2[0]);

			AssertEquals(typeof(decimal), keys1[0].GetType());
			AssertEquals(typeof(decimal), keys2[0].GetType());
		}
	}
}
