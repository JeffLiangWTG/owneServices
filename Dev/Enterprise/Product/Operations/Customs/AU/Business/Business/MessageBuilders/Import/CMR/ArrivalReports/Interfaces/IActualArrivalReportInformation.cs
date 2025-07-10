using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IActualArrivalReportInformation : IArrivalReportInformation
	{
		ZString PortOfArrival { get; }
		ZDateTime ActualArrivalDateTimeUTC { get; }
	}
}
