using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.GB.Business
{
	public class InstructionConfiguration : EU.Business.InstructionConfiguration
	{
		protected override ZBool FiscalReferencesSupportCore(BusinessObject businessObject) => true;

		protected override ZBool FiscalReferencesSupportOnCPC42And63OnlyCore(BusinessObject businessObject) => false;

		protected override ZBool UseEoriForAuthorisationReferenceCore => true;

		protected override ZBool AdditionalInfosSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => true;

		protected override ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new Declaration.UCC6ImportSupportingDocumentValidationDecider();
	}
}
