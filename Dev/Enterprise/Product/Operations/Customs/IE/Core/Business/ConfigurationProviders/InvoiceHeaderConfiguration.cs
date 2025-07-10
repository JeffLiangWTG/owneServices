using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class InvoiceHeaderConfiguration : EU.Business.InvoiceHeaderConfiguration
	{
		protected override ZBool ValueIndicatorsSupportCore(BusinessObject businessObject) => ((JobDeclaration)businessObject).IsImport;

		protected override ZBool AgreedPlaceCodeSupportCore(BusinessObject businessObject) => true;

		protected override IAdditionalInfoValidationDecider UCC6ImportAdditionalInfoValidationDecider => new Declaration.UCC6ImportAdditionalInfoValidationDecider();

		protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();

		protected override EU.Business.Declaration.IInvoiceHeaderValidationDecider GetValidationDeciderCore(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader)
			=> invoiceHeader?.JobDeclaration switch
			{
				{ IsUCC6AndIsImport: true } => new UCC6ImportInvoiceHeaderValidationDecider(),
				{ IsUCC5: true } => new UCC5ImportInvoiceHeaderValidationDecider(),
				_ => null
			};
	}
}
