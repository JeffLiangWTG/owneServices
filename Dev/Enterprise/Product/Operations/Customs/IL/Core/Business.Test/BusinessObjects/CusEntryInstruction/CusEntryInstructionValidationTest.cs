using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidationBaseType()
		{
			AssertEquals(typeof(AutoILCusEntryInstructionValidation), typeof(CusEntryInstructionValidation).BaseType);
		}

		public void TestCheckCEI_FormattedProcedure()
		{
			const string expectedWhenEmpty = "You have not entered";
			const string expectedWhenInvalid = "The code you have selected is not in the list.";
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "10", "11", "111", "One", "IMP", group: "IFD");

			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = "IMP";
			var info = instruction.CEI_FormattedProcedureInfo;
			instruction.CEI_FormattedProcedure = ZString.Empty;
			instruction.Validation.ValidateCEI_FormattedProcedure();
			AssertHasMessageErrorContaining("When Have Not Entered", info, expectedWhenEmpty);

			instruction.CEI_FormattedProcedure = "1234567";
			AssertHasMessageErrorContaining("When code is Invalid,", info, expectedWhenInvalid);
			AssertNoMessageErrorContaining("When Have Entered", info, expectedWhenEmpty);

			instruction.CEI_FormattedProcedure = "1011111";
			AssertNoMessageErrorContaining("When code is valid", info, expectedWhenInvalid);
		}

		public void TestCheckCEI_DateForDuty()
		{
			const string expectedWhenEmpty = "You have not entered";
			var instruction = Factory.New<CusEntryInstruction>();
			var info = instruction.CEI_DateForDutyInfo;
			instruction.Validation.ValidateCEI_DateForDuty();
			AssertHasMessageErrorContaining("When Have Not Entered", info, expectedWhenEmpty);

			instruction.CEI_DateForDuty = ZDateTime.Today;
			AssertNoMessageErrorContaining("When Have Entered", info, expectedWhenEmpty);
		}

		public void TestCheckCEI_Description()
		{
			const string expectedWhenEmpty = "You have not entered";
			var instruction = Factory.New<CusEntryInstruction>();
			var info = instruction.CEI_DescriptionInfo;
			instruction.Validation.ValidateCEI_Description();
			AssertHasMessageErrorContaining("When Have Not Entered", info, expectedWhenEmpty);

			instruction.CEI_Description = "Description";
			AssertNoMessageErrorContaining("When Have Entered", info, expectedWhenEmpty);
		}

		public void TestCheckCEI_AutonomyRegionType()
		{
			var factory = Factory;
			const string expectedWhenInvalid = "The code you have selected is not in the list.";
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateCusCodeType(code: "CFTRY", desc: "CFTRY", dataGrouping: Core.Constants.CountryCodes.Israel);
			helper.CreateCusCodeList(dataGroupingCode: Core.Constants.CountryCodes.Israel, codeType: "CFTRY", code: "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var instruction = Factory.New<CusEntryInstruction>();
			var info = instruction.CEI_AutonomyRegionTypeInfo;
			instruction.CEI_AutonomyRegionType = "2";
			AssertHasMessageErrorContaining("When Invalid", info, expectedWhenInvalid);

			instruction.CEI_AutonomyRegionType = "1";
			AssertNoMessageErrorContaining("When valid", info, expectedWhenInvalid);
		}

		public void TestCEI_CustomsPackType()
		{
			const string expectedWhenInvalid = "The code you have selected is not in the list.";
			var instruction = Factory.New<CusEntryInstruction>();
			var info = instruction.CEI_CustomsPackTypeInfo;
			instruction.CEI_CustomsPackType = "IN";
			AssertHasMessageErrorContaining("When Invalid", info, expectedWhenInvalid);

			instruction.CEI_CustomsPackType = "VN";
			AssertNoMessageErrorContaining("When valid", info, expectedWhenInvalid);
		}
	}
}
