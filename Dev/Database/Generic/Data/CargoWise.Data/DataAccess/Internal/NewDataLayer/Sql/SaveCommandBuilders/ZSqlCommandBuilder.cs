using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SqlServer.Types;
using static System.FormattableString;

namespace CargoWise.EntityFramework
{
	public abstract class ZSqlCommandBuilder
	{
		public const string DataParameterPrefix = "@D_";

#if DEBUG
		public List<DataColumn> ConcurrencyCheckFields = new List<DataColumn>();
#endif

		public ZSqlCommandBuilder(DataRow row, string tableName, bool compress, ITableSchema schema, List<ILargeColumnSaver> largeColumnSavers = null)
		{
			this.Row = row;
			this.TableName = AddSchemaName(tableName);
			Compress = compress;
			LargeColumnSavers = largeColumnSavers ?? new List<ILargeColumnSaver>();
			Schema = schema;
		}

		public bool Compress { get; private set; }

		public void AppendCommandTextAndBlobSaver(StringBuilder commandTextToAppendTo)
		{
			CommandText = commandTextToAppendTo;
			Build();
		}

		protected abstract void Build();

		protected string AddSchemaName(string tableName)
		{
			return SchemaPrepender.AddSchemaName(tableName);
		}

		protected object AddDataParameterAndBlobSaverIfLargeBlobOrText(DataRow row, object paramValue, SchemaColumn paramSchemaColumn)
		{
			var pkColumnName = Schema.PK.Name;
			var pk = (Guid)row[pkColumnName];
			if (paramValue is IStreamSource)
			{
				LargeColumnSavers.Add(new ZBinarySaver(TableName, pkColumnName, pk, paramSchemaColumn.Name, paramSchemaColumn.SqlDbType, (IStreamSource)paramValue, Compress));
				paramValue = Array.Empty<byte>();
			}
			else if (paramValue is ITextReaderSource)
			{
				LargeColumnSavers.Add(new ZTextSaver(TableName, pkColumnName, pk, paramSchemaColumn.Name, paramSchemaColumn.SqlDbType, (ITextReaderSource)paramValue));
				paramValue = "";
			}
			else
			{
				if (paramSchemaColumn is SchemaStringColumn)
				{
					if (paramValue is string textValue)
					{
						paramValue = textValue.Replace("\\\n", "\\\\\n\n");
					}
				}

				if (paramSchemaColumn.IsLargeBinaryOrText && paramValue != DBNull.Value)
				{
					if (paramSchemaColumn is SchemaBinaryColumn)
					{
						if (paramValue is byte[] blobValue && blobValue.Length > ZLargeColumnSaver.MaxChunkSize)
						{
							paramValue = Array.Empty<byte>();
							LargeColumnSavers.Add(new ZBinarySaver(TableName, pkColumnName, pk, paramSchemaColumn.Name, paramSchemaColumn.SqlDbType, new ByteArrayStreamSource(blobValue), Compress));
						}
					}
					else if (paramSchemaColumn is SchemaStringColumn)
					{
						if (paramSchemaColumn.SqlDbType != SqlDbType.Xml && paramValue is string textValue && textValue.Length > ZLargeColumnSaver.MaxChunkSize)
						{
							paramValue = "";
							LargeColumnSavers.Add(new ZTextSaver(TableName, pkColumnName, pk, paramSchemaColumn.Name, paramSchemaColumn.SqlDbType, new StringReaderSource(textValue)));
						}
					}
				}
			}

			return paramValue;
		}

		protected virtual object GetSourceValueIfSet(DataRow row, object value, SchemaColumn column)
		{
			return value;
		}

