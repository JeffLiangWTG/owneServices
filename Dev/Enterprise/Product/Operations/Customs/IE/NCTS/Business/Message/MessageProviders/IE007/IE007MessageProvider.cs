using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE007MessageProvider : NctsArrivalHeaderMessageProvider, IIE007Header
	{
		public IE007MessageProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ?? (authorisations = NctsHeader.CusAuthorizationUsages.Select(s => new AuthorisationProvider(s)).ToArray());
		IReadOnlyCollection<IAuthorisation> authorisations;

		public IIE007Consignment Consignment => consignment ?? (consignment = new IE007ConsignmentProvider(NctsHeader));
		IIE007Consignment consignment;

		public string CustomsOfficeOfDestination => ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival;

		public IIE007TraderAtDestination TraderAtDestination => traderAtDestination ?? (traderAtDestination = new IE007TraderAtDestinationProvider(NctsHeader));
		IIE007TraderAtDestination traderAtDestination;

		public IIE007TransitOperation TransitOperation => transitOperation ?? (transitOperation = new IE007TransitOperationProvider(NctsHeader));
		IIE007TransitOperation transitOperation;
	}
}
