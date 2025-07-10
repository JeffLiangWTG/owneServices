using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE007TraderAtDestinationProvider : IIE007TraderAtDestination
	{
		readonly OrgHeader trader;

		public IE007TraderAtDestinationProvider(NctsHeader nctsHeader)
		{
			Argument.NotNull(nctsHeader, nameof(nctsHeader));
			trader = nctsHeader.DestinationTrader?.Organisation;
		}
		public string IdentificationNumber => trader?.GetEoriDetails();

		public string CommunicationLanguageAtDestination => "IE";
	}
}
