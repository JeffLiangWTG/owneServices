using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.BuildTools;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Microsoft.SqlServer.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.BusinessObjectGenerator
{
	public interface ITableInfo
	{
		string PkIndex { get; }

		string[] Indexes { get; }

		string[] LiteralOnlyColumns { get; }

		IEnumerable<string> NonBlankFilteredIndexColumns { get; }
	}

	public class AutoProperty : AutoCode, IComparable
	{
		#region Constructors

		public AutoProperty(BusinessObjectInfo info, DataColumn column, int maxColumnLength)
			: this(info, column)
		{
			fColumnNameWhiteSpace = "".PadLeft(maxColumnLength - column.ColumnName.Length, ' ');
		}

		public AutoProperty(BusinessObjectInfo info, DataColumn column, int maxColumnLength, int decimalPrecision, int decimalScale)
			: this(info, column, maxColumnLength)
		{
			this.DecimalPrecision = decimalPrecision;
			this.DecimalScale = decimalScale;
		}

		public AutoProperty(BusinessObjectInfo info, DataColumn column, int maxColumnLength, int dateTimeOffsetScale)
			: this(info, column, maxColumnLength)
		{
			this.DateTimeOffsetScale = dateTimeOffsetScale;
		}

		[ThreadSafe]
		static readonly IEnumerable<string> charColumns = new[]
		{
			"Z0_IsValid", "Z0_Bool", "ZL2_IsValid", "S7_EnterpriseActivity", "SL_IsEstimate", "SL_IsCancelled", "SC_IsDeleted", "SC_IsPublished", "SC_IsSystemGenerated", "SC_Language", "SC_SaveVersions",
			"AH_AHECCSnapshotAssayRequiredIndicator", "QC_AQISCommodityImportedFoodsIndicator", "CK_AQISProducerRequiredIndicator", "CK_PermitApplicationIndicator", "EC_EstablishmentAQISPremisesIndicator",
			"CQ_LodgementQuestionIndicator", "PQ_PermitRequirementMandatoryIndicator", "UC_CategoryType", "UE_PreferenceCode", "UI_InstrumentStatus", "ZA_ExchangeDateDeterminationFlag",
			"ZA_InactiveInd", "ZA_PermitInd", "ZA_QuotaInd", "FR_IsDefaultRefNum", "CE_IsConveyanceIDRequired", "ZB_FreeInd", "ZB_Inactive", "ZC_FreeInd", "ZC_Inactive", "ZF_FreeInd",
			"ZF_GST0RateInd", "ZF_Inactive", "ZH_CheckInd", "ZH_Inactive", "ZD_Inactive", "CT_IsConveyanceIDRequired", "U0_GSTExempt", "U0_IsManual", "U0_NoPrefRate", "Q4_IsCountry",
			"Q4_IsFreeUnlessOtherwiseIndicated", "U6_AppliesToExport", "U6_AppliesToImport", "UA_IsSystem", "UD_IsSystem", "US_IsSystem", "UK_IsExportControl", "UK_IsImportControl",
			"UK_IsSystem", "UK_IsTranshipmentControl", "UT_IsSystem", "UL_QualifierIndicator", "UW_RateIndicator", "UC_DrawbackEligibility", "UC_GSPIndicator", "UC_LesserDevelopedCountry",
			"UC_RestrictionIndicator", "US_IsActive", "IE_IsSystemGenerated", "UT_IsHeld", "UR_PortOfUnlading", "UE_AdditionalTariffNumberIndicator", "UE_AntiDumping", "UE_CountervailingDutyFlag",
			"UE_IsBaseRate", "UE_QuotaIndicator", "UO_IsELVIS", "UO_IsExceptionToVisaReq", "UO_IsPartCategory", "UO_IsSpecialProgram", "UO_IsStardardVisaFormat", "UO_IsVisaExemptForForklore",
			"UO_IsVisaExemptForSample", "UO_IsVisaQuantityIndicated"
		};

		internal static (string SourceTable, string SourceColumn) GetBackingColumn(string columnName)
		{
			if (BuildXml.Instance.BackingColumns.TryGetValue(columnName, out var result))
			{
				if (!string.IsNullOrWhiteSpace(result.SourceTable) && !string.IsNullOrWhiteSpace(result.SourceColumn))
				{
					return result;
				}
			}

			return default;
		}

		internal static string IndexForColumn(string tableName, string columnName, RefDbTypeEnum? type)
		{
			if (type == RefDbTypeEnum.Single && RefDatabaseVersionMapHelper.TableViewMappings.TryGetValue(tableName, out var viewName))
			{
				tableName = viewName;
			}

			if (indexForColumns == null)
			{
				var cache = new Dictionary<string, string>();

				var databases = new List<string>();
				databases.AddRange(Db.Connection.GetDatabases(DatabaseType.ExclusiveRefOrSharedRef));
				databases.Add(Db.Connection.CurrentDatabase);
				foreach (string database in databases)
				{
					using (((ICurrentDbControl)Db.Connection).UseDatabase(database))
					{
						var sql = @"
SELECT 
ColumnName = col.name,
IndexName = ind.name,
TableName = tb.name
FROM
sys.indexes ind
INNER JOIN
sys.index_columns ic ON ind.object_id = ic.object_id and ind.index_id = ic.index_id
INNER JOIN
sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id
INNER JOIN
(
SELECT name, object_id
FROM sys.tables
UNION
SELECT name, object_id
FROM sys.views
) tb on tb.object_id = col.object_id
where key_ordinal = 1
and (filter_definition is null or filter_definition = '([' + col.name + '] IS NOT NULL)')";
						using (var cmd = Db.Connection.Command(sql))
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								var column = reader.GetString(0);
								var index = reader.GetString(1);
								var table = reader.GetString(2);
								//blacklist some columns that DAT regen tries to set to true that definitely should not be (have no index locally)
								if (column != "U5_U2_Concession" && column != "U3_U2_Concession" && column != "L0_U0_Classification" && column != "U1_U0_Classification" && column != "IA_IE")
								{
									cache[column + ";;" + table] = index;
								}
							}
						}
					}
				}

				indexForColumns = cache;
			}
			return indexForColumns.TryGetValue(columnName + ";;" + tableName, out var result) ? result : null;
		}
		[ThreadSafe]
		static Dictionary<string, string> indexForColumns;

		public AutoProperty(BusinessObjectInfo info, DataColumn column)
		{
			this.Info = info;
			fColumnName = column.ColumnName;
			fDataType = column.DataType;
			IsRequiredField = !column.AllowDBNull;
			AdoDefaultValue = column.DefaultValue;
			//quick hack fix - DbGeography null would be DbNull but SqlGeography has its own special Null
			if (column.AllowDBNull && AdoDefaultValue is SqlGeography geo && geo.IsNull)
			{
				AdoDefaultValue = DBNull.Value;
			}
			ColumnLength = column.MaxLength;
			Index = IndexForColumn(info.Table.TableName, column.ColumnName, info.RefDbType);

			if (ColumnLength == 1 && info.DbTypes[column.ColumnName] == "char" && charColumns.Contains(column.ColumnName))
			{
				IsBoolProperty = true;
			}
			else
			{
				IsBoolProperty = column.DataType == typeof(bool);
			}
			if (IsBoolProperty && info.DbTypes[column.ColumnName] != "bit")
			{
				string defaultString = AdoDefaultValue.ToString().ToUpper();
				AdoDefaultValue = defaultString == "Y" ? "true" : "false";
			}

			IsNAddInfoField = column.DataType == typeof(string)
				&& column.ExtendedProperties.ContainsKey("IsNAddInfoField")
				&& column.ExtendedProperties["IsNAddInfoField"].ToString().ToUpper() == "Y";

			IsSparse = column.ExtendedProperties.ContainsKey("IsSparse")
				&& column.ExtendedProperties["IsSparse"].ToString().ToUpper() == "Y";

			IsSensitive = column.ExtendedProperties.ContainsKey("IsSensitive")
				&& column.ExtendedProperties["IsSensitive"].ToString().ToUpper() == "Y";

			IsComputed = column.ExtendedProperties.ContainsKey(nameof(IsComputed))
				&& column.ExtendedProperties[nameof(IsComputed)].ToString().ToUpper() == "Y";

			string dbType;
			IsBinary = info.DbTypes.TryGetValue(column.ColumnName, out dbType) && (dbType == "varbinary" || dbType == "binary");
		}

		#endregion

		public string ColumnName
		{
			get { return fColumnName; }
		}

		public string ColumnNameWhiteSpace
		{
			get { return fColumnNameWhiteSpace; }
		}

		public string TableNameColumnName
		{
			get { return (Info.Table.TableName + "." + ColumnName).ToUpperInvariant(); }
		}

		public int MaxLength
		{
			get { return this.ColumnLength; }
		}

		public Type DataType
		{
			get { return fDataType; }
		}

		public SqlDbType SqlDbType
		{
			get { return DbTypes.Instance[Info.DbTypes[this.ColumnName]]; }
		}

		public bool HasSqlDbType
		{
			get { return Info.DbTypes.ContainsKey(ColumnName); }
		}

		public string GetColumnTypeFromDataType()
		{
			string result;

			switch (DataType.Name)
			{
				case "Guid":
					result = "Guid";
					break;
				case "DateTime":
					result = (SqlDbType == SqlDbType.Date) ? "Date" : "DateTime";
					break;
				case "DateTimeOffset":
					result = "DateTimeOffset";
					break;
				case "Decimal":
					result = "Decimal";
					break;
				case "Int64":
					result = "Long";
					break;
				case "Int32":
					result = "Int";
					break;
				case "Int16":
					result = "Short";
					break;
				case "Byte":
					result = "Byte";
					break;
				case "Byte[]":
					result = IsBinary ? "Binary" : "Blob";
					break;
				case "Boolean":
					result = "Bool";
					break;
				case "String":
					result = SqlDbType == SqlDbType.Xml ? "Xml" : IsBoolProperty ? "Bool" : "String";
					break;
				case "SqlGeography":
					result = "Geography";
					break;
				case "TimeSpan":
					result = "Time";
					break;

				default:
					throw new ArgumentException("The object type '" + DataType.Name + "' is not supported by the Generator.");
			}

			return result;
		}

		public string SchemaColumnType
		{
			get { return "Schema" + GetColumnTypeFromDataType() + "Column"; }
		}

		public bool IsNaturalKeyString =>
			ReturnType == "ZString"
			&& (Info?.UniqueKeys.Contains(TableNameColumnName) ?? false);

		public bool CanForceUpdateNaturalKeyCache =>
			ReturnType == "ZString"
			&& (Info?.CanForceUpdateNaturalKeyCacheColumns.Contains(TableNameColumnName) ?? false);

		string ShouldUpdateNKCacheWhenColumnChangesMethodName => $"ShouldUpdateNaturalKeyCacheWhen{ColumnName}Changes";

		/// <summary>
		/// All fields in the DB are now required fields, so a mandatory field in the database is designated as:
		///  - The field is required
		///  - The field has a default value
		/// </summary>
		public bool RequiresMandatoryValidation
		{
			get { return IsRequiredField && AdoDefaultValue == DBNull.Value; }
		}

		public bool RequiresTypeValidation
		{
			get
			{
				return
					DataType == typeof(DateTime) ||
					DataType == typeof(DateTimeOffset) ||
					DataType == typeof(Guid) ||
					DataType == typeof(byte[]) ||
					DataType == typeof(decimal) ||
					DataType == typeof(SqlGeography);
			}
		}

		public virtual bool GenerateValidation
		{
			get
			{
				return !ColumnName.Contains("_SystemCreate") && !ColumnName.Contains("_SystemLastEdit") && !IsComputed;
			}
		}

		public virtual bool RequiresEnglishCharactersValidation
		{
			get { return HasSqlDbType && SqlDbType == SqlDbType.VarChar; }
		}

		public virtual bool IsGeographyProperty
		{
			get
			{
				return
					DataType == typeof(SqlGeography);
			}
		}

		internal bool RequiresConcurrencyCheck
		{
			get { return !ColumnName.Contains("_System"); }
		}

		protected virtual string CodeForSchemaColumnPropertyCall
		{
			get { return Info.ClassNames.Schema + ".Constants." + ColumnName; }
		}

		public bool IsSparse { get; }

		public bool IsSensitive { get; }

		public bool IsLiteralOnly
		{
			get
			{
				var result = Info.LiteralOnlyColumns.Contains(ColumnName);
				if (result)
				{
					return result;
				}
				var backingColumn = GetBackingColumn(ColumnName);
				if (backingColumn.SourceTable != null)
				{
					Info.Tables.TryGetValue(backingColumn.SourceTable.ToUpperInvariant(), out var table);
					return table?.LiteralOnlyColumns.Contains(backingColumn.SourceColumn) ?? false;
				}
				return result;
			}
		}
		public bool IsNonBlankFilteredIndexParticipant
		{
			get
			{
				var result = Info.NonBlankFilteredIndexColumns.Contains(ColumnName);
				if (result)
				{
					return result;
				}
				var backingColumn = GetBackingColumn(ColumnName);
				if (backingColumn.SourceTable != null)
				{
					Info.Tables.TryGetValue(backingColumn.SourceTable.ToUpperInvariant(), out var table);
					return table?.NonBlankFilteredIndexColumns.Contains(backingColumn.SourceColumn) ?? false;
				}
				return result;
			}
		}

		public string SmartParameterizationTableOverride => GetBackingColumnForSmartParameterization(ColumnName).SourceTable;
		public string SmartParameterizationColumnOverride => GetBackingColumnForSmartParameterization(ColumnName).SourceColumn;

		(string SourceTable, string SourceColumn) GetBackingColumnForSmartParameterization(string columnName)
		{
			var backingColumn = GetBackingColumn(columnName);
			return !string.Equals(backingColumn.SourceTable, Info.Table.TableName, StringComparison.InvariantCultureIgnoreCase) ? backingColumn : default;
		}

		public bool IsComputed { get; }

		#region Code for Property

		public virtual string Code
		{
			get
			{
				return LinesOfCode(
					"",
					"		#region " + ColumnName,
					"",
							CodeForProperty,
					"",
							CodeForZPropertyInfo,
							CodeForStreamProperties,
					"		#endregion"
					);
			}
		}

		#endregion

		#region Code for Default Value

		public virtual string CodeForDefaultValue
		{
			get
			{
				if (AdoDefaultValue is SqlGeography)
				{
					var text = AdoDefaultValue.ToString().ToUpper(System.Globalization.CultureInfo.InvariantCulture);
					return "			row[" + CodeForSchemaColumnPropertyCall + ColumnNameWhiteSpace + "] = " + string.Format(System.Globalization.CultureInfo.InvariantCulture, "GetGeographyColumnValue(row, {0}, \"{1}\");", CodeForSchemaColumnPropertyCall, text != "NULL" ? text : "POINT EMPTY");
				}
				else
				{
					return "			row[" + CodeForSchemaColumnPropertyCall + ColumnNameWhiteSpace + "] = " + CodeForPropertyDefault + ";";
				}
			}
		}

		public string CodeForPropertyDefault
		{
			get
			{
				string result = null;

				if (AdoDefaultValue == DBNull.Value) // no db default
				{
					result = CodeRepresentationOfDataTypeDefault;
				}
				else
				{
					result = CodeRepresentationOfDbDefault;
				}

				return result;
			}
		}

		protected string CodeRepresentationOfDataTypeDefault
		{
			get
			{
				string result = null;

				if (DataType.ToString() == @"Enterprise.ZArchitecture.Business.SQLComparisonOperator")
				{
					result = "SQLComparisonOperator.NotSpecified";
				}
				else if (IsSparse && !IsRequiredField)
				{
					result = "DBNull.Value";
				}
				else if (IsComputed)
				{
					result = "DBNull.Value";
				}
				else if (DataType == typeof(Guid))
				{
					result = IsRequiredField ? "Guid.Empty" : "DBNull.Value";
				}
				else if (DataType == typeof(DateTime) || DataType == typeof(DateTimeOffset) || DataType == typeof(SqlGeography) || DataType == typeof(TimeSpan))
				{
					result = "DBNull.Value";
				}
				else if (DataType == typeof(string))
				{
					result = !IsRequiredField && IsSingleRefDatabaseNaturalKey(ColumnName) ? "DBNull.Value" : "\"\"";
				}
				else if (DataType == typeof(byte[]))
				{
					result = IsRequiredField ? "new byte[0]" : "DBNull.Value";
				}
				else if (IsNumericType(DataType))
				{
					result = "0";
				}
				else if (DataType == typeof(bool))
				{
					result = "false";
				}
				else
				{
					ThrowNotSupportedTypeException();
				}

				return result;
			}
		}

		bool IsSingleRefDatabaseNaturalKey(string columnName)
		{
			var result = false;

			if (Info.RefDbType == RefDbTypeEnum.Single && columnName.IndexOf("_NK") != -1)
			{
				var tablePrefix = columnName.Split('_')[1];
				var sqlText = string.Format(System.Globalization.CultureInfo.InvariantCulture, @"
FROM [{0}].sys.tables tab
INNER JOIN [{0}].sys.columns col ON tab.object_id = col.object_id
INNER JOIN [{0}].sys.schemas s ON tab.schema_id = s.schema_id
WHERE col.name LIKE '{1}[_]%'
AND s.name NOT IN ('{2}')", RefDbTableNameResolver.DefaultSingleRefDbName, tablePrefix, string.Join("', '", Db.SqlReservedSchemas));

				result = Db.Connection.Exists(sqlText);
			}

			return result;
		}

		protected string CodeRepresentationOfDbDefault
		{
			get
			{
				string result = null;

				if (AdoDefaultValue is DateTime)
				{
					result = "new DateTime(" + ((DateTime)AdoDefaultValue).Ticks + ")";
				}
				else if (AdoDefaultValue is DateTimeOffset)
				{
					result = "new DateTimeOffset(" + ((DateTimeOffset)AdoDefaultValue).Ticks + "," + ((DateTimeOffset)AdoDefaultValue).Offset + ")";
				}
				if (AdoDefaultValue is TimeSpan)
				{
					result = "new TimeSpan(" + ((TimeSpan)AdoDefaultValue).Ticks + ")";
				}
				else if (AdoDefaultValue is SqlGeography)
				{
					var text = AdoDefaultValue.ToString().ToUpper(System.Globalization.CultureInfo.InvariantCulture);
					result = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Microsoft.SqlServer.Types.SqlGeography.Parse(\"{0}\")", text != "NULL" ? text : "POINT EMPTY");
				}
				else if (IsBoolProperty)
				{
					result = AdoDefaultValue.ToString().ToLower();
				}
				else if (AdoDefaultValue is string)
				{
					result = "\"" + AdoDefaultValue + "\"";
				}
				else if (IsNumericType(AdoDefaultValue.GetType()))
				{
					result = "(" + GetColumnTypeFromDataType().ToLower() + ")" + AdoDefaultValue.ToString();
				}
				else if (AdoDefaultValue is byte[] && ((byte[])AdoDefaultValue).Length == 0)
				{
					result = "new byte[0]";
				}
				else if (AdoDefaultValue.GetType().ToString() == "Enterprise.ZArchitecture.Business.SQLComparisonOperator")
				{
					result = "SQLComparisonOperator.NotSpecified";
				}
				else
				{
					ThrowNotSupportedTypeException();
				}

				return result;
			}
		}

		protected void ThrowNotSupportedTypeException()
		{
			throw new ArgumentException("The type '" + DataType + "' is not supported.");
		}

		protected bool IsNumericType(Type valueType)
		{
			return
				valueType == typeof(decimal) ||
				valueType == typeof(double) ||
				valueType == typeof(float) ||
				valueType == typeof(int) ||
				valueType == typeof(long) ||
				valueType == typeof(short) ||
				valueType == typeof(byte);
		}

		#endregion

		#region Code for Get / Set

		protected virtual string CodeForProperty
		{
			get
			{
				var code = new List<string>
				{
					IsNAddInfoField ? "		[IsNAddInfoField]" : null,
					IsSensitive     ? "		[CargoWise.ComponentModel.PasswordAttribute]" : null,
					CodeForAllowSpatialTypesAttribute,
					"		public virtual " + ReturnType + " " + ColumnName,
					"		{",
								CodeForPropertyGet,
								CodeForPropertySet,
					"		}",
				};
				if (CanForceUpdateNaturalKeyCache)
				{
					code.AddRange(new[]
					{
						"",
						$"		protected virtual bool {ShouldUpdateNKCacheWhenColumnChangesMethodName}(ZString newValue) => false;",
					});
				}
				return LinesOfCode(code.ToArray());
			}
		}

		protected virtual string CodeForPropertyGet
		{
			get
			{
				string newZType;
				bool isLong = false;

				switch (DataType.Name)
				{
					case "Guid":
						newZType = "new ZGuid(" + GetValueFromRowSafelyCode + ")";
						break;
					case "DateTime":
						{
							string kind = ColumnName.EndsWith("utc", StringComparison.InvariantCultureIgnoreCase) ? "Utc" : "Local";
							if (SqlDbType == SqlDbType.Date)
							{
								newZType = "new ZDate(" + GetValueFromRowSafelyCode + ", DateTimeKind." + kind + ")";
							}
							else
							{
								newZType = "new ZDateTime(" + GetValueFromRowSafelyCode + ", DateTimeKind." + kind + ")";
							}
							break;
						}
					case "TimeSpan":
						{
							newZType = "new ZTime(" + GetValueFromRowSafelyCode + ")";
							break;
						}
					case "DateTimeOffset":
						{
							newZType = "new ZDateTimeOffset(" + GetValueFromRowSafelyCode + ")";
							break;
						}
					case "SqlGeography":
						{
							newZType = "new ZGeography(" + GetValueFromRowSafelyCode + ")";
							break;
						}
					case "Decimal":
						newZType = "new ZDecimal(" + GetValueFromRowSafelyCode + ")";
						break;
					case "Int64":
						newZType = "new ZLong(" + GetValueFromRowSafelyCode + ")";
						break;
					case "Int32":
						newZType = "new ZInt(" + GetValueFromRowSafelyCode + ")";
						break;
					case "Int16":
						newZType = "new ZShort(" + GetValueFromRowSafelyCode + ")";
						break;
					case "Byte":
						newZType = "new ZByte(" + GetValueFromRowSafelyCode + ")";
						break;
					case "Byte[]":
						newZType = "new ZBlob(" + GetValueFromRowSafelyCode + ")";
						isLong = true;
						break;
					case "Boolean":
						newZType = "new ZBool(" + GetValueFromRowSafelyCode + ")";
						break;
					case "SQLComparisonOperator":
						newZType = "((SQLComparisonOperator)" + GetValueFromRowSafelyCode + ")";
						break;
					case "String":
						{
							if (IsBoolProperty)
							{
								newZType = "new ZBool(" + GetValueFromRowSafelyCode + ")";
							}
							else
							{
								newZType = "new ZString(" + GetValueFromRowSafelyCode + ")";
								if (MaxLength >= CargoWise.Schema.SchemaStringColumn.LONG_TEXT_LENGTH)
								{
									isLong = true;
								}
							}
							break;
						}

					default:
						throw new InvalidOperationException("The object type '" + DataType.Name + "' is not supported by the Generator.");
				}

				string result = "			get { ";
				if (isLong)
				{
					result += System.Environment.NewLine + "				EnsureBlobField(" + Info.Table + "Schema." + ColumnName + ");" + System.Environment.NewLine + "\t\t\t\treturn " + newZType + "; }" + System.Environment.NewLine;
				}
				else
				{
					result += " return " + newZType + "; }";
				}
				return result;
			}
		}

		protected virtual string GetValueFromRowSafelyCode
		{
			get { return "GetValueFromRowSafely(" + Info.Table + "Schema." + ColumnName + ")"; }
		}

		protected virtual string CodeForPropertySet
		{
			get
			{
				string checkMaximumLengthCommand = null;
				string trimValueCommand = null;
				string convertValueToWesternEuropeanCharactersCommand = null;
				string uppercaseCommand = null;
				string updateNaturalKeyCacheCommand = null;
				string assignLocalFromPropertyInfoCommand = null;

				var propertyInfoName = ColumnName + "Info";
				var hasMultipleReferencesToPropertyInfo = ReturnType == "ZString"
					|| IsNaturalKeyString || CanForceUpdateNaturalKeyCache;
				if (hasMultipleReferencesToPropertyInfo)
				{
					const string localName = "zPropertyInfo";
					assignLocalFromPropertyInfoCommand = "\t\t\t\tvar " + localName + " = " + propertyInfoName + ";";
					propertyInfoName = localName;
				}

				var setPropertyValueCommand = "\t\t\t\tSetPropertyValue(" + propertyInfoName + ", value);";

				if (ReturnType == "ZString")
				{
					checkMaximumLengthCommand = "				CheckMaximumLength(" + propertyInfoName + ", value);";
					trimValueCommand = @"				value = value.TrimEndSpaceTab();";
					if (Info.ConvertZStringToWesternEuropeanCharacters && !IsNAddInfoField)
					{
						convertValueToWesternEuropeanCharactersCommand = "				value = value.ConvertToWesternEuropeanCharacters();";
					}

					// Convert UNLOCO and Country codes to uppercase
					if (Regex.IsMatch(ColumnName, @"(^[A-Z\d]{2,3}_R[NL]_NK)|(^R[LN]_Code$)"))
					{
						uppercaseCommand = "				value = value.ToUpperInvariant();";
					}
				}

				if (IsNaturalKeyString || CanForceUpdateNaturalKeyCache)
				{
					var condition = $"Factory != null && !{propertyInfoName}.Value.Equals(value)";
					if (CanForceUpdateNaturalKeyCache)
					{
						condition += $" && {ShouldUpdateNKCacheWhenColumnChangesMethodName}(value)";
					}
					updateNaturalKeyCacheCommand = LinesOfCode(
					$"				if ({condition})",
					"				{",
					$"					Factory.UpdateNaturalKeyCache(this, " + Info.Table + "Schema." + ColumnName + ", " + propertyInfoName + ".Value, value);",
					"				}");
				}

				string codeForCallingValidation = "";

				if (GenerateValidation)
				{
					codeForCallingValidation = LinesOfCode(
					"				if (!IsValidationSuspended)",
					"				{",
					"					" + CodeForCallingValidateMethodOnValidateObject,
					"				}");
				}

				return LinesOfCode(
					"			set",
					"			{",
									trimValueCommand,
									convertValueToWesternEuropeanCharactersCommand,
									uppercaseCommand,
									assignLocalFromPropertyInfoCommand,
									checkMaximumLengthCommand,
									updateNaturalKeyCacheCommand,
									setPropertyValueCommand,
									codeForCallingValidation,
					"			}"
					);
			}
		}

		protected string ReturnType
		{
			get
			{
				string result;
				switch (DataType.Name)
				{
					case "Guid":
						result = "ZGuid";
						break;
					case "DateTime":
						result = (SqlDbType == SqlDbType.Date) ? "ZDate" : "ZDateTime";
						break;
					case "DateTimeOffset":
						result = "ZDateTimeOffset";
						break;
					case "Decimal":
						result = "ZDecimal";
						break;
					case "Int64":
						result = "ZLong";
						break;
					case "Int32":
						result = "ZInt";
						break;
					case "Int16":
						result = "ZShort";
						break;
					case "Byte":
						result = "ZByte";
						break;
					case "Byte[]":
						result = "ZBlob";
						break;
					case "Boolean":
						result = "ZBool";
						break;
					case "String":
						result = IsBoolProperty ? "ZBool" : "ZString";
						break;
					case "SqlGeography":
						result = "ZGeography";
						break;
					case "TimeSpan":
						result = "ZTime";
						break;
					case "SQLComparisonOperator":
						result = "SQLComparisonOperator";
						break;

					default:
						throw new InvalidOperationException("The object type '" + DataType.Name + "' is not supported by the Generator.");
				}
				return result;
			}
		}

		#endregion

		#region Code for Calling Validation

		public virtual string CodeForCallingValidateMethod
		{
			get { return "Validate" + ColumnName + "();"; }
		}

		public virtual string CodeForCallingValidateMethodOnValidateObject
		{
			get { return "Validation.Validate" + ColumnName + "();"; }
		}

		public virtual string CodeForCallingValidateMethodOnParentObject
		{
			get { return "Parent.Validate" + ColumnName + "();"; }
		}

		#endregion

		#region Code for ZPropertyInfo

		protected virtual string CodeForZPropertyInfo
		{
			get
			{
				return LinesOfCode(
					"		public virtual ZPropertyInfo " + ColumnName + "Info",
					"		{",
					"			[System.Diagnostics.DebuggerStepThrough()]",
					"			get { return GetZPropertyInfo(" + CodeForSchemaColumnPropertyCall + "); }",
					"		}");
			}
		}

		#endregion

		#region Code for Stream Properties

		string CodeForStreamProperties
		{
			get
			{
				if (DataType.Name == "Byte[]" || (DataType.Name == "String" && MaxLength > CargoWise.Schema.SchemaStringColumn.LONG_TEXT_LENGTH))
				{
					return LinesOfCode(
						"",
						"		public virtual " + StreamType + " Get" + ColumnName + "Reader(" + ReaderParameters + ")",
						"		{",
									ReaderStreamCode,
						"		}",
						"",
						"		public virtual void Set" + ColumnName + "Source(" + StreamSourceType + " source)",
						"		{",
						"			SetSource(source, " + Info.Table + "Schema." + ColumnName + ");",
						"		}",
						""
						);
				}
				else
				{
					return "";
				}
			}
		}

		string StreamType
		{
			get { return DataType.Name == "Byte[]" ? "System.IO.Stream" : "System.IO.TextReader"; }
		}

		string StreamSourceType
		{
			get { return DataType.Name == "Byte[]" ? "IStreamSource" : "ITextReaderSource"; }
		}

		string ReaderParameters
		{
			get
			{
				return DataType.Name == "Byte[]" ? "" : "bool closeReaderBetweenReads = false";
			}
		}

		string ReaderStreamCode
		{
			get
			{
				return DataType.Name == "Byte[]" ?
						"			return Factory.GetBinaryFieldStream(this, " + Info.Table + "Schema." + ColumnName + ");"
					: "			return Factory.GetTextFieldReader(this, " + Info.Table + "Schema." + ColumnName + ", closeReaderBetweenReads);";
			}
		}

		#endregion

		#region Code for AllowSpatialTypesAttribute

		protected string CodeForAllowSpatialTypesAttribute
		{
			get
			{
				if (IsGeographyProperty && Info.IsPersistent)
				{
					var allowedTypes = new GeographyConstraintHelper().GetAllowedSpatialTypes(Info.TableName, ColumnName);
					var allowedTypesCode = from type in allowedTypes
										   select "ZGeography.SpatialType." + type;
					if (allowedTypesCode.ToList().Count > 0)
					{
						return "		[AllowSpatialTypes(" + string.Join(", ", allowedTypesCode) + ")]";
					}
				}

				return null;
			}
		}

		#endregion

		#region Implementation

		public readonly int DecimalPrecision;
		public readonly int DecimalScale;
		public readonly int DateTimeOffsetScale;
		public readonly bool IsBoolProperty;
		public readonly bool IsBinary;
		public readonly bool IsRequiredField;
		public readonly bool IsNAddInfoField;
		public readonly string Index;
		protected readonly BusinessObjectInfo Info;
		protected readonly int ColumnLength;
		protected readonly string fColumnName;
		protected readonly string fColumnNameWhiteSpace;
		protected readonly Type fDataType;
		protected readonly object AdoDefaultValue;

		#endregion

		#region IComparable Members

		public int CompareTo(object value)
		{
			return string.Compare(ColumnName, ((AutoProperty)value).ColumnName, StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}
