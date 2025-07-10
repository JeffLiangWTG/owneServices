using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using JobComInvoiceHeader = Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader;

namespace Enterprise.Customs.FR.Business
{
	public class InvoiceHeaderConfiguration : EU.Business.InvoiceHeaderConfiguration
	{
		protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();

		protected override IInvoiceHeaderValidationDecider GetValidationDeciderCore(JobComInvoiceHeader invoiceHeader) => invoiceHeader.JobDeclaration?.IsUCC6AndIsImport ?? false ? new Declaration.UCC6ImportInvoiceHeaderValidationDecider(invoiceHeader as Declaration.JobComInvoiceHeader) : null;

		protected override IAdditionalInfoValidationDecider UCC6ImportAdditionalInfoValidationDecider => new Declaration.UCC6ImportAdditionalInfoValidationDecider();

		protected override IPreviousDocumentValidationDecider UCC6ImportPreviousDocumentValidationDecider => new FR.Business.Declaration.UCC6ImportPreviousDocumentValidationDecider();
	}
}
