using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IAirImpendingArrivalReportInformation : IImpendingArrivalReportInformation
	{
		ZString FlightNo { get; }
	}
}
