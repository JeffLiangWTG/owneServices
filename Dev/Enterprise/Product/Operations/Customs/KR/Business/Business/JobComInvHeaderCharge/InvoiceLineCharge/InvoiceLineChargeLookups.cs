using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class InvoiceLineChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceLineChargeLookups(Customs.Business.BaseJobComInvHeaderCharge charge)
			: base(charge)
		{
		}

		public new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;

		public override CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				var result = base.ChargeTypeList;
				var invoice = Parent.InvoiceLine?.InvoiceHeader as JobComInvoiceHeader;
				if (invoice?.IsImport ?? false)
				{
					var valuationCode = invoice.JZ_ValuationCode;
					if (!valuationCode.IsEmpty)
					{
						result = Factory.GetCachedValue("KRInvoiceLineChargeLookups.ChargeTypeList" + valuationCode, () => invoice.GetChargeTypeList(Factory));
					}
				}
				return result;
			}
		}
	}
}
