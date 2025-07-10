using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusGuaranteeReferenceValidation))]
	sealed class CusGuaranteeReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFR_Reference()
		{
			var expectedMessageError = "Do not enter more than one guarantee when Declaration Type is H or N.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.N;
			var guarantees = instruction.Guarantees;

			var firstGuarantee = guarantees.AddNew();
			firstGuarantee.Validation.ValidateCFR_Reference();
			AssertNoMessageError(firstGuarantee.CFR_ReferenceInfo, expectedMessageError);

			var secondGuarantee = guarantees.AddNew();
			secondGuarantee.Validation.ValidateCFR_Reference();
			AssertHasMessageError(secondGuarantee.CFR_ReferenceInfo, expectedMessageError);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.B;
			secondGuarantee.Validation.ValidateCFR_Reference();
			AssertNoMessageError(secondGuarantee.CFR_ReferenceInfo, expectedMessageError);
		}

		public void TestCheckCFR_Reference_Duplicated()
		{
			const string expectedMessageError = "The same information has been entered.";
			var instruction = Factory.New<CusEntryInstruction>();
			var guarantees = new CusGuaranteeReferenceCollection(instruction);
			var firstGuarantee = guarantees.AddNew();
			var secondGuarantee = guarantees.AddNew();

			firstGuarantee.CFR_Reference = "123456";
			secondGuarantee.CFR_Reference = "123456";
			AssertHasMessageError(secondGuarantee.CFR_ReferenceInfo, expectedMessageError);

			secondGuarantee.CFR_Reference = "654321";
			AssertNoMessageError(secondGuarantee.CFR_ReferenceInfo, expectedMessageError);
		}
	}
}
