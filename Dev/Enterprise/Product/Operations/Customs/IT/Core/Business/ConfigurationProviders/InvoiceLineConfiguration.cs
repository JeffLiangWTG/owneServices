using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business;

public class InvoiceLineConfiguration : EU.Business.InvoiceLineConfiguration
{
	protected override ZBool TaxSupportCore(BusinessObject businessObject) => businessObject is CusClassPartPivot;

	protected override ZBool AuthorisationsSupportForInvoiceLineCore(JobDeclaration declaration) => !declaration.IsImport && base.AuthorisationsSupportForInvoiceLineCore(declaration);

	protected override ZBool AdditionalSupplyChainActorSupportCore(BusinessObject businessObject) => false;

	protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();

	protected override IInvoiceLineValidationDecider GetValidationDeciderCore(JobComInvoiceLine invoiceLine)
	{
		return invoiceLine.Declaration?.IsUCC6AndIsImport ?? false
			? new Declaration.UCC6ImportInvoiceLineValidationDecider()
			: null;
	}
}
