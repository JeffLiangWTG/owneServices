using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.ES.Business;

public class InstructionConfiguration : EU.Business.InstructionConfiguration
{
	protected override ZBool AdditionalSupplyChainActorSupportCore(JobDeclaration declaration) => declaration.IsExport || declaration.IsImport;

	protected override ZBool SealsSupportCore(JobDeclaration declaration) => false;

	protected override ZBool AdditionalInfosSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) =>
										declaration != null
										&& (
											declaration.IsUCC6AndIsExport || declaration.IsImport
											|| (declaration.IsExport && entryInstruction != null && ((Declaration.CusEntryInstruction)entryInstruction).IsEXS)
										);

	protected override ZBool SupportingDocumentsSupportCore(JobDeclaration declaration) => true;

	protected override ZBool PreviousDocumentsSupportCore(JobDeclaration declaration) => declaration.IsUCC6AndIsImport;

	protected override IEntryInstructionValidationDecider GetImportValidationDecider(CusEntryInstruction cusEntryInstruction) => new Declaration.UCC6ImportEntryInstructionValidationDecider();

	protected override IEntryInstructionValidationDecider GetExportValidationDecider(CusEntryInstruction cusEntryInstruction) => null;

	protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();
}
