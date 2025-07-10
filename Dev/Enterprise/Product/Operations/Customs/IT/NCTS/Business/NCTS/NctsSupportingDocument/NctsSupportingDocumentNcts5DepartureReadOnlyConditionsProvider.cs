using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsSupportingDocumentNcts5DepartureReadOnlyConditionsProvider : ISupportingDocumentReadOnlyConditions
{
	bool ISupportingDocumentReadOnlyConditions.CSI_LineNo_ReadOnly => false;

	bool ISupportingDocumentReadOnlyConditions.CSI_Status_ReadOnly => false;

	bool ISupportingDocumentReadOnlyConditions.CSI_Code_ReadOnly => false;

	bool ISupportingDocumentReadOnlyConditions.CSI_ReferenceNumber_ReadOnly => false;

	bool ISupportingDocumentReadOnlyConditions.CSI_ReferenceNumber2_ReadOnly => false;

	bool ISupportingDocumentReadOnlyConditions.CSI_Description_ReadOnly => false;

	bool ISupportingDocumentReadOnlyConditions.CSI_ItemNumber_ReadOnly => false;
}
