using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;

namespace CargoWise.Data.SqlProxy.Interface.Test;

#pragma warning disable CW1122 // Do Not Use DateTime Parse Method
public static class JsonConverterTestCaseSources
{
	public static IEnumerable<object?> PrimitiveTypesTestData()
	{
		yield return 1234.123456789f; // float 4 bytes, precision ~6-9 digits
		yield return 1234.987654321f;
		yield return 123456789.1234f;
		yield return float.MaxValue;
		yield return float.MinValue;
		yield return 1234.123456789012345d; // double 8 bytes, precision ~15-17 digits
		yield return 1234.123456789054321d;
		yield return 123456789012345.6789d;
		yield return double.MaxValue;
		yield return double.MinValue;
		yield return 12345.0123456789012345678912345m; // decimal 16 bytes, precision 28-29 digits
		yield return 1234567890123456789012345.12345m;
		yield return decimal.MaxValue;
		yield return decimal.MinValue;
		yield return Guid.Parse("F34B1044-428F-421B-B919-F90D85B6DF4D");
		yield return DateTime.Parse("2023-12-11T07:22:16.7752486", CultureInfo.InvariantCulture);
		yield return DateTime.Parse("2023-12-11T07:22:16.7752486Z", CultureInfo.InvariantCulture);
		yield return new DateTimeOffset(DateTime.Parse("2023-12-12T10:42:16.7752486Z", CultureInfo.InvariantCulture));
		yield return "2023-09-10T19:07:00";
		yield return Encoding.UTF8.GetBytes(nameof(JsonHelperTest));
		yield return nameof(JsonHelperTest);
		yield return true;
		yield return false;
		yield return short.MaxValue;
		yield return short.MinValue;
		yield return ushort.MaxValue;
		yield return ushort.MinValue;
		yield return int.MaxValue;
		yield return int.MinValue;
		yield return uint.MaxValue;
		yield return uint.MinValue;
		yield return long.MaxValue;
		yield return long.MinValue;
		yield return ulong.MaxValue;
		yield return ulong.MinValue;
		yield return 'c';
		yield return Convert.ToByte('x');
		yield return null;
	}

	public static IEnumerable<string> SqlDataTypes()
	{
		yield return "bigint";
		yield return "binary";
		yield return "bit";
		yield return "char";
		yield return "date";
		yield return "datetime";
		yield return "datetime2";
		yield return "datetimeoffset";
		yield return "decimal";
		yield return "filestream";
		yield return "float";
		yield return "image";
		yield return "int";
		yield return "money";
		yield return "nchar";
		yield return "ntext";
		yield return "numeric";
		yield return "nvarchar";
		yield return "real";
		yield return "rowversion";
		yield return "smalldatetime";
		yield return "smallint";
		yield return "smallmoney";
		yield return "sql_variant";
		yield return "text";
		yield return "time";
		yield return "timestamp";
		yield return "tinyint";
		yield return "uniqueidentifier";
		yield return "varbinary";
		yield return "varchar";
		yield return "xml";
	}

	public static IEnumerable<Type> SqlEquivalentTypes()
	{
		yield return typeof(bool);
		yield return typeof(char);
		yield return typeof(float);
		yield return typeof(double);
		yield return typeof(decimal);
		yield return typeof(sbyte);
		yield return typeof(short);
		yield return typeof(int);
		yield return typeof(long);
		yield return typeof(byte);
		yield return typeof(ushort);
		yield return typeof(uint);
		yield return typeof(ulong);
		yield return typeof(byte[]);
		yield return typeof(string);
		yield return typeof(DateTime);
		yield return typeof(DateTimeOffset);
		yield return typeof(TimeSpan);
		yield return typeof(Guid);
		yield return typeof(DBNull);
	}

