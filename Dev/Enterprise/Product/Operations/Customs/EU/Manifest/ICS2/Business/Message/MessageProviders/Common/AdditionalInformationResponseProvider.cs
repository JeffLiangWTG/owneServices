using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AdditionalInformationResponseProvider : IAdditionalInformationResponse
	{
		public AdditionalInformationResponseProvider(RequestHeader requestHeader)
		{
			this.requestHeader = Argument.NotNull(requestHeader, nameof(requestHeader));
		}

		readonly RequestHeader requestHeader;

		public string ReferralResponseReference => requestHeader.EUS_Identifier;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformation ?? (additionalInformation = MessageProviderHelper.GetAdditionalInformationCollection(requestHeader.RequestResponses));
		IReadOnlyCollection<IAdditionalInformation> additionalInformation;

		public IReadOnlyCollection<IBinaryFile> BinaryAttachments => binaryAttachments ?? (binaryAttachments = MessageProviderHelper.ToArray<CusStorageDocPivot, IBinaryFile>(requestHeader.Attachments, (doc) => new BinaryFileProvider(doc)));
		IReadOnlyCollection<IBinaryFile> binaryAttachments;

		public IIdentifierTypePair TransportDocumentHouseLevel => CachedValueHelper.GetValue(ref transportDocument, () => GetTransportDocumentProvider());
		CachedValue<IIdentifierTypePair> transportDocument;

		TransportDocumentProvider GetTransportDocumentProvider()
		{
			if (requestHeader.RelatedHouseBill == null)
			{
				return null;
			}
			else
			{
				return TransportDocumentProvider.NewOrNull(requestHeader.RelatedHouseBill.ABL_BillNumber, requestHeader.RelatedHouseBill.TransportDocumentType);
			}
		}
	}
}