		protected virtual void AddConcurrencyCheckWhereClause(bool ignoreAllConcurrencyCheck = false)
		{
			var rowHasCurrentVersion = Row.HasVersion(DataRowVersion.Current);

			CommandText.Append("WHERE\r\n");

			var numberOfWhereParams = 0;
			for (var i = 0; i < Row.Table.Columns.Count; i++)
			{
				var col = Row.Table.Columns[i];
				var originalData = Row[col, DataRowVersion.Original];
				var currentData = rowHasCurrentVersion ? Row[col, DataRowVersion.Current] : null;
				try
				{
					var schemaColumn = Schema.GetSchemaColumn(col.ColumnName);

					if (schemaColumn == null)
					{
						continue;
					}

					var isPK = col.ColumnName == Schema.PK.Name;
					var concurrencyPolicy = !ignoreAllConcurrencyCheck ? DataUtils.GetConcurrencyPolicy(Row, col) : null;
					var requiresConcurrencyCheck = concurrencyPolicy != null && concurrencyPolicy.ShouldCheck(Row, col);

					if (isPK || !ignoreAllConcurrencyCheck && requiresConcurrencyCheck)
					{
#if DEBUG
						if (!isPK)
						{
							ConcurrencyCheckFields.Add(col);
						}
#endif

						if (numberOfWhereParams == 0)
						{
							CommandText.Append("\t");
						}
						else
						{
							CommandText.Append("\tAND "); // part of sql query
						}

						var multiValue = !isPK && concurrencyPolicy.AllowMerge && rowHasCurrentVersion && !originalData.Equals(currentData);
						if (multiValue)
						{
							CommandText.Append("(");
						}
						numberOfWhereParams += AddColumnValueCheck(CommandText, schemaColumn, originalData);
						if (multiValue)
						{
							CommandText.Append(" OR ");
							numberOfWhereParams += AddColumnValueCheck(CommandText, schemaColumn, currentData);
							CommandText.Append(")");
						}
					}
				}
				catch (InvalidTableNameException)
				{
					CommandText.Append(ZSaver.TableDoesNotExistIdentifier).Append(" [").Append(TableName).Append("]"); // until we have AutoSchema.GetSchemaColumnSafe()
				}
			}
		}

		protected KeyValuePair<string, string> CreateLargeBinaryOrTextConcurrencyCheck(object originalData, SchemaColumn schemaColumn)
		{
			string key;
			string value;

			if (schemaColumn.DotNetType == typeof(string))
			{
				var data = Encoding.Unicode.GetBytes((string)originalData);
				value = string.Format("N'{0}'", HashCalculator.CalculateMD5Hash(data)); // this is a simple format string
				key = string.Format("[dbo].[CLRCalculateMD5Hash](convert(varbinary(max), convert(nvarchar(max), {0})))", schemaColumn.Name); // this is a simple format string
			}
			else if (schemaColumn.DotNetType == typeof(byte[]))
			{
				value = string.Format("'{0}'", HashCalculator.CalculateMD5Hash((byte[])originalData));

				key = string.Format("[dbo].[CLRCalculateMD5Hash](cast({0} as varbinary(max)))",
					schemaColumn.Name);
			}
			else
			{
				throw new InvalidOperationException(string.Format("Cannot create concurrency check for {0}", schemaColumn.Name));
			}

			return new KeyValuePair<string, string>(key, value);
		}

#if DEBUG
		protected
#endif
		internal int AddColumnValueCheck(StringBuilder sb, SchemaColumn column, object value)
		{
			if (value.Equals(DBNull.Value))
			{
				sb.Append(column.Name).Append(" is NULL\r\n"); // part of sql query
				return 0;
			}
			else if (column.IsLargeBinaryOrText)
			{
				var pair = CreateLargeBinaryOrTextConcurrencyCheck(value, column);
				sb.AppendLine(string.Format("{0} = {1}", pair.Key, pair.Value));
				return 1;
			}
			else if (column.ColumnType == SchemaColumnType.Geography)
			{
				sb.Append(column.Name).Append(".STAsText() = "); // part of sql query
				sb.AppendLine(GetQuoteEscapeTruncateAndCompress(value, column) + ".STAsText()"); // part of sql query
				return 1;
			}
			else
			{
				sb.Append(column.Name).Append(" = ");
				sb.AppendLine(GetQuoteEscapeTruncateAndCompress(value, column));
				return 1;
			}
		}

