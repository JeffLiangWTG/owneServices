using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business
{
	public class InvoiceHeaderConfiguration : EU.Business.InvoiceHeaderConfiguration
	{
		protected override ZBool ValueIndicatorsSupportCore(BusinessObject businessObject) => ((JobDeclaration)businessObject).IsImport;

		protected override ZBool MultipleSupportingDocumentsForAllInvoiceNumbersSupportCore(BusinessObject businessObject)
		{
			var multipleSupportingDocumentsForAllInvoiceNumbersSupport = (ZBool)Registry.GBCustomsDataRegistry.Instance.N935_AddAllInvoiceHeaderNumbersToAllInvoiceHeaders.Value;

			return ((JobDeclaration)businessObject).IsImport ? multipleSupportingDocumentsForAllInvoiceNumbersSupport :
				((JobDeclaration)businessObject).IsExport
				? multipleSupportingDocumentsForAllInvoiceNumbersSupport : ZBool.False;
		}

		protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();
	}
}
