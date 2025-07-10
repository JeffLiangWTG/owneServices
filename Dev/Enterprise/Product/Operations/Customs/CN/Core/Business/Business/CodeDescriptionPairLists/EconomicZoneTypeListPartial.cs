using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public partial class EconomicZoneTypeList
	{
		public static readonly IEnumerable<ZString> SupervisionAreaCodes = new ZString[]
		{
			Codes.BondedArea,
			Codes.ExportProcessingZone,
			Codes.ComprehensiveBondedArea,
			Codes.BondedLogisticsZone,
			Codes.ComprehensiveExperimentalZone,
			Codes.InternationalBorder,
			Codes.BondedLogisticsCenter
		};
	}
}
