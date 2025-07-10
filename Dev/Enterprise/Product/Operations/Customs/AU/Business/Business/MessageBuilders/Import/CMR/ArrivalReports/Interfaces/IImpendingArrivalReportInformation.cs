using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IImpendingArrivalReportInformation : IArrivalReportInformation
	{
		ZString PortOfFirstArrival { get; }
		ZDateTime DateTimeOfDepartureUTC { get; }
		IImpendingArrivalReportLineInformation[] Lines { get; }
		IImpendingArrivalReportLineInformation[] DatabaseLines { get; }
	}
}
