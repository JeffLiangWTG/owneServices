using CargoWise.Common;
using RefCusCodeListAttributeTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListAttributeTypes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsBillsArrivalAdditionalDocumentReadOnlyProvider : IAdditionalDocumentReadOnlyProvider
	{
		public NctsBillsArrivalAdditionalDocumentReadOnlyProvider(NctsBillAdditionalDocument additionalDocument)
		{
			this.additionalDocument = Argument.NotNull(additionalDocument, nameof(additionalDocument));
		}

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumberReadOnly => GetReferenceNumberReadOnlyStatus();

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumber2ReadOnly => GetReferenceNumber2ReadOnlyStatus();

		bool IAdditionalDocumentReadOnlyProvider.DescriptionReadOnly => GetDescriptionReadOnlyStatus();

		bool IAdditionalDocumentReadOnlyProvider.AdditionalInfoReadOnly => GetAdditionalInfoReadOnlyStatus();

		bool IAdditionalDocumentReadOnlyProvider.LineNoReadOnly => true;

		bool IAdditionalDocumentReadOnlyProvider.StatusReadOnly => true;

		#region Implementation

		bool FieldsReadOnlyForPhase5Arrival => additionalDocument.FieldsReadOnlyForPhase5Arrival;

		bool GetDescriptionReadOnlyStatus()
		{
			return additionalDocument.RefCusCode.MissesAttribute(RefCusCodeListAttributeTypes.Complement)
				|| StatusIsDEC || StatusIsMIS || FieldsReadOnlyForPhase5Arrival;
		}

		bool GetReferenceNumberReadOnlyStatus()
		{
			return additionalDocument.RefCusCode.MissesAttribute(RefCusCodeListAttributeTypes.Reference)
				|| StatusIsDEC || StatusIsMIS || FieldsReadOnlyForPhase5Arrival;
		}

		bool GetReferenceNumber2ReadOnlyStatus() => StatusIsMIS || FieldsReadOnlyForPhase5Arrival;

		bool GetAdditionalInfoReadOnlyStatus()
		{
			return StatusIsDEC || StatusIsMIS || FieldsReadOnlyForPhase5Arrival;
		}

		bool StatusIsDEC => additionalDocument.CSI_Status == SupportingDocumentStatusList.Codes.DEC;

		bool StatusIsMIS => additionalDocument.CSI_Status == SupportingDocumentStatusList.Codes.MIS;

		readonly NctsBillAdditionalDocument additionalDocument;

		#endregion
	}
}
