using System.Data;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data.SqlProxy.Interface;

public static class TypeMappingHelper
{
	public static SqlDbType? GetSqlDbType(Type? type)
	{
		if (type == null)
		{
			return null;
		}

		return CsTypeToSqlDbTypeMappings.TryGetValue(type, out var dbType) ? dbType : null;
	}

	public static Type SqlDbTypeToCsType(string type)
	{
		return SqlDbTypeToCsType(StringToSqlDbType(type));
	}

	public static Type SqlDbTypeToCsType(SqlDbType type)
	{
		return SqlDbTypeToCsTypeMappings.TryGetValue(type, out var csType)
			? csType
			: typeof(object);
	}

	public static string SqlDbTypeAsString(SqlDbType type)
	{
		return SqlDbTypeToStringMappings.TryGetValue(type, out var sqlDbType)
			? sqlDbType
			: type.ToString().ToLower();
	}

	public static SqlDbType StringToSqlDbType(string type)
	{
		return StringToSqlDbTypeMappings.TryGetValue(type, out var sqlDbType)
			? sqlDbType
			: SqlDbType.Udt;
	}

	public static SqlDbType DbTypeToSqlDbType(DbType type)
	{
		return DbTypeToSqlDbTypeMappings.TryGetValue(type, out var sqlDbType)
			? sqlDbType
			: SqlDbType.Udt;
	}

	public static DbType SqlDbTypeToDbType(SqlDbType type)
	{
		return SqlDbTypeToDbTypeMappings.TryGetValue(type, out var dbType)
			? dbType
			: DbType.Object;
	}

	public static Type GetTypeFromSqlType(string dbType)
	{
		if (SqlTypeStringToCsTypeMappings.TryGetValue(dbType, out var type))
		{
			return type;
		}

		if (SqlTypeStringToCsTypeMappings.TryGetValue(dbType, out type))
		{
			SqlTypeStringToCsTypeMappings[dbType] = type;
			return type;
		}

		var parts = dbType.Split('.');
		if (SqlTypeStringToCsTypeMappings.TryGetValue(parts.Last(), out type))
		{
			SqlTypeStringToCsTypeMappings[dbType] = type;
			return type;
		}

		throw new NotSupportedException($"Non supported sql DB Type : {dbType}");
	}

	[ThreadSafe]
	public static readonly Dictionary<Type, SqlDbType> CsTypeToSqlDbTypeMappings = new()
	{
		{ typeof(SqlBinary), SqlDbType.Binary },
		{ typeof(SqlBoolean), SqlDbType.Bit },
		{ typeof(SqlByte), SqlDbType.TinyInt },
		{ typeof(SqlBytes), SqlDbType.VarBinary },
		{ typeof(SqlChars), SqlDbType.NVarChar },
		{ typeof(SqlDateTime), SqlDbType.DateTime },
		{ typeof(SqlDecimal), SqlDbType.Decimal },
		{ typeof(SqlDouble), SqlDbType.Float },
		{ typeof(SqlGuid), SqlDbType.UniqueIdentifier },
		{ typeof(SqlInt16), SqlDbType.SmallInt },
		{ typeof(SqlInt32), SqlDbType.Int },
		{ typeof(SqlInt64), SqlDbType.BigInt },
		{ typeof(SqlMoney), SqlDbType.Money },
		{ typeof(SqlSingle), SqlDbType.Real },
		{ typeof(SqlString), SqlDbType.NVarChar },
		{ typeof(SqlXml), SqlDbType.Xml },

		// Native .NET types for modern SQL types
		{ typeof(bool), SqlDbType.Bit },
		{ typeof(byte), SqlDbType.TinyInt },
		{ typeof(short), SqlDbType.SmallInt },
		{ typeof(int), SqlDbType.Int },
		{ typeof(long), SqlDbType.BigInt },
		{ typeof(decimal), SqlDbType.Decimal },
		{ typeof(float), SqlDbType.Real },
		{ typeof(double), SqlDbType.Float },
		{ typeof(Guid), SqlDbType.UniqueIdentifier },
		{ typeof(string), SqlDbType.NVarChar },
		{ typeof(char), SqlDbType.NChar },
		{ typeof(byte[]), SqlDbType.VarBinary },
		{ typeof(char[]), SqlDbType.NVarChar },

		// Date/Time types
		{ typeof(DateTime), SqlDbType.DateTime },
		{ typeof(DateTimeOffset), SqlDbType.DateTimeOffset },
		{ typeof(TimeSpan), SqlDbType.Time },

		// Nullable versions
		{ typeof(bool?), SqlDbType.Bit },
		{ typeof(byte?), SqlDbType.TinyInt },
		{ typeof(short?), SqlDbType.SmallInt },
		{ typeof(int?), SqlDbType.Int },
		{ typeof(long?), SqlDbType.BigInt },
		{ typeof(decimal?), SqlDbType.Decimal },
		{ typeof(float?), SqlDbType.Real },
		{ typeof(double?), SqlDbType.Float },
		{ typeof(Guid?), SqlDbType.UniqueIdentifier },
		{ typeof(DateTime?), SqlDbType.DateTime },
		{ typeof(DateTimeOffset?), SqlDbType.DateTimeOffset },
		{ typeof(TimeSpan?), SqlDbType.Time },
	};

