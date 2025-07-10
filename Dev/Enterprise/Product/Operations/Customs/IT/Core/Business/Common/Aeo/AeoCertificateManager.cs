using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class AeoCertificateManager
{
	public AeoCertificateManager(IAeoCertificateSupporter aeoCertificateSupporter, ISupportingDocumentsProvider supportingDocumentProvider)
	{
		this.aeoCertificateSupporter = aeoCertificateSupporter;
		this.supportingDocumentProvider = Argument.NotNull(supportingDocumentProvider, nameof(supportingDocumentProvider));
	}
	readonly IAeoCertificateSupporter aeoCertificateSupporter;
	readonly ISupportingDocumentsProvider supportingDocumentProvider;

	public void AddAeoCertificatesIfNeeded()
	{
		AddAeoCertificateFromSupplierIfNeeded();
		AddAeoCertificateFromImporterIfNeeded();
		AddAeoCertificateFromDeclarantIfNeeded();
	}

	public void AddAeoCertificateFromSupplierIfNeeded()
	{
		if (aeoCertificateSupporter?.ShouldAddY022Certificate ?? ZBool.False)
		{
			AddAeoCertificateIfNotEmpty(UniversalReferenceConstants.SupportingDocumentTypes.Y022, aeoCertificateSupporter.Supplier);
		}
	}

	public void AddAeoCertificateFromImporterIfNeeded()
	{
		if (aeoCertificateSupporter?.ShouldAddY023Certificate ?? ZBool.False)
		{
			AddAeoCertificateIfNotEmpty(UniversalReferenceConstants.SupportingDocumentTypes.Y023, aeoCertificateSupporter.Importer);
		}
	}

	public void AddAeoCertificateFromDeclarantIfNeeded()
	{
		if (aeoCertificateSupporter != null && aeoCertificateSupporter.RepresentationType != RepresentationTypeList.Codes._1Self)
		{
			AddAeoCertificateIfNotEmpty(UniversalReferenceConstants.SupportingDocumentTypes.Y024, aeoCertificateSupporter.Declarant);
		}
	}

	void AddAeoCertificateIfNotEmpty(ZString aeoDocumentCode, OrgHeader organisation)
	{
		var aeoIdentificationNumber = organisation?.GetAeoCode() ?? ZString.Empty;
		if (!aeoIdentificationNumber.IsEmpty)
		{
			supportingDocumentProvider.SupportingDocuments.AddNewIfNotExsistWithSameCodeAndReference(aeoDocumentCode, aeoIdentificationNumber);
		}
	}
}
