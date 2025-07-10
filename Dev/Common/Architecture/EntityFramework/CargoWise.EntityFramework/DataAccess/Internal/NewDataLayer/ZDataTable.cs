using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class ZDataTable : DataTable
	{
		public ZDataTable()
			: this("")
		{
		}

		protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
		{
			return new ZDataRow(builder);
		}

		public ZDataTable(string tableName)
			: base(tableName)
		{
			Columns.CollectionChanged += OnColumns_CollectionChanged;
		}

		#region DataColumnCache

		internal DataColumn GetDataColumnFromSchemaColumn(SchemaColumn schemaColumn)
		{
			if (schemaColumn == null)
			{
				throw new ArgumentNullException(nameof(schemaColumn));
			}

			if (schemaColumn.Ordinal < DataColumnFromSchemaColumnCache.Length)
			{
				DataColumn column = DataColumnFromSchemaColumnCache[schemaColumn.Ordinal];
				if (column == null)
				{
					column = Columns[schemaColumn.Name];
					DataColumnFromSchemaColumnCache[schemaColumn.Ordinal] = column;
				}
				return column;
			}
			else
			{
				ErrorReporter.ReportOnce($"DataColumnFromSchemaColumnCache Capacity less then schemaColumn.Ordinal.", @$"DataColumnFromSchemaColumnCacheCapacity: {DataColumnFromSchemaColumnCacheCapacity}
schemaColumn.Ordinal: {schemaColumn.Ordinal}
Columns.Count: {Columns.Count}
");
				return Columns[schemaColumn.Name];
			}
		}

		void OnColumns_CollectionChanged(object sender, CollectionChangeEventArgs e)
		{
			DataColumnFromSchemaColumnCache = new DataColumn[GetDataColumnFromSchemaColumnCacheCapacity()];
		}

		DataColumn[] DataColumnFromSchemaColumnCache;
		internal int DataColumnFromSchemaColumnCacheCapacity;
		int GetDataColumnFromSchemaColumnCacheCapacity()
		{
			if (DataColumnFromSchemaColumnCacheCapacity == 0)
			{
				return Columns.Count;
			}
			return DataColumnFromSchemaColumnCacheCapacity;
		}

		#endregion

		public DataRow GetRowIncludingDeleted(Guid pK)
		{
			DataRow result;
			if (!deletedRows.TryGetValue(pK, out result))
			{
				try
				{
					result = Rows.Find(pK);
				}
				catch (MissingPrimaryKeyException)
				{
					DataRow[] rows = Select(Columns[0].ColumnName + " = '" + pK.ToString() + "'");
					if (rows.Length > 0)
					{
						result = rows[0];
					}
				}
			}
			return result;
		}

		readonly Dictionary<DataRow, Guid> rowsDeleting = new Dictionary<DataRow, Guid>();

		protected override void OnRowDeleting(DataRowChangeEventArgs e)
		{
			if (PrimaryKey.Length > 0)
			{
				rowsDeleting[e.Row] = (Guid)e.Row[PrimaryKey[0]];
			}
			base.OnRowDeleting(e);
		}

		protected override void OnRowDeleted(DataRowChangeEventArgs e)
		{
			base.OnRowDeleted(e);
			Guid pK;
			if (rowsDeleting.TryGetValue(e.Row, out pK))
			{
				deletedRows[pK] = e.Row;
				rowsDeleting.Remove(e.Row);
			}
		}

		public bool HasDeletedRowWithPK(Guid pk)
		{
			return deletedRows.ContainsKey(pk);
		}

		readonly Dictionary<Guid, DataRow> deletedRows = new Dictionary<Guid, DataRow>();
	}
}
