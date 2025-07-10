using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;

namespace Enterprise.DataTransfer.Native.Utils
{
	public static class DbCommandExtension
	{
		public static DataRow ExecuteDataRow(this DbCommand command, Func<DataTable, DataRow> filterOnMultiRowResult = null)
		{
			var table = new DataTable();
			using (var adapter = command.NewDataAdapter())
			{
				adapter.Fill(table);
			}

			if (filterOnMultiRowResult != null)
			{
				return filterOnMultiRowResult.Invoke(table);
			}

			return table.Rows.Count == 1 ? table.Rows[0] : null;
		}

		public static IEnumerable<DataRow> ExecuteDataTable(this DbCommand command)
		{
			var table = new DataTable();
			using (var adapter = command.NewDataAdapter())
			{
				adapter.Fill(table);
			}
			return table.AsEnumerable();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public static DataTableCollection ExecuteDataSet(this DbCommand command)
		{
			using (var dataSet = new DataSet())
			using (var adapter = command.NewDataAdapter())
			{
				dataSet.Locale = CultureInfo.InvariantCulture;
				adapter.Fill(dataSet);
				return dataSet.Tables;
			}
		}
	}
}
