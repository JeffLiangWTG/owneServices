using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Registry.Business.Customs.AU
{
	#region SuppressResourceStringsCheckRegion

	public class EntryChargeTypeList : Enterprise.Registry.Business.Customs.EntryChargeTypeList
	{
		public static class Codes
		{
			public const string DutyAmount = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;//DTY
			public const string EntryFee = "ENF";
			public const string GSTAmount = Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;//GST
			public const string GSTDeferred = Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred;//GSD
			public const string DutyDeferredAmount = "DTD";
			public const string InterimDumpingDuty = "IDD";
			public const string InterimAntiDumpingDuty = "ADD";
			public const string InterimCountervailingDuty = "CTD";
			public const string LCTAmount = "LCT";
			public const string MessageFee = "MGF";
			public const string ScreenFree = "SCF";
			public const string TradegateGST = "TDG";
			public const string WetAmount = "WET";
			public const string Woodlevy = "WDL";
			public const string OtherCharges = "OTH";
			public const string FlatDutyPortion = "FDP";
			public const string CountervailingSecurityAmount = "CSA";
			public const string DumpingSecurityAmount = "DSA";
			public const string CountervailingDuty = "CVD";
			public const string DumpingDuty = "DMP";
			public const string DutyOverride = "DTO";
			public const string AQISProcessingCharge = "APC";
			public const string AQISServicePaymentAmount = "ASP";
			public const string DeclarationProcessingCharge = "DPC";
			public const string TotalPayableAdmin = "TPA";
			public const string AQISContainerCharges = "ACC";
			public const string StandardDutyOverriden = "STD";
			public const string TotalDutyTaxForLine = "TDT";
			public const string SecurityConcession = "SEC";
			public const string SecurityLiability = "SEL";
		}

		public static class Descriptions
		{
			public const string DutyAmount = "Duty";
			public const string EntryFee = "Entry Fee";
			public const string GSTAmount = "GST Amount";
			public const string GSTDeferred = "GST Deferred Amount";
			public const string DutyDeferredAmount = "Duty Deferred Amount";
			public const string InterimDumpingDuty = "Interim Dumping Duty";
			public const string InterimAntiDumpingDuty = "Interim Anti Dumping Duty";
			public const string InterimCountervailingDuty = "Interim Coutervailing Duty";
			public const string LCTAmount = "Luxury Car Tax";
			public const string MessageFee = "Message Fee";
			public const string ScreenFree = "Screen Free Charge";
			public const string TradegateGST = "Tradegate GST";
			public const string WetAmount = "Wine Equalisation Tax";
			public const string Woodlevy = "Wood Levy";
			public const string OtherCharges = "Other Amount";
			public const string FlatDutyPortion = "Flat Duty Portion";

			public const string CountervailingSecurityAmount = "CountervailingSecurityAmount";
			public const string DumpingSecurityAmount = "DumpingSecurityAmount";
			public const string CountervailingDuty = "CountervailingDuty";
			public const string DumpingDuty = "DumpingDuty";
			public const string DutyOverride = "DutyOverride";
			public const string AQISProcessingCharge = "Quarantine Processing Charge";
			public const string AQISServicePaymentAmount = "Quarantine Service Payment Amount";
			public const string DeclarationProcessingCharge = "Declaration Processing Charge";
			public const string TotalPayableAdmin = "Total Payable Admin";
			public const string AQISContainerCharges = "Quarantine Container Charges";
			public const string StandardDutyOverriden = "Standard Duty Overriden";
			public const string TotalDutyTaxForLine = "Total Duty Tax Payable for Line";
			public const string SecurityConcession = "Security Concession Amount";
			public const string SecurityLiability = "Security Liability Amount";
		}

		public EntryChargeTypeList()
		{
			Add(Codes.DutyAmount, Descriptions.DutyAmount, true, "");
			Add(Codes.EntryFee, Descriptions.EntryFee, true, "");
			Add(Codes.GSTAmount, Descriptions.GSTAmount, true, "");
			Add(Codes.GSTDeferred, Descriptions.GSTDeferred, true, "");
			Add(Codes.DutyDeferredAmount, Descriptions.DutyDeferredAmount, true, "");
			Add(Codes.InterimAntiDumpingDuty, Descriptions.InterimAntiDumpingDuty, true, ""); // CMR only, not currenty on ICustomCharges on Dec. CustomsCharges needs refactor on the dec to aggregate from EntryHeader fees.
			Add(Codes.InterimCountervailingDuty, Descriptions.InterimCountervailingDuty, true, ""); // CMR only, not currenty on ICustomCharges on Dec. CustomsCharges needs refactor on the dec to aggregate from EntryHeader fees.
			Add(Codes.LCTAmount, Descriptions.LCTAmount, true, "");
			Add(Codes.MessageFee, Descriptions.MessageFee, true, "");
			Add(Codes.ScreenFree, Descriptions.ScreenFree, true, "");
			Add(Codes.TradegateGST, Descriptions.TradegateGST, true, "");
			Add(Codes.WetAmount, Descriptions.WetAmount, true, "");
			Add(Codes.Woodlevy, Descriptions.Woodlevy, true, "");
			Add(Codes.OtherCharges, Descriptions.OtherCharges, true, "");
			Add(Codes.FlatDutyPortion, Descriptions.FlatDutyPortion, false, "");

			Add(Codes.CountervailingSecurityAmount, Descriptions.CountervailingSecurityAmount, true, "");
			Add(Codes.DumpingSecurityAmount, Descriptions.DumpingSecurityAmount, true, "");
			Add(Codes.CountervailingDuty, Descriptions.CountervailingDuty, true, "");
			Add(Codes.DumpingDuty, Descriptions.DumpingDuty, true, "");
			Add(Codes.DutyOverride, Descriptions.DutyOverride, true, "");
			Add(Codes.AQISServicePaymentAmount, Descriptions.AQISServicePaymentAmount, true, ""); // CMR only, not currenty on ICustomCharges on Dec. CustomsCharges needs refactor on the dec to aggregate from EntryHeader fees.
			Add(Codes.AQISProcessingCharge, Descriptions.AQISProcessingCharge, true, "");
			Add(Codes.DeclarationProcessingCharge, Descriptions.DeclarationProcessingCharge, true, "");
			Add(Codes.TotalPayableAdmin, Descriptions.TotalPayableAdmin, true, "");
			Add(Codes.AQISContainerCharges, Descriptions.AQISContainerCharges, true, ""); // CMR only, not currenty on ICustomCharges on Dec. CustomsCharges needs refactor on the dec to aggregate from EntryHeader fees.

			Add(Codes.StandardDutyOverriden, Descriptions.StandardDutyOverriden, true, "");
			Add(Codes.TotalDutyTaxForLine, Descriptions.TotalDutyTaxForLine, false, "");
			Add(Codes.InterimDumpingDuty, Descriptions.InterimDumpingDuty, true, "");
			Add(Codes.SecurityConcession, Descriptions.SecurityConcession, false, "");
			Add(Codes.SecurityLiability, Descriptions.SecurityLiability, false, "");
		}

		public override string DutyCode => Codes.DutyAmount;

		public override string TaxCode => Codes.GSTAmount;

		protected override IEnumerable<ZGuid> GetSpecialChargeCodePks(ZGuid companyPK)
		{
			var result = base.GetSpecialChargeCodePks(companyPK).ToList();

			if (!RegistryItem?.Value.Cast<EntryChargeTypeSetting>().Any(x => x.ChargeType == EntryChargeTypeList.Codes.AQISServicePaymentAmount) ?? false)
			{
				ZGuid value = RatingDataRegistry.Instance.CustomsQuarantineChargeCode.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty).ChargeCode;
				if (value.IsValid)
				{
					result.Add(value);
				}
			}

			return result;
		}

		public override EntryChargeTypeSetting GetChargeTypeSpecificRegistrySetting(string chargeCode)
		{
			var result = base.GetChargeTypeSpecificRegistrySetting(chargeCode);
			if (result == null && chargeCode == EntryChargeTypeList.Codes.AQISServicePaymentAmount)
			{
				ZGuid asp = RatingDataRegistry.Instance.CustomsQuarantineChargeCode.Value.ChargeCode;
				if (asp.IsValid)
				{
					result = new EntryChargeTypeSetting
					{
						ChargeType = chargeCode,
						AC_ChargeCode = asp
					};
				}
			}

			return result;
		}
	}

	#endregion
}
