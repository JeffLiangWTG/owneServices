using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class DepartureTransportMeansProvider : IDepartureTransportMeans
	{
		public DepartureTransportMeansProvider(CusExitReport report)
		{
			this.report = Argument.NotNull(report, nameof(report));
		}
		protected readonly CusExitReport report;

		public string TypeOfIdentification => report.CER_TransportType;

		public string IdentificationNumber => report.CER_TransportID;

		public string Nationality => report.CER_RN_NKTransportNationality;
	}
}
