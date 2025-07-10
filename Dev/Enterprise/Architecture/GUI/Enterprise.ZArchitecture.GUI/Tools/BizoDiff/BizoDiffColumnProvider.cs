using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.DevTools
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
	public sealed class BizoDiffColumnProvider
	{
		readonly string[] ExcludedFieldTypes = ["Guid", "SqlGeography"];
		readonly IDictionary<Type, List<string>> RowColumns = new Dictionary<Type, List<string>>();

		public IDictionary<Type, List<string>> RegisteredTypes => RowColumns;

		public void RegisterType(INeedRow obj)
		{
			_ = GetPropertyList(obj);
		}

		public List<string> GetPropertyList(INeedRow obj)
		{
			var row = obj.Row;
			if (row == null)
			{
				return new List<string>();
			}

			var objType = obj.GetType();
			if (!RowColumns.ContainsKey(objType))
			{
				var columns = row.Table.Columns.Cast<DataColumn>().Select(v => v.ColumnName);

				columns = columns.Where(v => !CargoWise.Schema.Schema.IsSystemColumn(v)
					&& !ExcludedFieldTypes.Contains(obj.Row[v].GetType().Name)).OrderBy(v => v);

				RowColumns[objType] = columns.ToList();
			}

			return RowColumns[objType];
		}
	}
}
