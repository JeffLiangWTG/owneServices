using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class InvoiceLineConfiguration : EU.Business.InvoiceLineConfiguration
{
	protected override ZBool CountryOfDestinationVisibleOnImportControlCore(BusinessObject businessObject) => true;

	protected override ZBool AuthorisationsSupportForInvoiceLineCore(JobDeclaration declaration) => true;

	protected override ZBool OrganizationsSupportCore(BusinessObject businessObject) => true;

	protected override ZBool AdditionalSupplyChainActorSupportCore(BusinessObject businessObject) => false;

	protected override ZBool FiscalReferencesSupportCore(BusinessObject businessObject) => false;

	protected override IInvoiceLineValidationDecider GetValidationDeciderCore(JobComInvoiceLine invoiceLine)
	{
		return invoiceLine.Declaration?.IsImport ?? false
			? new Declaration.UCC6ImportInvoiceLineValidationDecider()
			: new Declaration.UCC6ExportInvoiceLineValidationDecider();
	}
}
