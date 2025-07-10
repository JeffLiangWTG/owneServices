using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class ExitHeaderProvider : IExitHeader
	{
		public ExitHeaderProvider(CusExitReport report)
		{
			this.report = Argument.NotNull(report, nameof(report));
			consignment = report.Consignment;
		}
		protected readonly CusExitReport report;
		protected readonly CusExitConsignment consignment;

		public string LRN => consignment.CXC_LocalReference;

		public string MRN => consignment.CXC_MovementReference;

		public string ActualExitCustomsOffice => report.CER_OfficeOfExit;

		public IPartyID Declarant => PartyIDProvider.NewOrNull(report.Declarant.Address);

		public IPartyID Representative => PartyIDProvider.NewOrNull(report.Representative.Address);

		public IPartyID ExitCarrier => PartyIDProvider.NewOrNull(report.Header.Carrier);

		public string RegistrationNumberExternal => consignment.CXC_ReferenceNumber;
	}
}
