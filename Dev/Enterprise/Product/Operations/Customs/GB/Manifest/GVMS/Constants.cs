using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public static class Constants
	{
		public static class GVMSMessageSubTypes
		{
			public const string NEW = GBMessageTypeList.Codes.New;
			public const string AMEND = GBMessageTypeList.Codes.Amend;
			public const string CANCEL = GBMessageTypeList.Codes.Cancel;
			public const string FINALISE = "FIN";
			public const string NOTIFICATIONMESSAGEID = "NID";
			public const string EHUBERRORRESPONSE = "EHE";
		}

		public static class MessageSubTypePreFixes
		{
			public const string GVMS = "GVMS";
		}
		public const string GVMSMessageRetrieverServiceTaskCode = "GVM";
	}
}
