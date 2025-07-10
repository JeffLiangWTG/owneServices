using System;
using System.Linq;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CA.Business
{
	public static class LVXJobComInvoiceHeaderExtension
	{
		public static ZBool IsRemissionAll(this JobComInvoiceHeader invoiceHeader, string countryOfExport = "")
		{
			var coe = string.IsNullOrEmpty(countryOfExport) ? invoiceHeader.CA_RN_NKExport : new ZString(countryOfExport);
			var thresholdRT1 = UniversalReferenceConstants.GetCLVSThreshold(invoiceHeader.Factory, invoiceHeader.JZ_ValuationDateOverride.IsValid ? invoiceHeader.JZ_ValuationDateOverride : ZDateTime.Today, Universal.Constants.RefCusTaxOrFeeTypes.CACLVSRemissionThresholdAll);
			return !IsUSorMexico(coe) && CheckTotalVFDRequired(invoiceHeader.TotalValueForDuty, thresholdRT1);
		}

		public static ZBool IsRemissionMexicoAndUSDutyAndTax(this JobComInvoiceHeader invoiceHeader, string countryOfExport = "")
		{
			var coe = string.IsNullOrEmpty(countryOfExport) ? invoiceHeader.CA_RN_NKExport : new ZString(countryOfExport);
			var thresholdRT2 = UniversalReferenceConstants.GetCLVSThreshold(invoiceHeader.Factory, invoiceHeader.JZ_ValuationDateOverride.IsValid ? invoiceHeader.JZ_ValuationDateOverride : ZDateTime.Today, Universal.Constants.RefCusTaxOrFeeTypes.CACLVSRemissionThresholdDutyAndTax);
			return IsUSorMexico(coe) && CheckTotalVFDRequired(invoiceHeader.TotalValueForDuty, thresholdRT2);
		}

		public static ZBool IsRemissionMexicoAndUSDutyOnly(this JobComInvoiceHeader invoiceHeader, string countryOfExport = "")
		{
			var coe = string.IsNullOrEmpty(countryOfExport) ? invoiceHeader.CA_RN_NKExport : new ZString(countryOfExport);
			var thresholdRT3 = UniversalReferenceConstants.GetCLVSThreshold(invoiceHeader.Factory, invoiceHeader.JZ_ValuationDateOverride.IsValid ? invoiceHeader.JZ_ValuationDateOverride : ZDateTime.Today, Universal.Constants.RefCusTaxOrFeeTypes.CACLVSRemissionThresholdDutyOnly);
			return IsUSorMexico(coe) && CheckTotalVFDRequired(invoiceHeader.TotalValueForDuty, thresholdRT3);
		}

		static bool IsUSorMexico(string countryCode) => countryCode == Core.Constants.CountryCodes.UnitedStates || countryCode == Core.Constants.CountryCodes.Mexico;

		static ZBool CheckTotalVFDRequired(ZDecimal totalVFD, ZDecimal thresholdValue)
		{
			if (thresholdValue > 0)
			{
				return totalVFD <= thresholdValue;
			}
			return ZBool.False;
		}

		public static void ApplyCLVSRemissionThresholdAll(this JobComInvoiceHeader invoiceHeader) => invoiceHeader.ApplyCLVSRemission(RemissionAll);
		public static void ApplyCLVSRemissionMexicoAndUSDutyAndTax(this JobComInvoiceHeader invoiceHeader) => invoiceHeader.ApplyCLVSRemission(RemissionMexicoAndUSDutyAndTax, x => !x.IsSurtax);
		public static void ApplyCLVSRemissionMexicoAndUSDutyOnly(this JobComInvoiceHeader invoiceHeader) => invoiceHeader.ApplyCLVSRemission(RemissionMexicoAndUSDutyOnly, x => !x.IsTax && !x.IsSurtax);

		public static void ApplyCLVSRemission(this JobComInvoiceHeader invoiceHeader, ZString[] remissionParams, Func<DutyAndTax, bool> additionalCheck = null)
		{
			foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
			{
				invoiceLine.CA_CalculationMethod = remissionParams[0];
				invoiceLine.CA_AuthorityNumber = remissionParams[1];
				invoiceLine.CA_RemissionType = RemissionTypeList.Codes.OrderInCouncil;

				invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
				foreach (var dutyAndTax in invoiceLine.DutiesAndTaxes)
				{
					if (!dutyAndTax.C1_Override && (additionalCheck == null || additionalCheck(dutyAndTax)))
					{
						dutyAndTax.C1_Amount = ZDecimal.Zero;
					}
				}
				invoiceLine.DutyAndTaxManager.ClearCachedValues();
			}
		}

		public static bool CheckCurrentRemissionConditions(this JobComInvoiceHeader invoiceHeader, ZString[] conditions)
		{
			return invoiceHeader.JobComInvoiceLines.Cast<JobComInvoiceLine>().All(x => x.CA_CalculationMethod == conditions[0] && x.CA_AuthorityNumber == conditions[1] && x.CA_RemissionType == RemissionTypeList.Codes.OrderInCouncil);
		}

		[ThreadSafe]
		public static ZString[] RemissionAll = { CalculationMethods.Codes.RegularRemission, DutyAndTaxManager.TaxRemittedOICNumber1 };
		[ThreadSafe]
		public static ZString[] RemissionMexicoAndUSDutyAndTax = { CalculationMethods.Codes.RegularRemission, DutyAndTaxManager.TaxRemittedOICNumber2 };
		[ThreadSafe]
		public static ZString[] RemissionMexicoAndUSDutyOnly = { CalculationMethods.Codes.RegularRemission, DutyAndTaxManager.TaxRemittedOICNumber3 };
		[ThreadSafe]
		public static ZString[] NoRemission = { CalculationMethods.Codes.NoRemission, ZString.Empty };
	}
}
