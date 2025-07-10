using System;
using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public abstract class DataSaveWithColumns : DataSave
	{
		protected DataSaveWithColumns() { }

		protected override String[] ColumnHeadings()
		{
			return ColumnNames.ToArray();
		}

		List<String> ColumnNames
		{
			get { return columnNames ?? (columnNames = NewColumnNames()); }
		}
		List<String> columnNames;

		protected abstract List<String> NewColumnNames();
	}
}
