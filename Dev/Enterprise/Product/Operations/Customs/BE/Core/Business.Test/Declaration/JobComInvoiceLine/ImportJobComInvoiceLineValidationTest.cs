using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class ImportJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateCountryOfOrigin()
	{
		var invoiceLine = GetInvoiceLine();
		invoiceLine.JI_CEI = invoiceLine.Declaration.CustomsEntryInstructions.AddNew().PK;

		CombineAssertions(() =>
		{
			foreach (var combination in PreferenceAndStyleTestCombination.CreateCombinations("Country of Preferential Origin", GetPrimaryPreferences(), GetStyles(), "Country of Origin"))
			{
				invoiceLine.EntryInstruction.CEI_Style = combination.Style;
				invoiceLine.JI_PrimaryPreference = combination.PrimaryPreference;

				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				AssertHasMessageError("JE_CEI = " + combination.Style + " Primary preference = " + combination.PrimaryPreference + ", CountryOfOrigin should have a message error", invoiceLine.JI_CountryOfOriginInfo, combination.ErrorMessage);
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Belgium;
				AssertNoMessageError("JE_CEI = " + combination.Style + " Primary preference = " + combination.PrimaryPreference + ", CountryOfOrigin should not have a message error", invoiceLine.JI_CountryOfOriginInfo, combination.ErrorMessage);
			}
		});

		List<string> GetPrimaryPreferences() => new List<string>
		{
			"",
			"100",
			"252",
			"304",
			"476",
		};

		List<string> GetStyles() => new List<string>
		{
			ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousing,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.TemporaryAdmission,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.InwardProcessing,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.IntroductionOfGoods,
			ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified,
		};
	}

	public void TestCheckJI_Procedure()
	{
		const string expectedErrorMessage = "Fiscal reference with code FR2 is mandatory if procedure is 42 or 63";
		const string expectedErrorMessage1 = "The CPC must start with 42 of 63 for this invoice line";

		var invoiceLine = GetInvoiceLine();
		var entryInstruction = invoiceLine.Declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation;
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			invoiceLine.JI_Procedure = "65777";
			var fiscalReference = invoiceLine.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = "FR2";
			invoiceLine.FiscalReferences.Add(fiscalReference);
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, expectedErrorMessage1);

			invoiceLine.JI_Procedure = "42777";
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, expectedErrorMessage1);

			invoiceLine.JI_Procedure = "63777";
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, expectedErrorMessage1);

			invoiceLine.JI_Procedure = "42777";
			invoiceLine.FiscalReferences.RemoveAndDeleteAll();
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, expectedErrorMessage);

			invoiceLine.JI_Procedure = "63777";
			invoiceLine.FiscalReferences.RemoveAndDeleteAll();
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, expectedErrorMessage);

			invoiceLine.JI_Procedure = "65777";
			invoiceLine.FiscalReferences.RemoveAndDeleteAll();
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, expectedErrorMessage);

			invoiceLine.JI_Procedure = "42777";
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, expectedErrorMessage);
		});
	}

	JobComInvoiceLine GetInvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		return declaration.Invoices.AddNew().InvoiceLines.AddNew();
	}
}
