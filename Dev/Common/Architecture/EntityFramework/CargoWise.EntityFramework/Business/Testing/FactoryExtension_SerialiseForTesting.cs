using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public static class FactoryExtension_SerialiseForTesting
	{
		public static string SerialiseForTesting<T>(this BusinessObjectFactory factory, ZQuery query, ZString orderBy) where T : BusinessObject
		{
			query.OrderBy = orderBy;
			var bos = factory.Load<T>(query);
			return SerialiseForTesting(bos);
		}

		static string SerialiseForTesting<T>(T[] bos) where T : BusinessObject
		{
			var result = new List<string>();
			foreach (var bo in bos)
			{
				result.Add("");
				result.Add(("---- " + typeof(T).Name + " ").PadRight(80, '-'));
				result.Add(SerialiseForTesting(bo));
			}

			return string.Join("\r\n", result.ToArray());
		}

		static string SerialiseForTesting(BusinessObject bo)
		{
			var pkColumn = bo.PKSchemaColumn;

			var columns = pkColumn.TableSchema.All.Cast<SchemaColumn>()
				.Where(column => column != pkColumn && !column.LightValidationIsValidColumn && !((IZType)bo[column]).IsEmpty)
				.OrderBy(column => column.Name)
				.Select(column => column.Name.PadRight(25) + " - [" + FormatValue(bo, column) + "]")
				.ToArray();

			return string.Join("\r\n", columns);
		}

		static string FormatValue(BusinessObject bo, SchemaColumn column)
		{
			var columnValue = bo[column];

			if (column.ColumnType == SchemaColumnType.Guid)
			{
				return "GUID Hidden";
			}

			return columnValue.ToString();
		}
	}
}
