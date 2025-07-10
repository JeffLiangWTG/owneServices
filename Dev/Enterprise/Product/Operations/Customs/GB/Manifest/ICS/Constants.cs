using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.ICS
{
	public static class Constants
	{
		public static class CusCodeDataTypes
		{
			public static class Codes
			{
				public const string OOF = "OOF";
				public const string OOS = "OOS";
			}
		}

		public static class ICSMessageSubTypes
		{
			public const string NEW = GBMessageTypeList.Codes.New;
			public const string AMEND = GBMessageTypeList.Codes.Amend;
			public const string CANCEL = GBMessageTypeList.Codes.Cancel;
		}

		public static class MessageSubTypePreFixes
		{
			public const string ICS = "ICS NI";
		}
	}
}
