using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.Business;

public class InvoiceHeaderConfiguration : EU.Business.InvoiceHeaderConfiguration
{
	protected override ZBool AgreedPlaceCodeSupportCore(BusinessObject businessObject) => true;

	protected override EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();
}