	public static IEnumerable<TestCaseData> SqlTestCases()
	{
		return
		[
			// BIGINT
			MakeCase("SELECT CAST(0 AS BIGINT)", SqlDbType.BigInt), // unbox long and box to int
			MakeCase("SELECT CAST(42 AS BIGINT)", SqlDbType.BigInt), // unbox long and box to int
			MakeCase("SELECT CAST(-123456789 AS BIGINT)", SqlDbType.BigInt), // unbox long and box to int
			MakeCase("SELECT CAST(999999999999999999 AS BIGINT)", SqlDbType.BigInt), // long
			MakeCase("SELECT CAST(9223372036854775807 AS BIGINT)", SqlDbType.BigInt), // Max value
			MakeCase("SELECT CAST(-9223372036854775808 AS BIGINT)", SqlDbType.BigInt), // Min value
			MakeCase("SELECT CAST(NULL AS BIGINT)", SqlDbType.BigInt), // unbox long and box to int

			// BINARY (fixed-length)
			MakeCase("SELECT CAST(0x00 AS BINARY(1))", SqlDbType.Binary),
			MakeCase("SELECT CAST(0x0123456789ABCDEF AS BINARY(8))", SqlDbType.Binary),
			MakeCase("SELECT CAST(REPLICATE(CAST(0xAA AS BINARY(1)), 16) AS BINARY(16))", SqlDbType.Binary), // Arbitrary chunk
			MakeCase("SELECT CAST(0xFFEE AS BINARY(2))", SqlDbType.Binary), // Another small example
			MakeCase("SELECT CAST(NULL AS BINARY(10))", SqlDbType.Binary),

			// BIT
			MakeCase("SELECT CAST(0 AS BIT)", SqlDbType.Bit),
			MakeCase("SELECT CAST(1 AS BIT)", SqlDbType.Bit),
			MakeCase("SELECT CAST(NULL AS BIT)", SqlDbType.Bit),

			// CHAR
			MakeCase("SELECT CAST('A' AS CHAR(1))", SqlDbType.Char),
			MakeCase("SELECT CAST('Test' AS CHAR(10))", SqlDbType.Char),  // Padded with spaces
			MakeCase("SELECT CAST('!@#$%^&*()' AS CHAR(10))", SqlDbType.Char),  // Special characters
			MakeCase("SELECT CAST(REPLICATE('x', 8000) AS CHAR(8000))", SqlDbType.Char),  // Max length
			MakeCase("SELECT CAST('' AS CHAR(1))", SqlDbType.Char),  // Empty string
			MakeCase("SELECT CAST(NULL AS CHAR(5))", SqlDbType.Char),

			// DATETIME
			// Range: 1753-01-01 through 9999-12-31
			MakeCase("SELECT CAST('1753-01-01 00:00:00' AS DATETIME)", SqlDbType.DateTime), // Min value
			MakeCase("SELECT CAST('9999-12-31 23:59:59.997' AS DATETIME)", SqlDbType.DateTime), // Max value
			MakeCase("SELECT CAST('2023-06-15 14:30:45.123' AS DATETIME)", SqlDbType.DateTime), // Millisecond precision
			MakeCase("SELECT CAST('1900-02-28 23:59:59' AS DATETIME)", SqlDbType.DateTime), // Leap year edge
			MakeCase("SELECT CAST('2000-02-29 00:00:00' AS DATETIME)", SqlDbType.DateTime), // Leap year
			MakeCase("SELECT CAST(NULL AS DATETIME)", SqlDbType.DateTime),

			// DECIMAL
			MakeCase("SELECT CAST(123.45 AS DECIMAL(5,2))", SqlDbType.Decimal),
			MakeCase("SELECT CAST(1234567890 AS DECIMAL(10,0))", SqlDbType.Decimal),
			MakeCase("SELECT CAST(0.0000000001 AS DECIMAL(11,10))", SqlDbType.Decimal),
			MakeCase("SELECT CAST(-9999999999999999999.9999999999 AS DECIMAL(30,10))", SqlDbType.Decimal),
			MakeCase("SELECT CAST(9999999999999999999.9999999999 AS DECIMAL(38,10))", SqlDbType.Decimal),
			MakeCase("SELECT CAST(NULL AS DECIMAL(18,4))", SqlDbType.Decimal),

			// FLOAT
			// Typically 53 bits of precision in SQL Server
			MakeCase("SELECT CAST(-12345.6789 AS FLOAT)", SqlDbType.Float),   // Negative
			MakeCase("SELECT CAST(0.0 AS FLOAT)", SqlDbType.Float),           // Zero
			MakeCase("SELECT CAST(1.23456789e+25 AS FLOAT)", SqlDbType.Float), // Arbitrary large
			MakeCase("SELECT CAST(3.14159 AS FLOAT)", SqlDbType.Float),        // Arbitrary
			MakeCase("SELECT CAST(NULL AS FLOAT)", SqlDbType.Float),

			// IMAGE (deprecated)
			MakeCase("SELECT CAST(0x1234 AS IMAGE)", SqlDbType.Image),
			MakeCase("SELECT CAST(REPLICATE(CAST(0xAB AS BINARY(1)), 100) AS IMAGE)", SqlDbType.Image), // Arbitrary bigger
			MakeCase("SELECT CAST(0x AS IMAGE)", SqlDbType.Image), // Empty
			MakeCase("SELECT CAST(0xFFEEAABBCCDDEE AS IMAGE)", SqlDbType.Image),
			MakeCase("SELECT CAST(0x9EE965 AS IMAGE)", SqlDbType.Image), // "null" when converted to base64
			MakeCase("SELECT CAST(NULL AS IMAGE)", SqlDbType.Image),

			// INT
			MakeCase("SELECT CAST(-2147483648 AS INT)", SqlDbType.Int),  // Min
			MakeCase("SELECT CAST(0 AS INT)", SqlDbType.Int),
			MakeCase("SELECT CAST(123456 AS INT)", SqlDbType.Int),       // Arbitrary
			MakeCase("SELECT CAST(2147483647 AS INT)", SqlDbType.Int),   // Max
			MakeCase("SELECT CAST(NULL AS INT)", SqlDbType.Int),

			// MONEY
			// Range: -922337203685477.5808 to 922337203685477.5807
			MakeCase("SELECT CAST(-123.45 AS MONEY)", SqlDbType.Money),
			MakeCase("SELECT CAST(0 AS MONEY)", SqlDbType.Money),
			MakeCase("SELECT CAST(999999.99 AS MONEY)", SqlDbType.Money), // Arbitrary
			MakeCase("SELECT CAST(922337203685477.5807 AS MONEY)", SqlDbType.Money), // Max
			MakeCase("SELECT CAST(NULL AS MONEY)", SqlDbType.Money),

			// NCHAR
			MakeCase("SELECT CAST(N'A' AS NCHAR(1))", SqlDbType.NChar),
			MakeCase("SELECT CAST(N'XYZ' AS NCHAR(3))", SqlDbType.NChar),
			MakeCase("SELECT CAST(N' ' AS NCHAR(1))", SqlDbType.NChar),
			MakeCase("SELECT CAST(N'\u03A9' AS NCHAR(1))", SqlDbType.NChar), // Greek Omega
			MakeCase("SELECT CAST(N'null' AS NCHAR(4))", SqlDbType.NChar),
			MakeCase("SELECT CAST(NULL AS NCHAR(5))", SqlDbType.NChar),

			// NTEXT (deprecated)
			MakeCase("SELECT CAST(N'Some longer unicode text' AS NTEXT)", SqlDbType.NText),
			MakeCase("SELECT CAST(N'' AS NTEXT)", SqlDbType.NText), // Empty
			MakeCase("SELECT CAST(N'\u4F60\u597D Hello' AS NTEXT)", SqlDbType.NText), // Mixed script
			MakeCase("SELECT CAST(N'Really long text ' + REPLICATE(N'X', 100) AS NTEXT)", SqlDbType.NText),
			MakeCase("SELECT CAST(N'null' AS NTEXT)", SqlDbType.NText),
			MakeCase("SELECT CAST(NULL AS NTEXT)", SqlDbType.NText),

			// NVARCHAR
			MakeCase("SELECT CAST(N'' AS NVARCHAR(53))", SqlDbType.NVarChar),
			MakeCase("SELECT CAST(N'-Hello World-' AS NVARCHAR(26))", SqlDbType.NVarChar),
			MakeCase("SELECT CAST(N'12345' AS NVARCHAR(50))", SqlDbType.NVarChar),
			MakeCase("SELECT CAST(N'\uD83D\uDE00 (emoji)' AS NVARCHAR(100))", SqlDbType.NVarChar), // Surrogate pair
			MakeCase("SELECT CAST(N'null' AS NVARCHAR(4))", SqlDbType.NVarChar),
			MakeCase("SELECT CAST(NULL AS NVARCHAR(1))", SqlDbType.NVarChar),
			MakeCase("SELECT CAST(REPLICATE(N'X', 2000) AS NVARCHAR(MAX))", SqlDbType.NVarChar),

			// REAL (float(24))
			MakeCase("SELECT CAST(-123.45 AS REAL)", SqlDbType.Real),
			MakeCase("SELECT CAST(0.0 AS REAL)", SqlDbType.Real),
			MakeCase("SELECT CAST(3.40282e+38 AS REAL)", SqlDbType.Real), // near max
			MakeCase("SELECT CAST(1.2345e-10 AS REAL)", SqlDbType.Real),  // smaller number
			MakeCase("SELECT CAST(NULL AS REAL)", SqlDbType.Real),

			// UNIQUEIDENTIFIER
			MakeCase("SELECT CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER)", SqlDbType.UniqueIdentifier),
			MakeCase("SELECT CAST('01234567-89AB-CDEF-0123-456789ABCDEF' AS UNIQUEIDENTIFIER)", SqlDbType.UniqueIdentifier),
			MakeCase("SELECT CAST('11111111-1111-1111-1111-111111111111' AS UNIQUEIDENTIFIER)", SqlDbType.UniqueIdentifier),
			MakeCase("SELECT CAST('99999999-9999-9999-9999-999999999999' AS UNIQUEIDENTIFIER)", SqlDbType.UniqueIdentifier),
			MakeCase("SELECT CAST(NULL AS UNIQUEIDENTIFIER)", SqlDbType.UniqueIdentifier),
			MakeCase("SELECT NEWID()", SqlDbType.UniqueIdentifier),

			// SMALLDATETIME
			// Range: 1900-01-01 through 2079-06-06
			MakeCase("SELECT CAST('1900-01-01 00:00' AS SMALLDATETIME)", SqlDbType.SmallDateTime),
			MakeCase("SELECT CAST('1975-12-31 23:59' AS SMALLDATETIME)", SqlDbType.SmallDateTime), // Arbitrary
			MakeCase("SELECT CAST('2079-06-06 23:59' AS SMALLDATETIME)", SqlDbType.SmallDateTime), // max
			MakeCase("SELECT CAST('2000-01-01 12:00' AS SMALLDATETIME)", SqlDbType.SmallDateTime), // Another arbitrary
			MakeCase("SELECT CAST(NULL AS SMALLDATETIME)", SqlDbType.SmallDateTime),

			// SMALLINT
			MakeCase("SELECT CAST(-32768 AS SMALLINT)", SqlDbType.SmallInt), // Min
			MakeCase("SELECT CAST(0 AS SMALLINT)", SqlDbType.SmallInt),
			MakeCase("SELECT CAST(12345 AS SMALLINT)", SqlDbType.SmallInt),  // Arbitrary
			MakeCase("SELECT CAST(32767 AS SMALLINT)", SqlDbType.SmallInt),  // Max
			MakeCase("SELECT CAST(NULL AS SMALLINT)", SqlDbType.SmallInt),

			// SMALLMONEY
			// Range: -214748.3648 to 214748.3647
			MakeCase("SELECT CAST(-214748.3648 AS SMALLMONEY)", SqlDbType.SmallMoney), // Min
			MakeCase("SELECT CAST(0.00 AS SMALLMONEY)", SqlDbType.SmallMoney),
			MakeCase("SELECT CAST(12345.6789 AS SMALLMONEY)", SqlDbType.SmallMoney),   // Arbitrary
			MakeCase("SELECT CAST(214748.3647 AS SMALLMONEY)", SqlDbType.SmallMoney),  // Max
			MakeCase("SELECT CAST(NULL AS SMALLMONEY)", SqlDbType.SmallMoney),

			// TEXT (deprecated)
			MakeCase("SELECT CAST('Short text' AS TEXT)", SqlDbType.Text),
			MakeCase("SELECT CAST(REPLICATE('X', 200) AS TEXT)", SqlDbType.Text), // Arbitrary larger
			MakeCase("SELECT CAST('Mixed content 123 !@#' AS TEXT)", SqlDbType.Text),
			MakeCase("SELECT CAST('' AS TEXT)", SqlDbType.Text),                 // Empty
			MakeCase("SELECT CAST(N'null' AS TEXT)", SqlDbType.Text),
			MakeCase("SELECT CAST(NULL AS TEXT)", SqlDbType.Text),

			// TIMESTAMP (rowversion)
			// We'll just cast known hex patterns for testing
			MakeCase("SELECT CAST(0x11223344 AS TIMESTAMP)", SqlDbType.Timestamp),
			MakeCase("SELECT CAST(0x0000000000000000 AS TIMESTAMP)", SqlDbType.Timestamp),
			MakeCase("SELECT CAST(0xFFFFFFFFFFFFFFFF AS TIMESTAMP)", SqlDbType.Timestamp),
			MakeCase("SELECT CAST(0xDEADBEEF AS TIMESTAMP)", SqlDbType.Timestamp),
			MakeCase("SELECT CAST(NULL AS TIMESTAMP)", SqlDbType.Timestamp),

			// TINYINT
			MakeCase("SELECT CAST(0 AS TINYINT)", SqlDbType.TinyInt),
			MakeCase("SELECT CAST(1 AS TINYINT)", SqlDbType.TinyInt),
			MakeCase("SELECT CAST(100 AS TINYINT)", SqlDbType.TinyInt),   // Arbitrary
			MakeCase("SELECT CAST(255 AS TINYINT)", SqlDbType.TinyInt),   // Max
			MakeCase("SELECT CAST(NULL AS TINYINT)", SqlDbType.TinyInt),

			// VARBINARY
			MakeCase("SELECT CAST(0x123456 AS VARBINARY(3))", SqlDbType.VarBinary),
			MakeCase("SELECT CAST(0x AS VARBINARY(10))", SqlDbType.VarBinary), // Empty
			MakeCase("SELECT CAST(REPLICATE(CAST(0xFF AS BINARY(1)), 16) AS VARBINARY(16))", SqlDbType.VarBinary),
			MakeCase("SELECT CAST(0xDEADBEEF AS VARBINARY(4))", SqlDbType.VarBinary), // Arbitrary chunk
			MakeCase("SELECT CAST(0x9EE965 AS VARBINARY(4))", SqlDbType.VarBinary), // "null" when converted to base64
			MakeCase("SELECT CAST(NULL AS VARBINARY(50))", SqlDbType.VarBinary),
			MakeCase("SELECT CAST(REPLICATE(CAST(0xAB AS BINARY(1)), 1000) AS VARBINARY(MAX))", SqlDbType.VarBinary),
			MakeCase("SELECT CAST(0x3000 AS VARBINARY)", SqlDbType.VarBinary), // 0

			// VARCHAR
			MakeCase("SELECT CAST('' AS VARCHAR(10))", SqlDbType.VarChar), // Empty
			MakeCase("SELECT CAST('Hello World' AS VARCHAR(50))", SqlDbType.VarChar),
			MakeCase("SELECT CAST('123!@#' AS VARCHAR(50))", SqlDbType.VarChar),      // Arbitrary
			MakeCase("SELECT CAST(REPLICATE('X', 100) AS VARCHAR(100))", SqlDbType.VarChar),
			MakeCase("SELECT CAST('NULL' AS VARCHAR(50))", SqlDbType.VarChar),
			MakeCase("SELECT CAST('null' AS VARCHAR(50))", SqlDbType.VarChar),
			MakeCase("SELECT CAST(NULL AS VARCHAR(50))", SqlDbType.VarChar),
			MakeCase("SELECT CAST(REPLICATE('Z', 2000) AS VARCHAR(MAX))", SqlDbType.VarChar),

			// XML
			MakeCase("SELECT CAST('<root><child>value</child></root>' AS XML)", SqlDbType.Xml, AssertXmlEqual),
			MakeCase("SELECT CAST('<empty/>' AS XML)", SqlDbType.Xml, AssertXmlEqual),
			MakeCase("SELECT CAST('<root>Some text &amp; more</root>' AS XML)", SqlDbType.Xml, AssertXmlEqual), // Entities
			MakeCase("SELECT CAST(REPLICATE(N'<tag>data</tag>', 5) AS XML)", SqlDbType.Xml, AssertXmlEqual),    // Larger
			MakeCase("SELECT CAST(NULL AS XML)", SqlDbType.Xml, AssertXmlEqual),

			// DATE
			// Range: 1753-01-01 through 9999-12-31
			MakeCase("SELECT CAST('1753-01-01' AS DATE)", SqlDbType.Date),
			MakeCase("SELECT CAST('1970-06-15' AS DATE)", SqlDbType.Date),
			MakeCase("SELECT CAST('9999-12-31' AS DATE)", SqlDbType.Date),
			MakeCase("SELECT CAST('2025-03-17' AS DATE)", SqlDbType.Date),
			MakeCase("SELECT CAST('1900-02-28' AS DATE)", SqlDbType.Date), // Non-leap year
			MakeCase("SELECT CAST('2000-02-29' AS DATE)", SqlDbType.Date), // Leap year
			MakeCase("SELECT CAST(NULL AS DATE)", SqlDbType.Date),

			// TIME
			MakeCase("SELECT CAST('00:00:00' AS TIME(0))", SqlDbType.Time),
			MakeCase("SELECT CAST('12:34:56.1' AS TIME(1))", SqlDbType.Time),
			MakeCase("SELECT CAST('23:59:59.99' AS TIME(2))", SqlDbType.Time),
			MakeCase("SELECT CAST('08:00:00.123' AS TIME(3))", SqlDbType.Time),
			MakeCase("SELECT CAST('15:30:45.1234' AS TIME(4))", SqlDbType.Time),
			MakeCase("SELECT CAST('18:45:30.12345' AS TIME(5))", SqlDbType.Time),
			MakeCase("SELECT CAST('21:15:00.123456' AS TIME(6))", SqlDbType.Time),
			MakeCase("SELECT CAST('12:34:56.1234567' AS TIME(7))", SqlDbType.Time),
			MakeCase("SELECT CAST('23:59:59.9999999' AS TIME(7))", SqlDbType.Time),
			MakeCase("SELECT CAST(NULL AS TIME(7))", SqlDbType.Time),

			// DATETIME2
			MakeCase("SELECT CAST('1753-01-01 00:00:00' AS DATETIME2(0))", SqlDbType.DateTime2),
			MakeCase("SELECT CAST('1970-06-15 13:45:59.9' AS DATETIME2(1))", SqlDbType.DateTime2),
			MakeCase("SELECT CAST('2000-12-31 23:59:59.99' AS DATETIME2(2))", SqlDbType.DateTime2),
			MakeCase("SELECT CAST('2025-03-17 12:34:56.123' AS DATETIME2(3))", SqlDbType.DateTime2),
			MakeCase("SELECT CAST('2050-07-04 08:15:30.1234' AS DATETIME2(4))", SqlDbType.DateTime2),
			MakeCase("SELECT CAST('2075-11-22 18:45:00.12345' AS DATETIME2(5))", SqlDbType.DateTime2),
			MakeCase("SELECT CAST('2099-09-09 21:21:21.123456' AS DATETIME2(6))", SqlDbType.DateTime2),
			MakeCase("SELECT CAST('9999-12-31 23:59:59.9999999' AS DATETIME2(7))", SqlDbType.DateTime2),
			MakeCase("SELECT CAST(NULL AS DATETIME2(7))", SqlDbType.DateTime2),

			// DATETIMEOFFSET
			MakeCase("SELECT CAST('1753-01-01 00:00:00 +00:00' AS DATETIMEOFFSET(0))", SqlDbType.DateTimeOffset),
			MakeCase("SELECT CAST('1970-06-15 13:45:59.1 -05:00' AS DATETIMEOFFSET(1))", SqlDbType.DateTimeOffset),
			MakeCase("SELECT CAST('2000-12-31 23:59:59.99 +01:00' AS DATETIMEOFFSET(2))", SqlDbType.DateTimeOffset),
			MakeCase("SELECT CAST('2025-03-17 12:34:56.123 -08:00' AS DATETIMEOFFSET(3))", SqlDbType.DateTimeOffset),
			MakeCase("SELECT CAST('2050-07-04 08:15:30.1234 +03:30' AS DATETIMEOFFSET(4))", SqlDbType.DateTimeOffset),
			MakeCase("SELECT CAST('2075-11-22 18:45:00.12345 -11:00' AS DATETIMEOFFSET(5))", SqlDbType.DateTimeOffset),
			MakeCase("SELECT CAST('2099-09-09 21:21:21.123456 +14:00' AS DATETIMEOFFSET(6))", SqlDbType.DateTimeOffset),
			MakeCase("SELECT CAST('9999-12-31 23:59:59.9999999 +14:00' AS DATETIMEOFFSET(7))", SqlDbType.DateTimeOffset),
			MakeCase("SELECT CAST(NULL AS DATETIMEOFFSET(7))", SqlDbType.DateTimeOffset),
		];

		TestCaseData MakeCase(string query, SqlDbType expectedDataType, Action<object, object>? assertEqual = null)
		{
			return new TestCaseData(query, expectedDataType, assertEqual ?? AssertEqual) { TestName = $"{{m}} - {expectedDataType} - {query.Substring(7)}" };
		}
	}

	static void AssertEqual(object expected, object actual)
	{
		Assert.That(actual, Is.EqualTo(expected));
	}

	static void AssertXmlEqual(object expected, object actual)
	{
		if (expected is SqlXml expectedXml && actual is SqlXml actualXml)
		{
			Assert.That(expectedXml.IsNull, Is.EqualTo(actualXml.IsNull));

			if (!expectedXml.IsNull && !actualXml.IsNull)
			{
				Assert.That(actualXml.Value, Is.EqualTo(expectedXml.Value));
			}
		}
		else
		{
			throw new ArgumentException("Expected and actual values must be SqlXml");
		}
	}
}
#pragma warning restore CW1122 // Do Not Use DateTime Parse Method
