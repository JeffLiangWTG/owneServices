using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CustomsFeeCalculatorTest : TestCaseWithFactory
	{
		public void TestGetChargePercentage()
		{
			testInvoiceHeader.JZ_IncoTerm = "CIF";
			var invoiceLine2 = (JobComInvoiceLine)testInvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testInstruction.PK;
			invoiceLine2.JI_CL = testEntryLine.PK;
			Factory.Save();
			var charge11 = testInvoiceLine.Charges.AddNew();
			charge11.J7_ChargeType = "OFT";
			charge11.J7_Percentage = 5;
			var charge12 = testInvoiceLine.Charges.AddNew();
			charge12.J7_ChargeType = "OFT";
			charge12.J7_Percentage = 6;
			var apportionedCharge1 = testInvoiceLine.ApportionedCharges.AddNew();
			apportionedCharge1.J7_ChargeType = "OFT";
			apportionedCharge1.J7_Percentage = 7;
			AssertEquals(0m, testCusEntryHeader.CalculateCustomsFee("OFT").Amount);
			testInvoiceLine.ApportionedCharges.RemoveAndDeleteAll();
			AssertEquals(0m, testCusEntryHeader.CalculateCustomsFee("OFT").Amount);
			testInvoiceLine.Charges.RemoveAndDelete(charge11);
			AssertEquals(0m, testCusEntryHeader.CalculateCustomsFee("OFT").Amount);
			var charge21 = invoiceLine2.Charges.AddNew();
			charge21.J7_ChargeType = "ONS";
			charge21.J7_Percentage = 6;
			AssertEquals(0m, testCusEntryHeader.CalculateCustomsFee("OFT").Amount);
			charge21.J7_ChargeType = "OFT";
			AssertEquals(6m, testCusEntryHeader.CalculateCustomsFee("OFT").Amount);
		}

		public void TestCalculateAmount()
		{
			testDeclaration.JE_MessageType = "IMP";
			testInvoiceHeader.JZ_IncoTerm = "FOB";
			var charge = testInvoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "OFT";
			charge.J7_Amount = 5.123m;
			charge.J7_RX_NKCurrency = "CNY";
			charge = testInvoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "ONS";
			charge.J7_Amount = 6.123m;
			charge.J7_RX_NKCurrency = "CNY";
			var charge1 = testInvoiceLine.ApportionedCharges.AddNew();
			charge1.J7_ChargeType = "ONS";
			charge1.J7_Amount = 6.123m;
			charge1.J7_RX_NKCurrency = "CNY";
			var result = CustomsFeeCalculator.CalculateCustomsFee(testCusEntryHeader, "OFT");
			AssertEquals(5.12m, result.Amount);
			result = CustomsFeeCalculator.CalculateCustomsFee(testCusEntryHeader, "ONS");
			AssertEquals(12.24m, result.Amount);
		}

		public void TestGetFeeAmount()
		{
			testDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			(testCusEntryHeader).CH_MessageType = EntryTypeList.Codes.CustomsEntry;
			var charge1 = testInvoiceLine.Charges.AddNew();
			charge1.J7_ChargeType = "OFT";
			charge1.J7_Percentage = 5m;
			AssertEquals(5m, testCusEntryHeader.FreightFee.Amount);
			charge1.J7_Percentage = 0;
			charge1.J7_Amount = 5.123m;
			charge1.J7_RX_NKCurrency = "CNY";
			var charge2 = testInvoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = "OFT";
			charge2.J7_Amount = 6.123m;
			charge2.J7_RX_NKCurrency = "CNY";
			AssertEquals(11.24m, testCusEntryHeader.FreightFee.Amount);
		}

		public void TestShouldPopulateFreightFee()
		{
			CombineAssertions(() =>
			{
				var chargeCode = CustomsChargeTypeList.Codes.OverseasFreight;
				testDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostAndInsurance, true);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostAndFreight, false);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostInsuranceAndFreight, false);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.FreeOnBoard, true);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.ExWorks, true);
				testDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostAndInsurance, false);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostAndFreight, true);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostInsuranceAndFreight, true);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.FreeOnBoard, false);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.ExWorks, false);
			});
		}

		public void TestShouldPopulateRoyaltyFee()
		{
			CombineAssertions(() =>
			{
				var chargeCode = CustomsChargeTypeList.Codes.Royalty;
				testDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostAndInsurance, true);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostAndFreight, true);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostInsuranceAndFreight, true);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.FreeOnBoard, true);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.ExWorks, true);
				testDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostAndInsurance, false);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostAndFreight, false);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostInsuranceAndFreight, false);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.FreeOnBoard, false);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.ExWorks, false);
			});
		}

		public void TestShouldPopulateInsuranceFee()
		{
			CombineAssertions(() =>
			{
				var chargeCode = CustomsChargeTypeList.Codes.OverseasInsurance;
				testDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostAndInsurance, false);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostAndFreight, true);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostInsuranceAndFreight, false);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.FreeOnBoard, true);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.ExWorks, true);
				testDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostAndInsurance, true);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostAndFreight, false);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.CostInsuranceAndFreight, true);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.FreeOnBoard, false);
				AssertShouldPopulateFee(chargeCode, Core.Constants.IncoTerms.ExWorks, false);
			});
		}

		void AssertShouldPopulateFee(string chargeCode, string incoTerm, bool expected)
		{
			testInvoiceHeader.JZ_IncoTerm = incoTerm;
			AssertEquals($"JE_MessageType:{testDeclaration.JE_MessageType}, JZ_IncoTerm:{testInvoiceHeader.JZ_IncoTerm}",
				expected, testCusEntryHeader.ShouldPopulateFee(chargeCode));
		}

		[TestDate(2018, 12, 11)]
		public void TestGetFeeCurrencyCode()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CURR", "Currencies");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CURR", "USD", "USD currency", new ZDateTime(2018, 1, 1), new ZDateTime(2018, 12, 31));
			Factory.Save();
			testDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testInvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			testCusEntryHeader.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
			var charge1 = testInvoiceLine.Charges.AddNew();
			charge1.J7_ChargeType = "OFT";
			charge1.J7_Percentage = 5m;
			charge1.J7_RX_NKCurrency = "USD";
			var charge2 = testInvoiceLine.ApportionedCharges.AddNew();
			charge2.J7_ChargeType = "OFT";
			charge2.J7_Percentage = 5m;
			charge2.J7_RX_NKCurrency = "USD";
			AssertEquals("", testCusEntryHeader.FreightFee.CurrencyCode);
			testInvoiceLine.ApportionedCharges.RemoveAndDeleteAll();
			AssertEquals("", testCusEntryHeader.FreightFee.CurrencyCode);
			charge1.J7_Percentage = 0;
			charge1.J7_Amount = 1m;
			AssertEquals("USD", testCusEntryHeader.FreightFee.CurrencyCode);
		}

		public void TestGetFeeMark()
		{
			testDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testInvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			(testCusEntryHeader).CH_MessageType = EntryTypeList.Codes.CustomsEntry;
			var charge1 = testInvoiceLine.Charges.AddNew();
			charge1.J7_ChargeType = "OFT";
			charge1.J7_Percentage = 5m;
			AssertEquals("1", testCusEntryHeader.CalculateCustomsFee("OFT").MarkCode);
			AssertEquals(FeeMarkTypeList.Descriptions.Percentage, testCusEntryHeader.CalculateCustomsFee("OFT").MarkDesc);
			var charge2 = testInvoiceLine.ApportionedCharges.AddNew();
			charge2.J7_ChargeType = "OFT";
			charge2.J7_Percentage = 5m;
			AssertEquals("3", testCusEntryHeader.CalculateCustomsFee("OFT").MarkCode);
			AssertEquals(FeeMarkTypeList.Descriptions.TotalPrice, testCusEntryHeader.CalculateCustomsFee("OFT").MarkDesc);
			charge1.J7_Percentage = 0;
			testInvoiceLine.ApportionedCharges.RemoveAndDeleteAll();
			AssertEquals("3", testCusEntryHeader.CalculateCustomsFee("OFT").MarkCode);
			AssertEquals(FeeMarkTypeList.Descriptions.TotalPrice, testCusEntryHeader.CalculateCustomsFee("OFT").MarkDesc);
			Assert("Fee mark type list should not be translatable", new FeeMarkTypeList() is UntranslatableCodeDescriptionPairList);
		}

		public void TestSum()
		{
			var cnyCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.China);
			SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 7.3281m);
			testInvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			for (var i = 0; i < 10; i++)
			{
				testInvoiceHeader.InvoiceLines.AddNew().JI_LinePrice = 1;
			}

			var sum = testInvoiceHeader.CurrencyConverter.Sum(testInvoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.JI_LinePriceMoney), cnyCurrency);
			AssertEquals("Amount should not be rounded", 73.281m, sum.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.China, sum.Currency.Code);
			sum = testInvoiceHeader.CurrencyConverter.Sum(testInvoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.JI_LinePriceMoney), cnyCurrency, true);
			AssertEquals("Amount should not be rounded", 73.28m, sum.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.China, sum.Currency.Code);
		}

		public void TestGetTotalPrice()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 7.3280m);
				testInvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.China;
				testInvoiceHeader.InvoiceLines.RemoveAndDeleteAll();
				testInvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 10m, Core.Constants.CurrencyCodes.UnitedStates);
				testInvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10m, Core.Constants.CurrencyCodes.UnitedStates);
				for (var i = 0; i < 10; i++)
				{
					var invoiceLine = testInvoiceHeader.InvoiceLines.AddNew();
					invoiceLine.JI_LinePrice = 100m;
					invoiceLine.JI_CL = testEntryLine.PK;
				}

				testInvoiceHeader.InvoiceLines[0].Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 3m, Core.Constants.CurrencyCodes.UnitedStates);
				testInvoiceHeader.InvoiceLines[1].Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 2m, Core.Constants.CurrencyCodes.UnitedStates);
				testDeclaration.ResumeApportionment();
				testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
				AssertEquals(1153.89m, testEntryLine.TotalPrice);
				testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
				AssertEquals(1080.61m, testEntryLine.TotalPrice);
				testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
				AssertEquals(1080.61m, testEntryLine.TotalPrice);
				testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				AssertEquals(1007.33m, testEntryLine.TotalPrice);
				testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
				AssertEquals(1007.33m, testEntryLine.TotalPrice);
			}
		}

		void SetExchangeRate(BusinessObjectFactory factory, string currencyCode, decimal rate)
		{
			var usdCurrency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
			var exchangeRate = usdCurrency.ExchangeRates.FirstOrDefault(x => x.RE_RX_NKExCurrency == usdCurrency.Code && x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRate && x.RE_GC == GlbCompany.CurrentCompany.PK && x.RE_StartDate <= ZDateTime.Today && x.RE_ExpiryDate >= ZDateTime.Today);
			if (exchangeRate == null)
			{
				exchangeRate = usdCurrency.ExchangeRates.AddNew();
				exchangeRate.RE_RX_NKExCurrency = currencyCode;
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = ZDateTime.Today.AddMonths(-1);
				exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);
			}

			exchangeRate.RE_SellRate = rate;
		}

		protected override void SetUp()
		{
			var setup = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testCusEntryHeader = setup.EntryHeader;
			testDeclaration = setup.JobDeclaration;
			testEntryLine = setup.EntryLine;
			testInvoiceHeader = setup.InvoiceHeader;
			testInvoiceLine = setup.InvoiceLine;
			testInstruction = setup.EntryInstruction;
		}

		CusEntryHeader testCusEntryHeader;
		CusEntryLine testEntryLine;
		JobDeclaration testDeclaration;
		JobComInvoiceHeader testInvoiceHeader;
		JobComInvoiceLine testInvoiceLine;
		CusEntryInstruction testInstruction;
	}
}
