using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.US.AMS
{
	public class AMSConsolBillCustomsStatusList : AMSBillCustomsStatusList
	{
		public new class Codes : AMSBillCustomsStatusList.Codes
		{
			public const string Multiple = "MULTIPLE";
			public const string OutOfSync = "OUT_OF_SYNC";
		}

		public new class Descriptions : AMSBillCustomsStatusList.Descriptions
		{
			public static MultilingualString Multiple { get { return ResString.GetMultilingualString("AMSConsolBillCustomsStatusList|Multiple", "There are some AMS Bill Of Ladings 'On File' and some 'Not On File'"); } }
			public static MultilingualString OutOfSync { get { return ResString.GetMultilingualString("AMSConsolBillCustomsStatusList|OutOfSync", "Bill Status cannot be determined as AMS Bill Of Ladings and Consol Shipments do not match"); } }
		}

		public AMSConsolBillCustomsStatusList()
		{
			AddPairIfNotExist(Codes.Multiple, Descriptions.Multiple);
			AddPairIfNotExist(Codes.OutOfSync, Descriptions.OutOfSync);
		}
	}
}
