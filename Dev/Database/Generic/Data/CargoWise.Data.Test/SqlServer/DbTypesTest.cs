using System;
using System.Data;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbTypesTest : TestCase
	{
		public void TestCount()
		{
			AssertEquals(35, DbTypes.Instance.Count);
		}

		public void TestIndexer()
		{
			AssertEquals(SqlDbType.NVarChar, DbTypes.Instance["string"]);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "Not supported DataType: Costa Rica Tarazzu.")]
		public void TestIndexerWithItemNotInListThrowsNotSupportedException()
		{
			object o = DbTypes.Instance["Costa Rica Tarazzu"];
		}

		public void TestEnumerating()
		{
			int itemsInDbTypes = 0;

			foreach (SqlDbType value in DbTypes.Instance)
			{
				object variableToKeepTheCompilerQuiet = value;
				itemsInDbTypes++;
			}

			AssertEquals("Number of elements enumerated should match DbTypes.Count.", DbTypes.Instance.Count, itemsInDbTypes);
		}

		public void TestListContents()
		{
			AssertEquals(SqlDbType.BigInt, DbTypes.Instance["bigint"]);
			AssertEquals(SqlDbType.Binary, DbTypes.Instance["binary"]);
			AssertEquals(SqlDbType.Bit, DbTypes.Instance["bit"]);
			AssertEquals(SqlDbType.Bit, DbTypes.Instance["boolean"]);
			AssertEquals(SqlDbType.DateTime, DbTypes.Instance["datetime"]);
			AssertEquals(SqlDbType.DateTimeOffset, DbTypes.Instance["datetimeoffset"]);
			AssertEquals(SqlDbType.DateTime2, DbTypes.Instance["datetime2"]);
			AssertEquals(SqlDbType.Date, DbTypes.Instance["date"]);
			AssertEquals(SqlDbType.Decimal, DbTypes.Instance["decimal"]);
			AssertEquals(SqlDbType.Decimal, DbTypes.Instance["numeric"]);
			AssertEquals(SqlDbType.Float, DbTypes.Instance["float"]);
			AssertEquals(SqlDbType.Int, DbTypes.Instance["int"]);
			AssertEquals(SqlDbType.BigInt, DbTypes.Instance["int64"]); // used by generated .XML files only
			AssertEquals(SqlDbType.Int, DbTypes.Instance["int32"]); // used by generated .XML files only
			AssertEquals(SqlDbType.SmallInt, DbTypes.Instance["int16"]); // used by generated .XML files only
			AssertEquals(SqlDbType.VarBinary, DbTypes.Instance["byte[]"]); // used by generated .XML files only
			AssertEquals(SqlDbType.Money, DbTypes.Instance["money"]);
			AssertEquals(SqlDbType.NVarChar, DbTypes.Instance["string"]);
			AssertEquals(SqlDbType.NVarChar, DbTypes.Instance["nchar"]);
			AssertEquals(SqlDbType.NVarChar, DbTypes.Instance["nvarchar"]);
			AssertEquals(SqlDbType.Real, DbTypes.Instance["real"]);
			AssertEquals(SqlDbType.SmallDateTime, DbTypes.Instance["smalldatetime"]);
			AssertEquals(SqlDbType.SmallInt, DbTypes.Instance["smallint"]);
			AssertEquals(SqlDbType.SmallMoney, DbTypes.Instance["smallmoney"]);
			AssertEquals(SqlDbType.Timestamp, DbTypes.Instance["timestamp"]);
			AssertEquals(SqlDbType.TinyInt, DbTypes.Instance["tinyint"]);
			AssertEquals(SqlDbType.UniqueIdentifier, DbTypes.Instance["guid"]); // used by generated .XML files only
			AssertEquals(SqlDbType.UniqueIdentifier, DbTypes.Instance["uniqueidentifier"]);
			AssertEquals(SqlDbType.VarBinary, DbTypes.Instance["varbinary"]);
			AssertEquals(SqlDbType.VarChar, DbTypes.Instance["char"]);
			AssertEquals(SqlDbType.VarChar, DbTypes.Instance["varchar"]);
			AssertEquals(SqlDbType.Variant, DbTypes.Instance["sql_variant"]);
			AssertEquals(SqlDbType.Xml, DbTypes.Instance["xml"]);
			AssertEquals(SqlDbType.NVarChar, DbTypes.Instance["geography"]);
			AssertEquals(SqlDbType.Time, DbTypes.Instance["time"]);

			AssertEquals("If this test fails a new type has been added and needs to be tested here.", 35, DbTypes.Instance.Count);
		}
	}
}
