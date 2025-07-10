using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IOutturnReportHeaderInformation
	{
		ZString ResponsiblePartyID { get; }
		ZString EstablishmentID { get; }
	}
}
