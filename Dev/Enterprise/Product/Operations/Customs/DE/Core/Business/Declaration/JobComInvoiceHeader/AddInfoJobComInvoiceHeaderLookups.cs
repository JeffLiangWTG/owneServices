using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class AddInfoJobComInvoiceHeaderLookups : EU.Business.Declaration.AddInfoJobComInvoiceHeaderLookups
	{
		public AddInfoJobComInvoiceHeaderLookups(EU.Business.Declaration.AddInfoJobComInvoiceHeader parent) : base(parent)
		{
		}

		public new AddInfoJobComInvoiceHeader Parent => (AddInfoJobComInvoiceHeader)base.Parent;

		public override CodeDescriptionPairList TransportChargesMethodOfPaymentList => Factory.GetCachedValue<DEExportMethodOfPaymentList>();
	}
}