		public virtual object GetTruncatedBasicValueForDatabase(object value, SchemaColumn schemaColumn)
		{
			return DataUtils.GetTruncatedBasicValueForDatabase(value, schemaColumn);
		}

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		protected string GetQuoteEscapeTruncateAndCompress(object value, SchemaColumn schemaColumn)
		{
			string result;

			if (value == DBNull.Value)
			{
				result = SQL_NULL_KEYWORD;
				return result;
			}

			value = GetTruncatedBasicValueForDatabase(value, schemaColumn);

			DetectEmptySP_CustomProperties(value as byte[], "GetTruncatedBasicValueForDatabase");

			if (schemaColumn is SchemaStringColumn)
			{
				if (value is string textValue)
				{
					value = textValue
						.Replace("\\\n", "\\\\\n\n")
						.Replace("\\\r\n", "\\\\\r\n\r\n");
				}
			}

			if (Compress)
			{
				value = ZCompressor.GetCompressedVersion(value, schemaColumn.Name);
			}

			DetectEmptySP_CustomProperties(value as byte[], "GetCompressedVersion");
			ReportEmptySP_CustomPropertiesErrorIfNeeded(value as byte[]);

			switch (value)
			{
				case string stringValue:
					result = GetQuotedAndEscapedString(stringValue, schemaColumn.IsUnicode);
					break;
				case DateTime dateTime when dateTime.Year == 1: //can only happen if validation is suspended
					result = schemaColumn.IsNullable
						? SQL_NULL_KEYWORD
						: QuoteString(SqlFormatInfo.ToSqlDateTimeString(new DateTime(ZTypeConstants.MinSmallDateTimeYearValue, dateTime.Month, dateTime.Day)));
					break;
				case DateTime dateTime:
					result = QuoteString(SqlFormatInfo.ToSqlDateTimeString(dateTime));
					break;
				case TimeSpan time:
					result = QuoteString(SqlFormatInfo.ToSqlTimeString(time));
					break;
				case DateTimeOffset dateTimeOffset when dateTimeOffset.Year == 1: //can only happen if validation is suspended
					result = schemaColumn.IsNullable
						? SQL_NULL_KEYWORD
						: QuoteString(SqlFormatInfo.ToSqlDateTimeOffsetString(new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.Zero)));
					break;
				case DateTimeOffset dateTimeOffset:
					result = QuoteString(SqlFormatInfo.ToSqlDateTimeOffsetString(dateTimeOffset));
					break;
				case Guid guidValue:
					CheckValidGuidValue(guidValue, schemaColumn);
					result = QuoteString(value.ToString());
					break;
				case byte[] byteArrayValue:
					result = GetBytesAsString(byteArrayValue);
					break;
				case decimal decimalValue:
					result = decimalValue.ToString(CultureInfo.InvariantCulture);
					break;
				case long longValue:
					result = longValue.ToString(CultureInfo.InvariantCulture);
					break;
				case int intValue:
					result = intValue.ToString(CultureInfo.InvariantCulture);
					break;
				case short shortValue:
					result = shortValue.ToString(CultureInfo.InvariantCulture);
					break;
				case byte byteValue:
					result = byteValue.ToString(CultureInfo.InvariantCulture);
					break;
				case bool boolValue:
					result = QuoteString(boolValue ? "Y" : "N");
					break;
				case SqlGeography geographyValue:
					result = string.Format(CultureInfo.InvariantCulture, "geography::STGeomFromText('{0}', 4326)", geographyValue.AsTextZM().ToSqlString().ToString()); // format string for code
					break;
				default:
					result = GetQuotedAndEscapedString(value.ToString(), false);
					ErrorReporter.ReportOnce("DataLayerInvalidType" + value.GetType().FullName, "Data layer does not know how to deal with unknown type: " + value.GetType().FullName);
					break;
			}

			return result;
		}

		void CheckValidGuidValue(Guid guidValue, SchemaColumn schemaColumn)
		{
			var isInvalidGuidValue = guidValue.Equals(ZTypeConstants.InvalidGuid);
			var isMissingGuidValue = guidValue.Equals(ZTypeConstants.MissingGuid);

			if (isInvalidGuidValue || isMissingGuidValue)
			{
				// If foreign key table prefix is not empty, it is column like XX_YY or XX_YY_Zzzz
				// and there likely is a foreign key constraint in database (there is just few such columns without constraint),
				// so save to will fail due to constraint violation, causing ZSaveException and possibly error handling.
				// As there will ZSaveException with handling (or unhandled error report) - there is no need to report invalid value.
				//
				// Columns like XX_ParentPK without foreign key constraint will be saved with special invalid Guid value, so should be reported.
				if (string.IsNullOrEmpty(ZRowRelationshipManager.GetForeignKeyTablePrefix(schemaColumn.Name)) &&
					!GuidColumnsAllowedToHaveSpecialValues.Contains(schemaColumn.Name))
				{
					var invalidValueName = isMissingGuidValue ? ZTypeConstants.MissingGuidName : ZTypeConstants.InvalidGuidName;
					ErrorReporter.ReportOnce(Invariant($"ZGuid.{invalidValueName} is saved to database in column {schemaColumn.Name}"));
				}
			}
		}

		IEnumerable<string> GuidColumnsAllowedToHaveSpecialValues =>
			guidColumnsAllowedToHaveSpecialValues ?? (guidColumnsAllowedToHaveSpecialValues = new List<string>
			{
				"GG_ActiveDirectoryObjectGuid",
				"GS_ActiveDirectoryObjectGuid"
			});

