using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public static class IncoTermValidationHelper
	{
		public static void ValidateWaterIncoTermAndTransportMode(ZPropertyInfo propertyInfo, ZString transportMode)
		{
			var incoTerm = (ZString)propertyInfo.Value;
			var isInlandWaterwayIncoTerm = incoTerm == Core.Constants.IncoTerms.FreeAlongsideShip
											|| incoTerm == Core.Constants.IncoTerms.FreeOnBoard
											|| incoTerm == Core.Constants.IncoTerms.CostAndFreight
											|| incoTerm == Core.Constants.IncoTerms.CostInsuranceAndFreight;

			var isSeaOrInlandWaterwayTransportMode = transportMode == TransportTypeList.Codes.Sea
													|| transportMode == TransportTypeList.Codes.InlandWaterwayTransport;

			if (isInlandWaterwayIncoTerm && !isSeaOrInlandWaterwayTransportMode)
			{
				propertyInfo.AddWarning(Res.GetString("9C453643-0CA0-4E41-917F-0C4034C9DB7A", "This Incoterm ({0}) is only valid for sea and inland waterway transport. Please check against the transport mode.", incoTerm));
			}
		}
	}
}
