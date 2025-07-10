using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business;

public class DLMTransportTypeList : CodeDescriptionPairList
{
	public DLMTransportTypeList()
	{
		AddPair(TransportTypeList.Codes.Air, Customs.Business.TransportTypeList.Descriptions.Air);
		AddPair(TransportTypeList.Codes.InlandWaterwayTransport, Customs.Business.TransportTypeList.Descriptions.InlandWaterwayTransport);
		AddPair(TransportTypeList.Codes.FixedTransportInstallations, Customs.Business.TransportTypeList.Descriptions.FixedTransportInstallations);
		AddPair(TransportTypeList.Codes.Rail, Customs.Business.TransportTypeList.Descriptions.Rail);
		AddPair(TransportTypeList.Codes.Road, Customs.Business.TransportTypeList.Descriptions.Road);
		AddPair(TransportTypeList.Codes.Sea, Customs.Business.TransportTypeList.Descriptions.Sea);
		AddPair(TransportTypeList.Codes.Mail, Customs.Business.TransportTypeList.Descriptions.Mail);
	}
}
