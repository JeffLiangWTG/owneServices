using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZStoredProcedureTest : TestCase
	{
		public void TestZStoredProcedureConstructor()
		{
			Assert(storedProcQuery.IsDBOnlyQuery);
		}

		public void TestGetDataQuery()
		{
			var dataQuery = storedProcQuery.GetDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName);
			AssertEquals(typeof(ZDataQuery), dataQuery.GetType());
			AssertEquals(1, dataQuery.Parameters.Length);
			AssertEquals(parameter, dataQuery.Parameters[0]);
			AssertEquals("TestStoredProcedure", dataQuery.ParameterisedQueryText);
			Assert(dataQuery.isStoredProc);
		}

		public void TestCannotAddToItself()
		{
			AssertExceptionThrown<NotSupportedException>(() => storedProcQuery.AddToFilter(storedProcQuery));
			AssertExceptionThrown<NotSupportedException>(() => storedProcQuery.AddToFilter(storedProcQuery, JoinCondition.And));
		}

		public void TestZStoredProcedureAddToZStoredProcedure()
		{
			parameter = ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description);
			var storedProcQuery2 = new ZStoredProcedureQuery(storedProcedureNameForTest, new ZSqlParameter[1] { parameter });
			AssertExceptionThrown<NotSupportedException>(() => storedProcQuery.AddToFilter(storedProcQuery2));
			AssertExceptionThrown<NotSupportedException>(() => storedProcQuery.AddToFilter(storedProcQuery2, JoinCondition.And));
		}

		public void TestDownCastZStoredProceduresCannotAddTogether()
		{
			var spQuery = (ZQuery)storedProcQuery;
			parameter = ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description);
			var spQuery2 = (ZQuery)new ZStoredProcedureQuery(storedProcedureNameForTest, new ZSqlParameter[1] { parameter });
			AssertExceptionThrown<NotSupportedException>(() => spQuery.AddToFilter(spQuery2));
			AssertExceptionThrown<NotSupportedException>(() => spQuery.AddToFilter(spQuery2, JoinCondition.And));
		}

		public void TestQueryAddToFilterToZStoredProcedureFails()
		{
			var query = new ZQuery(DummyBizoSchema.Z0_Code, "FAILED");
			AssertExceptionThrown<NotSupportedException>(() => storedProcQuery.AddToFilter(query));
			AssertExceptionThrown<NotSupportedException>(() => storedProcQuery.AddToFilter(query, JoinCondition.And));
		}

		public void TestAddToFilterAsDowncast()
		{
			var spQuery = (ZQuery)storedProcQuery;
			var query = new ZQuery(DummyBizoSchema.Z0_Code, "FAILED");
			AssertExceptionThrown<NotSupportedException>(() => query.AddToFilter(spQuery));
			AssertExceptionThrown<NotSupportedException>(() => query.AddToFilter(spQuery, JoinCondition.And));
		}

		public void TestSupportsFetchHints()
		{
			Assert(!storedProcQuery.SupportsFetchHints);
		}

		public void TestLiteralTextADO()
		{
			{
				var testParams = new ZSqlParameterCollection();
				testParams.Add(ZSqlParameter.New("@testParam2", "A", DummyBizoSchema.Z0_VarCharMax));
				testParams.Add(ZSqlParameter.New("@testParam1", "B", DummyBizoSchema.Z0_VarCharMax));
				var query = new ZStoredProcedureQuery(storedProcedureNameForTest, testParams.ToArray());
				AssertEquals("TestStoredProcedure @testParam1 = 'B', @testParam2 = 'A'", query.LiteralTextADO);
			}
			{
				var testParams = ZSqlParameter.EmptyArray;
				var query = new ZStoredProcedureQuery(storedProcedureNameForTest, testParams);
				AssertEquals("TestStoredProcedure ", query.LiteralTextADO);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			parameter = ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description);
			storedProcQuery = new ZStoredProcedureQuery(storedProcedureNameForTest, new ZSqlParameter[1] { parameter });
		}
		ZSqlParameter parameter;
		ZStoredProcedureQuery storedProcQuery;
		const string storedProcedureNameForTest = "TestStoredProcedure";
	}
}
