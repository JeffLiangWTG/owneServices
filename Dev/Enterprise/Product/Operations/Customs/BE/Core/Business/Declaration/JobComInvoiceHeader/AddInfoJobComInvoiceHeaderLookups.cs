using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class AddInfoJobComInvoiceHeaderLookups : EU.Business.Declaration.AddInfoJobComInvoiceHeaderLookups
{
	public AddInfoJobComInvoiceHeaderLookups(AddInfoJobComInvoiceHeader parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList TransportChargesMethodOfPaymentList => Factory.GetCachedValue<ExportMethodOfPaymentList>();
}
