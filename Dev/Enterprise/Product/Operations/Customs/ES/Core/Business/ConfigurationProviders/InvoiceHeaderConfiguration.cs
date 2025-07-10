using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.ES.Business
{
	public class InvoiceHeaderConfiguration : EU.Business.InvoiceHeaderConfiguration
	{
		protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();
	}
}
