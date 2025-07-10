using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class EuCustomsValuationCalculator : CustomsValuationCalculator
	{
		public EuCustomsValuationCalculator(IChargeApportionee chargeApportionee) : base(chargeApportionee)
		{
		}
		/// <summary>
		/// Always in local/customs currency
		/// </summary>
		public ZDecimal GetEuCifAmount(RefCurrency invoiceCurrency, ZDecimal customsValueInInvoiceCurrency, IncoTermAndCustomsChargeFactory chargesFactory)
		{
			return GetEuCifAmountCore(invoiceCurrency, customsValueInInvoiceCurrency, chargesFactory);
		}

		protected virtual ZDecimal GetEuCifAmountCore(RefCurrency invoiceCurrency, ZDecimal customsValueInInvoiceCurrency, IncoTermAndCustomsChargeFactory chargesFactory)
		{
			// BP says: CIF Value = Customs Value + Freight costs After EU border (non-dutiable freight amounts) + Insurance after the border (non-dutiable insurance amounts - I think the practice is that there is normally none).
			// DJC says: we do not have a way to differentiate insurance before vs after the border. So for now assume none. Assume CIF = customs value + freight costs after EU border.
			var fab = GetFreightAfterEuBorderCode(chargesFactory);
			return
				customsValueInInvoiceCurrency +
				ChargeApportionee.Charges.GetCharge(fab, invoiceCurrency, FuncToFurtherFilterChargesAfterEUBorder) +
				ChargeApportionee.ApportionedCharges.GetCharge(fab, invoiceCurrency, FuncToFurtherFilterChargesAfterEUBorder);
		}

		protected virtual string GetFreightAfterEuBorderCode(IncoTermAndCustomsChargeFactory chargesFactory)
		{
			return ((EUIncoTermAndCustomsChargeFactory)chargesFactory).FreightAfterEUBorderCode;
		}

		protected virtual Func<JobComInvCharge, bool> FuncToFurtherFilterChargesAfterEUBorder => x => !x.J7_IsDutiable;
	}
}
