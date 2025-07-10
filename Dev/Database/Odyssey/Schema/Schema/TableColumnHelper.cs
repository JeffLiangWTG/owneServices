using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Schema
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This is used by AddInfoPartialWrapperGenerator")]
	public static class TableColumnHelper
	{
		public static KeyValuePair<string, string>[] GetColumns(string tableName)
		{
			var tableSchema = EnterpriseSchema.GetTableSchema(tableName)
				?? throw new NotSupportedException($"{tableName} is not currently supported.");
			var pkColumn = tableSchema.PK;
			var result = new Dictionary<string, string>();
			foreach (var column in tableSchema.All.Where(x => x != pkColumn && !CargoWise.Schema.Schema.IsSystemColumn(x.Name)))
			{
				result.Add(column.Name, GetDataType(column));
			}

			return result.ToArray();
		}

		static string GetDataType(SchemaColumn column)
		{
			if (column is SchemaGuidColumn)
			{
				return "ZGuid";
			}
			else if (column is SchemaXmlColumn || column is SchemaStringColumn)
			{
				return "ZString";
			}
			else if (column is SchemaBinaryColumn)
			{
				return "ZBlob";
			}
			else if (column is SchemaDateColumn)
			{
				return "ZDate";
			}
			else if (column is SchemaDateTimeColumn)
			{
				return "ZDateTime";
			}
			else if (column is SchemaShortColumn)
			{
				return "ZShort";
			}
			else if (column is SchemaIntColumn)
			{
				return "ZInt";
			}
			else if (column is SchemaLongColumn)
			{
				return "ZLong";
			}
			else if (column is SchemaGeographyColumn)
			{
				return "ZGeography";
			}
			else if (column is SchemaDecimalColumn)
			{
				return "ZDecimal";
			}
			else if (column is SchemaDateTimeOffsetColumn)
			{
				return "ZDateTimeOffset";
			}
			else if (column is SchemaByteColumn)
			{
				return "ZByte";
			}
			else if (column is SchemaBoolColumn)
			{
				return "ZBool";
			}
			else
			{
				return column.GetType().Name;
			}
		}
	}
}
