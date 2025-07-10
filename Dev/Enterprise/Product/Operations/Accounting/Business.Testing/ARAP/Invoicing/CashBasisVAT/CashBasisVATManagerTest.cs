using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class CashBasisVATManagerTest : TestCaseWithFactory
	{
		public void TestPaymentMethodPRP()
		{
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.Proportionally.Code);

			var invoice = CreateARInvoice();

			var matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 0;
			matchLink.AP_MatchDate = ZDateTime.BrettsBirthday;
			matchLink.AP_MatchGroupNum = "M000";
			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 100;
			matchLink.AP_MatchDate = ZDateTime.BrettsBirthday;
			matchLink.AP_MatchGroupNum = "M123";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(4, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 26.25m, 3.24m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 52.51m, 6.49m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, 26.25m, 0m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 4, -13.13m, -1.62m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 35;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M321";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(4, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(8);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 9.19m, 1.14m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 18.38m, 2.27m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, 9.19m, 0m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 4, -4.59m, -0.57m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 260;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M222";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(4, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(12);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 64.56m, 7.98m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 129.11m, 15.96m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, 64.56m, 0m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 4, -32.28m, -3.99m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 335.9;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M333";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(12);

			AssertInvoiceFullyMatched(invoice);
		}

		public void TestPaymentMethodPRP_WithMatchingByLines()
		{
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.Proportionally.Code);

			var invoice = CreateARInvoice();

			var matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 100;
			matchLink.AP_MatchDate = ZDateTime.BrettsBirthday;
			matchLink.AP_MatchGroupNum = "M123";
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[2], 100.01m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[3], 56.17m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[4], -56.18m);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(3, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(3);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 89.01m, 11.00m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, 56.17m, 0m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 4, -50.00m, -6.18m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 35;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M321";
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[1], 35m);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(1, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 31.15m, 3.85m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 260;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M222";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[1], 77.36m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[2], 124.71m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[3], 43.83m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[5], 14.10m);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			AssertEquals(3, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(7);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 68.85m, 8.51m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 110.99m, 13.72m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, 43.83m, 0m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 335.90;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M333";
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[0]);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[5], 285.90m);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(7);

			AssertInvoiceFullyMatched(invoice);
		}

		public void TestPaymentMethodPRP_WithZeroTotalTax()
		{
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.Proportionally.Code);

			var chargeCode = TestObjectCreator.CC1;
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", TestObjectCreator.USD, 2M, TestObjectCreator.LocalClient);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, 200M, 0M, 0, chargeCode.PK);
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", 100M, line.AL_LineAmount);
			AssertEquals("Precondition", 0M, line.AL_GSTVAT);

			line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, 200M, 0M, 0, chargeCode.PK);
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", 100M, line.AL_LineAmount);
			AssertEquals("Precondition", 0M, line.AL_GSTVAT);

			line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, 100M, 0M, 0, chargeCode.PK);
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", 50M, line.AL_LineAmount);
			AssertEquals("Precondition", 0M, line.AL_GSTVAT);

			var matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 20.77;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M123";
			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(2);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 13.85m, 0m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 6.92m, 0m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 130;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M111";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 86.15m, 0m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 43.08m, 0m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 99.23;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M222";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);

			AssertInvoiceFullyMatched(invoice);
		}

		public void TestPaymentMethodPRP_WithZeroTotalTax_WithMatchingByLines()
		{
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.Proportionally.Code);

			var chargeCode = TestObjectCreator.CC1;
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", TestObjectCreator.USD, 2M, TestObjectCreator.LocalClient);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, 200M, 0M, 0, chargeCode.PK);
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", 100M, line.AL_LineAmount);
			AssertEquals("Precondition", 0M, line.AL_GSTVAT);

			line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, 200M, 0M, 0, chargeCode.PK);
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", 100M, line.AL_LineAmount);
			AssertEquals("Precondition", 0M, line.AL_GSTVAT);

			line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, 100M, 0M, 0, chargeCode.PK);
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", 50M, line.AL_LineAmount);
			AssertEquals("Precondition", 0M, line.AL_GSTVAT);

			var matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 20.77;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M123";
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[1], 10.55m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[2], 10.22m);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(2);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 10.55m, 0m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 10.22m, 0m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 130;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M111";
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[0], 0.77m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[1], 89.45m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[2], 39.78m);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 89.45m, 0m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 39.78m, 0m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 99.23;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M222";
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[0], 99.23m);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);

			AssertInvoiceFullyMatched(invoice);
		}

		public void TestPaymentMethod1ST_Case1()
		{
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.TaxFirst.Code);

			var invoice = CreateARInvoice();

			var matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 100;
			matchLink.AP_MatchDate = ZDateTime.BrettsBirthday;
			matchLink.AP_MatchGroupNum = "M123";
			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(4, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 100.00m, 12.36m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 200.00m, 24.72m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, 100.00m, 0m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 4, -50.00m, -6.18m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 35;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M111";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 260;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M222";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 335.90;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M333";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);

			AssertInvoiceFullyMatched(invoice);
		}

		public void TestPaymentMethod1ST_Case1_WithMatchingByLines()
		{
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.TaxFirst.Code);

			var invoice = CreateARInvoice();

			var matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 100;
			matchLink.AP_MatchDate = ZDateTime.BrettsBirthday;
			matchLink.AP_MatchGroupNum = "M123";
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[1], 100.01m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[2], 20.11m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[4], -20.12m);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(3, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(3);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 100.00m, 12.36m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 162.70m, 20.11m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 4, -50.00m, -6.18m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 35;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M111";
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[1], 12.35m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[3], 22.65m);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(1, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, 100.00m, 0m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 260;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M222";
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[0], 14.10m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[2], 204.61m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[3], 77.35m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[4], -36.06m);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(1, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(5);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 37.30m, 4.61m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 335.9;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M333";
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[0], 35.90m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[5]);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(5);

			AssertInvoiceFullyMatched(invoice);
		}

		public void TestPaymentMethod1ST_Case2()
		{
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.TaxFirst.Code);

			var invoice = CreateARInvoice();

			var matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 20;
			matchLink.AP_MatchDate = ZDateTime.BrettsBirthday;
			matchLink.AP_MatchGroupNum = "M123";
			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(4, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 64.72m, 8.00m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 129.45m, 16.00m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, 100.00m, 0m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 4, -32.36m, -4.00m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 35;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M111";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(3, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(7);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 35.28m, 4.36m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 70.55m, 8.72m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 4, -17.64m, -2.18m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 260;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M222";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(7);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 415.9;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M333";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(7);

			AssertInvoiceFullyMatched(invoice);
		}

		public void TestPaymentMethod1ST_WithZeroTotalTax()
		{
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.TaxFirst.Code);

			var chargeCode = TestObjectCreator.CC1;
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", TestObjectCreator.USD, 2M, TestObjectCreator.LocalClient);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, 200M, 0M, 0, chargeCode.PK);
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", 100M, line.AL_LineAmount);
			AssertEquals("Precondition", 0M, line.AL_GSTVAT);

			line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, 100M, 0M, 0, chargeCode.PK);
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", 50M, line.AL_LineAmount);
			AssertEquals("Precondition", 0M, line.AL_GSTVAT);

			var matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 20.77;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M123";
			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(2);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, 100m, 0m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 50m, 0m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 129.23;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M111";
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(2);

			AssertInvoiceFullyMatched(invoice);
		}

		public void TestPaymentMethod1ST_WithZeroTotalTax_WithMatchingByLines()
		{
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.TaxFirst.Code);

			var chargeCode = TestObjectCreator.CC1;
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", TestObjectCreator.USD, 2M, TestObjectCreator.LocalClient);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, 200M, 0M, 0, chargeCode.PK);
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", 100M, line.AL_LineAmount);
			AssertEquals("Precondition", 0M, line.AL_GSTVAT);

			line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, 100M, 0M, 0, chargeCode.PK);
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", 50M, line.AL_LineAmount);
			AssertEquals("Precondition", 0M, line.AL_GSTVAT);

			var matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 20.77;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M123";
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[0], 10.55m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[1], 10.22m);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(2);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, 100m, 0m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 50m, 0m);

			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 129.23;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M111";
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[0], 89.45m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[1], 39.78m);
			AssertMatchLinkEqualsLineMatchLinksTotal(matchLink);
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(2);

			AssertInvoiceFullyMatched(invoice);
		}

		public void TestCreateRecordsForTransactionFullyPaidWithoutMatchingAndReversed()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 100, 12);
			TestObjectCreator.CreateCashVATLines(invoice, -100, -12);
			invoice.AH_FullyPaidDate = ZDateTime.Today.AddDays(-5);
			var cashVATRecords = CashBasisVATManager.CreateRecordsForTransactionFullyPaidWithoutMatching(invoice);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(2);
			AssertCashVATRecord(cashVATRecords, invoice, null, 0, 100m, 12m);
			AssertCashVATRecord(cashVATRecords, invoice, null, 1, -100m, -12m);

			AssertInvoiceFullyMatched(invoice, withMatchLinks: false);

			var creditNote = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARCreditNote), 100, 12);
			TestObjectCreator.CreateCashVATLines(creditNote, -100, -12);
			var matchLinkInvoice = Factory.New<TransactionMatchLink>();
			matchLinkInvoice.AP_AH = invoice.PK;
			matchLinkInvoice.AP_Amount = 0;
			matchLinkInvoice.AP_MatchDate = ZDateTime.BrettsBirthday;
			matchLinkInvoice.AP_MatchGroupNum = "M123";
			var matchLinkCreditNote = Factory.New<TransactionMatchLink>();
			matchLinkCreditNote.AP_AH = creditNote.PK;
			matchLinkCreditNote.AP_Amount = 0;
			matchLinkCreditNote.AP_MatchDate = ZDateTime.BrettsBirthday;
			matchLinkCreditNote.AP_MatchGroupNum = "M123";

			cashVATRecords = CashBasisVATManager.CreateRecords(matchLinkInvoice);
			AssertEquals(4, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(6);
			AssertCashVATRecord(cashVATRecords, invoice, matchLinkInvoice, 0, -100, -12, useMatchLinkDateOnly: true);
			AssertCashVATRecord(cashVATRecords, invoice, matchLinkInvoice, 1, 100, 12, useMatchLinkDateOnly: true);
			AssertCashVATRecord(cashVATRecords, invoice, matchLinkInvoice, 0, 100, 12);
			AssertCashVATRecord(cashVATRecords, invoice, matchLinkInvoice, 1, -100, -12);

			cashVATRecords = CashBasisVATManager.CreateRecords(matchLinkCreditNote);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(8);
			AssertCashVATRecord(cashVATRecords, creditNote, matchLinkCreditNote, 0, -100, -12);
			AssertCashVATRecord(cashVATRecords, creditNote, matchLinkCreditNote, 1, 100, 12);

			AssertInvoiceFullyMatched(invoice);
			AssertInvoiceFullyMatched(creditNote);
		}

		public void TestCreateRecordsForTransactionFullyPaidWithoutMatchingAndReversed_WhenOtherTaxesAmountNonZero()
		{
			var creditNote = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARCreditNote), 100, 12);
			TestObjectCreator.CreateCashVATLines(creditNote, -100, -12);
			creditNote.AH_LocalTaxAmountOtherTaxes = 10m;
			AssertEquals(0m, creditNote.AH_InvoiceAmount + creditNote.AH_GSTAmount);
			AssertNotEquals(0m, creditNote.AH_LocalTotal);

			var matchLinkCreditNote = Factory.New<TransactionMatchLink>();
			matchLinkCreditNote.AP_AH = creditNote.PK;
			matchLinkCreditNote.AP_Amount = 0;
			matchLinkCreditNote.AP_MatchDate = ZDateTime.BrettsBirthday;
			matchLinkCreditNote.AP_MatchGroupNum = "M123";

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLinkCreditNote);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(2);
			AssertCashVATRecord(cashVATRecords, creditNote, matchLinkCreditNote, 0, -100, -12);
			AssertCashVATRecord(cashVATRecords, creditNote, matchLinkCreditNote, 1, 100, 12);

			AssertInvoiceFullyMatched(creditNote);
		}

		public void TerstReverseRecords()
		{
			var record1 = Factory.New<AccCashBasisVAT>();
			record1.YC_MatchGroupNum = "M2";
			record1.YC_AL_TransactionLine = ZGuid.NewZGuid();
			record1.YC_PostDate = ZDateTime.Now;
			record1.YC_TaxBaseAmount = 10;
			record1.YC_TaxAmount = 2;
			var record1_anotheCountry = Factory.New<AccCashBasisVAT>();
			record1_anotheCountry.YC_MatchGroupNum = "M2";
			record1_anotheCountry.YC_AL_TransactionLine = ZGuid.NewZGuid();
			record1_anotheCountry.YC_PostDate = ZDateTime.Now;
			record1_anotheCountry.YC_TaxBaseAmount = 10;
			record1_anotheCountry.YC_TaxAmount = 2;
			record1_anotheCountry.YC_GC = ZGuid.NewZGuid();
			var record2 = Factory.New<AccCashBasisVAT>();
			record2.YC_MatchGroupNum = "M7";
			record2.YC_AL_TransactionLine = ZGuid.NewZGuid();
			record2.YC_PostDate = ZDateTime.Now;
			record2.YC_TaxBaseAmount = 20;
			record2.YC_TaxAmount = 3;
			var record3 = Factory.New<AccCashBasisVAT>();
			record3.YC_MatchGroupNum = "M14";
			record3.YC_AL_TransactionLine = ZGuid.NewZGuid();
			record3.YC_PostDate = ZDateTime.Now;
			record3.YC_TaxBaseAmount = 20;
			record3.YC_TaxAmount = 3;

			var collection = new TransactionMatchLinkCollection(Factory);
			var matchLink = collection.AddNew();
			matchLink.AP_MatchGroupNum = "M2";
			matchLink = collection.AddNew();
			matchLink.AP_MatchGroupNum = "M7";

			Action<AccCashBasisVAT, IEnumerable<AccCashBasisVAT>> assertReversedRecord = (originalRecord, reversedRecordSet) =>
			{
				AssertEquals("Only one reversed record must be created.", 1, reversedRecordSet.Count());
				var reversedRecord = reversedRecordSet.First();
				AssertEquals("YC_MatchGroupNum", originalRecord.YC_MatchGroupNum, reversedRecord.YC_MatchGroupNum);
				AssertEquals("YC_AL_TransactionLine", originalRecord.YC_AL_TransactionLine, reversedRecord.YC_AL_TransactionLine);
				AssertEquals("YC_PostDate", originalRecord.YC_PostDate, reversedRecord.YC_PostDate);
				AssertEquals("YC_TaxBaseAmount", originalRecord.YC_TaxBaseAmount, reversedRecord.YC_TaxBaseAmount);
				AssertEquals("YC_TaxAmount", originalRecord.YC_TaxAmount, reversedRecord.YC_TaxAmount);
				AssertEquals("YC_GC", originalRecord.YC_GC, reversedRecord.YC_GC);
			};

			AssertCashVATRecordsCountInFactory(4);

			var unmatchDate = ZDateTime.Today.AddDays(-10);
			var reversedRecords = CashBasisVATManager.ReverseRecords(collection, unmatchDate);
			AssertEquals("Reversed records", 2, reversedRecords.Length);
			assertReversedRecord(record1, reversedRecords.Where(item => item.YC_MatchGroupNum == "M2"));
			assertReversedRecord(record2, reversedRecords.Where(item => item.YC_MatchGroupNum == "M7"));

			AssertCashVATRecordsCountInFactory(6);
		}

		public void TestCreateRecordsForCreditNote()
		{
			var creditNote = CreateTransaction(typeof(ARCreditNote));
			var matchLinkCreditNote = Factory.New<TransactionMatchLink>();
			matchLinkCreditNote.AP_AH = creditNote.PK;
			matchLinkCreditNote.AP_Amount = -100;
			matchLinkCreditNote.AP_MatchDate = ZDateTime.BrettsBirthday;
			matchLinkCreditNote.AP_MatchGroupNum = "M123";

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLinkCreditNote);
			AssertEquals(4, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);
			AssertCashVATRecord(cashVATRecords, creditNote, matchLinkCreditNote, 1, -26.25m, -3.24m);
			AssertCashVATRecord(cashVATRecords, creditNote, matchLinkCreditNote, 2, -52.51m, -6.49m);
			AssertCashVATRecord(cashVATRecords, creditNote, matchLinkCreditNote, 3, -26.25m, 0m);
			AssertCashVATRecord(cashVATRecords, creditNote, matchLinkCreditNote, 4, 13.13m, 1.62m);

			matchLinkCreditNote = Factory.New<TransactionMatchLink>();
			matchLinkCreditNote.AP_AH = creditNote.PK;
			matchLinkCreditNote.AP_Amount = -630.90;
			matchLinkCreditNote.AP_MatchDate = ZDateTime.Today;
			matchLinkCreditNote.AP_MatchGroupNum = "M321";

			cashVATRecords = CashBasisVATManager.CreateRecords(matchLinkCreditNote);
			AssertEquals(4, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(8);
			AssertCashVATRecord(cashVATRecords, creditNote, matchLinkCreditNote, 1, -73.75m, -9.12m);
			AssertCashVATRecord(cashVATRecords, creditNote, matchLinkCreditNote, 2, -147.49m, -18.23m);
			AssertCashVATRecord(cashVATRecords, creditNote, matchLinkCreditNote, 3, -73.75m, 0m);
			AssertCashVATRecord(cashVATRecords, creditNote, matchLinkCreditNote, 4, 36.87m, 4.56m);

			AssertInvoiceFullyMatched(creditNote);
		}

		public void TestCreateRecordsForCreditNoteWhenPaidAmountAndPendingAmountWithDifferentSigns()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARCreditNote), 200, 38);
			invoice.Lines[0].AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code;
			TestObjectCreator.CreateCashVATLines(invoice, -5, -3);

			var matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = -230;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "M123";

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(1, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(1);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 5, 3);

			AssertInvoiceFullyMatched(invoice);

			cashVATRecords.ToList().ForEach(item => item.Delete());
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.TaxFirst.Code);

			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(1, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(1);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 5, 3);

			AssertInvoiceFullyMatched(invoice);
		}

		public void TestCreateRecordsForARInvoiceWhenTaxAmountCanBeRecognizedInFullButTaxBaseAmountIsNot()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 101.10m, 16.18m);
			TestObjectCreator.CreateCashVATLines(invoice, 102.10m, 16.34m);
			TestObjectCreator.CreateCashVATLines(invoice, 103.10m, 16.49m);
			TestObjectCreator.CreateCashVATLines(invoice, 66.66m, 7.33m).AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code;

			var matchDate = ZDateTime.Today.AddDays(-10);
			var matchLink = TestObjectCreator.CreateMatchLink(invoice, 30, matchDate.AddDays(1), "M1");

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(3, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(3);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, 8.54m, 1.37m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 8.62m, 1.38m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 8.71m, 1.39m);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, 324.00m, matchDate.AddDays(2), "M2");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(3, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(6);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, 92.19m, 14.75m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 93.11m, 14.90m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 94.01m, 15.04m);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, 1.21m, matchDate.AddDays(3), "M3");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(3, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(9);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, 0.37m, 0.06m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 0.37m, 0.06m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 0.38m, 0.06m);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, 0.10m, matchDate.AddDays(4), "M4");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, 73.99m, matchDate.AddDays(5), "M5");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);

			AssertInvoiceFullyMatched(invoice);
		}

		public void TestCreateRecordsForARCreditNoteWhenTaxAmountCanBeRecognizedInFullButTaxBaseAmountIsNot()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARCreditNote), 101.10m, 16.18m);
			TestObjectCreator.CreateCashVATLines(invoice, 102.10m, 16.34m);
			TestObjectCreator.CreateCashVATLines(invoice, 103.10m, 16.49m);
			TestObjectCreator.CreateCashVATLines(invoice, 66.66m, 7.33m).AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code;

			var matchDate = ZDateTime.Today.AddDays(-10);
			var matchLink = TestObjectCreator.CreateMatchLink(invoice, -30, matchDate.AddDays(1), "M1");

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(3, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(3);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, -8.54m, -1.37m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, -8.62m, -1.38m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, -8.71m, -1.39m);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, -324.00m, matchDate.AddDays(2), "M2");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(3, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(6);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, -92.19m, -14.75m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, -93.11m, -14.90m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, -94.01m, -15.04m);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, -1.21m, matchDate.AddDays(3), "M3");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(3, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(9);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, -0.37m, -0.06m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, -0.37m, -0.06m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, -0.38m, -0.06m);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, -0.10m, matchDate.AddDays(4), "M4");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, -73.99m, matchDate.AddDays(5), "M5");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(0, cashVATRecords.Length);

			AssertInvoiceFullyMatched(invoice);
		}

		public void TestCreateRecordsForARInvoiceWhenNonCashLineHasNegativeAmount()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 1000m, 200m);
			TestObjectCreator.CreateCashVATLines(invoice, -400m, 0).AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code;

			invoice.AH_FullyPaidDate = ZDateTime.Today;
			invoice.AH_OutstandingAmount = 0;
			var matchLink = TestObjectCreator.CreateMatchLink(invoice, 800, ZDateTime.Today, "M1");

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(1, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(1);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, 1000m, 200m);
		}

		public void TestCreateRecordsForARInvoiceWhenMatchAmountVerySmall_PRP()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 1.00m, 0.05m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.05m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.05m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.05m);

			var matchDate = ZDateTime.Today.AddDays(-10);
			var matchLink = TestObjectCreator.CreateMatchLink(invoice, 0.01m, matchDate.AddDays(1), "M1");

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(1, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(1);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, 0.01m, 0.01m);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, 0.03m, matchDate.AddDays(2), "M2");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(3);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, 0.01m, 0.01m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 0.01m, 0.01m);
		}

		public void TestCreateRecordsForARInvoiceWhenMatchAmountVerySmall_PRP_ByLines()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 1.00m, 0.05m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.05m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.05m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.05m);

			var matchDate = ZDateTime.Today.AddDays(-10);
			var matchLink = TestObjectCreator.CreateMatchLink(invoice, 0.03m, matchDate.AddDays(1), "M1");
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[0], 0.01m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[3], 0.02m);

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(2);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, 0.01m, 0.01m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, 0.02m, 0.01m);
		}

		public void TestCreateRecordsForARCreditNoteWhenMatchAmountVerySmall_PRP_ByLines()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARCreditNote), 1.00m, 0.05m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.05m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.05m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.05m);

			var matchDate = ZDateTime.Today.AddDays(-10);
			var matchLink = TestObjectCreator.CreateMatchLink(invoice, -0.03m, matchDate.AddDays(1), "M1");
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[0], -0.01m);
			TestObjectCreator.CreateLineMatchLink(matchLink, invoice.Lines[3], -0.02m);

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(2);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, -0.01m, -0.01m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, -0.02m, -0.01m);
		}

		public void TestCreateRecordsForARCreditNoteWhenMatchAmountVerySmall_PRP()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARCreditNote), 1.00m, 0.05m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.05m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.05m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.05m);

			var matchDate = ZDateTime.Today.AddDays(-10);
			var matchLink = TestObjectCreator.CreateMatchLink(invoice, -0.01m, matchDate.AddDays(1), "M1");

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(1, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(1);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, -0.01m, -0.01m);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, -0.03m, matchDate.AddDays(2), "M2");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(2, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(3);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, -0.01m, -0.01m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, -0.01m, -0.01m);
		}

		public void TestCreateRecordsForARInvoiceWhenMatchAmountVerySmall_1ST()
		{
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.TaxFirst.Code);

			var invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 100.00m, 1m);
			TestObjectCreator.CreateCashVATLines(invoice, 100.00m, 1m);
			TestObjectCreator.CreateCashVATLines(invoice, 100.00m, 1m);
			TestObjectCreator.CreateCashVATLines(invoice, 100.00m, 1m);

			var matchDate = ZDateTime.Today.AddDays(-10);
			var matchLink = TestObjectCreator.CreateMatchLink(invoice, 0.01m, matchDate.AddDays(1), "M1");

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(1, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(1);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, 0.25m, 0.01m);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, 0.02m, matchDate.AddDays(2), "M2");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(3, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, 0.50m, 0.01m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, 0.50m, 0.01m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, 0.50m, 0.01m);
		}

		public void TestCreateRecordsForARInvoiceWhenMatchAmountVerySmall_1ST2()
		{
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.TaxFirst.Code);

			var invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 100.00m, 1m);
			TestObjectCreator.CreateCashVATLines(invoice, 100.00m, 1m);
			TestObjectCreator.CreateCashVATLines(invoice, 100.00m, 1m);
			TestObjectCreator.CreateCashVATLines(invoice, 1000.00m, 100m);

			var matchDate = ZDateTime.Today.AddDays(-10);
			var matchLink = TestObjectCreator.CreateMatchLink(invoice, 0.01m, matchDate.AddDays(1), "M1");

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(1, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(1);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, 0.1m, 0.01m);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, 0.02m, matchDate.AddDays(2), "M2");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(1, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(2);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, 0.19m, 0.02m);
		}

		public void TestCreateRecordsForARCreditNoteWhenMatchAmountVerySmall_1ST()
		{
			AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.PartPaymentTaxRealizationRuleTypes.TaxFirst.Code);

			var invoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARCreditNote), 1.00m, 0.03m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.03m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.03m);
			TestObjectCreator.CreateCashVATLines(invoice, 1.00m, 0.03m);

			var matchDate = ZDateTime.Today.AddDays(-10);
			var matchLink = TestObjectCreator.CreateMatchLink(invoice, -0.01m, matchDate.AddDays(1), "M1");

			var cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(1, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(1);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 0, -0.08m, -0.01m);

			matchLink = TestObjectCreator.CreateMatchLink(invoice, -0.02m, matchDate.AddDays(2), "M2");
			cashVATRecords = CashBasisVATManager.CreateRecords(matchLink);
			AssertEquals(3, cashVATRecords.Length);
			AssertCashVATRecordsCountInFactory(4);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 1, -0.18m, -0.01m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 2, -0.18m, -0.01m);
			AssertCashVATRecord(cashVATRecords, invoice, matchLink, 3, -0.18m, -0.01m);
		}

		InvoicingBase CreateARInvoice()
		{
			return CreateTransaction(typeof(ARInvoice));
		}

		InvoicingBase CreateTransaction(Type invoiceType)
		{
			var chargeCode = TestObjectCreator.CC1;
			var transaction = TestObjectCreator.CreateInvoice(invoiceType, "INV1", TestObjectCreator.USD, 2M, TestObjectCreator.LocalClient);
			var line = TestObjectCreator.CreateInvoiceLine(transaction, TestObjectCreator.USD, 2M, 100M, 0M, 0, chargeCode.PK);
			line.AL_AT = ZGuid.Empty;

			int multiplier = Math.Sign(transaction.AH_InvoiceAmount);

			line = TestObjectCreator.CreateInvoiceLine(transaction, TestObjectCreator.USD, 2M, 200M, 24.72M, 0, chargeCode.PK);
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", multiplier * 100M, line.AL_LineAmount);
			AssertEquals("Precondition", multiplier * 12.36M, line.AL_GSTVAT);

			line = TestObjectCreator.CreateInvoiceLine(transaction, TestObjectCreator.USD, 2M, 400M, 49.44M, 0, chargeCode.PK);
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", multiplier * 200M, line.AL_LineAmount);
			AssertEquals("Precondition", multiplier * 24.72M, line.AL_GSTVAT);

			line = TestObjectCreator.CreateInvoiceLine(transaction, TestObjectCreator.USD, 2M, 200M, 0M, 0, chargeCode.PK);
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", multiplier * 100M, line.AL_LineAmount);
			AssertEquals("Precondition", multiplier * 0M, line.AL_GSTVAT);

			line = TestObjectCreator.CreateInvoiceLine(transaction, TestObjectCreator.USD, 2M, -100M, -12.36M, 0, chargeCode.PK);
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", multiplier * -50M, line.AL_LineAmount);
			AssertEquals("Precondition", multiplier * -6.18M, line.AL_GSTVAT);

			line = TestObjectCreator.CreateInvoiceLine(transaction, TestObjectCreator.USD, 2M, 600M, 0M, 0, chargeCode.PK);
			AssertNotNull("Precondition", line.TaxRate);
			AssertEquals("Precondition", multiplier * 300M, line.AL_LineAmount);
			AssertEquals("Precondition", multiplier * 0M, line.AL_GSTVAT);

			return transaction;
		}

		void AssertCashVATRecord(AccCashBasisVAT[] cashVATRecords, InvoicingBase invoice, TransactionMatchLink matchLink, int lineNumber, decimal taxBaseAmount, decimal taxAmount, bool useMatchLinkDateOnly = false)
		{
			var expectedMatchGroupNum = matchLink == null || useMatchLinkDateOnly ? ZString.Empty : matchLink.AP_MatchGroupNum;
			var cashVATRecordsForLine = cashVATRecords.Where(item => item.YC_AL_TransactionLine == invoice.Lines[lineNumber].PK && item.YC_MatchGroupNum == expectedMatchGroupNum).ToArray();
			AssertEquals("One Cash Basis VAT record must be created.", 1, cashVATRecordsForLine.Length);
			var cashVATRecord = cashVATRecordsForLine[0];
			AssertNotNull("Cash Basis VAT record must be created.", cashVATRecord);
			AssertEquals("YC_TaxBaseAmount", taxBaseAmount, cashVATRecord.YC_TaxBaseAmount);
			AssertEquals("YC_TaxAmount", taxAmount, cashVATRecord.YC_TaxAmount);
			if (matchLink == null)
			{
				Assert("Invoice must be fully paid if there is no matchlink.", !invoice.AH_FullyPaidDate.IsEmpty);
			}
			var expectedPostDate = matchLink == null ? invoice.AH_FullyPaidDate : matchLink.AP_MatchDate;
			AssertEquals("YC_MatchGroupNum", expectedPostDate, cashVATRecord.YC_PostDate);
		}

		void AssertInvoiceFullyMatched(InvoicingBase transaction, bool withMatchLinks = true)
		{
			if (withMatchLinks)
			{
				var matchLinks = Factory.Load<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, transaction.PK));
				AssertEquals("Postcondition: invoice total should be equal its matchlinks total", transaction.AH_InvoiceAmount + transaction.AH_GSTAmount, matchLinks.Sum(item => item.AP_Amount));
			}

			foreach (InvoicingLineBase line in transaction.Lines)
			{
				var cashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_AL_TransactionLine, line.PK));
				if (line.AL_GSTVATBasis == AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code)
				{
					AssertEquals("Postcondition: Cash VAT Tax Base total amount must be equal AL_LineAmount", line.AL_LineAmount, cashVATs.Sum(item => item.YC_TaxBaseAmount));
					AssertEquals("Postcondition: Cash VAT Tax total amount must be equal AL_GSTVAT", line.AL_GSTVAT, cashVATs.Sum(item => item.YC_TaxAmount));
				}
				else
				{
					Assert("Cash VAT must not be created for lines with Accrual basis tax.", !cashVATs.Any());
				}
			}
		}

		void AssertMatchLinkEqualsLineMatchLinksTotal(TransactionMatchLink matchLink)
		{
			var lineMatchLinks = Factory.Load<AccTransLinePay>(new ZQuery(AccTransLinePaySchema.A7_AP, matchLink.PK));
			AssertEquals("Precondition: match link amount should be equal its line matchlinks total", matchLink.AP_Amount, lineMatchLinks.Sum(item => item.A7_Amount));
		}

		void AssertCashVATRecordsCountInFactory(int count)
		{
			var records = Factory.Load<AccCashBasisVAT>(new ZQuery());
			AssertEquals("Exect amount of Cash Basis VAT records created in a Factory.", count, records.Length);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
