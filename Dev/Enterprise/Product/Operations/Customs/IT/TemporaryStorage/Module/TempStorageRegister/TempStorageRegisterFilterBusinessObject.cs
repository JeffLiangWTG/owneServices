using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public class TempStorageRegisterFilterBusinessObject : EU.TemporaryStorage.Module.TempStorageRegisterFilterBusinessObject
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter constants")]
	public static class FilterConstants
	{
		public const string RegisterReference = "Register Reference";
		public const string BillMrn = "Bill MRN";
	}

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = base.GetModuleFiltersCore();
		FilterInflators.ForEach(inflator => inflator?.InflateFilter(filters));
		return filters;
	}

	protected override void AddReferenceFilter(ModuleFilterCollection filters)
	{
		var registerReferenceTextFilter = filters.AddTextFilter(FilterConstants.RegisterReference, CusTempStorageRegHeaderSchema.SRH_Reference);
		registerReferenceTextFilter.Category = FilterCategories.NumbersAndReferences;
		registerReferenceTextFilter.MultilingualDescription = ResString.GetMultilingualString("1E073742-DD87-440F-892D-3027F8AE40DF", FilterConstants.RegisterReference);
	}

	protected override void AddPreviousReferenceFilter(ModuleFilterCollection filters)
	{
		var billMrnTextFilter = filters.AddTextFilter(FilterConstants.BillMrn, CusTempStorageRegHeaderSchema.SRH_PreviousReference);
		billMrnTextFilter.Category = FilterCategories.NumbersAndReferences;
		billMrnTextFilter.MultilingualDescription = ResString.GetMultilingualString("CA4ECE65-471B-4FE7-B20D-D831306C3BEC", FilterConstants.BillMrn);
	}

	protected override void AddPreviousReferenceTypeFilter(ModuleFilterCollection filters)
	{
	}

	protected override void AddRemainingPackagesQuantityFilter(ModuleFilterCollection filters)
	{
	}

	protected override void AddPackageTypeFilter(ModuleFilterCollection filters)
	{
	}

	List<IFilterInflator> FilterInflators =>
	[
		new ContainerNumberFilterInflator(this),
		new GrossWeightTransactionFilterInflator(this),
		new JobReferenceTransactionFilterInflator(this),
		new PreviousDocumentTransactionFilterInflator(this),
		new PreviousDocumentTypeTransactionFilterInflator(this),
		new ReferenceTypeFilterInflator(this),
		new TransactionTypeFilterInflator(this)
	];
}
