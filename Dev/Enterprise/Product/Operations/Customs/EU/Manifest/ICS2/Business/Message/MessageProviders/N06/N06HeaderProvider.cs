using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class N06HeaderProvider : ICS2BaseMessageProvider, IN06Header
	{
		public N06HeaderProvider(AsycudaManifestHeader manifestHeader) : base(manifestHeader)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(manifestHeader, nameof(manifestHeader)));
		}

		readonly MessageHeaderProviderHelper helper;

		public IReadOnlyCollection<IPartyId> NotifyParty => helper.NotifyParty;

		public string ActiveBorderTransportMeansIdentificationNumber => string.Empty;

		public string ActiveBorderTransportMeansIdentificationType => string.Empty;

		public string ModeofTransport => helper.WCOTransportMode;

		public DateTime ActualArrivalDate => helper.ActualArrivalDate;

		public DateTime? EstimatedArrivalDate => helper.EstimatedArrivalDate;

		public string ConveyanceReferenceNumber => helper.ConveyanceReferenceNumber;

		public IReadOnlyCollection<IIdentifierTypePair> RelatedTransportDocument => helper.AllTransportDocuments;

		public IReadOnlyCollection<string> RelatedMRN => realatedMRN ?? (realatedMRN = new string[] { MRN });
		IReadOnlyCollection<string> realatedMRN;

		public IPartyId PersonNotifyingArrival => PartyIdProvider.New(Declarant.IdentificationNumber, new Collection<IIdentifierTypePair>());

		public string CustomsOfficeReferenceNumber => helper.CustomsOfficeReferenceNumber;
	}
}
