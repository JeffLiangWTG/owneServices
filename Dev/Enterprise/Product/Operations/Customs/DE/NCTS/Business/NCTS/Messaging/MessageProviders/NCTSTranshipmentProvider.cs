using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NCTSTranshipmentProvider : INCTSTranshipment
	{
		public static NCTSTranshipmentProvider NewOrNull(EnRouteTransshipment transshipment) => transshipment == null ? null : new NCTSTranshipmentProvider(transshipment);

		readonly EnRouteTransshipment transshipment;

		NCTSTranshipmentProvider(EnRouteTransshipment transshipment)
		{
			this.transshipment = transshipment;
		}

		public string TransportMeansIdentity => transshipment.BN_TransportID.ValueOrNullIfEmpty();

		public string TransportMeansNationality => transshipment.BN_TransportCountryCode.ValueOrNullIfEmpty();

		public INCTSEndorsement Endorsement => endorsement ?? (endorsement = NCTSEndorsementProvider.NewOrNull(transshipment));
		INCTSEndorsement endorsement;

		public IReadOnlyCollection<string> ContainerIdentificationNumbers => containerIdentificationNumbers ?? (containerIdentificationNumbers = transshipment.ContainersNumbers.Select(n => n.ToString()).ToArray());
		IReadOnlyCollection<string> containerIdentificationNumbers;
	}
}
