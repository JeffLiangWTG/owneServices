using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class InvoiceLineChargeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestExchangeRateDateValidation()
		{
			const string messageError = "The Exchange Rate Date must not be in the future.";
			lineCharge.J7_ExchangeRateDate = ZDate.Today.AddDays(1);
			CombineAssertions(() =>
			{
				AssertHasMessageError("Future", lineCharge.J7_ExchangeRateDateInfo, messageError);
				lineCharge.J7_ExchangeRateDate = ZDate.Today;
				AssertNoMessageError("Current", lineCharge.J7_ExchangeRateDateInfo, messageError);
			});
		}

		public void TestExchangeRateDateRequiredOnFixedRate()
		{
			lineCharge.IsJ7_ExchangeRateUserEnterable = true;
			lineCharge.Validation.ValidateJ7_ExchangeRateDate();
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining(lineCharge.J7_ExchangeRateDateInfo, MandatoryValidation.YouHaveNotEntered);
				lineCharge.IsJ7_ExchangeRateUserEnterable = false;
				lineCharge.Validation.ValidateJ7_ExchangeRateDate();
				AssertNoMessageErrorContaining(lineCharge.J7_ExchangeRateDateInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJ7_ChargeDescription()
		{
			CombineAssertions(() =>
			{
				lineCharge.J7_ChargeType = ImportChargeCodeList.Codes._016;
				lineCharge.Validation.ValidateJ7_ChargeDescription();
				AssertHasMessageErrorContaining("Empty", lineCharge.J7_ChargeDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
				lineCharge.J7_ChargeDescription = "TEST";
				AssertNoMessageErrorContaining("Entered", lineCharge.J7_ChargeDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestValidationAgainstLineCharges()
		{
			const string errorMessage = "Group, Invoice and Line Charges of the same type cannot be entered with different currencies.";
			CombineAssertions(() =>
			{
				lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				lineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

				var lineCharge2 = invoiceLine.Charges.AddNew();
				lineCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				lineCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedArabEmirates;
				AssertHasMessageError("Same charge code, different currency", lineCharge2.J7_RX_NKCurrencyInfo, errorMessage);

				lineCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertNoMessageError("Same charge code, same currency", lineCharge2.J7_RX_NKCurrencyInfo, errorMessage);
			});
		}

		public void TestValidationAgainstHeaderCharges()
		{
			const string errorMessage = "Group, Invoice and Line Charges of the same type cannot be entered with different currencies.";
			CombineAssertions(() =>
			{
				var headerCharge = invoice.Charges.AddNew();
				headerCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				headerCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

				lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				lineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedArabEmirates;
				AssertHasMessageError("Same charge code, different currency", lineCharge.J7_RX_NKCurrencyInfo, errorMessage);

				lineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertNoMessageError("Same charge code, same currency", lineCharge.J7_RX_NKCurrencyInfo, errorMessage);
			});
		}

		public void TestValidationAgainstGroupCharges()
		{
			const string errorMessage = "Group, Invoice and Line Charges of the same type cannot be entered with different currencies.";
			CombineAssertions(() =>
			{
				var groupCharge = groupInvoice.Charges.AddNew();
				groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				groupCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

				lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				lineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedArabEmirates;
				AssertHasMessageError("Same charge code, different currency", lineCharge.J7_RX_NKCurrencyInfo, errorMessage);

				lineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertNoMessageError("Same charge code, same currency", lineCharge.J7_RX_NKCurrencyInfo, errorMessage);
			});
		}

		public void TestCheckJ7_AmountIsValidMoney()
		{
			const string errorMessage = "is too large, the value's range of";
			var targetInfo = lineCharge.J7_AmountInfo;
			CombineAssertions(() =>
			{
				lineCharge.J7_Amount = 1000000000m;
				AssertNoErrorContaining("Export, Amount < maximum", targetInfo, errorMessage);

				lineCharge.J7_Amount = 1000000000000000m;
				AssertHasErrorContaining("Export, Amount > maximum", targetInfo, errorMessage);
			});
		}

		public void TestCheckJ7_IsGSTApplicable_IsFreightChargeToEUBorderByAirInsideEU()
		{
			const string message = "is GST-applicable.";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._3;
			lineCharge.J7_ChargeType = ImportChargeCodeList.Codes._011;
			lineCharge.J7_IsGSTApplicable = false;
			var info = lineCharge.J7_IsGSTApplicableInfo;

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("IsFreightChargeToEUBorderByAirInsideEU = False", info, message);

				lineCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
				lineCharge.J7_IsGSTApplicable = false;
				AssertNoMessageErrorContaining("IsFreightChargeToEUBorderByAirInsideEU = True", info, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
			groupInvoice = declaration.JobComInvoiceGroupHeaders[0];
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			lineCharge = invoiceLine.Charges.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceGroupHeader groupInvoice;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		InvoiceLineCharge lineCharge;
	}
}
