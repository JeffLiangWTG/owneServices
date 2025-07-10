using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business
{
	public class InvoiceLineConfiguration : EU.Business.InvoiceLineConfiguration
	{
		protected override ZBool MethodOfPaymentVisibleOnImportControlCore(BusinessObject businessObject) => IsCDSDeclaration(businessObject);

		protected override ZBool CountryOfDestinationVisibleOnImportControlCore(BusinessObject businessObject) => IsCDSDeclaration(businessObject);

		ZBool IsCDSDeclaration(BusinessObject businessObject)
		{
			return businessObject is JobDeclaration dec && dec.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
		}

		protected override ZBool TaxSupportCore(BusinessObject businessObject)
		{
			return (businessObject is JobDeclaration dec)
				? ZBool.False // CDS
				: ZBool.True; // pivot
		}

		protected override ZBool ValueIndicatorsSupportCore(BusinessObject businessObject)
		{
			var declaration = (JobDeclaration)businessObject;
			return declaration.IsImport;
		}

		protected override ZBool UseMultipleVatFieldsCore => false;

		protected override ZBool AdditionalSupplyChainActorSupportCore(BusinessObject businessObject) => false;

		protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();
	}
}
