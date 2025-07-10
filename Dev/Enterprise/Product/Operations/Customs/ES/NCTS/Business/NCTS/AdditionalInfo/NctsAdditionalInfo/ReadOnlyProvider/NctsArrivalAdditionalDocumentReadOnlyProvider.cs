using CargoWise.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	sealed class NctsArrivalAdditionalDocumentReadOnlyProvider : IAdditionalDocumentReadOnlyProvider
	{
		public NctsArrivalAdditionalDocumentReadOnlyProvider(NctsAdditionalInfo additionalDocument, IAdditionalDocumentReadOnlyProvider baseReadOnlyProvider)
		{
			this.additionalDocument = Argument.NotNull(additionalDocument, nameof(additionalDocument));
			this.baseReadOnlyProvider = Argument.NotNull(baseReadOnlyProvider, nameof(baseReadOnlyProvider));
		}

		readonly NctsAdditionalInfo additionalDocument;
		readonly IAdditionalDocumentReadOnlyProvider baseReadOnlyProvider;

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumberReadOnly => baseReadOnlyProvider.ReferenceNumberReadOnly;

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumber2ReadOnly => baseReadOnlyProvider.ReferenceNumber2ReadOnly;

		bool IAdditionalDocumentReadOnlyProvider.DescriptionReadOnly => baseReadOnlyProvider.DescriptionReadOnly;

		bool IAdditionalDocumentReadOnlyProvider.AdditionalInfoReadOnly => baseReadOnlyProvider.AdditionalInfoReadOnly;

		bool IAdditionalDocumentReadOnlyProvider.LineNoReadOnly => baseReadOnlyProvider.LineNoReadOnly;

		bool IAdditionalDocumentReadOnlyProvider.StatusReadOnly => GetStatusReadOnly;

		bool GetStatusReadOnly => StatusIsNew || NctsArrivalMovementHeader.IsUnloadingRemarksReadOnly;

		bool StatusIsNew => additionalDocument.CSI_Status == NctsUnloadedStateList.Codes.NEW;

		NctsArrivalMovementHeader NctsArrivalMovementHeader => additionalDocument.Parent is NctsArrivalCargoDesc goodsItem
			? goodsItem.Header.ArrivalMovementHeader
			: additionalDocument.Parent as NctsArrivalMovementHeader;
	}
}
