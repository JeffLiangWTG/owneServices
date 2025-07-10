using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	sealed class InvoiceChargeTest : EU.Business.Declaration.Testing.InvoiceChargeTest
	{
		public void TestLookups()
		{
			AssertType<InvoiceChargeLookups>(invoiceCharge.Lookups);
		}

		public void TestValidation()
		{
			AssertType<InvoiceChargeValidation>(invoiceCharge.Validation);
		}

		public void TestJ7_IsDutiable_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes._001, invoiceCharge.J7_IsDutiableInfo);
		}

		public void TestJ7_IsStatisticalValueApplicable_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes._002, invoiceCharge.J7_IsStatisticalValueApplicableInfo);
		}

		public void TestJ7_IsGSTApplicable_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes._003, invoiceCharge.J7_IsGSTApplicableInfo);
		}

		public void TestJ7_Calc_IsIncludedInInvoiceAmount_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(EU.Business.ChargeTypeList.Codes.StatisticalValue, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmountInfo);
		}

		public void TestJ7_IsIncludedInITOT_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes.TCE, invoiceCharge.J7_IsIncludedInITOTInfo);
		}

		public void TestJ7_Percentage_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(CustomsChargeTypeList.Codes.Discount, invoiceCharge.J7_PercentageInfo);
		}

		public void TestIsJ7_ExchangeRateIATA_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes.INP, invoiceCharge.IsJ7_ExchangeRateIATAInfo);
		}

		public void TestIsJ7_ExchangeRateIATA()
		{
			CombineAssertions(() =>
			{
				invoiceCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.IATARate;
				AssertEquals("Getter - true", true, invoiceCharge.IsJ7_ExchangeRateIATA);
				invoiceCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
				AssertEquals("Getter - false", false, invoiceCharge.IsJ7_ExchangeRateIATA);

				invoiceCharge.IsJ7_ExchangeRateIATA = true;
				AssertEquals("Setter - true", ChargeExchangeRateTypeList.Codes.IATARate, invoiceCharge.J7_ExchangeRateType);
				invoiceCharge.IsJ7_ExchangeRateIATA = false;
				AssertEquals("Setter - false", ZString.Empty, invoiceCharge.J7_ExchangeRateType);
			});
		}

		public void TestIATAAndFixedRateAreMutuallyExclusive()
		{
			CombineAssertions(() =>
			{
				invoiceCharge.IsJ7_ExchangeRateUserEnterable = true;
				invoiceCharge.J7_ExchangeRate = 1.25m;
				AssertEquals("Fixed Rate = true -> IATA = false", false, invoiceCharge.IsJ7_ExchangeRateIATA);

				invoiceCharge.IsJ7_ExchangeRateIATA = true;
				AssertEquals("IATA = true -> Fixed Rate = false", false, invoiceCharge.IsJ7_ExchangeRateUserEnterable);
				AssertEquals("IATA = true -> Exchange Rate = 0", 0m, invoiceCharge.J7_ExchangeRate);
			});
		}

		public void TestJ7_ExchangeRate_IATARateUsed()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestJ7_ExchangeRate_IATARateUsed(invoiceCharge, invoiceCharge.IsJ7_ExchangeRateIATAInfo);
			});
		}

		public void TestChangingChargeTypeResetsIATA()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default value", false, invoiceCharge.IsJ7_ExchangeRateIATA);
				invoiceCharge.IsJ7_ExchangeRateIATA = true;
				invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
				AssertEquals("Updated charge code supports IATA", true, invoiceCharge.IsJ7_ExchangeRateIATA);
				invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._011;
				AssertEquals("Updated another charge code supports IATA", true, invoiceCharge.IsJ7_ExchangeRateIATA);
				invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._012;
				AssertEquals("Updated charge code doesn't support IATA", false, invoiceCharge.IsJ7_ExchangeRateIATA);
			});
		}

		public void TestClearAmountIfChargeTypeIsSetToBlank()
		{
			invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			invoiceCharge.J7_Amount = 11.0m;
			CombineAssertions(() =>
			{
				AssertNotEquals("Pre-condition", ZDecimal.Zero, invoiceCharge.J7_Amount);
				invoiceCharge.J7_ChargeType = ZString.Empty;
				AssertEquals("Cleared", ZDecimal.Zero, invoiceCharge.J7_Amount);
			});
		}

		public void TestClearCurrencyIfChargeTypeIsSetToBlank()
		{
			invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			CombineAssertions(() =>
			{
				AssertNotEquals("Pre-condition", ZString.Empty, invoiceCharge.J7_RX_NKCurrency);
				invoiceCharge.J7_ChargeType = ZString.Empty;
				AssertEquals("Cleared", ZString.Empty, invoiceCharge.J7_RX_NKCurrency);
			});
		}

		public void TestJ7_ChargeType_SetValuesIfNeeded()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._014;
				AssertEquals("J7_IsDutiable", false, invoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsStatisticalValueApplicable", false, invoiceCharge.J7_IsStatisticalValueApplicable);
				AssertEquals("J7_IsGSTApplicable", true, invoiceCharge.J7_IsGSTApplicable);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_IsIncludedInITOT", true, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("IsJ7_ExchangeRateIATA", false, invoiceCharge.IsJ7_ExchangeRateIATA);
			});
		}

		public void TestOnSaving_SetValuesForApportionChargesIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			var invoiceCharge = invoice.Charges.AddNew(ImportChargeCodeList.Codes._010, 1000m, declaration.LocalCurrencyCode);
			invoiceCharge.IsJ7_ExchangeRateIATA = true;
			declaration.ResumeApportionment();

			invoiceCharge.OnSaving();
			AssertEquals(true, invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == ImportChargeCodeList.Codes._010).IsJ7_ExchangeRateIATA);
		}

		public void TestJ7_ChargeType_DefaultCurrencyIfNeeded()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes.INP;
				AssertEquals("J7_RX_NKCurrency is empty", ZString.Empty, invoiceCharge.J7_RX_NKCurrency);

				invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes.TCE;
				AssertEquals("J7_RX_NKCurrency is 'EUR'", Core.Constants.CurrencyCodes.EuropeanUnion, invoiceCharge.J7_RX_NKCurrency);
			});
		}

		public void TestJ7_RX_NKCurrencyInfo_ReadOnly()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes.INP;
				AssertEquals("Not TCE type", false, invoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);

				invoiceCharge.J7_Percentage = 0.25m;
				AssertEquals("base.GetJ7_RX_NKCurrency_ReadOnly() is true", true, invoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);

				invoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes.TCE;
				invoiceCharge.J7_Percentage = ZDecimal.Zero;
				AssertEquals("Is TCE type", true, invoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			});
		}

		public override void TestResetDefaultIsIncludedInAmount()
		{
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = GetIncotermToTestIsIncludedInInvoice();
			var internationalFreightCharge = invoice.Charges.AddNew();

			internationalFreightCharge.J7_ChargeType = ChargeCodeList.Codes.OverseasFreight;
			internationalFreightCharge.J7_Amount = 100m;
			internationalFreightCharge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;
			AssertEquals("Default IsIncludedInAmount for 011", true, internationalFreightCharge.J7_Calc_IsIncludedInInvoiceAmount);
		}

		public override void TestReadOnlyOfIsIncludedInInvoice()
		{
			Assert("DE has its specific rule for the logic of IntoTerm -> ChargeCode readonly/requires.", true);
		}

		public override void TestJ7_Calc_IsIncludedInInvoice()
		{
			Assert("Split into multiple tests (TestChargeFlags_)", true);
		}

		public void TestChargeFlags_AdditionCharge()
		{
			var charge = SetupChargeWithType(ChargeCodeList.Codes.AdditionCharge);
			AssertChargeFlags(charge, false, false, false, true, false);
		}

		public void TestChargeFlags_DeductionCharge()
		{
			var charge = SetupChargeWithType(ChargeCodeList.Codes.DeductionCharge);
			AssertChargeFlags(charge, false, false, false, false, false);
		}

		public void TestChargeFlags_EUBorderFreight()
		{
			var charge = SetupChargeWithType(ChargeCodeList.Codes.EUBorderFreight);
			AssertChargeFlags(charge, false, false, false, true, false);
		}

		public void TestChargeFlags_EUBorderInsurance()
		{
			var charge = SetupChargeWithType(ChargeCodeList.Codes.EUBorderInsurance);
			AssertChargeFlags(charge, false, false, false, true, false);
		}

		public void TestChargeFlags_OverseasFreight()
		{
			var charge = SetupChargeWithType(ChargeCodeList.Codes.OverseasFreight);
			AssertChargeFlags(charge, false, false, false, false, false);
		}

		public void TestChargeFlags_OverseasInsurance()
		{
			var charge = SetupChargeWithType(ChargeCodeList.Codes.OverseasInsurance);
			AssertChargeFlags(charge, false, false, false, false, false);
		}

		public void TestDefaultValues()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			var charge = invoice.Charges.AddNew();

			AssertEquals(false, charge.J7_IsDutiable);
		}

		public override void TestIsIncludedInLinesReadOnly()
		{
			// no charges included in lines
			Assert(true);
		}

		protected override string GetChargeTypeForTestJ7_IsNotIncludedInInvoiceSetOnFactorySaving() => ChargeCodeList.Codes.OverseasFreight;

		protected override string GetIncoTermForTestJ7_IsNotIncludedInInvoiceSetOnFactorySaving() => "CFR";

		protected override string GetIncotermToTestIsIncludedInInvoice() => "CFR";

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoiceCharge = invoice.Charges.AddNew();
		}
		JobDeclaration declaration;
		InvoiceCharge invoiceCharge;

		void AssertReadOnlyForImportAndHighValueOvrd(ZString chargeType, ZPropertyInfo propertyInfo)
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				invoiceCharge.J7_ChargeType = chargeType;
				AssertEquals("ZPropertyInfo ReadOnly is false", false, propertyInfo.ReadOnly);

				declaration.ZG_IsHighValueOvrd = ZBool.True;
				AssertEquals("ZPropertyInfo ReadOnly is true", true, propertyInfo.ReadOnly);
			});
		}

		InvoiceCharge SetupChargeWithType(string chargeType)
		{
			var invoice = declaration.Invoices.AddNew();
			var charge = invoice.Charges.AddNew();
			charge.J7_Amount = 10m;
			charge.J7_RX_NKCurrency = "AUD";
			charge.J7_ChargeType = chargeType;
			return charge;
		}

		void AssertChargeFlags(InvoiceCharge charge, bool isIncludedInInvoice, bool isIncludedInInvoiceLine, bool isDutiable, bool isStatisticalValueApplicable, bool isVATApplicable)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Included In Invoice", isIncludedInInvoice, !charge.J7_IsNotIncludedInInvoice);
				AssertEquals("Included In Invoice Line", isIncludedInInvoiceLine, charge.J7_IsIncludedInITOT);
				AssertEquals("Dutiable", isDutiable, charge.J7_IsDutiable);
				AssertEquals("Statistical Value Applicable", isStatisticalValueApplicable, charge.J7_IsStatisticalValueApplicable);
				AssertEquals("VAT Applicable", isVATApplicable, charge.J7_IsGSTApplicable);
			});
		}
	}
}
