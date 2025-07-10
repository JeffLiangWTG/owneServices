using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Xml;
using CargoWise.Data.SqlProxy.Interface.Converters;
using CargoWise.Data.SqlProxy.Interface.Models;
using Microsoft.SqlServer.Types;

namespace CargoWise.Data.SqlProxy.Interface.Test.EndToEndTest;

public class SqlValueTest
{
	[Test]
	public void ExecuteReaderHavingValuesOfCompleteSqlTypes()
	{
		const string sql = $"SELECT TOP 1 * FROM {TestTableName}";
		using var reader = sqlConnection?.ExecuteReader(sql);
		Assert.That(reader, Is.Not.Null);

		var schemaTable = reader!.GetSchemaTable();
		if (schemaTable != null)
		{
			File.WriteAllText(@"C:\ProgramData\schema.txt", Newtonsoft.Json.JsonConvert.SerializeObject(schemaTable));
		}

		Assert.That(schemaTable, Is.Not.Null);
		Assert.That(reader.Read(), Is.True);

		Assert.Multiple(() =>
		{
			Assert.That(reader.GetValue(0), Is.Not.Null);
			Assert.That(schemaTable!.Rows[0]["ProviderType"], Is.EqualTo((int)SqlDbType.Int));
			Assert.That(reader.GetDataTypeName(0), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.Int)).IgnoreCase);

			Assert.That(reader.GetValue(1), Is.InstanceOf<byte[]>());
			Assert.That(schemaTable.Rows[1]["ProviderType"], Is.EqualTo((int)SqlDbType.VarBinary));
			Assert.That(reader.GetDataTypeName(1), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.VarBinary)).IgnoreCase);

			Assert.That(reader.GetValue(2), Is.InstanceOf<bool>());
			Assert.That(schemaTable.Rows[2]["ProviderType"], Is.EqualTo((int)SqlDbType.Bit));
			Assert.That(reader.GetDataTypeName(2), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.Bit)).IgnoreCase);

			Assert.That(reader.GetValue(3), Is.InstanceOf<byte>());
			Assert.That(schemaTable.Rows[3]["ProviderType"], Is.EqualTo((int)SqlDbType.TinyInt));
			Assert.That(reader.GetDataTypeName(3), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.TinyInt)).IgnoreCase);

			Assert.That(reader.GetValue(4), Is.InstanceOf<byte[]>());
			Assert.That(schemaTable.Rows[4]["ProviderType"], Is.EqualTo((int)SqlDbType.VarBinary));
			Assert.That(reader.GetDataTypeName(4), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.VarBinary)).IgnoreCase);

			Assert.That(reader.GetValue(5), Is.InstanceOf<string>());
			Assert.That(schemaTable.Rows[5]["ProviderType"], Is.EqualTo((int)SqlDbType.NVarChar));
			Assert.That(reader.GetDataTypeName(5), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.NVarChar)).IgnoreCase);

			Assert.That(reader.GetValue(6), Is.InstanceOf<DateTime>());
			Assert.That(schemaTable.Rows[6]["ProviderType"], Is.EqualTo((int)SqlDbType.DateTime));
			Assert.That(reader.GetDataTypeName(6), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.DateTime)).IgnoreCase);

			Assert.That(reader.GetValue(7), Is.InstanceOf<decimal>());
			Assert.That(schemaTable.Rows[7]["ProviderType"], Is.EqualTo((int)SqlDbType.Decimal));
			Assert.That(reader.GetDataTypeName(7), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.Decimal)).IgnoreCase);

			Assert.That(reader.GetValue(8), Is.InstanceOf<double>());
			Assert.That(schemaTable.Rows[8]["ProviderType"], Is.EqualTo((int)SqlDbType.Float));
			Assert.That(reader.GetDataTypeName(8), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.Float)).IgnoreCase);

			Assert.That(reader.GetValue(9), Is.InstanceOf<Guid>());
			Assert.That(schemaTable.Rows[9]["ProviderType"], Is.EqualTo((int)SqlDbType.UniqueIdentifier));
			Assert.That(reader.GetDataTypeName(9), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.UniqueIdentifier)).IgnoreCase);

			Assert.That(reader.GetValue(10), Is.InstanceOf<short>());
			Assert.That(schemaTable.Rows[10]["ProviderType"], Is.EqualTo((int)SqlDbType.SmallInt));
			Assert.That(reader.GetDataTypeName(10), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.SmallInt)).IgnoreCase);

			Assert.That(reader.GetValue(11), Is.InstanceOf<int>());
			Assert.That(schemaTable.Rows[11]["ProviderType"], Is.EqualTo((int)SqlDbType.Int));
			Assert.That(reader.GetDataTypeName(11), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.Int)).IgnoreCase);

			Assert.That(reader.GetValue(12), Is.InstanceOf<long>());
			Assert.That(schemaTable.Rows[12]["ProviderType"], Is.EqualTo((int)SqlDbType.BigInt));
			Assert.That(reader.GetDataTypeName(12), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.BigInt)).IgnoreCase);

			Assert.That(reader.GetValue(13), Is.InstanceOf<decimal>());
			Assert.That(schemaTable.Rows[13]["ProviderType"], Is.EqualTo((int)SqlDbType.Money));
			Assert.That(reader.GetDataTypeName(13), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.Money)).IgnoreCase);

			Assert.That(reader.GetValue(14), Is.InstanceOf<float>());
			Assert.That(schemaTable.Rows[14]["ProviderType"], Is.EqualTo((int)SqlDbType.Real));
			Assert.That(reader.GetDataTypeName(14), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.Real)).IgnoreCase);

			Assert.That(reader.GetValue(15), Is.InstanceOf<string>());
			Assert.That(schemaTable.Rows[15]["ProviderType"], Is.EqualTo((int)SqlDbType.NVarChar));
			Assert.That(reader.GetDataTypeName(15), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.NVarChar)).IgnoreCase);

			Assert.That(reader.GetValue(16), Is.InstanceOf<string>());
			Assert.That(schemaTable.Rows[16]["ProviderType"], Is.EqualTo((int)SqlDbType.Xml));
			Assert.That(reader.GetDataTypeName(16), Is.EqualTo(Enum.GetName(typeof(SqlDbType), SqlDbType.Xml)).IgnoreCase);

			Assert.That(reader.GetValue(17), Is.InstanceOf<SqlGeography>());
			Assert.That(schemaTable.Rows[17]["ProviderType"], Is.EqualTo((int)SqlDbType.Udt));
			Assert.That(reader.GetDataTypeName(17), Does.EndWith("GEOGRAPHY").IgnoreCase);

			Assert.That(reader.GetValue(18), Is.InstanceOf<SqlGeometry>());
			Assert.That(reader.GetDataTypeName(18), Does.EndWith("GEOMETRY").IgnoreCase);
			Assert.That(schemaTable.Rows[18]["ProviderType"], Is.EqualTo((int)SqlDbType.Udt));

			Assert.That(reader.GetValue(19), Is.InstanceOf<SqlHierarchyId>());
			Assert.That(reader.GetDataTypeName(19), Does.EndWith("HIERARCHYID").IgnoreCase);
			Assert.That(schemaTable.Rows[19]["ProviderType"], Is.EqualTo((int)SqlDbType.Udt));

			for (var col = 1; col <= 19; col++)
			{
				AssertSqlValueSerialization(reader, col);
			}
		});
	}

	[TestCaseSource(nameof(SqlVariantTestCases))]
	public void ExecuteReaderHavingSqlVariantValues(object value)
	{
		sqlConnection?.ExecuteNonQuery($"DELETE FROM {TestTableNameSqlVariant}");
		sqlConnection?.ExecuteNonQuery(InsertSqlVariantDataScript, command =>
		{
			command.Parameters.Add(new SqlParameter("@SqlVariant", SqlDbType.Variant) { Value = value });
		});

		const string sql = $"SELECT TOP 1 * FROM {TestTableNameSqlVariant}";
		using var reader = sqlConnection?.ExecuteReader(sql);
		Assert.That(reader, Is.Not.Null);

		var schemaTable = reader!.GetSchemaTable();
		Assert.That(schemaTable, Is.Not.Null);
		Assert.That(reader.Read(), Is.True);

		Assert.That(schemaTable.Rows[0]["ProviderType"], Is.EqualTo((int)SqlDbType.Variant));
		Assert.That(reader.GetDataTypeName(0), Is.EqualTo("sql_variant"));

		var actual = reader.GetValue(0);
		Assert.That(actual, Is.EqualTo(value));

		AssertSqlValueSerialization(reader, 0);
	}

	static void AssertSqlValueSerialization(SqlDataReader reader, int col)
	{
		var value = reader.GetValue(col);
		var columnDataTypeName = reader.GetDataTypeName(col);

		var sqlValue = new SqlValue(value, columnDataTypeName);
		var csValue = SqlValueConverter.SqlValueToCsValue(sqlValue.ToSqlValue(columnDataTypeName));

		switch (value)
		{
			case SqlGeography sqlGeography:
			{
				var actualValue = csValue as SqlGeography;
				Assert.That(actualValue, Is.Not.Null);
				Assert.That(actualValue!.STEquals(sqlGeography).IsTrue, Is.True);
				break;
			}
			case SqlGeometry sqlGeometry:
			{
				var actualValue = csValue as SqlGeometry;
				Assert.That(actualValue, Is.Not.Null);
				Assert.That(actualValue!.STEquals(sqlGeometry).IsTrue, Is.True);
				break;
			}
			default:
			{
				Assert.That(csValue, Is.EqualTo(value));
				break;
			}
		}
	}

	[OneTimeSetUp]
	public void OnTimeSetup()
	{
		sqlConnection = TestHelper.OpenLocalSqlConnection();
		sqlConnection.CreateDatabaseDropExisting(TestDatabaseName);
		sqlConnection.ExecuteNonQuery(CreateTableScript);
		sqlConnection.ExecuteNonQuery(CreateSqlVariantTableScript);

		sqlConnection.ExecuteNonQuery(InsertDataScript, command =>
		{
			command.Parameters.AddWithValue("@Binary", new SqlBinary([0x01, 0x02, 0x03]));
			command.Parameters.AddWithValue("@Bit", new SqlBoolean(true));
			command.Parameters.AddWithValue("@TinyInt", new SqlByte(255));
			command.Parameters.AddWithValue("@VarBinaryMax", new SqlBytes([0x04, 0x05, 0x06]));
			command.Parameters.AddWithValue("@NVarCharMax", new SqlChars("Large text data".ToCharArray()));
			command.Parameters.AddWithValue("@DateTime", new SqlDateTime(2025, 5, 6, 12, 0, 0));
			command.Parameters.AddWithValue("@Decimal", new SqlDecimal(123.45m));
			command.Parameters.AddWithValue("@Float", new SqlDouble(123.456));
			command.Parameters.AddWithValue("@UniqueIdentifier", new SqlGuid(Guid.NewGuid()));
			command.Parameters.AddWithValue("@SmallInt", new SqlInt16(1234));
			command.Parameters.AddWithValue("@Int", new SqlInt32(123456));
			command.Parameters.AddWithValue("@BigInt", new SqlInt64(1234567890));
			command.Parameters.AddWithValue("@Money", new SqlMoney(999.99));
			command.Parameters.AddWithValue("@Real", new SqlSingle(123.45f));
			command.Parameters.AddWithValue("@NVarChar", new SqlString("Test string"));
			command.Parameters.AddWithValue("@Xml", new SqlXml(new XmlTextReader(new StringReader("<root>Test</root>"))));

			command.Parameters.Add(new SqlParameter
			{
				ParameterName = "@Geography",
				SqlDbType = SqlDbType.Udt,
				UdtTypeName = "GEOGRAPHY",
				Value = SqlGeography.Point(47.6, -122.3, 4326)
			});

			command.Parameters.Add(new SqlParameter
			{
				ParameterName = "@Geometry",
				SqlDbType = SqlDbType.Udt,
				UdtTypeName = "GEOMETRY",
				Value = SqlGeometry.Parse("POINT(1 1)")
			});

			command.Parameters.Add(new SqlParameter
			{
				ParameterName = "@HierarchyId",
				SqlDbType = SqlDbType.Udt,
				UdtTypeName = "HIERARCHYID",
				Value = SqlHierarchyId.Parse("/1/")
			});
		});
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		sqlConnection?.Dispose();
		TestHelper.DropDatabaseIfExists(TestHelper.OpenLocalSqlConnection(), TestDatabaseName);
	}

	SqlConnection? sqlConnection;

	static IEnumerable<object> SqlVariantTestCases()
	{
		yield return 42;
		yield return "TestString";
		yield return new DateTime(2025, 5, 6, 12, 0, 0);
		yield return 123.45m;
		yield return true;
		yield return Guid.NewGuid();
		yield return new byte[] { 0x01, 0x02, 0x03 };
		yield return DBNull.Value;
	}

	const string TestDatabaseName = nameof(SqlValueTest);
	const string TestTableNameSqlVariant = nameof(SqlValueTest) + nameof(SqlDbType.Variant);

	const string CreateSqlVariantTableScript = $@"
