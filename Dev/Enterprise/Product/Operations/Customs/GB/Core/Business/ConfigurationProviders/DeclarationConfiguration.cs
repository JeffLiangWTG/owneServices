using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business
{
	public class DeclarationConfiguration : EU.Business.DeclarationConfiguration
	{
		protected override EU.Business.InvoiceHeaderConfiguration GetNewInvoiceHeaderConfiguration() => new InvoiceHeaderConfiguration();
		protected override EU.Business.InvoiceLineConfiguration GetNewInvoiceLineConfiguration() => new InvoiceLineConfiguration();
		protected override EU.Business.EntryLineConfiguration GetNewEntryLineConfiguration() => new EntryLineConfiguration();
		protected override EU.Business.InstructionConfiguration GetNewInstructionConfiguration() => new InstructionConfiguration();

		protected override ZBool MiscGuaranteesSupportCore(BusinessObject businessObject) => true;
		protected override ZBool UseUniversalFeeCalculationCore(BusinessObject businessObject) => businessObject is JobDeclaration dec;
		protected override ZBool IsUCC5Core(BusinessObject businessObject) => businessObject is JobDeclaration jobDeclaration && (jobDeclaration.IsExport || jobDeclaration.IsImport);
		protected override ZBool UseEoriForFiscalReferenceCore => true;
		protected override ZBool UseEucdmSupportingDocumentGoodsShipmentCore => false;
	}
}
