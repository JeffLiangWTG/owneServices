using System.Collections.Generic;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class TableColumnCalculator
	{
		public TableColumn[] GetTableColumnsOnThisObject(BusinessObject bizObj, DataGridColumn[] columns)
		{
			List<TableColumn> result = new List<TableColumn>();
			for (int i = 0; i < columns.Length; i++)
			{
				ZTemplateColumn column = columns[i] as ZTemplateColumn;
				if (column != null && column.Visible)
				{
					IBindToListSupport listColumn = column as IBindToListSupport;
					if (listColumn != null &&
						!string.IsNullOrEmpty(listColumn.BindToList) &&
						!column.BindTo.Contains("+") &&
						!column.BindTo.Contains(".") &&
						!listColumn.BindToList.Contains("+") &&
						!listColumn.BindToList.Contains("."))
					{
						IBusinessObjectCollection list = bizObj[((IBindToListSupport)column).BindToList] as IBusinessObjectCollection;
						string tableName = (list != null) ? BusinessObjectFactory.GetTableNameFromType(list.TypeOfElements) : string.Empty;
						result.Add(new TableColumn(tableName, column.BindTo));
					}
					else
					{
						result.Add(new TableColumn(string.Empty, column.BindTo));
					}
				}
			}
			return result.ToArray();
		}
	}
}
