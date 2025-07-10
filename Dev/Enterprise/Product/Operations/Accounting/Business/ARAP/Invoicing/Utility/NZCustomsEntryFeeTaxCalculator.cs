using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	/// <summary>
	/// please refer to WI00099179 for more details.
	/// </summary>
	public static class NZCustomsEntryFeeTaxCalculator
	{
		public static ZDecimal GetEntryFeeGST(ZDecimal entryFeeAmount, ZDecimal defaultEntryFeeGST)
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand
				&& Math.Abs(entryFeeAmount) == EntryFeeAmount_Year2015 && Math.Abs(defaultEntryFeeGST.Round(2)) == IncorrectEntryFeeGST_Year2015)
			{
				return CorrectEntryFeeGST_Year2015 * Math.Sign(defaultEntryFeeGST);
			}
			else
			{
				return defaultEntryFeeGST;
			}
		}

		public static bool IsEntryFeeChargeWithCorrectAmount(InvoicingLineBase invoicingLineBase)
		{
			var result = false;
			if (invoicingLineBase != null && invoicingLineBase.AL_AC.IsValid && invoicingLineBase.AL_OSGSTAmount == invoicingLineBase.AL_OSTaxAmount)
			{
				var entryFeeChargeCodePK = GetEntryFeeChargeCodePKIfApplicable(invoicingLineBase.AL_RX_NKTransactionCurrency, invoicingLineBase.AL_OSExTaxAmount, invoicingLineBase.Company.GC_RN_NKCountryCode, invoicingLineBase.Company.PK);
				result = IsGSTCorrect(entryFeeChargeCodePK, invoicingLineBase.AL_AC, invoicingLineBase.AL_OSExTaxAmount, invoicingLineBase.AL_OSGSTAmount);
			}
			return result;
		}

		public static bool IsEntryFeeRevenueChargeWithCorrectAmount(BaseCharge charge)
		{
			var result = false;
			if (charge != null && charge.JR_AC.IsValid)
			{
				var entryFeeChargeCodePK = GetEntryFeeChargeCodePKIfApplicable(charge.JR_SellCurrency, charge.JR_OSSellAmt, charge.Company.GC_RN_NKCountryCode, charge.Company.PK);
				result = IsGSTCorrect(entryFeeChargeCodePK, charge.JR_AC, charge.JR_OSSellAmt, charge.JR_OSSellGSTAmt_Calc);
			}
			return result;
		}

		public static void UpdateEntryFeeGST(InvoicingLineBase invoicingLineBase)
		{
			if (invoicingLineBase != null && invoicingLineBase.AL_AC.IsValid && invoicingLineBase.AL_OSGSTAmount == invoicingLineBase.AL_OSTaxAmount)
			{
				var entryFeeChargeCodePK = GetEntryFeeChargeCodePKIfApplicable(invoicingLineBase.AL_RX_NKTransactionCurrency, invoicingLineBase.AL_OSExTaxAmount, invoicingLineBase.Company.GC_RN_NKCountryCode, invoicingLineBase.Company.PK);
				if (IsGSTCorrectionRequired_Year2015(entryFeeChargeCodePK, invoicingLineBase.AL_AC, invoicingLineBase.AL_OSExTaxAmount, invoicingLineBase.AL_OSGSTAmount))
				{
					invoicingLineBase.AL_OSTaxAmount = CorrectEntryFeeGST_Year2015;
					invoicingLineBase.AL_OSGSTAmount = CorrectEntryFeeGST_Year2015;
				}
			}
		}

		public static ZDecimal GetEntryFeeGSTForRevenueCharge(ZGuid chargeCodePK, ZString sellCurrency, ZDecimal osSellAmount, ZDecimal currentTaxAmount, string countryCode, ZGuid companyPK)
		{
			if (chargeCodePK.IsValid)
			{
				var entryFeeChargeCodePK = GetEntryFeeChargeCodePKIfApplicable(sellCurrency, osSellAmount, countryCode, companyPK);
				if (IsGSTCorrectionRequired_Year2015(entryFeeChargeCodePK, chargeCodePK, osSellAmount, currentTaxAmount))
				{
					return CorrectEntryFeeGST_Year2015;
				}
			}

			return currentTaxAmount;
		}

		public static ZDecimal GetEntryFeeGSTForCostCharge(ZGuid chargeCodePK, ZGuid jr_E6,  ZString costCurrency, ZDecimal osCostAmount, ZDecimal currentTaxAmount, string countryCode, ZGuid companyPK)
		{
			ZDecimal result = currentTaxAmount;
			if (chargeCodePK.IsValid && !jr_E6.IsValid)
			{
				var entryFeeChargeCodePK = GetEntryFeeChargeCodePKIfApplicable(costCurrency, osCostAmount, countryCode, companyPK);
				if (IsGSTCorrectionRequired_Year2015(entryFeeChargeCodePK, chargeCodePK, osCostAmount, currentTaxAmount))
				{
					result = CorrectEntryFeeGST_Year2015;
				}
			}
			return result;
		}

		static ZGuid GetEntryFeeChargeCodePKIfApplicable(ZString currency, ZDecimal osAmount, string countryCode, ZGuid companyPK)
		{
			if (countryCode == Core.Constants.CountryCodes.NewZealand
				&& currency == NewZealandDollar
				&& (osAmount == EntryFeeAmount_Year2015 || osAmount == EntryFeeAmount_Year2018))
			{
				var regConfig = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty).Cast<EntryChargeTypeSetting>()
					.Where(x => x.ChargeType == ValidNZChargeTypes.EntryFee).ToArray();

				if (regConfig.Length == 1)
				{
					return regConfig[0].AC_ChargeCode;
				}
			}

			return ZGuid.Empty;
		}

		static bool IsGSTCorrectionRequired_Year2015(ZGuid entryFeeChargeCodePK, ZGuid chargeCodePK, ZDecimal osAmount, ZDecimal osTaxamount) => entryFeeChargeCodePK.IsValid && entryFeeChargeCodePK == chargeCodePK && osAmount == EntryFeeAmount_Year2015 && osTaxamount == IncorrectEntryFeeGST_Year2015;

		static bool IsGSTCorrect(ZGuid entryFeeChargeCodePK, ZGuid chargeCodePK, ZDecimal osAmount, ZDecimal osTaxamount) => entryFeeChargeCodePK.IsValid && entryFeeChargeCodePK == chargeCodePK &&
			(
				osAmount == EntryFeeAmount_Year2015 && osTaxamount == CorrectEntryFeeGST_Year2015
				||
				osAmount == EntryFeeAmount_Year2018 && osTaxamount == CorrectEntryFeeGST_Year2018
			);

		#region hard coded numbers
		/*
		 * the following hard coded numbers are from CustomsChargeCalculators.EntryFeeCalculator.FeeChargeCalculator/GSTCalculator
		 * there are unit tests to guarantee the integrity between Acc. code and Cus. code. 
		*/
		static readonly ZDecimal EntryFeeAmount_Year2015 = 42.81m;
		static readonly ZDecimal IncorrectEntryFeeGST_Year2015 = 6.42m;
		static readonly ZDecimal CorrectEntryFeeGST_Year2015 = 6.43m;
		static readonly ZDecimal EntryFeeAmount_Year2018 = 45.8m;
		static readonly ZDecimal CorrectEntryFeeGST_Year2018 = 6.87m;

		#endregion

		#region Constants

		public static class ValidNZChargeTypes
		{
			public const string Duty = "DTY";
			public const string EntryFee = "ENF";
			public const string GST = "GST";
			public const string Default = "DEFAULT"; // this is used as an identifier
		}

		#endregion

		static readonly ZString NewZealandDollar = "NZD";

#if DEBUG
		public static ZDecimal EntryFeeAmount_ForTestOnly => EntryFeeAmount_Year2015;
		public static ZDecimal IncorrectEntryFeeGST_ForTestOnly => IncorrectEntryFeeGST_Year2015;
		public static ZDecimal CorrectEntryFeeGST_ForTestOnly => CorrectEntryFeeGST_Year2015;
#endif
	}
}
