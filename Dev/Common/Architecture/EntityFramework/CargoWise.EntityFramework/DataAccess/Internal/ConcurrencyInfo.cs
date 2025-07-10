using System.Collections;
using System.Collections.Specialized;
using System.Data;
using CargoWise.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public static class ConcurrencyInfo
	{
		#region GetConcurrency

		public static ConcurrencyPolicy GetConcurrencyPolicy(this ZPropertyInfo propertyInfo, DataRow row)
		{
			return propertyInfo != null
				? Get(row, row.Table.Columns[propertyInfo.Name])
				: null;
		}

		public static bool ShouldCheckConcurrency(this SchemaColumn schemaColumn, DataRow row, DataColumn column)
		{
			return DataUtils.ShouldCheckConcurrency(schemaColumn, row, column);
		}

		public static ConcurrencyPolicy Get(DataRow row, DataColumn column)
		{
			return DataUtils.GetConcurrencyPolicy(row, column);
		}

		#endregion

		#region Set Concurrency

		public static void SetConcurrencyPolicy(this DataRow row, string columnName, ConcurrencyPolicy strategy)
		{
			if (row != null && strategy != null)
			{
				Set(row, row.Table.Columns[columnName], strategy);
			}
		}

		public static void SetConcurrencyPolicy(this BusinessObject bizo, string columnName, ConcurrencyPolicy strategy)
		{
			var row = bizo.Row;
			SetConcurrencyPolicy(row, columnName, strategy);
		}

		static void Set(DataRow row, DataColumn column, ConcurrencyPolicy strategy)
		{
			IDictionary dictionary = GetConcurrencyInfoDictionary(column);

			if (dictionary == null)
			{
				dictionary = new HybridDictionary();
				SetConcurrencyInfoDictionary(column, dictionary);
			}

			dictionary[ZDataUtils.GetPK(row)] = strategy;
		}

		static IDictionary GetConcurrencyInfoDictionary(DataColumn column)
		{
			return (IDictionary)column.ExtendedProperties[typeof(ConcurrencyPolicy)];
		}

		static void SetConcurrencyInfoDictionary(DataColumn column, IDictionary dictionary)
		{
			column.ExtendedProperties[typeof(ConcurrencyPolicy)] = dictionary;
		}

		#endregion
	}
}
