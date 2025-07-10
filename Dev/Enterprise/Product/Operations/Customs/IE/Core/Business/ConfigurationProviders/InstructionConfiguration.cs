using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IE.Business
{
	public class InstructionConfiguration : EU.Business.InstructionConfiguration
	{
		protected override ZBool AdditionalSupplyChainActorSupportCore(JobDeclaration declaration) => declaration.IsUCC6AndIsExport || declaration.IsImport;
		protected override ZBool AdditionalInfosSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => true;
		protected override ZBool SupportingDocumentsSupportCore(JobDeclaration declaration) => true;
		protected override ZBool PreviousDocumentsSupportCore(JobDeclaration declaration) => true;
		protected override ZBool RequestedDocumentsSupportCore(JobDeclaration declaration) => true;
		protected override ZBool SpecialProceduresSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.In(new ZString[] { "H1", "H3", "H4" }) ?? false;

		protected override ZBool GuaranteesSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => declaration.IsImport;
		protected override IAdditionalInfoValidationDecider UCC6ImportAdditionalInfoValidationDecider => new Declaration.UCC6ImportAdditionalInfoValidationDecider();
		protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();
		protected override ICusAuthorizationUsageValidationDecider UCC6ImportCusAuthorizationUsageValidationDecider => new UCC6ImportCusAuthorizationUsageValidationDecider();
	}
}
