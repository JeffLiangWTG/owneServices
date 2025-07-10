using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CHouseConsignmentItemProvider
	{
		public CC043CHouseConsignmentItemProvider(ConsignmentItemType04 consignmentItem)
		{
			this.consignmentItem = Argument.NotNull(consignmentItem, nameof(consignmentItem));
		}

		readonly ConsignmentItemType04 consignmentItem;

		public string GoodsItemNumber => consignmentItem.GoodsItemNumber;
		public string DeclarationGoodsItemNumber => consignmentItem.DeclarationGoodsItemNumber;
		public string DeclarationType => consignmentItem.DeclarationType;
		public string CountryOfDestination => consignmentItem.CountryOfDestination;

		public CC043CCommodityProvider Commodity => commodity ?? (commodity = new CC043CCommodityProvider(consignmentItem.Commodity));
		CC043CCommodityProvider commodity;

		public IReadOnlyCollection<CC043CPackagingProvider> Packaging => packaging ?? (packaging = consignmentItem.Packaging?.Select(x => new CC043CPackagingProvider(x)).ToArray() ?? Array.Empty<CC043CPackagingProvider>());
		IReadOnlyCollection<CC043CPackagingProvider> packaging;

		public IReadOnlyCollection<CC043CPreviousDocumentWithGoodsItemNumberProvider> PreviousDocument => previousDocument ?? (previousDocument = consignmentItem.PreviousDocument?.Select(x => new CC043CPreviousDocumentWithGoodsItemNumberProvider(x)).ToArray() ?? Array.Empty<CC043CPreviousDocumentWithGoodsItemNumberProvider>());
		IReadOnlyCollection<CC043CPreviousDocumentWithGoodsItemNumberProvider> previousDocument;

		public IReadOnlyCollection<CC043CSupportingDocumentProvider> SupportingDocument => supportingDocument ?? (supportingDocument = consignmentItem.SupportingDocument?.Select(x => new CC043CSupportingDocumentProvider(x)).ToArray() ?? Array.Empty<CC043CSupportingDocumentProvider>());
		IReadOnlyCollection<CC043CSupportingDocumentProvider> supportingDocument;

		public IReadOnlyCollection<CC043CDocumentProvider> TransportDocument => transportDocument ?? (transportDocument = consignmentItem.TransportDocument?.Select(x => new CC043CDocumentProvider(x)).ToArray() ?? Array.Empty<CC043CDocumentProvider>());
		IReadOnlyCollection<CC043CDocumentProvider> transportDocument;

		public IReadOnlyCollection<CC043CDocumentProvider> AdditionalReference => additionalReference ?? (additionalReference = consignmentItem.AdditionalReference?.Select(x => new CC043CDocumentProvider(x)).ToArray() ?? Array.Empty<CC043CDocumentProvider>());
		IReadOnlyCollection<CC043CDocumentProvider> additionalReference;

		public IReadOnlyCollection<CC043AdditionalInformationProvider> AdditionalInformation => additionalInformation ?? (additionalInformation = consignmentItem.AdditionalInformation?.Select(x => new CC043AdditionalInformationProvider(x)).ToArray());
		IReadOnlyCollection<CC043AdditionalInformationProvider> additionalInformation;
	}
}
