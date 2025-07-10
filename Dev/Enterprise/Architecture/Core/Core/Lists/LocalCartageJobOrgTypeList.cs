using System;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	public class LocalCartageJobOrgTypeList : CodeDescriptionPairList
	{
		protected LocalCartageJobOrgTypeList()
		{
			AddPair(Codes.CFS, Descriptions.CFS);
			AddPair(Codes.CNE, Descriptions.CNE);
			AddPair(Codes.CNR, Descriptions.CNR);
			AddPair(Codes.CTO, Descriptions.CTO);
			AddPair(Codes.CYD, Descriptions.CYD);
			AddPair(Codes.MSC, Descriptions.MSC);
			AddPair(Codes.SRV, Descriptions.SRV);
			AddPair(Codes.WHS, Descriptions.WHS);
		}

		public static LocalCartageJobOrgTypeList Instance
		{
			get { return instance ?? (instance = new LocalCartageJobOrgTypeList()); }
		}
		[ThreadStatic]
		static LocalCartageJobOrgTypeList instance;

		public abstract class Codes
		{
			public const string CTO = "CTO";
			public const string CFS = "CFS";
			public const string CNR = "CNR";
			public const string CNE = "CNE";
			public const string CYD = "CYD";
			public const string SRV = "SRV";
			public const string MSC = "MSC";
			public const string WHS = "WHS";
		}

		public abstract class Descriptions
		{
			public static string CTO { get { return Res.GetString("Common|LocalCartageJobOrgTypeList|CTO", "Container Terminal Operator"); } }
			public static string CFS { get { return Res.GetString("Common|LocalCartageJobOrgTypeList|CFS", "Container Freight Station"); } }
			public static string CNR { get { return Res.GetString("Common|LocalCartageJobOrgTypeList|CNR", "Consignor"); } }
			public static string CNE { get { return Res.GetString("Common|LocalCartageJobOrgTypeList|CNE", "Consignee"); } }
			public static string CYD { get { return Res.GetString("Common|LocalCartageJobOrgTypeList|CYD", "Container Yard"); } }
			public static string SRV { get { return Res.GetString("Common|LocalCartageJobOrgTypeList|SRV", "Service Provider"); } }
			public static string MSC { get { return Res.GetString("Common|LocalCartageJobOrgTypeList|MSC", "Miscellaneous"); } }
			public static string WHS { get { return Res.GetString("Common|LocalCartageJobOrgTypeList|WHS", "Warehouse"); } }
		}
	}
}
