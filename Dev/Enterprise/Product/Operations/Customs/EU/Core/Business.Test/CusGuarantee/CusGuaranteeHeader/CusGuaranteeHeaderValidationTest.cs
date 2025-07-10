using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing.CusGuarantee
{
	public class CusGuaranteeHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPH_Number()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Type = PermitTransactionTypeList.Codes.TRA;
			guarantee.CPH_SubType = EUNctsGuaranteeTypeList.Codes.FlatRateVoucher;
			guarantee.CPH_Number = "123456789012345678901234";
			guarantee.Validation.ValidateCPH_Number();
			AssertNoMessageErrorContaining(guarantee.CPH_NumberInfo, "Guarantee reference must be ");

			guarantee.CPH_Number = "12345678901234567890123";
			AssertHasMessageErrorContaining(guarantee.CPH_NumberInfo, "Guarantee reference must be ");

			guarantee.CPH_SubType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			guarantee.Validation.ValidateCPH_Number();
			AssertHasMessageErrorContaining(guarantee.CPH_NumberInfo, "Guarantee reference must be ");

			guarantee.CPH_Number = "12345678901234567";
			guarantee.Validation.ValidateCPH_Number();
			AssertNoMessageErrorContaining(guarantee.CPH_NumberInfo, "Guarantee reference must be ");

			guarantee.CPH_Number = "1234567890123456";
			AssertHasMessageErrorContaining(guarantee.CPH_NumberInfo, "Guarantee reference must be ");

			guarantee.CPH_SubType = EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee;
			guarantee.Validation.ValidateCPH_Number();
			AssertNoMessageErrorContaining(guarantee.CPH_NumberInfo, "Guarantee reference must be ");

			guarantee.CPH_SubType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			guarantee.CPH_Type = PermitTransactionTypeList.Codes.ADJ;
			guarantee.Validation.ValidateCPH_Number();
			AssertNoMessageErrorContaining(guarantee.CPH_NumberInfo, "Guarantee reference must be ");
		}

		[ExpectNoExceptions]
		public void TestCheckCPH_UnitOfMeasure_ApplyValidationWhenEditable()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			NUnit.Framework.Assert.That(guarantee.CPH_UnitOfMeasure_ReadOnly, NUnit.Framework.Is.EqualTo(false));

			guarantee.CPH_UnitOfMeasure = "";
			AssertHasErrorContaining(guarantee.CPH_UnitOfMeasureInfo, MandatoryValidation.MustBeEntered);
			guarantee.CPH_UnitOfMeasure = "X~";
			AssertHasErrorContaining(guarantee.CPH_UnitOfMeasureInfo, ListValidation.InvalidCodeError);
			guarantee.CPH_UnitOfMeasure = Core.Constants.CurrencyCodes.EuropeanUnion;
			AssertNoErrors(guarantee.CPH_UnitOfMeasureInfo);
		}

		[ExpectNoExceptions]
		public void TestCheckCPH_UnitOfMeasure_DoNotApplyValidationWhenReadOnly()
		{
			var guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var transaction = guarantee.CusGuaranteeLineTransactions.AddNew();
			transaction.FillWithValidTestData();
			Factory.Save();
			NUnit.Framework.Assert.That(guarantee.CPH_UnitOfMeasure_ReadOnly, NUnit.Framework.Is.EqualTo(true));

			guarantee.CPH_UnitOfMeasure = "";
			AssertNoErrors(guarantee.CPH_UnitOfMeasureInfo);
			guarantee.CPH_UnitOfMeasure = "X~";
			AssertNoErrors(guarantee.CPH_UnitOfMeasureInfo);
		}
	}
}
