using System;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	sealed class NctsTypesProvider : INctsTypesProvider
	{
		public Type NctsAdditionalInfoType => typeof(EU.NCTS.Business.NctsAdditionalInfo);
		public Type NctsArrivalCargoDescType => typeof(EU.NCTS.Business.NctsArrivalCargoDesc);
		public Type NctsDepartureCargoDescType => typeof(NctsDepartureCargoDesc);
	}
}
