using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class InvoiceChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceChargeLookups(Customs.Business.BaseJobComInvHeaderCharge charge)
			: base(charge)
		{
		}

		public new InvoiceCharge Parent => (InvoiceCharge)base.Parent;

		public override CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				var result = base.ChargeTypeList;
				var invoice = Parent.Invoice as JobComInvoiceHeader;
				if (invoice?.IsImport ?? false)
				{
					var valuationCode = invoice.JZ_ValuationCode;
					if (!valuationCode.IsEmpty)
					{
						result = Factory.GetCachedValue("KRInvoiceChargeLookups.ChargeTypeList" + valuationCode, () => invoice.GetChargeTypeList(Factory));
					}
				}
				return result;
			}
		}
	}
}
