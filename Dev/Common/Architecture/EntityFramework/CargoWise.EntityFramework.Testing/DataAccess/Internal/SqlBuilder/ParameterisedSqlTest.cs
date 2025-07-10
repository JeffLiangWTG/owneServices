using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ParameterisedSqlTest : TransactionedTestCase
	{
		public void TestTwoNormalParams()
		{
			FilterStringBuilder filterStringBuilder = new FilterStringBuilder();
			ZSqlParameter parameter1 = ZSqlParameter.New("@P1", 1, DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThan);
			ZSqlParameter parameter2 = ZSqlParameter.New("@P2", 2, DummyBizoSchema.Z0_Number, SQLComparisonOperator.LessThan);

			filterStringBuilder.Append(JoinCondition.And, parameter1);
			filterStringBuilder.Append(JoinCondition.And, parameter2);

			var builder = new SqlFilterPartAppender(SqlBuilder.QueryType.Parameterised);
			ZNonPersistentDataQuery query = builder.GetParameterisedSql(filterStringBuilder);
			AssertEquals("Parameter Count", 2, query.Parameters.Length);
			AssertEquals("Query text", "Z0_Number > @P1 and Z0_Number < @P2", query.ParameterisedQueryText);
			AssertEquals("builder.ToString()", "Z0_Number > 1 and Z0_Number < 2", query.LiteralTextADO);
		}

		public void TestTwoDifferentParamsWithSameValue()
		{
			FilterStringBuilder filterStringBuilder = new FilterStringBuilder();
			ZSqlParameter parameter1 = ZSqlParameter.New("@P1", 1, DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThan);
			ZSqlParameter parameter2 = ZSqlParameter.New("@P2", 1, DummyBizoSchema.Z0_Number, SQLComparisonOperator.LessThan);

			filterStringBuilder.Append(JoinCondition.And, parameter1);
			filterStringBuilder.Append(JoinCondition.And, parameter2);

			var builder = new SqlFilterPartAppender(SqlBuilder.QueryType.Parameterised);
			ZNonPersistentDataQuery query = builder.GetParameterisedSql(filterStringBuilder);
			AssertEquals("Parameter Count", 2, query.Parameters.Length);
			AssertEquals("Query text", "Z0_Number > @P1 and Z0_Number < @P2", query.ParameterisedQueryText);
			AssertEquals("builder.ToString()", "Z0_Number > 1 and Z0_Number < 1", query.LiteralTextADO);
		}

		public void TestAutoParameterUsedTwiceInFilterIsHandled()
		{
			FilterStringBuilder filterStringBuilder = new FilterStringBuilder();
			ZQuery subFilter = new ZQuery();

			filterStringBuilder.Append(JoinCondition.Or, subFilter);
			filterStringBuilder.Append(JoinCondition.Or, subFilter);

			subFilter.AddToFilter(DummyBizoSchema.Z0_Code, "123");

			var builder = new SqlFilterPartAppender(SqlBuilder.QueryType.Parameterised);
			ZNonPersistentDataQuery query = builder.GetParameterisedSql(filterStringBuilder);
			AssertEquals("Parameter Count", 1, query.Parameters.Length);
			AssertEquals("Query text", "Z0_Code = @CWO1_ or Z0_Code = @CWO1_", query.ParameterisedQueryText);
		}

		public void TestManualParameterUsedTwiceInFilterIsHandled()
		{
			FilterStringBuilder filterStringBuilder = new FilterStringBuilder();
			ZSqlParameter parameter = ZSqlParameter.New("@P1", 1, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal);

			filterStringBuilder.Append(JoinCondition.Or, parameter);
			filterStringBuilder.Append(JoinCondition.Or, parameter);

			var builder = new SqlFilterPartAppender(SqlBuilder.QueryType.Parameterised);
			ZNonPersistentDataQuery query = builder.GetParameterisedSql(filterStringBuilder);
			AssertEquals("Parameter Count", 1, query.Parameters.Length);
			AssertEquals("Query text", "Z0_Number = @P1 or Z0_Number = @P1", query.ParameterisedQueryText);
		}

		public void TestTwoManualParamsWithTheSameNameButDifferentValuesIsHandled()
		{
			FilterStringBuilder filterStringBuilder = new FilterStringBuilder();
			ZSqlParameter parameter1 = ZSqlParameter.New("@P1", 1, DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThan);
			ZSqlParameter parameter2 = ZSqlParameter.New("@P1", 2, DummyBizoSchema.Z0_Number, SQLComparisonOperator.LessThan);

			filterStringBuilder.Append(JoinCondition.And, parameter1);
			filterStringBuilder.Append(JoinCondition.And, parameter2);

			var builder = new SqlFilterPartAppender(SqlBuilder.QueryType.Parameterised);
			ZNonPersistentDataQuery query = builder.GetParameterisedSql(filterStringBuilder);
			AssertEquals("Parameter Count", 2, query.Parameters.Length);
			AssertEquals("Query text", "Z0_Number > @P1 and Z0_Number < @P11", query.ParameterisedQueryText);
			AssertEquals("builder.ToString()", "Z0_Number > 1 and Z0_Number < 2", query.LiteralTextADO);
		}

		public void TestTwoParamsWithNamesStartingWithTheSameStringIsHandled()
		{
			ZSqlParameter parameter1A = ZSqlParameter.New("@P1", 1, DummyBizoSchema.Z0_Number);
			ZSqlParameter parameter1B = ZSqlParameter.New("@P1", 2, DummyBizoSchema.Z0_Number);
			ZSqlParameter parameterWhoseNameStartsLikeParamater1 = ZSqlParameter.New("@P1Bis", "foo", DummyBizoSchema.Z0_Code);

			ZQuery query1 = new ZQuery();
			string sqlText = @"Z0_Number > @P1 and Z0_Code = @P1Bis";
			ZSqlParameterCollection sqlParameters = new ZSqlParameterCollection();
			sqlParameters.Add(parameter1A);
			sqlParameters.Add(parameterWhoseNameStartsLikeParamater1);
			query1.AddFilterAndZSQLParameterCollection(sqlText, sqlParameters);

			ZQuery query2 = new ZQuery();
			sqlText = @"Z0_Number < @P1 and Z0_Code = @P1Bis";
			sqlParameters = new ZSqlParameterCollection();
			sqlParameters.Add(parameterWhoseNameStartsLikeParamater1);
			sqlParameters.Add(parameter1B);
			query2.AddFilterAndZSQLParameterCollection(sqlText, sqlParameters);

			FilterStringBuilder filterStringBuilder = new FilterStringBuilder();
			filterStringBuilder.Append(query1);
			filterStringBuilder.Append(JoinCondition.Or, query2);

			var builder = new SqlFilterPartAppender(SqlBuilder.QueryType.Parameterised);
			ZNonPersistentDataQuery resultQuery = builder.GetParameterisedSql(filterStringBuilder);

			CombineAssertions(delegate
			{
				AssertEquals("Parameter Count", 3, resultQuery.Parameters.Length);
				AssertEquals("Query text", "(Z0_Number > @P1 and Z0_Code = @P1Bis) or (Z0_Number < @P11 and Z0_Code = @P1Bis)", resultQuery.ParameterisedQueryText);
				AssertEquals("builder.ToString()", "(Z0_Number > 1 and Z0_Code = 'foo') or (Z0_Number < 2 and Z0_Code = 'foo')", resultQuery.LiteralTextADO);
			});
		}

		public void TestTwoTableValuedParamsWithSameNameAndDifferentValuesIsHandled()
		{
			var parameter1A = ZSqlParameter.New("@P1List", new List<string> { "ABC", "DEF" }, DummyBizoSchema.Z0_NVarChar, true);
			var parameter1B = ZSqlParameter.New("@P1List", new List<string> { "GHI", "JKL" }, DummyBizoSchema.Z0_NVarChar, true);
			var parameter2 = ZSqlParameter.New("@P2", 2, DummyBizoSchema.Z0_Number);

			var query1 = new ZQuery();
			var sqlText = @"Z0_NVarChar in (SELECT Value FROM @P1List) AND Z0_Number > @P2";
			var sqlParameters = new ZSqlParameterCollection();
			sqlParameters.Add(parameter1A);
			sqlParameters.Add(parameter2);
			query1.AddFilterAndZSQLParameterCollection(sqlText, sqlParameters);

			var query2 = new ZQuery();
			sqlText = @"Z0_NVarChar in (SELECT Value FROM @P1List) AND Z0_Number < @P2";
			sqlParameters = new ZSqlParameterCollection();
			sqlParameters.Add(parameter1B);
			query2.AddFilterAndZSQLParameterCollection(sqlText, sqlParameters);

			var filterStringBuilder = new FilterStringBuilder();
			filterStringBuilder.Append(query1);
			filterStringBuilder.Append(JoinCondition.Or, query2);

			var builder = new SqlFilterPartAppender(SqlBuilder.QueryType.Parameterised);
			var resultQuery = builder.GetParameterisedSql(filterStringBuilder);

			CombineAssertions(delegate
			{
				AssertEquals("Parameter Count", 3, resultQuery.Parameters.Length);
				AssertEquals("Query text", "(Z0_NVarChar in (SELECT Value FROM @P1List) AND Z0_Number > @P2) or (Z0_NVarChar in (SELECT Value FROM @P1List1) AND Z0_Number < @P2)", resultQuery.ParameterisedQueryText);
				AssertEquals("builder.ToString()", "(Z0_NVarChar in ('ABC', 'DEF') AND Z0_Number > 2) or (Z0_NVarChar in ('GHI', 'JKL') AND Z0_Number < 2)", resultQuery.LiteralTextADO);

				var p1A = resultQuery.Parameters.Single(p => p.ParameterName == "@P1List");
				var p1B = resultQuery.Parameters.Single(p => p.ParameterName == "@P1List1");
				var p2 = resultQuery.Parameters.Single(p => p.ParameterName == "@P2");

				Assert(p1A.IsTableValued);
				Assert(p1B.IsTableValued);
				Assert(!p2.IsTableValued);
			});
		}

		public void TestLiteralTextADO()
		{
			ZQuery result = new ZQuery();

			ZQuery query1 = new ZQuery();
			ZQuery query11 = new ZQuery();
			string sqlText = @"Z0_Number > @P1A and Z0_Code = @P1B";
			ZSqlParameterCollection sqlParameters = new ZSqlParameterCollection();
			ZSqlParameter parameter1A = ZSqlParameter.New("@P1A", 1, DummyBizoSchema.Z0_Number);
			ZSqlParameter parameter1B = ZSqlParameter.New("@P1B", "foo", DummyBizoSchema.Z0_Code);
			sqlParameters.Add(parameter1A);
			sqlParameters.Add(parameter1B);
			query11.AddFilterAndZSQLParameterCollection(sqlText, sqlParameters);
			query1.AddToFilter(query11);

			ZQuery query2 = new ZQuery();
			ZQuery query21 = new ZQuery();
			sqlText = @"Z0_Number < @P2A and Z0_Code = @P2B";
			sqlParameters = new ZSqlParameterCollection();
			ZSqlParameter parameter2A = ZSqlParameter.New("@P2A", 2, DummyBizoSchema.Z0_Number);
			ZSqlParameter parameter2B = ZSqlParameter.New("@P2B", "foo2", DummyBizoSchema.Z0_Code);
			sqlParameters.Add(parameter2A);
			sqlParameters.Add(parameter2B);
			query21.AddFilterAndZSQLParameterCollection(sqlText, sqlParameters);
			query2.AddToFilter(query21);

			result.AddToFilter(query1);
			result.AddToFilter(query2);

			AssertEquals("ParameterisedText.ParameterisedQueryText", "(Z0_Number > @P1A and Z0_Code = @P1B) and (Z0_Number < @P2A and Z0_Code = @P2B)", result.ParameterisedText.ParameterisedQueryText);
			AssertEquals("ParameterisedText.LiteralTextADO", "(Z0_Number > 1 and Z0_Code = 'foo') and (Z0_Number < 2 and Z0_Code = 'foo2')", result.ParameterisedText.LiteralTextADO);
			AssertEquals("LiteralTextADO", "(Z0_Number > 1 and Z0_Code = 'foo') and (Z0_Number < 2 and Z0_Code = 'foo2')", result.LiteralTextADO);
		}
	}
}
