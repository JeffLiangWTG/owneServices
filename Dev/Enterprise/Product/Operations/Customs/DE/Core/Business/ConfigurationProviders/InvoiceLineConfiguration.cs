using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class InvoiceLineConfiguration : EU.Business.InvoiceLineConfiguration
	{
		protected override ZBool SecondQuotaVisibleCore(BusinessObject businessObject) => true;

		protected override ZBool PreviousDocumentsSupportCore(BusinessObject businessObject)
		{
			switch (businessObject)
			{
				case JobDeclaration declaration:
					return declaration.IsExport();
				case CusClassPartPivot pivot:
					return pivot.IsExport();
				default:
					return false;
			}
		}

		protected override ZBool TaxSupportCore(BusinessObject businessObject) => !(businessObject is IImportExport importExport) || importExport.IsImport();

		protected override ZBool OrganizationsSupportCore(BusinessObject businessObject) => (businessObject is EU.Business.Declaration.JobDeclaration declaration) && declaration.IsExport();

		protected override ZBool CountryOfDestinationVisibleOnExportControlCore(BusinessObject businessObject) => false;

		protected override ZBool AdditionalSupplyChainActorSupportCore(BusinessObject businessObject) => false;

		protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new UCC6ImportSupportingDocumentValidationDecider();
	}
}
