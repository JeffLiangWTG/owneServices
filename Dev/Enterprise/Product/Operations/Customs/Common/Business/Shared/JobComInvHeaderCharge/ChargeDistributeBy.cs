using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public class ChargeDistributeByList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Quantity = "QTY";
			public const string Value = "VAL";
			public const string Volume = "VOL";
			public const string Weight = "WGT";
		}

		public static class Descriptions
		{
			public static string Quantity
			{
				get { return Res.GetString("552941B1-006C-4743-AE8F-057E91E41354", "Invoice Quantity"); }
			}
			public static string Value
			{
				get { return Res.GetString("b8e61ae9-18b5-40bc-9d72-8e49e583f32f", "Value"); }
			}
			public static string Volume
			{
				get { return Res.GetString("2f0f6d84-f67c-4997-a933-483c2cac2e9d", "Volume"); }
			}
			public static string Weight
			{
				get { return Res.GetString("67a7884c-2478-4232-872b-663bf0235bc1", "Weight"); }
			}
		}

		public ChargeDistributeByList()
		{
			AddPair(Codes.Quantity, Descriptions.Quantity);
			AddPair(Codes.Value, Descriptions.Value);
			AddPair(Codes.Volume, Descriptions.Volume);
			AddPair(Codes.Weight, Descriptions.Weight);
		}
	}
}
