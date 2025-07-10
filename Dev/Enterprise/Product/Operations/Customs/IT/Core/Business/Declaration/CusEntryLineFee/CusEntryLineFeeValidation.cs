using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class CusEntryLineFeeValidation : EUUniversalCusEntryLineFeeValidation
{
	public CusEntryLineFeeValidation(AutoCusEntryLineFee parent) : base(parent)
	{
	}

	public new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

	protected override void CheckCF_ChargeType()
	{
		base.CheckCF_ChargeType();
		var chargeType = Parent.CF_ChargeType;
		if (chargeType == UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatCode)
		{
			Parent.CF_ChargeTypeInfo.AddError(ValidationCaptions.CusEntryLineFee.DoNotUse405Code);
		}
		else if (chargeType == UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406)
		{
			CheckDOISupportingDocumentIsAdded();
		}
		else if (chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat)
		{
			CheckVatExemptionFeeIsAdded();
		}
		else if (Parent.IsPortTax)
		{
			CheckDuplicatedPortTax();
		}
	}

	protected override void CheckCF_MethodOfCalculation()
	{
		base.CheckCF_MethodOfCalculation();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CF_MethodOfCalculationInfo);
	}

	protected override void CheckCF_Rate()
	{
		base.CheckCF_Rate();

		var parent = Parent;

		if (parent.IsVat
			&& !parent.CF_Rate.IsEmpty
			&& EntryLine is CusEntryLine entryLine
			&& entryLine.Declaration is JobDeclaration declaration
			&& declaration.IsImport
			&& entryLine.Header?.EntryInstruction is CusEntryInstruction instruction
			&& (!instruction.RandomProcedure?.IsCalculateVAT ?? false))
		{
			parent.CF_RateInfo.AddWarning(ValidationCaptions.CusEntryLineFee.TaxRateShouldBeZero(instruction.CEI_Procedure));
		}
	}

	protected override void CheckCF_MethodOfPayment()
	{
		base.CheckCF_MethodOfPayment();

		MandatoryValidation.MessageErrorIfNotEntered(Parent.CF_MethodOfPaymentInfo);
		new CusEntryLineFeeMopValidator(Parent).CheckMethodOfPayment();
	}

	protected override void CheckCF_ChargeAmount()
	{
		base.CheckCF_ChargeAmount();
		CheckEnteredTotalAmountAgainstSystemCalculatedTotalAmount();
	}

	protected override void CheckCF_BaseValue()
	{
		if (CanIgnoreBaseValueMandatoryValidation())
		{
			return;
		}

		base.CheckCF_BaseValue();
	}

	protected override void CheckBaseValuePrecision(ZPropertyInfo baseValueInfo)
	{
		if (Parent.CF_MethodOfCalculation == Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage)
		{
			base.CheckBaseValuePrecision(baseValueInfo);
		}
	}

	#region Implementation

	void CheckDuplicatedPortTax()
	{
		var entryLine = EntryLine;

		if (entryLine != null && entryLine.Declaration != null)
		{
			var declaration = entryLine.Declaration;

			if (!declaration.IsSea)
			{
				Parent.CF_ChargeTypeInfo.AddMessageError(ValidationCaptions.CusEntryLineFee.PortTaxShouldNotBeEntered);
			}
			else if (HasMorePortTaxes())
			{
				Parent.CF_ChargeTypeInfo.AddMessageError(ValidationCaptions.CusEntryLineFee.PortTaxCanOnlyBeEnteredOnce);
			}
		}

		bool HasMorePortTaxes() => entryLine
			.Fees.Cast<CusEntryLineFee>()
			.Where(x => x.PK != Parent.PK)
			.Any(x => x.IsPortTax);
	}

	void CheckVatExemptionFeeIsAdded()
	{
		var entryLine = EntryLine;
		if (entryLine != null && entryLine.RequiresVATExemption && entryLine.Fees.GetElementWithThisCode(UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406) == null)
		{
			Parent.CF_ChargeTypeInfo.AddMessageError(ValidationCaptions.CusEntryLineFee.DeclarationOfIntentSupportingDocumentRequiresVatExemptionFee);
		}
	}

	void CheckDOISupportingDocumentIsAdded()
	{
		var entryLine = EntryLine;
		if (entryLine != null && !entryLine.RequiresVATExemption)
		{
			Parent.CF_ChargeTypeInfo.AddMessageError(ValidationCaptions.CusEntryLineFee.VatExemptionFeeRequiresDeclarationOfIntentSupportingDocument);
		}
	}

	void CheckEnteredTotalAmountAgainstSystemCalculatedTotalAmount()
	{
		if (Parent.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Additional || Parent.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Override)
		{
			var chargeAmountCalculator = Parent.ChargeAmountRefresher.GetNewChargeAmountCalculator();
			var effectiveSystemCalculatedValue = Parent.ChargeAmountRounder.Round(chargeAmountCalculator.Calculate());
			if (effectiveSystemCalculatedValue != Parent.CF_ChargeAmount)
			{
				Parent.CF_ChargeAmountInfo.AddWarning(ValidationCaptions.CusEntryLineFee.TotalAmountIsDifferentFromSystemCalculatedAmount(effectiveSystemCalculatedValue));
			}
		}
	}

	bool CanIgnoreBaseValueMandatoryValidation()
	{
		var parent = Parent;
		var declaration = EntryLine?.Declaration;
		return declaration != null
				&& (declaration.IsImport || declaration.IsExport)
				&& parent.IsActionExclude;
	}

	CusEntryLine EntryLine => Parent.EntryLine;

	#endregion
}
