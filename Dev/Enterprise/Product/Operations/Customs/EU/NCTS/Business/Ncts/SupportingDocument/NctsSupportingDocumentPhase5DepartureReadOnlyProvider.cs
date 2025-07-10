namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsSupportingDocumentPhase5DepartureReadOnlyProvider : ISupportingDocumentReadOnlyConditions
	{
		public NctsSupportingDocumentPhase5DepartureReadOnlyProvider(NctsSupportingDocument supportingDocument)
		{
			this.supportingDocument = supportingDocument;
		}
		readonly NctsSupportingDocument supportingDocument;

		public bool CSI_LineNo_ReadOnly => false;

		public bool CSI_Status_ReadOnly => false;

		public bool CSI_Code_ReadOnly => false;

		public bool CSI_ReferenceNumber_ReadOnly => supportingDocument.CSI_Code.IsEmpty || (supportingDocument.RefCusCode?.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference) ?? false);

		public bool CSI_ReferenceNumber2_ReadOnly => supportingDocument.CSI_Code.IsEmpty || (supportingDocument.RefCusCode?.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributeTypes.Complement) ?? false);

		public bool CSI_Description_ReadOnly => false;

		public bool CSI_ItemNumber_ReadOnly => false;
	}
}
