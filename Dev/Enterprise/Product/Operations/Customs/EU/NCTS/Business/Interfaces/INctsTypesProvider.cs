using System;

namespace Enterprise.Customs.EU.NCTS.Business.Interfaces
{
	public interface INctsTypesProvider
	{
		Type NctsAdditionalInfoType { get; }
		Type NctsArrivalCargoDescType { get; }
		Type NctsDepartureCargoDescType { get; }
	}
}
