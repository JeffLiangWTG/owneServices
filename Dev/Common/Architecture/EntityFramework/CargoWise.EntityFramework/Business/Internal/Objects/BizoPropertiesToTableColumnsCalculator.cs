using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CargoWise.EntityFramework
{
	public static class BizoPropertiesToTableColumnsCalculator
	{
		public static TableColumn[] GetBusinessObjectTableColumns(params PropertyDescriptor[] propertyDescriptors)
		{
			if (propertyDescriptors != null)
			{
				List<TableColumn> tableColumns = new List<TableColumn>(propertyDescriptors.Length);

				foreach (PropertyDescriptor propertyDescriptor in propertyDescriptors.Where(x => x != null))
				{
					tableColumns.Add(new TableColumn("", propertyDescriptor.Name));
				}
				return tableColumns.ToArray();
			}
			return new List<TableColumn>().ToArray();
		}
	}
}
