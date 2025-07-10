using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IE.Business
{
	public class InvoiceLineConfiguration : EU.Business.InvoiceLineConfiguration
	{
		protected override ZBool CountryOfDestinationVisibleOnImportControlCore(BusinessObject businessObject) => true;

		protected override ZBool OrganizationsSupportCore(BusinessObject businessObject) => businessObject is Declaration.JobDeclaration jobDeclaration && (jobDeclaration.IsExport || jobDeclaration.IsUCC5AndIsImport);

		protected override ZBool AdditionalSupplyChainActorSupportCore(BusinessObject businessObject) => true;

		protected override ZBool FiscalReferencesSupportCore(BusinessObject businessObject) => businessObject is Declaration.JobDeclaration jobDeclaration && jobDeclaration.IsImport;

		protected override ZBool ValueIndicatorsSupportCore(BusinessObject businessObject) => businessObject is Declaration.JobDeclaration jobDeclaration && jobDeclaration.IsImport;

		protected override IInvoiceLineValidationDecider GetValidationDeciderCore(JobComInvoiceLine invoiceLine) => invoiceLine.Declaration?.IsUCC6AndIsImport ?? false ? new Declaration.UCC6ImportInvoiceLineValidationDecider() : null;

		protected override IAdditionalInfoValidationDecider UCC6ImportAdditionalInfoValidationDecider => new Declaration.UCC6ImportAdditionalInfoValidationDecider();

		protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();
	}
}
