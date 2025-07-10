using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business
{
	public class InvoiceHeaderConfiguration
	{
		public ZBool InvoicePaymentSupport(BusinessObject businessObject) => InvoicePaymentSupportCore(businessObject);
		protected virtual ZBool InvoicePaymentSupportCore(BusinessObject businessObject) => false;

		public ZBool AdditionalInfosSupport(BusinessObject businessObject) => AdditionalInfosSupportCore(businessObject);
		protected virtual ZBool AdditionalInfosSupportCore(BusinessObject businessObject) => true;

		public ZBool SupportingDocumentsSupport(BusinessObject businessObject) => SupportingDocumentsSupportCore(businessObject);
		protected virtual ZBool SupportingDocumentsSupportCore(BusinessObject businessObject) => true;

		public ZBool PreviousDocumentsSupport(JobDeclaration declaration) => PreviousDocumentsSupportCore(declaration);
		protected virtual ZBool PreviousDocumentsSupportCore(JobDeclaration declaration) => true;

		public ZBool MultipleSupportingDocumentsForAllInvoiceNumbersSupport(BusinessObject businessObject) => MultipleSupportingDocumentsForAllInvoiceNumbersSupportCore(businessObject);

		protected virtual ZBool MultipleSupportingDocumentsForAllInvoiceNumbersSupportCore(BusinessObject businessObject) => false;

		public ZBool TaxSupport(BusinessObject businessObject) => TaxSupportCore(businessObject);
		protected virtual ZBool TaxSupportCore(BusinessObject businessObject) => true;

		public ZBool ValueIndicatorsSupport(BusinessObject businessObject) => ValueIndicatorsSupportCore(businessObject);
		protected virtual ZBool ValueIndicatorsSupportCore(BusinessObject businessObject) => businessObject is JobDeclaration declaration && declaration.IsUCC6AndIsImport;

		public ZBool AgreedPlaceCodeSupport(BusinessObject businessObject) => AgreedPlaceCodeSupportCore(businessObject);
		protected virtual ZBool AgreedPlaceCodeSupportCore(BusinessObject businessObject) => businessObject is JobDeclaration declaration && declaration.IsUCC6;

		public ISupportingDocumentValidationDecider GetSupportingDocumentValidationDecider(JobComInvoiceHeader invoiceHeader) => GetSupportingDocumentValidationDeciderCore(invoiceHeader);
		protected virtual ISupportingDocumentValidationDecider GetSupportingDocumentValidationDeciderCore(JobComInvoiceHeader invoiceHeader) => (invoiceHeader as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? UCC6ImportSupportingDocumentValidationDecider : null;
		protected virtual ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new UCC6ImportSupportingDocumentValidationDecider();

		public IAdditionalInfoValidationDecider GetAdditionalInfoValidationDecider(JobComInvoiceHeader invoiceHeader) => GetAdditionalInfoValidationDeciderCore(invoiceHeader);
		protected virtual IAdditionalInfoValidationDecider GetAdditionalInfoValidationDeciderCore(JobComInvoiceHeader invoiceHeader) => (invoiceHeader as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? UCC6ImportAdditionalInfoValidationDecider : null;
		protected virtual IAdditionalInfoValidationDecider UCC6ImportAdditionalInfoValidationDecider => new UCC6ImportAdditionalInfoValidationDecider();

		public IInvoiceHeaderValidationDecider GetValidationDecider(JobComInvoiceHeader invoiceHeader) => GetValidationDeciderCore(invoiceHeader);

		protected virtual IInvoiceHeaderValidationDecider GetValidationDeciderCore(JobComInvoiceHeader invoiceHeader) => (invoiceHeader.JobDeclaration?.IsUCC6AndIsImport ?? false) ? UCC6ImportInvoiceHeaderValidationDecider : null;

		protected virtual IInvoiceHeaderValidationDecider UCC6ImportInvoiceHeaderValidationDecider => new UCC6ImportInvoiceHeaderValidationDecider();

		public IPreviousDocumentValidationDecider GetPreviousDocumentValidationDecider(JobComInvoiceHeader invoiceHeader) => GetPreviousDocumentValidationDeciderCore(invoiceHeader);
		protected virtual IPreviousDocumentValidationDecider GetPreviousDocumentValidationDeciderCore(JobComInvoiceHeader invoiceHeader) => (invoiceHeader as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? UCC6ImportPreviousDocumentValidationDecider : null;
		protected virtual IPreviousDocumentValidationDecider UCC6ImportPreviousDocumentValidationDecider => new UCC6ImportPreviousDocumentValidationDecider();

		public ZBool ExportCostCalculationsTotalsUISupport => ExportCostCalculationsTotalsUISupportCore;
		protected virtual ZBool ExportCostCalculationsTotalsUISupportCore => false;
	}
}
