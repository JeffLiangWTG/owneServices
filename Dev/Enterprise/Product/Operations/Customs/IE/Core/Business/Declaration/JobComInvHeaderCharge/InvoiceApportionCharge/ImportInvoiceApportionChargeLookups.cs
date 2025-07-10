using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class ImportInvoiceApportionChargeLookups : EU.Business.Declaration.InvoiceApportionChargeLookups
	{
		public ImportInvoiceApportionChargeLookups(InvoiceApportionCharge invoiceApportionCharge) : base(invoiceApportionCharge)
		{
		}

		public new InvoiceApportionCharge Parent => (InvoiceApportionCharge)base.Parent;

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<AISChargeCodeList>();
	}
}
