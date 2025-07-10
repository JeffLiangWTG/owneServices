using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class GuaranteeForEntryInstructionValidationTest : EU.Business.Declaration.Testing.CommonGuaranteeValidationTest
	{
		GuaranteeForEntryInstruction CreateGuarantee()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = entryInstruction.Guarantees.AddNew();

			return guarantee;
		}

		public void TestCheckPW_BondAmount()
		{
			var guarantee = CreateGuarantee();
			const string error = MandatoryValidation.ValueCannotBeNegative;
			const string errorMessage = MandatoryValidation.YouHaveNotEntered;

			CombineAssertions(() =>
			{
				guarantee.PW_BondAmount = 1;
				AssertNoErrorContaining(guarantee.PW_BondAmountInfo, error);
				AssertNoMessageErrorContaining(guarantee.PW_BondAmountInfo, errorMessage);

				guarantee.PW_BondAmount = 0;
				AssertHasMessageErrorContaining(guarantee.PW_BondAmountInfo, errorMessage);

				guarantee.PW_BondAmount = -1;
				AssertHasErrorContaining(guarantee.PW_BondAmountInfo, error);
			});
		}
	}
}
