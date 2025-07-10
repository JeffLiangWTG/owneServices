using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc.Testing
{
	[TestedType(typeof(GetNextDateAsDateTime))]
	class GetNextDateAsDateTimeTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			CombineAssertions(() =>
			{
				AssertNextDateResult(new SqlDateTime(2020, 11, 19));
				AssertNextDateResult(new SqlDateTime(2020, 11, 19, 10, 11, 12));
				AssertNextDateResult(new SqlDateTime(2020, 11, 19, 23, 59, 59));
				AssertNextDateResult(new SqlDateTime(DateTime.UtcNow));
				GetNextDateTestHelper.AssertReturnsNullOnNullInput(TestConnection, ScriptToTest);
			});
		}

		void AssertNextDateResult(SqlDateTime inputDateTime) => GetNextDateTestHelper.AssertNextDateResult(TestConnection, ScriptToTest, inputDateTime);

		public void TestReturnTypeIsDateTime()
		{
			GetNextDateTestHelper.AssertReturnType(TestConnection, ScriptToTest, "datetime");
		}
	}

	static class GetNextDateTestHelper
	{
		public static void AssertNextDateResult(DbConnection connection, IDbScript scriptToTest, SqlDateTime inputDateTime)
		{
			var sql = $"SELECT {scriptToTest.SchemaName}.{scriptToTest.Name}('{SqlFormatInfo.ToSqlDateTimeString(inputDateTime.Value)}')";
			var result = connection.ExecuteScalar<DateTime>(sql);
			Assertion.AssertEquals($"{scriptToTest}({inputDateTime.Value})", inputDateTime.Value.Date.AddDays(1), result);
		}

		public static void AssertReturnsNullOnNullInput(DbConnection connection, IDbScript scriptToTest)
		{
			var sql = $"SELECT {scriptToTest.SchemaName}.{scriptToTest.Name}(NULL)";
			var result = connection.ExecuteScalar(sql);
			Assertion.AssertEquals($"{scriptToTest}(NULL)", DBNull.Value, result);
		}

		public static void AssertReturnType(DbConnection connection, IDbScript scriptToTest, string expectedType)
		{
			var sql = $@"
				DECLARE @SqlVariantResult SQL_VARIANT = {scriptToTest.SchemaName}.{scriptToTest.Name}(GETDATE());
				SELECT SQL_VARIANT_PROPERTY(@SqlVariantResult, 'BaseType');";
			var result = connection.ExecuteScalar<string>(sql);
			Assertion.AssertEquals($"{scriptToTest} return type", expectedType, result.ToLowerInvariant());
		}
	}
}

