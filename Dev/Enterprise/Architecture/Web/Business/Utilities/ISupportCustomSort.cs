using System.Collections;
using System.ComponentModel;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	public interface ISupportCustomSorter
	{
		IComparer GetCustomSorter(ListSortDirection sortDirection);
		ZGuid CustomSortedColumnID { get; }
		string SortExpression { get; }
	}

	public static class CustomSorterSupportConst
	{
		public const string IsCustomSorter = "*CustommSorterColumn="; // Hard-coded constant
	}
}
