using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsHeaderArrivalAdditionalDocumentReadOnlyProvider : IAdditionalDocumentReadOnlyProvider
	{
		public NctsHeaderArrivalAdditionalDocumentReadOnlyProvider(NctsAdditionalInfo additionalDocument)
		{
			this.additionalDocument = Argument.NotNull(additionalDocument, nameof(additionalDocument));
			referenceNumberReadOnlyLazy = new Lazy<bool>(GetReferenceNumberReadOnlyStatus);
		}

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumberReadOnly => referenceNumberReadOnlyLazy.Value;

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumber2ReadOnly => false;

		bool IAdditionalDocumentReadOnlyProvider.DescriptionReadOnly => GetDescriptionReadOnlyStatus();

		bool IAdditionalDocumentReadOnlyProvider.AdditionalInfoReadOnly => GetAdditionalInfoReadOnlyStatus();

		bool IAdditionalDocumentReadOnlyProvider.LineNoReadOnly => additionalDocument.IsPhase5;

		bool IAdditionalDocumentReadOnlyProvider.StatusReadOnly => additionalDocument.IsPhase5;

		#region Implementation

		bool GetDescriptionReadOnlyStatus()
		{
			if (additionalDocument.Parent is NctsCommonMovementHeader
				&& additionalDocument.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation)
			{
				return false;
			}

			return true;
		}

		bool GetReferenceNumberReadOnlyStatus()
		{
			var result = true;
			if (additionalDocument.Parent is NctsCommonMovementHeader header)
			{
				var factory = additionalDocument.Factory;
				var cusCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(
					factory,
					additionalDocument.CSI_Code,
					header.DefaultDataGroupingCode,
					additionalDocument.CodeListType,
					ZDateTime.Today);

				result = !(cusCodeList?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference) ?? false);
			}

			return result;
		}

		bool GetAdditionalInfoReadOnlyStatus()
		{
			return (additionalDocument.CSI_Status == NctsUnloadedStateList.Codes.DEC && additionalDocument.IsPhase5) ||
				(additionalDocument.FieldsReadOnlyForPhase5Arrival);
		}

		readonly NctsAdditionalInfo additionalDocument;
		readonly Lazy<bool> referenceNumberReadOnlyLazy;

		#endregion
	}
}
