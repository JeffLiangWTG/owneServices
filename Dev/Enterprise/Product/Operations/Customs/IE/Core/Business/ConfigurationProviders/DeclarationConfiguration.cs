using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class DeclarationConfiguration : EU.Business.DeclarationConfiguration
	{
		protected override EU.Business.InvoiceLineConfiguration GetNewInvoiceLineConfiguration() => new InvoiceLineConfiguration();

		protected override EU.Business.InvoiceHeaderConfiguration GetNewInvoiceHeaderConfiguration() => new InvoiceHeaderConfiguration();

		protected override EU.Business.InstructionConfiguration GetNewInstructionConfiguration() => new InstructionConfiguration();

		protected override EU.Business.EntryLineConfiguration GetNewEntryLineConfiguration() => new EntryLineConfiguration();

		protected override ZBool IsUCC5Core(BusinessObject businessObject) => businessObject is JobDeclaration declaration && declaration.IsImport && declaration.JE_ApplicationCode.EqualsIgnoringCase(ImportDeclarationApplicationCodeList.Codes.V1);

		protected override ZBool IsUCC6Core(BusinessObject businessObject) => businessObject is JobDeclaration && !IsUCC5(businessObject);

		protected override ZBool UCCAdditionalInfosSupportCore(BusinessObject businessObject) => true;

		protected override ZBool UseEucdmSupportingDocumentGoodsShipmentAndItemCore => true;

		protected override ZBool IsPopulateAuthorisationsForOfficeOfPresentationEnabledCore(EU.Business.Declaration.JobDeclaration declaration) => declaration.IsExport && !declaration.IsUXMLImportingData;
	}
}
