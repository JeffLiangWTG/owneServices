using System.Collections.Generic;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class R02HeaderProvider : ICS2BaseMessageProvider, IR02Header, IAmendedItemsProvider
	{
		public R02HeaderProvider(AsycudaManifestHeader manifestHeader)
			: base(manifestHeader)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(manifestHeader, nameof(manifestHeader)));
		}

		readonly MessageHeaderProviderHelper helper;

		public string ResponsibleMemberStateCountry => helper.GetResponsibleMemberStateCountry(this);

		public IIdentifierTypePair TransportDocumentMasterLevel => helper.TransportDocument;

		public string DeclarantIdentificationNumber => Declarant.IdentificationNumber;

		public IReadOnlyCollection<IAdditionalInformationResponse> AdditionalInformationResponses => additionalInformationResponses ?? (additionalInformationResponses = helper.GetAdditionalInformationResponses(this));
		IReadOnlyCollection<IAdditionalInformationResponse> additionalInformationResponses;

		IAmendedItem[] IAmendedItemsProvider.AmendedItems { get; set; }
	}
}
