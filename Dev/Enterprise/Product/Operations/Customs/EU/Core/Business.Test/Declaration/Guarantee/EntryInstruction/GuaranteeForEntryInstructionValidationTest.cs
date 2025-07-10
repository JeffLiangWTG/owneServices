using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class GuaranteeForEntryInstructionValidationTest : CommonGuaranteeValidationTest
	{
		public void TestCheckPW_BondTypeEmpty()
		{
			var guarantee = Factory.New<GuaranteeForEntryInstruction>();
			guarantee.PW_BondType = ZString.Empty;
			AssertHasMessageErrorContaining("When BondType is empty", guarantee.PW_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);

			guarantee.PW_BondType = "1";
			AssertNoMessageErrorContaining("When BondType is not empty", guarantee.PW_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestTestCheckPW_BondNumberEmpty()
		{
			var guarantee = Factory.New<GuaranteeForEntryInstruction>();
			var expectedMessage = "At least a Reference (GRN) or a Reference 2 is required";
			guarantee.PW_BondNumber2 = ZString.Empty;

			guarantee.PW_BondNumber = ZString.Empty;
			AssertHasMessageErrorContaining("When PW_BondNumber and PW_BondNumber2 are empty, at PW_BondNumber", guarantee.PW_BondNumberInfo, expectedMessage);

			guarantee.PW_BondNumber = "ABC";
			AssertNoMessageErrorContaining("When PW_BondNumber is not empty and PW_BondNumber2 is empty, at PW_BondNumber", guarantee.PW_BondNumberInfo, expectedMessage);
		}

		public void TestTestCheckPW_BondNumberNotEmpty()
		{
			var guarantee = Factory.New<GuaranteeForEntryInstruction>();
			var expectedMessage = "Both Reference (GRN) and Reference 2 cannot be entered in the same line";
			guarantee.PW_BondNumber2 = "XYZ";

			guarantee.PW_BondNumber = ZString.Empty;
			AssertNoMessageErrorContaining("When PW_BondNumber is empty and PW_BondNumber2 is not empty, at PW_BondNumber", guarantee.PW_BondNumberInfo, expectedMessage);

			guarantee.PW_BondNumber = "ABC";
			AssertHasMessageErrorContaining("When PW_BondNumber and PW_BondNumber2 are not empty, at PW_BondNumber", guarantee.PW_BondNumberInfo, expectedMessage);
		}

		public void TestTestCheckPW_BondNumber2Empty()
		{
			var guarantee = Factory.New<GuaranteeForEntryInstruction>();
			var expectedMessage = "At least a Reference (GRN) or a Reference 2 is required";
			guarantee.PW_BondNumber = ZString.Empty;

			guarantee.PW_BondNumber2 = ZString.Empty;
			AssertHasMessageErrorContaining("When PW_BondNumber2 and PW_BondNumber are empty, at PW_BondNumber2", guarantee.PW_BondNumber2Info, expectedMessage);

			guarantee.PW_BondNumber2 = "XYZ";
			AssertNoMessageErrorContaining("When PW_BondNumber2 is not empty and PW_BondNumber is empty, at PW_BondNumber2", guarantee.PW_BondNumber2Info, expectedMessage);
		}

		public void TestTestCheckPW_BondNumber2NotEmpty()
		{
			var guarantee = Factory.New<GuaranteeForEntryInstruction>();
			var expectedMessage = "Both Reference (GRN) and Reference 2 cannot be entered in the same line";
			guarantee.PW_BondNumber = "ABC";

			guarantee.PW_BondNumber2 = ZString.Empty;
			AssertNoMessageErrorContaining("When PW_BondNumber2 is empty and PW_BondNumber is not empty, at PW_BondNumber2", guarantee.PW_BondNumber2Info, expectedMessage);

			guarantee.PW_BondNumber2 = "XYZ";
			AssertHasMessageErrorContaining("When PW_BondNumber2 and PW_BondNumber are not empty, at PW_BondNumber2", guarantee.PW_BondNumber2Info, expectedMessage);
		}

		public void TestCheckPW_PasswordEmpty()
		{
			var guarantee = Factory.New<GuaranteeForEntryInstruction>();
			guarantee.PW_Password = ZString.Empty;
			AssertHasMessageErrorContaining("When Password is empty", guarantee.PW_PasswordInfo, MandatoryValidation.YouHaveNotEntered);

			guarantee.PW_Password = "1234";
			AssertNoMessageErrorContaining("When Password is not empty", guarantee.PW_PasswordInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckPW_RX_NKCurrencyEmpty()
		{
			var guarantee = Factory.New<GuaranteeForEntryInstruction>();
			guarantee.PW_RX_NKCurrency = ZString.Empty;
			AssertHasMessageErrorContaining("When Currency is empty", guarantee.PW_RX_NKCurrencyInfo, MandatoryValidation.YouHaveNotEntered);

			guarantee.PW_RX_NKCurrency = "USD";
			AssertNoMessageErrorContaining("When Currency is not empty", guarantee.PW_RX_NKCurrencyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckPW_RX_NKCurrencyNotInList()
		{
			var guarantee = Factory.New<GuaranteeForEntryInstruction>();
			guarantee.PW_RX_NKCurrency = "USD";
			AssertNoMessageErrorContaining("When Currency is in lookup list", guarantee.PW_RX_NKCurrencyInfo, ListValidation.InvalidCodeMessageError.ToString());

			guarantee.PW_RX_NKCurrency = ZString.Empty;
			AssertNoMessageErrorContaining("When Currency is empty", guarantee.PW_RX_NKCurrencyInfo, ListValidation.InvalidCodeMessageError.ToString());

			guarantee.PW_RX_NKCurrency = "XXZ";
			AssertHasMessageErrorContaining("When Currency is not in lookup list", guarantee.PW_RX_NKCurrencyInfo, ListValidation.InvalidCodeMessageError.ToString());
		}

		public void TestCheckPW_BondFiledPortEmpty()
		{
			var guarantee = Factory.New<GuaranteeForEntryInstruction>();
			guarantee.PW_BondFiledPort = ZString.Empty;
			AssertHasMessageErrorContaining("When BondFiledPort is empty", guarantee.PW_BondFiledPortInfo, MandatoryValidation.YouHaveNotEntered);

			guarantee.PW_BondFiledPort = "EUPort";
			AssertNoMessageErrorContaining("When BondFiledPort is not empty", guarantee.PW_BondFiledPortInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckPW_SuretyCodeNotInList()
		{
			var guarantee = Factory.New<GuaranteeForEntryInstruction>();
			guarantee.PW_SuretyCode = "FUL";
			AssertNoMessageErrorContaining("When Reduction Fraction is in lookup list", guarantee.PW_SuretyCodeInfo, ListValidation.InvalidCodeMessageError.ToString());

			guarantee.PW_SuretyCode = ZString.Empty;
			AssertNoMessageErrorContaining("When Reduction Fraction is empty", guarantee.PW_SuretyCodeInfo, ListValidation.InvalidCodeMessageError.ToString());

			guarantee.PW_SuretyCode = "XYZ";
			AssertHasMessageErrorContaining("When Reduction Fraction is not in lookup list", guarantee.PW_SuretyCodeInfo, ListValidation.InvalidCodeMessageError.ToString());
		}
	}
}
