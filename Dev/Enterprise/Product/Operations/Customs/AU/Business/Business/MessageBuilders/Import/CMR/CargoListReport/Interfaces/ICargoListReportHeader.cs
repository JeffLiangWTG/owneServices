using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICargoListReportHeader
	{
		ZString CargoResponsiblePartyID { get; }
		ZString LloydsNumber { get; }
		ZString VoyageNumber { get; }
		ZString DischargePort { get; }
		ICargoListReportLine[] Lines { get; }
		ICargoListReportLine[] DatabaseLines { get; }
	}
}
