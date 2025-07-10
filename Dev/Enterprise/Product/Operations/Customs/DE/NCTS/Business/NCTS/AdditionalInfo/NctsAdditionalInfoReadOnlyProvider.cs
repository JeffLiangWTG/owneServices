using System;
using CargoWise.Common;
using Enterprise.Customs.EU.NCTS.Business;
using DEReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business
{
	sealed class NctsAdditionalInfoReadOnlyProvider : IAdditionalDocumentReadOnlyProvider
	{
		public NctsAdditionalInfoReadOnlyProvider(NctsAdditionalInfo additionalDocument, IAdditionalDocumentReadOnlyProvider baseReadOnlyProvider)
		{
			this.additionalDocument = Argument.NotNull(additionalDocument, nameof(additionalDocument));
			this.baseReadOnlyProvider = Argument.NotNull(baseReadOnlyProvider, nameof(baseReadOnlyProvider));
			referenceNumberReadOnlyLazy = new Lazy<bool>(GetReferenceNumberReadOnlyStatus);
			descriptionReadOnlyLazy = new Lazy<bool>(GetDescriptionReadOnlyStatus);
		}

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumberReadOnly => referenceNumberReadOnlyLazy.Value;

		bool IAdditionalDocumentReadOnlyProvider.ReferenceNumber2ReadOnly => baseReadOnlyProvider.ReferenceNumber2ReadOnly;

		bool IAdditionalDocumentReadOnlyProvider.DescriptionReadOnly => descriptionReadOnlyLazy.Value;

		bool IAdditionalDocumentReadOnlyProvider.AdditionalInfoReadOnly => false;

		bool IAdditionalDocumentReadOnlyProvider.LineNoReadOnly => baseReadOnlyProvider.LineNoReadOnly;

		bool IAdditionalDocumentReadOnlyProvider.StatusReadOnly => baseReadOnlyProvider.StatusReadOnly;

		#region Implementation

		bool GetDescriptionReadOnlyStatus()
		{
			return !additionalDocument.IsAnAdditionalInformation
					&& (additionalDocument.CSI_Code.IsEmpty
							|| (additionalDocument.RefCusCode != null
									&& CusSupportingInfoHelper.MissesAttribute(additionalDocument.RefCusCode, DEReferenceConstants.RefCusCodeListAttributes.Name.Complement)));
		}

		bool GetReferenceNumberReadOnlyStatus()
		{
			return additionalDocument.CSI_Code.IsEmpty
						|| (additionalDocument.RefCusCode != null
								&& CusSupportingInfoHelper.MissesAttribute(additionalDocument.RefCusCode, DEReferenceConstants.RefCusCodeListAttributes.Name.Reference));
		}

		readonly NctsAdditionalInfo additionalDocument;
		readonly IAdditionalDocumentReadOnlyProvider baseReadOnlyProvider;
		readonly Lazy<bool> descriptionReadOnlyLazy;
		readonly Lazy<bool> referenceNumberReadOnlyLazy;

		#endregion
	}
}
