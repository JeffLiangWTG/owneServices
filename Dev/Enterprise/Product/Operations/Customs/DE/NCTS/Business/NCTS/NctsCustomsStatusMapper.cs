using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public static class NctsCustomsStatusMapper
	{
		[ThreadSafe]
		static readonly ImmutableDictionary<string, string> NctsCustomsStatusMappingDictionary = new Dictionary<string, string>
		{
			{ string.Empty, NctsTransitStatusList.Codes.Unknown },
			{ "12", NctsTransitStatusList.Codes.DeclarationMrnAllocated },
			{ "13", NctsTransitStatusList.Codes.DeclarationRejected },
			{ "14", NctsTransitStatusList.Codes.DeclarationMrnAllocated },
			{ "15", NctsTransitStatusList.Codes.GoodsNotReleasedForTransit },
			{ "31", NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture },
			{ "34", NctsTransitStatusList.Codes.GoodsWrittenOff },
			{ "35", NctsTransitStatusList.Codes.GoodsWrittenOff },
			{ "38", NctsTransitStatusList.Codes.DeclarationCancelled },
			{ "54", NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted },
			{ "56", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease },
			{ "311", NctsTransitStatusList.Codes.DeclarationDataRequested },
			{ "343", NctsTransitStatusList.Codes.ControlResultCaptured },
			{ "361", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease },
		}.ToImmutableDictionary();

		public static (bool success, string mappedStatusCode) GetCW1StatusFromNctsCustomsStatus(ZString nctsCustomsStatusCode) => (NctsCustomsStatusMappingDictionary.TryGetValue(nctsCustomsStatusCode, out var mappedStatusCode), mappedStatusCode);
	}
}
