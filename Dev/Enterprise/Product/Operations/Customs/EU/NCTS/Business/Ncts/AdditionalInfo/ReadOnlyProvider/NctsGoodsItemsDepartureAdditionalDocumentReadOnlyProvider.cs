using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsGoodsItemsDepartureAdditionalDocumentReadOnlyProvider : IAdditionalDocumentReadOnlyProvider
	{
		public NctsGoodsItemsDepartureAdditionalDocumentReadOnlyProvider(NctsAdditionalInfo additionalDocument)
		{
			this.additionalDocument = Argument.NotNull(additionalDocument, nameof(additionalDocument));
			documentKind = additionalDocument.CSI_SubType;
			documentType = additionalDocument.CSI_Code;
			lazyIsPhase5DepartureAndDocTypeEmptyAndDocKindIsREF = new Lazy<bool>(GetIsPhase5DepartureAndDocTypeEmptyAndDocKindIsREF);
		}

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumberReadOnly => documentKind != AdditionalInfoSubTypeList.Codes.AdditionalReference && !HasReferenceAttributeAndIsPhase5AndDocKindIsTRA();

		public bool IsPhase5DepartureAndDocTypeEmptyAndDocKindIsREF => lazyIsPhase5DepartureAndDocTypeEmptyAndDocKindIsREF.Value;

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumber2ReadOnly => IsPhase5DepartureDocKindNotINFOrREF;

		bool IAdditionalDocumentReadOnlyProvider.DescriptionReadOnly => IsPhase5DepartureDocKindNotINFOrREF || IsPhase5DepartureAndDocTypeEmptyAndDocKindIsREF || IsPhase5DepartureDocKindREFAndDocTypeIsEmpty;

		bool IAdditionalDocumentReadOnlyProvider.AdditionalInfoReadOnly => false;

		bool IAdditionalDocumentReadOnlyProvider.LineNoReadOnly => false;

		bool IAdditionalDocumentReadOnlyProvider.StatusReadOnly => false;

		#region Implementation

		bool IsPhase5DepartureDocKindNotINFOrREF => IsPhase5 && documentKind != AdditionalInfoSubTypeList.Codes.AdditionalInformation && documentKind != AdditionalInfoSubTypeList.Codes.AdditionalReference;

		bool IsPhase5 => additionalDocument.IsPhase5;

		bool HasReferenceAttributeAndIsPhase5AndDocKindIsTRA()
		{
			var result = false;
			if (IsPhase5 && additionalDocument.IsATransportDocument)
			{
				var codeListType = additionalDocument.CodeListType;
				if (!codeListType.IsEmpty)
				{
					var cusCodeList = additionalDocument.GetRefCusCodeListByCodeType(codeListType);
					result = cusCodeList == null
								|| cusCodeList.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes)
								|| cusCodeList.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);
				}
			}

			return result && !documentType.IsEmpty;
		}

		bool IsPhase5DepartureDocKindREFAndDocTypeIsEmpty => IsPhase5 && documentKind == AdditionalInfoSubTypeList.Codes.AdditionalReference && documentType.IsEmpty;

		bool GetIsPhase5DepartureAndDocTypeEmptyAndDocKindIsREF()
		{
			var header = additionalDocument.ParentAsGoodsItem?.Header;

			if (header is null
			  || documentType.IsEmpty
			  || documentKind != AdditionalInfoSubTypeList.Codes.AdditionalReference
			  || !IsPhase5)
			{
				return false;
			}

			return documentType.IsEmpty;
		}

		readonly NctsAdditionalInfo additionalDocument;
		readonly ZString documentKind;
		readonly ZString documentType;
		readonly Lazy<bool> lazyIsPhase5DepartureAndDocTypeEmptyAndDocKindIsREF;

		#endregion
	}
}
