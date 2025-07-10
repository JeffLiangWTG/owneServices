using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.DB.Sql
{
	public class FindByCriteriaStatementTest : TransactionedTestCase
	{
		public void TestGenerate()
		{
			var result = sql.Generate();
			AssertEquals("SELECT DummyBizo.* FROM dbo.DummyBizo WHERE (Z0_Bool=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "N", SqlDbType.VarChar);
		}

		public void TestCreateWhereStatement()
		{
			var criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_PK", Value = pk.ToString() };
			var result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_PK=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", pk.ToString(), SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Bool", Value = false };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Bool=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "N", SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Bool", Value = true };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Bool=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "Y", SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Code", Value = "ABC'" };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Code=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "ABC'", SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Guid", Value = DBNull.Value };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Guid IS NULL)", result);
			AssertEquals(0, keyToParameters.Count);

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Date", Value = new DateTime(2001, 09, 19, 1, 2, 3) };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Date=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "2001-09-19T01:02:03", SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_DateTimeOffset", Value = new DateTimeOffset(2001, 09, 19, 1, 2, 3, 123, TimeSpan.FromHours(8)) };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_DateTimeOffset=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "2001-09-19T01:02:03.1230000+08:00", SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Time", Value = new TimeSpan(1, 2, 3) };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Time=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "01:02:03", SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Geography", Value = SqlGeography.STGeomFromText(new SqlChars("POINT (121 41)"), 4326) };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Geography=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "POINT (121 41)", SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_NVarChar", Value = "Freddo Frog" };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_NVarChar=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "Freddo Frog", SqlDbType.NVarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_NVarChar", Value = "Freddo Frog%" };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_NVarChar LIKE @param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "Freddo Frog%", SqlDbType.NVarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_NVarChar", Value = "%Freddo Frog" };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_NVarChar LIKE @param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "%Freddo Frog", SqlDbType.NVarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_NVarChar", Value = "\r\n Freddo Frog\t" };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("White Spaces should be trimed", "(Z0_NVarChar=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "Freddo Frog", SqlDbType.NVarChar);
			keyToParameters.Clear();
		}

		public void TestCreateWhereStatement_StringValueRepresentingBoolean()
		{
			var criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Bool", Value = "true" };
			var result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Bool=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "Y", SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Bool", Value = "Y" };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Bool=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "Y", SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Bool", Value = "false" };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Bool=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "N", SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Bool", Value = "N" };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Bool=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "N", SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Bool", Value = "blah" };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Bool=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "N", SqlDbType.VarChar);
			keyToParameters.Clear();

			criteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Bool", Value = "" };
			result = sql.CreateWhereSearchCondition(criteria);
			AssertEquals("(Z0_Bool=@param1)", result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "N", SqlDbType.VarChar);
			keyToParameters.Clear();
		}

		public void TestTVP()
		{
			var tvpParameters = new Dictionary<string, TvpItem>();
			SetupFindByCriteriaStatement(tvpParameters);
			var sqlString = sql.Generate();
			AssertEquals("SELECT DummyBizo.* FROM dbo.DummyBizo WHERE (Z0_Bool in (SELECT value FROM @tvpParam0))", sqlString);
			AssertContainsExactElementsInAnyOrder(new[] { false }, tvpParameters.Values.Single().Values);
		}

		void AssertContainsParameter(string paramName, string paramValue, SqlDbType paramType)
		{
			foreach (var keyToParam in keyToParameters)
			{
				var param = keyToParam.Value;
				if (param.Name == paramName && param.Value == paramValue && param.Type == paramType)
				{
					return;
				}
			}
			Fail(string.Format("parameters doesn't contain parameter with name {0}, value {1}, type {2}", paramName, paramValue, paramType));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
			pk = TestUtil.PrepareDummyBizoData();

			SetupFindByCriteriaStatement(null);
		}

		void SetupFindByCriteriaStatement(Dictionary<string, TvpItem> tvpParameters)
		{
			var boolCriteria = new Criteria { TableName = "DummyBizo", ColumnName = "Z0_Bool", Value = false };
			keyToParameters = new Dictionary<string, SqlParameter>();

			sql = new FindByCriteriaStatement(new TableBuilder().Construct("DummyBizo"), null, new[] { boolCriteria }, keyToParameters, tvpParameters);
		}

		#endregion

		FindByCriteriaStatement sql;
		Guid pk;
		Dictionary<string, SqlParameter> keyToParameters;
	}
}
