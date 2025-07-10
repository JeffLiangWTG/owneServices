using System;
using CargoWise.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

static class SendingObjectHelper
{
	internal static NctsHeader CheckHeaderNotNullAndDepartureType(NctsHeader nctsHeader) => CheckHeaderNotNullAndHeaderType(nctsHeader, NctsMovementType.Codes.Departure);

	internal static NctsHeader CheckHeaderNotNullAndArrivalType(NctsHeader nctsHeader) => CheckHeaderNotNullAndHeaderType(nctsHeader, NctsMovementType.Codes.Arrival);

	internal static NctsHeader CheckHeaderNotNullAndHeaderType(NctsHeader nctsHeader, string headerType)
	{
		if (Argument.NotNull(nctsHeader, nameof(nctsHeader)).BH_HeaderType != headerType)
		{
			throw new ArgumentException($"Wrong movement header type ({headerType})");
		}
		return nctsHeader;
	}
}
