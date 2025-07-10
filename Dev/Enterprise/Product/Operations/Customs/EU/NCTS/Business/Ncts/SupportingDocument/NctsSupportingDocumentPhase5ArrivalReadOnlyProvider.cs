namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsSupportingDocumentPhase5ArrivalReadOnlyProvider : ISupportingDocumentReadOnlyConditions
	{
		public NctsSupportingDocumentPhase5ArrivalReadOnlyProvider(NctsSupportingDocument supportingDocument)
		{
			this.supportingDocument = supportingDocument;
		}
		readonly NctsSupportingDocument supportingDocument;

		public bool CSI_LineNo_ReadOnly => true;

		public bool CSI_Status_ReadOnly => true;

		public bool CSI_Code_ReadOnly => CommonReadOnlyCondition;

		public bool CSI_ReferenceNumber_ReadOnly => CommonReadOnlyCondition;

		public bool CSI_ReferenceNumber2_ReadOnly => CommonReadOnlyCondition;

		public bool CSI_Description_ReadOnly => CommonReadOnlyCondition;

		public bool CSI_ItemNumber_ReadOnly => supportingDocument.RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributeTypes.ItemNumber) || StatusIsMIS;

		bool DeclaredOrUnloadingMarksFullyAccepted => supportingDocument.IsStatusDeclared || (NctsArrivalMovementHeader?.IsUnloadingRemarksReadOnly ?? false);

		bool CommonReadOnlyCondition => DeclaredOrUnloadingMarksFullyAccepted || StatusIsMIS;

		bool StatusIsMIS => supportingDocument.CSI_Status == SupportingDocumentStatusList.Codes.MIS;

		NctsArrivalMovementHeader NctsArrivalMovementHeader => supportingDocument.Parent is NctsBill bill
			? bill.Header.ArrivalMovementHeader
			: supportingDocument.Parent is NctsArrivalCargoDesc goodItem
				? goodItem.Header.ArrivalMovementHeader
				: supportingDocument.Parent is NctsArrivalMovementHeader arrivalMovementHeader
					? arrivalMovementHeader
					: null;
	}
}
