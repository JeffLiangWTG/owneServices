using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class InvoiceLineConfiguration : EU.Business.InvoiceLineConfiguration
{
	protected override ZBool OrganizationsSupportCore(BusinessObject businessObject) => true;

	protected override ZBool AuthorisationsSupportForInvoiceLineCore(JobDeclaration declaration) => true;

	protected override ZBool CountryOfDestinationVisibleOnImportControlCore(BusinessObject businessObject) => true;

	protected override ZBool CountryOfDestinationVisibleOnExportControlCore(BusinessObject businessObject) => true;

	protected override ZBool ValueIndicatorsSupportCore(BusinessObject businessObject) => true;

	protected override ZBool AdditionalSupplyChainActorSupportCore(BusinessObject businessObject) => false;

	protected override EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();
}
