using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class ESCustomsValuationCalculatorTest : TestCaseWithFactory
	{
		public void TestAdjValues()
		{
			invoice.JZ_IncoTerm = "DDP";
			invoice.JZ_RX_NKInvoice_Currency = jobDeclaration.LocalCurrencyCode;
			var groupHeader = invoice.GroupHeader;

			var oNS = groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 30m, jobDeclaration.LocalCurrencyCode);
			oNS.J7_IsDutiable = true;
			oNS.J7_IsIncludedInITOT = false;
			oNS.J7_DistributeBy = "VAL";
			var oFT = groupHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, 20m, jobDeclaration.LocalCurrencyCode);
			oFT.J7_IsDutiable = false;
			oFT.J7_IsIncludedInITOT = true;
			oFT.J7_DistributeBy = "VAL";

			invoiceLine.JI_LinePrice = 100m;
			jobDeclaration.ResumeApportionment();

			var calculator = new ESCustomsValuationCalculator(invoiceLine);

			AssertEquals("Positive adjustment", 30m, calculator.GetAdjustement(true));
			AssertEquals("Negative adjustment", 20m, calculator.GetAdjustement(false));
		}

		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestVATAdditionsCalculation()
		{
			SetUpExchangeRate();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.EuropeanUnion;

			var charge1 = invoiceLine.Charges.AddNew();
			charge1.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			charge1.J7_IsDutiable = false;
			charge1.J7_IsIncludedInITOT = false;
			charge1.J7_IsGSTApplicable = true;
			charge1.J7_Amount = 1220m;
			charge1.J7_RX_NKCurrency = CurrencyCodes.Japan;

			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			charge2.J7_IsDutiable = false;
			charge2.J7_IsIncludedInITOT = false;
			charge2.J7_IsGSTApplicable = true;
			charge2.J7_Amount = 50;
			charge2.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

			var charge3 = invoiceLine.Charges.AddNew();
			charge3.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			charge3.J7_IsDutiable = false;
			charge3.J7_IsIncludedInITOT = true;
			charge3.J7_IsGSTApplicable = true;
			charge3.J7_Amount = 35;
			charge3.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

			var charge4 = invoiceLine.Charges.AddNew();
			charge4.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			charge4.J7_IsDutiable = true;
			charge4.J7_IsIncludedInITOT = false;
			charge4.J7_IsGSTApplicable = false;
			charge4.J7_Amount = 45;
			charge4.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

			invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, 25m, jobDeclaration.LocalCurrencyCode);

			var calculator = new ESCustomsValuationCalculator(invoiceLine);
			AssertEquals("VAT Additions (12.2+50)+35-45-25", 27.2m, calculator.GetVATAdditions(invoiceLine.LocalCurrency), 0.01m);
		}

		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestGetVATAdditionValue()
		{
			SetUpExchangeRate();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.EuropeanUnion;

			var charge3 = invoiceLine.Charges.AddNew();
			charge3.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			charge3.J7_IsDutiable = false;
			charge3.J7_IsIncludedInITOT = true;
			charge3.J7_IsGSTApplicable = true;
			charge3.J7_Amount = 35;
			charge3.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

			var charge4 = invoiceLine.Charges.AddNew();
			charge4.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			charge4.J7_IsDutiable = true;
			charge4.J7_IsIncludedInITOT = false;
			charge4.J7_IsGSTApplicable = false;
			charge4.J7_Amount = 45;
			charge4.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

			var calculator = new ESCustomsValuationCalculator(invoiceLine);
			AssertEquals("VAT Addition Value 45", 45m, calculator.GetVATAdditionValue(invoiceLine.LocalCurrency));
		}

		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestGetVATDeductionValue()
		{
			SetUpExchangeRate();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.EuropeanUnion;

			var charge3 = invoiceLine.Charges.AddNew();
			charge3.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			charge3.J7_IsDutiable = false;
			charge3.J7_IsIncludedInITOT = true;
			charge3.J7_IsGSTApplicable = true;
			charge3.J7_Amount = 35;
			charge3.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

			var charge4 = invoiceLine.Charges.AddNew();
			charge4.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			charge4.J7_IsDutiable = true;
			charge4.J7_IsIncludedInITOT = false;
			charge4.J7_IsGSTApplicable = false;
			charge4.J7_Amount = 45;
			charge4.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

			var calculator = new ESCustomsValuationCalculator(invoiceLine);
			AssertEquals("VAT Deduction Value 35", 35m, calculator.GetVATDeductionValue(invoiceLine.LocalCurrency));
		}

		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestGetEGVChargeAmount()
		{
			SetUpExchangeRate();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.EuropeanUnion;

			var charge3 = invoiceLine.Charges.AddNew();
			charge3.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			charge3.J7_IsDutiable = true;
			charge3.J7_IsIncludedInITOT = false;
			charge3.J7_IsGSTApplicable = false;
			charge3.J7_Amount = 45;
			charge3.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

			CombineAssertions(() =>
			{
				var calculator = new ESCustomsValuationCalculator(invoiceLine);
				AssertEquals("EGVChargeAmount is 0 when no EGV charge is declared", 0m, calculator.GetEGVChargeAmount());

				var charge4 = invoiceLine.Charges.AddNew();
				charge4.J7_ChargeType = ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing;
				charge4.J7_Amount = 35;
				charge4.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

				AssertEquals("EGVChargeAmount is 35 when an EGV charge is declared with that value", 35m, calculator.GetEGVChargeAmount());

				var charge5 = invoiceLine.Charges.AddNew();
				charge5.J7_ChargeType = ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing;
				charge5.J7_Amount = 35;
				charge5.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

				AssertEquals("EGVChargeAmount is 70 (the sum of all EGV charges declared)", 70m, calculator.GetEGVChargeAmount());
			});
		}

		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestGetEGPChargeAmount()
		{
			SetUpExchangeRate();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.EuropeanUnion;

			var charge3 = invoiceLine.Charges.AddNew();
			charge3.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			charge3.J7_IsDutiable = true;
			charge3.J7_IsIncludedInITOT = false;
			charge3.J7_IsGSTApplicable = false;
			charge3.J7_Amount = 45;
			charge3.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

			CombineAssertions(() =>
			{
				var calculator = new ESCustomsValuationCalculator(invoiceLine);
				AssertEquals("EGPChargeAmount is 0 when no EGP charge is declared", 0m, calculator.GetEGPChargeAmount());

				var charge4 = invoiceLine.Charges.AddNew();
				charge4.J7_ChargeType = ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing;
				charge4.J7_Amount = 35;
				charge4.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

				AssertEquals("EGPChargeAmount is 35 when an EGP charge is declared with that value", 35m, calculator.GetEGPChargeAmount());

				var charge5 = invoiceLine.Charges.AddNew();
				charge5.J7_ChargeType = ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing;
				charge5.J7_Amount = 35;
				charge5.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

				AssertEquals("EGPChargeAmount is 70 (the sum of all EGP charges declared)", 70m, calculator.GetEGPChargeAmount());
			});
		}

		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestGetREAChargeAmount()
		{
			SetUpExchangeRate();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.EuropeanUnion;

			var charge3 = invoiceLine.Charges.AddNew();
			charge3.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			charge3.J7_IsDutiable = true;
			charge3.J7_IsIncludedInITOT = false;
			charge3.J7_IsGSTApplicable = false;
			charge3.J7_Amount = 45;
			charge3.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

			CombineAssertions(() =>
			{
				var calculator = new ESCustomsValuationCalculator(invoiceLine);
				AssertEquals("REAChargeAmount is 0 when no REA charge is declared", 0m, calculator.GetREAChargeAmount());

				var charge4 = invoiceLine.Charges.AddNew();
				charge4.J7_ChargeType = ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation;
				charge4.J7_Amount = 35;
				charge4.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

				AssertEquals("REAChargeAmount is 35 when an REA charge is declared with that value", 35m, calculator.GetREAChargeAmount());

				var charge5 = invoiceLine.Charges.AddNew();
				charge5.J7_ChargeType = ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation;
				charge5.J7_Amount = 35;
				charge5.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;

				AssertEquals("REAChargeAmount is 70 (the sum of all REA charges declared)", 70m, calculator.GetREAChargeAmount());

				AssertEquals("REAChargeAmount is 7000 (the sum of all REA charges declared) when currency is specified (JPY)", 7000m, calculator.GetREAChargeAmount(RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.Japan)));
			});
		}

		void SetUpExchangeRate()
		{
			var usd = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.Japan);
			usd.ExchangeRates.DeleteAll();

			var thisMonthUsdRate = usd.ExchangeRates.AddNew();
			thisMonthUsdRate.RE_ExRateType = "CUS";
			thisMonthUsdRate.RE_StartDate = new ZDateTime(1986, 3, 1, 0, 0, 1);
			thisMonthUsdRate.RE_ExpiryDate = new ZDateTime(1987, 3, 1, 0, 0, 1);
			thisMonthUsdRate.RE_SellRate = 100;
			Factory.Save();
		}

		public void TestGetAmountToAddToITOTForStatistical()
		{
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.EuropeanUnion;
			invoice.JZ_IncoTerm = "EXW";
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;

			var charge1 = AddNewCharge(ESCustomsChargeTypeList.Codes.InternationalFreight, true, false, true, 20);
			var charge2 = AddNewCharge(ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry, false, false, false, 10);
			var charge3 = AddNewCharge(UCCCustomsChargeTypeList.Codes.InterestCharge, false, true, false, 5);

			var calculator = new ESCustomsValuationCalculator(invoiceLine);

			CombineAssertions(() =>
			{
				AssertEquals("Statistical Value Calculator for Import Declaration (20-5 = 15)", 15m, calculator.GetAmountToAddToITOTForStatistical(invoiceLine.LocalCurrency));

				jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
				foreach (var chargeKey in invoiceLine.Charges.AllChargeKeys)
				{
					invoiceLine.Charges.ClearCharge(chargeKey.ChargeKey);
				}
				charge1 = AddNewCharge(ESCustomsChargeTypeList.Codes.InternationalFreightExp, true, true, true, 5);
				charge2 = AddNewCharge(ESCustomsChargeTypeList.Codes.InsuranceUntilESBorder, false, false, true, 10);
				charge3 = AddNewCharge(ESCustomsChargeTypeList.Codes.OtherNationalPayments, false, true, true, 20);

				AssertEquals("Statistical Value Calculator for Export Declaration (-5+10 = 5)", 5m, calculator.GetAmountToAddToITOTForStatistical(invoiceLine.LocalCurrency));
			});
		}

		InvoiceLineCharge AddNewCharge(string chargeCode, bool isDutiable, bool isIncludedInTOT, bool isStatisticalValueApplicable, ZDecimal amount)
		{
			var newCharge = invoiceLine.Charges.AddNew();
			newCharge.J7_ChargeType = chargeCode;
			newCharge.J7_IsDutiable = isDutiable;
			newCharge.J7_IsIncludedInITOT = isIncludedInTOT;
			newCharge.J7_Amount = amount;
			newCharge.J7_IsStatisticalValueApplicable = isStatisticalValueApplicable;
			newCharge.J7_RX_NKCurrency = jobDeclaration.LocalCurrencyCode;
			return newCharge;
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			invoice = jobDeclaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
		JobDeclaration jobDeclaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
