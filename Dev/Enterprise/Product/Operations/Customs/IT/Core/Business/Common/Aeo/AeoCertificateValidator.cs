using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business;

public class AeoCertificateValidator
{
	public AeoCertificateValidator(IAeoCertificateSupporter aeoCertificateSupporter, IAeoCertificateOrganisationCaptionProvider aeoCertificateOrganisationCaptionProvider)
	{
		this.aeoCertificateSupporter = Argument.NotNull(aeoCertificateSupporter, nameof(aeoCertificateSupporter));
		this.aeoCertificateOrganisationCaptionProvider = Argument.NotNull(aeoCertificateOrganisationCaptionProvider, nameof(aeoCertificateOrganisationCaptionProvider));
	}
	readonly IAeoCertificateSupporter aeoCertificateSupporter;
	readonly IAeoCertificateOrganisationCaptionProvider aeoCertificateOrganisationCaptionProvider;

	public void CheckAEOCertificate(SupportingDocument supportingDocument)
	{
		Argument.NotNull(supportingDocument, nameof(supportingDocument));

		var certificateCode = supportingDocument.CSI_Code;
		if (IsAeoCertificateCode(certificateCode))
		{
			(var aeoCertificateCode, var organisationCaption) = GetAeoAndOrganisationDescription(certificateCode);

			var referenceNumberInfo = supportingDocument.CSI_ReferenceNumberInfo;
			if (aeoCertificateCode.IsEmpty)
			{
				referenceNumberInfo.AddWarning(ValidationCaptions.SupportingDocument.GetMissingAEOCertificateMessage(organisationCaption));
			}
			else if (aeoCertificateCode != supportingDocument.CSI_ReferenceNumber)
			{
				referenceNumberInfo.AddWarning(ValidationCaptions.SupportingDocument.GetUnmatchedAEOCertificateMessage(organisationCaption));
			}
		}
	}

	#region Implementation

	(ZString aeoCode, ZString organisationCaption) GetAeoAndOrganisationDescription(ZString certificateCode)
	{
		OrgHeader organisation = null;
		ZString organisationCaption = ZString.Empty;

		switch (certificateCode)
		{
			case SupportingDocumentTypes.Y022:
				organisation = aeoCertificateSupporter.Supplier;
				organisationCaption = aeoCertificateOrganisationCaptionProvider.SupplierCaption;
				break;

			case SupportingDocumentTypes.Y023:
				organisation = aeoCertificateSupporter.Importer;
				organisationCaption = aeoCertificateOrganisationCaptionProvider.ImporterCaption;
				break;

			case SupportingDocumentTypes.Y024:
				organisation = aeoCertificateSupporter.Declarant;
				organisationCaption = aeoCertificateOrganisationCaptionProvider.DeclarantCaption;
				break;
			default:
				break;
		}

		var aeoCode = organisation?.GetAeoCode() ?? ZString.Empty;
		return (aeoCode, organisationCaption);
	}

	ZBool IsAeoCertificateCode(ZString code) => aeoCodes.Contains(code);

	readonly ImmutableArray<string> aeoCodes = new string[]
	{
		SupportingDocumentTypes.Y022,
		SupportingDocumentTypes.Y023,
		SupportingDocumentTypes.Y024,
	}.ToImmutableArray();

	#endregion
}
