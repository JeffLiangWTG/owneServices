using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class ImportInvoiceLineChargeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJ7_RX_NKCurrency_Mandatory()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(lineCharge.J7_RX_NKCurrencyInfo);

				lineCharge.J7_ChargeType = ImportChargeCodeList.Codes.AIR;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(lineCharge.J7_RX_NKCurrencyInfo);
			});
		}

		public void TestCheckJ7_Amount_AmountCannotBe0IfChargeTypeSelected()
		{
			const string messageError = "Amount cannot be zero.";

			CombineAssertions(() =>
			{
				lineCharge.J7_ChargeType = ZString.Empty;
				lineCharge.Validation.ValidateJ7_Amount();
				AssertNoMessageError("No charge code, amount 0", lineCharge.J7_AmountInfo, messageError);

				lineCharge.J7_ChargeType = ImportChargeCodeList.Codes.AIR;
				lineCharge.Validation.ValidateJ7_Amount();
				AssertHasMessageError("charge code, amount 0", lineCharge.J7_AmountInfo, messageError);

				lineCharge.J7_Amount = 1m;
				AssertNoMessageError("Charge code, amount 1", lineCharge.J7_AmountInfo, messageError);
			});
		}

		public void TestCheckJ7_AmountIsValidMoney_ChargeTypeAIR()
		{
			const string errorMessage = "is too large, the maximum value allowed for Charge Code Amount is 999,999,999.99.";
			var targetInfo = lineCharge.J7_AmountInfo;
			lineCharge.J7_ChargeType = ImportChargeCodeList.Codes.AIR;
			CombineAssertions(() =>
			{
				lineCharge.J7_Amount = 0m;
				AssertNoErrorContaining("Import, Amount < maximum", targetInfo, errorMessage);

				lineCharge.J7_Amount = 999999999.99m;
				AssertNoErrorContaining("Import, Amount maximum", targetInfo, errorMessage);

				lineCharge.J7_Amount = 1000000000m;
				AssertHasErrorContaining("Import, Amount > maximum", targetInfo, errorMessage);

				lineCharge.J7_Amount = -1000000000m;
				AssertHasErrorContaining("Import, Amount < minimum", targetInfo, errorMessage);
			});
		}

		public void TestCheckJ7_ChargeType_CombinationOfCodesAIR_010()
		{
			lineCharge.J7_ChargeType = ImportChargeCodeList.Codes.AIR;
			var secondCharge = invoiceLine.Charges.AddNew();
			CombineAssertions(() =>
			{
				secondCharge.J7_ChargeType = ImportChargeCodeList.Codes._012;
				AssertNoMessageError("'AIR' and not '010'", secondCharge.J7_ChargeTypeInfo, CombinationOfCodesAIR_010_014MessageError);

				secondCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
				AssertHasMessageError("'AIR' and '010'", secondCharge.J7_ChargeTypeInfo, CombinationOfCodesAIR_010_014MessageError);
			});
		}

		public void TestCheckJ7_ChargeType_CombinationOfCodesAIR_014()
		{
			lineCharge.J7_ChargeType = ImportChargeCodeList.Codes.AIR;
			var secondCharge = invoiceLine.Charges.AddNew();
			CombineAssertions(() =>
			{
				secondCharge.J7_ChargeType = ImportChargeCodeList.Codes._012;
				AssertNoMessageError("'AIR' and not '014'", secondCharge.J7_ChargeTypeInfo, CombinationOfCodesAIR_010_014MessageError);

				secondCharge.J7_ChargeType = ImportChargeCodeList.Codes._014;
				AssertHasMessageError("'AIR' and '014'", secondCharge.J7_ChargeTypeInfo, CombinationOfCodesAIR_010_014MessageError);
			});
		}

		public void TestCheckJ7_ChargeType_Uniqueness()
		{
			const string messageError = "A charge code of type 'AIR' has already been entered.";

			CombineAssertions(() =>
			{
				lineCharge.J7_ChargeType = ImportChargeCodeList.Codes.AIR;
				var secondCharge = invoiceLine.Charges.AddNew();
				secondCharge.J7_ChargeType = ImportChargeCodeList.Codes.AIR;
				AssertHasMessageError("Duplicate", secondCharge.J7_ChargeTypeInfo, messageError);

				secondCharge.J7_ChargeType = ImportChargeCodeList.Codes._014;
				AssertNoMessageError("Unique", secondCharge.J7_ChargeTypeInfo, messageError);
			});
		}

		public void TestCheckJ7_ChargeType_Uniqueness_DISChargeType()
		{
			const string message = "A charge code of type 'DIS' has already been entered.";

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				lineCharge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.Discount;
				AssertNoError("Only 1 DIS charge", lineCharge.J7_ChargeTypeInfo, message);

				var secondCharge = invoiceLine.Charges.AddNew();
				secondCharge.J7_ChargeType = ImportChargeCodeList.Codes._014;
				AssertNoError("1 DIS charge and 1 other charge", secondCharge.J7_ChargeTypeInfo, message);

				secondCharge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.Discount;
				AssertHasError("2 DIS charges ", secondCharge.J7_ChargeTypeInfo, message);
			});
		}

		public void TestCheckJ7_RX_NKCurrency()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestCheckJ7_RX_NKCurrency(lineCharge);
			});
		}

		public void TestCheckIsJ7_ExchangeRateIATA()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestCheckIsJ7_ExchangeRateIATA(lineCharge, charge => charge.IsJ7_ExchangeRateIATAInfo);
			});
		}

		public void TestCheckIsJ7_ExchangeRateUserEnterable()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestCheckIsJ7_ExchangeRateUserEnterable(lineCharge, charge => charge.IsJ7_ExchangeRateIATAInfo);
			});
		}

		public void TestCheckJ7_ExchangeRateDate()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestCheckJ7_ExchangeRateDate(lineCharge);
			});
		}

		public void TestValidateForRowNotificationJ_ChargeTypes_010_014()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestValidateForRowNotificationJ_ChargeTypes_010_014(lineCharge);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			lineCharge = invoiceLine.Charges.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		InvoiceLineCharge lineCharge;

		const string CombinationOfCodesAIR_010_014MessageError = "Charge Codes '010' and '014' are not allowed in combination with Charge Code 'AIR'.";
	}
}
