using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public partial class NctsMessageTypeList : CodeDescriptionPairList
	{
		public static CodeDescriptionPairList NctsDepartureMessageTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.DEPDAT, Descriptions.DEPDAT);
				return result;
			}
		}

		public static CodeDescriptionPairList NctsArrivalMessageTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.DESNOT, Descriptions.DESNOT);
				result.AddPair(Codes.DESREM, Descriptions.DESREM);
				return result;
			}
		}
	}
}
