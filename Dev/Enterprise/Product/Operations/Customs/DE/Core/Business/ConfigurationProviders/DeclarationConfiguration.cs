using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public class DeclarationConfiguration : EU.Business.DeclarationConfiguration
	{
		protected override ZBool MiscAdditionalInfosSupportCore(BusinessObject businessObject) => false;

		protected override ZBool MiscSupportingDocumentsSupportCore(BusinessObject businessObject) => false;

		protected override ZBool MiscPreviousDocumentsSupportCore(BusinessObject businessObject) => false;

		protected override ZBool DV1DetailsSupportCore(BusinessObject businessObject) => true;

		protected override EU.Business.InvoiceHeaderConfiguration GetNewInvoiceHeaderConfiguration() => new InvoiceHeaderConfiguration();

		protected override EU.Business.InvoiceLineConfiguration GetNewInvoiceLineConfiguration() => new InvoiceLineConfiguration();

		protected override EU.Business.InstructionConfiguration GetNewInstructionConfiguration() => new InstructionConfiguration();

		protected override ZBool UseUniversalFeeCalculationCore(BusinessObject businessObject) => true;

		protected override ZBool IsUCC6Core(BusinessObject businessObject) => (businessObject is Declaration.JobDeclaration jobDeclaration) && jobDeclaration.IsExport;

		protected override ZBool UCCAdditionalInfosSupportCore(BusinessObject businessObject) => (businessObject is Declaration.JobDeclaration jobDeclaration) && jobDeclaration.IsExport;

		protected override ZBool UseEucdmSupportingDocumentGoodsShipmentCore => false;

		protected override ZBool LockNumberOfEntryLinesForRegisteredEntryCore => true;
	}
}
