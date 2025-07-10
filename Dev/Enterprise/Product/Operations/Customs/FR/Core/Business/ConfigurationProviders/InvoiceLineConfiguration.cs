using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business
{
	public class InvoiceLineConfiguration : EU.Business.InvoiceLineConfiguration
	{
		protected override ZBool TaxSupportCore(BusinessObject businessObject) => false;

		protected override ZBool FiscalReferencesSupportCore(BusinessObject businessObject) => true;

		protected override ZBool DefaultCountryOfSupplyFromSupplierCore(BusinessObject businessObject) => true;

		protected override ZBool CountryOfSupplyMustBeTheSameForAllLinesOnInstructionCore(BusinessObject businessObject) => true;

		protected override ZBool InflateItemPriceByValuationMarkupCore(BusinessObject businessObject) => true;

		protected override ZBool OrganizationsSupportCore(BusinessObject businessObject) => businessObject is JobDeclaration declaration && declaration.IsUCC6AndIsImport;

		protected override ZBool CountryOfDestinationVisibleOnImportControlCore(BusinessObject businessObject) => businessObject is JobDeclaration declaration && declaration.IsUCC6AndIsImport;

		protected override ZBool AdditionalSupplyChainActorSupportCore(BusinessObject businessObject) => false;

		protected override EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new UCC6ImportSupportingDocumentValidationDecider();

		protected override EU.Business.Declaration.IInvoiceLineValidationDecider GetValidationDeciderCore(EU.Business.Declaration.JobComInvoiceLine invoiceLine) => invoiceLine?.Declaration?.IsUCC6AndIsImport ?? false ? new UCC6ImportInvoiceLineValidationDecider(invoiceLine as JobComInvoiceLine) : null;

		protected override EU.Business.Declaration.MultiLineAddInfos.IAdditionalInfoValidationDecider UCC6ImportAdditionalInfoValidationDecider => new UCC6ImportAdditionalInfoValidationDecider();

		protected override EU.Business.Declaration.MultiLineAddInfos.IPreviousDocumentValidationDecider UCC6ImportPreviousDocumentValidationDecider => new UCC6ImportPreviousDocumentValidationDecider();
	}
}
