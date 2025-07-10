using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class InstructionConfiguration : EU.Business.InstructionConfiguration
{
	protected override ZBool GuaranteesSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => declaration.IsImport;

	protected override ZBool PreviousDocumentsSupportCore(JobDeclaration declaration) => true;

	protected override ZBool AdditionalInfosSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => true;

	protected override ZBool SupportingDocumentsSupportCore(JobDeclaration declaration) => true;

	protected override EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();

	protected override IEntryInstructionValidationDecider GetImportValidationDecider(CusEntryInstruction cusEntryInstruction) => new Declaration.UCC6ImportEntryInstructionValidationDecider();

	protected override IEntryInstructionValidationDecider GetExportValidationDecider(CusEntryInstruction cusEntryInstruction) => null;
}
