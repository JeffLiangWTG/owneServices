using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using IExternalModuleColumnsAndFiltersProvider = Enterprise.Integration.Customs.CA.IExternalModuleColumnsAndFiltersProvider;
using IFilterControl = Enterprise.Integration.ZArchitecture.IFilterControl;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public abstract class CAExternalModuleColumnsAndFiltersProvider : IExternalModuleColumnsAndFiltersProvider
	{
		#region Implementation of IExternalModuleColumnsAndFiltersProvider

		public void AddFilters(IModuleFilterCollection filters, BusinessObjectFactory factory)
		{
			foreach (var provider in ColumnsAndFiltersProviders)
			{
				provider.AddFilters(filters, factory);
			}
		}

		public void AddColumns(IFilterControl filterControl)
		{
			foreach (var provider in ColumnsAndFiltersProviders)
			{
				provider.AddColumns(filterControl);
			}
		}

		IEnumerable<IExternalModuleColumnsAndFiltersProvider> ColumnsAndFiltersProviders
		{
			get { return columnsAndFiltersProviders ?? (columnsAndFiltersProviders = GetColumnsAndFiltersProviders()); }
		}
		IEnumerable<IExternalModuleColumnsAndFiltersProvider> columnsAndFiltersProviders;

		protected abstract IEnumerable<IExternalModuleColumnsAndFiltersProvider> GetColumnsAndFiltersProviders();

		#endregion

		#region EqualModuleStatusFilter

		public class EqualModuleStatusFilter : ModuleTextFilter
		{
			public EqualModuleStatusFilter(string filterName, MultilingualString description, GetTextQueryWithOperator queryDelegate, GetList listDelegate)
				: base(filterName, queryDelegate, listDelegate)
			{
				MultilingualDescription = description;
			}

			protected override FilterCategory DefaultCategory
			{
				get { return FilterCategories.StatusAndFlags; }
			}

			public override IReadOnlyList<string> AllowedComparisonOperators
			{
				get { return new[] { string.Empty, ComparisonConstants.Exact, ComparisonConstants.NotEqual }; }
			}
		}

		#endregion

		#region EqualAndBlankModuleStatusFilter

		public class EqualAndBlankModuleStatusFilter : ModuleTextFilter
		{
			public EqualAndBlankModuleStatusFilter(string filterName, MultilingualString description, GetTextQueryWithOperator queryDelegate, GetList listDelegate)
				: base(filterName, queryDelegate, listDelegate)
			{
				MultilingualDescription = description;
			}

			protected override FilterCategory DefaultCategory
			{
				get { return FilterCategories.StatusAndFlags; }
			}

			public override IReadOnlyList<string> AllowedComparisonOperators
			{
				get { return new[] { string.Empty, ComparisonConstants.Exact, ComparisonConstants.NotEqual, ComparisonConstants.IsBlank, ComparisonConstants.IsNotBlank }; }
			}
		}

		#endregion
	}
}
