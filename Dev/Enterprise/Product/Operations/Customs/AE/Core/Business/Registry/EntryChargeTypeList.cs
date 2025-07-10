namespace Enterprise.Customs.AE.Registry
{
	public sealed class EntryChargeTypeList : Enterprise.Registry.Business.Customs.EntryChargeTypeList
	{
		public static class Codes
		{
			public const string DutyAmount = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;//DTY
			public const string RegistrationFee = "REG";
		}

		public static class Descriptions
		{
			public const string DutyAmount = "Duty";
			public const string RegistrationFee = "Registration Fee";
		}

		public EntryChargeTypeList()
		{
			Add(Codes.DutyAmount, Descriptions.DutyAmount, true, "");
			Add(Codes.RegistrationFee, Descriptions.RegistrationFee, true, "");
		}

		public override string DutyCode => Codes.DutyAmount;

		public override string TaxCode => "";
	}
}
