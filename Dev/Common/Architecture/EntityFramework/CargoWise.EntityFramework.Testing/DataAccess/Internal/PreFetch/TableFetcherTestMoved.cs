using System;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	/// <summary>
	/// Moved from Main project
	/// </summary>
	sealed partial class TableFetcherTest
	{
		public void TestQueryStringForZBool()
		{
			TestValues("'Y'", DummyBizoSchema.Z0_Bool, ZBool.True);
		}

		public void TestQueryStringForZDateTimeOffset()
		{
			TestValues("'1971-09-18 12:34:00.0000000 +11:00'", DummyBizoSchema.Z0_DateTimeOffset, new ZDateTimeOffset(1971, 9, 18, 12, 34, 0, TimeSpan.FromHours(11)));
		}

		public void TestQueryStringForZDateTime()
		{
			TestValues("'1971-09-18 12:34:00.000'", DummyBizoSchema.Z0_Date, new ZDateTime(1971, 9, 18, 12, 34, 0));
		}

		public void TestQueryStringForZTime()
		{
			TestValues("'12:34:00'", DummyBizoSchema.Z0_Time, new ZTime(12, 34));
		}

		public void TestQueryStringForZGeography()
		{
			TestValues("N'POINT (-121 48)'", DummyBizoSchema.Z0_Geography, new ZGeography("POINT (-121 48)"));
		}

		public void TestQueryStringForZDecimal()
		{
			TestValues("6.2", DummyBizoSchema.Z0_AnotherDecimal, new ZDecimal(6.2m));
		}

		public void TestQueryStringForZGuid()
		{
			TestValues("'" + ZGuid.Invalid.ToString() + "'", DummyBizoSchema.Z0_Guid, ZGuid.Invalid);
		}

		public void TestQueryStringForZString()
		{
			TestValues("'Hi'", DummyBizoSchema.Z0_Code, new ZString("Hi"));
		}

		public void TestQueryStringForZStringWithSingleQuote()
		{
			TestValues("'Hi'''", DummyBizoSchema.Z0_Code, new ZString("Hi'"));
		}

		void TestValues(string expectedOutput, SchemaColumn column, params IZType[] values)
		{
			var rowFactory = new RowFactory();
			var tableFetcher = new TableFetcher(DummyBizoSchema.Constants.TableName);
			IFetchHint hint = null;
			using (var fetchHintQueryStorer = new FetchHintQueryStorer(rowFactory.QueryCache))
			{
				foreach (IZType value in values)
				{
					hint = new FetchHint(column, value);
					tableFetcher.AddFetchHint(hint);
				}

				var fetchHintQueryCacheManager = new FetchHintQueryCacheManager(tableFetcher.FetchHints.Values);
				tableFetcher.BuildQueries(fetchHintQueryStorer, fetchHintQueryCacheManager);
			}

			var expected = column.Name;
			if (values.Length > 1)
			{
				expected += " in (" + expectedOutput + ")";
			}
			else
			{
				expected += " = " + expectedOutput;
			}

			var provider = tableFetcher.activeProviderQueries[hint.BuilderKey];
			AssertEquals(expected, provider.Builder.ToString());
		}
	}
}
