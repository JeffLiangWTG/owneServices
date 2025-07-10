using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IImpendingArrivalReportLineInformation
	{
		ZString PortOfArrival { get; }
		ZDateTime EstimatedDateTimeOfArrivalUTC { get; }
		ZString DischargeCTOEstablishmentID { get; }
		ZString StevedoreID { get; }
		bool DischargeIndicator { get; }
	}
}
