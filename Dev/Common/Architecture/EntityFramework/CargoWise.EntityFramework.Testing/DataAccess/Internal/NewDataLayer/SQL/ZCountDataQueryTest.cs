using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZCountDataQueryTest : TransactionedTestCase
	{
		public void TestZCountDataQuery_Union()
		{
			var filter = new ZQuery();
			filter.AddToFilter(JoinCondition.Union, DummyBizoSchema.Z0_Date, new ZDateTime(1999, 10, 10));
			filter.AddToFilter(JoinCondition.Union, DummyBizoSchema.Z0_Code, "BBB");
			AssertEquals(@"SELECT COUNT(*) FROM (SELECT 
Z0_PK, Z0_AddInfo, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber,
Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte,
Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal,
Z0_Description, Z0_FK_Code, CAST(Z0_Geography AS varbinary(max)) AS Z0_Geography, Z0_Guid, Z0_IsSystem,
Z0_IsValid, Z0_Long, Z0_Money, Z0_NAddInfo, Z0_Number,
Z0_NVarChar, case when Z0_NVarCharMax is null then null when datalength(Z0_NVarCharMax) < 1024 then Z0_NVarCharMax else char(0) + char(0) end as Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_SparseBit,
Z0_SparseByte, Z0_SparseChar, Z0_SparseDate, Z0_SparseDateTime, Z0_SparseDateTimeOffset,
Z0_SparseDecimal, Z0_SparseGuid, Z0_SparseLong, Z0_SparseMoney, Z0_SparseNumber,
Z0_SparseNVarChar, Z0_SparseShort, Z0_SparseSmallDateTime, Z0_SparseTime, case when Z0_SparseVarBinaryMax is null then null when datalength(Z0_SparseVarBinaryMax) < 1024 then Z0_SparseVarBinaryMax else 0x0000 end as Z0_SparseVarBinaryMax,
Z0_SparseVarChar, case when Z0_SparseXml is null then null when datalength(Z0_SparseXml) < 1024 then cast(Z0_SparseXml as nvarchar(max)) else '<?placeholder LazyLoading=""Yes""?>' end as Z0_SparseXml, Z0_Time, case when Z0_VarBinaryMax is null then null when datalength(Z0_VarBinaryMax) < 1024 then Z0_VarBinaryMax else 0x0000 end as Z0_VarBinaryMax, case when Z0_VarCharMax is null then null when datalength(Z0_VarCharMax) < 1024 then Z0_VarCharMax else char(0) + char(0) end as Z0_VarCharMax,
case when Z0_Xml is null then null when datalength(Z0_Xml) < 1024 then cast(Z0_Xml as nvarchar(max)) else '<?placeholder LazyLoading=""Yes""?>' end as Z0_Xml
	FROM dbo.DummyBizo
	WHERE Z0_Date = @CWO1_
union
SELECT 
Z0_PK, Z0_AddInfo, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber,
Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte,
Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal,
Z0_Description, Z0_FK_Code, CAST(Z0_Geography AS varbinary(max)) AS Z0_Geography, Z0_Guid, Z0_IsSystem,
Z0_IsValid, Z0_Long, Z0_Money, Z0_NAddInfo, Z0_Number,
Z0_NVarChar, case when Z0_NVarCharMax is null then null when datalength(Z0_NVarCharMax) < 1024 then Z0_NVarCharMax else char(0) + char(0) end as Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_SparseBit,
Z0_SparseByte, Z0_SparseChar, Z0_SparseDate, Z0_SparseDateTime, Z0_SparseDateTimeOffset,
Z0_SparseDecimal, Z0_SparseGuid, Z0_SparseLong, Z0_SparseMoney, Z0_SparseNumber,
Z0_SparseNVarChar, Z0_SparseShort, Z0_SparseSmallDateTime, Z0_SparseTime, case when Z0_SparseVarBinaryMax is null then null when datalength(Z0_SparseVarBinaryMax) < 1024 then Z0_SparseVarBinaryMax else 0x0000 end as Z0_SparseVarBinaryMax,
Z0_SparseVarChar, case when Z0_SparseXml is null then null when datalength(Z0_SparseXml) < 1024 then cast(Z0_SparseXml as nvarchar(max)) else '<?placeholder LazyLoading=""Yes""?>' end as Z0_SparseXml, Z0_Time, case when Z0_VarBinaryMax is null then null when datalength(Z0_VarBinaryMax) < 1024 then Z0_VarBinaryMax else 0x0000 end as Z0_VarBinaryMax, case when Z0_VarCharMax is null then null when datalength(Z0_VarCharMax) < 1024 then Z0_VarCharMax else char(0) + char(0) end as Z0_VarCharMax,
case when Z0_Xml is null then null when datalength(Z0_Xml) < 1024 then cast(Z0_Xml as nvarchar(max)) else '<?placeholder LazyLoading=""Yes""?>' end as Z0_Xml
	FROM dbo.DummyBizo WHERE Z0_Code = @CWO2_) AS MyUnion", new ZCountDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, filter).ParameterisedQueryText);
		}

		public void TestZCountDataQuery()
		{
			var filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_BitFalse, true);
			var query = new ZCountDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, filter);
			var expected = "SELECT COUNT(*) FROM dbo.DummyBizo WHERE Z0_BitFalse = @CWO1_";

			AssertEquals("Query text", expected, query.ParameterisedQueryText);
			AssertEquals("Query param name", "@CWO1_", query.Parameters[0].ParameterName);
			AssertEquals("Query param value", 1, query.Parameters[0].Value);
			AssertEquals("Table name", query.TableName, DummyBizoSchema.Constants.TableName);

			filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_BitFiltered, true);
			query = new ZCountDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, filter);
			expected = "SELECT COUNT(*) FROM dbo.DummyBizo WHERE Z0_BitFiltered = 1";

			AssertEquals("Query text", expected, query.ParameterisedQueryText);
			AssertEquals("Query param name", "@CWO1_", query.Parameters[0].ParameterName);
			AssertEquals("Query param value", 1, query.Parameters[0].Value);
			AssertEquals("Table name", query.TableName, DummyBizoSchema.Constants.TableName);
		}
	}
}
