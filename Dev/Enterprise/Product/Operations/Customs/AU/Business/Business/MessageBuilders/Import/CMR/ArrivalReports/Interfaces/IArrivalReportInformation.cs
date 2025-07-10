using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IArrivalReportInformation
	{
		ZString LastOverseasPortOfDeparture { get; }
		ZString ResponsiblePartyID { get; }
	}
}
