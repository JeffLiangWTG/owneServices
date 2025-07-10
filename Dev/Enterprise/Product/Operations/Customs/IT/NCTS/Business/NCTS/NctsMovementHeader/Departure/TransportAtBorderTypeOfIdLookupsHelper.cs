using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

static class TransportAtBorderTypeOfIdLookupsHelper
{
	public static CodeDescriptionPairList GetCachedTransportAtBorderTypeOfIdList(NctsCommonMovementHeader header)
	{
		Argument.NotNull(header, nameof(header));
		var factory = header.Factory;
		var cacheKey = "IT.NCTS.GetCachedTransportAtBorderTypeOfIdList";

		if (header.IsPhase5)
		{
			var transportMode = header.BM_ExportTransportMode;
			var isInPhase5TransitionPeriod = header.IsInPhase5TransitionPeriod;
			cacheKey += (transportMode + isInPhase5TransitionPeriod);

			return factory.GetCachedValue(cacheKey, () => GetListForTransportAtBorderTypeOfIdForPhase5(header.BM_ExportTransportMode, isInPhase5TransitionPeriod));
		}

		return factory.GetCachedValue(cacheKey, GetListForTransportAtBorderTypeOfId);
	}

	static CodeDescriptionPairList GetListForTransportAtBorderTypeOfId()
	{
		var list = new NctsTransportTypeOfIdList();
		list.RemoveCode(NctsTransportTypeOfIdList.Codes._20);
		list.RemoveCode(NctsTransportTypeOfIdList.Codes._31);
		return list;
	}

	static CodeDescriptionPairList GetListForTransportAtBorderTypeOfIdForPhase5(ZString exportTransportMode, bool isInPhase5TransitionPeriod)
	{
		var list = new CodeDescriptionPairList();

		switch (exportTransportMode)
		{
			case EU.Business.ModeOfTransportList.Codes._1_SeaTransport:
				list.AddPair(NctsTransportTypeOfIdList.Codes._10, NctsTransportTypeOfIdList.Descriptions._10);
				list.AddPair(NctsTransportTypeOfIdList.Codes._11, NctsTransportTypeOfIdList.Descriptions._11);
				break;
			case EU.Business.ModeOfTransportList.Codes._2_RailTransport:
				list.AddPair(NctsTransportTypeOfIdList.Codes._21, NctsTransportTypeOfIdList.Descriptions._21);
				break;
			case EU.Business.ModeOfTransportList.Codes._3_RoadTransport:
				list.AddPair(NctsTransportTypeOfIdList.Codes._30, NctsTransportTypeOfIdList.Descriptions._30);
				break;
			case EU.Business.ModeOfTransportList.Codes._4_AirTransport:
				list.AddPair(NctsTransportTypeOfIdList.Codes._40, NctsTransportTypeOfIdList.Descriptions._40);
				list.AddPair(NctsTransportTypeOfIdList.Codes._41, NctsTransportTypeOfIdList.Descriptions._41);
				break;
			case EU.Business.ModeOfTransportList.Codes._7_FixedTransportInstallations:
			case EU.Business.ModeOfTransportList.Codes._9_OwnPropulsion:
				list = new NctsTransportTypeOfIdList();
				list.RemoveCode(NctsTransportTypeOfIdList.Codes._20);
				list.RemoveCode(NctsTransportTypeOfIdList.Codes._31);
				list.RemoveCode(NctsTransportTypeOfIdList.Codes._99);
				break;
			case EU.Business.ModeOfTransportList.Codes._8_InlandWaterwayTransport:
				list.AddPair(NctsTransportTypeOfIdList.Codes._80, NctsTransportTypeOfIdList.Descriptions._80);
				list.AddPair(NctsTransportTypeOfIdList.Codes._81, NctsTransportTypeOfIdList.Descriptions._81);
				break;
			default:
				return list;
		}

		if (isInPhase5TransitionPeriod)
		{
			list.AddPair(NctsTransportTypeOfIdList.Codes._99, NctsTransportTypeOfIdList.Descriptions._99);
		}

		return list;
	}
}