	[ThreadSafe]
	public static readonly Dictionary<SqlDataType, Type> SqlDataTypeToCsTypeMappings = new()
	{
		{ SqlDataType.varbinary, typeof(byte[]) },
		{ SqlDataType.binary, typeof(byte[]) },
		{ SqlDataType.filestream, typeof(byte[]) },
		{ SqlDataType.image, typeof(byte[]) },
		{ SqlDataType.rowversion, typeof(byte[]) },
		{ SqlDataType.timestamp, typeof(byte[]) },
		{ SqlDataType.tinyint, typeof(byte) },
		{ SqlDataType.varchar, typeof(string) },
		{ SqlDataType.nvarchar, typeof(string) },
		{ SqlDataType.nchar, typeof(string) },
		{ SqlDataType.text, typeof(string) },
		{ SqlDataType.ntext, typeof(string) },
		{ SqlDataType.xml, typeof(string) },
		{ SqlDataType.@char, typeof(string) },
		{ SqlDataType.bigint, typeof(long) },
		{ SqlDataType.bit, typeof(bool) },
		{ SqlDataType.smalldatetime, typeof(DateTime) },
		{ SqlDataType.datetime, typeof(DateTime) },
		{ SqlDataType.date, typeof(DateTime) },
		{ SqlDataType.datetime2, typeof(DateTime) },
		{ SqlDataType.datetimeoffset, typeof(DateTimeOffset) },
		{ SqlDataType.@decimal, typeof(decimal) },
		{ SqlDataType.money, typeof(decimal) },
		{ SqlDataType.numeric, typeof(decimal) },
		{ SqlDataType.smallmoney, typeof(decimal) },
		{ SqlDataType.@float, typeof(double) },
		{ SqlDataType.@int, typeof(int) },
		{ SqlDataType.real, typeof(float) },
		{ SqlDataType.smallint, typeof(short) },
		{ SqlDataType.uniqueidentifier, typeof(Guid) },
		{ SqlDataType.sql_variant, typeof(object) },
		{ SqlDataType.time, typeof(TimeSpan) },
	};

	[ThreadSafe]
	public static readonly Dictionary<SqlDbType, Type> SqlDbTypeToCsTypeMappings = new()
	{
		{ SqlDbType.BigInt, typeof(long) },
		{ SqlDbType.Binary, typeof(byte[]) },
		{ SqlDbType.Bit, typeof(bool) },
		{ SqlDbType.Char, typeof(string) },
		{ SqlDbType.Date, typeof(DateTime) },
		{ SqlDbType.DateTime, typeof(DateTime) },
		{ SqlDbType.DateTime2, typeof(DateTime) },
		{ SqlDbType.DateTimeOffset, typeof(DateTimeOffset) },
		{ SqlDbType.Decimal, typeof(decimal) },
		{ SqlDbType.Float, typeof(double) },
		{ SqlDbType.Image, typeof(byte[]) },
		{ SqlDbType.Int, typeof(int) },
		{ SqlDbType.Money, typeof(decimal) },
		{ SqlDbType.NChar, typeof(string) },
		{ SqlDbType.NText, typeof(string) },
		{ SqlDbType.NVarChar, typeof(string) },
		{ SqlDbType.Real, typeof(float) },
		{ SqlDbType.SmallDateTime, typeof(DateTime) },
		{ SqlDbType.SmallInt, typeof(short) },
		{ SqlDbType.SmallMoney, typeof(decimal) },
		{ SqlDbType.Structured, typeof(object) },
		{ SqlDbType.Text, typeof(string) },
		{ SqlDbType.Time, typeof(TimeSpan) },
		{ SqlDbType.Timestamp, typeof(byte[]) },
		{ SqlDbType.TinyInt, typeof(byte) },
		{ SqlDbType.Udt, typeof(object) },
		{ SqlDbType.UniqueIdentifier, typeof(Guid) },
		{ SqlDbType.VarBinary, typeof(byte[]) },
		{ SqlDbType.VarChar, typeof(string) },
		{ SqlDbType.Variant, typeof(object) },
		{ SqlDbType.Xml, typeof(string) },
	};

