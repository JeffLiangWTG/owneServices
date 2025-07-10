using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public class ExportCusEntryInstructionValidation : EU.Business.Declaration.CusEntryInstructionValidation
{
	public ExportCusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
	{
	}

	protected override void CheckCEI_Style()
	{
		base.CheckCEI_Style();

		var instruction = (CusEntryInstruction)Parent;
		CheckCEI_Style_ForExportNormal(instruction);
		CheckCEI_Style_ForExportToSpecialTerritory(instruction);
	}

	static void CheckCEI_Style_ForExportNormal(CusEntryInstruction instruction)
	{
		var style = instruction.CEI_Style;
		if ((style == ExportCusEntryInstructionsDeclarationTypeList.Codes.ExportAndReExport
			|| style == ExportCusEntryInstructionsDeclarationTypeList.Codes.OutwardProcessing)
			&& instruction.JobDeclaration.JE_EntryStyle != EntryStyleListExportUCC.Codes.ExportNormal)
		{
			instruction.CEI_StyleInfo.AddMessageError(Res.GetString("621D736E-3991-4690-B919-A7EF54F71FDD", "When the Declaration Type is B1 or B2, the Entry Style in the Declaration tab must be EX."));
		}
	}

	static void CheckCEI_Style_ForExportToSpecialTerritory(CusEntryInstruction instruction)
	{
		var style = instruction.CEI_Style;
		if ((style == ExportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousingOfUnionGoods
			|| style == ExportCusEntryInstructionsDeclarationTypeList.Codes.DispatchOfGoodsForTradeWithSpecialTerritories)
			&& instruction.JobDeclaration.JE_EntryStyle != EntryStyleListExportUCC.Codes.ExportToSpecialTerritory)
		{
			instruction.CEI_StyleInfo.AddMessageError(Res.GetString("EC0FE264-29D1-4E0F-9535-F65400BFC181", "When the Declaration Type is B3 or B4, the Entry Style in the Declaration tab must be CO."));
		}
	}
}
