
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class TransportTypeList : CodeDescriptionPairList
	{
		public TransportTypeList()
		{
			AddPair(Codes.Sea, Descriptions.Sea);
			AddPair(Codes.River, Descriptions.River);
			AddPair(Codes.Lake, Descriptions.Lake);
			AddPair(Codes.Air, Descriptions.Air);
			AddPair(Codes.Mail, Descriptions.Mail);
			AddPair(Codes.Rail, Descriptions.Rail);
			AddPair(Codes.Road, Descriptions.Road);
			AddPair(Codes.Fixed, Descriptions.Fixed);
			AddPair(Codes.Own, Descriptions.Own);
			AddPair(Codes.Other, Descriptions.Other);
		}

		public static class Codes
		{
			public const string Sea = Core.Constants.TransportModes.Sea;
			public const string River = Constants.BRTransportModes.River;
			public const string Lake = Constants.BRTransportModes.Lake;
			public const string Air = Core.Constants.TransportModes.Air;
			public const string Mail = Core.Constants.TransportModes.Mail;
			public const string Rail = Core.Constants.TransportModes.Rail;
			public const string Road = Core.Constants.TransportModes.Road;
			public const string Fixed = Core.Constants.TransportModes.FixedTransportInstallations;
			public const string Own = Core.Constants.TransportModes.OwnPropulsion;
			public const string Other = Core.Constants.TransportModes.Other;
		}

		public static class Descriptions
		{
			public static string Sea => Res.GetString("FB1758B0-614E-435C-B2CF-D30B6B59B8E9", "Sea");
			public static string River => Res.GetString("97E3D0F3-D23E-4D04-BF71-27CA57A5F328", "River");
			public static string Lake => Res.GetString("584A285A-A9F4-44FB-B970-17CE5A41E5E5", "Lake");
			public static string Air => Res.GetString("D68157D1-3997-4976-B1A7-DA1F609EA204", "Air");
			public static string Mail => Res.GetString("D0230A6A-5563-4CFA-9206-ED3533D3326A", "Postal");
			public static string Rail => Res.GetString("EAC3F9DF-1370-4FC5-B3A3-87E81B6FAEB4", "Rail");
			public static string Road => Res.GetString("4B6AD769-9EF4-4116-A910-B66FF73E0E3A", "Road");
			public static string Fixed => Res.GetString("3E1DFE41-BE57-4260-B871-3F1C0D527283", "Fixed Transport Installations");
			public static string Own => Res.GetString("1CCB34EA-C44C-459A-990F-6AFBEC14FCE6", "Own Means");
			public static string Other => Res.GetString("00F67425-0C7F-4A41-883B-29CDA8513C52", "Other");
		}
	}
}
