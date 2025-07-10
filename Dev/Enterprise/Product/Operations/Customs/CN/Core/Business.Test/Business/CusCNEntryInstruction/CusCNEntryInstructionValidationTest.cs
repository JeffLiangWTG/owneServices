using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusCNEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCNE_CEI()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var cusCNEntryinstruction = instruction.AddInfoChild;
			Factory.Save();

			cusCNEntryinstruction.Validation.ValidateCNE_CEI();
			AssertNoErrors("Should not add error message.", cusCNEntryinstruction.CNE_CEIInfo);

			var newCNEntryinstruction = Factory.New<CusCNEntryInstruction>();
			newCNEntryinstruction.CNE_CEI = instruction.PK;
			AssertHasError(newCNEntryinstruction.CNE_CEIInfo, "There is another record linked to the same Entry Instruction.");
		}

		public void TestCheckCNE_TransitionSite()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "DSSSF", "CNWEH42S202_01", "威海海纳食品有限公司进口水产品存储冷库", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeListAttribute(codeList.PK, "CustomsOffice", "42");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OfficeOfEntryExit = "4211";
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew().AddInfoChild;
			var targetInfo = instruction.CNE_TransitionSiteInfo;

			instruction.CNE_ApplyForTransition = true;
			AssertHasMessageErrorContaining("ApplyForTransition unchecked, TransitionSite is mandatory.", targetInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.CNE_TransitionSite = "XXX";
			AssertHasMessageErrorContaining("TransitionSite list validation should worked.", targetInfo, ListValidation.InvalidCodeMessageError.ToString());
			instruction.CNE_TransitionSite = "CNWEH42S202_01";
			AssertNoMessageErrors("Correctly input, TransitionSite validation should pass.", targetInfo);

			instruction.CNE_ApplyForTransition = false;
			AssertHasMessageErrorContaining("ApplyForTransition checked, TransitionSite should not have value.", targetInfo, "Transition Site is not required when Apply for Transition is unchecked.");
			instruction.CNE_TransitionSite = "";
			AssertNoMessageErrors("ApplyForTransition checked, TransitionSite should not have value.", targetInfo);
		}

		public void TestValidationModeProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew().AddInfoChild;
			ValidationExtensionsTest.AssertValidationModeProvider(instruction.EntryInstruction.JobDeclaration, instruction.Validation.ValidationModeProvider);

			instruction = Factory.New<CusCNEntryInstruction>();
			AssertNull(instruction.Validation.ValidationModeProvider);
		}
	}
}
