using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class DeclarationConfiguration : EU.Business.DeclarationConfiguration
{
	protected override EU.Business.InvoiceHeaderConfiguration GetNewInvoiceHeaderConfiguration() => new InvoiceHeaderConfiguration();

	protected override EU.Business.InvoiceLineConfiguration GetNewInvoiceLineConfiguration() => new InvoiceLineConfiguration();

	protected override ZBool IsUCC6Core(BusinessObject businessObject) => true;

	protected override EU.Business.InstructionConfiguration GetNewInstructionConfiguration() => new InstructionConfiguration();

	protected override ZBool MiscAdditionalInfosSupportCore(BusinessObject businessObject) => !IsUCC6(businessObject);

	protected override ZBool MiscPreviousDocumentsSupportCore(BusinessObject businessObject) => !IsUCC6(businessObject);

	protected override ZBool MiscSupportingDocumentsSupportCore(BusinessObject businessObject) => !IsUCC6(businessObject);

	protected override ZBool UCCAdditionalInfosSupportCore(BusinessObject businessObject) => businessObject is JobDeclaration;

	protected override EU.Business.EntryLineConfiguration GetNewEntryLineConfiguration() => new EntryLineConfiguration();

	protected override EU.Business.Declaration.IDeclarationValidationDecider GetValidationDeciderCore(BusinessObject businessObject)
			=> businessObject switch
			{
				JobDeclaration { IsUCC6AndIsImport: true } => new UCC6ImportDeclarationValidationDecider(),
				JobDeclaration { IsUCC6AndIsExport: true } => new UCC6ExportDeclarationValidationDecider(),
				_ => null
			};
}
