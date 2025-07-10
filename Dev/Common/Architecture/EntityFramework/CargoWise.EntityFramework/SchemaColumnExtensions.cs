using System;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public static class SchemaColumnExtensions
	{
		public static Type GetEquivalentZType(this SchemaColumn column)
		{
			switch (column.ColumnType)
			{
				case SchemaColumnType.Binary:
				case SchemaColumnType.Blob:
					return typeof(ZBlob);

				case SchemaColumnType.Bool:
					return typeof(ZBool);

				case SchemaColumnType.Byte:
					return typeof(ZByte);

				case SchemaColumnType.Date:
					return typeof(ZDate);

				case SchemaColumnType.DateTime:
					return typeof(ZDateTime);

				case SchemaColumnType.DateTimeOffset:
					return typeof(ZDateTimeOffset);

				case SchemaColumnType.Decimal:
					return typeof(ZDecimal);

				case SchemaColumnType.Geography:
					return typeof(ZGeography);

				case SchemaColumnType.Guid:
					return typeof(ZGuid);

				case SchemaColumnType.Int:
					return typeof(ZInt);

				case SchemaColumnType.Short:
					return typeof(ZShort);

				case SchemaColumnType.Long:
					return typeof(ZLong);

				case SchemaColumnType.String:
				case SchemaColumnType.Xml:
					return typeof(ZString);

				case SchemaColumnType.Time:
					return typeof(ZTime);

				default:
					throw new ArgumentException(FormattableString.Invariant($"Unknown schema column type '{column.ColumnType}' for column '{column.GetType().FullName}'"), nameof(column));
			}
		}
	}
}
