using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.FR.Business
{
	public class InstructionConfiguration : EU.Business.InstructionConfiguration
	{
		protected override ZBool FiscalReferencesSupportCore(BusinessObject businessObject) => true;

		protected override ZBool FiscalReferencesSupportOnCPC42And63OnlyCore(BusinessObject businessObject) => false;

		protected override ZBool AdditionalSupplyChainActorSupportCore(JobDeclaration declaration) => declaration.IsUCC6AndIsImport;

		protected override ZBool GuaranteesSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => declaration.IsUCC6 && IsGuaranteeSupportedForEntryInstructionStyle(entryInstruction?.CEI_Style ?? string.Empty);

		protected override IEntryInstructionValidationDecider GetImportValidationDecider(CusEntryInstruction cusEntryInstruction) => new Declaration.UCC6ImportEntryInstructionValidationDecider((Declaration.CusEntryInstruction)cusEntryInstruction);

		protected override IEntryInstructionValidationDecider GetExportValidationDecider(CusEntryInstruction cusEntryInstruction) => null;

		protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();

		protected override IAdditionalInfoValidationDecider GetAdditionalInfoValidationDeciderCore(CusEntryInstruction entryInstruction) => new FR.Business.Declaration.UCC6ImportAdditionalInfoValidationDecider();

		protected override ZBool AdditionalInfosSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => declaration.IsUCC6;

		protected override ZBool SupportingDocumentsSupportCore(JobDeclaration declaration) => declaration.IsUCC6;

		protected override ZBool PreviousDocumentsSupportCore(JobDeclaration declaration) => declaration.IsUCC6;

		protected override IPreviousDocumentValidationDecider UCC6ImportPreviousDocumentValidationDecider => new FR.Business.Declaration.UCC6ImportPreviousDocumentValidationDecider();

		ZBool IsGuaranteeSupportedForEntryInstructionStyle(string type) => stylesSupportGuarantees.Contains(type);

		readonly ImmutableArray<string> stylesSupportGuarantees = new string[]
		{
			DeltaIEImportDeclarationTypeList.Codes.H1,
			DeltaIEImportDeclarationTypeList.Codes.H2,
			DeltaIEImportDeclarationTypeList.Codes.H3,
			DeltaIEImportDeclarationTypeList.Codes.H4,
		}.ToImmutableArray();
	}
}
