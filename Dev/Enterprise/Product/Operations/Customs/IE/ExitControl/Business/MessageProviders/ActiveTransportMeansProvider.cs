using CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class ActiveTransportMeansProvider : ITransportMeans
	{
		public ActiveTransportMeansProvider(CusExitReport exitReport)
		{
			this.exitReport = exitReport;
		}
		readonly CusExitReport exitReport;

		public string TypeOfIdentification => exitReport.CER_TransportType;

		public string IdentificationNumber => exitReport.CER_TransportID;

		public string Nationality => exitReport.CER_RN_NKTransportNationality;
	}
}
