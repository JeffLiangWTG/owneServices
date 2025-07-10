using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business;

sealed class InstructionConfiguration : EU.Business.InstructionConfiguration
{
	protected override ZBool GuaranteesSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => (declaration?.IsImport ?? ZBool.False) && IsGuaranteeSupportedForEntryInstructionStyle(entryInstruction?.CEI_Style ?? string.Empty);

	protected override ZBool SealsSupportCore(JobDeclaration declaration) => !(declaration?.IsUCC6 ?? ZBool.False);

	protected override ZBool AdditionalSupplyChainActorSupportCore(JobDeclaration declaration) => declaration?.IsUCC6 ?? ZBool.False;

	protected override ZBool AdditionalInfosSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => declaration?.IsUCC6AndIsExport ?? ZBool.False;

	protected override ZBool SupportingDocumentsSupportCore(JobDeclaration declaration) => declaration?.IsUCC6 ?? ZBool.False;

	protected override ZBool PreviousDocumentsSupportCore(JobDeclaration declaration) => declaration?.IsUCC6 ?? ZBool.False;

	protected override IEntryInstructionValidationDecider GetImportValidationDecider(CusEntryInstruction cusEntryInstruction) => new Declaration.UCC6ImportEntryInstructionValidationDecider();

	protected override IEntryInstructionValidationDecider GetExportValidationDecider(CusEntryInstruction cusEntryInstruction) => null;

	protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();

	ZBool IsGuaranteeSupportedForEntryInstructionStyle(string type)
	{
		return stylesSupportGuarantees.Contains(type);
	}

	readonly ImmutableArray<string> stylesSupportGuarantees = new string[]
	{
		ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1,
		ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3,
		ImportUCC6DeclarationTypeList.Codes.RegimeSpecialePerfezionamentoAttivoH4,
	}.ToImmutableArray();
}
