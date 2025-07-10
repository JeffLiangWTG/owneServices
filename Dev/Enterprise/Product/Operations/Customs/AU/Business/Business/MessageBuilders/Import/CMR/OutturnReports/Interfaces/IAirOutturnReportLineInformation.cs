using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IAirOutturnReportLineInformation : IOutturnReportLineInformation
	{
		ZString HouseAirWaybillNumber { get; }
		ZString MasterAirWaybillNumber { get; }
	}
}
