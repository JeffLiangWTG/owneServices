using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public class ImportCusEntryInstructionValidation : EU.Business.Declaration.CusEntryInstructionValidation
{
	public ImportCusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
	{
	}

	protected override void CheckCEI_Style()
	{
		base.CheckCEI_Style();

		var instruction = (CusEntryInstruction)Parent;
		CheckCEI_Style_ForImportNormal(instruction);
		CheckCEI_Style_ForImportFromSpecialTerritory(instruction);
	}

	static void CheckCEI_Style_ForImportNormal(CusEntryInstruction instruction)
	{
		var style = instruction.CEI_Style;
		if ((style == ImportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousing
			|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.TemporaryAdmission
			|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.InwardProcessing
			|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.ProbablyNoControlRequired)
			&& instruction.JobDeclaration.JE_EntryStyle != EntryStyleListImportUCC.Codes.ImportNormal)
		{
			instruction.CEI_StyleInfo.AddMessageError(Res.GetString("B4E7C666-6BF5-44F6-8AB5-BB25DEA871D8", "When the Declaration Type is H2, H3, H4 or H6, the Entry Style in the Declaration tab must be IM."));
		}
	}

	static void CheckCEI_Style_ForImportFromSpecialTerritory(CusEntryInstruction instruction)
	{
		var style = instruction.CEI_Style;
		if (style == ImportCusEntryInstructionsDeclarationTypeList.Codes.IntroductionOfGoods
			&& instruction.JobDeclaration.JE_EntryStyle != EntryStyleListImportUCC.Codes.ImportFromSpecialTerritory)
		{
			instruction.CEI_StyleInfo.AddMessageError(Res.GetString("D6E6D99B-AC07-4B3B-854F-DCFDDA1E557D", "When the Declaration Type is H5, the Entry Style in the Declaration tab must be CO."));
		}
	}

	protected override void CheckCEI_OA_Warehouse2()
	{
		base.CheckCEI_OA_Warehouse2();

		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(Parent.CEI_OA_Warehouse2Info, Parent.CEI_StyleInfo, (ZString)ImportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousing);
	}
}
