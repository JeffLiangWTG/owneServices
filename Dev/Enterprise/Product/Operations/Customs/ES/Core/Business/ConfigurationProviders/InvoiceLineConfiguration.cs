using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.ES.Business;

public class InvoiceLineConfiguration : EU.Business.InvoiceLineConfiguration
{
	protected override ZBool TaxSupportCore(BusinessObject businessObject) => false;

	protected override ZBool MethodOfPaymentVisibleOnImportControlCore(BusinessObject businessObject) => true;

	protected override ZBool CountryOfDestinationVisibleOnExportControlCore(BusinessObject businessObject) => false;

	protected override ZBool OrganizationsSupportCore(BusinessObject businessObject)
		=> (businessObject is JobDeclaration declaration) && (declaration.IsExport || declaration.IsImport);

	protected override ZBool CountryOfDestinationVisibleOnImportControlCore(BusinessObject businessObject) => true;

	protected override ZBool AdditionalSupplyChainActorSupportCore(BusinessObject businessObject) => false;

	protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();
}
