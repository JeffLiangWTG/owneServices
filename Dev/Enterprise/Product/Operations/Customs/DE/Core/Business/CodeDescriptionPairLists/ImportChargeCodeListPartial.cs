namespace Enterprise.Customs.DE.Business
{
	public partial class ImportChargeCodeList
	{
		public static bool IsSpecificRate(string chargeCode)
		{
			return chargeCode == Codes.SRC
					|| chargeCode == Codes.SRN
					|| chargeCode == Codes.SRS;
		}

		public static bool IsSpecialRate(string chargeCode)
		{
			return IsSpecificRate(chargeCode)
					|| chargeCode == Codes.OPF
					|| chargeCode == Codes.TCE;
		}
	}
}
