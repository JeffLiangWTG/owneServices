using System.Linq;
using Enterprise.Customs.EU.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class InvoiceChargeValidation : EU.Business.Declaration.InvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge parent) : base(parent)
		{
		}

		protected new InvoiceCharge Parent => (InvoiceCharge)base.Parent;

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			var chargeType = Parent.J7_ChargeType;
			var info = Parent.J7_ChargeTypeInfo;

			if (chargeType == UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge && Parent.Parent != null && Parent.Parent.IncoTerm != IncoTerms.DeliveredDutyPaid)
			{
				info.AddWarning(Res.GetString("3BA1E9DF-C4D5-4820-A294-F5BA0A96B2CF", "IDO charge only applies to INCO Term DDP."));
			}

			var invoiceHeader = (JobComInvoiceHeader)Parent?.Invoice;
			var decider = (IInvoiceHeaderValidationDecider)invoiceHeader?.Validation?.ValidationDecider;

			if (decider != null && decider.IsRuleNAT_237Active && invoiceHeader.Charges.Cast<InvoiceCharge>().Count(x => x.J7_ChargeType == chargeType) > 1)
			{
				info.AddMessageError(Res.GetString("35D22D9D-4E50-4F07-8E60-EE6985FE732A", "Rule Nat_237 - This charge type has already been entered at this level."));
			}

			if (decider != null && decider.IsRuleNAT_154Active && !CusEntryLineCalculatedFeeCollection.IsTransportCharge(chargeType) && invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().Any(y => y.Charges.Cast<InvoiceLineCharge>().Any(z => z.J7_ChargeType == chargeType)))
			{
				info.AddMessageError(Res.GetString("C4C5D91D-04CD-4F97-BE33-628435C57687", "Rule Nat_154 - This charge type has already been entered at Invoice Line level."));
			}
		}
	}
}
