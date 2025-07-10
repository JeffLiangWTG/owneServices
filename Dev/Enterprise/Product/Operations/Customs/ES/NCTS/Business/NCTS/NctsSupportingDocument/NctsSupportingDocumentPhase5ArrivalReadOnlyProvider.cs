using CargoWise.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public sealed class NctsSupportingDocumentPhase5ArrivalReadOnlyProvider : ISupportingDocumentReadOnlyConditions
	{
		public NctsSupportingDocumentPhase5ArrivalReadOnlyProvider(NctsSupportingDocument supportingDocument, ISupportingDocumentReadOnlyConditions baseReadOnlyProvider)
		{
			this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
			this.baseReadOnlyProvider = Argument.NotNull(baseReadOnlyProvider, nameof(baseReadOnlyProvider));
		}

		readonly NctsSupportingDocument supportingDocument;
		readonly ISupportingDocumentReadOnlyConditions baseReadOnlyProvider;

		bool ISupportingDocumentReadOnlyConditions.CSI_LineNo_ReadOnly => baseReadOnlyProvider.CSI_LineNo_ReadOnly;

		bool ISupportingDocumentReadOnlyConditions.CSI_Code_ReadOnly => baseReadOnlyProvider.CSI_Code_ReadOnly;

		bool ISupportingDocumentReadOnlyConditions.CSI_ReferenceNumber_ReadOnly => baseReadOnlyProvider.CSI_ReferenceNumber_ReadOnly;

		bool ISupportingDocumentReadOnlyConditions.CSI_ReferenceNumber2_ReadOnly => baseReadOnlyProvider.CSI_ReferenceNumber_ReadOnly;

		bool ISupportingDocumentReadOnlyConditions.CSI_Description_ReadOnly => baseReadOnlyProvider.CSI_Description_ReadOnly;

		bool ISupportingDocumentReadOnlyConditions.CSI_ItemNumber_ReadOnly => baseReadOnlyProvider.CSI_ItemNumber_ReadOnly;

		bool ISupportingDocumentReadOnlyConditions.CSI_Status_ReadOnly => GetStatusReadOnly;

		bool GetStatusReadOnly => StatusIsNew || NctsArrivalMovementHeader.IsUnloadingRemarksReadOnly;

		bool StatusIsNew => supportingDocument.CSI_Status == NctsUnloadedStateList.Codes.NEW;

		NctsArrivalMovementHeader NctsArrivalMovementHeader => supportingDocument.Parent is NctsBill bill
																	? bill.Header.ArrivalMovementHeader
																	: supportingDocument.Parent is NctsArrivalCargoDesc goodsItem
																			? goodsItem.Header.ArrivalMovementHeader
																			: supportingDocument.Parent is NctsArrivalMovementHeader movementHeader
																					? movementHeader
																					: ((NctsHeader)supportingDocument.Parent).ArrivalMovementHeader;
	}
}
