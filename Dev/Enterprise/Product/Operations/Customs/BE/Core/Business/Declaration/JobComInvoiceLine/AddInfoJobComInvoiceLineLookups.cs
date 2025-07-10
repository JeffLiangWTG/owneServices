using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BE.Business.Declaration;

public class AddInfoJobComInvoiceLineLookups : EU.Business.Declaration.AddInfoJobComInvoiceLineLookups
{
	public AddInfoJobComInvoiceLineLookups(AddInfoJobComInvoiceLine parent)
		: base(parent)
	{
	}

	protected override System.Collections.ICollection RegionOfDestinationListCore => Factory.GetCachedValue<BERegionList>();

	public ZZRefCusCodeListCombinedCollection CountryOfDestinationList
	{
		get
		{
			var invoiceLine = Parent.Parent;
			var codeList = invoiceLine.IsExport ? UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_EX17 : UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_IM17;
			var resultCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, invoiceLine.GetDefaultDataGroupingCode(), codeList, ZDateTime.Today);
			return resultCollection;
		}
	}
}
