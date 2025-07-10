using CargoWise.EntityFramework;
using Enterprise.Integration.ZArchitecture;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IForwardingConsolModuleCustomColumnsAndFiltersProvider
		{
			void AddColumns(IGridControl zGrid);
			void AddFilters(IModuleFilterCollection filters, BusinessObjectFactory factory, IBusiness filterBusinessObject = null);
		}
	}
}
