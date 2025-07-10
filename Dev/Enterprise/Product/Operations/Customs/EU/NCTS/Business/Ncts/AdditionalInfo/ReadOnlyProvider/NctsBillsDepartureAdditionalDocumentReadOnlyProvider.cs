using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsBillsDepartureAdditionalDocumentReadOnlyProvider : IAdditionalDocumentReadOnlyProvider
	{
		public NctsBillsDepartureAdditionalDocumentReadOnlyProvider(NctsBillAdditionalDocument additionalDocument)
		{
			this.additionalDocument = Argument.NotNull(additionalDocument, nameof(additionalDocument));
			docKind = additionalDocument.CSI_SubType;
		}

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumberReadOnly
			=> IsStatusDeclared
			|| IsReferenceNumberReadOnlyWhenPhase5Departure
			|| ((additionalDocument.Parent?.Header?.IsDepartureMovement ?? false) && docKind == AdditionalInfoSubTypeList.Codes.AdditionalInformation);

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumber2ReadOnly => false;

		bool IAdditionalDocumentReadOnlyProvider.DescriptionReadOnly => IsStatusDeclared || IsDescriptionReadOnlyWhenPhase5Departure;

		bool IAdditionalDocumentReadOnlyProvider.AdditionalInfoReadOnly => IsStatusDeclared;

		bool IAdditionalDocumentReadOnlyProvider.LineNoReadOnly => true;

		bool IAdditionalDocumentReadOnlyProvider.StatusReadOnly => true;

		#region Implementation

		bool IsStatusDeclared => additionalDocument.CSI_Status == NctsBillAdditionalDocumentStatusList.Codes.DEC;

		bool IsDescriptionReadOnlyWhenPhase5Departure => IsPhase5 && (IsDocKindNotInList || DocKindIsREFOrTRAAndDocTypeIsEmpty);

		bool DocKindIsREFOrTRAAndDocTypeIsEmpty => (docKind == AdditionalInfoSubTypeList.Codes.AdditionalReference || docKind == AdditionalInfoSubTypeList.Codes.TransportDocument) && DocTypeIsEmpty;

		bool IsReferenceNumberReadOnlyWhenPhase5Departure => IsPhase5 && (IsDocKindNotInList || DocKindIsINFAndDocTypeIsEmpty);

		bool IsPhase5 => additionalDocument.Parent?.Header?.IsPhase5 ?? ZBool.False;

		bool DocKindIsINFAndDocTypeIsEmpty => docKind == AdditionalInfoSubTypeList.Codes.AdditionalInformation && DocTypeIsEmpty;

		bool IsDocKindNotInList => docKind != AdditionalInfoSubTypeList.Codes.AdditionalInformation && docKind != AdditionalInfoSubTypeList.Codes.AdditionalReference && docKind != AdditionalInfoSubTypeList.Codes.TransportDocument;

		bool DocTypeIsEmpty => additionalDocument.CSI_Code.IsEmpty;

		readonly NctsBillAdditionalDocument additionalDocument;
		readonly string docKind;

		#endregion
	}
}
