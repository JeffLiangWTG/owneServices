using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class R03HeaderProvider : ICS2BaseMessageProvider, IR03Header, IAmendedItemsProvider
	{
		public R03HeaderProvider(AsycudaManifestHeader manifestHeader) : base(manifestHeader)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(manifestHeader, nameof(manifestHeader)));
		}

		readonly MessageHeaderProviderHelper helper;

		public string ResponsibleMemberStateCountry => helper.GetResponsibleMemberStateCountry(this);

		public string RepresentativeIdentificationNumber => helper.Representative?.IdentificationNumber ?? string.Empty;

		public IIdentifierTypePair TransportDocument => helper.TransportDocument;

		public string DeclarantIdentificationNumber => Declarant.IdentificationNumber;

		public IReadOnlyCollection<IHrcmScreeningResults> HrcmScreeningResults => hrcmScreeningResults ?? (hrcmScreeningResults = helper.GetHrcmScreeningResults(this));
		IReadOnlyCollection<IHrcmScreeningResults> hrcmScreeningResults;

		IAmendedItem[] IAmendedItemsProvider.AmendedItems { get; set; }
	}
}