	[ThreadSafe]
	public static readonly Dictionary<string, Type> SqlTypeStringToCsTypeMappings = new(StringComparer.OrdinalIgnoreCase)
	{
		{ "UNIQUEIDENTIFIER", typeof(Guid) },
		{ "DECIMAL", typeof(decimal) },
		{ "DATE", typeof(DateTime) },
		{ "DATETIME2", typeof(DateTime) },
		{ "DATETIMEOFFSET", typeof(DateTimeOffset) },
		{ "DATETIME", typeof(DateTime) },
		{ "SMALLDATETIME", typeof(DateTime) },
		{ "TIME", typeof(TimeSpan) },
		{ "INT", typeof(int) },
		{ "BIGINT", typeof(long) },
		{ "MONEY", typeof(decimal) },
		{ "SMALLMONEY", typeof(decimal) },
		{ "NUMERIC", typeof(decimal) },
		{ "REAL", typeof(float) },
		{ "FLOAT", typeof(double) },
		{ "SMALLINT", typeof(short) },
		{ "TINYINT", typeof(byte) },
		{ "CHAR", typeof(string) },
		{ "NCHAR", typeof(string) },
		{ "VARCHAR", typeof(string) },
		{ "NVARCHAR", typeof(string) },
		{ "TEXT", typeof(string) },
		{ "NTEXT", typeof(string) },
		{ "XML", typeof(string) },
		{ "BIT", typeof(bool) },
		{ "BINARY", typeof(byte[]) },
		{ "VARBINARY", typeof(byte[]) },
		{ "IMAGE", typeof(byte[]) },
		{ "SQL_VARIANT", typeof(object) },
		{ "GEOGRAPHY", typeof(SqlGeography) },
		{ "GEOMETRY", typeof(SqlGeometry) }
	};

	[ThreadSafe]
	public static readonly Dictionary<SqlDbType, string> SqlDbTypeToStringMappings = new()
	{
		{ SqlDbType.BigInt, "bigint" },
		{ SqlDbType.Binary, "binary" },
		{ SqlDbType.Bit, "bit" },
		{ SqlDbType.Char, "char" },
		{ SqlDbType.Date, "date" },
		{ SqlDbType.DateTime, "datetime" },
		{ SqlDbType.DateTime2, "datetime2" },
		{ SqlDbType.DateTimeOffset, "datetimeoffset" },
		{ SqlDbType.Decimal, "decimal" },
		{ SqlDbType.Float, "float" },
		{ SqlDbType.Image, "image" },
		{ SqlDbType.Int, "int" },
		{ SqlDbType.Money, "money" },
		{ SqlDbType.NChar, "nchar" },
		{ SqlDbType.NText, "ntext" },
		{ SqlDbType.NVarChar, "nvarchar" },
		{ SqlDbType.Real, "real" },
		{ SqlDbType.SmallDateTime, "smalldatetime" },
		{ SqlDbType.SmallInt, "smallint" },
		{ SqlDbType.SmallMoney, "smallmoney" },
		{ SqlDbType.Structured, "structured" },
		{ SqlDbType.Text, "text" },
		{ SqlDbType.Time, "time" },
		{ SqlDbType.Timestamp, "timestamp" },
		{ SqlDbType.TinyInt, "tinyint" },
		{ SqlDbType.Udt, "udt" },
		{ SqlDbType.UniqueIdentifier, "uniqueidentifier" },
		{ SqlDbType.VarBinary, "varbinary" },
		{ SqlDbType.VarChar, "varchar" },
		{ SqlDbType.Variant,"variant" },
		{ SqlDbType.Xml,"xml" },
	};

