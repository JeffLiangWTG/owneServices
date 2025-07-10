using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusEntryLineFeeValidation : EUUniversalCusEntryLineFeeValidation
{
	public CusEntryLineFeeValidation(AutoCusEntryLineFee parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateMethodOfPayment();
	}

	public new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

	protected override void CheckCF_ChargeType()
	{
		base.CheckCF_ChargeType();
		if (Parent.EntryLine.Declaration is JobDeclaration declaration && declaration.IsImport)
		{
			var invoiceLines = Parent.EntryLine.InvoiceLines.Cast<JobComInvoiceLine>();
			var chargeType = Parent.CF_ChargeType;
			if (chargeType.IsEmpty
				&& !invoiceLines.Any(x => x.HasAtLeastOneRelatedIndicatorChecked))
			{
				Parent.CF_ChargeTypeInfo.AddMessageError(Res.GetString("D43D36B6-06EA-4C32-BF00-F50F33CDC87E", "Tax type is required"));
			}

			if (Parent.CF_BaseValue.IsEmpty
				&& (chargeType.Equals(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts)
					|| chargeType.Equals(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat)))
			{
				Parent.CF_ChargeTypeInfo.AddMessageError(Res.GetString("AF2642D2-8BA3-4284-A94D-84DA4CB59BF4", "Base Amount is required"));
			}

			if (chargeType.Equals(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts)
				&& !invoiceLines.Any(x => x.HasProcedureStartingWithAny(NLConstants.ProcedureCodes._51, NLConstants.ProcedureCodes._53))
				&& invoiceLines.Any(x => x.HasAtLeastOneRelatedIndicatorChecked))
			{
				Parent.CF_ChargeTypeInfo.AddWarning(Res.GetString("3CB921E2-3BBB-47D9-ABC9-A204B00F9BB3", "Tax type A00 is not allowed for this procedure"));
			}
		}
	}

	public void ValidateMethodOfPayment()
	{
		foreach (var invoiceLine in Parent.EntryLine.InvoiceLines.Cast<JobComInvoiceLine>())
		{
			if (invoiceLine.EntryInstruction != null)
			{
				CusAuthorizationUsageValidationHelper.CheckAgcCodeMOP(invoiceLine.EntryInstruction, Parent, invoiceLine.EntryInstruction.AllAuthorizationUsages());
			}
		}
	}
}
