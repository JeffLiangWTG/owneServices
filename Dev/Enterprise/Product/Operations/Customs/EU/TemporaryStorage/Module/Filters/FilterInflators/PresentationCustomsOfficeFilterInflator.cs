using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public class PresentationCustomsOfficeFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	protected const string FilterDescription = "Presentation Customs Office";

	public PresentationCustomsOfficeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var presentationCustomsOfficeFilter = filterCollection.AddTextFilter(FilterDescription, CusCodeDataSchema.CY_Data);
		presentationCustomsOfficeFilter.SubGroup = new PresentationCustomsOfficeSubGroup();
		presentationCustomsOfficeFilter.Category = FilterCategories.Locations;
		presentationCustomsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("7742813C-35B2-4210-ACD5-009C83099BC1", FilterDescription);
	}
}
