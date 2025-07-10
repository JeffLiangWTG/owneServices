using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IAirActualArrivalReportInformation : IActualArrivalReportInformation
	{
		ZDateTime EstimatedArrivalDate { get; }
		ZString FlightNo { get; }
		ZDateTime DateTimeOfDepartureUTC { get; }
	}
}
