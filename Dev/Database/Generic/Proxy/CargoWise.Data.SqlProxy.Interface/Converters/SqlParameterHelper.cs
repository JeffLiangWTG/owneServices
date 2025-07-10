using System.Collections;
using System.Data;
using System.Globalization;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.Data.SqlProxy.Interface.Converters;

public static class SqlParameterHelper
{
	public static SqlParameter CreateTableValuedParameter(string parameterName, SchemaColumn column, IEnumerable values)
	{
		var table = new DataTable { Locale = CultureInfo.InvariantCulture };
		var tableColumn = table.Columns.Add("Value", column.DotNetType);
		tableColumn.AllowDBNull = false;
		table.PrimaryKey = [tableColumn];

		var converter = new ZDbValueConversion().TryGetConverterForValues(values);
		foreach (var value in values)
		{
			var row = converter is null ? value : converter.ConvertTo(value, column.DotNetType);

			if (!table.Rows.Contains(row))
			{
				table.Rows.Add(row);
			}
		}

		return CreateTableValuedParameter(parameterName, column.TVPName, table);
	}

	public static SqlParameter CreateTableValuedParameter(string parameterName, string parameterTypeName, DataTable parameterValue)
	{
		if (string.IsNullOrWhiteSpace(parameterValue.TableName))
		{
			parameterValue.TableName = string.Format(CultureInfo.InvariantCulture, "Data{0}", Guid.NewGuid());
		}

		return new SqlParameter(parameterName, SqlDbType.Structured) { TypeName = parameterTypeName, Value = parameterValue };
	}

	public static SqlParameter CreateTableValuedParameter<T>(string parameterName, string parameterTypeName, IEnumerable<T> values)
	{
		using var dataTable = new DataTable();

		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("Value", typeof(T));

		foreach (var value in values)
		{
			dataTable.Rows.Add(value);
		}

		return CreateTableValuedParameter(parameterName, parameterTypeName, dataTable);
	}
}
