using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Module;

public sealed class CustomerReferenceFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Customer Reference";

	public CustomerReferenceFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var customerReferenceFilter = filterCollection.AddTextFilter(FilterDescription, CusTempStorageJobHeaderSchema.SJH_ReferenceNumber);
		customerReferenceFilter.Category = FilterCategories.NumbersAndReferences;
		customerReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("17F0AC4D-FEB3-4E5C-8471-50054145AE0F", FilterDescription);
	}
}
