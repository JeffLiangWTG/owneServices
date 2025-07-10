using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class GroupInvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestValidationAgainstGroupCharges()
		{
			const string errorMessage = "Charges of the same type cannot be entered with different currencies.";
			CombineAssertions(() =>
			{
				groupInvoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				groupInvoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

				var groupCharge2 = groupInvoice.Charges.AddNew();
				groupCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				groupCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedArabEmirates;
				AssertHasMessageError("Same charge code, different currency", groupCharge2.J7_RX_NKCurrencyInfo, errorMessage);

				groupCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertNoMessageError("Same charge code, same currency", groupCharge2.J7_RX_NKCurrencyInfo, errorMessage);
			});
		}

		public void TestExchangeRateDateValidation()
		{
			const string messageError = "The Exchange Rate Date must not be in the future.";
			CombineAssertions(() =>
			{
				groupInvoiceCharge.Validation.ValidateJ7_ExchangeRateDate();
				AssertNoMessageError("No Exchange Rate Date", groupInvoiceCharge.J7_ExchangeRateDateInfo, messageError);
				groupInvoiceCharge.J7_ExchangeRateDate = ZDate.Today.AddDays(1);
				AssertHasMessageError("Future", groupInvoiceCharge.J7_ExchangeRateDateInfo, messageError);
				groupInvoiceCharge.J7_ExchangeRateDate = ZDate.Today;
				AssertNoMessageError("Current", groupInvoiceCharge.J7_ExchangeRateDateInfo, messageError);
			});
		}

		public void TestExchangeRateDateRequiredOnFixedRate()
		{
			CombineAssertions(() =>
			{
				groupInvoiceCharge.IsJ7_ExchangeRateUserEnterable = false;
				groupInvoiceCharge.Validation.ValidateJ7_ExchangeRateDate();
				AssertNoMessageErrorContaining("Non-fixed", groupInvoiceCharge.J7_ExchangeRateDateInfo, MandatoryValidation.YouHaveNotEntered);

				groupInvoiceCharge.IsJ7_ExchangeRateUserEnterable = true;
				groupInvoiceCharge.Validation.ValidateJ7_ExchangeRateDate();
				AssertHasMessageErrorContaining("Fixed rate, no Exchange Rate Date", groupInvoiceCharge.J7_ExchangeRateDateInfo, MandatoryValidation.YouHaveNotEntered);
				groupInvoiceCharge.J7_ExchangeRateDate = ZDate.Today;
				AssertNoMessageErrorContaining("Fixed rate, has Exchange Rate Date", groupInvoiceCharge.J7_ExchangeRateDateInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJ7_AmountIsValidMoney_Import_ChargeTypeAIR()
		{
			const string errorMessage = "is too large, the maximum value allowed for Charge Code Amount is 999,999,999.99.";
			var targetInfo = groupInvoiceCharge.J7_AmountInfo;
			groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes.AIR;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				groupInvoiceCharge.J7_Amount = 0m;
				AssertNoErrorContaining("Import, Amount < maximum", targetInfo, errorMessage);

				groupInvoiceCharge.J7_Amount = 999999999.99m;
				AssertNoErrorContaining("Import, Amount maximum", targetInfo, errorMessage);

				groupInvoiceCharge.J7_Amount = 1000000000m;
				AssertHasErrorContaining("Import, Amount > maximum", targetInfo, errorMessage);

				groupInvoiceCharge.J7_Amount = -1000000000m;
				AssertHasErrorContaining("Import, Amount < minimum", targetInfo, errorMessage);
			});
		}

		public void TestCheckJ7_AmountIsValidMoney_Import_ChargeTypeOther()
		{
			const string errorMessage = "is too large, the value's range of";
			var targetInfo = groupInvoiceCharge.J7_AmountInfo;
			groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				groupInvoiceCharge.J7_Amount = 1000000000m;
				AssertNoErrorContaining("Import, Amount < maximum", targetInfo, errorMessage);

				groupInvoiceCharge.J7_Amount = 1000000000000000m;
				AssertHasErrorContaining("Import, Amount > maximum", targetInfo, errorMessage);
			});
		}

		public void TestCheckJ7_AmountIsValidMoney_Export()
		{
			const string errorMessage = "is too large, the value's range of";
			var targetInfo = groupInvoiceCharge.J7_AmountInfo;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				groupInvoiceCharge.J7_Amount = 1000000000m;
				AssertNoErrorContaining("Export, Amount < maximum", targetInfo, errorMessage);

				groupInvoiceCharge.J7_Amount = 1000000000000000m;
				AssertHasErrorContaining("Export, Amount > maximum", targetInfo, errorMessage);
			});
		}

		public void TestCheckJ7_IsGSTApplicable_IsFreightChargeToEUBorderByAirInsideEU()
		{
			const string message = "is GST-applicable.";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoice = groupInvoice.JobComInvoiceHeaders.AddNew();
			invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._3;
			groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._011;
			groupInvoiceCharge.J7_IsGSTApplicable = false;
			var info = groupInvoiceCharge.J7_IsGSTApplicableInfo;

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("IsFreightChargeToEUBorderByAirInsideEU = False", info, message);

				groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
				groupInvoiceCharge.J7_IsGSTApplicable = false;
				AssertNoMessageErrorContaining("IsFreightChargeToEUBorderByAirInsideEU = True", info, message);
			});
		}

		public void TestCheckJ7_RX_NKCurrency()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestCheckJ7_RX_NKCurrency(groupInvoiceCharge);
			});
		}

		public void TestCheckIsJ7_ExchangeRateIATA()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestCheckIsJ7_ExchangeRateIATA(groupInvoiceCharge, charge => charge.IsJ7_ExchangeRateIATAInfo);
			});
		}

		public void TestCheckIsJ7_ExchangeRateUserEnterable()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestCheckIsJ7_ExchangeRateUserEnterable(groupInvoiceCharge, charge => charge.IsJ7_ExchangeRateIATAInfo);
			});
		}

		public void TestCheckJ7_ExchangeRateDate()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestCheckJ7_ExchangeRateDate(groupInvoiceCharge);
			});
		}

		public void TestValidateForRowNotificationJ_ChargeTypes_010_014()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestValidateForRowNotificationJ_ChargeTypes_010_014(groupInvoiceCharge);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			groupInvoice = declaration.JobComInvoiceGroupHeaders[0];
			groupInvoiceCharge = groupInvoice.Charges.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceGroupHeader groupInvoice;
		GroupInvoiceCharge groupInvoiceCharge;
	}
}
