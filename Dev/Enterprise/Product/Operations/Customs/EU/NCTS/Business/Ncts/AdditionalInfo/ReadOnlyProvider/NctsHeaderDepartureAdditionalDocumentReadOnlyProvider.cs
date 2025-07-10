using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsHeaderDepartureAdditionalDocumentReadOnlyProvider : IAdditionalDocumentReadOnlyProvider
	{
		public NctsHeaderDepartureAdditionalDocumentReadOnlyProvider(NctsAdditionalInfo additionalDocument)
		{
			this.additionalDocument = Argument.NotNull(additionalDocument, nameof(additionalDocument));
			referenceNumberReadOnlyLazy = new Lazy<bool>(GetReferenceNumberReadOnlyStatus);
			subType = additionalDocument.CSI_SubType;
		}

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumberReadOnly => referenceNumberReadOnlyLazy.Value;

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumber2ReadOnly => false;

		bool IAdditionalDocumentReadOnlyProvider.DescriptionReadOnly => GetDescriptionReadOnlyStatus();

		bool IAdditionalDocumentReadOnlyProvider.AdditionalInfoReadOnly => false;

		bool IAdditionalDocumentReadOnlyProvider.StatusReadOnly => false;

		bool IAdditionalDocumentReadOnlyProvider.LineNoReadOnly => false;

		#region Implementation

		bool GetDescriptionReadOnlyStatus()
		{
			if (additionalDocument.ParentAsNctsHeader != null
				&& subType == AdditionalInfoSubTypeList.Codes.AdditionalInformation)
			{
				return false;
			}

			return true;
		}

		bool GetReferenceNumberReadOnlyStatus()
		{
			var result = true;
			if (additionalDocument.Parent is NctsHeader header)
			{
				var factory = additionalDocument.Factory;
				if (additionalDocument.CSI_Code.IsEmpty)
				{
					result = !(additionalDocument.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalReference) || additionalDocument.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.TransportDocument));
				}
				else
				{
					var cusCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(
					factory,
					additionalDocument.CSI_Code,
					header.DefaultDataGroupingCode,
					additionalDocument.CodeListType,
					ZDateTime.Today);

					result = !(cusCodeList?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference) ?? true);
				}
			}
			return result;
		}

		readonly NctsAdditionalInfo additionalDocument;
		readonly Lazy<bool> referenceNumberReadOnlyLazy;
		readonly string subType;

		#endregion
	}
}
