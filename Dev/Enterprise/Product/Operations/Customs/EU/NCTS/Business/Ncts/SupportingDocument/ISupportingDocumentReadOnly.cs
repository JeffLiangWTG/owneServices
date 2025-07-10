namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface ISupportingDocumentReadOnlyConditions
	{
		bool CSI_LineNo_ReadOnly { get; }

		bool CSI_Status_ReadOnly { get; }

		bool CSI_Code_ReadOnly { get; }

		bool CSI_ReferenceNumber_ReadOnly { get; }

		bool CSI_ReferenceNumber2_ReadOnly { get; }

		bool CSI_Description_ReadOnly { get; }

		bool CSI_ItemNumber_ReadOnly { get; }
	}
}
