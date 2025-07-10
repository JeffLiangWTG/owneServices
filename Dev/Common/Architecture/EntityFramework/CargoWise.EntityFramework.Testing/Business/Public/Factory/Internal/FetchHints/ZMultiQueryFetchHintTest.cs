using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZMultiQueryFetchHintTest : TestCase
	{
		public void TestIsNeeded()
		{
			RowFactory factory = new RowFactory();
			QueryHistoryProvider historyProvider = new QueryHistoryProvider(factory);

			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			ZQuery secondQuery = new ZQuery(DummyBizoSchema.Z0_Number, 1);
			IFetchHint hint = new ZMultiQueryFetchHint(DummyBizoSchema.Instance, mainQuery, secondQuery);
			AssertEquals(true, hint.IsNeeded(historyProvider));

			factory.AddFetchHint(hint);
			AssertEquals(true, hint.IsNeeded(historyProvider));

			factory.ExecuteAllFetchHints();
			AssertEquals(false, hint.IsNeeded(historyProvider));
		}

		public void TestGenerateQuery()
		{
			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			IFetchHint fetchHint1 = new ZMultiQueryFetchHint(DummyBizoSchema.Instance, mainQuery, new ZQuery(DummyBizoSchema.Z0_Number, 1));
			IFetchHint fetchHint2 = new ZMultiQueryFetchHint(DummyBizoSchema.Instance, mainQuery, new ZQuery(DummyBizoSchema.Z0_Number, 2));
			var builder = new QueryBuilder();
			fetchHint1.GenerateQuery(builder);
			fetchHint2.GenerateQuery(builder);
			AssertEquals("Z0_Code = '123' and (Z0_Number = 1 or Z0_Number = 2)", builder.ToString());
		}

		public void TestTableName()
		{
			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			ZQuery secondQuery = new ZQuery(DummyBizoSchema.Z0_Number, 1);
			IFetchHint fetchHint = new ZMultiQueryFetchHint(DummyBizoSchema.Instance, mainQuery, secondQuery);
			AssertEquals("DummyBizo", fetchHint.TableName);
		}

		public void TestGetQuery()
		{
			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			ZQuery secondQuery = new ZQuery(DummyBizoSchema.Z0_Number, 1);
			IFetchHint fetchHint = new ZMultiQueryFetchHint(DummyBizoSchema.Instance, mainQuery, secondQuery);
			AssertEquals("Z0_Code = '123' and Z0_Number = 1", fetchHint.GetQuery().LiteralTextADO);
		}

		public void TestGetHashString()
		{
			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			ZQuery secondQuery = new ZQuery(DummyBizoSchema.Z0_Number, 1);
			IFetchHint fetchHint = new ZMultiQueryFetchHint(DummyBizoSchema.Instance, mainQuery, secondQuery);
			AssertEquals("Z0_Code = '123' and Z0_Number = 1", fetchHint.GetHashKeyObject().KeyParts.Single());
		}

		public void TestIsDataHintLoaded()
		{
			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			ZQuery secondQuery = new ZQuery(DummyBizoSchema.Z0_Number, 1);
			IFetchHint fetchHint = new ZMultiQueryFetchHint(DummyBizoSchema.Instance, mainQuery, secondQuery);
			AssertEquals(false, fetchHint.IsDataHintLoaded);
			fetchHint.IsDataHintLoaded = true;
			AssertEquals(true, fetchHint.IsDataHintLoaded);
		}
	}
}
