using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineCalculatedFeeCollection))]
	sealed class CusEntryLineCalculatedFeeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusEntryLineCalculatedFeeCollection>
	{
		public void TestTheCurrencyOfCharges()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateCusMapType("CHG", "OUT", "CW1 Charge Codes to Customs codes", true);
			referenceDataHelper.CreateCusMap("CHG", "COM", "CZ", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2), "FR");
			referenceDataHelper.CreateCusMap("CHG", "CPA", "AD", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2), "FR");
			Factory.Save();

			SetExchangeRate(Factory, "EUR", 3m, new ZDateTime(2023, 1, 21));
			SetExchangeRate(Factory, "USD", 0.1m, new ZDateTime(2023, 1, 21));
			SetExchangeRate(Factory, "JPY", 1m, new ZDateTime(2023, 1, 21));

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = (CusEntryLine)entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_RX_NKInvoice_Currency = "JPY";
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Style = "11";
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 1, 21);

			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "COM";
			charge.J7_Amount = 1m;
			charge.J7_RX_NKCurrency = "USD";

			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = "CPA";
			charge2.J7_Amount = 2m;
			charge2.J7_RX_NKCurrency = "USD";

			var transportCharge1 = invoiceLine.Charges.AddNew();
			transportCharge1.J7_ChargeType = "OFT";
			transportCharge1.J7_Amount = 4m;
			transportCharge1.J7_IsDutiable = true;
			transportCharge1.J7_IsIncludedInITOT = false;
			transportCharge1.J7_RX_NKCurrency = "USD";

			var transportCharge2 = invoiceLine.Charges.AddNew();
			transportCharge2.J7_ChargeType = "ONS";
			transportCharge2.J7_Amount = 8m;
			transportCharge2.J7_IsDutiable = false;
			transportCharge2.J7_IsIncludedInITOT = true;
			transportCharge2.J7_RX_NKCurrency = "USD";

			var transportCharge3 = invoiceLine.Charges.AddNew();
			transportCharge3.J7_ChargeType = "OFT";
			transportCharge3.J7_Amount = 16m;
			transportCharge3.J7_IsDutiable = false;
			transportCharge3.J7_IsIncludedInITOT = false;
			transportCharge3.J7_RX_NKCurrency = "USD";

			var apportionedCharge1 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge1.J7_ChargeType = "COM";
			apportionedCharge1.J7_Amount = 32m;
			apportionedCharge1.J7_RX_NKCurrency = "USD";

			var apportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge2.J7_ChargeType = "COM";
			apportionedCharge2.J7_Amount = 64m;
			apportionedCharge2.J7_RX_NKCurrency = "USD";

			var transportApportionedCharge1 = invoiceLine.ApportionedCharges.AddNew();
			transportApportionedCharge1.J7_ChargeType = "CNE";
			transportApportionedCharge1.J7_Amount = 128m;
			transportApportionedCharge1.J7_IsDutiable = true;
			transportApportionedCharge1.J7_IsIncludedInITOT = false;
			transportApportionedCharge1.J7_RX_NKCurrency = "USD";

			var transportApportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
			transportApportionedCharge2.J7_ChargeType = "CEI";
			transportApportionedCharge2.J7_Amount = 256m;
			transportApportionedCharge2.J7_IsDutiable = false;
			transportApportionedCharge2.J7_IsIncludedInITOT = false;
			transportApportionedCharge2.J7_RX_NKCurrency = "USD";

			var transportApportionedCharge3 = invoiceLine.ApportionedCharges.AddNew();
			transportApportionedCharge3.J7_ChargeType = "CEI";
			transportApportionedCharge3.J7_Amount = 512m;
			transportApportionedCharge3.J7_IsDutiable = false;
			transportApportionedCharge3.J7_IsIncludedInITOT = true;
			transportApportionedCharge3.J7_RX_NKCurrency = "USD";

			GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency = "JPY";
			entryLine.CL_StatisticalValue = 1024;
			invoiceLine.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
			Assert(entryHeader.IsMultiInvoiceCurrency);
			AssertContainsExactElementsInAnyOrder("IsMultiInvoiceCurrency is true, InvoiceCurrency is equal to LocalCurrency, convert exchange rates for all charges to JPY.", new string[] { "AK|1320JPY", "BA|5200JPY", "CA|7920JPY", "AD|20JPY", "CZ|970JPY", "|1024JPY" }, entryLine.CusEntryLineCalculatedFees.Cast<CusEntryLineCalculatedFee>().Select(x => x.CustomsCode + "|" + x.Amount.ToString() + x.Currency));

			invoiceHeader2.JZ_RX_NKInvoice_Currency = "USD";
			Assert(!entryHeader.IsMultiInvoiceCurrency);
			AssertContainsExactElementsInAnyOrder("IsMultiInvoiceCurrency is false, InvoiceCurrency is USD, convert exchange rates for all charges to USD, excluding StatisticalValue.", new string[] { "AK|132USD", "BA|520USD", "CA|792USD", "AD|2USD", "CZ|97USD", "|1024JPY" }, entryLine.CusEntryLineCalculatedFees.Cast<CusEntryLineCalculatedFee>().Select(x => x.CustomsCode + "|" + x.Amount.ToString() + x.Currency));
		}

		void SetExchangeRate(BusinessObjectFactory factory, string currencyCode, decimal rate, ZDateTime date)
		{
			var currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
			var exchangeRate = currency.ExchangeRates.FirstOrDefault(x => x.RE_RX_NKExCurrency == currency.Code && x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRate && x.RE_GC == GlbCompany.CurrentCompany.PK && x.RE_StartDate <= ZDateTime.Today && x.RE_ExpiryDate >= ZDateTime.Today);
			if (exchangeRate == null)
			{
				exchangeRate = currency.ExchangeRates.AddNew();
				exchangeRate.RE_RX_NKExCurrency = currencyCode;
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = date.AddMonths(-1);
				exchangeRate.RE_ExpiryDate = date.AddMonths(1);
			}

			exchangeRate.RE_SellRate = rate;
		}

		public void TestCalculateCharge()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateCusMapType("CHG", "OUT", "CW1 Charge Codes to Customs codes", true);
			referenceDataHelper.CreateCusMap("CHG", "COM", "CZ", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2), "FR");
			referenceDataHelper.CreateCusMap("CHG", "CPA", "AD", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2), "FR");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = (CusEntryLine)entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "COM";
			charge.J7_Amount = 1m;
			charge.J7_RX_NKCurrency = "USD";

			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = "CPA";
			charge2.J7_Amount = 2m;
			charge2.J7_RX_NKCurrency = "USD";

			var transportCharge1 = invoiceLine.Charges.AddNew();
			transportCharge1.J7_ChargeType = "OFT";
			transportCharge1.J7_Amount = 4m;
			transportCharge1.J7_IsDutiable = true;
			transportCharge1.J7_IsIncludedInITOT = false;
			transportCharge1.J7_RX_NKCurrency = "USD";

			var transportCharge2 = invoiceLine.Charges.AddNew();
			transportCharge2.J7_ChargeType = "ONS";
			transportCharge2.J7_Amount = 8m;
			transportCharge2.J7_IsDutiable = false;
			transportCharge2.J7_IsIncludedInITOT = true;
			transportCharge2.J7_RX_NKCurrency = "USD";

			var transportCharge3 = invoiceLine.Charges.AddNew();
			transportCharge3.J7_ChargeType = "OFT";
			transportCharge3.J7_Amount = 16m;
			transportCharge3.J7_IsDutiable = false;
			transportCharge3.J7_IsIncludedInITOT = false;
			transportCharge3.J7_RX_NKCurrency = "USD";

			var apportionedCharge1 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge1.J7_ChargeType = "COM";
			apportionedCharge1.J7_Amount = 32m;
			apportionedCharge1.J7_RX_NKCurrency = "USD";

			var apportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge2.J7_ChargeType = "COM";
			apportionedCharge2.J7_Amount = 64m;
			apportionedCharge2.J7_RX_NKCurrency = "USD";

			var transportApportionedCharge1 = invoiceLine.ApportionedCharges.AddNew();
			transportApportionedCharge1.J7_ChargeType = "CNE";
			transportApportionedCharge1.J7_Amount = 128m;
			transportApportionedCharge1.J7_IsDutiable = true;
			transportApportionedCharge1.J7_IsIncludedInITOT = false;
			transportApportionedCharge1.J7_RX_NKCurrency = "USD";

			var transportApportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
			transportApportionedCharge2.J7_ChargeType = "CEI";
			transportApportionedCharge2.J7_Amount = 256m;
			transportApportionedCharge2.J7_IsDutiable = false;
			transportApportionedCharge2.J7_IsIncludedInITOT = false;
			transportApportionedCharge2.J7_RX_NKCurrency = "USD";

			var transportApportionedCharge3 = invoiceLine.ApportionedCharges.AddNew();
			transportApportionedCharge3.J7_ChargeType = "CEI";
			transportApportionedCharge3.J7_Amount = 512m;
			transportApportionedCharge3.J7_IsDutiable = false;
			transportApportionedCharge3.J7_IsIncludedInITOT = true;
			transportApportionedCharge3.J7_RX_NKCurrency = "USD";

			entryLine.CL_StatisticalValue = 1024;

			invoiceLine.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
			AssertContainsExactElementsInAnyOrder("There should be 2 AdditionsAndDeductions elements as both Charges and Apportioned Charges should be merged by charge type.", new string[] { "AK|132USD", "BA|520USD", "CA|792USD", "AD|2USD", "CZ|97USD", "|1024EUR" }, entryLine.CusEntryLineCalculatedFees.Cast<CusEntryLineCalculatedFee>().Select(x => x.CustomsCode + "|" + x.Amount.ToString() + x.Currency));
			AssertEquals("Stat. Value", entryLine.CusEntryLineCalculatedFees.Cast<CusEntryLineCalculatedFee>().First(x => x.Amount == 1024m).Category);
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			AssertEquals("AK = Sum of all dutiable transport charges non included in invoice", new Money(132, usd), entryLine.CusEntryLineCalculatedFees.CL_CalcAK);
			AssertEquals("CA = Sum of all non dutiable transport charges", new Money(792, usd), entryLine.CusEntryLineCalculatedFees.CL_CalcCA);
			AssertEquals("BA = Sum of all non dutiable transport charges included in invoice", new Money(520, usd), entryLine.CusEntryLineCalculatedFees.CL_CalcBA);

			invoiceLine.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._2;
			AssertContainsExactElementsInAnyOrder("If Valuation Method is other than 1, then only calculate codes AK / BA/ AN / BG / BC / CA / CZ.", new string[] { "AK|132USD", "BA|520USD", "CA|792USD", "CZ|97USD", "|1024EUR" }, entryLine.CusEntryLineCalculatedFees.Cast<CusEntryLineCalculatedFee>().Select(x => x.CustomsCode + "|" + x.Amount.ToString() + x.Currency));

			invoiceLine.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			Assert("When Valuation method = 1 and incoterm is CIF, transport charge AK will not be calculated", entryLine.CusEntryLineCalculatedFees.CL_CalcAK.Amount.IsDefault);

			invoiceLine.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeAlongsideShip;
			Assert(entryLine.CusEntryLineCalculatedFees.IsNat_146Applicable);
			Assert("When entryLine satisfies rule Nat_146, transport charge BA will not be calculated", entryLine.CusEntryLineCalculatedFees.CL_CalcBA.Amount.IsDefault);

			invoiceLine.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes._1277;
			Assert("If supplementary code uses '1277', transport charge CA will not be calculated", entryLine.CusEntryLineCalculatedFees.CL_CalcCA.Amount.IsDefault);
			AssertContainsExactElementsInAnyOrder("If supplementary code uses '1277', transport charge CZ will not be calculated", new string[] { "AD|2USD", "AK|132USD", "|1024EUR" }, entryLine.CusEntryLineCalculatedFees.Cast<CusEntryLineCalculatedFee>().Select(x => x.CustomsCode + "|" + x.Amount.ToString() + x.Currency));
		}

		public void TestIsIsNat_146Applicable()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			invoiceLine.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._2;
			Assert("ValuationCode is not equal to 1.", !entryLine.CusEntryLineCalculatedFees.IsNat_146Applicable);

			invoiceLine.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
			Assert("IncoTerm satisfies rule Nat_146, IncoTerm is ValuationCode is equal to 1.", entryLine.CusEntryLineCalculatedFees.IsNat_146Applicable);

			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeAlongsideShip;
			Assert("IncoTerm satisfies rule Nat_146, IncoTerm is ValuationCode is equal to 1.", entryLine.CusEntryLineCalculatedFees.IsNat_146Applicable);

			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeCarrier;
			Assert("IncoTerm satisfies rule Nat_146, IncoTerm is ValuationCode is equal to 1.", entryLine.CusEntryLineCalculatedFees.IsNat_146Applicable);

			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			Assert("IncoTerm satisfies rule Nat_146, IncoTerm is ValuationCode is equal to 1.", entryLine.CusEntryLineCalculatedFees.IsNat_146Applicable);

			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
			Assert("IncoTerm does not satisfy rule Nat_146, IncoTerm is ValuationCode is equal to 1.", !entryLine.CusEntryLineCalculatedFees.IsNat_146Applicable);
		}

		public void TestConstructor()
		{
			var cusEntryLine = Factory.New<CusEntryLine>();
			var invoiceLine = cusEntryLine.InvoiceLines.AddNew();
			invoiceLine.JI_ValuationCode = "A";

			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "COM";
			charge.J7_Amount = 10m;
			charge.J7_RX_NKCurrency = "EUR";

			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = "ADV";
			charge2.J7_Amount = 2m;
			charge2.J7_RX_NKCurrency = "EUR";

			var transportCharge1 = invoiceLine.Charges.AddNew();
			transportCharge1.J7_ChargeType = "CEI";
			transportCharge1.J7_Amount = 3m;
			transportCharge1.J7_IsDutiable = true;
			transportCharge1.J7_IsIncludedInITOT = false;
			transportCharge1.J7_RX_NKCurrency = "EUR";

			var transportCharge2 = invoiceLine.Charges.AddNew();
			transportCharge2.J7_ChargeType = "CEI";
			transportCharge2.J7_Amount = 8m;
			transportCharge2.J7_IsDutiable = false;
			transportCharge2.J7_IsIncludedInITOT = true;
			transportCharge2.J7_RX_NKCurrency = "EUR";
			var collection = new CusEntryLineCalculatedFeeCollection(cusEntryLine);
			var category = collection.Select(x => x.Category).ToArray();
			var code = collection.Select(x => x.ChargeType).ToArray();
			var amount = collection.Select(x => x.Amount).ToArray();
			var currency = collection.Select(x => x.Currency).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(4, collection.Count);
				AssertArrayEqualsByElements(new ZString[] { "Transport", "Transport", "Transport", "Stat. Value" }, category);
				AssertArrayEqualsByElements(new ZString[] { "AK", "BA", "CA", "" }, code);
				AssertArrayEqualsByElements(new ZDecimal[] { 3, 8, 8, 0 }, amount);
				AssertArrayEqualsByElements(new ZString[] { "EUR", "EUR", "EUR", "EUR" }, currency);
			});
		}

		public void TestApportionnedChargeWithRuleNat237()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = (CusEntryLine)entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.Charges.RemoveAndDeleteAll();
			invoiceLine.ApportionedCharges.RemoveAndDeleteAll();

			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = UniversalReferenceConstants.RefCusCodeList.ChargeType.AK;
			charge.J7_Amount = 1m;
			charge.J7_RX_NKCurrency = "USD";

			var apportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge2.J7_ChargeType = UniversalReferenceConstants.RefCusCodeList.ChargeType.AK;
			apportionedCharge2.J7_Amount = 64m;
			apportionedCharge2.J7_RX_NKCurrency = "USD";

			var collection = new CusEntryLineCalculatedFeeCollection(entryLine);
			CombineAssertions(() =>
			{
				var results = collection.Where(x => x.Category != "Stat. Value");
				AssertEquals(2, results.Count());
				AssertContainsExactElementsInAnyOrder("Rule 237 is enable as declaration is DeltaIE", new ZBool[] { true, false }, results.Select(x => x.IsLineLevel).ToArray());
			});

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			collection = new CusEntryLineCalculatedFeeCollection(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder("Rule 237 is disable as declaration is DeltaG", new ZBool[] { true, true }, collection.Select(x => x.IsLineLevel).ToArray());
			});
		}

		protected override CusEntryLineCalculatedFeeCollection GetCollectionToTest()
		{
			var entryLine = Factory.New<CusEntryLine>();
			return new CusEntryLineCalculatedFeeCollection(entryLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CusEntryLineCalculatedFee(Factory, ZString.Empty, Money.Empty);
	}
}
