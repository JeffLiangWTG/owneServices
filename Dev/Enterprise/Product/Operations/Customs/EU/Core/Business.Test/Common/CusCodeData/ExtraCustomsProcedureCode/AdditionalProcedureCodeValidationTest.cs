using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	class AdditionalProcedureCodeValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestCheckCY_Code()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP", group: "IFD");
			helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "333", "Three", "IMP", group: "IFD");
			helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "444", "Four", "IMP", group: "IFD");

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "IFD";

			var messageShouldBeTheSame = "The first 4 characters of additional procedure code should be same as the main procedure code's.";
			var messageShouldNotBeDuplicated = "Additional procedure code already specified. Additional procedure codes should not be duplicated.";

			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.JI_RN_NKCountryOfExport = currentCountry;
			invLine.JI_Procedure = "1111111";
			var additionalProcedueCode = invLine.AdditionalProcedureCodes.AddNew();
			additionalProcedueCode.CY_Code = "2222222";
			AssertHasMessageError(additionalProcedueCode.CY_CodeInfo, messageShouldBeTheSame);
			AssertHasMessageError(invLine.AdditionalProcedureCodesAsStringInfo, messageShouldBeTheSame);
			invLine.Validation.ValidateAdditionalProcedureCodesAsString();
			AssertHasMessageError(invLine.AdditionalProcedureCodesAsStringInfo, messageShouldBeTheSame);

			additionalProcedueCode.CY_Code = "1111333";
			AssertNoMessageError(additionalProcedueCode.CY_CodeInfo, messageShouldBeTheSame);
			invLine.Validation.ValidateAdditionalProcedureCodesAsString();
			AssertNoMessageError(invLine.AdditionalProcedureCodesAsStringInfo, messageShouldBeTheSame);

			additionalProcedueCode.CY_Code = "1111111";
			AssertHasMessageError(additionalProcedueCode.CY_CodeInfo, messageShouldNotBeDuplicated);
			AssertHasMessageError(invLine.AdditionalProcedureCodesAsStringInfo, messageShouldNotBeDuplicated);
			invLine.Validation.ValidateAdditionalProcedureCodesAsString();
			AssertHasMessageError(invLine.AdditionalProcedureCodesAsStringInfo, messageShouldNotBeDuplicated);

			additionalProcedueCode.CY_Code = "1111333";
			AssertNoMessageError(additionalProcedueCode.CY_CodeInfo, messageShouldNotBeDuplicated);
			invLine.Validation.ValidateAdditionalProcedureCodesAsString();
			AssertNoMessageError(invLine.AdditionalProcedureCodesAsStringInfo, messageShouldNotBeDuplicated);

			var additionalProcedueCode2 = invLine.AdditionalProcedureCodes.AddNew();
			additionalProcedueCode2.CY_Code = "1111333";
			AssertHasMessageError(additionalProcedueCode2.CY_CodeInfo, messageShouldNotBeDuplicated);
			invLine.Validation.ValidateAdditionalProcedureCodesAsString();
			AssertHasMessageError(invLine.AdditionalProcedureCodesAsStringInfo, messageShouldNotBeDuplicated);

			additionalProcedueCode2.CY_Code = "1111444";
			AssertNoMessageError(additionalProcedueCode2.CY_CodeInfo, messageShouldNotBeDuplicated);
			invLine.Validation.ValidateAdditionalProcedureCodesAsString();
			AssertNoMessageError(invLine.AdditionalProcedureCodesAsStringInfo, messageShouldNotBeDuplicated);
		}
	}
}
