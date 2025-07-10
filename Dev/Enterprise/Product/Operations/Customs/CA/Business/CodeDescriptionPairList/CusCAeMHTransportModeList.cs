
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHTransportModeList : CodeDescriptionPairList
	{
		public CusCAeMHTransportModeList()
		{
			AddPair(TransportTypeList.Codes.Sea, CBSATransportTypeList.Descriptions.Sea);
			AddPair(TransportTypeList.Codes.Road, CBSATransportTypeList.Descriptions.Road);
			AddPair(TransportTypeList.Codes.Rail, CBSATransportTypeList.Descriptions.Rail);
			AddPair(TransportTypeList.Codes.Air, CBSATransportTypeList.Descriptions.Air);
		}
	}
}
