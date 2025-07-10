using System;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Extensions
{
	public static class DataIndexerExtensions
	{
		public static void SetValue(this IColumnIndexer row, SchemaColumn column, IZType value, Func<SchemaColumn, string> getColumnName = null)
		{
			var columnName = getColumnName == null ? column.Name : getColumnName(column);
			row[columnName] = value;
		}

		public static ZString GetValue(this IColumnIndexer row, SchemaStringColumn column)
		{
			return GetValue<ZString>(row, column);
		}

		public static ZByte GetValue(this IColumnIndexer row, SchemaByteColumn column)
		{
			return GetValue<ZByte>(row, column);
		}

		public static ZDecimal GetValue(this IColumnIndexer row, SchemaDecimalColumn column)
		{
			return GetValue<ZDecimal>(row, column);
		}

		public static ZBool GetValue(this IColumnIndexer row, SchemaBoolColumn column)
		{
			return GetValue<ZBool>(row, column);
		}

		public static ZDate GetValue(this IColumnIndexer row, SchemaDateColumn column)
		{
			return GetValue<ZDate>(row, column);
		}

		public static ZDateTime GetValue(this IColumnIndexer row, SchemaDateTimeColumn column)
		{
			return GetValue<ZDateTime>(row, column);
		}

		public static ZDateTimeOffset GetValue(this IColumnIndexer row, SchemaDateTimeOffsetColumn column)
		{
			return GetValue<ZDateTimeOffset>(row, column);
		}

		public static ZGeography GetValue(this IColumnIndexer row, SchemaGeographyColumn column)
		{
			return GetValue<ZGeography>(row, column);
		}

		public static ZTime GetValue(this IColumnIndexer row, SchemaTimeColumn column)
		{
			return GetValue<ZTime>(row, column);
		}

		public static ZInt GetValue(this IColumnIndexer row, SchemaIntColumn column)
		{
			return GetValue<ZInt>(row, column);
		}

		public static ZShort GetValue(this IColumnIndexer row, SchemaShortColumn column)
		{
			return GetValue<ZShort>(row, column);
		}

		public static ZLong GetValue(this IColumnIndexer row, SchemaLongColumn column)
		{
			return GetValue<ZLong>(row, column);
		}

		public static ZGuid GetValue(this IColumnIndexer row, SchemaGuidColumn column)
		{
			return GetValue<ZGuid>(row, column);
		}

		public static ZBlob GetValue(this IColumnIndexer row, SchemaBinaryColumn column)
		{
			return GetValue<ZBlob>(row, column);
		}

		public static TSource GetValue<TSource>(this IColumnIndexer row, SchemaColumn column)
		{
			return (TSource)row.GetValue(column);
		}

		public static IZType GetValue(this IColumnIndexer row, SchemaColumn column)
		{
			return ZDataType.ObjectToZType(column.GetEquivalentZType(), row[column.Name]);
		}
	}
}
