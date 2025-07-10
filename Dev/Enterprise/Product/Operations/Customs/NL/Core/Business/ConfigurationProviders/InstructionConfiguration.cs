using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class InstructionConfiguration : EU.Business.InstructionConfiguration
{
	protected override ZBool SupportingDocumentsSupportCore(JobDeclaration declaration) => true;

	protected override ZBool AdditionalSupplyChainActorSupportCore(JobDeclaration declaration)
	{
		return !((declaration.IsExport && declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.CEI_Style == NLConstants.EntryStyles.ExportDeclarationC2)) ||
				 (declaration.IsImport && declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.CEI_Style == NLConstants.EntryStyles.ImportDeclarationI2)));
	}

	protected override ZBool GuaranteesSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => !declaration.IsExport;

	protected override ZBool PreviousDocumentsSupportCore(JobDeclaration declaration) => true;

	protected override ZBool AdditionalInfosSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => true;
}
