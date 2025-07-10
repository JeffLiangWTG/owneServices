using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

sealed class InvoiceHeaderConfiguration : EU.Business.InvoiceHeaderConfiguration
{
	protected override ZBool AdditionalInfosSupportCore(BusinessObject businessObject)
		=> businessObject is JobDeclaration declaration && declaration.IsUCC6AndIsExport;

	protected override ZBool PreviousDocumentsSupportCore(EU.Business.Declaration.JobDeclaration declaration) => !declaration.IsUCC6;

	protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();

	protected override EU.Business.Declaration.IInvoiceHeaderValidationDecider GetValidationDeciderCore(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader) => invoiceHeader.JobDeclaration?.IsUCC6AndIsImport ?? false ? new UCC6ImportInvoiceHeaderValidationDecider() : null;
}
