using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(ComprehensiveValuationValidation))]
	sealed class ComprehensiveValuationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFR_Reference()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var cusReference = invoiceHeader.ComprehensiveValuations.AddNew();
			var targetInfo = cusReference.CFR_ReferenceInfo;
			var expectedErrorMessage = "Comprehensive Valuation Number cannot be entered when Shipment Type is IMP and Declaration Type is Y, H, or N.";

			cusReference.CFR_Reference = "ABC";
			cusReference.Validation.ValidateCFR_Reference();
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			cusReference.Validation.ValidateCFR_Reference();
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.H;
			cusReference.Validation.ValidateCFR_Reference();
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.N;
			cusReference.Validation.ValidateCFR_Reference();
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			expectedErrorMessage = "Only capital letters and digits are allowed.";
			cusReference.CFR_Reference = "a";
			AssertHasError(targetInfo, expectedErrorMessage);
			cusReference.CFR_Reference = "A";
			AssertNoError(targetInfo, expectedErrorMessage);
		}

		public void TestCheckCFR_Reference_Duplicated()
		{
			const string expectedMessageError = "The same information has been entered.";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var firstReference = invoiceHeader.ComprehensiveValuations.AddNew();
			var secondReference = invoiceHeader.ComprehensiveValuations.AddNew();
			var targetInfo = secondReference.CFR_ReferenceInfo;

			firstReference.CFR_Reference = "123456";
			secondReference.CFR_Reference = "123456";
			AssertHasMessageError(targetInfo, expectedMessageError);

			secondReference.CFR_Reference = "654321";
			AssertNoMessageError(targetInfo, expectedMessageError);
		}
	}
}
