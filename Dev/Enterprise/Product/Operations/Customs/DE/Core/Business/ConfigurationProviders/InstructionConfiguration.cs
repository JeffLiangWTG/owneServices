using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.DE.Business
{
	public class InstructionConfiguration : EU.Business.InstructionConfiguration
	{
		protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new UCC6ImportSupportingDocumentValidationDecider();
		protected override IEntryInstructionValidationDecider GetImportValidationDecider(CusEntryInstruction cusEntryInstruction) => new UCC6ImportEntryInstructionValidationDecider();
		protected override IEntryInstructionValidationDecider GetExportValidationDecider(CusEntryInstruction cusEntryInstruction) => new UCC6ExportEntryInstructionValidationDecider();
		protected override ZBool AdditionalSupplyChainActorSupportCore(JobDeclaration declaration) => true;
	}
}
