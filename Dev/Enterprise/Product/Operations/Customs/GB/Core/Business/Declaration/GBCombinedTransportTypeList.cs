using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GBCombinedTransportTypeList : TransportTypeList
	{
		public GBCombinedTransportTypeList()
		{
			AddPairIfNotExist(GBTransportTypeList.Codes.ROR, GBTransportTypeList.Descriptions.ROR);
			Sort();
		}
	}
}
