using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.BuildTools;
using CargoWise.Data;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	public abstract class AutoCodeTestCase : TestCase
	{
		#region Supported SQL and ADO Types

		protected string[] SupportedSqlDataTypes
		{
			get { return fSupportedSqlDataTypes; }
		}

		protected SqlDbType[] SupportedAdoDataTypes
		{
			get
			{
				if (fSupportedAdoDataTypes == null)
				{
					fSupportedAdoDataTypes = new SqlDbType[SupportedSqlDataTypes.Length];

					for (int i = 0; i < SupportedSqlDataTypes.Length; i++)
					{
						fSupportedAdoDataTypes[i] = DbTypes.Instance[SupportedSqlDataTypes[i]];
					}
				}

				return fSupportedAdoDataTypes;
			}
		}

		SqlDbType[] fSupportedAdoDataTypes;

		readonly string[] fSupportedSqlDataTypes = new string[]
		{
			"bit",
			"binary",
			"char",
			"date",
			"datetime",
			"datetime2",
			"datetimeoffset",
			"decimal",
			"geography",
			"int",
			"money",
			"nvarchar",
			"smalldatetime",
			"smallint",
			"tinyint",
			"uniqueidentifier",
			"varchar",
			"xml",
			"varbinary",
			"nchar",
			"bigint"
		};

		#endregion

		protected BusinessObjectInfo CreateInfo(DataTable table)
		{
			return CreateInfo(table, Array.Empty<string>());
		}

		protected BusinessObjectInfo CreateInfo(DataTable table, string[] indexes)
		{
			return CreateInfo(table, indexes, Array.Empty<string>(), Array.Empty<string>());
		}

		protected virtual BusinessObjectInfo CreateInfo(DataTable table, string[] indexes, string[] literalOnlyColumns, string[] nonBlankFilteredIndexColumns, string baseClassName = "BusinessObject", string @namespace = "Enterprise", bool masterFileReference = true, BuildXmlBizOEntryCollection masterFiles = null)
		{
			return new BusinessObjectInfo(
				fileName: "DummyFileName"
				, isInZArchitectureSolution: false
				, isPersistent: true
				, @namespace: @namespace
				, namespaceOfSchema: "Enterprise"
				, baseClassName: baseClassName
				, sqlSchemaName: Db.SqlDbOwnerSchema
				, table: table
				, decimalScaleTable: new DataTable()
				, foreignKeysTable: new DataTable()
				, dateTimeOffsetTable: new DataTable()
				, refDbCountry: ""
				, refDbType: null
				, dbTypes: CreateTestDbTypes()
				, uniqueKeys: new HashSet<string> { { "TT_PK" } }
				, canForceUpdateNaturalKeyCacheColumns: new HashSet<string>()
				, masterFiles: masterFiles ?? new BuildXmlBizOEntryCollection()
				, pkIndex: "PK_UX__TT_PK"
				, indexes: indexes
				, masterFileReference: masterFileReference
				, preventDelete: false
				, literalOnlyColumns: literalOnlyColumns
				, nonBlankFilteredIndexColumns: nonBlankFilteredIndexColumns
				, tables: new Dictionary<string, ITableInfo>());
		}

		protected virtual Dictionary<string, string> CreateTestDbTypes()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();

			result.Add("TT_PK", "uniqueidentifier");

			result.Add("TT_Char", "char");
			result.Add("TT_Code", "char");
			result.Add("TT_DateTime", "datetime");
			result.Add("TT_DateTime2", "datetime2");
			result.Add("TT_DateTimeOffset", "datetimeoffset");
			result.Add("TT_Date", "date");
			result.Add("TT_Decimal", "decimal");
			result.Add("TT_Int32", "int");
			result.Add("TT_Money", "money");
			result.Add("TT_UnicodeString", "nvarchar");
			result.Add("TT_SmallDateTime", "smalldatetime");
			result.Add("TT_Int16", "smallint");
			result.Add("TT_Byte", "tinyint");
			result.Add("TT_Guid", "uniqueidentifier");
			result.Add("TT_String", "varchar");
			result.Add("TT_Xml", "xml");
			result.Add("TT_Binary", "varbinary");
			result.Add("TT_FixedBinary", "binary");
			result.Add("TT_Bit", "bit");
			result.Add("TT_BigInt", "bigint");
			result.Add("TT_Geography", "geography");
			result.Add("TT_Time", "time");

			result.Add("TT_SparseChar", "char");
			result.Add("TT_SparseDateTime", "datetime");
			result.Add("TT_SparseDateTime2", "datetime2");
			result.Add("TT_SparseDateTimeOffset", "datetimeoffset");
			result.Add("TT_SparseDate", "date");
			result.Add("TT_SparseDecimal", "decimal");
			result.Add("TT_SparseInt32", "int");
			result.Add("TT_SparseMoney", "money");
			result.Add("TT_SparseUnicodeString", "nvarchar");
			result.Add("TT_SparseSmallDateTime", "smalldatetime");
			result.Add("TT_SparseInt16", "smallint");
			result.Add("TT_SparseByte", "tinyint");
			result.Add("TT_SparseGuid", "uniqueidentifier");
			result.Add("TT_SparseString", "varchar");
			result.Add("TT_SparseXml", "xml");
			result.Add("TT_SparseBinary", "varbinary");
			result.Add("TT_SparseFixedBinary", "binary");
			result.Add("TT_SparseBit", "bit");
			result.Add("TT_SparseBigInt", "bigint");
			result.Add("TT_SparseTime", "time");

			result.Add("TT_ComputedChar", "char");
			result.Add("TT_ComputedDateTime", "datetime");
			result.Add("TT_ComputedDateTime2", "datetime2");
			result.Add("TT_ComputedDateTimeOffset", "datetimeoffset");
			result.Add("TT_ComputedDate", "date");
			result.Add("TT_ComputedDecimal", "decimal");
			result.Add("TT_ComputedInt32", "int");
			result.Add("TT_ComputedMoney", "money");
			result.Add("TT_ComputedUnicodeString", "nvarchar");
			result.Add("TT_ComputedSmallDateTime", "smalldatetime");
			result.Add("TT_ComputedInt16", "smallint");
			result.Add("TT_ComputedByte", "tinyint");
			result.Add("TT_ComputedGuid", "uniqueidentifier");
			result.Add("TT_ComputedString", "varchar");
			result.Add("TT_ComputedXml", "xml");
			result.Add("TT_ComputedBinary", "varbinary");
			result.Add("TT_ComputedFixedBinary", "binary");
			result.Add("TT_ComputedBit", "bit");
			result.Add("TT_ComputedBigInt", "bigint");
			result.Add("TT_ComputedTime", "time");

			return result;
		}

		protected virtual DataTable CreateTestDataTable()
		{
			DataTable result = new DataTable("TestTable");

			DataColumn tT_PKColumn = new DataColumn("TT_PK", typeof(Guid));
			result.Columns.Add(tT_PKColumn);
			result.PrimaryKey = new DataColumn[] { tT_PKColumn };

			result.Columns.AddRange(
				new DataColumn[]
				{
					new DataColumn("TT_Char", typeof(string)) { MaxLength = 3 },
					new DataColumn("TT_Code", typeof(string)) { MaxLength = 3 },
					new DataColumn("TT_DateTime", typeof(DateTime)),
					new DataColumn("TT_DateTime2", typeof(DateTime)),
					new DataColumn("TT_DateTimeOffset", typeof(DateTimeOffset)),
					new DataColumn("TT_Date", typeof(DateTime)),
					new DataColumn("TT_Decimal", typeof(decimal)),
					new DataColumn("TT_Int32", typeof(int)),
					new DataColumn("TT_Money", typeof(decimal)),
					new DataColumn("TT_UnicodeString", typeof(string)) { MaxLength = 10 },
					new DataColumn("TT_SmallDateTime", typeof(DateTime)),
					new DataColumn("TT_Int16", typeof(short)),
					new DataColumn("TT_Byte", typeof(byte)),
					new DataColumn("TT_Guid", typeof(Guid)),
					new DataColumn("TT_String", typeof(string)),
					new DataColumn("TT_Xml", typeof(string)),
					new DataColumn("TT_Binary", typeof(byte[])),
					new DataColumn("TT_FixedBinary", typeof(byte[])),
					new DataColumn("TT_Bit", typeof(bool)),
					new DataColumn("TT_BigInt", typeof(long)),
					new DataColumn("TT_Geography", typeof(SqlGeography)),
					new DataColumn("TT_Time", typeof(TimeSpan)),

					CreateSparseColumn("TT_SparseChar", typeof(string), x => x.MaxLength = 3),
					CreateSparseColumn("TT_SparseDateTime", typeof(DateTime)),
					CreateSparseColumn("TT_SparseDateTime2", typeof(DateTime)),
					CreateSparseColumn("TT_SparseDateTimeOffset", typeof(DateTimeOffset)),
					CreateSparseColumn("TT_SparseDate", typeof(DateTime)),
					CreateSparseColumn("TT_SparseDecimal", typeof(decimal)),
					CreateSparseColumn("TT_SparseInt32", typeof(int)),
					CreateSparseColumn("TT_SparseMoney", typeof(decimal)),
					CreateSparseColumn("TT_SparseUnicodeString", typeof(string), x => x.MaxLength = 10),
					CreateSparseColumn("TT_SparseSmallDateTime", typeof(DateTime)),
					CreateSparseColumn("TT_SparseInt16", typeof(short)),
					CreateSparseColumn("TT_SparseByte", typeof(byte)),
					CreateSparseColumn("TT_SparseGuid", typeof(Guid)),
					CreateSparseColumn("TT_SparseString", typeof(string)),
					CreateSparseColumn("TT_SparseXml", typeof(string)),
					CreateSparseColumn("TT_SparseBinary", typeof(byte[])),
					CreateSparseColumn("TT_SparseFixedBinary", typeof(byte[])),
					CreateSparseColumn("TT_SparseBit", typeof(bool)),
					CreateSparseColumn("TT_SparseBigInt", typeof(long)),
					CreateSparseColumn("TT_SparseTime", typeof(TimeSpan)),

					CreateComputedColumn("TT_ComputedChar", typeof(string), x => x.MaxLength = 3),
					CreateComputedColumn("TT_ComputedDateTime", typeof(DateTime)),
					CreateComputedColumn("TT_ComputedDateTime2", typeof(DateTime)),
					CreateComputedColumn("TT_ComputedDateTimeOffset", typeof(DateTimeOffset)),
					CreateComputedColumn("TT_ComputedDate", typeof(DateTime)),
					CreateComputedColumn("TT_ComputedDecimal", typeof(decimal)),
					CreateComputedColumn("TT_ComputedInt32", typeof(int)),
					CreateComputedColumn("TT_ComputedMoney", typeof(decimal)),
					CreateComputedColumn("TT_ComputedUnicodeString", typeof(string), x => x.MaxLength = 10),
					CreateComputedColumn("TT_ComputedSmallDateTime", typeof(DateTime)),
					CreateComputedColumn("TT_ComputedInt16", typeof(short)),
					CreateComputedColumn("TT_ComputedByte", typeof(byte)),
					CreateComputedColumn("TT_ComputedGuid", typeof(Guid)),
					CreateComputedColumn("TT_ComputedString", typeof(string)),
					CreateComputedColumn("TT_ComputedXml", typeof(string)),
					CreateComputedColumn("TT_ComputedBinary", typeof(byte[])),
					CreateComputedColumn("TT_ComputedFixedBinary", typeof(byte[])),
					CreateComputedColumn("TT_ComputedBit", typeof(bool)),
					CreateComputedColumn("TT_ComputedBigInt", typeof(long)),
					CreateComputedColumn("TT_ComputedTime", typeof(TimeSpan)),
				});

			return result;
		}

		static DataColumn CreateSparseColumn(string columnName, Type type, Action<DataColumn> modifier = null)
		{
			var result = new DataColumn(columnName, type) { AllowDBNull = true };
			result.ExtendedProperties.Add("IsSparse", "Y");
			modifier?.Invoke(result);
			return result;
		}

		static DataColumn CreateComputedColumn(string columnName, Type type, Action<DataColumn> modifier = null)
		{
			var result = new DataColumn(columnName, type) { AllowDBNull = true };
			result.ExtendedProperties.Add("IsComputed", "Y");
			modifier?.Invoke(result);
			return result;
		}

		#region Test objects

		protected AutoProperty GetAutoPropertyWithAdoDataType(SqlDbType sqlDbType)
		{
			foreach (AutoProperty property in CodeCollection.AutoBusinessObject.AutoProperties.Properties)
			{
				if (property.SqlDbType == sqlDbType)
				{
					return property;
				}
			}

			throw new ArgumentException(string.Format("No column exists in the test table with SqlDbType of <{0}>.", sqlDbType));
		}

		protected AutoBusinessObjectCodeCollection CodeCollection
		{
			get
			{
				if (fCodeCollection == null)
				{
					fCodeCollection = new AutoBusinessObjectCodeCollection(CreateInfo(CreateTestDataTable()));
				}
				return fCodeCollection;
			}
		}

		AutoBusinessObjectCodeCollection fCodeCollection;

		#endregion

		#region Expected Schema result

		protected const string ExpectedSchemaResult = @"		#region Schema

		public abstract class Schema
		{
			public const string TableName = ""TestTable"";
			public const string PK        = ""TT_PK"";

			public const string TT_BigInt                 = ""TT_BigInt"";
			public const string TT_Binary                 = ""TT_Binary"";
			public const string TT_Bit                    = ""TT_Bit"";
			public const string TT_Byte                   = ""TT_Byte"";
			public const string TT_Char                   = ""TT_Char"";
			public const string TT_Code                   = ""TT_Code"";
			public const string TT_ComputedBigInt         = ""TT_ComputedBigInt"";
			public const string TT_ComputedBinary         = ""TT_ComputedBinary"";
			public const string TT_ComputedBit            = ""TT_ComputedBit"";
			public const string TT_ComputedByte           = ""TT_ComputedByte"";
			public const string TT_ComputedChar           = ""TT_ComputedChar"";
			public const string TT_ComputedDate           = ""TT_ComputedDate"";
			public const string TT_ComputedDateTime       = ""TT_ComputedDateTime"";
			public const string TT_ComputedDateTime2      = ""TT_ComputedDateTime2"";
			public const string TT_ComputedDateTimeOffset = ""TT_ComputedDateTimeOffset"";
			public const string TT_ComputedDecimal        = ""TT_ComputedDecimal"";
			public const string TT_ComputedFixedBinary    = ""TT_ComputedFixedBinary"";
			public const string TT_ComputedGuid           = ""TT_ComputedGuid"";
			public const string TT_ComputedInt16          = ""TT_ComputedInt16"";
			public const string TT_ComputedInt32          = ""TT_ComputedInt32"";
			public const string TT_ComputedMoney          = ""TT_ComputedMoney"";
			public const string TT_ComputedSmallDateTime  = ""TT_ComputedSmallDateTime"";
			public const string TT_ComputedString         = ""TT_ComputedString"";
			public const string TT_ComputedTime           = ""TT_ComputedTime"";
			public const string TT_ComputedUnicodeString  = ""TT_ComputedUnicodeString"";
			public const string TT_ComputedXml            = ""TT_ComputedXml"";
			public const string TT_Date                   = ""TT_Date"";
			public const string TT_DateTime               = ""TT_DateTime"";
			public const string TT_DateTime2              = ""TT_DateTime2"";
			public const string TT_DateTimeOffset         = ""TT_DateTimeOffset"";
			public const string TT_Decimal                = ""TT_Decimal"";
			public const string TT_FixedBinary            = ""TT_FixedBinary"";
			public const string TT_Geography              = ""TT_Geography"";
			public const string TT_Guid                   = ""TT_Guid"";
			public const string TT_Int16                  = ""TT_Int16"";
			public const string TT_Int32                  = ""TT_Int32"";
			public const string TT_Money                  = ""TT_Money"";
			public const string TT_SmallDateTime          = ""TT_SmallDateTime"";
			public const string TT_SparseBigInt           = ""TT_SparseBigInt"";
			public const string TT_SparseBinary           = ""TT_SparseBinary"";
			public const string TT_SparseBit              = ""TT_SparseBit"";
			public const string TT_SparseByte             = ""TT_SparseByte"";
			public const string TT_SparseChar             = ""TT_SparseChar"";
			public const string TT_SparseDate             = ""TT_SparseDate"";
			public const string TT_SparseDateTime         = ""TT_SparseDateTime"";
			public const string TT_SparseDateTime2        = ""TT_SparseDateTime2"";
			public const string TT_SparseDateTimeOffset   = ""TT_SparseDateTimeOffset"";
			public const string TT_SparseDecimal          = ""TT_SparseDecimal"";
			public const string TT_SparseFixedBinary      = ""TT_SparseFixedBinary"";
			public const string TT_SparseGuid             = ""TT_SparseGuid"";
			public const string TT_SparseInt16            = ""TT_SparseInt16"";
			public const string TT_SparseInt32            = ""TT_SparseInt32"";
			public const string TT_SparseMoney            = ""TT_SparseMoney"";
			public const string TT_SparseSmallDateTime    = ""TT_SparseSmallDateTime"";
			public const string TT_SparseString           = ""TT_SparseString"";
			public const string TT_SparseTime             = ""TT_SparseTime"";
			public const string TT_SparseUnicodeString    = ""TT_SparseUnicodeString"";
			public const string TT_SparseXml              = ""TT_SparseXml"";
			public const string TT_String                 = ""TT_String"";
			public const string TT_Time                   = ""TT_Time"";
			public const string TT_UnicodeString          = ""TT_UnicodeString"";
			public const string TT_Xml                    = ""TT_Xml"";

			public const int TT_CharMaxLength = 3;
			public const int TT_CodeMaxLength = 3;
			public const int TT_ComputedCharMaxLength = 3;
			public const int TT_ComputedUnicodeStringMaxLength = 10;
			public const int TT_SparseCharMaxLength = 3;
			public const int TT_SparseUnicodeStringMaxLength = 10;
			public const int TT_UnicodeStringMaxLength = 10;
		}

		#endregion";

		#endregion
	}
}
