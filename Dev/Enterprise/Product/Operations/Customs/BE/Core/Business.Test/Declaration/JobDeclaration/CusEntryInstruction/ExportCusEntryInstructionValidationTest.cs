using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class ExportCusEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCEI_Style_ForExportNormal()
	{
		const string errorMessage = "When the Declaration Type is B1 or B2, the Entry Style in the Declaration tab must be EX.";

		var validStyles = new ZString[] {
			ExportCusEntryInstructionsDeclarationTypeList.Codes.ExportAndReExport,
			ExportCusEntryInstructionsDeclarationTypeList.Codes.OutwardProcessing,
		};

		var nonValidStyles = new ZString[] {
			ExportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousingOfUnionGoods,
			ExportCusEntryInstructionsDeclarationTypeList.Codes.T2L_T2LF,
			ExportCusEntryInstructionsDeclarationTypeList.Codes.DispatchOfGoodsForTradeWithSpecialTerritories,
			ExportCusEntryInstructionsDeclarationTypeList.Codes.ExportSimplified,
		};

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			AssertCheckCEI_Style(declaration, EntryStyleListExportUCC.Codes.ExportToSpecialTerritory, instruction, validStyles, errorMessage, hasError: true);
			AssertCheckCEI_Style(declaration, EntryStyleListExportUCC.Codes.ExportToSpecialTerritory, instruction, nonValidStyles, errorMessage, hasError: false);
			AssertCheckCEI_Style(declaration, EntryStyleListExportUCC.Codes.ExportNormal, instruction, validStyles, errorMessage, hasError: false);
			AssertCheckCEI_Style(declaration, EntryStyleListExportUCC.Codes.ExportNormal, instruction, nonValidStyles, errorMessage, hasError: false);
		});
	}

	public void TestCheckCEI_Style_ForExportToSpecialTerritory()
	{
		const string errorMessage = "When the Declaration Type is B3 or B4, the Entry Style in the Declaration tab must be CO.";

		var validStyles = new ZString[] {
			ExportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousingOfUnionGoods,
			ExportCusEntryInstructionsDeclarationTypeList.Codes.DispatchOfGoodsForTradeWithSpecialTerritories,
		};

		var nonValidStyles = new ZString[] {
			ExportCusEntryInstructionsDeclarationTypeList.Codes.ExportAndReExport,
			ExportCusEntryInstructionsDeclarationTypeList.Codes.OutwardProcessing,
			ExportCusEntryInstructionsDeclarationTypeList.Codes.T2L_T2LF,
			ExportCusEntryInstructionsDeclarationTypeList.Codes.ExportSimplified,
		};

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			AssertCheckCEI_Style(declaration, EntryStyleListExportUCC.Codes.ExportNormal, instruction, validStyles, errorMessage, hasError: true);
			AssertCheckCEI_Style(declaration, EntryStyleListExportUCC.Codes.ExportNormal, instruction, nonValidStyles, errorMessage, hasError: false);
			AssertCheckCEI_Style(declaration, EntryStyleListExportUCC.Codes.ExportToSpecialTerritory, instruction, validStyles, errorMessage, hasError: false);
			AssertCheckCEI_Style(declaration, EntryStyleListExportUCC.Codes.ExportToSpecialTerritory, instruction, nonValidStyles, errorMessage, hasError: false);
		});
	}

	static void AssertCheckCEI_Style(JobDeclaration declaration, ZString entryStyle, CusEntryInstruction instruction, ZString[] declarationTypes, string errorMessage, bool hasError)
	{
		declaration.JE_EntryStyle = entryStyle;
		var targetInfo = instruction.CEI_StyleInfo;
		foreach (var style in declarationTypes)
		{
			instruction.CEI_Style = style;
			instruction.Validation.ValidateCEI_Style();
			var assertMessage = $"HasError = {hasError}, JE_EntryStyle = {entryStyle}, CEI_Style = {style}";
			if (hasError)
			{
				AssertHasMessageError(assertMessage, targetInfo, errorMessage);
			}
			else
			{
				AssertNoMessageError(assertMessage, targetInfo, errorMessage);
			}
		}
	}
}
