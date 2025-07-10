using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IAirOutturnReportHeaderInformation : IOutturnReportHeaderInformation
	{
		ZDateTime DateTimeOfOutturn { get; }
		ZString FlightNumber { get; }
		ZDateTime EstimatedDateOfArrival { get; }
		IAirOutturnReportLineInformation[] Lines { get; }
		IAirOutturnReportLineInformation[] DatabaseLines { get; }
	}
}
