using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExitSummaryJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest<ExitSummaryJobComInvoiceLineValidation>
	{
		public void TestJI_TariffInfo()
		{
			(_, var invoiceLine, _, var declaration) = SetupData();
			var entry = declaration.CustomsEntryInstructions.AddNew();
			entry.CEI_Style = "A1";
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertHasMessageError($"Ensure validation present where JE_MessageType is EXS", invoiceLine.JI_TariffInfo, "Tariff may not be empty");
		}

		public void TestJI_CountryOfOrigin()
		{
			(var validation, var invoiceLine, _, var declaration) = SetupData();
			var errorMessage = $"You have not entered a {invoiceLine.JI_CountryOfOriginInfo.HumanReadableName} Origin is mandatory when Additional Procedure begins with 'E'.";

			invoiceLine.JI_Procedure = "1000E00";
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageError("Has error when JI_Procedure starts with E", invoiceLine.JI_CountryOfOriginInfo, errorMessage);

			invoiceLine.JI_Procedure = "1000F00";
			validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageError("No error when JI_Procedure does not start with E", invoiceLine.JI_CountryOfOriginInfo, errorMessage);
		}

		public void TestCheckJI_Procedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Ireland, "", "76", "00", "E71", "", MessageType, "B3");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			(_, var invoiceLine, _, var declaration) = SetupData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
			invoiceLine.JI_CEI = instruction.PK;

			invoiceLine.JI_Procedure = "7600E71";
			var message = "Procedure must be into warehouse when Instruction Declaration Type is B3.";
			AssertHasMessageErrorContaining(invoiceLine.JI_ProcedureInfo, message);

			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageErrorContaining(instruction.CEI_OA_WarehouseInfo, message);
		}

		public void TestCheckJI_Weight()
		{
			(_, var invoiceLine, _, _) = SetupData();
			ValidationTestHelper.AssertValueCannotBeZeroMessageError(invoiceLine.JI_WeightInfo);
		}

		public void TestCheckJI_CustomsSecondUnitQty()
		{
			var factory = new BusinessObjectFactory();

			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ireland, Universal.Constants.TariffTypes.Export);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "9999001000", startDate, endDate);
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram);
			factory.Save();

			(_, var invoiceLine, _, _) = SetupData();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			invoiceLine.Validation.ValidateJI_CustomsSecondUnitQty();
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsSecondUnitQtyInfo, $"{invoiceLine.JI_CustomsSecondUnitQtyInfo.HumanReadableName} for this {invoiceLine.JI_TariffInfo.HumanReadableName} should be KGM");
		}

		public void Test_PackagesAreSelected()
		{
			(_, var invoiceLine, _, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "A3";
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, "This line has no packaging details");

			entryInstruction.CEI_Style = "A1";
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");

			declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicatorForUCCList.Codes.A20;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.ExitSummary;
	}
}
