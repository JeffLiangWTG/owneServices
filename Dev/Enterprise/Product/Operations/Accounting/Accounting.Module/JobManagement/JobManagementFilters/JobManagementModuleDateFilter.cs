using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public class JobManagementModuleDateFilter : ModuleDateFilter
	{
		#region Construction

		public JobManagementModuleDateFilter(ZString description, GetDateQuery queryDelegate)
			: base(description, queryDelegate, false)
		{
		}

		public JobManagementModuleDateFilter(ZString description, SchemaDateTimeColumn filterColumn)
			: base(description, filterColumn, false)
		{
		}

		#endregion

		#region Overridden Methods

		protected override DateRangePairList CreatePropertySearch_ListCore()
		{
			DateRangePairList dateRangePairList = base.CreatePropertySearch_ListCore();
			dateRangePairList.RemoveCode(HasNoDateEntered);
			return dateRangePairList;
		}

		#endregion
	}
}
