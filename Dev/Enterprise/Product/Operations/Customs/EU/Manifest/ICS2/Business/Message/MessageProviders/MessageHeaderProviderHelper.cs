using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class MessageHeaderProviderHelper
	{
		public MessageHeaderProviderHelper(AsycudaManifestHeader header)
		{
			manifestHeader = Argument.NotNull(header, nameof(header));
		}

		readonly AsycudaManifestHeader manifestHeader;

		public string SpecificCircumstanceIndicator => manifestHeader.SpecificCircumstanceIndicator;

		public int ReEntryIndicator => manifestHeader.ReEntryIndicator ? 1 : 0;

		public string GetReferralRequestReference(IAmendedItemsProvider amendedItemsProvider)
		{
			return amendedItemsProvider?.AmendedItems?.FirstOrDefault()?.Identifier;
		}

		public string GetResponsibleMemberStateCountry(IAmendedItemsProvider amendedItemsProvider)
		{
			return amendedItemsProvider?.AmendedItems?.FirstOrDefault()?.ResponsibleMemberState;
		}

		public string AddressedMemberStateCountry => manifestHeader.AddressedMemberState;

		public IParty Representative => CachedValueHelper.GetValue(ref representative, () => RepresentativeProvider.NewOrNull(manifestHeader));
		CachedValue<IParty> representative;

		public IReadOnlyCollection<IPartyId> NotifyParty => notifyParty ?? (notifyParty = new Collection<IPartyId> { PartyIdProvider.New(manifestHeader.ShippingAgent?.Header?.GetICS2EoriDetails() ?? string.Empty, new Collection<IIdentifierTypePair>()) });
		IReadOnlyCollection<IPartyId> notifyParty;

		public string WCOTransportMode
		{
			get
			{
				var translator = new TransportModeTranslator();
				return translator.TranslateToWCOCode(manifestHeader.AMA_TransportMode);
			}
		}

		public IReadOnlyCollection<IHrcmScreeningResults> HrcmScreeningResults => hrcmScreeningResults ?? (hrcmScreeningResults = GetHrcmScreeningResults(null));
		IReadOnlyCollection<IHrcmScreeningResults> hrcmScreeningResults;

		public IReadOnlyCollection<IHrcmScreeningResults> GetHrcmScreeningResults(IAmendedItemsProvider amendedItemsProvider)
		{
			var result = new List<IHrcmScreeningResults>();

			var amendedItems = amendedItemsProvider?.AmendedItems;
			if (amendedItems != null)
			{
				var requestHeaders = amendedItems
				.Select(c => c.RequestHeader)
				.Where(c => c != null)
				.Distinct();

				foreach (var requestHeader in requestHeaders)
				{
					var bill = requestHeader?.RelatedHouseBill;

					if (bill != null)
					{
						var referralRequestReference = requestHeader.EUS_Identifier;
						result.AddRange(bill.BillScreenings.ToArray<AsycudaBillScreening, IHrcmScreeningResults>(c => new HrcmScreeningResultsProvider(c, referralRequestReference)));
					}
				}
			}
			else
			{
				result.AddRange(manifestHeader.BillScreenings.ToArray<AsycudaBillScreening, IHrcmScreeningResults>(c => new HrcmScreeningResultsProvider(c)));
			}

			return result;
		}

		public IIdentifierTypePair TransportDocument => CachedValueHelper.GetValue(ref transportDocument,
														() => TransportDocumentProvider.NewOrNull(manifestHeader.AMA_MasterBill, manifestHeader.MasterBill.TransportDocumentType));
		CachedValue<IIdentifierTypePair> transportDocument;

		public IReadOnlyCollection<IIdentifierTypePair> AllTransportDocuments => allTransportDocument ?? (allTransportDocument = new ReadOnlyCollection<IIdentifierTypePair>(manifestHeader.Bills.Cast<AsycudaBill>().Select(bill => TransportDocumentProvider.NewOrNull(bill.ABL_BillNumber, bill.TransportDocumentType) as IIdentifierTypePair).Prepend(TransportDocument).ToList()));
		IReadOnlyCollection<IIdentifierTypePair> allTransportDocument;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocumentsMasterLevel => supportingDocumentsMasterLevel ?? (supportingDocumentsMasterLevel = MessageProviderHelper.GetSupportingDocuments(manifestHeader.SupportingDocuments));
		IReadOnlyCollection<IIdentifierTypePair> supportingDocumentsMasterLevel;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationsMasterLevel => additionalInformationsMasterLevel ?? (additionalInformationsMasterLevel = MessageProviderHelper.GetAdditionalInformationCollection(manifestHeader.AdditionalInfos));
		IReadOnlyCollection<IAdditionalInformation> additionalInformationsMasterLevel;

		public DateTime ActualArrivalDate => MessageProviderHelper.UtcDateTime(manifestHeader.AMA_A_ARV);

		public DateTime? EstimatedArrivalDate => MessageProviderHelper.ToNullableDateTime(manifestHeader.AMA_E_ARV);

		public string ConveyanceReferenceNumber => manifestHeader.AMA_Voyage;

		public IReadOnlyCollection<string> RelatedMRN => realatedMRN ?? (realatedMRN = new string[] { manifestHeader.RegistrationNumber });
		IReadOnlyCollection<string> realatedMRN;

		public string CustomsOfficeReferenceNumber => manifestHeader.AMA_CustomsOffice;

		public IReadOnlyCollection<string> ReceptacleIdentificationNumbers => receptacleIdentificationNumbers ??= GetReceptacleIdentificationNumbers();
		IReadOnlyCollection<string> receptacleIdentificationNumbers;

		IReadOnlyCollection<string> GetReceptacleIdentificationNumbers()
		{
			return manifestHeader.Receptacles.Cast<Receptacle>().ToArray(c => c.CY_Data.ToString());
		}

		public IReadOnlyCollection<IAdditionalInformationResponse> GetAdditionalInformationResponses(IAmendedItemsProvider amendedItemsProvider)
		{
			var result = new List<IAdditionalInformationResponse>();

			var amendedItems = amendedItemsProvider?.AmendedItems;
			if (amendedItems != null)
			{
				var requestHeaders = amendedItems
					.Select(c => c.RequestHeader)
					.Where(c => c != null)
					.Distinct();

				foreach (var requestHeader in requestHeaders)
				{
					result.Add(new AdditionalInformationResponseProvider(requestHeader));
				}
			}

			return result;
		}

		public string PreviousDocumentIdentification => manifestHeader.PreviousMRN;
	}
}
