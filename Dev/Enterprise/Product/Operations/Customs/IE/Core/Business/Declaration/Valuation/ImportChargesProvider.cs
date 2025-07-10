using System;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.IE.Business.Declaration
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:Field names should begin with lower-case letter", Justification = "Charge provider codes all start with numbers")]
	static class ImportChargesProvider
	{
		public static ICustomsChargeCode[] Codes => codes ?? (codes = [AB, AD, AE, AF, AG, AH, AI, AJ, AK, ONS, OFT, AL, AN, BA, BB, BC, BD, BE, BF, BG, _1X, _2X]);
		[ThreadStatic]
		static ICustomsChargeCode[] codes;

		public static ICustomsChargeCode[] IncludedInInvoice => includedInInvoice ?? (includedInInvoice = [BA, BB, BC, BD, BE, BF, BG, _2X]);
		[ThreadStatic]
		static ICustomsChargeCode[] includedInInvoice;

		public static ICustomsChargeCode[] IncludedInInvoiceDeemedForThisCharge => includedInInvoiceDeemedForThisCharge ?? (includedInInvoiceDeemedForThisCharge = [BA, BB, BC, BD, BE, BF, BG, _1X, _2X]);
		[ThreadStatic]
		static ICustomsChargeCode[] includedInInvoiceDeemedForThisCharge;

		public static ICustomsChargeCode[] RecommendedAndMandatoryCodesEXW_FCA_FAS_FOB => recommendedAndMandatoryCodesEXW_FCA_FAS_FOB ?? (recommendedAndMandatoryCodesEXW_FCA_FAS_FOB = [AK, BA, _1X]);
		[ThreadStatic]
		static ICustomsChargeCode[] recommendedAndMandatoryCodesEXW_FCA_FAS_FOB;

		public static bool IsRecommendedAndMandatoryCodesEXW_FCA_FAS_FOB(this ZString incoterm) =>
			incoterm == Core.Constants.IncoTerms.ExWorks
			|| incoterm == Core.Constants.IncoTerms.FreeOnBoard
			|| incoterm == Core.Constants.IncoTerms.FreeCarrier
			|| incoterm == Core.Constants.IncoTerms.FreeAlongsideShip;

		public static ICustomsChargeCode[] RecommendedCodesCFR_CIF_CPT => recommendedCodesCFR_CIF_CPT ?? (recommendedCodesCFR_CIF_CPT = [AK]);
		[ThreadStatic]
		static ICustomsChargeCode[] recommendedCodesCFR_CIF_CPT;

		public static ICustomsChargeCode[] RecommendedAndMandatoryCodesDDP => recommendedAndMandatoryCodesDDP ?? (recommendedAndMandatoryCodesDDP = [BC]);
		[ThreadStatic]
		static ICustomsChargeCode[] recommendedAndMandatoryCodesDDP;

		public static CustomsChargeCode AB => ab ?? (ab = new CustomsChargeCode(AISChargeCodeList.Codes.AB, AISChargeCodeList.Descriptions.AB)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
		});
		[ThreadStatic]
		static CustomsChargeCode ab;

		public static CustomsChargeCode AD => ad ?? (ad = new CustomsChargeCode(AISChargeCodeList.Codes.AD, AISChargeCodeList.Descriptions.AD)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode ad;

		public static CustomsChargeCode AE => ae ?? (ae = new CustomsChargeCode(AISChargeCodeList.Codes.AE, AISChargeCodeList.Descriptions.AE)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
		});
		[ThreadStatic]
		static CustomsChargeCode ae;

		public static CustomsChargeCode AF => af ?? (af = new CustomsChargeCode(AISChargeCodeList.Codes.AF, AISChargeCodeList.Descriptions.AF)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
		});
		[ThreadStatic]
		static CustomsChargeCode af;

		public static CustomsChargeCode AG => ag ?? (ag = new CustomsChargeCode(AISChargeCodeList.Codes.AG, AISChargeCodeList.Descriptions.AG)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
		});
		[ThreadStatic]
		static CustomsChargeCode ag;

		public static CustomsChargeCode AH => ah ?? (ah = new CustomsChargeCode(AISChargeCodeList.Codes.AH, AISChargeCodeList.Descriptions.AH)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode ah;

		public static CustomsChargeCode AI => ai ?? (ai = new CustomsChargeCode(AISChargeCodeList.Codes.AI, AISChargeCodeList.Descriptions.AI)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode ai;

		public static CustomsChargeCode AJ => aj ?? (aj = new CustomsChargeCode(AISChargeCodeList.Codes.AJ, AISChargeCodeList.Descriptions.AJ)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
		});
		[ThreadStatic]
		static CustomsChargeCode aj;

		public static CustomsChargeCode AK => ak ?? (ak = new CustomsChargeCode(AISChargeCodeList.Codes.AK, AISChargeCodeList.Descriptions.AK)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode ak;

		public static CustomsChargeCode OFT => oft ?? (oft = new CustomsChargeCode(AISChargeCodeList.Codes.InternationalFreight, AISChargeCodeList.Descriptions.InternationalFreight)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode oft;

		public static CustomsChargeCode ONS => ons ?? (ons = new CustomsChargeCode(AISChargeCodeList.Codes.InternationalInsurance, AISChargeCodeList.Descriptions.InternationalInsurance)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode ons;

		public static CustomsChargeCode AL => al ?? (al = new CustomsChargeCode(AISChargeCodeList.Codes.AL, AISChargeCodeList.Descriptions.AL)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode al;

		public static CustomsChargeCode AN => an ?? (an = new CustomsChargeCode(AISChargeCodeList.Codes.AN, AISChargeCodeList.Descriptions.AN)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode an;

		public static CustomsChargeCode BA => ba ?? (ba = new CustomsChargeCode(AISChargeCodeList.Codes.BA, AISChargeCodeList.Descriptions.BA)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode ba;

		public static CustomsChargeCode BB => bb ?? (bb = new CustomsChargeCode(AISChargeCodeList.Codes.BB, AISChargeCodeList.Descriptions.BB)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
		});
		[ThreadStatic]
		static CustomsChargeCode bb;

		public static CustomsChargeCode BC => bc ?? (bc = new CustomsChargeCode(AISChargeCodeList.Codes.BC, AISChargeCodeList.Descriptions.BC)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
		});
		[ThreadStatic]
		static CustomsChargeCode bc;

		public static CustomsChargeCode BD => bd ?? (bd = new CustomsChargeCode(AISChargeCodeList.Codes.BD, AISChargeCodeList.Descriptions.BD)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode bd;

		public static CustomsChargeCode BE => be ?? (be = new CustomsChargeCode(AISChargeCodeList.Codes.BE, AISChargeCodeList.Descriptions.BE)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode be;

		public static CustomsChargeCode BF => bf ?? (bf = new CustomsChargeCode(AISChargeCodeList.Codes.BF, AISChargeCodeList.Descriptions.BF)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode bf;

		public static CustomsChargeCode BG => bg ?? (bg = new CustomsChargeCode(AISChargeCodeList.Codes.BG, AISChargeCodeList.Descriptions.BG)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode bg;

		public static CustomsChargeCode _1X => _1x ?? (_1x = new CustomsChargeCode(AISChargeCodeList.Codes._1X, AISChargeCodeList.Descriptions._1X)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
		});
		[ThreadStatic]
		static CustomsChargeCode _1x;

		public static CustomsChargeCode _2X => _2x ?? (_2x = new CustomsChargeCode(AISChargeCodeList.Codes._2X, AISChargeCodeList.Descriptions._2X)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
		});
		[ThreadStatic]
		static CustomsChargeCode _2x;
	}
}
