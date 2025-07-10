using CargoWise.Common;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsGoodsItemsArrivalAdditionalDocumentReadOnlyProvider : IAdditionalDocumentReadOnlyProvider
	{
		public NctsGoodsItemsArrivalAdditionalDocumentReadOnlyProvider(NctsAdditionalInfo additionalDocument)
		{
			this.additionalDocument = Argument.NotNull(additionalDocument, nameof(additionalDocument));
		}

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumberReadOnly => GetReferenceNumberReadOnlyStatus();

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumber2ReadOnly => GetReferenceNumber2ReadOnlyStatus();

		bool IAdditionalDocumentReadOnlyProvider.DescriptionReadOnly => GetDescriptionReadOnlyStatus();

		bool IAdditionalDocumentReadOnlyProvider.AdditionalInfoReadOnly => GetAdditionalInfoReadOnlyStatus();

		bool IAdditionalDocumentReadOnlyProvider.LineNoReadOnly => additionalDocument.IsPhase5;

		bool IAdditionalDocumentReadOnlyProvider.StatusReadOnly => additionalDocument.IsPhase5;

		#region Implementation

		bool FieldsReadOnlyForPhase5Arrival => additionalDocument.FieldsReadOnlyForPhase5Arrival;

		bool GetAdditionalInfoReadOnlyStatus() => StatusIsMIS || (StatusIsDEC && additionalDocument.IsPhase5Arrival) || FieldsReadOnlyForPhase5Arrival;

		bool GetReferenceNumberReadOnlyStatus() => GetReferenceNumberReadOnlyStatusBySubType() || GetAdditionalInfoReadOnlyStatus();

		bool GetReferenceNumberReadOnlyStatusBySubType() => !additionalDocument.IsAnAdditionalReference && !DocumentHasReferenceAttributeAndTRASubType();

		bool GetReferenceNumber2ReadOnlyStatus() => StatusIsMIS || FieldsReadOnlyForPhase5Arrival;

		bool GetDescriptionReadOnlyStatus() => StatusIsMIS || additionalDocument.IsPhase5 || FieldsReadOnlyForPhase5Arrival;

		bool DocumentHasReferenceAttributeAndTRASubType()
		{
			var result = false;
			if (additionalDocument.IsPhase5Arrival && additionalDocument.IsATransportDocument)
			{
				var codeListType = additionalDocument.CodeListType;
				if (!codeListType.IsEmpty)
				{
					var cusCodeList = additionalDocument.GetRefCusCodeListByCodeType(codeListType);
					result = cusCodeList != null &&
						(cusCodeList.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes)
						|| cusCodeList.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListAttributeValues.No));
				}
			}

			return result;
		}

		bool StatusIsDEC => additionalDocument.CSI_Status == SupportingDocumentStatusList.Codes.DEC;

		bool StatusIsMIS => additionalDocument.CSI_Status == SupportingDocumentStatusList.Codes.MIS;

		readonly NctsAdditionalInfo additionalDocument;

		#endregion
	}
}
