namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsSupportingDocumentPhase4ReadOnlyProvider : ISupportingDocumentReadOnlyConditions
	{
		public NctsSupportingDocumentPhase4ReadOnlyProvider()
		{
		}

		public bool CSI_LineNo_ReadOnly => false;

		public bool CSI_Status_ReadOnly => false;

		public bool CSI_Code_ReadOnly => false;

		public bool CSI_ReferenceNumber_ReadOnly => false;

		public bool CSI_ReferenceNumber2_ReadOnly => false;

		public bool CSI_Description_ReadOnly => false;

		public bool CSI_ItemNumber_ReadOnly => false;
	}
}
