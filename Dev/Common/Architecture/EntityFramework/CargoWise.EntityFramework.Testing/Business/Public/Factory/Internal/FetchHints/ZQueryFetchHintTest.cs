using System;
using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZQueryFetchHintTest : TestCase
	{
		public void TestIsNeeded()
		{
			RowFactory factory = new RowFactory();
			QueryHistoryProvider historyProvider = new QueryHistoryProvider(factory);

			IFetchHint hint = new ZQueryFetchHint(DummyBizoSchema.Instance, new ZQuery(DummyBizoSchema.Z0_Code, "123"));
			AssertEquals(true, hint.IsNeeded(historyProvider));

			factory.AddFetchHint(hint);
			AssertEquals(true, hint.IsNeeded(historyProvider));

			factory.ExecuteAllFetchHints();
			AssertEquals(false, hint.IsNeeded(historyProvider));
		}

		public void TestConstructor()
		{
			IFetchHint fetchHint = new ZQueryFetchHint(DummyBizoSchema.Instance, new ZQuery(DummyBizoSchema.Z0_Code, "123"));
			AssertEquals("DummyBizo", fetchHint.TableName);
		}

		public void TestGetHashStringIncludingLoadWithBlobs()
		{
			IFetchHint fetchHint = new ZQueryFetchHint(DummyBizoSchema.Instance, new ZQuery(DummyBizoSchema.Z0_Code, "123"));
			AssertEquals("Z0_Code = '123'", fetchHint.GetHashKeyObject().KeyParts.Single());
		}

		public void TestGetQuery()
		{
			IFetchHint fetchHint = new ZQueryFetchHint(DummyBizoSchema.Instance, new ZQuery(DummyBizoSchema.Z0_Code, "123"));
			AssertEquals("Z0_Code = '123'", fetchHint.GetQuery().LiteralTextADO);
		}

		public void TestIsDataHintLoaded()
		{
			IFetchHint fetchHint = new ZQueryFetchHint(DummyBizoSchema.Instance, new ZQuery(DummyBizoSchema.Z0_Code, "123"));
			AssertEquals(false, fetchHint.IsDataHintLoaded);
			fetchHint.IsDataHintLoaded = true;
			AssertEquals(true, fetchHint.IsDataHintLoaded);
		}

		public void TestBuilderKey()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Code, "123");
			IFetchHint fetchHint = new ZQueryFetchHint(DummyBizoSchema.Instance, query);
			AssertEquals(DummyBizoSchema.Constants.TableName + ":ZQuery:Z0_Code", fetchHint.BuilderKey);

			((ISeparateFetchQuery)query).CannotBeJoinedInFetchHint = true;
			fetchHint = new ZQueryFetchHint(DummyBizoSchema.Instance, query);
			AssertEquals(DummyBizoSchema.Constants.TableName + ":ZQuery:" + new ZQuery(DummyBizoSchema.Z0_Code, "123").LiteralTextSql, fetchHint.BuilderKey);
		}

		public void TestZStoredProcedureQueryThrowsException()
		{
			var parameter = ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description);
			var storedProcQuery = new ZStoredProcedureQuery("TestStoredProcedure", new ZSqlParameter[1] { parameter });
			AssertExceptionThrown<NotSupportedException>("Exception for ZStoredProcedure", "Query of type: ZStoredProcedureQuery does not support fetch hints", () => new ZQueryFetchHint(DummyBizoSchema.Instance, storedProcQuery));
		}
	}
}
