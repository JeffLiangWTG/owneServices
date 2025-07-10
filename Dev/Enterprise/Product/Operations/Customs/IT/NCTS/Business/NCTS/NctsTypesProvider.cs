using System;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsTypesProvider : INctsTypesProvider
{
	public Type NctsAdditionalInfoType => typeof(NctsAdditionalInfo);
	public Type NctsArrivalCargoDescType => typeof(EU.NCTS.Business.NctsArrivalCargoDesc);
	public Type NctsDepartureCargoDescType => typeof(NctsDepartureCargoDesc);
}
