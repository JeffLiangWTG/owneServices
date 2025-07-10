using CargoWise.Types;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitReportUcc6Lookups : CusExitReportLookups
{
	public CusExitReportUcc6Lookups(AutoCusExitReport parent)
		: base(parent)
	{
	}

	public override CodeDescriptionPairList TransportTypeList
	{
		get
		{
			var transportMode = Parent.CER_TransportMode;
			return Factory.GetCachedValue("EU.CusExitReportLookups.TransportTypeList" + transportMode, () => GetApplicableTransportTypes(transportMode));
		}
	}

	CodeDescriptionPairList GetApplicableTransportTypes(ZString transportMode)
	{
		var transportTypesList = new CodeDescriptionPairList();
		switch (transportMode)
		{
			case Customs.Business.TransportTypeList.Codes.Sea:
				transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._10, CusExitReportTransportTypeList.Descriptions._10);
				transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._11, CusExitReportTransportTypeList.Descriptions._11);
				break;
			case Customs.Business.TransportTypeList.Codes.Rail:
				transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._21, CusExitReportTransportTypeList.Descriptions._21);
				break;
			case Customs.Business.TransportTypeList.Codes.Road:
				transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._30, CusExitReportTransportTypeList.Descriptions._30);
				break;
			case Customs.Business.TransportTypeList.Codes.Air:
				transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._40, CusExitReportTransportTypeList.Descriptions._40);
				transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._41, CusExitReportTransportTypeList.Descriptions._41);
				break;
			case Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport:
				transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._80, CusExitReportTransportTypeList.Descriptions._80);
				transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._81, CusExitReportTransportTypeList.Descriptions._81);
				break;
			default:
				transportTypesList = new CusExitReportTransportTypeList();
				break;
		}
		transportTypesList.Sort();
		return transportTypesList;
	}
}

