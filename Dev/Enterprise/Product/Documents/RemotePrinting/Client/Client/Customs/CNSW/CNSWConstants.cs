namespace Enterprise.RemotePrinting.Client
{
	public static class CNSWConstants
	{
		public static class XmlElements
		{
			public const string GenericMessageInterchange = "GenericMessageInterchange";
			public const string ImportAgrRequest = "ImportAgrRequest";
			public const string ImportAgrResponse = "ImportAgrResponse";
		}

		public static class XmlNamespaces
		{
			public const string GenericMessageDelivery = "http://cargowise.com/ehub/core/genericmessagedelivery";
			public const string ChinaPortDec = "http://www.chinaport.gov.cn/dec";
		}

		public const string TimeStampFormat = "yyyyMMddHHmmssfffffff";
	}
}
