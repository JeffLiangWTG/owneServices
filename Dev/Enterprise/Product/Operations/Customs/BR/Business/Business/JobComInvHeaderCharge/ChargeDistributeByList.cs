
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ChargeDistributeByList : Common.ChargeDistributeByList
	{
		public new static class Codes
		{
			public const string NetWeight = "NWT";
			public const string FOB = "FOB";
		}

		public new static class Descriptions
		{
			public static string NetWeight => Res.GetString("C0DB8017-FABE-47DC-8D62-ED924932A8FC", "Net Weight");
			public static string FOB => Res.GetString("240f1880-9b4c-4870-adbe-205603462c75", "FOB Value");
		}

		public ChargeDistributeByList()
		{
			AddPair(Codes.NetWeight, Descriptions.NetWeight);
			AddPair(Codes.FOB, Descriptions.FOB);
		}
	}
}