		IEnumerable<string> guidColumnsAllowedToHaveSpecialValues;

#if DEBUG
		public string GetQuoteEscapeTruncateAndCompressExposedForTest(object value, SchemaColumn schemaColumn)
		{
			return GetQuoteEscapeTruncateAndCompress(value, schemaColumn);
		}
#endif

		protected virtual string InputQuotes
		{
			get { return "'"; }
		}

		protected virtual string OutputQuotes
		{
			get { return "'"; }
		}

		protected virtual string EscapedInputQuotes
		{
			get { return "''"; }
		}

		public abstract bool IsConcurrencyCheckRequired { get; }

		protected string QuoteString(string value)
		{
			return OutputQuotes + value + OutputQuotes;
		}

#if DEBUG
		public string QuoteStringExposedForTest(string value)
		{
			return QuoteString(value);
		}
#endif

		protected string GetQuotedAndEscapedString(string value, bool isUnicode)
		{
			return isUnicode
				? string.Concat("N", OutputQuotes, value.Replace(InputQuotes, EscapedInputQuotes), OutputQuotes)
				: string.Concat(OutputQuotes, value.Replace(InputQuotes, EscapedInputQuotes), OutputQuotes);
		}

		protected readonly DataRow Row;
		protected readonly string TableName;
		protected StringBuilder CommandText;

		protected ITableSchema Schema { get; }
		public List<ILargeColumnSaver> LargeColumnSavers { get; private set; }

#if DEBUG
		public
#endif
		string GetBytesAsString(byte[] bytes)
		{
			const string codes = "0123456789ABCDEF";

			var builder = new StringBuilder("0x", bytes.Length * 2 + 2); // building a hex value

			for (var i = 0; i < bytes.Length; i++)
			{
				int index = bytes[i];
				builder.Append(codes[(index & 0xF0) >> 4]);
				builder.Append(codes[index & 0x0F]);
			}

			return builder.ToString();
		}

		protected const string SQL_NULL_KEYWORD = "NULL";

		internal virtual void AddConcurrencyCheck(StringBuilder currentCommandText, string concurrencyErrorText)
		{
			if (IsConcurrencyCheckRequired)
			{
				currentCommandText.AppendLine(string.Format("if @@ROWCOUNT = 0 RAISERROR('{0}', 16, 1);", concurrencyErrorText)); // sql command part
			}
		}

		protected bool UseParameters
		{
			get
			{
				if (!fUseParameters.HasValue)
				{ fUseParameters = GlobalServiceProvider.Instance.GetRequiredService<IZSqlSaverConfiguration>().ParameterizeInsertAndUpdateStatements; }
				return fUseParameters.Value;
			}
		}
		bool? fUseParameters;

		#region Empty SP_CustomProperties Detecting
		//Temporarily added to solve CS01111146

		protected void DetectEmptySP_CustomProperties(byte[] value, string detectedPlace)
		{
			if (!NeedToDetectSP_CustomProperties || value == null)
			{
				return;
			}

			if (value.Length > 0)
			{
				OriginalSP_CustomPropertiesValue = value;
			}
			else if (string.IsNullOrEmpty(FirstDetectedPlaceFlag))
			{
				FirstDetectedPlaceFlag = detectedPlace;
			}
		}

		protected void ClearEmptySP_CustomPropertiesDetectingInfo()
		{
			if (!NeedToDetectSP_CustomProperties)
			{
				return;
			}

			OriginalSP_CustomPropertiesValue = Array.Empty<byte>();
			FirstDetectedPlaceFlag = null;
			NeedToDetectSP_CustomProperties = false;
		}

		protected void ReportEmptySP_CustomPropertiesErrorIfNeeded(byte[] value)
		{
			if (!NeedToDetectSP_CustomProperties || value == null || value.Length > 0 || FirstDetectedPlaceFlag == "AddDataParameterAndBlobSaverIfLargeBlobOrText")
			{
				return;
			}
			var message = $@"Empty SP_CustomProperties in ZSqlCommandBuilder.
First Detected Place: {FirstDetectedPlaceFlag}
Original Value: {GetBytesAsString(OriginalSP_CustomPropertiesValue)}";
			ErrorReporter.ReportOnce("EmptySP_CustomPropertiesError", message);
		}

		protected byte[] OriginalSP_CustomPropertiesValue { get; set; } = Array.Empty<byte>();

		protected string FirstDetectedPlaceFlag { get; set; }

		protected bool NeedToDetectSP_CustomProperties { get; set; }

		#endregion
	}
}
