using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class ColumnIndexerExtensions
	{
		public static void SetValue(this IColumnIndexer row, SchemaStringColumn column, ZString? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				var value = valueSource.Value;
				var maxLengthOverride = (row as BusinessObject)?.ZPropertyInfoHash.GetPropertySafe(column.Name)?.MaxLength ?? 0;
				row.SetValue(column, value.GetValidMaxLengthValue(column, logger, maxLengthOverride));
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaGuidColumn column, ZGuid? valueSource, IXmlImportLogger logger, Func<SchemaColumn, string> getColumnName = null)
		{
			if (valueSource.HasValue)
			{
				var value = valueSource.Value;
				if (value.IsEmpty || value.IsValid)
				{
					row.SetValue(column, value, getColumnName);
				}
				else
				{
					logger.Log(LogType.Error, Res.GetString("7DF94D84-3A9A-4829-9E69-D5AEE1BA9D5F", "Cannot set Invalid GUID '{0}' to Column {1}.", value, column.Name));
				}
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaBoolColumn column, ZBool? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				row.SetValue(column, valueSource.Value);
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaBinaryColumn column, ZBlob? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				row.SetValue(column, valueSource.Value);
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaByteColumn column, ZByte? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				row.SetValue(column, valueSource.Value);
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaDateColumn column, ZDate? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				var value = valueSource.Value;

				if (value.IsEmpty || value.IsValid)
				{
					row.SetValue(column, value);
				}
				else
				{
					logger.Log(LogType.Error, Res.GetString("cf0e0997-9157-476a-b174-75c5f4c3d863", "Cannot set Invalid Date to Column {0}.{1}.", column.TableName, column.Name));
				}
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaDateTimeColumn column, ZDateTime? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				var value = valueSource.Value;
				if (value.IsEmpty || value.IsValidForTargetColumn(column))
				{
					row.SetValue(column, column.SqlDbType == System.Data.SqlDbType.Date ? value.Date : value);
				}
				else
				{
					logger.Log(LogType.Warning, Res.GetString("2f34024b-932f-425d-a342-c70dac69b99a", "Cannot set Column {1}.{2} to '{0}' - Value is out of range.", value.ToString(), column.TableName, column.Name));
				}
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaDateTimeOffsetColumn column, ZDateTimeOffset? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				var value = valueSource.Value;
				if (value.IsEmpty || value.IsValid)
				{
					row.SetValue(column, value);
				}
				else
				{
					logger.Log(LogType.Warning, Res.GetString("cb485b71-5e38-4e94-a47b-1cb4a29a1bcb", "Cannot set Column {1}.{2} to '{0}' - Value is invalid.", value.ToString(), column.TableName, column.Name));
				}
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaTimeColumn column, ZTime? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				var value = valueSource.Value;
				if (value.IsEmpty || value.IsValidForTargetColumn(column))
				{
					row.SetValue(column, value);
				}
				else
				{
					logger.Log(LogType.Warning, Res.GetString("2f34024b-932f-425d-a342-c70dac69b99a", "Cannot set Column {1}.{2} to '{0}' - Value is out of range.", value.ToString(), column.TableName, column.Name));
				}
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaGeographyColumn column, ZGeography? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				var value = valueSource.Value;
				if (value.IsEmpty || value.IsValid)
				{
					row.SetValue(column, value);
				}
				else
				{
					logger.Log(LogType.Warning, Res.GetString("cb485b71-5e38-4e94-a47b-1cb4a29a1bcb", "Cannot set Column {1}.{2} to '{0}' - Value is invalid.", value.ToString(), column.TableName, column.Name));
				}
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaIntColumn column, ZInt? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				row.SetValue(column, valueSource.Value);
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaIntColumn column, ZLong? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				row.SetValue(column, GetValidIntValue(column, valueSource.Value, logger));
			}
		}

		static ZInt GetValidIntValue(SchemaIntColumn column, ZLong value, IXmlImportLogger logger)
		{
			ZInt result;
			if (value > SchemaIntColumn.MaxValue)
			{
				var maxValue = SchemaIntColumn.MaxValue;
				logger.Log(LogType.Warning, MaxValueWarning(value.ToString(), column.Name, maxValue.ToString()));
				result = maxValue;
			}
			else if (value < SchemaIntColumn.MinValue)
			{
				var minValue = SchemaIntColumn.MinValue;
				logger.Log(LogType.Warning, MinValueWarning(value.ToString(), column.Name, minValue.ToString()));
				result = minValue;
			}
			else
			{
				result = value.ToZInt();
			}
			return result;
		}

		public static void SetValue(this IColumnIndexer row, SchemaShortColumn column, ZLong? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				row.SetValue(column, GetValidShortValue(column, valueSource.Value, logger));
			}
		}

		static ZShort GetValidShortValue(SchemaShortColumn column, ZLong value, IXmlImportLogger logger)
		{
			ZShort result;
			if (value > SchemaShortColumn.MaxValue)
			{
				var maxValue = SchemaShortColumn.MaxValue;
				logger.Log(LogType.Warning, MaxValueWarning(value.ToString(), column.Name, maxValue.ToString()));
				result = (ZShort)maxValue;
			}
			else if (value < SchemaShortColumn.MinValue)
			{
				var minValue = SchemaShortColumn.MinValue;
				logger.Log(LogType.Warning, MinValueWarning(value.ToString(), column.Name, minValue.ToString()));
				result = (ZShort)minValue;
			}
			else
			{
				result = (ZShort)value.ToZInt();
			}

			return result;
		}

		public static void SetValue(this IColumnIndexer row, SchemaShortColumn column, ZInt? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				row.SetValue(column, GetValidShortValue(column, valueSource.Value, logger));
			}
		}

		static ZShort GetValidShortValue(SchemaShortColumn column, ZInt value, IXmlImportLogger logger)
		{
			ZShort result;
			if (value > SchemaShortColumn.MaxValue)
			{
				var maxValue = SchemaShortColumn.MaxValue;
				logger.Log(LogType.Warning, MaxValueWarning(value.ToString(), column.Name, maxValue.ToString()));
				result = (ZShort)maxValue;
			}
			else if (value < SchemaShortColumn.MinValue)
			{
				var minValue = SchemaShortColumn.MinValue;
				logger.Log(LogType.Warning, MinValueWarning(value.ToString(), column.Name, minValue.ToString()));
				result = (ZShort)minValue;
			}
			else
			{
				result = (ZShort)value;
			}

			return result;
		}

		public static void SetValue(this IColumnIndexer row, SchemaShortColumn column, ZShort? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				row.SetValue(column, valueSource.Value);
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaDecimalColumn column, ZDecimal? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				row.SetValue(column, GetValidDecimalValue(column, valueSource.Value, logger));
			}
		}

		static ZDecimal GetValidDecimalValue(SchemaDecimalColumn column, ZDecimal value, IXmlImportLogger logger)
		{
			if (!value.IsWithinSqlPrecisionAndScale(column.Precision, column.Scale))
			{
				var maxValue = (decimal)Math.Pow(10, column.Precision - column.Scale) - 1;
				logger.Log(LogType.Warning, MaxValueWarning(value.ToString(), column.Name, maxValue.ToString()));
				value = maxValue;
			}
			return value;
		}

		public static void SetValue(this IColumnIndexer row, SchemaStringColumn column, ICodeDataObject property, IXmlImportLogger logger)
		{
			ZString code;
			if (property.TryGetCodeAsUpperCase(out code))
			{
				SetValue(row, column, code, logger);
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaLongColumn column, ZLong? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				row.SetValue(column, valueSource.Value);
			}
		}

		public static void SetValue(this IColumnIndexer row, SchemaLongColumn column, ZInt? valueSource, IXmlImportLogger logger)
		{
			if (valueSource.HasValue)
			{
				row.SetValue(column, valueSource.Value);
			}
		}

		static ZString MaxValueWarning(ZString value, ZString column, ZString maxValue) => Res.GetString("3d66ae80-b950-47d3-911c-c67ca32b23c5", "Attempted to insert '{0}' into Field [{1}] which has a maximum numeric value of '{2}'. Field was truncated to the max value.", value.ToString(), column, maxValue);

		static ZString MinValueWarning(ZString value, ZString column, ZString minValue) => Res.GetString("2394dff9-87ca-48fd-bbed-8a513526c16c", "Attempted to insert '{0}' into Field [{1}] which has a minimum numeric value of '{2}'. Field was truncated to the min value.", value.ToString(), column, minValue);
	}
}
