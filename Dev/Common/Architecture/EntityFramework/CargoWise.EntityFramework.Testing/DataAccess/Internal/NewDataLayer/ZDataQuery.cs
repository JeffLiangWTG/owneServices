using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZNonPersistentDataQueryTest : TransactionedTestCase
	{
		public void TestCustomParametersAreNotRecreated()
		{
			var sql = @"
			SELECT *
			FROM dbo.DummyBizo
			WHERE
				Z0_Bool = @myBoolParam
				AND Z0_Guid = @myGuidParam
				AND Z0_Description = @myDescriptionParam
				AND Z0_BitFiltered = @myLiteralOnlyParam";

			var expectedParameterisedQueryText = @"
			SELECT *
			FROM dbo.DummyBizo
			WHERE
				Z0_Bool = @myBoolParam
				AND Z0_Guid = @myGuidParam
				AND Z0_Description = @myDescriptionParam
				AND Z0_BitFiltered = 1 /* Parameterised value literalised by ZNonPersistentDataQuery */";

			var guid = Guid.NewGuid();
			var parameters = new ZSqlParameter[]
				{
					ZSqlParameter.New("@myBoolParam", true, DummyBizoSchema.Z0_Bool),
					ZSqlParameter.New("@myGuidParam", guid, DummyBizoSchema.Z0_Guid),
					ZSqlParameter.New("@myDescriptionParam", "abcdefghij", DummyBizoSchema.Z0_Description),
					ZSqlParameter.New("@myLiteralOnlyParam", true, DummyBizoSchema.Z0_BitFiltered),
				};
			var query = new ZNonPersistentDataQuery(sql, parameters);

			AssertMultilineASCIIEquals("Sql text should have NOT had parameters replaced.", expectedParameterisedQueryText, query.ParameterisedQueryText);
			AssertEquals(parameters.Length, query.Parameters.Length);
			AssertEquals("@myBoolParam", query.Parameters[0].ParameterName);
			AssertEquals("@myGuidParam", query.Parameters[1].ParameterName);
			AssertEquals("@myDescriptionParam", query.Parameters[2].ParameterName);
			AssertEquals("@myLiteralOnlyParam", query.Parameters[3].ParameterName);
		}

		public void TestCustomParametersAreNotUnifiedWithExternalFactory()
		{
			var query = new ZQuery();
			query.AddFilterAndZSQLParameterCollection(DummyBizoSchema.Constants.Z0_Code + " = @param1", new ZSqlParameterCollection { { "@param1", "abc", DummyBizoSchema.Z0_Code } });
			query.AddToFilter(DummyBizoSchema.Z0_Description, "XYZ");

			var parameterisedQuery = query.ParameterisedText;

			var expectedParameterisedQueryText = "(" + DummyBizoSchema.Constants.Z0_Code + " = @param1) and " + DummyBizoSchema.Constants.Z0_Description + " = @CWO1_";
			AssertEquals(expectedParameterisedQueryText, parameterisedQuery.ParameterisedQueryText);

			AssertEquals(2, parameterisedQuery.Parameters.Length);
			AssertEquals("@param1", parameterisedQuery.Parameters[0].ParameterName);
			AssertEquals("@CWO1_", parameterisedQuery.Parameters[1].ParameterName);
		}

		public void TestZNonPersistentDataQuery()
		{
			ZSqlParameterCollection @params = new ZSqlParameterCollection(ZSqlParameter.New("@Param1", "ABC", DummyBizoSchema.Z0_Description));
			ZNonPersistentDataQuery query = new ZNonPersistentDataQuery("QueryText", @params);
			AssertEquals("Query params", query.Parameters[0].Value, "ABC");
			AssertEquals("Query text", "QueryText", query.ParameterisedQueryText);
		}

		public void TestToCSharpCode()
		{
			ZNonPersistentDataQuery query = new ZNonPersistentDataQuery("QueryText");
			AssertEquals(query.ToCSharpCode(),
@"<parameters>
nonPersistentDataQuery1 = new ZNonPersistentDataQuery(""QueryText"", <parameters>);");
		}

		public void TestDeepClone()
		{
			ZNonPersistentDataQuery query = new ZNonPersistentDataQuery("Sql");
			IFilterPart clonedQuery = query.DeepClone();
			Assert(!object.ReferenceEquals(query, clonedQuery));
			AssertEquals("Sql", clonedQuery.LiteralTextADO);
		}

		public void TestGetSimplifiedVersion()
		{
			ZNonPersistentDataQuery query = new ZNonPersistentDataQuery("Sql");
			IFilterPart[] simplifiedVersion = ((IFilterPart)query).GetSimplifiedVersion(null);
			AssertEquals(query, simplifiedVersion[0]);
		}
	}

	sealed class ZDataQueryTest : TransactionedTestCase
	{
		public void TestQueryTimeout()
		{
			var filter = new ZQuery(DummyBizoSchema.Z0_Date, new ZDateTime(1971, 9, 18));
			filter.Timeout = 10;
			var dataQuery = new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter);
			AssertEquals(10, dataQuery.Timeout);
		}

		public void TestQueryWithEscapeClauseImplementsParametersUsingTheEscapeCharacterProperly()
		{
			var query = new ZQuery(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.StartsWith, "~~1~~");
			var nonPersistentQuery = query.ParameterisedText;
			AssertEquals("(Z0_VarCharMax like '~~~~1~~~~%' escape '~' AND Z0_VarCharMax >= '~~1~~' AND Z0_VarCharMax <= '~~1~þ')", nonPersistentQuery.LiteralTextSql);
		}

		public void TestReplaceParametersInQuery()
		{
			SQLComparisonOperator comparisonOperator = SQLComparisonOperator.Equal;
			ParameterNameFactory parameterNameFactory = new ParameterNameFactory();
			var testSql = "CWO1_ in (SELECT Value FROM @CWO1_) AND CWO2_ in (SELECT Value FROM @CWO1_)";
			ZSqlParameter sqlParameter = ZSqlParameter.New(parameterNameFactory, new int[] { 1, 2 }, DummyBizoSchema.Z0_Number, comparisonOperator, ComparisonOptions.Default, true);
			ZNonPersistentDataQuery nonPersistentDataQuery = new ZNonPersistentDataQuery(testSql, new ZSqlParameter[] { sqlParameter });
			AssertEquals("CWO1_ in (1, 2) AND CWO2_ in (1, 2)", nonPersistentDataQuery.LiteralTextSql);
		}

		public void TestQueryWith10ParametersGeneratesCorrectParameterisedText()
		{
			ZQuery query = new ZQuery();
			query.DefaultJoinCondition = JoinCondition.Or;
			for (int i = 0; i <= 9; i++)
			{
				query.AddToFilter(DummyBizoSchema.Z0_Number, i);
			}
			ZNonPersistentDataQuery nonPersistentQuery = query.ParameterisedText;
			string parameterisedText = nonPersistentQuery.LiteralTextSql;
			AssertEquals("Z0_Number = 0 or Z0_Number = 1 or Z0_Number = 2 or Z0_Number = 3 or Z0_Number = 4 or Z0_Number = 5 or Z0_Number = 6 or Z0_Number = 7 or Z0_Number = 8 or Z0_Number = 9", parameterisedText);
		}

		public void TestQueryWith3ParametersEscapesOutSingleQuotes()
		{
			ZQuery query = new ZQuery();
			query.DefaultJoinCondition = JoinCondition.Or;
			for (int i = 1; i <= 3; i++)
			{
				query.AddToFilter(DummyBizoSchema.Z0_VarCharMax, i.ToString() + ZString.Replicate('\'', i));
			}
			ZNonPersistentDataQuery nonPersistentQuery = query.ParameterisedText;
			string parameterisedText = nonPersistentQuery.LiteralTextSql;
			AssertEquals("Z0_VarCharMax = '1''' or Z0_VarCharMax = '2''''' or Z0_VarCharMax = '3'''''''", parameterisedText);
		}

		public void TestLiteralTextADOForDate()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Date, new ZDateTime(1971, 9, 18));
			ZDataQuery dataQuery = new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter);
			Assert(dataQuery.LiteralTextADO.Contains("Z0_Date = #1971-09-18 00:00:00.000#"));
		}

		public void TestLiteralTextADO()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Date, new ZDateTime(1971, 9, 18));
			filter.AddToFilter(DummyBizoSchema.Z0_Code, new ZString[] { "123", "456" });
			ZDataQuery dataQuery = new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter);
			Assert(dataQuery.LiteralTextADO.EndsWith("WHERE Z0_Date = #1971-09-18 00:00:00.000# and (Z0_Code in ('123', '456'))"));
		}

		public void TestLiteralTextSQLForDate()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Date, new ZDateTime(1971, 9, 18));
			ZDataQuery dataQuery = new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter);
			Assert(dataQuery.LiteralTextSql.Contains("Z0_Date = '1971-09-18 00:00:00.000'"));
		}

		public void TestLiteralTextSQLForDateTimeOffset()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_DateTimeOffset, new ZDateTimeOffset(1971, 9, 18, 1, 2, 3, TimeSpan.FromHours(11)));
			ZDataQuery dataQuery = new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter);
			Assert(dataQuery.LiteralTextSql.Contains("Z0_DateTimeOffset = '1971-09-18 01:02:03.0000000 +11:00'"));
		}

		//For now, ADO when DateTimeOffset columns and Geography columns are involved is not supported, because the column names are pre-rendered in SQL mode.

		public void TestLiteralTextSQLForGeography()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Geography, new ZGeography("POINT (121 48)"));
			ZDataQuery dataQuery = new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter);
			Assert(dataQuery.LiteralTextSql.Contains("Z0_Geography.STAsText() = N'POINT (121 48)'"));
		}

		public void TestHasBlobFiltersYes()
		{
			ZQuery filter = new ZQuery();
			filter.IsNoLock = false;
			filter.AddToFilter(DummyBizoSchema.Z0_VarCharMax, "ABC");

			IFilterPart query = new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter);
			AssertEquals(true, query.BlobFilters.Contains(DummyBizoSchema.Z0_VarCharMax));
		}

		public void TestHasBlobFiltersNo()
		{
			ZQuery filter = new ZQuery();
			filter.IsNoLock = false;
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "ABC");

			IFilterPart query = new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter);
			AssertEquals(false, query.BlobFilters.Contains(DummyBizoSchema.Z0_VarCharMax));
		}

		public void TestHasParametersEmpty()
		{
			ZQuery filter = new ZQuery();
			IFilterPart filterPart = new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter);
			AssertEquals(false, filterPart.HasParameters);
		}

		public void TestHasParametersNotEmpty()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "CODE");
			IFilterPart filterPart = new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter);
			AssertEquals(true, filterPart.HasParameters);
		}

		public void TestChangeParameterSettingDuringExecution()
		{
			var query = new ZQueryForTest(DummyBizoSchema.Z0_BitFalse, true);

			var dataQuery = new ZDataQuery(Connection, DummyDependentBizoSchema.Constants.TableName, query);
			var sqlText = dataQuery.ParameterisedQueryText;
			var parameters = dataQuery.Parameters;
			Assert("all parameters in sql must exist in query.Parameters", IsAllParameterInSqlFound(sqlText, parameters));

			dataQuery = new ZDataQuery(Connection, DummyDependentBizoSchema.Constants.TableName, query);
			sqlText = dataQuery.ParameterisedQueryText;
			parameters = dataQuery.Parameters;
			Assert("all parameters in sql must exist in query.Parameters", IsAllParameterInSqlFound(sqlText, parameters));
		}

		public void TestIsStoredProcedure()
		{
			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			ZDataQuery persistentStoredProc = new ZDataQuery("TestStoredProcedure", parameter, DummyBizoSchema.Constants.TableName);
			Assert(persistentStoredProc.isStoredProc);
		}

		class ZQueryForTest : ZQuery
		{
			public ZQueryForTest(SchemaColumn schemaColumn, object value)
				: base(schemaColumn, value)
			{ }

			public override void AddAsCompleteSQLStatement(SqlBuilder sqlBuilder, string tableName, bool combineFilterAndParams, SchemaColumn[] selectList = null)
			{
				base.AddAsCompleteSQLStatement(sqlBuilder, tableName, combineFilterAndParams);
			}
		}

		bool IsAllParameterInSqlFound(string sql, ZSqlParameter[] parameters)
		{
			foreach (Match match in Regex.Matches(sql, @"@\w+\b"))
			{
				if (!IsStringInParameters(match.Value, parameters))
				{
					return false;
				}
			}
			return true;
		}

		bool IsStringInParameters(string aPara, ZSqlParameter[] parameters)
		{
			foreach (var para in parameters)
			{
				if (aPara == para.ParameterName)
				{
					return true;
				}
			}
			return false;
		}

		#region Equals / GetHashCode

		public void TestEquals()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "1");
			ZQuery filter2 = new ZQuery(DummyBizoSchema.Z0_Code, "2");
			AssertEquals(
				"Equals",
				new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter),
				new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter));
			AssertNotEquals(
				"Not Equals",
				new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter),
				new ZDataQuery(Connection, DummyBizoSchema.Constants.TableName, filter2));
		}

		#endregion

		#region HasComparisonOperator

		public void TestHasComparisonOperator()
		{
			var nonPersistentDataQuery = new ZNonPersistentDataQuery("");
			AssertEquals(nonPersistentDataQuery.HasComparisonOperatorLike, false);

			nonPersistentDataQuery = new ZNonPersistentDataQuery(null, (ZSqlParameter[])null);
			AssertEquals(nonPersistentDataQuery.HasComparisonOperatorLike, false);

			var query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.NotContains, "A");
			AssertEquals(query.HasComparisonOperatorLike, true);

			nonPersistentDataQuery = new ZNonPersistentDataQuery("", query.Params);
			AssertEquals(nonPersistentDataQuery.HasComparisonOperatorLike, true);

			nonPersistentDataQuery = new ZNonPersistentDataQuery("select * from table");
			AssertEquals(nonPersistentDataQuery.HasComparisonOperatorLike, false);

			nonPersistentDataQuery = new ZNonPersistentDataQuery("select * from table where name like N'A%'");
			AssertEquals(nonPersistentDataQuery.HasComparisonOperatorLike, true);

			nonPersistentDataQuery = new ZNonPersistentDataQuery("select * from table where name like N'A%'", query.Params);
			AssertEquals(nonPersistentDataQuery.HasComparisonOperatorLike, true);
		}

		#endregion

		#region Implementation

		ZSqlConnectionInfo Connection
		{
			get { return new ZSqlConnectionInfo(Db.Connection, ""); }
		}

		#endregion
	}
}
