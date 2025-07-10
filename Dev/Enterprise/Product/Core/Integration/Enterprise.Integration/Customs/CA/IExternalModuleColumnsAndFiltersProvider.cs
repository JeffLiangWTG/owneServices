namespace Enterprise.Integration
{
	using CargoWise.EntityFramework;
	using Enterprise.Integration.ZArchitecture;

	public static partial class Customs
	{
		public static partial class CA
		{
			public interface IShipmentModuleColumnsAndFiltersProvider : IExternalModuleColumnsAndFiltersProvider
			{
			}

			public interface IConsolModuleColumnsAndFiltersProvider : IExternalModuleColumnsAndFiltersProvider
			{
			}

			public interface IExternalModuleColumnsAndFiltersProvider
			{
				void AddFilters(IModuleFilterCollection filters, BusinessObjectFactory factory);
				void AddColumns(IFilterControl filterControl);
			}

			public interface ICFSShipmentModuleColumnsAndFiltersProvider : IExternalModuleColumnsAndFiltersProvider
			{
			}
		}
	}
}
