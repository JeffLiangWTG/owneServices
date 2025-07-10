using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class InvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestValidationAgainstHeaderCharges()
		{
			const string errorMessage = "Group and Invoice Charges of the same type cannot be entered with different currencies.";
			CombineAssertions(() =>
			{
				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

				var headerCharge2 = invoice.Charges.AddNew();
				headerCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				headerCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedArabEmirates;
				AssertHasMessageError("Same charge code, different currency", headerCharge2.J7_RX_NKCurrencyInfo, errorMessage);

				headerCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertNoMessageError("Same charge code, same currency", headerCharge2.J7_RX_NKCurrencyInfo, errorMessage);
			});
		}

		public void TestValidationAgainstGroupCharges()
		{
			const string errorMessage = "Group and Invoice Charges of the same type cannot be entered with different currencies.";
			CombineAssertions(() =>
			{
				var groupCharge = groupInvoice.Charges.AddNew();
				groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				groupCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedArabEmirates;
				AssertHasMessageError("Same charge code, different currency", invoiceCharge.J7_RX_NKCurrencyInfo, errorMessage);

				invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertNoMessageError("Same charge code, same currency", invoiceCharge.J7_RX_NKCurrencyInfo, errorMessage);
			});
		}

		public void TestCheckJ7_AmountIsValidMoney_Import_ChargeTypeAIR()
		{
			const string errorMessage = "is too large, the maximum value allowed for Charge Code Amount is 999,999,999.99.";
			var targetInfo = invoiceCharge.J7_AmountInfo;
			invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes.AIR;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				invoiceCharge.J7_Amount = 0m;
				AssertNoErrorContaining("Import, Amount < maximum", targetInfo, errorMessage);

				invoiceCharge.J7_Amount = 999999999.99m;
				AssertNoErrorContaining("Import, Amount maximum", targetInfo, errorMessage);

				invoiceCharge.J7_Amount = 1000000000m;
				AssertHasErrorContaining("Import, Amount > maximum", targetInfo, errorMessage);

				invoiceCharge.J7_Amount = -1000000000m;
				AssertHasErrorContaining("Import, Amount < minimum", targetInfo, errorMessage);
			});
		}

		public void TestCheckJ7_AmountIsValidMoney_Import_ChargeTypeOther()
		{
			const string errorMessage = "is too large, the value's range of";
			var targetInfo = invoiceCharge.J7_AmountInfo;
			invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				invoiceCharge.J7_Amount = 1000000000m;
				AssertNoErrorContaining("Import, Amount < maximum", targetInfo, errorMessage);

				invoiceCharge.J7_Amount = 1000000000000000m;
				AssertHasErrorContaining("Import, Amount > maximum", targetInfo, errorMessage);
			});
		}

		public void TestCheckJ7_AmountIsValidMoney_Export()
		{
			const string errorMessage = "is too large, the value's range of";
			var targetInfo = invoiceCharge.J7_AmountInfo;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				invoiceCharge.J7_Amount = 1000000000m;
				AssertNoErrorContaining("Export, Amount < maximum", targetInfo, errorMessage);

				invoiceCharge.J7_Amount = 1000000000000000m;
				AssertHasErrorContaining("Export, Amount > maximum", targetInfo, errorMessage);
			});
		}

		public void TestExchangeRateDateValidation()
		{
			const string messageError = "The Exchange Rate Date must not be in the future.";
			CombineAssertions(() =>
			{
				invoiceCharge.Validation.ValidateJ7_ExchangeRateDate();
				AssertNoMessageError("No Exchange Rate Date", invoiceCharge.J7_ExchangeRateDateInfo, messageError);
				invoiceCharge.J7_ExchangeRateDate = ZDate.Today.AddDays(1);
				AssertHasMessageError("Future", invoiceCharge.J7_ExchangeRateDateInfo, messageError);
				invoiceCharge.J7_ExchangeRateDate = ZDate.Today;
				AssertNoMessageError("Current", invoiceCharge.J7_ExchangeRateDateInfo, messageError);
			});
		}

		public void TestExchangeRateDateRequiredOnFixedRate()
		{
			CombineAssertions(() =>
			{
				invoiceCharge.IsJ7_ExchangeRateUserEnterable = false;
				invoiceCharge.Validation.ValidateJ7_ExchangeRateDate();
				AssertNoMessageErrorContaining("Non-fixed", invoiceCharge.J7_ExchangeRateDateInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceCharge.IsJ7_ExchangeRateUserEnterable = true;
				invoiceCharge.Validation.ValidateJ7_ExchangeRateDate();
				AssertHasMessageErrorContaining("Fixed rate, no Exchange Rate Date", invoiceCharge.J7_ExchangeRateDateInfo, MandatoryValidation.YouHaveNotEntered);
				invoiceCharge.J7_ExchangeRateDate = ZDate.Today;
				AssertNoMessageErrorContaining("Fixed rate, has Exchange Rate Date", invoiceCharge.J7_ExchangeRateDateInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJ7_IsGSTApplicable_IsFreightChargeToEUBorderByAirInsideEU()
		{
			const string message = "is GST-applicable.";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._3;
			invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._011;
			invoiceCharge.J7_IsGSTApplicable = false;
			var info = invoiceCharge.J7_IsGSTApplicableInfo;

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("IsFreightChargeToEUBorderByAirInsideEU = False", info, message);

				invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
				invoiceCharge.J7_IsGSTApplicable = false;
				AssertNoMessageErrorContaining("IsFreightChargeToEUBorderByAirInsideEU = True", info, message);
			});
		}

		public void TestCheckJ7_RX_NKCurrency()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestCheckJ7_RX_NKCurrency(invoiceCharge);
			});
		}

		public void TestCheckIsJ7_ExchangeRateIATA()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestCheckIsJ7_ExchangeRateIATA(invoiceCharge, charge => charge.IsJ7_ExchangeRateIATAInfo);
			});
		}

		public void TestCheckIsJ7_ExchangeRateUserEnterable()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestCheckIsJ7_ExchangeRateUserEnterable(invoiceCharge, charge => charge.IsJ7_ExchangeRateIATAInfo);
			});
		}

		public void TestCheckJ7_ExchangeRateDate()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestCheckJ7_ExchangeRateDate(invoiceCharge);
			});
		}

		public void TestValidateForRowNotificationJ_ChargeTypes_010_014()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestValidateForRowNotificationJ_ChargeTypes_010_014(invoiceCharge);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			groupInvoice = declaration.JobComInvoiceGroupHeaders[0];
			invoice = declaration.Invoices.AddNew();
			invoiceCharge = invoice.Charges.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceGroupHeader groupInvoice;
		JobComInvoiceHeader invoice;
		InvoiceCharge invoiceCharge;
	}
}
