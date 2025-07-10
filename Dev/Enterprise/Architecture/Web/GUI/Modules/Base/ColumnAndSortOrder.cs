using System.ComponentModel;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class ColumnAndSortOrder
	{
		public ColumnAndSortOrder(string orderByColumnName, ListSortDirection sortDirection)
		{
			OrderByColumnName = orderByColumnName ?? string.Empty;
			SortDirection = sortDirection;
		}

		public string OrderByColumnName { get; private set; }

		public ListSortDirection SortDirection { get; private set; }
	}
}
