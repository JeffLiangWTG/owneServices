#pragma warning disable NUnit1028 // The non-test method is public.
using System.Data;
using System.Data.SqlTypes;
using CargoWise.Data.SqlProxy.Interface.Converters;

namespace CargoWise.Data.SqlProxy.Interface.Test.EndToEndTest;

[WTG.StaticAnalysis.Annotation.CodeAlive("Test Class")]
class SqlValueConverterTest
{
	[TestCaseSource(typeof(JsonConverterTestCaseSources), nameof(JsonConverterTestCaseSources.SqlTestCases))]
	public void TestSqlValueConverter(string query, SqlDbType expectedDataType, Action<object, object> assertEqual)
	{
		// Arrange
		var rawValue = ExecuteReader(query, expectedDataType);

		// Act
		var serialized = SqlValueConverter.ToJson(rawValue, expectedDataType);
		var deserialized = SqlValueConverter.FromJson(serialized, expectedDataType);

		// Assert
		assertEqual(deserialized, rawValue);
	}

	[TestCase("<xml><type>a</type><value>1</value></xml>", DbType.Xml, typeof(SqlXml))]
	public void TestCsValueToSqlValue(string csValue, DbType dbType, Type targetType)
	{
		var sqlDbValue = SqlValueConverter.CsValueToSqlValue(csValue, dbType);
		Assert.That(sqlDbValue.GetType(), Is.EqualTo(targetType));
	}

	object ExecuteReader(string sql, SqlDbType expectedDataType)
	{
		using var connection = TestHelper.OpenLocalSqlConnection();
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		using var reader = command.ExecuteReader();

		var read = reader.Read();

		Assert.That(read, Is.True);
		Assert.That(reader.FieldCount, Is.EqualTo(1));

		var expectedDataTypeName = TypeMappingHelper.SqlDbTypeAsString(expectedDataType);
		Assert.That(reader.GetDataTypeName(0), Is.EqualTo(expectedDataTypeName));

		return reader.GetSqlValue(0);
	}

	[TestCaseSource(typeof(JsonConverterTestCaseSources), nameof(JsonConverterTestCaseSources.SqlTestCases))]
	public void TestResultingCsType(string query, SqlDbType expectedDataType, Action<object, object> assertEqual)
	{
		using var connection = TestHelper.OpenLocalSqlConnection();
		using var command = connection.CreateCommand();
		command.CommandText = query;
		using var reader = command.ExecuteReader();

		var read = reader.Read();
		Assert.That(read, Is.True);
		Assert.That(reader.FieldCount, Is.EqualTo(1));

		var sqlValue = reader.GetSqlValue(0);

		// Test if getting csValue throws
		object csValue;
		try
		{
			csValue = reader.GetValue(0);
		}
		catch (Exception ex)
		{
			// If getting it from native library code throws, we should also throw
			Assert.Throws(ex.GetType(), () => SqlValueConverter.SqlValueToCsValue(sqlValue));
			return;
		}

		var converted = SqlValueConverter.SqlValueToCsValue(sqlValue);
		Assert.That(converted!.GetType(), Is.EqualTo(csValue.GetType()));

		// Ensure type mappings also match, except for DBNull which is an edge case
		if (converted is not DBNull)
		{
			var csType = TypeMappingHelper.SqlDbTypeToCsType(expectedDataType);
			Assert.That(converted.GetType(), Is.EqualTo(csType));
		}
	}

	[TestCaseSource(typeof(JsonConverterTestCaseSources), nameof(JsonConverterTestCaseSources.SqlTestCases))]
	public void TestEndToEndCsTypeTransfer(string query, SqlDbType expectedDataType, Action<object, object> assertEqual)
	{
		using var connection = TestHelper.OpenLocalSqlConnection();
		using var command = connection.CreateCommand();
		command.CommandText = query;
		using var reader = command.ExecuteReader();

		var read = reader.Read();
		Assert.That(read, Is.True);
		Assert.That(reader.FieldCount, Is.EqualTo(1));

		object csValue;
		try
		{
			csValue = reader.GetValue(0);
		}
		catch
		{
			// If the native data reader can't handle the value, then it's not our problem
			return;
		}

		var sqlDataTypeName = reader.GetDataTypeName(0);

		var sqlDbType = TypeMappingHelper.StringToSqlDbType(sqlDataTypeName);
		var dbType = TypeMappingHelper.SqlDbTypeToDbType(sqlDbType);

		var sqlDbValue = SqlValueConverter.CsValueToSqlValue(csValue, dbType);
		var json = SqlValueConverter.ToJson(sqlDbValue, sqlDbType);

		var deserialized = SqlValueConverter.FromJson(json, sqlDbType);
		var resultingCsValue = SqlValueConverter.SqlValueToCsValue(deserialized);
		var resultingDbType = TypeMappingHelper.SqlDbTypeToDbType(sqlDbType);

		Assert.That(resultingDbType, Is.EqualTo(dbType));
		Assert.That(resultingCsValue, Is.EqualTo(csValue));
	}
}
