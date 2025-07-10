using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.LandedCosting
{
	public class GbLandedCostingHelper : EuLandedCostingHelper
	{
		// TODO: Will be replaced with GenericLandedCostingConfig
		protected override DutyTaxEntryFee GetTotalDutyTaxEntryFeeItemsCore(Customs.Business.BaseJobDeclaration declaration)
		{
			var result = new DutyTaxEntryFee();
			foreach (CusEntryHeader entryHeader in declaration.ActiveEntryHeaders)
			{
				if (entryHeader != null)
				{
					result[CustomsDisbursementChargeCode.TotalDuty] = entryHeader.SumLineFees(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);  // A00
					result[CustomsDisbursementChargeCode.OtherOrFlatDutyAmount] = entryHeader.SumLineFees(UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts); // A10 - kept for historical reasons
					result[CustomsDisbursementChargeCode.SpecialTax1] = entryHeader.SumLineFees(UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge); // A20
					result[CustomsDisbursementChargeCode.SpecialTax2] = entryHeader.SumLineFees(UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty);// A30
					result[CustomsDisbursementChargeCode.SpecialTax3] = entryHeader.SumLineFees(UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty); // A40
				}
			}
			return result;
		}

		// TODO: Will be replaced with GenericLandedCostingConfig
		protected override DutyTaxEntryFee GetLineDutyTaxEntryFeeItemsCore(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var cusEntryLine = invoiceLine.CusEntryLine;
			var result = new DutyTaxEntryFee();
			if (cusEntryLine != null)
			{
				if (invoiceLine.Taxes.Count > 0)
				{
					result[CustomsDisbursementChargeCode.TotalDuty] = GetTaxAmount(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, invoiceLine); //A00
					result[CustomsDisbursementChargeCode.OtherOrFlatDutyAmount] = GetTaxAmount(UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts, invoiceLine); //A10. Can keep this as it allows landed cost to be run on older jobs.
					result[CustomsDisbursementChargeCode.SpecialTax1] = GetTaxAmount(UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge, invoiceLine); //A20
					result[CustomsDisbursementChargeCode.SpecialTax2] = GetTaxAmount(UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, invoiceLine); //A30
					result[CustomsDisbursementChargeCode.SpecialTax3] = GetTaxAmount(UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, invoiceLine); //A40
				}
				else
				{
					var totalDuty = invoiceLine.JI_Calc_DutyAmount;
					var ratio = cusEntryLine.DutyAmount > 0 ? (ZDecimal)(totalDuty / cusEntryLine.DutyAmount) : 1;
					if (cusEntryLine.ConfirmedFees.Count > 0)
					{
						result[CustomsDisbursementChargeCode.TotalDuty] = totalDuty; //A00
						var a30 = GetTaxAmountConfirmedFees(UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, cusEntryLine); //A30
						var a80 = GetTaxAmountConfirmedFees(GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveAntiDumpingDuty, cusEntryLine); //A80
						result[CustomsDisbursementChargeCode.SpecialTax2] = (a30 + a80) * ratio;
						var a40 = GetTaxAmountConfirmedFees(UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, cusEntryLine); //A40
						var a90 = GetTaxAmountConfirmedFees(GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveCountervailingDuty, cusEntryLine); //A90
						result[CustomsDisbursementChargeCode.SpecialTax3] = (a40 + a90) * ratio;
					}
					else if (cusEntryLine.Fees.Count > 0)
					{
						result[CustomsDisbursementChargeCode.TotalDuty] = totalDuty; //A00
						var a30 = GetTaxAmountCalculatedFees(UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, cusEntryLine); //A30
						var a80 = GetTaxAmountCalculatedFees(GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveAntiDumpingDuty, cusEntryLine); //A80
						result[CustomsDisbursementChargeCode.SpecialTax2] = (a30 + a80) * ratio;
						var a40 = GetTaxAmountCalculatedFees(UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, cusEntryLine); //A40
						var a90 = GetTaxAmountCalculatedFees(GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveCountervailingDuty, cusEntryLine); //A90
						result[CustomsDisbursementChargeCode.SpecialTax3] = (a40 + a90) * ratio;
					}
				}
			}
			return result;
		}

		ZDecimal GetTaxAmount(string taxCode, JobComInvoiceLine invoiceLine)
		{
			foreach (EU.Business.Declaration.JobComInvoiceLineTax tax in invoiceLine.Taxes)
			{
				if (tax.Data.G4_Type == taxCode)
				{
					return tax.CalcAmountInDeclarationCurrency;
				}
			}
			return 0m;
		}

		ZDecimal GetTaxAmountConfirmedFees(string taxCode, CusEntryLine cusEntryLine)
		{
			return cusEntryLine.ConfirmedFees?.Cast<CusEntryLineFee>().Where(cf => cf.CF_ChargeType == taxCode)?.Select(cf => cf.CF_ChargeAmount).Sum(ca => ca) ?? 0m;
		}

		ZDecimal GetTaxAmountCalculatedFees(string taxCode, CusEntryLine cusEntryLine)
		{
			return cusEntryLine.Fees?.Cast<CusEntryLineFee>().Where(cf => cf.CF_ChargeType == taxCode).Select(cf => cf.CF_ChargeAmount).Sum(ca => ca) ?? 0m;
		}
	}
}
