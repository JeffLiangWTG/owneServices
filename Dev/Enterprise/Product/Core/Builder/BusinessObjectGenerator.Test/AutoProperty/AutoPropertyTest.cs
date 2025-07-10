using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.BuildTools;
using CargoWise.Data;
using Enterprise.ZArchitecture.Business;
using Microsoft.SqlServer.Types;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	sealed class AutoPropertyTest : AutoCodeTestCase
	{
		public void TestAllViewBackingColumnsAreAcknowledged()
		{
			//first, make a list of all Views that are also BusinessObjects
			var views = GetViews();

			//ok, now grab all their columns from the database
			var sql = string.Format(@"select distinct name from sys.columns c where
object_id in (select object_id from sys.views v
where v.name in ({0}))", views.Select(x => "'" + x + "'").Aggregate((x, y) => x + ", " + y));
			var columns = new List<string>();

			using (var reader = Db.Connection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					columns.Add(reader.GetString(0));
				}
			}

			//now make a list of all failing items
			var missingFailures = new StringBuilder();
			foreach (var column in columns)
			{
				if (!BuildXml.Instance.BackingColumns.ContainsKey(column))
				{
					missingFailures.AppendLine(string.Format("    <BackingColumn Column=\"{0}\" SourceColumn=\"\" SourceTable=\"\" />", column));
				}
			}

			//next make a list of unnecessary items
			var columnHashSet = columns.ToHashSet();
			var additionalColumns = BuildXml.Instance.BackingColumns.Keys.Where(x => !columnHashSet.Contains(x)).ToList();

			CombineAssertions(() =>
			{
				var missingFailuresString = missingFailures.ToString();
				if (!string.IsNullOrEmpty(missingFailuresString))
				{
					Fail(@"Some view columns don't have BackingColumns in Build.xml.
To inherit underlying properties and optimizations (such as IsNonBlankFilteredIndexParticipant and Smart parameterization), you need to do this.
Note that, for Smart Parameterization bucketing, any Foreign Keys should reference an FK column with a matching density, for example WhsInventoryView.WI_OP should map to WhsDocketLine.WE_OP rather than OrgSupplierPart.OP_PK.

Copy paste this into Build.xml and fill out the ones that are unambiguous and important:

" + missingFailuresString);
				}

				var extraFailuresString = string.Join(Environment.NewLine, additionalColumns);
				if (!string.IsNullOrEmpty(extraFailuresString))
				{
					Fail("There are some redundant BackingColumns in Build.xml. These columns are not used in any view and should be removed from Build.xml:\r\n" + extraFailuresString);
				}
			});

			Assert(true);
		}

		static HashSet<string> GetViews()
		{
			var views = new HashSet<string>();
			var sql = "select distinct name from sys.views";
			var schemaOnlyViews = BuildXml.Instance.SchemaOnlyViews.ToHashSet();

			using (var reader = Db.Connection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					var view = reader.GetString(0);
					if (BuildXml.Instance.AllBusinessObjects[view] != null || schemaOnlyViews.Contains(view))
					{
						views.Add(view);
					}
				}
			}

			return views;
		}

		public void TestRequiresEnglishCharactersValidation()
		{
			AutoProperty property = GetAutoPropertyWithAdoDataType(SqlDbType.VarChar);
			AssertEquals("AutoProperty with type SqlDbType.VarChar requires english-language validation.", true, property.RequiresEnglishCharactersValidation);

			// ensure all other types do not require english-language validation to be generated
			foreach (SqlDbType adoDataType in SupportedAdoDataTypes)
			{
				if (adoDataType != SqlDbType.VarChar)
				{
					property = GetAutoPropertyWithAdoDataType(adoDataType);
					AssertEquals("AutoProperty with type " + adoDataType + " does not require english-language validation.", false, property.RequiresEnglishCharactersValidation);
					Assert(property.HasSqlDbType);
				}
			}
		}

		public void TestHasSqlDbType()
		{
			DataTable table = CreateTestDataTable();
			BusinessObjectInfo bizObjInfo = GetBizObjInfo(table);

			AutoProperty property = new AutoProperty(bizObjInfo, table.Columns[0]);

			Assert(!property.HasSqlDbType);
			Assert(!property.RequiresEnglishCharactersValidation);
		}

		public void TestOptimisticConcurrencyCheckIsNotUsedForSystemAuditProperties()
		{
			DataTable table = CreateTableWithSystemFields();
			BusinessObjectInfo bizObjInfo = GetBizObjInfo(table);

			AutoProperty property = new AutoProperty(bizObjInfo, table.Columns["TT_NonSystemField"]);
			Assert("Should require optimistic conccurency check for none-system fields", property.RequiresConcurrencyCheck);

			property = new AutoProperty(bizObjInfo, table.Columns["TT_SystemField"]);
			Assert("Should not require optimistic conccurency check for system fields", !property.RequiresConcurrencyCheck);
		}

		public void TestShouldOverrideConcurrencyPolicyInZPropertyInfoTemplateCodeForSystemProperties()
		{
			StringLineAssembler asm;
			MockAutoProperty property;

			DataTable table = CreateTableWithSystemFields();
			BusinessObjectInfo bizObjInfo = GetBizObjInfo(table);

			property = new MockAutoProperty(bizObjInfo, table.Columns["TT_NonSystemField"]);

			asm = new StringLineAssembler(2)
			.Add("public virtual ZPropertyInfo TT_NonSystemFieldInfo")
			.Add("{")
					.Add("[System.Diagnostics.DebuggerStepThrough()]", 1)
					.Add("get { return GetZPropertyInfo(); }", 1)
			.Add("}");

			AssertEquals(asm.ToString(), property.CodeForZPropertyInfoAccessor);

			property = new MockAutoProperty(bizObjInfo, table.Columns["TT_SystemField"]);

			asm = new StringLineAssembler(2)
			.Add("public virtual ZPropertyInfo TT_SystemFieldInfo")
			.Add("{")
					.Add("[System.Diagnostics.DebuggerStepThrough()]", 1)
					.Add("get { return GetZPropertyInfo(); }", 1)
			.Add("}");

			AssertEquals("concurrency info change happens in constructor now", asm.ToString(), property.CodeForZPropertyInfoAccessor);
		}

		public void TestIsNaturalKeyString()
		{
			DataTable table = CreateTableWithSystemFields();
			BusinessObjectInfo bizObjInfo = GetBizObjInfo(table);

			string columnName1 = "TT_IntField";
			string columnName2 = "TT_StringField";

			bizObjInfo.Table.Columns.Add(new DataColumn(columnName1, typeof(int)));
			bizObjInfo.Table.Columns.Add(new DataColumn(columnName2, typeof(string)));

			string keyName1 = bizObjInfo.Table.TableName + "." + columnName1;
			string keyName2 = bizObjInfo.Table.TableName + "." + columnName2;

			bizObjInfo.UniqueKeys.Add(keyName1.ToUpperInvariant());
			bizObjInfo.UniqueKeys.Add(keyName2.ToUpperInvariant());

			AutoProperty property1 = new AutoProperty(bizObjInfo, table.Columns[columnName1]);
			AutoProperty property2 = new AutoProperty(bizObjInfo, table.Columns[columnName2]);

			AssertEquals("Property is not a string so should not be considered a natural key string even though it has a unique index", false, property1.IsNaturalKeyString);
			AssertEquals("Property should be considered a natural key string", true, property2.IsNaturalKeyString);

			bizObjInfo.UniqueKeys.Clear();
			AssertEquals("Property does not have a unique index so should not be considered a natural key string", false, property2.IsNaturalKeyString);
		}

		public void TestIsLiteralOnly()
		{
			string columnName1 = "TT_IntField";
			string columnName2 = "TT_StringField";

			var literalOnlyColumns = new string[]
				{
					columnName1,
				};

			var table = CreateTableWithSystemFields();
			var info = GetBizObjInfo(table, literalOnlyColumns: literalOnlyColumns);

			info.Table.Columns.Add(new DataColumn(columnName1, typeof(int)));
			info.Table.Columns.Add(new DataColumn(columnName2, typeof(string)));

			var property1 = new AutoProperty(info, table.Columns[columnName1]);
			var property2 = new AutoProperty(info, table.Columns[columnName2]);

			AssertEquals("Property should be considered a literal only", true, property1.IsLiteralOnly);
			AssertEquals("Property should not be considered a literal only", false, property2.IsLiteralOnly);
		}

		public void TestIsSparse()
		{
			const string columnName1 = "TT_IntField";
			const string columnName2 = "TT_StringField";
			const string systemField = "TT_SystemField";

			var table = CreateTableWithSystemFields();
			var info = GetBizObjInfo(table);

			var column1 = new DataColumn(columnName1, typeof(int));
			column1.ExtendedProperties.Add("IsSparse", "Y");
			var column2 = new DataColumn(columnName2, typeof(string));
			column2.ExtendedProperties.Add("IsSparse", "y");
			info.Table.Columns.Add(column1);
			info.Table.Columns.Add(column2);

			var property1 = new AutoProperty(info, table.Columns[columnName1]);
			var property2 = new AutoProperty(info, table.Columns[columnName2]);
			var property3 = new AutoProperty(info, table.Columns[systemField]);

			AssertEquals("Extended Property IsSparse is Y", true, property1.IsSparse);
			AssertEquals("Extended Property IsSparse is y", true, property2.IsSparse);
			AssertEquals("Extended Property IsSparse doesn't exist", false, property3.IsSparse);
		}

		public void TestIsSensitive()
		{
			const string columnName1 = "TT_IntField";
			const string columnName2 = "TT_StringField";
			const string systemField = "TT_SystemField";

			var table = CreateTableWithSystemFields();
			var info = GetBizObjInfo(table);

			var column1 = new DataColumn(columnName1, typeof(int));
			column1.ExtendedProperties.Add("IsSensitive", "Y");
			var column2 = new DataColumn(columnName2, typeof(string));
			column2.ExtendedProperties.Add("IsSensitive", "y");
			info.Table.Columns.Add(column1);
			info.Table.Columns.Add(column2);

			var property1 = new AutoProperty(info, table.Columns[columnName1]);
			var property2 = new AutoProperty(info, table.Columns[columnName2]);
			var property3 = new AutoProperty(info, table.Columns[systemField]);

			AssertEquals("Extended Property IsSensitive is Y", true, property1.IsSensitive);
			AssertEquals("Extended Property IsSensitive is y", true, property2.IsSensitive);
			AssertEquals("Extended Property IsSensitive doesn't exist", false, property3.IsSensitive);
		}

		public void TestCanForceUpdateNaturalKeyCache()
		{
			DataTable table = CreateTableWithSystemFields();
			BusinessObjectInfo bizObjInfo = GetBizObjInfo(table);

			string intColumn = "TT_IntField";
			string stringColumn = "TT_StringField";

			bizObjInfo.Table.Columns.Add(new DataColumn(intColumn, typeof(int)));
			bizObjInfo.Table.Columns.Add(new DataColumn(stringColumn, typeof(string)));

			AutoProperty intProperty = new AutoProperty(bizObjInfo, table.Columns[intColumn]);
			AutoProperty stringProperty = new AutoProperty(bizObjInfo, table.Columns[stringColumn]);

			bizObjInfo.CanForceUpdateNaturalKeyCacheColumns.Add(intProperty.TableNameColumnName);
			bizObjInfo.CanForceUpdateNaturalKeyCacheColumns.Add(stringProperty.TableNameColumnName);

			AssertEquals("Property is not a string so cannot force update natural key cache",
				false, intProperty.CanForceUpdateNaturalKeyCache);
			AssertEquals("Property can force update natural key cache because it is in the list",
				true, stringProperty.CanForceUpdateNaturalKeyCache);

			bizObjInfo.CanForceUpdateNaturalKeyCacheColumns.Clear();
			AssertEquals("Property cannot force update natural key cache because it is not in the list",
				false, stringProperty.CanForceUpdateNaturalKeyCache);
		}

		public void TestCodeForPropertySet_ZString()
		{
			var table = new DataTable("test");
			var stringColumn = "TT_MyText";
			table.Columns.Add(new DataColumn(stringColumn, typeof(string)));
			var bizObjInfo = GetBizObjInfo(table);
			var stringProperty = new AutoProperty(bizObjInfo, table.Columns[stringColumn]);
			var expected =
@"			{
				value = value.TrimEndSpaceTab();
				var zPropertyInfo = TT_MyTextInfo;
				CheckMaximumLength(zPropertyInfo, value);
				SetPropertyValue(zPropertyInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTT_MyText();
				}
			}";

			AssertContains(expected, stringProperty.Code);
		}

		public void TestCodeToUpdateNaturalKeyCacheIsInPropertySetter()
		{
			DataTable table = CreateTableWithSystemFields();
			BusinessObjectInfo bizObjInfo = GetBizObjInfo(table);

			string intColumn = "TT_IntField";
			string stringColumn = "TT_StringField";
			bizObjInfo.Table.Columns.Add(new DataColumn(stringColumn, typeof(string)));
			bizObjInfo.Table.Columns.Add(new DataColumn(intColumn, typeof(int)));
			string intKey = bizObjInfo.Table.TableName + "." + intColumn;
			string stringKey = bizObjInfo.Table.TableName + "." + stringColumn;
			bizObjInfo.UniqueKeys.Add(intKey.ToUpperInvariant());
			bizObjInfo.UniqueKeys.Add(stringKey.ToUpperInvariant());

			AutoProperty intProperty = new AutoProperty(bizObjInfo, table.Columns[intColumn]);
			AutoProperty stringProperty = new AutoProperty(bizObjInfo, table.Columns[stringColumn]);
			string intCode = "Factory.UpdateNaturalKeyCache";
			string stringCode = "Factory.UpdateNaturalKeyCache(this, testSchema.TT_StringField, zPropertyInfo.Value, value);";
			AssertEquals(false, intProperty.Code.Contains(intCode));
			AssertEquals(true, stringProperty.Code.Contains(stringCode));
			AssertEquals(true, stringProperty.Code.Contains("var zPropertyInfo = TT_StringFieldInfo;"));

			bizObjInfo.UniqueKeys.Clear();
			stringProperty = new AutoProperty(bizObjInfo, table.Columns[stringColumn]);
			AssertEquals(false, stringProperty.Code.Contains(stringCode));
		}

		public void TestCodeToUpdateNaturalKeyCacheIsInPropertySetter_CanForce()
		{
			DataTable table = CreateTableWithSystemFields();
			BusinessObjectInfo bizObjInfo = GetBizObjInfo(table);

			string intColumn = "TT_IntField";
			string stringColumn = "TT_StringField";

			bizObjInfo.Table.Columns.Add(new DataColumn(intColumn, typeof(int)));
			bizObjInfo.Table.Columns.Add(new DataColumn(stringColumn, typeof(string)));

			AutoProperty intProperty = new AutoProperty(bizObjInfo, table.Columns[intColumn]);
			AutoProperty stringProperty = new AutoProperty(bizObjInfo, table.Columns[stringColumn]);

			bizObjInfo.CanForceUpdateNaturalKeyCacheColumns.Add(intProperty.TableNameColumnName);
			bizObjInfo.CanForceUpdateNaturalKeyCacheColumns.Add(stringProperty.TableNameColumnName);

			string intCode = "ShouldUpdateNaturalKeyCacheWhen";
			string stringCode = "&& ShouldUpdateNaturalKeyCacheWhenTT_StringFieldChanges(value))";
			AssertEquals(false, intProperty.Code.Contains(intCode));
			AssertEquals(true, stringProperty.Code.Contains(stringCode));

			bizObjInfo.CanForceUpdateNaturalKeyCacheColumns.Clear();
			stringProperty = new AutoProperty(bizObjInfo, table.Columns[stringColumn]);
			AssertEquals(false, stringProperty.Code.Contains(stringCode));
		}

		public void TestCodeForDefaultValueForSqlGeographyIsPointEmpty()
		{
			DefaultValueIsPointEmpty(property => property.CodeForDefaultValue, "\t\t\trow[testSchema.Constants.TT_Geography] = GetGeographyColumnValue(row, testSchema.Constants.TT_Geography, \"POINT EMPTY\");");
		}

		public void TestCodeForPropertyDefaultForSqlGeographyIsPointEmpty()
		{
			DefaultValueIsPointEmpty(property => property.CodeForPropertyDefault, "Microsoft.SqlServer.Types.SqlGeography.Parse(\"POINT EMPTY\")");
		}

		static void DefaultValueIsPointEmpty(Func<AutoProperty, string> getFunc, string expected)
		{
			// Arrange
			const string columnName = "TT_Geography";
			var table = new DataTable("test");
			var dataColumn = new DataColumn(columnName, typeof(SqlGeography))
			{
				AllowDBNull = false,
				DefaultValue = SqlGeography.Null
			};
			table.Columns.Add(dataColumn);
			var bizObjInfo = GetBizObjInfo(table, literalOnlyColumns: columnName);
			var property = new AutoProperty(bizObjInfo, dataColumn);

			// Act
			var result = getFunc(property);

			// Assert
			AssertEquals(expected, result);
		}

		public void TestCodeForPropertyDefaultForRefDbTableFKStringColumn()
		{
			var table = new DataTable("RefCusRateCode");
			var dataColumn = new DataColumn("ZY1_ZZZ_NKDataGrouping", typeof(string)) { AllowDBNull = true };
			table.Columns.Add(dataColumn);

			var bizObjInfo = new BusinessObjectInfo(
				fileName: "TestRefDbSchemaIndexMetadata"
				, isInZArchitectureSolution: false
				, isPersistent: true
				, @namespace: "Enterprise"
				, namespaceOfSchema: "Enterprise"
				, baseClassName: "BusinessObject"
				, sqlSchemaName: Db.SqlDbOwnerSchema
				, table: table
				, decimalScaleTable: new DataTable()
				, foreignKeysTable: new DataTable()
				, dateTimeOffsetTable: new DataTable()
				, refDbCountry: null
				, refDbType: RefDbTypeEnum.Single
				, dbTypes: new Dictionary<string, string>()
				, uniqueKeys: new HashSet<string> { }
				, canForceUpdateNaturalKeyCacheColumns: new HashSet<string>()
				, masterFiles: new BuildXmlBizOEntryCollection()
				, pkIndex: null
				, indexes: Array.Empty<string>()
				, masterFileReference: false
				, preventDelete: false
				, literalOnlyColumns: Array.Empty<string>()
				, nonBlankFilteredIndexColumns: Array.Empty<string>()
				, tables: new Dictionary<string, ITableInfo>());

			var property = new AutoProperty(bizObjInfo, dataColumn);
			CombineAssertions(() =>
			{
				AssertEquals("FK string default value", "DBNull.Value", property.CodeForPropertyDefault);
				AssertEquals("FK string default value code", "\t\t\trow[RefCusRateCodeSchema.Constants.ZY1_ZZZ_NKDataGrouping] = DBNull.Value;", property.CodeForDefaultValue);
			});
		}

		#region TestIsNAddInfoField

		public void TestIsNAddInfoField()
		{
			var table = new DataTable("CustomAddInfo");
			var bizObjInfo = GetBizObjInfo(table);

			AssertIsNAddInfoField(bizObjInfo, "Y", true);
			AssertIsNAddInfoField(bizObjInfo, "N", false);
		}

		void AssertIsNAddInfoField(BusinessObjectInfo bizObjInfo, string value, bool expected)
		{
			var column = new DataColumn("TT_String", typeof(string));
			if (value != null)
			{
				column.ExtendedProperties.Add("IsNAddInfoField", value);
			}
			var property = new AutoProperty(bizObjInfo, column);

			AssertEquals(property.IsNAddInfoField, expected);
		}

		#endregion

		#region TestUppercaseValueInSetterForRL_NKAndRN_NK

		public void TestUppercaseValueInSetterForRL_NKAndRN_NK()
		{
			AssertUppercaseValueInSetter(false, "TT_StringField");
			AssertUppercaseValueInSetter(true, "TT_RL_NKField");
			AssertUppercaseValueInSetter(true, "TT_RN_NKField");
			AssertUppercaseValueInSetter(true, "TT_RL_NK_Field");
			AssertUppercaseValueInSetter(true, "TT_RN_NK_Field");
			AssertUppercaseValueInSetter(true, "RL_Code");
			AssertUppercaseValueInSetter(true, "RN_Code");
			AssertUppercaseValueInSetter(false, "TT_RR_NKField");
			AssertUppercaseValueInSetter(false, "TT__RL_NKField");
			AssertUppercaseValueInSetter(false, "TT_RLNKField");
			AssertUppercaseValueInSetter(false, "T_RL_NKField");
			AssertUppercaseValueInSetter(false, "_RL_NKField");
			AssertUppercaseValueInSetter(false, "RL_NKField");
			AssertUppercaseValueInSetter(true, "ABC_RL_NK_Field");
			AssertUppercaseValueInSetter(true, "ABC_RN_NK_Field");
			AssertUppercaseValueInSetter(false, "ABCD_RN_NK_Field");
			AssertUppercaseValueInSetter(false, "ABCD_RN_NK_Field");
		}

		void AssertUppercaseValueInSetter(bool shouldUpperCase, string columnName)
		{
			DataTable table = CreateTableWithSystemFields();
			BusinessObjectInfo bizObjInfo = GetBizObjInfo(table);

			string columnName1 = columnName;
			bizObjInfo.Table.Columns.Add(new DataColumn(columnName1, typeof(string)));

			AutoProperty property = new AutoProperty(bizObjInfo, table.Columns[columnName]);
			AssertEquals(shouldUpperCase, property.Code.Contains("value = value.ToUpperInvariant();"));
		}

		#endregion

		public void TestIndexInfoForRefDbTable_Single()
		{
			var bizObjInfo = GetRefDbTableBizObjInfo(RefDbTypeEnum.Single, null);
			var pkColumn = new DataColumn(bizObjInfo.PKColumnName, typeof(Guid));
			var autoPty = new AutoProperty(bizObjInfo, pkColumn);
			AssertNullOrEmpty("PK Index Name", autoPty.Index);
		}

		public void TestIsComputed()
		{
			const string columnName1 = "TT_IntField";
			const string columnName2 = "TT_StringField";
			const string systemField = "TT_SystemField";

			var table = CreateTableWithSystemFields();
			var info = GetBizObjInfo(table);

			var column1 = new DataColumn(columnName1, typeof(int));
			column1.ExtendedProperties.Add("IsComputed", "Y");
			info.Table.Columns.Add(column1);

			var column2 = new DataColumn(columnName2, typeof(string));
			column2.ExtendedProperties.Add("IsComputed", "y");
			info.Table.Columns.Add(column2);

			var property1 = new AutoProperty(info, table.Columns[columnName1]);
			var property2 = new AutoProperty(info, table.Columns[columnName2]);
			var property3 = new AutoProperty(info, table.Columns[systemField]);

			Assert("Extended Property IsComputed is true", property1.IsComputed);
			Assert("Extended Property IsComputed is true", property2.IsComputed);
			Assert("Extended Property IsSparse doesn't exist", !property3.IsComputed);
		}

		public void TestSmartParameterizationOverrides()
		{
			const string columnName1 = "TT_IntField";
			const string columnName2 = "WI_OP";

			var table = CreateTableWithSystemFields();
			var info = GetBizObjInfo(table);

			var column1 = new DataColumn(columnName1, typeof(int));
			info.Table.Columns.Add(column1);

			var column2 = new DataColumn(columnName2, typeof(Guid));
			info.Table.Columns.Add(column2);

			var property1 = new AutoProperty(info, table.Columns[columnName1]);
			AssertNull("Should not have smart parameterization overrides.", property1.SmartParameterizationTableOverride);
			AssertNull("Should not have smart parameterization overrides.", property1.SmartParameterizationColumnOverride);

			var property2 = new AutoProperty(info, table.Columns[columnName2]);
			AssertEquals("Should have smart parameterization overrides.", "WhsDocketLine", property2.SmartParameterizationTableOverride);
			AssertEquals("Should have smart parameterization overrides.", "WE_OP", property2.SmartParameterizationColumnOverride);
		}

		public void TestGenerateValidation()
		{
			const string computedColumnName = "TT_RandomComputedColumn";
			const string systemCreateName = "TT_SystemCreate";
			const string systemLastEditName = "TT_SystemLastEdit";
			const string randomColumnName = "TT_RandomColumn";

			var table = CreateTableWithSystemFields();
			var info = GetBizObjInfo(table);

			var computedColumn = new DataColumn(computedColumnName, typeof(int));
			computedColumn.ExtendedProperties.Add("IsComputed", "Y");
			info.Table.Columns.Add(computedColumn);
			info.Table.Columns.Add(new DataColumn(systemCreateName, typeof(int)));
			info.Table.Columns.Add(new DataColumn(systemLastEditName, typeof(int)));
			info.Table.Columns.Add(new DataColumn(randomColumnName, typeof(int)));

			var computedColumnProperty = new AutoProperty(info, table.Columns[computedColumnName]);
			var systemCreateProperty = new AutoProperty(info, table.Columns[systemCreateName]);
			var systemLastEditProperty = new AutoProperty(info, table.Columns[systemLastEditName]);
			var nonComputedColumnProperty = new AutoProperty(info, table.Columns[randomColumnName]);

			Assert("Should not generate validation for computed columns", !computedColumnProperty.GenerateValidation);
			Assert("Should not generate validation for SystemCreate column", !systemCreateProperty.GenerateValidation);
			Assert("Should not generate validation for SystemLastEdit column", !systemLastEditProperty.GenerateValidation);
			Assert("Should generate validation", nonComputedColumnProperty.GenerateValidation);
		}

		public void TestCodeRepresentationOfDataTypeDefault()
		{
			var table = CreateTableWithSystemFields();
			var info = GetBizObjInfo(table);
			info.DbTypes["Col_System.Boolean_True_True_False"] = "bit";
			info.DbTypes["Col_System.Boolean_True_False_False"] = "bit";
			info.DbTypes["Col_System.Boolean_False_True_False"] = "bit";
			info.DbTypes["Col_System.Boolean_False_False_False"] = "bit";
			info.DbTypes["Col_System.Boolean_True_True_True"] = "bit";
			info.DbTypes["Col_System.Boolean_True_False_True"] = "bit";
			info.DbTypes["Col_System.Boolean_False_True_True"] = "bit";
			info.DbTypes["Col_System.Boolean_False_False_True"] = "bit";

			var typeTestList = GetTypeTestList();

			CombineAssertions(() =>
			{
				foreach (var typeTest in typeTestList)
				{
					var colName = $"Col_{typeTest.dataType}_{typeTest.isSparse}_{typeTest.isComputed}_{typeTest.isRequired}";
					var column = new DataColumn(colName, typeTest.dataType);

					if (typeTest.isSparse)
					{
						column.ExtendedProperties.Add("IsSparse", "Y");
					}

					if (typeTest.isComputed)
					{
						column.ExtendedProperties.Add("IsComputed", "Y");
					}

					column.AllowDBNull = !typeTest.isRequired;

					info.Table.Columns.Add(column);

					var autoProperty = new MockAutoProperty(info, column);

					if (typeTest.throwsExp)
					{
						AssertExceptionThrown<ArgumentException>(string.Join(", ", typeTest), () =>
						{
							_ = autoProperty.CodeRepresentationOfDataTypeDefault_Exposed;
						});
					}
					else
					{
						AssertEquals(string.Join(", ", typeTest), typeTest.expectedResult, autoProperty.CodeRepresentationOfDataTypeDefault_Exposed);
					}
				}
			});
		}

		static List<(Type dataType, bool isSparse, bool isComputed, bool isRequired, bool throwsExp, string expectedResult)> GetTypeTestList()
		{
			var typeTestList = new List<(Type dataType, bool isSparse, bool isComputed, bool isRequired, bool throwsExp, string expectedResult)>
			{
				(typeof(SQLComparisonOperator), true, true, false, false, "SQLComparisonOperator.NotSpecified"),
				(typeof(SQLComparisonOperator), true, false, false, false, "SQLComparisonOperator.NotSpecified"),
				(typeof(SQLComparisonOperator), false, true, false, false, "SQLComparisonOperator.NotSpecified"),
				(typeof(SQLComparisonOperator), false, false, false, false, "SQLComparisonOperator.NotSpecified"),
				(typeof(SQLComparisonOperator), true, true, true, false, "SQLComparisonOperator.NotSpecified"),
				(typeof(SQLComparisonOperator), true, false, true, false, "SQLComparisonOperator.NotSpecified"),
				(typeof(SQLComparisonOperator), false, true, true, false, "SQLComparisonOperator.NotSpecified"),
				(typeof(SQLComparisonOperator), false, false, true, false, "SQLComparisonOperator.NotSpecified"),
				(typeof(Guid), true, true, false, false, "DBNull.Value"),
				(typeof(Guid), true, false, false, false, "DBNull.Value"),
				(typeof(Guid), false, true, false, false, "DBNull.Value"),
				(typeof(Guid), false, false, false, false, "DBNull.Value"),
				(typeof(Guid), true, true, true, false, "DBNull.Value"),
				(typeof(Guid), true, false, true, false, "Guid.Empty"),
				(typeof(Guid), false, true, true, false, "DBNull.Value"),
				(typeof(Guid), false, false, true, false, "Guid.Empty"),
				(typeof(DateTime), true, true, false, false, "DBNull.Value"),
				(typeof(DateTime), true, false, false, false, "DBNull.Value"),
				(typeof(DateTime), false, true, false, false, "DBNull.Value"),
				(typeof(DateTime), false, false, false, false, "DBNull.Value"),
				(typeof(DateTime), true, true, true, false, "DBNull.Value"),
				(typeof(DateTime), true, false, true, false, "DBNull.Value"),
				(typeof(DateTime), false, true, true, false, "DBNull.Value"),
				(typeof(DateTime), false, false, true, false, "DBNull.Value"),
				(typeof(DateTimeOffset), true, true, false, false, "DBNull.Value"),
				(typeof(DateTimeOffset), true, false, false, false, "DBNull.Value"),
				(typeof(DateTimeOffset), false, true, false, false, "DBNull.Value"),
				(typeof(DateTimeOffset), false, false, false, false, "DBNull.Value"),
				(typeof(DateTimeOffset), true, true, true, false, "DBNull.Value"),
				(typeof(DateTimeOffset), true, false, true, false, "DBNull.Value"),
				(typeof(DateTimeOffset), false, true, true, false, "DBNull.Value"),
				(typeof(DateTimeOffset), false, false, true, false, "DBNull.Value"),
				(typeof(SqlGeography), true, true, false, false, "DBNull.Value"),
				(typeof(SqlGeography), true, false, false, false, "DBNull.Value"),
				(typeof(SqlGeography), false, true, false, false, "DBNull.Value"),
				(typeof(SqlGeography), false, false, false, false, "DBNull.Value"),
				(typeof(SqlGeography), true, true, true, false, "DBNull.Value"),
				(typeof(SqlGeography), true, false, true, false, "DBNull.Value"),
				(typeof(SqlGeography), false, true, true, false, "DBNull.Value"),
				(typeof(SqlGeography), false, false, true, false, "DBNull.Value"),
				(typeof(TimeSpan), true, true, false, false, "DBNull.Value"),
				(typeof(TimeSpan), true, false, false, false, "DBNull.Value"),
				(typeof(TimeSpan), false, true, false, false, "DBNull.Value"),
				(typeof(TimeSpan), false, false, false, false, "DBNull.Value"),
				(typeof(TimeSpan), true, true, true, false, "DBNull.Value"),
				(typeof(TimeSpan), true, false, true, false, "DBNull.Value"),
				(typeof(TimeSpan), false, true, true, false, "DBNull.Value"),
				(typeof(TimeSpan), false, false, true, false, "DBNull.Value"),
				(typeof(string), true, true, false, false, "DBNull.Value"),
				(typeof(string), true, false, false, false, "DBNull.Value"),
				(typeof(string), false, true, false, false, "DBNull.Value"),
				(typeof(string), false, false, false, false, "\"\""),
				(typeof(string), true, true, true, false, "DBNull.Value"),
				(typeof(string), true, false, true, false, "\"\""),
				(typeof(string), false, true, true, false, "DBNull.Value"),
				(typeof(string), false, false, true, false, "\"\""),
				(typeof(int), true, true, false, false, "DBNull.Value"),
				(typeof(int), true, false, false, false, "DBNull.Value"),
				(typeof(int), false, true, false, false, "DBNull.Value"),
				(typeof(int), false, false, false, false, "0"),
				(typeof(int), true, true, true, false, "DBNull.Value"),
				(typeof(int), true, false, true, false, "0"),
				(typeof(int), false, true, true, false, "DBNull.Value"),
				(typeof(int), false, false, true, false, "0"),
				(typeof(byte[]), true, true, false, false, "DBNull.Value"),
				(typeof(byte[]), true, false, false, false, "DBNull.Value"),
				(typeof(byte[]), false, true, false, false, "DBNull.Value"),
				(typeof(byte[]), false, false, false, false, "DBNull.Value"),
				(typeof(byte[]), true, true, true, false, "DBNull.Value"),
				(typeof(byte[]), true, false, true, false, "new byte[0]"),
				(typeof(byte[]), false, true, true, false, "DBNull.Value"),
				(typeof(byte[]), false, false, true, false, "new byte[0]"),
				(typeof(double), true, true, false, false, "DBNull.Value"),
				(typeof(double), true, false, false, false, "DBNull.Value"),
				(typeof(double), false, true, false, false, "DBNull.Value"),
				(typeof(double), false, false, false, false, "0"),
				(typeof(double), true, true, true, false, "DBNull.Value"),
				(typeof(double), true, false, true, false, "0"),
				(typeof(double), false, true, true, false, "DBNull.Value"),
				(typeof(double), false, false, true, false, "0"),
				(typeof(float), true, true, false, false, "DBNull.Value"),
				(typeof(float), true, false, false, false, "DBNull.Value"),
				(typeof(float), false, true, false, false, "DBNull.Value"),
				(typeof(float), false, false, false, false, "0"),
				(typeof(float), true, true, true, false, "DBNull.Value"),
				(typeof(float), true, false, true, false, "0"),
				(typeof(float), false, true, true, false, "DBNull.Value"),
				(typeof(float), false, false, true, false, "0"),
				(typeof(decimal), true, true, false, false, "DBNull.Value"),
				(typeof(decimal), true, false, false, false, "DBNull.Value"),
				(typeof(decimal), false, true, false, false, "DBNull.Value"),
				(typeof(decimal), false, false, false, false, "0"),
				(typeof(decimal), true, true, true, false, "DBNull.Value"),
				(typeof(decimal), true, false, true, false, "0"),
				(typeof(decimal), false, true, true, false, "DBNull.Value"),
				(typeof(decimal), false, false, true, false, "0"),
				(typeof(long), true, true, false, false, "DBNull.Value"),
				(typeof(long), true, false, false, false, "DBNull.Value"),
				(typeof(long), false, true, false, false, "DBNull.Value"),
				(typeof(long), false, false, false, false, "0"),
				(typeof(long), true, true, true, false, "DBNull.Value"),
				(typeof(long), true, false, true, false, "0"),
				(typeof(long), false, true, true, false, "DBNull.Value"),
				(typeof(long), false, false, true, false, "0"),
				(typeof(short), true, true, false, false, "DBNull.Value"),
				(typeof(short), true, false, false, false, "DBNull.Value"),
				(typeof(short), false, true, false, false, "DBNull.Value"),
				(typeof(short), false, false, false, false, "0"),
				(typeof(short), true, true, true, false, "DBNull.Value"),
				(typeof(short), true, false, true, false, "0"),
				(typeof(short), false, true, true, false, "DBNull.Value"),
				(typeof(short), false, false, true, false, "0"),
				(typeof(byte), true, true, false, false, "DBNull.Value"),
				(typeof(byte), true, false, false, false, "DBNull.Value"),
				(typeof(byte), false, true, false, false, "DBNull.Value"),
				(typeof(byte), false, false, false, false, "0"),
				(typeof(byte), true, true, true, false, "DBNull.Value"),
				(typeof(byte), true, false, true, false, "0"),
				(typeof(byte), false, true, true, false, "DBNull.Value"),
				(typeof(byte), false, false, true, false, "0"),
				(typeof(bool), true, true, false, false, "DBNull.Value"),
				(typeof(bool), true, false, false, false, "DBNull.Value"),
				(typeof(bool), false, true, false, false, "DBNull.Value"),
				(typeof(bool), false, false, false, false, "false"),
				(typeof(bool), true, true, true, false, "DBNull.Value"),
				(typeof(bool), true, false, true, false, "false"),
				(typeof(bool), false, true, true, false, "DBNull.Value"),
				(typeof(bool), false, false, true, false, "false"),
				(typeof(DataTable), true, true, false, false, "DBNull.Value"),
				(typeof(DataTable), true, false, false, false, "DBNull.Value"),
				(typeof(DataTable), false, true, false, false, "DBNull.Value"),
				(typeof(DataTable), false, false, false, true, string.Empty),
				(typeof(DataTable), true, true, true, false, "DBNull.Value"),
				(typeof(DataTable), true, false, true, true, string.Empty),
				(typeof(DataTable), false, true, true, false, "DBNull.Value"),
				(typeof(DataTable), false, false, true, true, string.Empty),
			};
			return typeTestList;
		}

		#region Implementation

		static DataTable CreateTableWithSystemFields()
		{
			DataTable result = new DataTable("test");

			result.Columns.Add(new DataColumn("TT_NonSystemField", typeof(DateTime)));
			result.Columns.Add(new DataColumn("TT_SystemField", typeof(DateTime)));

			return result;
		}

		public static BusinessObjectInfo GetBizObjInfo(DataTable table)
		{
			return GetBizObjInfo(table, literalOnlyColumns: Array.Empty<string>());
		}

		internal static BusinessObjectInfo GetBizObjInfo(DataTable table, bool masterFileReference = true, params string[] literalOnlyColumns)
		{
			return new BusinessObjectInfo(
				fileName: "DummyFileName"
				, isInZArchitectureSolution: false
				, isPersistent: true
				, @namespace: "Enterprise"
				, namespaceOfSchema: "Enterprise"
				, baseClassName: "BusinessObject"
				, sqlSchemaName: Db.SqlDbOwnerSchema
				, table: table
				, decimalScaleTable: new DataTable()
				, foreignKeysTable: new DataTable()
				, dateTimeOffsetTable: new DataTable()
				, refDbCountry: ""
				, refDbType: null
				, dbTypes: new Dictionary<string, string>()
				, uniqueKeys: new HashSet<string>()
				, canForceUpdateNaturalKeyCacheColumns: new HashSet<string>()
				, masterFiles: new BuildXmlBizOEntryCollection()
				, pkIndex: null
				, indexes: Array.Empty<string>()
				, masterFileReference: masterFileReference
				, preventDelete: false
				, literalOnlyColumns: literalOnlyColumns
				, nonBlankFilteredIndexColumns: Array.Empty<string>()
				, tables: new Dictionary<string, ITableInfo>());
		}

		public static BusinessObjectInfo GetRefDbTableBizObjInfo(RefDbTypeEnum refDbType, string refCountryCode)
		{
			var refDbName = refDbType == RefDbTypeEnum.Single ? RefDbTableNameResolver.SingleRefDatabaseName : RefDbTableNameResolver.GetExclusiveRefDbName(Db.DatabaseName, refDbType, refCountryCode);

			var sqlText = $@"
				SELECT TOP (1)
					TableName = tab.name,
					PkConstraintName = pk.name,
					PkColumnName = col.name
				FROM
					[{refDbName}].sys.key_constraints pk
					INNER JOIN [{refDbName}].sys.tables tab ON tab.object_id = pk.parent_object_id
					INNER JOIN [{refDbName}].sys.columns col ON col.object_id = tab.object_id
				WHERE
					pk.type = 'PK'
					AND col.name LIKE '___[_]PK'
				";

			string tableName = null;
			string pkColumnName = null;
			string pkIndexName = null;

			Db.Connection.ExecuteReader(sqlText, (reader) =>
			{
				tableName = (string)reader["TableName"];
				pkIndexName = (string)reader["PkConstraintName"];
				pkColumnName = (string)reader["PkColumnName"];
			});

			AssertNotNull("Random Ref DB Table Name", tableName);

			var pkColumn = new DataColumn(pkColumnName, typeof(Guid));
			var testDataTable = new DataTable(tableName);
			testDataTable.Columns.Add(pkColumn);
			testDataTable.PrimaryKey = new DataColumn[] { pkColumn };

			return new BusinessObjectInfo(
				fileName: "TestRefDbSchemaIndexMetadata"
				, isInZArchitectureSolution: false
				, isPersistent: true
				, @namespace: "Enterprise"
				, namespaceOfSchema: "Enterprise"
				, baseClassName: "BusinessObject"
				, sqlSchemaName: Db.SqlDbOwnerSchema
				, table: testDataTable
				, decimalScaleTable: new DataTable()
				, foreignKeysTable: new DataTable()
				, dateTimeOffsetTable: new DataTable()
				, refDbCountry: refCountryCode
				, refDbType: refDbType
				, dbTypes: new Dictionary<string, string>() { { pkColumnName, "uniqueidentifier" } }
				, uniqueKeys: new HashSet<string> { { pkColumnName } }
				, canForceUpdateNaturalKeyCacheColumns: new HashSet<string>()
				, masterFiles: new BuildXmlBizOEntryCollection()
				, pkIndex: pkIndexName
				, indexes: Array.Empty<string>()
				, masterFileReference: false
				, preventDelete: false
				, literalOnlyColumns: Array.Empty<string>()
				, nonBlankFilteredIndexColumns: Array.Empty<string>()
				, tables: new Dictionary<string, ITableInfo>());
		}

		class MockAutoProperty : AutoProperty
		{
			public MockAutoProperty(BusinessObjectInfo info, DataColumn column) : base(info, column) { }

			protected override string CodeForSchemaColumnPropertyCall
			{
				get { return ""; }
			}

			public string CodeForZPropertyInfoAccessor
			{
				get { return CodeForZPropertyInfo; }
			}

			public string CodeRepresentationOfDataTypeDefault_Exposed => CodeRepresentationOfDataTypeDefault;
		}

		class StringLineAssembler
		{
			readonly int startIndent;
			readonly List<StringLine> lines = new List<StringLine>();

			public StringLineAssembler(int startIndent)
			{
				this.startIndent = startIndent;
			}

			public StringLineAssembler Add(string line)
			{
				return Add(line, 0);
			}

			public StringLineAssembler Add(string line, int indent)
			{
				lines.Add(new StringLine(line, indent + startIndent));
				return this;
			}

			public override string ToString()
			{
				StringBuilder builder = new StringBuilder();

				for (int i = 0; i < lines.Count; i++)
				{
					builder.Append(lines[i]);

					if (i != lines.Count - 1)
					{
						builder.Append(System.Environment.NewLine);
					}
				}

				return builder.ToString();
			}

			class StringLine
			{
				public static char IndentCharacter = char.Parse("\t");

				public string Text = "";
				public int Indent;

				public StringLine(string text, int indent)
				{
					Text = text;
					Indent = indent;
				}

				public override string ToString()
				{
					return Text.PadLeft(Text.Length + Indent, IndentCharacter);
				}
			}
		}

		#endregion
	}
}

namespace Enterprise.ZArchitecture.Business
{
	class SQLComparisonOperator
	{ }
}
