using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class ImportCusEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCEI_Style_ForImportNormal()
	{
		const string errorMessage = "When the Declaration Type is H2, H3, H4 or H6, the Entry Style in the Declaration tab must be IM.";

		var validStyles = new ZString[] {
			ImportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousing,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.TemporaryAdmission,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.InwardProcessing,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.ProbablyNoControlRequired,
		};

		var nonValidStyles = new ZString[] {
			ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.IntroductionOfGoods
		};

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			AssertCheckCEI_Style(declaration, EntryStyleListImportUCC.Codes.ImportFromSpecialTerritory, instruction, validStyles, errorMessage, hasError: true);
			AssertCheckCEI_Style(declaration, EntryStyleListImportUCC.Codes.ImportFromSpecialTerritory, instruction, nonValidStyles, errorMessage, hasError: false);
			AssertCheckCEI_Style(declaration, EntryStyleListImportUCC.Codes.ImportNormal, instruction, validStyles, errorMessage, hasError: false);
			AssertCheckCEI_Style(declaration, EntryStyleListImportUCC.Codes.ImportNormal, instruction, nonValidStyles, errorMessage, hasError: false);
		});
	}

	public void TestCheckCEI_Style_ForImportFromSpecialTerritory()
	{
		const string errorMessage = "When the Declaration Type is H5, the Entry Style in the Declaration tab must be CO.";

		var validStyles = new ZString[] { ImportCusEntryInstructionsDeclarationTypeList.Codes.IntroductionOfGoods };

		var nonValidStyles = new ZString[] {
			ImportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousing,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.TemporaryAdmission,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.InwardProcessing,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.ProbablyNoControlRequired,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified,
		};

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			AssertCheckCEI_Style(declaration, EntryStyleListImportUCC.Codes.ImportNormal, instruction, validStyles, errorMessage, hasError: true);
			AssertCheckCEI_Style(declaration, EntryStyleListImportUCC.Codes.ImportNormal, instruction, nonValidStyles, errorMessage, hasError: false);
			AssertCheckCEI_Style(declaration, EntryStyleListImportUCC.Codes.ImportFromSpecialTerritory, instruction, validStyles, errorMessage, hasError: false);
			AssertCheckCEI_Style(declaration, EntryStyleListImportUCC.Codes.ImportFromSpecialTerritory, instruction, nonValidStyles, errorMessage, hasError: false);
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

	public void TestCheckCEI_OA_Warehouse2_MandatoryWhenStyleIsH2()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(instruction.CEI_OA_Warehouse2Info, instruction.CEI_StyleInfo, (ZString)ImportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousing);
	}
}
