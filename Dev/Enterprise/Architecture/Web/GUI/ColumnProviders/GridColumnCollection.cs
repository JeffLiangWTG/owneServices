using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI
{
	public class GridColumnCollection : List<DataGridColumn>
	{
		public List<string> Keys
		{
			get
			{
				List<string> result = new List<string>();
				foreach (var column in this)
				{
					result.Add(column.HeaderText);
				}
				return result;
			}
		}
		public DataGridColumn this[string columnName]
		{
			get
			{
				foreach (var column in this)
				{
					if (column.HeaderText == columnName)
					{
						return column;
					}
				}
				return null;
			}
		}

		public bool ContainsKey(string columnName)
		{
			return this[columnName] != null;
		}
	}
}