IF OBJECT_ID('dbo.{TestTableNameSqlVariant}', 'U') IS NOT NULL
	DROP TABLE dbo.{TestTableNameSqlVariant}
;

CREATE TABLE dbo.{TestTableNameSqlVariant}
(
	SqlVariantCol         SQL_VARIANT
);
";
	const string InsertSqlVariantDataScript = $@"
INSERT INTO {TestTableNameSqlVariant} (
	SqlVariantCol
) VALUES (
	@SqlVariant
)";

	const string TestTableName = nameof(SqlValueTest);
	const string CreateTableScript = $@"
IF OBJECT_ID('dbo.{TestTableName}', 'U') IS NOT NULL
	DROP TABLE dbo.{TestTableName};

CREATE TABLE dbo.{TestTableName}
(
	Id INT IDENTITY(1,1)  PRIMARY KEY,
	BinaryCol             VARBINARY(50),
	BitCol                BIT,
	TinyIntCol            TINYINT,
	VarBinaryMaxCol       VARBINARY(MAX),
	NVarCharMaxCol        NVARCHAR(MAX),
	DateTimeCol           DATETIME,
	DecimalCol            DECIMAL(18,2),
	FloatCol              FLOAT,
	UniqueIdentifierCol   UNIQUEIDENTIFIER,
	SmallIntCol           SMALLINT,
	IntCol                INT,
	BigIntCol             BIGINT,
	MoneyCol              MONEY,
	RealCol               REAL,
	NVarCharCol           NVARCHAR(100),
	XmlCol                XML,
	GeographyCol          GEOGRAPHY,
	GeometryCol           GEOMETRY,
	HierarchyIdCol        HIERARCHYID,
);
";

	const string InsertDataScript = $@"
INSERT INTO {TestTableName} (
	BinaryCol,
	BitCol,
	TinyIntCol,
	VarBinaryMaxCol,
	NVarCharMaxCol,
	DateTimeCol,
	DecimalCol,
	FloatCol,
	UniqueIdentifierCol,
	SmallIntCol,
	IntCol,
	BigIntCol,
	MoneyCol,
	RealCol,
	NVarCharCol,
	XmlCol,
	GeographyCol,
	GeometryCol,
	HierarchyIdCol
) VALUES (
	@Binary,
	@Bit,
	@TinyInt,
	@VarBinaryMax,
	@NVarCharMax,
	@DateTime,
	@Decimal,
	@Float,
	@UniqueIdentifier,
	@SmallInt,
	@Int,
	@BigInt,
	@Money,
	@Real,
	@NVarChar,
	@Xml,
	@Geography,
	@Geometry,
	@HierarchyId
);
";
}
