using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ImmediateZMultiQueryFetchHintTest : TestCase
	{
		public void TestLoadWithBlobs()
		{
			ZQuery query1 = new ZQuery();
			ZQuery query2 = new ZQuery();
			IFetchHint fetchHint = new ImmediateZMultiQueryFetchHint(typeof(DummyBusinessObject), query1, query2);
			AssertEquals(false, Enumerable.Contains(fetchHint.LoadWithBlobs, DummyBizoSchema.Z0_NVarCharMax));

			query1.AddToFilter(DummyBizoSchema.Z0_NVarCharMax, "123");
			AssertEquals(true, Enumerable.Contains(fetchHint.LoadWithBlobs, DummyBizoSchema.Z0_NVarCharMax));

			query1.Clear();
			AssertEquals(false, Enumerable.Contains(fetchHint.LoadWithBlobs, DummyBizoSchema.Z0_NVarCharMax));

			query2.AddToFilter(DummyBizoSchema.Z0_NVarCharMax, "123");
			AssertEquals(true, Enumerable.Contains(fetchHint.LoadWithBlobs, DummyBizoSchema.Z0_NVarCharMax));

			query2.Clear();
			query1.IncludeBlob(DummyBizoSchema.Z0_NVarCharMax);
			AssertEquals(true, Enumerable.Contains(fetchHint.LoadWithBlobs, DummyBizoSchema.Z0_NVarCharMax));

			query1.ClearBlobs();
			query2.IncludeBlob(DummyBizoSchema.Z0_NVarCharMax);
			AssertEquals(true, Enumerable.Contains(fetchHint.LoadWithBlobs, DummyBizoSchema.Z0_NVarCharMax));

			query2.ClearBlobs();
			AssertEquals(false, Enumerable.Contains(fetchHint.LoadWithBlobs, DummyBizoSchema.Z0_NVarCharMax));
		}

		public void TestIsNeeded()
		{
			RowFactory factory = new RowFactory();
			QueryHistoryProvider historyProvider = new QueryHistoryProvider(factory);

			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			ZQuery secondQuery = new ZQuery(DummyBizoSchema.Z0_Number, 1);
			IFetchHint hint = new ImmediateZMultiQueryFetchHint(typeof(DummyBusinessObject), mainQuery, secondQuery);
			AssertEquals(true, hint.IsNeeded(historyProvider));

			factory.AddFetchHint(hint);
			AssertEquals(true, hint.IsNeeded(historyProvider));

			factory.ExecuteAllFetchHints();
			AssertEquals(false, hint.IsNeeded(historyProvider));
		}

		public void TestGenerateSql()
		{
			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			IFetchHint fetchHint1 = new ImmediateZMultiQueryFetchHint(typeof(DummyBusinessObject), mainQuery, new ZQuery(DummyBizoSchema.Z0_Number, 1));
			IFetchHint fetchHint2 = new ImmediateZMultiQueryFetchHint(typeof(DummyBusinessObject), mainQuery, new ZQuery(DummyBizoSchema.Z0_Number, 2));
			var builder = new QueryBuilder();
			fetchHint1.GenerateQuery(builder);
			fetchHint2.GenerateQuery(builder);
			AssertEquals("Z0_Code = '123' and (Z0_Number = 1 or Z0_Number = 2)", builder.ToString());
		}

		public void TestTableName()
		{
			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			ZQuery secondQuery = new ZQuery(DummyBizoSchema.Z0_Number, 1);
			IFetchHint fetchHint = new ImmediateZMultiQueryFetchHint(typeof(DummyBusinessObject), mainQuery, secondQuery);
			AssertEquals("DummyBizo", fetchHint.TableName);
		}

		public void TestGetQuery()
		{
			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			ZQuery secondQuery = new ZQuery(DummyBizoSchema.Z0_Number, 1);
			IFetchHint fetchHint = new ImmediateZMultiQueryFetchHint(typeof(DummyBusinessObject), mainQuery, secondQuery);
			AssertEquals("Z0_Code = '123' and Z0_Number = 1", fetchHint.GetQuery().LiteralTextADO);
		}

		public void TestGetHashString()
		{
			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			ZQuery secondQuery = new ZQuery(DummyBizoSchema.Z0_Number, 1);
			IFetchHint fetchHint = new ImmediateZMultiQueryFetchHint(typeof(DummyBusinessObject), mainQuery, secondQuery);
			AssertEquals("Z0_Code = '123' and Z0_Number = 1", fetchHint.GetHashKeyObject().KeyParts.Single());
		}

		public void TestIsDataHintLoaded()
		{
			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			ZQuery secondQuery = new ZQuery(DummyBizoSchema.Z0_Number, 1);
			IFetchHint fetchHint = new ImmediateZMultiQueryFetchHint(typeof(DummyBusinessObject), mainQuery, secondQuery);
			AssertEquals(false, fetchHint.IsDataHintLoaded);
			fetchHint.IsDataHintLoaded = true;
			AssertEquals(true, fetchHint.IsDataHintLoaded);
		}
	}
}
