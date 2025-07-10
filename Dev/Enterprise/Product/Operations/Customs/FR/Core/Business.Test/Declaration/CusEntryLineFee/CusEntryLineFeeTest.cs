using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	class CusEntryLineFeeTest : EU.Business.Declaration.Testing.CusEntryLineFeeTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
	{
		public void TestMethodOfPaymentDescription()
		{
			Setup104IMList();
			var (declaration, entryLine, entryLineFee) = SetEntryLineFeeData();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			entryLineFee.CF_MethodOfPayment = "A";
			AssertEquals("Payment in cash", entryLineFee.MethodOfPaymentDescription);
			entryLineFee.CF_MethodOfPayment = "B";
			AssertEquals("Payment by credit card", entryLineFee.MethodOfPaymentDescription);
			entryLineFee.CF_MethodOfPayment = "C";
			AssertEquals("Payment by cheque", entryLineFee.MethodOfPaymentDescription);
			entryLineFee.CF_MethodOfPayment = "D";
			AssertEquals("Other (e. g. direct debit to agent's cash account)", entryLineFee.MethodOfPaymentDescription);
		}

		public void TestUserEnteredStashSource()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			AssertType<CusEntryLineFeeUserEnteredStashSource>(entryLineFee.UserEnteredStashSource);
		}

		public void TestChargeAmountRounderType()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			AssertType<IntegerFeeRounder>(entryLineFee.ChargeAmountRounder);
		}

		public void TestIsNationalIndirectTaxationFee()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			entryLineFee.CF_ChargeType = "C702";
			AssertEquals(false, entryLineFee.IsNationalIndirectTaxationFee);
			entryLineFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A336;
			AssertEquals(false, entryLineFee.IsNationalIndirectTaxationFee);
		}

		public void TestReadOnlyFields()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			entryLineFee.G4_RateOverride = "";

			AssertEquals("G4_MethodOfPayment isn't ReadOnly", false, entryLineFee.G4_MethodOfPaymentInfo.ReadOnly);
			AssertEquals("G4_RateOverride isn't ReadOnly", false, entryLineFee.G4_RateOverrideInfo.ReadOnly);

			AssertEquals("G4_RateDuty is ReadOnly", true, entryLineFee.G4_RateDutyInfo.ReadOnly);
			AssertEquals("G4_Type is ReadOnly", true, entryLineFee.G4_TypeInfo.ReadOnly);
			AssertEquals("G4_Amount is ReadOnly", false, entryLineFee.G4_AmountInfo.ReadOnly);
			AssertEquals("G4_BaseAmount is ReadOnly", true, entryLineFee.G4_BaseAmountInfo.ReadOnly);
			AssertEquals("G4_RateSuspension is ReadOnly", true, entryLineFee.G4_RateSuspensionInfo.ReadOnly);

			entryLineFee.G4_RateOverride = "ADD";
			AssertEquals("G4_MethodOfPayment isn't ReadOnly", false, entryLineFee.G4_MethodOfPaymentInfo.ReadOnly);
			AssertEquals("G4_RateOverride isn't ReadOnly", false, entryLineFee.G4_RateOverrideInfo.ReadOnly);
			AssertEquals("G4_RateDuty isn't ReadOnly", false, entryLineFee.G4_RateDutyInfo.ReadOnly);
			AssertEquals("G4_Type isn't ReadOnly", false, entryLineFee.G4_TypeInfo.ReadOnly);
			AssertEquals("G4_BaseAmount isn't ReadOnly", false, entryLineFee.G4_BaseAmountInfo.ReadOnly);
			AssertEquals("G4_RateSuspension isn't ReadOnly", false, entryLineFee.G4_RateSuspensionInfo.ReadOnly);

			AssertEquals("G4_Amount is ReadOnly", false, entryLineFee.G4_AmountInfo.ReadOnly);

			entryLineFee.G4_RateOverride = "PRE";

			AssertEquals("G4_MethodOfPayment isn't ReadOnly", false, entryLineFee.G4_MethodOfPaymentInfo.ReadOnly);
			AssertEquals("G4_RateOverride isn't ReadOnly", false, entryLineFee.G4_RateOverrideInfo.ReadOnly);

			AssertEquals("G4_RateDuty is ReadOnly", true, entryLineFee.G4_RateDutyInfo.ReadOnly);
			AssertEquals("G4_Type is ReadOnly", true, entryLineFee.G4_TypeInfo.ReadOnly);
			AssertEquals("G4_Amount is ReadOnly", false, entryLineFee.G4_AmountInfo.ReadOnly);
			AssertEquals("G4_BaseAmount is ReadOnly", true, entryLineFee.G4_BaseAmountInfo.ReadOnly);
			AssertEquals("G4_RateSuspension is ReadOnly", true, entryLineFee.G4_RateSuspensionInfo.ReadOnly);
		}

		public void TestAi2Amount()
		{
			var toDay = ZDate.Today;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("ZZ1", 10.0m, "FR", 0m, 0m, GuaranteeTypeList.Codes.AI2, toDay.AddDays(-1), toDay.AddDays(1), "Test AI2");
			helper.CreateTaxOrFee("ZZ2", 20.0m, "FR", 0m, 0m, Core.Constants.Customs.CusEntryFeeTypes.VAT, toDay.AddDays(-1), toDay.AddDays(1), "Test VAT");
			helper.CreateTaxOrFee("ZZ3", 30.0m, "FR", 0m, 0m, Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, toDay.AddDays(-1), toDay.AddDays(1), "Test DTY");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = toDay;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();

			invoiceLine.JI_CL = cusEntryLine.PK;

			var fee1 = cusEntryLine.Fees.AddNew();
			fee1.NationalFeeTypeCode = "ZZ1";
			fee1.CF_ChargeAmount = 100m;

			var fee2 = cusEntryLine.Fees.AddNew();
			fee2.NationalFeeTypeCode = "ZZ2";
			fee2.CF_ChargeAmount = 200m;

			var fee3 = cusEntryLine.Fees.AddNew();
			fee3.NationalFeeTypeCode = "ZZ3";
			fee3.CF_ChargeAmount = 400m;

			var fee4 = cusEntryLine.Fees.AddNew();
			fee4.NationalFeeTypeCode = "";
			fee4.CF_ChargeAmount = 1m;

			var fee5 = cusEntryLine.Fees.AddNew();
			fee5.NationalFeeTypeCode = "";
			fee5.CF_ChargeAmount = 1m;

			var fee6 = cusEntryLine.Fees.AddNew();
			fee6.NationalFeeTypeCode = "ZZ4";
			fee6.CF_ChargeAmount = 1m;
			fee6.CF_ChargeType = FeeTypeCodeConverter.EUFeeCodeForVAT;

			AssertEquals(0m, cusEntryHeader.Ai2Amount);

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;

			AssertEquals(301m, cusEntryHeader.Ai2Amount);

			fee2.CF_ChargeAmount = 1200m;
			AssertEquals(1301m, cusEntryHeader.Ai2Amount);

			fee5.CF_ChargeType = FeeTypeCodeConverter.EUFeeCodeForVAT;
			AssertEquals(1302m, cusEntryHeader.Ai2Amount);
		}

		public void TestTaxBaseAndAmountInDeclarationCurrencyHaveNoDecimal()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			var taxBoxSupporter = (IDocSADHLineTaxBoxSupporter)entryLineFee;
			entryLineFee.CF_ChargeType = "C702";

			entryLineFee.CF_BaseValue = 212.012m;
			entryLineFee.CF_ChargeAmount = 213.021m;
			AssertEquals("TaxBase", "212", taxBoxSupporter.TaxBase);
			AssertEquals("AmountInDeclarationCurrency", "213", taxBoxSupporter.AmountInDeclarationCurrency);

			entryLineFee.CF_BaseValue = 212.912m;
			entryLineFee.CF_ChargeAmount = 213.621m;
			AssertEquals("TaxBase", "213", taxBoxSupporter.TaxBase);
			AssertEquals("AmountInDeclarationCurrency", "214", taxBoxSupporter.AmountInDeclarationCurrency);

			entryLineFee.CF_BaseValue = 212m;
			entryLineFee.CF_ChargeAmount = 213m;
			AssertEquals("TaxBase", "212", taxBoxSupporter.TaxBase);
			AssertEquals("AmountInDeclarationCurrency", "213", taxBoxSupporter.AmountInDeclarationCurrency);

			entryLineFee.CF_BaseValue = 0m;
			AssertEquals("TaxBase", "0", taxBoxSupporter.TaxBase);

			entryLineFee.CF_ChargeAmount = 0.09m;
			AssertEquals("AmountInDeclarationCurrency should be 0 if < 0.50", "0", taxBoxSupporter.AmountInDeclarationCurrency);

			entryLineFee.CF_ChargeAmount = 0.51m;
			AssertEquals("AmountInDeclarationCurrency should be 1 if > 0.50", "1", taxBoxSupporter.AmountInDeclarationCurrency);

			entryLineFee.CF_ChargeAmount = 0.50m;
			AssertEquals("AmountInDeclarationCurrency should be 1 if = 0.50", "1", taxBoxSupporter.AmountInDeclarationCurrency);
		}

		public void TestIDocSADHLineTaxRate()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			var taxBoxSupporter = (IDocSADHLineTaxBoxSupporter)entryLineFee;

			entryLineFee.CF_Rate = 12.1450000;
			AssertEquals("Rate should have 3 decimals: 12.145 from 12.1450000.", "12.145", taxBoxSupporter.Rate);

			entryLineFee.CF_Rate = 12.1452000;
			AssertEquals("Rate should have 3 decimals: 12.145 from 12.1452000.", "12.145", taxBoxSupporter.Rate);

			entryLineFee.CF_Rate = 12.1459000;
			AssertEquals("Rate should have 3 decimals: 12.145 without round from 12.1459000.", "12.145", taxBoxSupporter.Rate);

			entryLineFee.CF_Rate = 12.14000;
			AssertEquals("Rate should have 3 decimals: 12.140 from 12.14000.", "12.140", taxBoxSupporter.Rate);

			entryLineFee.CF_Rate = 12.100000;
			AssertEquals("Rate should have 3 decimals: 12.100 from 12.100000.", "12.100", taxBoxSupporter.Rate);

			entryLineFee.CF_Rate = 12.00000;
			AssertEquals("Rate should have 3 decimals: 12.000 from 12.00000.", "12.000", taxBoxSupporter.Rate);
		}

		public void TestIsActionBlank()
		{
			var (declaration, entryLine, entryLineFee) = SetEntryLineFeeData();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			entryLineFee.CF_RateOverrideReasonCode = ZString.Empty;
			AssertEquals("In DeltaIE Declaration, IsActionBlank should be true when RateOverrideReasonCode is empty.", true, entryLineFee.IsActionBlank);
			entryLineFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Precalcule;
			AssertEquals("In DeltaIE Declaration, IsActionBlank should be false when RateOverrideReasonCode is PRE.", false, entryLineFee.IsActionBlank);
			entryLineFee.CF_RateOverrideReasonCode = "ADD";
			AssertEquals("In DeltaIE Declaration, IsActionBlank should be false when RateOverrideReasonCode is not empty and not PRE.", false, entryLineFee.IsActionBlank);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			entryLineFee.CF_RateOverrideReasonCode = ZString.Empty;
			AssertEquals("In DeltaG Declaration, IsActionBlank should be true when RateOverrideReasonCode is empty.", true, entryLineFee.IsActionBlank);
			entryLineFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Precalcule;
			AssertEquals("In DeltaG Declaration, IsActionBlank should be true when RateOverrideReasonCode is PRE.", true, entryLineFee.IsActionBlank);
			entryLineFee.CF_RateOverrideReasonCode = "ADD";
			AssertEquals("In DeltaG Declaration, IsActionBlank should be false when RateOverrideReasonCode is not empty and not PRE.", false, entryLineFee.IsActionBlank);
		}

		public void TestLookups()
		{
			var (declaration, entryLine, entryLineFee) = SetEntryLineFeeData();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<DeltaIECusEntryLineFeeLookups>("In DeltaIE Declaration, Lookups should be DeltaIECusEntryLineFeeLookups.", entryLineFee.Lookups);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertType<DeltaGCusEntryLineFeeLookups>("In DeltaG Declaration, Lookups should be DeltaGCusEntryLineFeeLookups.", entryLineFee.Lookups);
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new CusEntryLineFeeLightValidationTester(bizObjToTest);
		}

		sealed class CusEntryLineFeeLightValidationTester : LightValidationTester
		{
			public CusEntryLineFeeLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return base.ShouldTestProperty(info)
					&& propertyName != JobComInvoiceLine.Schema.JI_Tariff
					&& propertyName != JobComInvoiceLine.Schema.JI_CEI
					&& propertyName != CusEntryInstruction.Schema.CEI_DateForDuty;
			}
		}

		void Setup104IMList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "CCI Method of Payment");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "A", "Payment in cash", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "B", "Payment by credit card", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "C", "Payment by cheque", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "D", "Other (e. g. direct debit to agent's cash account)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}
	}
}
