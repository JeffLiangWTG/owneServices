using System.Linq;
using CargoWise.EntityFramework;
using static Enterprise.Customs.FR.Business.UniversalReferenceDataHelper;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryHeaderValidation(CusEntryHeader parent) : AutoFRCusEntryHeaderValidation(parent)
	{
		public new CusEntryHeader Parent => (CusEntryHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCH_CalcTotalInvoicedAmountInLocalCurrency();
		}

		public void ValidateCH_CalcTotalInvoicedAmountInLocalCurrency()
		{
			ValidateCalculatedProperty(Parent.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo);
		}

		protected virtual void CheckCH_CalcTotalInvoicedAmountInLocalCurrency()
		{
			CheckRuleNAT_174();
			CheckRuleNAT_177();
			CheckRuleNAT_178();
			CheckRuleNAT_179();
			CheckRuleNAT_185();
			CheckRuleNAT_189();
		}

		void CheckRuleNAT_174()
		{
			var parent = Parent;
			if (parent.Validation.ValidationDecider is IEntryHeaderValidationDecider { IsRuleNAT_174Active: true } && parent.MergedLines.Cast<CusEntryLine>().All(x => x.HasNegligibleValueProcedure)
				&& UniversalReferenceDataHelper.CheckFeeValueInThreshold(parent.CH_CalcTotalInvoicedAmountInLocalCurrency, UniversalReferenceDataHelper.CountComparisonConstants.GreaterThan, FRConstants.ThresholdsAndLimits.NegligibleValueLimit, parent.Factory, parent.Declaration?.GetDefaultDataGroupingCode() ?? string.Empty, parent.EffectiveValuationDate, out decimal rule174, FeeValueToLookAt.Threshold))
			{
				parent.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo.AddMessageError(Res.GetString("E9A3F7CF-40F1-4A07-9CAB-1CB618109EDD", "[NAT_174] Total Invoiced Amount (EUR) should not be greater than {0} EUR when all items have C07 (negligible value) concession.", rule174));
			}
		}

		void CheckRuleNAT_185()
		{
			var parent = Parent;
			if (parent.Validation.ValidationDecider is IEntryHeaderValidationDecider { IsRuleNAT_185Active: true }
			&& UniversalReferenceDataHelper.CheckFeeValueInThreshold(parent.MergedLines.Where(x => x.IsPromotionalProductToDROM).Sum(x => x.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency), UniversalReferenceDataHelper.CountComparisonConstants.GreaterThan, FRConstants.ThresholdsAndLimits.PromotionalProductToDROMValueLimit, parent.Factory, parent.Declaration?.GetDefaultDataGroupingCode() ?? string.Empty, parent.EffectiveValuationDate, out decimal rule185, FeeValueToLookAt.Threshold))
			{
				parent.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo.AddMessageError(Res.GetString("C803D019-5949-42C1-9BCA-76DB005B6D59", "[NAT_185] Total Invoiced Amount (EUR) should not be greater than {0} EUR when CANA 0090(promotional product to DROM) is selected.", rule185));
			}
		}

		void CheckRuleNAT_177()
		{
			var parent = Parent;
			if (parent.Validation.ValidationDecider is IEntryHeaderValidationDecider { IsRuleNAT_177Active: true }
			&& UniversalReferenceDataHelper.CheckFeeValueInThreshold(parent.CH_CalcTotalInvoicedAmountInLocalCurrency, UniversalReferenceDataHelper.CountComparisonConstants.LessThanOrEqualTo, FRConstants.ThresholdsAndLimits.C2CValueProcedureRule177, parent.Factory, parent.Declaration?.GetDefaultDataGroupingCode() ?? string.Empty, parent.EffectiveValuationDate, out decimal rule177, FeeValueToLookAt.Threshold)
			&& parent.MergedLines.Cast<CusEntryLine>().Any(x => x.HasC2CProcedure) && parent.MergedLines.Cast<CusEntryLine>().Any(x => !x.HasC2CProcedure))
			{
				parent.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo.AddMessageError(Res.GetString("3FBA3BE6-845F-434C-A19D-685F848663F1", "[NAT_177] If one entry line has concession C08, and total invoice amount is lesser or equal to {0}, all other lines must have concession C08 also.", rule177));
			}
		}

		void CheckRuleNAT_178()
		{
			var parent = Parent;
			if (parent.Validation.ValidationDecider is IEntryHeaderValidationDecider { IsRuleNAT_178Active: true }
			&& UniversalReferenceDataHelper.CheckFeeValueInThreshold(parent.CH_CalcTotalInvoicedAmountInLocalCurrency, UniversalReferenceDataHelper.CountComparisonConstants.GreaterThan, FRConstants.ThresholdsAndLimits.C2CValueProcedureRule178, parent.Factory, parent.Declaration?.GetDefaultDataGroupingCode() ?? string.Empty, parent.EffectiveValuationDate, out decimal rule178minimum, FeeValueToLookAt.Minimum)
			&& UniversalReferenceDataHelper.CheckFeeValueInThreshold(parent.CH_CalcTotalInvoicedAmountInLocalCurrency, UniversalReferenceDataHelper.CountComparisonConstants.LessThanOrEqualTo, FRConstants.ThresholdsAndLimits.C2CValueProcedureRule178, parent.Factory, parent.Declaration?.GetDefaultDataGroupingCode() ?? string.Empty, parent.EffectiveValuationDate, out decimal rule178maximum, FeeValueToLookAt.Maximum)
			&& parent.MergedLines.Cast<CusEntryLine>().Any(x => x.HasC2CProcedure))
			{
				parent.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo.AddMessageError(Res.GetString("20C95973-EDD2-4FAA-85D3-E56B7EFFE44E", "[NAT_178] At least one entry line uses concession C08, but it is not allowed when total invoiced amount is greater than {0} and lesser or equal to {1} EUR.", rule178minimum, rule178maximum));
			}
		}

		void CheckRuleNAT_179()
		{
			var parent = Parent;
			if(parent.Validation.ValidationDecider is IEntryHeaderValidationDecider { IsRuleNAT_179Active: true }
			&& UniversalReferenceDataHelper.CheckFeeValueInThreshold(parent.CH_CalcTotalInvoicedAmountInLocalCurrency, UniversalReferenceDataHelper.CountComparisonConstants.GreaterThan, FRConstants.ThresholdsAndLimits.C2CValueProcedureRule179A, parent.Factory, parent.Declaration?.GetDefaultDataGroupingCode() ?? string.Empty, parent.EffectiveValuationDate, out decimal rule179A, FeeValueToLookAt.Threshold)
			&& UniversalReferenceDataHelper.CheckFeeValueInThreshold(parent.MergedLines.Where(x => x.HasC2CProcedure).Sum(x => x.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency), UniversalReferenceDataHelper.CountComparisonConstants.GreaterThan, FRConstants.ThresholdsAndLimits.C2CValueProcedureRule179B, parent.Factory, parent.Declaration?.GetDefaultDataGroupingCode() ?? string.Empty, parent.EffectiveValuationDate, out decimal rule179B, FeeValueToLookAt.Threshold))
			{
				parent.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo.AddMessageError(Res.GetString("E08990BF-D38C-4CA1-A556-A2C8F957CED6", "[NAT_179] Cumulative total invoice amount for concession C08 should not be greater than {0} EUR when total invoiced amount exceeds {1} EUR.", rule179B, rule179A));
			}
		}

		void CheckRuleNAT_189()
		{
			var parent = Parent;
			if (parent.Validation.ValidationDecider is IEntryHeaderValidationDecider { IsRuleNAT_189Active: true }
			&& UniversalReferenceDataHelper.CheckFeeValueInThreshold(parent.MergedLines.Where(x => x.IsProductOfNegligibleValueToDROM).Sum(x => x.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency), UniversalReferenceDataHelper.CountComparisonConstants.GreaterThan, FRConstants.ThresholdsAndLimits.ProductOfNegligibleValueToDROM, parent.Factory, parent.Declaration?.GetDefaultDataGroupingCode() ?? string.Empty, parent.EffectiveValuationDate, out decimal rule189, FeeValueToLookAt.Threshold))
			{
				parent.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo.AddMessageError(Res.GetString("0F89D984-F443-4071-A959-A3F0E7258A0E", "[NAT_189] Total Invoiced Amount (EUR) should not be greater than {0} EUR when CANA 0089(product of negligible value to DROM) is selected.", rule189));
			}
		}

		protected override void CheckCH_TriggeringPointForValidation()
		{
			base.CheckCH_TriggeringPointForValidation();
			ListValidation.ErrorIfInvalidCode(Parent.CH_TriggeringPointForValidationInfo);
		}
	}
}
