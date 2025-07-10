using System.Linq;
using Enterprise.Customs.EU.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class InvoiceLineChargeValidation : EU.Business.Declaration.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge parent) : base(parent)
		{
		}

		protected new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			var chargeType = Parent.J7_ChargeType;
			var info = Parent.J7_ChargeTypeInfo;

			if (chargeType == UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge && Parent.Parent != null && Parent.Parent.IncoTerm != IncoTerms.DeliveredDutyPaid)
			{
				info.AddWarning(Res.GetString("3BA1E9DF-C4D5-4820-A294-F5BA0A96B2CF", "IDO charge only applies to INCO Term DDP."));
			}

			var invoiceLine = Parent.InvoiceLine;
			var invoiceHeader = invoiceLine?.InvoiceHeader;

			if (((JobComInvoiceLine)invoiceLine)?.Validation?.ValidationDecider is IImportInvoiceLineValidationDecider { IsRuleNAT_237Active: true } && invoiceLine.Charges.Count(x => x.J7_ChargeType == chargeType) > 1)
			{
				info.AddMessageError(Res.GetString("3FAF2810-18E4-48F6-854E-0413E9EF8CCF", "Rule Nat_237 - This charge type has already been entered at this level."));
			}

			if (((JobComInvoiceLine)invoiceLine)?.Validation?.ValidationDecider is IImportInvoiceLineValidationDecider { IsRuleNAT_154Active: true } && !CusEntryLineCalculatedFeeCollection.IsTransportCharge(chargeType) && invoiceHeader != null && invoiceHeader.Charges.Any(x => x.J7_ChargeType == chargeType))
			{
				info.AddMessageError(Res.GetString("BF0B83A5-0CB1-45E6-9619-775568021FBC", "Rule Nat_154 - This charge type has already been entered at Invoice level."));
			}
		}
	}
}
