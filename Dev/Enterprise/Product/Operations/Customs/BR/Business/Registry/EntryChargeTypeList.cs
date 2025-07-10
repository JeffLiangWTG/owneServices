using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.Customs.BR.Business.ResString;

namespace Enterprise.Customs.BR.Registry
{
	public sealed class EntryChargeTypeList : Enterprise.Registry.Business.Customs.EntryChargeTypeList
	{
		public static class Codes
		{
			public const string Antidumping = "ADD";
			public const string Cofins = "COF";
			public const string DutyAmount = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
			public const string ICMS = "ICM";
			public const string ImportLicenseFine = "ILF";
			public const string IPI = "IPI";
			public const string PIS = "PIS";
			public const string SiscomexUsageEntryFee = "SUF";
		}

		public static class Descriptions
		{
			public static MultilingualString Antidumping { get { return ResString.GetMultilingualString("EntryChargeTypeList|Antidumping", "Antidumping"); } }
			public static MultilingualString Cofins { get { return ResString.GetMultilingualString("EntryChargeTypeList|Cofins", "Contribution for Social Security Financing"); } }
			public static MultilingualString DutyAmount { get { return ResString.GetMultilingualString("EntryChargeTypeList|DutyAmount", "Duty"); } }
			public static MultilingualString ICMS { get { return ResString.GetMultilingualString("EntryChargeTypeList|ICMS", "ICMS"); } }
			public static MultilingualString ImportLicenseFine { get { return ResString.GetMultilingualString("EntryChargeTypeList|ImportLicenseFine", "Import License Fine"); } }
			public static MultilingualString IPI { get { return ResString.GetMultilingualString("EntryChargeTypeList|IPI", "Tax or Industrialized Products"); } }
			public static MultilingualString PIS { get { return ResString.GetMultilingualString("EntryChargeTypeList|PIS", "Social Integration Program"); } }
			public static MultilingualString SiscomexUsageEntryFee { get { return ResString.GetMultilingualString("EntryChargeTypeList|SISCOMEXUsageEntryFee", "SISCOMEX Usage Entry Fee"); } }
		}

		public EntryChargeTypeList()
		{
			Add(Codes.Antidumping, Descriptions.Antidumping, true, "");
			Add(Codes.Cofins, Descriptions.Cofins, true, "");
			Add(Codes.DutyAmount, Descriptions.DutyAmount, true, "");
			Add(Codes.ICMS, Descriptions.ICMS, true, "");
			Add(Codes.ImportLicenseFine, Descriptions.ImportLicenseFine, true, "");
			Add(Codes.IPI, Descriptions.IPI, true, "");
			Add(Codes.PIS, Descriptions.PIS, true, "");
			Add(Codes.SiscomexUsageEntryFee, Descriptions.SiscomexUsageEntryFee, true, "");
		}

		public override string DutyCode => Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;

		public override string TaxCode => ZString.Empty;
	}
}
