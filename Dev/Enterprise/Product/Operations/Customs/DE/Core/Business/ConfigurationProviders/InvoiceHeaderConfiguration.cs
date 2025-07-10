using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class InvoiceHeaderConfiguration : EU.Business.InvoiceHeaderConfiguration
	{
		protected override ZBool AdditionalInfosSupportCore(BusinessObject businessObject) => businessObject is IImportExport importExport && importExport.IsExport();

		protected override ZBool SupportingDocumentsSupportCore(BusinessObject businessObject) => !(businessObject is IImportExport importExport) || importExport.IsImport() || importExport.IsExport();

		protected override ZBool PreviousDocumentsSupportCore(EU.Business.Declaration.JobDeclaration declaration) => declaration.IsExport;

		protected override ZBool AgreedPlaceCodeSupportCore(BusinessObject businessObject) => businessObject is IImportExport importExport && importExport.IsExport();

		protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new UCC6ImportSupportingDocumentValidationDecider();
	}
}
