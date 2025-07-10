using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class DdpCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			SetupInvoiceForDDP();
			var invoiceLine1 = SetupInvoiceLine();
			var invoiceLine2 = SetupInvoiceLine();

			var dutyCalculator = new DutyCalculatorStrategy(declaration);
			var testResult = dutyCalculator.CalculateAdValoremDutyRateForInvoiceLine(invoiceLine1);
			AssertEquals("Pre-requisite: calculated duty rate for line 1", 20m, testResult);

			var ddpResult = DdpCalculator.Calculate(declaration);
			AssertNotNull(ddpResult);
			AssertEquals("LinesMissingCommodityPreferenceOriginOrPrice", 0, ddpResult.LinesMissingCommodityPreferenceOriginOrPrice.Count);
			AssertEquals("LinesNotAttractingSimpleAdValoremDuty", 0, ddpResult.LinesNotAttractingSimpleAdValoremDuty.Count);
			AssertEquals("LinesAlreadyHave9WKSWorksheet", 0, ddpResult.LinesAlreadyHave9WKSWorksheet.Count);

			AssertEquals("Line1 Price", 25.66m, invoiceLine1.JI_LinePrice);
			AssertEquals("Line2 Price", 25.66m, invoiceLine2.JI_LinePrice);

			var doc1 = ((IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument>)invoiceLine1.SupportingDocuments).FirstOrDefault(x => x.CSI_Code == "9WKS");
			AssertNotNull("Line1 should have a 9WKS supporting document", doc1);
			AssertEquals("Line1 9WKS text", "DDP adjustment from 30.79 with rate 20% down to 25.66", doc1.CSI_Description);

			var doc2 = ((IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument>)invoiceLine2.SupportingDocuments).FirstOrDefault(x => x.CSI_Code == "9WKS");
			AssertNotNull("Line2 should have a 9WKS supporting document", doc2);
			AssertEquals("Line2 9WKS text", "DDP adjustment from 30.79 with rate 20% down to 25.66", doc2.CSI_Description);
			AssertEquals("Line2 reference text", "JOB123", doc2.CSI_ReferenceNumber);
		}

		public void TestCalculate_Empty()
		{
			var invoiceLine1 = SetupInvoiceLine();
			var invoiceLine2 = SetupInvoiceLine();

			var ddpResult = DdpCalculator.Calculate(declaration);
			AssertEquals(DdpCalculator.NoValidInvoicesFound, ddpResult);

			AssertEquals("Line1 Price should not have been changed", 30.79m, invoiceLine1.JI_LinePrice);
			AssertEquals("Line2 Price should not have been changed", 30.79m, invoiceLine2.JI_LinePrice);
		}

		public void TestLinesMissingCommodityPreferenceOriginOrPrice()
		{
			SetupInvoiceForDDP();
			var invoiceLine1 = SetupInvoiceLine(setTariff: false);
			var invoiceLine2 = SetupInvoiceLine(setPrice: false);
			var invoiceLine3 = SetupInvoiceLine(setOrigin: false);
			var invoiceLine4 = SetupInvoiceLine(setPreference: false);
			var invoiceLine5 = SetupInvoiceLine();

			var ddpResult = DdpCalculator.Calculate(declaration);
			AssertNotNull(ddpResult);
			AssertContainsExactElementsInAnyOrder("LinesMissingCommodityPreferenceOriginOrPrice", new List<JobComInvoiceLine> { invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4 }, ddpResult.LinesMissingCommodityPreferenceOriginOrPrice);
			AssertEquals("LinesNotAttractingSimpleAdValoremDuty", 0, ddpResult.LinesNotAttractingSimpleAdValoremDuty.Count);
			AssertEquals("LinesAlreadyHave9WKSWorksheet", 0, ddpResult.LinesAlreadyHave9WKSWorksheet.Count);

			AssertEquals("Line1 Price should not have been changed", 30.79m, invoiceLine1.JI_LinePrice);
			AssertEquals("Line2 Price should not have been changed", 0m, invoiceLine2.JI_LinePrice);
			AssertEquals("Line3 Price should not have been changed", 30.79m, invoiceLine3.JI_LinePrice);
			AssertEquals("Line4 Price should not have been changed", 30.79m, invoiceLine4.JI_LinePrice);
			AssertEquals("Line5 Price", 25.66m, invoiceLine5.JI_LinePrice);
		}

		public void TestLinesNotAttractingSimpleAdValoremDuty()
		{
			SetupInvoiceForDDP();
			var invoiceLine1 = SetupInvoiceLine();
			var invoiceLine2 = SetupInvoiceLine(setComplexTariff: true);

			var dutyCalculator = new DutyCalculatorStrategy(declaration);
			var testResult = dutyCalculator.CalculateAdValoremDutyRateForInvoiceLine(invoiceLine2);
			AssertNull("Pre-requisite: line 2 should not have ad-valorem duty", testResult);

			var ddpResult = DdpCalculator.Calculate(declaration);
			AssertNotNull(ddpResult);
			AssertEquals("LinesMissingCommodityPreferenceOriginOrPrice", 0, ddpResult.LinesMissingCommodityPreferenceOriginOrPrice.Count);
			AssertContainsExactElementsInAnyOrder("LinesNotAttractingSimpleAdValoremDuty", new List<JobComInvoiceLine> { invoiceLine2 }, ddpResult.LinesNotAttractingSimpleAdValoremDuty);
			AssertEquals("LinesAlreadyHave9WKSWorksheet", 0, ddpResult.LinesAlreadyHave9WKSWorksheet.Count);

			AssertEquals("Line1 Price", 25.66m, invoiceLine1.JI_LinePrice);
			AssertEquals("Line2 Price should not have been changed", 30.79m, invoiceLine2.JI_LinePrice);

			var doc1 = ((IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument>)invoiceLine1.SupportingDocuments).FirstOrDefault(x => x.CSI_Code == "9WKS");
			AssertNotNull("Line1 should have a 9WKS supporting document", doc1);
			AssertEquals("Line1 9WKS text", "DDP adjustment from 30.79 with rate 20% down to 25.66", doc1.CSI_Description);
			var doc2 = ((IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument>)invoiceLine2.SupportingDocuments).FirstOrDefault(x => x.CSI_Code == "9WKS");
			AssertNull("Line2 should not have a 9WKS supporting document", doc2);
		}

		public void TestLinesAlreadyHave9WKSWorksheet()
		{
			SetupInvoiceForDDP();
			var invoiceLine1 = SetupInvoiceLine();
			var invoiceLine2 = SetupInvoiceLine();
			_ = invoiceLine1.SupportingDocuments.AddNew("9WKS", "reference number");

			var ddpResult = DdpCalculator.Calculate(declaration);
			AssertNotNull(ddpResult);
			AssertEquals("LinesMissingCommodityPreferenceOriginOrPrice", 0, ddpResult.LinesMissingCommodityPreferenceOriginOrPrice.Count);
			AssertEquals("LinesNotAttractingSimpleAdValoremDuty", 0, ddpResult.LinesNotAttractingSimpleAdValoremDuty.Count);
			AssertContainsExactElementsInAnyOrder("LinesAlreadyHave9WKSWorksheet", new List<JobComInvoiceLine> { invoiceLine1 }, ddpResult.LinesAlreadyHave9WKSWorksheet);

			AssertEquals("Line1 Price should not have been changed", 30.79m, invoiceLine1.JI_LinePrice);
			AssertEquals("Line2 Price", 25.66m, invoiceLine2.JI_LinePrice);
		}

		public void TestGetDDPCalculationResults()
		{
			SetupInvoiceForDDP();
			var invoiceLine1 = SetupInvoiceLine(setTariff: false, invoiceNumber: "INVOICE1", invoiceHeader: invoice1);
			var invoiceLine2 = SetupInvoiceLine(setPrice: false, invoiceNumber: "INVOICE1", invoiceHeader: invoice1);
			var invoiceLine3 = SetupInvoiceLine(setOrigin: false, invoiceNumber: "INVOICE1", invoiceHeader: invoice1);
			var invoiceLine4 = SetupInvoiceLine(setPreference: false, invoiceNumber: "INVOICE1", invoiceHeader: invoice1);
			var invoiceLine5 = SetupInvoiceLine(invoiceNumber: "INVOICE1", invoiceHeader: invoice1);

			var invoiceLine1OnHeader2 = SetupInvoiceLine(setTariff: false, invoiceNumber: "INVOICE2", invoiceHeader: invoice2);
			var invoiceLine2OnHeader2 = SetupInvoiceLine(setPrice: false, invoiceNumber: "INVOICE2", invoiceHeader: invoice2);

			var invoiceLine6 = SetupInvoiceLine(invoiceNumber: "INVOICE3", invoiceHeader: invoice3);
			var invoiceLine7 = SetupInvoiceLine(setComplexTariff: true, invoiceNumber: "INVOICE3", invoiceHeader: invoice3);

			var invoiceLine8 = SetupInvoiceLine(invoiceNumber: "INVOICE4", invoiceHeader: invoice4);
			var invoiceLine9 = SetupInvoiceLine(invoiceNumber: "INVOICE4", invoiceHeader: invoice4);
			_ = invoiceLine8.SupportingDocuments.AddNew("9WKS", "reference number");

			AssertContains("DDP Calculation results formatted for display",
				"The calculator skipped one or more invoice lines due to the following incompatible data or state. Please adjust those manually.\n" +
				"Lines lacking commodity, preference, origin or price:\n" +
				"\tInvoice INVOICE1, line(s) 1, 2, 3, 4\n" +
				"\tInvoice INVOICE2, line(s) 1, 2\n" +
				"Lines not attracting simple ad valorem duty:\n" +
				"\tInvoice INVOICE3, line(s) 2\n" +
				"Lines which already have a 9WKS worksheet:\n" +
				"\tInvoice INVOICE4, line(s) 1\n",
				declaration.GetDDPCalculationResults());
		}

		public void TestGetDDPCalculationResults_NoLinesSkipped()
		{
			SetupInvoiceForDDP();
			var invoiceLine1 = SetupInvoiceLine(invoiceNumber: "INVOICE1", invoiceHeader: invoice1);
			var invoiceLine2 = SetupInvoiceLine(invoiceNumber: "INVOICE1", invoiceHeader: invoice1);
			var invoiceLine3 = SetupInvoiceLine(invoiceNumber: "INVOICE1", invoiceHeader: invoice1);
			var invoiceLine4 = SetupInvoiceLine(invoiceNumber: "INVOICE1", invoiceHeader: invoice1);
			var invoiceLine5 = SetupInvoiceLine(invoiceNumber: "INVOICE1", invoiceHeader: invoice1);
			var invoiceLine1OnHeader2 = SetupInvoiceLine(invoiceNumber: "INVOICE2", invoiceHeader: invoice2);
			var invoiceLine2OnHeader2 = SetupInvoiceLine(invoiceNumber: "INVOICE2", invoiceHeader: invoice2);
			var invoiceLine6 = SetupInvoiceLine(invoiceNumber: "INVOICE3", invoiceHeader: invoice3);
			var invoiceLine8 = SetupInvoiceLine(invoiceNumber: "INVOICE4", invoiceHeader: invoice4);
			var invoiceLine9 = SetupInvoiceLine(invoiceNumber: "INVOICE4", invoiceHeader: invoice4);

			AssertContains("DDP Calculation results when no lines have been skipped", "The calculator successfully processed 10 invoice lines.", declaration.GetDDPCalculationResults());
		}

		public void SetupTariffs()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedKingdom, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedKingdom, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.CreateCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, rateType.PK);
			var preference = helper.CreatePreferenceForCountry("100", "1233", Core.Constants.CountryCodes.UnitedKingdom);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedKingdom, tariffType.PK, "11111111", startDate, endDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedKingdom, tariffType.PK, "22222222", startDate, endDate);
			var rate1 = helper.CreateRefCusRate(tariff1.PK, rateCode.PK, startDate, endDate, "VFD * 0.2", preferencePk: preference.PK, dataGrouping: Core.Constants.CountryCodes.UnitedKingdom);
			var rate2 = helper.CreateRefCusRate(tariff2.PK, rateCode.PK, startDate, endDate, "[AVX] * 120", preferencePk: preference.PK, dataGrouping: Core.Constants.CountryCodes.UnitedKingdom);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedKingdom, "TRADE GROUP", startDate, endDate);
			_ = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Finland, startDate.Date, endDate.Date);
			_ = helper.CreateCusApplicability(rate1.PK, tradeGroup, startDate, endDate);
			_ = helper.CreateCusApplicability(rate2.PK, tradeGroup, startDate, endDate);
			Factory.Save();
		}

		public void SetupInvoiceForDDP()
		{
			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoice3.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice3.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoice4.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice4.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
		}

		public JobComInvoiceLine SetupInvoiceLine(bool setTariff = true, bool setPrice = true, bool setOrigin = true, bool setPreference = true, bool setComplexTariff = false, string invoiceNumber = "", JobComInvoiceHeader invoiceHeader = null)
		{
			JobComInvoiceLine invoiceLine;

			if (invoiceHeader == null)
			{
				invoiceHeader = invoice1;
			}
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			if (setComplexTariff)
			{
				invoiceLine.JI_Tariff = "22222222";
			}
			else if (setTariff)
			{
				invoiceLine.JI_Tariff = "11111111";
			}
			if (setPrice)
			{
				invoiceLine.JI_LinePrice = 30.79m;
			}
			if (setOrigin)
			{
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Finland;
			}
			if (setPreference)
			{
				invoiceLine.JI_PrimaryPreference = "100";
			}
			if (!string.IsNullOrEmpty(invoiceNumber))
			{
				invoiceHeader.JZ_InvoiceNumber = invoiceNumber;
			}
			return invoiceLine;
		}

		protected override void SetUp()
		{
			SetupTariffs();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Core.Constants.CountryCodes.UnitedKingdom;
			declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "JOB123";
			invoice1 = declaration.Invoices.AddNew();
			invoice2 = declaration.Invoices.AddNew();
			invoice3 = declaration.Invoices.AddNew();
			invoice4 = declaration.Invoices.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice1;
		JobComInvoiceHeader invoice2;
		JobComInvoiceHeader invoice3;
		JobComInvoiceHeader invoice4;
	}
}
