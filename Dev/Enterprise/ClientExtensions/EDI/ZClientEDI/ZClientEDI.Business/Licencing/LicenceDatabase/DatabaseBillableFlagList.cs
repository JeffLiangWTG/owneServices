using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public sealed class DatabaseBillableFlagList : CodeDescriptionPairList
	{
		public sealed class Codes
		{
			Codes() { } //CA1053
			public const string Null = "X";
			public const string YesCustomer = "Y";
			public const string YesPartner = "P";
			public const string No = "N";
			public const string InvalidCode = "@";
		}

		public sealed class Descriptions
		{
			Descriptions() { } //CA1053
			public const string Null = "";
			public const string YesCustomer = "Yes - Customer";
			public const string YesPartner = "Yes - Partner";
			public const string No = "No";
		}

		public DatabaseBillableFlagList()
		{
			AddPair(Codes.Null, Descriptions.Null);
			AddPair(Codes.YesCustomer, Descriptions.YesCustomer);
			AddPair(Codes.YesPartner, Descriptions.YesPartner);
			AddPair(Codes.No, Descriptions.No);
		}
	}
}
