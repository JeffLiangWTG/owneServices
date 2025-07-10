using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Schema.Testing
{
	public class TableColumnHelperTest : ThreadSafeAccessTestCase
	{
		public void TestGetColumns_NoSystemColumns()
		{
			CombineAssertions(() =>
			{
				var columns = new Dictionary<string, string>();
				foreach (var pair in TableColumnHelper.GetColumns(JobDeclarationSchema.Constants.TableName).Union(TableColumnHelper.GetColumns(OrgSalesProductSchema.Constants.TableName)).Union(TableColumnHelper.GetColumns(RefCityPCodePivotSchema.Constants.TableName)))
				{
					columns.Add(pair.Key, pair.Value);
				}
				foreach (var column in new SchemaColumn[]
				{
					JobDeclarationSchema.PK,
					JobDeclarationSchema.JE_IsValid,
					JobDeclarationSchema.JE_ClusterKey,
					JobDeclarationSchema.JE_SystemCreateTimeUtc,
					JobDeclarationSchema.JE_SystemCreateUser,
					JobDeclarationSchema.JE_SystemLastEditTimeUtc,
					JobDeclarationSchema.JE_SystemLastEditUser,
					OrgSalesProductSchema.MP_IsSystemDefined,
					OrgSalesProductSchema.MP_IsActive,
					RefCityPCodePivotSchema.R0_IsSystem
				})
				{
					AssertEquals($"{column.GetType().Name} {column.Name}", false, columns.ContainsKey(column.Name));
				}
			});
		}

		public void TestGetColumns()
		{
			CombineAssertions(() =>
			{
				ITableSchema tableSchema = DummyBizoSchema.Instance;
				var columns = new Dictionary<string, string>();
				foreach (var pair in TableColumnHelper.GetColumns(tableSchema.TableName))
				{
					columns.Add(pair.Key, pair.Value);
				}
				AssertEquals("Should not contain PK", false, columns.ContainsKey(DummyBizoSchema.PK.Name));
				foreach (var expectResult in new (SchemaColumn column, string dataType)[]
												{
												(DummyBizoSchema.Z0_VarBinaryMax, "ZBlob"),
												(DummyBizoSchema.Z0_Xml, "ZString"),
												(DummyBizoSchema.Z0_Short, "ZShort"),
												(DummyBizoSchema.Z0_Bool, "ZBool"),
												(DummyBizoSchema.Z0_Byte, "ZByte"),
												(DummyBizoSchema.Z0_Code, "ZString"),
												(DummyBizoSchema.Z0_Date, "ZDateTime"),
												(DummyBizoSchema.Z0_DateOnly, "ZDate"),
												(DummyBizoSchema.Z0_Decimal, "ZDecimal"),
												(DummyBizoSchema.Z0_Geography, "ZGeography"),
												(DummyBizoSchema.Z0_Guid, "ZGuid"),
												(DummyBizoSchema.Z0_Long, "ZLong"),
												(DummyBizoSchema.Z0_Number, "ZInt"),
												(DummyBizoSchema.Z0_DateTimeOffset, "ZDateTimeOffset")
												})
				{
					AssertEquals($"{expectResult.column.GetType().Name} {expectResult.column.Name}", expectResult.dataType, columns[expectResult.column.Name]);
				}
			});
		}
	}
}
