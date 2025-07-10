using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using CargoWise.Common;

namespace CargoWise.Data
{
	static class AdapterExtensions
	{
		public static void ProcessStringInterning(this DbDataAdapter adapter, DataSet dataSet)
		{
			if (adapter is not DbDataAdapterWithTimeout)
			{
				var interner = new StringInterner();
				foreach (DataTable table in dataSet.Tables)
				{
					interner.InternStringCells(table);
				}
			}
		}

		public static void ProcessStringInterning(this DbDataAdapter adapter, DataTable dataTable)
		{
			if (adapter is not DbDataAdapterWithTimeout)
			{
				var interner = new StringInterner();
				interner.InternStringCells(dataTable);
			}
		}
	}

	#region StringInterner

	class StringInterner
	{
		readonly Hashtable internedStrings = new Hashtable();

		internal void InternStringCells(DataTable dataTable)
		{
			Argument.NotNull(dataTable, nameof(dataTable)); // Suggested By ReviewBot 

			for (var i = 0; i < dataTable.Columns.Count; i++)
			{
				if (dataTable.Columns[i].DataType == typeof(string))
				{
					foreach (DataRow row in dataTable.Rows)
					{
						InternString(row[i] as string, val => row[i] = val);
					}
				}
			}
			dataTable.AcceptChanges();
		}

		internal void InternStringCells(IDataReader internalReader, object[] values)
		{
			for (var i = 0; i < values.Length; i++)
			{
				if (internalReader.GetFieldType(i) == typeof(string))
				{
					InternString(values[i] as string, val => values[i] = val);
				}
			}
		}

		void InternString(string value, Action<string> action)
		{
			if (value != null)
			{
				var internedValue = InternValue(value);
				if (!object.ReferenceEquals(value, internedValue))
				{
					action(internedValue);
				}
			}
		}

		string InternValue(string value)
		{
			if (value != null)
			{
				var internedValue = internedStrings[value];
				if (internedValue != null)
				{
					return (string)internedValue;
				}
				internedStrings.Add(value, value);
			}

			return value;
		}
	}

	#endregion
}
