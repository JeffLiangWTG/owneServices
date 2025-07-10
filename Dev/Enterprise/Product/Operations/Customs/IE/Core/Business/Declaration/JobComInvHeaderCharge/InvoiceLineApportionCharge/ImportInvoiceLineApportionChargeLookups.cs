using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class ImportInvoiceLineApportionChargeLookups : EU.Business.Declaration.InvoiceLineApportionChargeLookups
	{
		public ImportInvoiceLineApportionChargeLookups(InvoiceLineApportionCharge invoiceLineApportionCharge) : base(invoiceLineApportionCharge)
		{
		}

		public new InvoiceLineApportionCharge Parent => (InvoiceLineApportionCharge)base.Parent;

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<AISChargeCodeList>();
	}
}