	[ThreadSafe]
	public static readonly Dictionary<string, SqlDbType> StringToSqlDbTypeMappings = new(StringComparer.OrdinalIgnoreCase)
	{
		{ "bigint", SqlDbType.BigInt },
		{ "binary", SqlDbType.Binary },
		{ "bit", SqlDbType.Bit },
		{ "bool", SqlDbType.Bit },
		{ "boolean", SqlDbType.Bit },
		{ "byte", SqlDbType.TinyInt },
		{ "byte[]", SqlDbType.VarBinary },
		{ "char", SqlDbType.Char },
		{ "date", SqlDbType.Date },
		{ "datetime", SqlDbType.DateTime },
		{ "datetime2", SqlDbType.DateTime2 },
		{ "datetimeoffset", SqlDbType.DateTimeOffset },
		{ "dbnull", SqlDbType.Variant },
		{ "decimal", SqlDbType.Decimal },
		{ "double", SqlDbType.Float },
		{ "float", SqlDbType.Float },
		{ "guid", SqlDbType.UniqueIdentifier },
		{ "image", SqlDbType.Image },
		{ "int", SqlDbType.Int },
		{ "int16", SqlDbType.SmallInt },
		{ "int32", SqlDbType.Int },
		{ "int64", SqlDbType.BigInt },
		{ "long", SqlDbType.BigInt },
		{ "money", SqlDbType.Money },
		{ "nchar", SqlDbType.NChar },
		{ "ntext", SqlDbType.NText },
		{ "nvarchar", SqlDbType.NVarChar },
		{ "object", SqlDbType.Variant },
		{ "real", SqlDbType.Real },
		{ "sbyte", SqlDbType.TinyInt },
		{ "short", SqlDbType.SmallInt },
		{ "smalldatetime", SqlDbType.SmallDateTime },
		{ "smallint", SqlDbType.SmallInt },
		{ "smallmoney", SqlDbType.SmallMoney },
		{ "sql_variant", SqlDbType.Variant },
		{ "string", SqlDbType.NVarChar },
		{ "structured", SqlDbType.Structured },
		{ "text", SqlDbType.Text },
		{ "time", SqlDbType.Time },
		{ "timespan", SqlDbType.Time },
		{ "timestamp", SqlDbType.Timestamp },
		{ "tinyint", SqlDbType.TinyInt },
		{ "udt", SqlDbType.Udt },
		{ "uint", SqlDbType.Int },
		{ "ulong", SqlDbType.BigInt },
		{ "uniqueidentifier", SqlDbType.UniqueIdentifier },
		{ "ushort", SqlDbType.SmallInt },
		{ "varbinary", SqlDbType.VarBinary },
		{ "varchar", SqlDbType.VarChar },
		{ "xml", SqlDbType.Xml },
	};

	[ThreadSafe]
	public static readonly Dictionary<DbType, SqlDbType> DbTypeToSqlDbTypeMappings = new()
	{
		{ DbType.AnsiString, SqlDbType.VarChar },
		{ DbType.AnsiStringFixedLength, SqlDbType.Char },
		{ DbType.Binary, SqlDbType.VarBinary },
		{ DbType.Boolean, SqlDbType.Bit },
		{ DbType.Byte, SqlDbType.TinyInt },
		{ DbType.Currency, SqlDbType.Money },
		{ DbType.Date, SqlDbType.Date },
		{ DbType.DateTime, SqlDbType.DateTime },
		{ DbType.Decimal, SqlDbType.Decimal },
		{ DbType.Double, SqlDbType.Float },
		{ DbType.Guid, SqlDbType.UniqueIdentifier },
		{ DbType.Int16, SqlDbType.SmallInt },
		{ DbType.Int32, SqlDbType.Int },
		{ DbType.Int64, SqlDbType.BigInt },
		{ DbType.Object, SqlDbType.Variant },
		{ DbType.SByte, SqlDbType.TinyInt },
		{ DbType.Single, SqlDbType.Real },
		{ DbType.String, SqlDbType.NVarChar },
		{ DbType.StringFixedLength, SqlDbType.NChar },
		{ DbType.Time, SqlDbType.Time },
	};

	[ThreadSafe]
	public static readonly Dictionary<SqlDbType, DbType> SqlDbTypeToDbTypeMappings = new()
	{
		{ SqlDbType.BigInt, DbType.Int64 },
		{ SqlDbType.Binary, DbType.Binary },
		{ SqlDbType.Bit, DbType.Boolean },
		{ SqlDbType.Char, DbType.AnsiStringFixedLength },
		{ SqlDbType.Date, DbType.Date },
		{ SqlDbType.DateTime, DbType.DateTime },
		{ SqlDbType.DateTime2, DbType.DateTime },
		{ SqlDbType.DateTimeOffset, DbType.DateTimeOffset },
		{ SqlDbType.Decimal, DbType.Decimal },
		{ SqlDbType.Float, DbType.Double },
		{ SqlDbType.Image, DbType.Binary },
		{ SqlDbType.Int, DbType.Int32 },
		{ SqlDbType.Money, DbType.Currency },
		{ SqlDbType.NChar, DbType.StringFixedLength },
		{ SqlDbType.NText, DbType.String },
		{ SqlDbType.NVarChar, DbType.String },
		{ SqlDbType.Real, DbType.Single },
		{ SqlDbType.SmallDateTime, DbType.DateTime },
		{ SqlDbType.SmallInt, DbType.Int16 },
		{ SqlDbType.SmallMoney, DbType.Currency },
		{ SqlDbType.Structured, DbType.Object },
		{ SqlDbType.Text, DbType.String },
		{ SqlDbType.Time, DbType.Time },
		{ SqlDbType.Timestamp, DbType.Binary },
		{ SqlDbType.TinyInt, DbType.Byte },
	};
}
