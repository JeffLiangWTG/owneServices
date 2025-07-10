using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class ImportInvoiceChargeLookups : EU.Business.Declaration.InvoiceChargeLookups
	{
		public ImportInvoiceChargeLookups(InvoiceCharge invoiceCharge) : base(invoiceCharge)
		{
		}

		public new InvoiceCharge Parent => (InvoiceCharge)base.Parent;

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<AISChargeCodeList>();
	}
}
