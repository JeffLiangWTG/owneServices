using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class TableColumnCalculator
	{
		public TableColumn[] GetTableColumnsOnThisObject(BusinessObject bizO, ZGridColumns columns, bool includeAllColumns = false)
		{
			var result = new List<TableColumn>();
			for (var columnNo = 0; columnNo < columns.Count; columnNo++)
			{
				var columnInfo = columns[columnNo];
				if ((columnInfo.IsVisible || includeAllColumns) && columnInfo.ColumnStyle != null && !(columnInfo.ColumnStyle.PropertyDescriptor is IGuiOnlyPropertyDescriptor))
				{
					var columnStyle = columnInfo.ColumnStyle as ZGuidFindBoxColumnStyle;
					var columnName = columnInfo.ColumnStyle.MappingName;
					if (columnStyle != null && !columnName.Contains("+"))
					{
						var findBox = (ZGridFindBox)columnStyle.EditControl;
						findBox.DataPropertyName = columnStyle.MappingName;
						findBox.CurrentItem = bizO;
						findBox.PullList();
						var list = ((IFindBoxListProvider)columnStyle.EditControl).List;
						var tableName = (list != null) ? BusinessObjectFactory.GetTableNameFromType(list.TypeOfElements) : string.Empty;
						result.Add(new TableColumn(tableName, columnName));
					}
					else
					{
						result.Add(new TableColumn(string.Empty, columnName));
					}
				}
			}
			return result.ToArray();
		}
	}
}
