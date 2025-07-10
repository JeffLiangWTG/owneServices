using CargoWise.Types;

namespace Enterprise.Customs.CA.Registry
{
	public partial class EntryChargeTypeList : Enterprise.Registry.Business.Customs.EntryChargeTypeList
	{
		public static class Codes
		{
			public const string CustomsValueForTax = "CVT";
			public const string CustomsValueInInvoiceCurrency = "CVC";
			public const string TotalDutyAmount = "DTY";
			public const string Duty1 = "DT1";
			public const string Duty2 = "DT2";
			public const string Duty3 = "DT3";
			public const string TotalExciseTaxAmount = "EXS";
			public const string TotalGSTAmount = "GST";
			public const string TotalSIMAAmount = "SIM";
			public const string TotalNonBillableSIMAAmount = "SIN";
			public const string K84LateFilingPenalty = "KLP";
			public const string TotalGSTDirectAmount = "GSD";
			public const string Others = "OTH";
		}

		public static class Descriptions
		{
			public const string CustomsValueForTax = "Customs Value For Tax";
			public const string CustomsValueInInvoiceCurrency = "Customs Value In Invoice Currency";
			public const string TotalDutyAmount = "Total Duty Amount";
			public const string Duty1 = "Duty1 Amount";
			public const string Duty2 = "Duty2 Amount";
			public const string Duty3 = "Duty3 Amount";
			public const string TotalExciseTaxAmount = "Total Excise Tax Amount";
			public const string TotalGSTAmount = "Total GST Amount";
			public const string TotalSIMAAmount = "Total SIMA Amount";
			public const string TotalNonBillableSIMAAmount = "Total Non-Billable SIMA Amount";
			public const string K84LateFilingPenalty = "K84 Late Filing Penalty";
			public const string TotalGSTDirectAmount = "Total GST Direct Amount";
			public const string Others = "Others";
		}

		public EntryChargeTypeList()
		{
			Add(Codes.CustomsValueForTax, Descriptions.CustomsValueForTax, false, ZString.Empty);
			Add(Codes.CustomsValueInInvoiceCurrency, Descriptions.CustomsValueInInvoiceCurrency, false, ZString.Empty);
			Add(Codes.TotalDutyAmount, Descriptions.TotalDutyAmount, true, ZString.Empty);
			Add(Codes.Duty1, Descriptions.Duty1, false, ZString.Empty);
			Add(Codes.Duty2, Descriptions.Duty2, false, ZString.Empty);
			Add(Codes.Duty3, Descriptions.Duty3, false, ZString.Empty);
			Add(Codes.TotalExciseTaxAmount, Descriptions.TotalExciseTaxAmount, true, ZString.Empty);
			Add(Codes.TotalGSTAmount, Descriptions.TotalGSTAmount, true, ZString.Empty);
			Add(Codes.TotalSIMAAmount, Descriptions.TotalSIMAAmount, true, ZString.Empty);
			Add(Codes.TotalNonBillableSIMAAmount, Descriptions.TotalNonBillableSIMAAmount, true, ZString.Empty);
			Add(Codes.K84LateFilingPenalty, Descriptions.K84LateFilingPenalty, false, ZString.Empty);
			Add(Codes.TotalGSTDirectAmount, Descriptions.TotalGSTDirectAmount, true, ZString.Empty);
			Add(Codes.Others, Descriptions.Others, false, ZString.Empty);
		}

		public override string DutyCode => Codes.TotalDutyAmount;

		public override string TaxCode => Codes.TotalGSTAmount;
	}
}
