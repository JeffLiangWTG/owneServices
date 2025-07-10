using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public static class NctsExtensionsForTest
	{
		public static NctsHeader NewDepartureNctsHeader(this BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			return nctsHeader;
		}

		public static NctsHeader NewArrivalNctsHeader(this BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			return nctsHeader;
		}
	}
}
