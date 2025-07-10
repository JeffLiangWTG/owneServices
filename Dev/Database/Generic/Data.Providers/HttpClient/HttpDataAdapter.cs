using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Data.SqlProxy.Interface.Converters;

namespace CargoWise.Data.HttpClient;

[SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "BusinessObjectFactory not available from SQL Over Http Server side process")]
public class HttpDataAdapter : DbDataAdapter, IDbDataAdapter
{
	HttpDataAdapter(IDbCommand selectCommand)
	{
		this.selectCommand = selectCommand;
	}

	public static HttpDataAdapter New(IDbCommand selectCommand)
	{
		var newHttpDataAdapter = new HttpDataAdapter(selectCommand);
		return newHttpDataAdapter;
	}

	public override int Fill(DataSet dataSet)
	{
		if (dataSet == null)
		{
			throw new ArgumentNullException(nameof(dataSet));
		}

		var table = new DataTable("SchemaTable");
		using (var reader = selectCommand.ExecuteReader())
		{
			if (table.Columns.Count == 0)
			{
				var schemaTable = reader.GetSchemaTable();
				if (schemaTable != null)
				{
					foreach (DataColumn sourceDataColumn in schemaTable.Columns)
					{
						var dataColumn = new SerializedDataColumn(sourceDataColumn).ToDataColumn();
						table.Columns.Add(dataColumn);
						dataColumn.SetOrdinal(dataColumn.Ordinal);
					}
				}
			}

			while (reader.Read())
			{
				var row = table.NewRow();
				for (var i = 0; i < reader.FieldCount; i++)
				{
					row[i] = reader.GetValue(i);
				}

				table.Rows.Add(row);
			}
		}

		dataSet.Tables.Add(table);
		return table.Rows.Count;
	}

	public override DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType)
	{
		if (dataSet == null)
		{
			throw new ArgumentNullException(nameof(dataSet));
		}

		var table = new DataTable("SchemaTable");
		using (var reader = selectCommand.ExecuteReader(CommandBehavior.SchemaOnly))
		{
			var schemaTable = reader.GetSchemaTable();
			if (schemaTable != null)
			{
				foreach (DataColumn sourceDataColumn in schemaTable.Columns)
				{
					var dataColumn = new SerializedDataColumn(sourceDataColumn).ToDataColumn();
					table.Columns.Add(dataColumn);
					dataColumn.SetOrdinal(dataColumn.Ordinal);
				}
			}
		}

		dataSet.Tables.Add(table);
		return dataSet.Tables.Cast<DataTable>().ToArray();
	}

	IDbCommand IDbDataAdapter.SelectCommand
	{
		get { return selectCommand; }
		set
		{
			if (!disposing)
			{
				throw new ReadOnlyException("HttpDataAdapter SelectCommand cannot be set. It's readonly.");
			}
		}
	}
	readonly IDbCommand selectCommand;

	protected override void Dispose(bool disposing)
	{
		this.disposing = disposing;
	}

	bool disposing;

	#region Not Implemented Methods

	IDbCommand IDbDataAdapter.DeleteCommand
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}

	IDbCommand IDbDataAdapter.InsertCommand
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}

	IDbCommand IDbDataAdapter.UpdateCommand
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}

	IDataParameter[] IDataAdapter.GetFillParameters()
	{
		throw new NotImplementedException();
	}

	MissingMappingAction IDataAdapter.MissingMappingAction
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}

	MissingSchemaAction IDataAdapter.MissingSchemaAction
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}

	ITableMappingCollection IDataAdapter.TableMappings
	{
		get { throw new NotImplementedException(); }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
	int IDataAdapter.Update(DataSet dataSet)
	{
		throw new NotImplementedException();
	}

	#endregion
}
