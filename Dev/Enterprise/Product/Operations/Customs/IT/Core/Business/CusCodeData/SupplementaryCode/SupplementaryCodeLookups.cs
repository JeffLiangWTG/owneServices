using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

sealed class SupplementaryCodeLookups : BaseSupplementaryCodeLookups
{
	public SupplementaryCodeLookups(BaseSupplementaryCode parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList CY_CodeList => Parent.Parent switch
	{
		JobComInvoiceLine invoiceLine => invoiceLine.Lookups.AdditionalCodesList,
		_ => base.CY_CodeList
	};
}
