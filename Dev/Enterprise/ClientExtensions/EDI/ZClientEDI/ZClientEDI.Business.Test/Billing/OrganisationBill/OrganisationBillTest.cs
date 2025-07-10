using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Client.EDI.Billing.Hosting;
using Enterprise.Client.EDI.Billing.ODPL;
using Enterprise.Client.EDI.Billing.ODPL.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(OrganisationBill))]
	sealed class OrganisationBillTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrganisationRelated()
		{
			AssertEquals(organisation, organisationBill.Organisation);
			AssertEquals(organisation.LicCompany, organisationBill.LicCompany);
		}

		public void TestOrganisationInformation()
		{
			AssertEquals(organisation.OH_Code, organisationBill.OrganisationCode);
			AssertEquals(organisation.OH_FullName, organisationBill.OrganisationName);
			AssertEquals(organisation.OH_RL_NKClosestPort, organisationBill.OrganisationUNLOCO);
			AssertEquals(organisation.LicEnterprise.LE_EnterpriseCode, organisationBill.EnterpriseCode);
			AssertEquals("Invoice currency", "AUD", organisationBill.InvoiceCurrencyCode);
		}

		public void TestAmountProperties()
		{
			organisationBill.CalculateAll(0);
			AssertEquals("Amount: sum of all system bills amounts", 150m, organisationBill.Amount);
			AssertEquals("Discount: sum of all system bills discounts", 15m, organisationBill.DiscountAmount);
			AssertEquals("Surcharge: sum of all system bills surcharges", 5m, organisationBill.SurchargeAmount);
			AssertEquals("Total: sum of all system bills total amounts", 140m, organisationBill.TotalAmount);

			organisation.LicCompany.SelfBilling.L4_ProcessingFee = "MPF";
			organisation.LicCompany.SelfBilling.L4_ProcessingFeePercent = 5.0m;
			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(40m, "AUD");
			organisationBill.CalculateAll(0);
			AssertEquals("Processing Fee: applied to (Total Amount - Deposit Deducted)", 5m, organisationBill.ProcessingFeeAmount);
			AssertEquals("Total due (Total Amount - Deposit Deducted + ProcessingFee)", 105m, organisationBill.TotalDue);

			organisation.LicCompany.SelfBilling.L4_ProcessingFee = "DDE";
			organisation.LicCompany.SelfBilling.L4_ProcessingFeePercent = 3.0m;
			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(40m, "AUD");
			organisationBill.CalculateAll(0);
			AssertEquals("Processing Fee", -140m * 0.03m, organisationBill.ProcessingFeeAmount);
			AssertEquals("Total due", 100m - 140m * 0.03m, organisationBill.TotalDue);

			organisation.LicCompany.SelfBilling.L4_ProcessingFee = "NON";
			organisation.LicCompany.SelfBilling.L4_ProcessingFeePercent = 0.0m;
			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(40m, "AUD");
			organisationBill.CalculateAll(0);
			AssertEquals("Processing Fee", 0m, organisationBill.ProcessingFeeAmount);
			AssertEquals("Total due", 100m, organisationBill.TotalDue);
		}

		public void TestAmountProperties_WithCurrencyConversion()
		{
			ZDecimal exchangeRate1 = 0.8m;
			ZDecimal exchangeRate2 = 2m;
			ZDecimal exchangeRate3 = 10m;

			BillingTestHelper.CreateExchangeRate(Factory, "USD", exchangeRate1);
			organisation.LicCompany.PriceHeaders[0].L6_RX_NKCurrency = "USD";
			Factory.Save();

			var systemBill1 = new OdplSystemBill(Factory);
			var odplUsage1 = new OdplUsage(Factory, user, new ZDateTime(2010, 10, 01));
			OdplUsageTest.AddModuleUsage(odplUsage1, "COR", 100, 1);
			systemBill1.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1 });

			var systemBill2 = new DummySystemBill(organisation, "AAA", 200m, 20m);
			SetCurrencyAndExchangeRate(systemBill2, "NZD", exchangeRate2);

			var systemBill3 = new DummySystemBill(organisation, "BBB", 300m, 30m);
			SetCurrencyAndExchangeRate(systemBill3, "MDL", exchangeRate3);

			organisationBill = NewBill();
			organisationBill.DateTo = systemBill1.PeriodStart;

			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
			organisationBill.AddSystemBill(systemBill3);

			organisation.LicCompany.SelfBilling.L4_ProcessingFee = "MPF";
			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(40m, "USD");

			organisationBill.CalculateAll(0);

			var expectedAmount = (100m / exchangeRate1) + (200m / exchangeRate2) + (300m / exchangeRate3);
			AssertEquals("Amount", expectedAmount, organisationBill.Amount);

			var expectedDiscount = (10m / exchangeRate1) + (20m / exchangeRate2) + (30m / exchangeRate3);
			AssertEquals("DiscountAmount", expectedDiscount, organisationBill.DiscountAmount);

			var expectedSurcharge = 5m / exchangeRate1;
			AssertEquals("SurchargeAmount", expectedSurcharge, organisationBill.SurchargeAmount);

			var expectedTotalAmount = expectedAmount - expectedDiscount + expectedSurcharge;
			AssertEquals("TotalAmount", expectedTotalAmount, organisationBill.TotalAmount);

			AssertEquals("DepositDeducted", 40m, organisationBill.DepositDeducted);

			var expectedDepositDeductedInInvoiceCurrency = 40m / exchangeRate1;
			AssertEquals("DepositDeductedInInvoiceCurrency", expectedDepositDeductedInInvoiceCurrency, organisationBill.DepositDeductedInInvoiceCurrency);

			var expectedProcessingFeeAmount = organisationBill.CalculateProcessingFeeAmount_Exposed(expectedTotalAmount - expectedDepositDeductedInInvoiceCurrency);
			AssertEquals("ProcessingFeeAmount", expectedProcessingFeeAmount, organisationBill.ProcessingFeeAmount);

			var expectedTotalDue = expectedTotalAmount - expectedDepositDeductedInInvoiceCurrency + expectedProcessingFeeAmount;
			AssertEquals("TotalDue", expectedTotalDue, organisationBill.TotalDue);
		}

		public void TestDepositProperties()
		{
			AssertEquals("Precondition", 0m, organisationBill.Deposit);
			AssertEquals("Precondition", 0m, organisationBill.DepositDeducted);

			organisationBill.CalculateAll(0);
			AssertEquals("No deposit", 0m, organisationBill.Deposit);
			AssertEquals("No deposit deducted", 0m, organisationBill.DepositDeducted);

			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(32m, "AUD");
			organisationBill.CalculateAll(0);
			AssertEquals("Deposit", 32m, organisationBill.Deposit);
			AssertEquals("Deposit deducted: minimum of deposit & total amount", 32m, organisationBill.DepositDeducted);

			organisationBill.LicCompany.SetMonthlyUsageDepositBalanceForTest(-500m, "AUD");
			organisationBill.CalculateAll(0);
			AssertEquals("Negative deposit balance treated as zero", 0m, organisationBill.Deposit);
			AssertEquals("Deposit deducted: minimum of deposit & total amount", 0m, organisationBill.DepositDeducted);

			organisationBill.LicCompany.SetMonthlyUsageDepositBalanceForTest(500m, "AUD");
			organisationBill.CalculateAll(0);
			AssertEquals("Deposit", 500m, organisationBill.Deposit);
			AssertEquals("Deposit deducted: minimum of deposit & total amount", 140m, organisationBill.DepositDeducted);

			var systemBill1 = new DummySystemBill(organisation, "AAA", 100m, 10m);
			var systemBill2 = new DummySystemBill(organisation, "BBB", 200m, 20m);

			organisationBill.SystemBills.RemoveAndDeleteAll();
			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
			organisationBill.CalculateAll(0);
			AssertEquals("Deposit", 500m, organisationBill.Deposit);
			AssertEquals("deposit still deducted if no odpl system bill amount", 270m, organisationBill.DepositDeducted);

			var odplBill = new DummySystemBill(organisation, BillingConstants.BillingSystem.ODM, 300m, 30m);
			organisationBill.AddSystemBill(odplBill);
			organisationBill.CalculateAll(0);
			AssertEquals("Deposit deducted: minimum of deposit & total amount", 500m, organisationBill.DepositDeducted);

			AssertEquals("Deposit currency", "AUD", organisationBill.DepositCurrencyCode);
			((DummyUsage)odplBill.SystemUsages[0]).CurrencyCode_Exposed = "XXX";
			organisationBill.CalculateAll(0);
			AssertEquals("Deposit currency", "AUD", organisationBill.DepositCurrencyCode);
		}

		public void TestAmountProperties_WithDepositProcessingFeeDiscountAndExemptions()
		{
			organisation.LicCompany.SelfBilling.L4_ProcessingFeePercent = 10;
			organisation.LicCompany.SelfBilling.L4_ProcessingFee = "DDE";
			organisationBill.LicCompany.SetMonthlyUsageDepositBalanceForTest(500m, "AUD");
			var systemBill1 = new DummySystemBill(organisation, "RSH", 50m, 0m, 0m);
			var systemBill2 = new DummySystemBill(organisation, "ZZ1", 10m, 0m, 5m);
			organisationBill = NewBill();
			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
			organisationBill.CalculateAll(0);

			var systemBillsReverseOrder = NewBill();
			systemBillsReverseOrder.AddSystemBill(systemBill2);
			systemBillsReverseOrder.AddSystemBill(systemBill1);
			systemBillsReverseOrder.CalculateAll(0);

			AssertEquals("TotalDue", 0m, organisationBill.TotalDue);
			AssertEquals("RSH is exempt from ProcessingFeeAmount", 5m * -0.1m, organisationBill.ProcessingFeeAmount);
			AssertEquals("DepositDeducted", 60m + (5m * -0.1m), organisationBill.DepositDeducted);

			AssertEquals("TotalDue", 0m, systemBillsReverseOrder.TotalDue);
			AssertEquals("RSH is exempt from ProcessingFeeAmount", 5m * -0.1m, systemBillsReverseOrder.ProcessingFeeAmount);
			AssertEquals("DepositDeducted", 60m + (5m * -0.1m), systemBillsReverseOrder.DepositDeducted);
		}

		public void TestDocumentSupporter()
		{
			AssertEquals(typeof(OrganisationBillDocumentSupporter), NewBill().DocumentSupporter.GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_SystemBillAmounts()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			CreateTestBill();

			foreach (DummySystemBill systemBill in organisationBill.SystemBills)
			{
				AssertEquals("Precondition", false, systemBill.MethodWasCalled("CreateInvoiceLines"));
			}

			organisationBill.CreateInvoice(ZDateTime.Empty);
			foreach (DummySystemBill systemBill in organisationBill.SystemBills)
			{
				AssertEquals("Method was called", true, systemBill.MethodWasCalled("CreateInvoiceLines"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_OnInvoiceFactorySavingCore()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			CreateTestBill();

			foreach (DummySystemBill systemBill in organisationBill.SystemBills)
			{
				AssertEquals("Precondition", false, systemBill.MethodWasCalled("OnInvoiceFactorySavingCore"));
			}

			organisationBill.CreateInvoice(ZDateTime.Empty);
			foreach (DummySystemBill systemBill in organisationBill.SystemBills)
			{
				AssertEquals("Method was called", true, systemBill.MethodWasCalled("OnInvoiceFactorySavingCore"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_NewFactory()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			CreateTestBill();

			var invoice1FactoryRef = CreateInvoice(out var invoice1factoryInstance);
			var invoice2 = organisationBill.CreateInvoice(ZDateTime.Empty);
			AssertNotEquals("new factory per invoice", invoice1factoryInstance, invoice2.Factory._Instance);

			GC.Collect();
			Assert("invoice factory was collected - no references are keeping it alive", !invoice1FactoryRef.IsAlive);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_InvoiceTemplate()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var invoice = organisationBill.CreateInvoice(ZDateTime.Empty);

			AssertEquals(organisationBill.OrganisationPK, invoice.AH_OH);
			AssertEquals(InvoiceTypesList.Codes.FinalInvoice, invoice.AH_TransactionCategory);
			AssertEquals(organisationBill.InvoiceCurrencyCode, invoice.AH_RX_NKTransactionCurrency);
			AssertEquals(organisationBill.InvoicingBranch.PK, invoice.AH_GB);
			AssertEquals(organisationBill.InvoicingBranch.GB_GC, invoice.AH_GC);
			AssertEquals(organisationBill.LocalExchangeRate, invoice.AH_ExchangeRate);
		}

		[DeveloperOnlyTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_CommissionCreator()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			CreateTestBill();

			var invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			var commissionCreator = invoice.CommissionCreatorOverride;
			AssertType(typeof(BillingCommissionCreator), commissionCreator);
			AssertType(typeof(OdplBilledUsageCommissionGroupsCalculator), ((BillingCommissionCreator)commissionCreator).UsageGroupsCalculator);
		}

		public void TestProcessingFeeAmount_IsProcessingFeeExempt()
		{
			organisation.LicCompany.SelfBilling.L4_ProcessingFeePercent = 10;
			var systemBill = new DummySystemBill(organisation, "ZZ1", 50m, 5m, 25m);
			organisationBill = NewBill();

			organisationBill.AddSystemBill(systemBill);
			organisationBill.CalculateAll(0);
			AssertEquals("exempt amount not included in fee", (50m - 5m - 25m) * 0.1m, organisationBill.ProcessingFeeAmount);

			systemBill = new DummySystemBill(organisation, "ZZ1", 50m, 5m, 0m);
			organisationBill = NewBill();
			organisationBill.AddSystemBill(systemBill);
			organisationBill.CalculateAll(0);
			AssertEquals("exempt amount zero", (50m - 5m) * 0.1m, organisationBill.ProcessingFeeAmount);

			systemBill = new DummySystemBill(organisation, "ZZ1", 50m, 5m, 45m);
			organisationBill = NewBill();
			organisationBill.AddSystemBill(systemBill);
			organisationBill.CalculateAll(0);
			AssertEquals("fully exempt", 0m, organisationBill.ProcessingFeeAmount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_IsProcessingFeeExempt()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var billing = organisation.LicCompany.SelfBilling;
			billing.L4_ProcessingFee = "DDE";
			billing.L4_ProcessingFeePercent = 8.0m;

			var systemBill = new DummySystemBill(organisation, "ZZ1", 50m, 5m);
			systemBill.DummyLines.Add(new SystemBill.BillLine(70, new SystemBill.TaxGroup(null, null), "AUD", EDIDataRegistry.Instance.OdplUsageChargeCode.Value, "Desc 1", 1, "FEE", Env.CurrentDepartment.PK));
			systemBill.DummyLines.Add(new SystemBill.BillLine(190, new SystemBill.TaxGroup(null, null), "AUD", EDIDataRegistry.Instance.OdplUsageChargeCode.Value, "Desc 2", 2, "FEE", Env.CurrentDepartment.PK));
			systemBill.DummyLines[1].IsProcessingFeeExempt = true;

			organisationBill = NewBill("AUD");
			organisationBill.DateTo = systemBill.PeriodStart;
			organisationBill.AddSystemBill(systemBill);
			organisationBill.CalculateAll(0m);

			var invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			var invoiceLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => !x.AL_Desc.IsEmpty).ToArray();
			AssertInvoiceLine(invoiceLines[1], 70, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, "Desc 1");
			AssertInvoiceLine(invoiceLines[4], (70m + 50m - 5) * -0.08m, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value, "Direct Debit Discount");

			// exempt line after processing line
			AssertInvoiceLine(invoiceLines[5], 190, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, "Desc 2");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvoiceDescriptionIsFromRegistry()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			AssertEquals(EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription.Value + " - " + organisationBill.DateTo.ToString("MMMM yyyy"), invoice.AH_Desc);
			EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test description");
			invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			AssertEquals("test description" + " - " + organisationBill.DateTo.ToString("MMMM yyyy"), invoice.AH_Desc);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_InvoiceLines()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var billing = organisation.LicCompany.SelfBilling;
			billing.L4_ProcessingFee = "MPF";
			billing.L4_ProcessingFeePercent = 5.0m;
			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(20m, "AUD");
			organisationBill.CalculateAll(0);

			AssertEquals("Precondition: amount", 150m, organisationBill.Amount);
			AssertEquals("Precondition: discount", 15m, organisationBill.DiscountAmount);
			AssertEquals("Precondition: surcharge", 5m, organisationBill.SurchargeAmount);
			AssertEquals("Precondition: deposit deducted", 20m, organisationBill.DepositDeducted);
			AssertEquals("Precondition: processing fee", 6m, organisationBill.ProcessingFeeAmount);

			var invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			var invoiceLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => !x.AL_Desc.IsEmpty).ToArray();

			var amountChargeCodeName = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			var discountChargeCodeName = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			var surchargeChargeCodeName = EDIDataRegistry.Instance.OdplSurchargeChargeCode.Value;
			var bill0 = organisationBill.SystemBills[0];
			var bill1 = organisationBill.SystemBills[1];

			var i = 0;
			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription.Value + " - " + organisationBill.DateTo.ToString("MMMM yyyy"));
			AssertInvoiceLine(invoiceLines[i++], bill0.Amount, amountChargeCodeName, "On Demand Usage");
			AssertInvoiceLine(invoiceLines[i++], -bill0.DiscountAmount, discountChargeCodeName, "On Demand Discount");
			AssertInvoiceLine(invoiceLines[i++], bill0.SurchargeAmount, surchargeChargeCodeName, "On Demand Surcharge");

			AssertInvoiceLine(invoiceLines[i++], bill1.Amount, amountChargeCodeName, " Usage");
			AssertInvoiceLine(invoiceLines[i++], -bill1.DiscountAmount, discountChargeCodeName, " Discount");
			AssertInvoiceLine(invoiceLines[i++], organisationBill.ProcessingFeeAmount, EDIDataRegistry.Instance.MonthlyUsageProcessingFeeChargeCode.Value, "Manual Processing Fee");
			AssertInvoiceLine(invoiceLines[i++], -organisationBill.DepositDeductedInInvoiceCurrency, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted");
			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageReportBreakdownComment.Value);

			AssertEquals("Invoice lines: 1 comment, 2 amount lines, 2 discount lines, deposit deducted, surcharge, manual processing fee and comment", 9, invoiceLines.Length);

			var usdExchangeRate = 2m;
			BillingTestHelper.CreateExchangeRate(organisationBill.Factory, "USD", usdExchangeRate);
			Factory.Save();

			billing.L4_ProcessingFee = "DDE";
			billing.L4_ProcessingFeePercent = 3.0m;
			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(0m, "AUD");
			CreateTestBill("USD");
			organisationBill.CalculateAll(0);
			bill0 = organisationBill.SystemBills[0];
			bill1 = organisationBill.SystemBills[1];

			invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			invoiceLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => !x.AL_Desc.IsEmpty).ToArray();
			AssertEquals("Invoice lines: 2 amount lines, 2 discount lines, direct debit, 1 exchange rate comment and common comment", 8, invoiceLines.Length);

			i = 0;
			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription.Value + " - " + organisationBill.DateTo.ToString("MMMM yyyy"));
			AssertInvoiceLine(invoiceLines[i++], bill0.Amount * usdExchangeRate, amountChargeCodeName, "On Demand Usage");
			AssertInvoiceLine(invoiceLines[i++], -bill0.DiscountAmount * usdExchangeRate, discountChargeCodeName, "On Demand Discount");

			AssertInvoiceLine(invoiceLines[i++], bill1.Amount * usdExchangeRate, amountChargeCodeName, " Usage");
			AssertInvoiceLine(invoiceLines[i++], -bill1.DiscountAmount * usdExchangeRate, discountChargeCodeName, " Discount");

			AssertInvoiceLine(invoiceLines[i++], organisationBill.ProcessingFeeAmount, discountChargeCodeName, "Direct Debit Discount");
			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, "Exchange rate used: 1 AUD = 2.00000 USD");
			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageReportBreakdownComment.Value);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_Department()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var departmentPK = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentDepartment.PK)).PK;

			var systemBill = new DummySystemBill(organisation, "ZZ1", 50m, 5m);
			systemBill.DummyLines.Add(new SystemBill.BillLine(7, new SystemBill.TaxGroup(null, null), "AUD", EDIDataRegistry.Instance.OdplUsageChargeCode.Value, "Desc", 1, "ODM", departmentPK));
			organisationBill = NewBill("AUD");
			organisationBill.DateTo = systemBill.PeriodStart;
			organisationBill.AddSystemBill(systemBill);

			var invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			var invoiceLines = invoice.Lines.Cast<ARInvoiceLine>().ToArray();
			var line = invoiceLines.Single(x => x.AL_Desc == "Desc");
			AssertEquals(departmentPK, line.AL_GE);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_SalesTax_PartialDeposit_ProcessingCharge()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var salesTax = BillingTestHelper.CreateChargeCode(Factory, null, "SALESTAX");
			salesTax.AC_Desc = "Sales Tax 9.5%";
			Factory.Save();

			var salesTaxRates = new CodeDescriptionPairList();
			salesTaxRates.AddPair(salesTax.AC_Code, "9.5");
			EDIDataRegistry.Instance.SalesTaxRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, salesTaxRates);

			var billing = organisation.LicCompany.SelfBilling;
			billing.L4_ProcessingFee = "MPF";
			billing.L4_ProcessingFeePercent = 5.0m;
			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(55m, "AUD");

			// usage incurs sales tax
			var onDemandDelivery = organisation.LicCompany.InvoiceDeliveries[0];
			onDemandDelivery.L9_AC_SalesTaxChargeCode = salesTax.PK;
			onDemandDelivery.L9_SystemCode = BillingConstants.BillingSystem.ODM;
			// Dummy usage incurs no sales tax
			var otherDelivery = organisation.LicCompany.InvoiceDeliveries.AddNew();
			otherDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			otherDelivery.L9_RX_NKInvoiceCurrency = "AUD";

			organisationBill.CalculateAll(0);

			AssertEquals("Precondition: amount", 150m, organisationBill.Amount);
			AssertEquals("Precondition: discount", 15m, organisationBill.DiscountAmount);
			AssertEquals("Precondition: surcharge", 5m, organisationBill.SurchargeAmount);
			AssertEquals("Precondition: deposit deducted", 55m, organisationBill.DepositDeducted);
			AssertEquals("Precondition: processing fee", 4.25m, organisationBill.ProcessingFeeAmount);

			var invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			var invoiceLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => !x.AL_Desc.IsEmpty).ToArray();

			var amountChargeCodeName = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			var discountChargeCodeName = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			var odplSurchargeChargeCodeName = EDIDataRegistry.Instance.OdplSurchargeChargeCode.Value;

			var odplBill = organisationBill.SystemBills[0];
			var otherBill = organisationBill.SystemBills[1];
			var odplDepositDeducted = 55m - otherBill.TotalAmount;
			var odplProcessingFee = organisationBill.CalculateProcessingFeeAmount_Exposed(odplBill.TotalAmount - odplDepositDeducted);

			var i = 0;
			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription.Value + " - " + organisationBill.DateTo.ToString("MMMM yyyy"));
			AssertInvoiceLine(invoiceLines[i++], otherBill.Amount, amountChargeCodeName, " Usage");
			AssertInvoiceLine(invoiceLines[i++], -otherBill.DiscountAmount, discountChargeCodeName, " Discount");
			AssertInvoiceLine(invoiceLines[i++], -otherBill.TotalAmount, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted");

			AssertInvoiceLine(invoiceLines[i++], odplBill.Amount, amountChargeCodeName, "On Demand Usage (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], -odplBill.DiscountAmount, discountChargeCodeName, "On Demand Discount (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], odplBill.SurchargeAmount, odplSurchargeChargeCodeName, "On Demand Surcharge (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], odplProcessingFee, EDIDataRegistry.Instance.MonthlyUsageProcessingFeeChargeCode.Value, "Manual Processing Fee (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], -odplDepositDeducted, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], Utilities.Round((odplBill.TotalAmount - odplDepositDeducted + odplProcessingFee) * 0.095m, BillingConstants.RoundingDecimals), salesTax.AC_Code, salesTax.AC_Desc + " (SALESTAX)");

			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageReportBreakdownComment.Value);

			AssertEquals("Invoice lines", 11, invoiceLines.Length);

			// processing is a discount rather than a fee
			billing.L4_ProcessingFee = "DDE";
			CreateTestBillWithOdpl();
			organisationBill.CalculateAll(0);
			AssertEquals("Precondition: amount", 150m, organisationBill.Amount);
			AssertEquals("Precondition: discount", 15m, organisationBill.DiscountAmount);
			AssertEquals("Precondition: surcharge", 5m, organisationBill.SurchargeAmount);
			AssertEquals("Precondition: deposit deducted", 55m, organisationBill.DepositDeducted);
			AssertEquals("Precondition: processing fee", -7m, organisationBill.ProcessingFeeAmount);
			AssertEquals("Precondition: TotalDue", 150m - 15m + 5m - 55m - 7m, organisationBill.TotalDue);
			invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			invoiceLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => !x.AL_Desc.IsEmpty).ToArray();
			odplBill = organisationBill.SystemBills[0];
			otherBill = organisationBill.SystemBills[1];

			var otherDepositDeducted = 0.95m * otherBill.TotalAmount;
			odplDepositDeducted = 55m - otherDepositDeducted;
			odplProcessingFee = organisationBill.CalculateProcessingFeeAmount_Exposed(odplBill.TotalAmount);

			i = 0;
			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription.Value + " - " + organisationBill.DateTo.ToString("MMMM yyyy"));
			AssertInvoiceLine(invoiceLines[i++], otherBill.Amount, amountChargeCodeName, " Usage");
			AssertInvoiceLine(invoiceLines[i++], -otherBill.DiscountAmount, discountChargeCodeName, " Discount");
			AssertInvoiceLine(invoiceLines[i++], -otherBill.TotalAmount * 0.05m, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value, "Direct Debit Discount");
			AssertInvoiceLine(invoiceLines[i++], -otherDepositDeducted, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted");

			AssertInvoiceLine(invoiceLines[i++], odplBill.Amount, amountChargeCodeName, "On Demand Usage (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], -odplBill.DiscountAmount, discountChargeCodeName, "On Demand Discount (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], odplBill.SurchargeAmount, odplSurchargeChargeCodeName, "On Demand Surcharge (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], odplProcessingFee, discountChargeCodeName, "Direct Debit Discount (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], -odplDepositDeducted, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], Utilities.Round((odplBill.TotalAmount + odplProcessingFee - odplDepositDeducted) * 0.095m, BillingConstants.RoundingDecimals), salesTax.AC_Code, salesTax.AC_Desc + " (SALESTAX)");

			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageReportBreakdownComment.Value);

			AssertEquals("Invoice lines", 12, invoiceLines.Length);

			// missing tax rate
			salesTaxRates = new CodeDescriptionPairList();
			EDIDataRegistry.Instance.SalesTaxRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, salesTaxRates);

			organisationBill.CalculateAll(0);
			try
			{
				organisationBill.CreateInvoice(ZDateTime.Empty);
				Fail("Exception should be thrown");
			}
			catch (InvalidOperationException ex)
			{
				Assert("Exception was thrown", ex.Message.StartsWith("Invalid or missing sales tax rate for SALESTAX in registry"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_DepositCurrencyDiffersFromInvoiceCurrency()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var usdExchangeRate = 2m;
			BillingTestHelper.CreateExchangeRate(organisationBill.Factory, "USD", usdExchangeRate);
			Factory.Save();

			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(20.10, "USD");
			organisationBill.CalculateAll(0);

			AssertEquals("Precondition", "USD", organisationBill.DepositCurrencyCode);
			AssertEquals("Precondition", "AUD", organisationBill.InvoiceCurrencyCode);
			AssertEquals("Precondition: amount", 150m, organisationBill.Amount);
			AssertEquals("Precondition: discount", 15m, organisationBill.DiscountAmount);
			AssertEquals("Precondition: deposit deducted", 20.10m, organisationBill.DepositDeducted);
			AssertEquals("Precondition: deposit deducted", 10.05m, organisationBill.DepositDeductedInInvoiceCurrency);

			var invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			var depositLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => !x.AL_Desc.IsEmpty && x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDepositChargeCode.Value).ToArray();

			AssertInvoiceLine(depositLines[0], -organisationBill.DepositDeductedInInvoiceCurrency, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_SalesTax_FullDeposit_ProcessingCharge()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var salesTax = BillingTestHelper.CreateChargeCode(Factory, null, "SALESTAX");
			salesTax.AC_Desc = "Sales Tax 9.5%";
			Factory.Save();

			var salesTaxRates = new CodeDescriptionPairList();
			salesTaxRates.AddPair(salesTax.AC_Code, "9.5");
			EDIDataRegistry.Instance.SalesTaxRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, salesTaxRates);

			var billing = organisation.LicCompany.SelfBilling;
			billing.L4_ProcessingFee = "MPF";
			billing.L4_ProcessingFeePercent = 5.0m;
			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(1000m, "AUD");

			// usage incurs sales tax
			var onDemandDelivery = organisation.LicCompany.InvoiceDeliveries[0];
			onDemandDelivery.L9_AC_SalesTaxChargeCode = salesTax.PK;
			onDemandDelivery.L9_SystemCode = BillingConstants.BillingSystem.ODM;
			// Dummy usage incurs no sales tax
			var otherDelivery = organisation.LicCompany.InvoiceDeliveries.AddNew();
			otherDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			otherDelivery.L9_RX_NKInvoiceCurrency = "AUD";

			organisationBill.CalculateAll(0);

			AssertEquals("Precondition: amount", 150m, organisationBill.Amount);
			AssertEquals("Precondition: discount", 15m, organisationBill.DiscountAmount);
			AssertEquals("Precondition: surcharge", 5m, organisationBill.SurchargeAmount);
			AssertEquals("Precondition: deposit deducted", 140m, organisationBill.DepositDeducted);
			AssertEquals("Precondition: processing fee", 0m, organisationBill.ProcessingFeeAmount);

			var invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			var invoiceLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => !x.AL_Desc.IsEmpty).ToArray();

			var odplAmountChargeCodeName = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			var odplDiscountChargeCodeName = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			var odplSurchargeChargeCodeName = EDIDataRegistry.Instance.OdplSurchargeChargeCode.Value;
			var odplBill = organisationBill.SystemBills[0];
			var otherBill = organisationBill.SystemBills[1];

			var i = 0;
			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription.Value + " - " + organisationBill.DateTo.ToString("MMMM yyyy"));
			AssertInvoiceLine(invoiceLines[i++], otherBill.Amount, odplAmountChargeCodeName, " Usage");
			AssertInvoiceLine(invoiceLines[i++], -otherBill.Amount, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted");
			AssertInvoiceLine(invoiceLines[i++], -otherBill.DiscountAmount, odplDiscountChargeCodeName, " Discount");
			AssertInvoiceLine(invoiceLines[i++], otherBill.DiscountAmount, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted");

			AssertInvoiceLine(invoiceLines[i++], odplBill.Amount, odplAmountChargeCodeName, "On Demand Usage (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], -odplBill.Amount, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], -odplBill.DiscountAmount, odplDiscountChargeCodeName, "On Demand Discount (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], +odplBill.DiscountAmount, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], +odplBill.SurchargeAmount, odplSurchargeChargeCodeName, "On Demand Surcharge (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], -odplBill.SurchargeAmount, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted (SALESTAX)");

			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageReportBreakdownComment.Value);

			AssertEquals("Invoice lines", 12, invoiceLines.Length);

			// processing is a discount rather than a fee
			billing.L4_ProcessingFee = "DDE";
			CreateTestBillWithOdpl();
			organisationBill.CalculateAll(0);
			AssertEquals("Precondition: amount", 150m, organisationBill.Amount);
			AssertEquals("Precondition: discount", 15m, organisationBill.DiscountAmount);
			AssertEquals("Precondition: surcharge", 5m, organisationBill.SurchargeAmount);
			AssertEquals("Precondition: deposit deducted", 133m, organisationBill.DepositDeducted);
			AssertEquals("Precondition: processing fee", -7m, organisationBill.ProcessingFeeAmount);
			AssertEquals("Precondition: TotalDue", 0m, organisationBill.TotalDue);

			invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			invoiceLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => !x.AL_Desc.IsEmpty).ToArray();
			odplBill = organisationBill.SystemBills[0];
			otherBill = organisationBill.SystemBills[1];
			i = 0;
			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription.Value + " - " + organisationBill.DateTo.ToString("MMMM yyyy"));
			AssertInvoiceLine(invoiceLines[i++], otherBill.Amount, odplAmountChargeCodeName, " Usage");
			AssertInvoiceLine(invoiceLines[i++], -otherBill.Amount, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted");
			AssertInvoiceLine(invoiceLines[i++], -otherBill.DiscountAmount, odplDiscountChargeCodeName, " Discount");
			AssertInvoiceLine(invoiceLines[i++], otherBill.DiscountAmount, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted");
			AssertInvoiceLine(invoiceLines[i++], -otherBill.TotalAmount * 0.05m, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value, "Direct Debit Discount");
			AssertInvoiceLine(invoiceLines[i++], otherBill.TotalAmount * 0.05m, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted");

			AssertInvoiceLine(invoiceLines[i++], odplBill.Amount, odplAmountChargeCodeName, "On Demand Usage (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], -odplBill.Amount, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], -odplBill.DiscountAmount, odplDiscountChargeCodeName, "On Demand Discount (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], odplBill.DiscountAmount, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], +odplBill.SurchargeAmount, odplSurchargeChargeCodeName, "On Demand Surcharge (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], -odplBill.SurchargeAmount, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], -odplBill.TotalAmount * 0.05m, odplDiscountChargeCodeName, "Direct Debit Discount (SALESTAX)");
			AssertInvoiceLine(invoiceLines[i++], +odplBill.TotalAmount * 0.05m, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted (SALESTAX)");

			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageReportBreakdownComment.Value);

			AssertEquals("Invoice lines", 16, invoiceLines.Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_ForeignCurrencyZeroAmount()
		{
			// Promlem: Sum(round(values)) may not equal round(sum(values))
			// E.g. if 1.334 is exchange rate and we have two lines both with amount 10.01
			// 10.01 * 1.334 = 13.35[334]
			// 2 * 13.35 / 1.334 = 20.01
			// 20.01 != 2 * 10.01
			BillingTestHelper.LoadClientSpecificDocuments();
			var usdExchangeRate = 1.334m;
			BillingTestHelper.CreateExchangeRate(Factory, "USD", usdExchangeRate);
			Factory.Save();
			var amountChargeCodeName = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var systemBill1 = new DummySystemBill(organisation, BillingConstants.BillingSystem.ODM, 10.01m, 0m);
			var systemBill2 = new DummySystemBill(organisation, "ZZ1", 10.01m, 0m);
			organisationBill = NewBill("USD");
			organisationBill.DateTo = systemBill1.PeriodStart;
			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);

			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(50m, "AUD");
			organisationBill.CalculateAll(0);

			var bill0 = organisationBill.SystemBills[0];
			var bill1 = organisationBill.SystemBills[1];

			var invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			AssertEquals(0m, invoice.AH_OSExTaxAmount);
			AssertEquals(0m, invoice.AH_LocalExTaxAmount);
			var invoiceLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => !x.AL_Desc.IsEmpty).ToArray();
			var i = 0;
			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription.Value + " - " + organisationBill.DateTo.ToString("MMMM yyyy"));
			AssertInvoiceLine(invoiceLines[i++], Utilities.Round(bill0.Amount * usdExchangeRate, BillingConstants.RoundingDecimals), amountChargeCodeName, "On Demand Usage");
			AssertInvoiceLine(invoiceLines[i++], -Utilities.Round(bill0.Amount * usdExchangeRate, BillingConstants.RoundingDecimals), EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted");
			AssertInvoiceLine(invoiceLines[i++], Utilities.Round(bill1.Amount * usdExchangeRate, BillingConstants.RoundingDecimals), amountChargeCodeName, " Usage");
			AssertInvoiceLine(invoiceLines[i++], -Utilities.Round(bill1.Amount * usdExchangeRate, BillingConstants.RoundingDecimals), EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "Deposit Deducted");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_ExceptionHandling()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var systemBill = new DummySystemBill(organisation, BillingConstants.BillingSystem.ODM, 100m, 10m);

			organisationBill = NewBill();
			organisationBill.DateTo = systemBill.PeriodStart;
			organisationBill.AddSystemBill(systemBill);

			var anotherFactory = new BusinessObjectFactory();
			var invoices = anotherFactory.Load<ARInvoice>(new ZQuery());
			AssertEquals("Precondition", 0, invoices.Length);

			ARInvoice invoice = null;
			systemBill.ThrowExceptionOnInvoiceSaving = true;
			try
			{
				invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
				Fail("Exception should be thrown from system bill");
			}
			catch (Exception ex)
			{
				AssertEquals("Exception was processed and then thrown again", "Christmas exception from system bill", ex.Message);
			}
			AssertEquals("No invoice", null, invoice);

			Factory.Save();
			invoices = anotherFactory.Load<ARInvoice>(new ZQuery());
			AssertEquals("Invoice not saved", 0, invoices.Length);

			systemBill.ThrowExceptionOnInvoiceSaving = false;
			invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			AssertNotNull("Invoice created", invoice);

			invoices = anotherFactory.Load<ARInvoice>(new ZQuery());
			AssertEquals("Only newly created invoice saved", 1, invoices.Length);
			AssertEquals("Newly created invoice", invoice.PK, invoices[0].PK);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2017, 6, 20)]
		public void TestCreateInvoice_NextPrepay()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var inv1 = BillingTestHelper.CreateDepositInvoice(Factory, licence, 1, 10.5m, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, null);
			inv1.AH_DueDate = ZDateTime.Empty;
			Factory.Save();
			AssertEquals(11.55m, inv1.AH_OutstandingAmount);

			AssertEquals(0m, organisationBill.PrepayNext.CurrentInvoiceTotalAmount);
			AssertEquals(0m, organisationBill.PrepayNext.CurrentPrepaymentBalance);
			AssertEquals(0m, organisationBill.PrepayNext.PrepaymentBalanceRequired);

			organisation.LicCompany.SelfBilling.L4_RX_NKPredeterminedPrepaidBalanceCurrency = "AUD";
			organisation.LicCompany.SelfBilling.L4_PredeterminedPrepaidBalance = 1003.55m;
			organisation.LicCompany.SelfBilling.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency = "AUD";
			organisation.LicCompany.SelfBilling.L4_FuturePredeterminedPrepaidBalance = 1203.55m;

			organisationBill.CalculateAll(0);
			organisationBill.CreateInvoice(ZDateTime.Empty);

			AssertEquals(142.45m, organisationBill.PrepayNext.CurrentInvoiceTotalAmount);
			AssertEquals(-11.55m, organisationBill.PrepayNext.CurrentPrepaymentBalance);
			AssertEquals(1003.55m, organisationBill.PrepayNext.PrepaymentBalanceRequired);
			AssertEquals(1203.55m, organisationBill.PrepayNext.FuturePrepaymentBalanceRequired);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2017, 7, 5)]
		public void TestCreateInvoice_NextPrepay_After201706()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			CreateTestBillWithOdpl(new ZDateTime(2017, 7, 1));
			BillingTestHelper.CreateChargeCode(Factory, null, "PPBC");
			EDIDataRegistry.Instance.PrepaidBalanceChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PPBC");
			var prepaidBalanceChargeCode = EDIDataRegistry.Instance.PrepaidBalanceChargeCode.Value;
			Factory.Save();
			BillingTestHelper.SetDepositBalance(organisation.PK.ToGuid(), prepaidBalanceChargeCode, 99.99m, 0, "AUD");
			Factory.Save();

			AssertEquals(0m, organisationBill.PrepayNext.CurrentInvoiceTotalAmount);
			AssertEquals(0m, organisationBill.PrepayNext.CurrentPrepaymentBalance);
			AssertEquals(0m, organisationBill.PrepayNext.PrepaymentBalanceRequired);

			organisation.LicCompany.SelfBilling.L4_RX_NKPredeterminedPrepaidBalanceCurrency = "AUD";
			organisation.LicCompany.SelfBilling.L4_PredeterminedPrepaidBalance = 1003.55m;
			organisation.LicCompany.SelfBilling.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency = "AUD";
			organisation.LicCompany.SelfBilling.L4_FuturePredeterminedPrepaidBalance = 1203.55m;

			organisationBill.CalculateAll(0);
			organisationBill.CreateInvoice(ZDateTime.Empty);

			AssertEquals(154m, organisationBill.PrepayNext.CurrentInvoiceTotalAmount);
			AssertEquals(99.99m, organisationBill.PrepayNext.CurrentPrepaymentBalance);
			AssertEquals(1003.55m, organisationBill.PrepayNext.PrepaymentBalanceRequired);
			AssertEquals(1003.55m, organisationBill.PrepayNext.PrepaymentBalanceRequired);
			AssertEquals(1203.55m, organisationBill.PrepayNext.FuturePrepaymentBalanceRequired);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_WithExternalUSSalesTaxCalculator()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var mockCalculator = new Mock<IUSSalesTaxCalculator>();
			mockCalculator.Setup(x => x.IsEnabled(It.IsAny<GlbBranch>())).Returns(true);
			mockCalculator.Setup(x => x.ShouldSetSalesTaxOnPost(It.IsAny<InvoicingBase>())).Returns(true);
			var calculationResult = new CalculationResult(5m);
			mockCalculator.Setup(x => x.CalculateSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));
			mockCalculator.Setup(x => x.SubmitSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));

			var mockCalculatorExternal = new Mock<IUSSalesTaxCalculator>();
			mockCalculatorExternal.Setup(x => x.IsEnabled(It.IsAny<GlbBranch>())).Returns(true);
			mockCalculatorExternal.Setup(x => x.ShouldSetSalesTaxOnPost(It.IsAny<InvoicingBase>())).Returns(true);
			mockCalculatorExternal.Setup(x => x.CalculateSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));
			mockCalculatorExternal.Setup(x => x.SubmitSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				var invoice = organisationBill.CreateInvoice(ZDateTime.Empty, usSalesTaxCalculator: mockCalculatorExternal.Object);

				AssertSame("External Calculator should be used when passed to CreateInvoice(ZDateTime.Empty)", mockCalculatorExternal.Object, invoice.USSalesTaxCalculator);
				mockCalculatorExternal.Verify(x => x.Dispose(), Times.Never(), "Calculator should not be disposed as it is owned by caller");
				mockCalculatorExternal.Verify(x => x.SetSalesTaxLineItem(It.IsAny<InvoicingBase>(), It.IsAny<decimal>()), Times.Once(), "Sales tax line should be added");
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_WithInternalUSSalesTaxCalculator()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var mockCalculator = new Mock<IUSSalesTaxCalculator>();
			mockCalculator.Setup(x => x.IsEnabled(It.IsAny<GlbBranch>())).Returns(true);
			mockCalculator.Setup(x => x.ShouldSetSalesTaxOnPost(It.IsAny<InvoicingBase>())).Returns(true);
			var calculationResult = new CalculationResult(5m);
			mockCalculator.Setup(x => x.CalculateSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));
			mockCalculator.Setup(x => x.SubmitSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				var invoice = organisationBill.CreateInvoice(ZDateTime.Empty);

				AssertSame("Calculator should be created by ObjectFactory when not passed to CreateInvoice(ZDateTime.Empty)", mockCalculator.Object, invoice.USSalesTaxCalculator);
				mockCalculator.Verify(x => x.Dispose(), Times.Once(), "Calculator should be disposed as it is owned by the invoice itself");
				mockCalculator.Verify(x => x.SetSalesTaxLineItem(It.IsAny<InvoicingBase>(), It.IsAny<decimal>()), Times.Once(), "Sales tax line should be added");
			}
		}

		public void TestAddSystemBill()
		{
			var periodStart = EdiDateTest.MonthToday;
			var organisationBill = NewBill();
			var user1 = new UsingParty();
			var systemBill1 = new SystemBill(Factory);
			systemBill1.SystemUsages.Add(new DummyUsage(Factory, user1, periodStart));
			systemBill1.SystemUsages.Add(new DummyUsage(Factory, user1, periodStart));
			var systemBill2 = new SystemBill(Factory);
			systemBill2.SystemUsages.Add(new DummyUsage(Factory, user1, periodStart));
			systemBill2.SystemUsages.Add(new DummyUsage(Factory, user1, periodStart));
			systemBill2.SystemUsages.Add(new DummyUsage(Factory, user1, periodStart));

			organisationBill.AddSystemBill(systemBill1);
			AssertEquals(1, organisationBill.SystemBills.Count);
			AssertEquals(systemBill1, organisationBill.SystemBills[0]);
			AssertEquals(2, organisationBill.SystemUsages.Count);

			organisationBill.AddSystemBill(systemBill2);
			AssertEquals(2, organisationBill.SystemBills.Count);
			AssertEquals(systemBill1, organisationBill.SystemBills[0]);
			AssertEquals(systemBill2, organisationBill.SystemBills[1]);
			AssertEquals(5, organisationBill.SystemUsages.Count);

			AssertEquals(systemBill1.SystemUsages[0], organisationBill.SystemUsages[0]);
			AssertEquals(systemBill1.SystemUsages[1], organisationBill.SystemUsages[1]);
			AssertEquals(systemBill2.SystemUsages[0], organisationBill.SystemUsages[2]);
			AssertEquals(systemBill2.SystemUsages[1], organisationBill.SystemUsages[3]);
			AssertEquals(systemBill2.SystemUsages[2], organisationBill.SystemUsages[4]);
		}

		public void TestValidateAll()
		{
			var child1 = BillingTestHelper.CreateDependentOrganisation(organisation, "GGG");

			var depositChild1 = BillingTestHelper.CreateDependentOrganisation(organisation, "DD1");
			depositChild1.LicCompany.SetMonthlyUsageDepositBalanceForTest(100m, "AUD");

			var depositChild2 = BillingTestHelper.CreateDependentOrganisation(organisation, "DD2");
			depositChild2.LicCompany.SetMonthlyUsageDepositBalanceForTest(200m, "AUD");

			var systemBill1 = new DummySystemBill(child1, "DUM", 100m, 10m);
			var systemBill2 = new DummySystemBill(depositChild1, "DUM", 100m, 10m);
			var systemBill3 = new DummySystemBill(depositChild1, BillingConstants.BillingSystem.ODM, 100m, 10m);
			systemBill3.SystemUsages.Add(new DummyUsage(Factory, new UsingParty(depositChild2), systemBill3.PeriodStart));

			organisationBill = NewBill();
			organisationBill.DateTo = systemBill1.PeriodStart;

			organisationBill.AddSystemBill(systemBill1);
			organisationBill.CalculateAll(0);
			AssertNoNotifications(organisationBill);

			organisationBill.AddSystemBill(systemBill2);
			organisationBill.CalculateAll(0);
			AssertNoNotifications("No validation of subsidiary deposits, as there is no ODPL bill", organisationBill);

			organisationBill.AddSystemBill(systemBill3);
			organisationBill.CalculateAll(0);
			AssertHasRowError(organisationBill, depositChild1.OH_Code + " " + OrganisationBill.ValidationMessages.SubsidiaryHaveDeposit);
			AssertHasRowError(organisationBill, depositChild2.OH_Code + " " + OrganisationBill.ValidationMessages.SubsidiaryHaveDeposit);
			AssertEquals("Notifications are distinct per organisation", 2, organisationBill.RowErrors.Count());
		}

		public void TestValidateAll_ExchangeRate()
		{
			var systemBill1 = new DummySystemBill(organisation, "DUM", 100m, 10m);
			var systemBill2 = new DummySystemBill(organisation, "DUM", 100m, 10m);
			var usage1 = new DummyUsage(Factory, user, systemBill1.PeriodStart);
			var usage2 = new DummyUsage(Factory, user, systemBill1.PeriodStart);
			var usage3 = new DummyUsage(Factory, user, systemBill1.PeriodStart);
			usage1.CurrencyCode_Exposed = "USD";
			usage1.Amount_Exposed = 100m;
			usage2.CurrencyCode_Exposed = "NZD";
			usage2.Amount_Exposed = 200m;
			usage3.CurrencyCode_Exposed = "EUR";
			usage3.Amount_Exposed = 0m;
			systemBill1.SystemUsages.Add(usage1);
			systemBill2.SystemUsages.Add(usage2);

			organisationBill = new OrganisationBill(Factory, Env.CurrentBranch.PK, organisation.PK, "AUD", new ZDateTime(2016, 6, 30));
			organisationBill.DateTo = systemBill1.PeriodStart;

			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
			organisationBill.CalculateAll(0);
			AssertHasRowError(organisationBill, "Exchange rate for 30-Jun-16 not found for NZD, USD to AUD");

			usage1.CurrencyCode_Exposed = "AUD";
			usage2.CurrencyCode_Exposed = "";
			organisationBill = new OrganisationBill(Factory, Env.CurrentBranch.PK, organisation.PK, "AUD", new ZDateTime(2016, 6, 30));
			organisationBill.DateTo = systemBill1.PeriodStart;
			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
			organisationBill.CalculateAll(0);
			AssertNoRowErrorContaining(organisationBill, "Exchange rate for 30-Jun-16 not found");
			AssertHasRowErrorContaining(organisationBill, "No currency specified");

			usage2.CurrencyCode_Exposed = "AUD";
			organisationBill = new OrganisationBill(Factory, Env.CurrentBranch.PK, organisation.PK, "AUD", new ZDateTime(2016, 6, 30));
			organisationBill.DateTo = systemBill1.PeriodStart;
			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
			organisationBill.CalculateAll(0);
			AssertNoNotifications(organisationBill);
		}

		public void TestValidateAll_SystemBills()
		{
			CreateTestBill();

			foreach (DummySystemBill systemBill in organisationBill.SystemBills)
			{
				AssertEquals("Precondition", false, systemBill.MethodWasCalled("ValidateAllCore"));
			}

			organisationBill.ValidateAll();
			foreach (DummySystemBill systemBill in organisationBill.SystemBills)
			{
				AssertEquals("Method was called", true, systemBill.MethodWasCalled("ValidateAllCore"));
			}
		}

		[TestDate(2017, 1, 1)]
		public void TestValidateAll_Deposit()
		{
			organisationBill.CalculateAll(0);
			AssertNoRowErrors(organisationBill);

			organisation.LicCompany.SelfBilling.L4_ProcessingFee = "MPF";
			organisation.LicCompany.SelfBilling.L4_ProcessingFeePercent = 5.0m;
			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(10m, "GBP", false);
			organisationBill.CalculateAll(0);

			AssertHasRowErrorContaining(organisationBill, "Deposit Exchange Rate for 01-Jan-17 not found for GBP to AUD");
			AssertHasRowErrorContaining(organisationBill, "The Deposit is invalid. Please refer to [Organisation -> License -> Invoicing -> Deposits] for more details.");
		}

		public void TestAlreadyInvoiced()
		{
			CreateTestBill();

			var chargeableUsage1 = Factory.NewWithValidTestData<ClientChargeableUsage>();
			var invoice = Factory.New<ARInvoice>();
			chargeableUsage1.U1_AH_Invoice = invoice.PK;

			organisationBill.CalculateAll(0);
			AssertNoNotifications(organisationBill);
			Assert("CanInvoice", organisationBill.CanInvoice);

			organisationBill.SystemBills[0].SystemUsages[0].ChargeableUsagePKs.Add(chargeableUsage1.PK);
			organisationBill.CalculateAll(0);
			AssertNoNotifications(organisationBill);
			Assert("CanInvoice", !organisationBill.CanInvoice);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddAttachments()
		{
			try
			{
				SqlBlockingObserver.Start(TimeSpan.FromMilliseconds(100));

				BillingTestHelper.LoadClientSpecificDocuments();
				var organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA", "SYD", "AAA");
				var organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB", "SYD", "BBB");
				var organisation3 = BillingTestHelper.CreateOrganisation(Factory, "CCC", "SYD", "CCC");

				var periodStart = EdiDateTest.MonthToday;
				var systemUsage1 = new DummyUsage(Factory, new UsingParty(organisation1), periodStart, 100m);
				var systemUsage2 = new DummyUsage(Factory, new UsingParty(organisation2), periodStart, 200m);
				var systemUsage3 = new DummyUsage(Factory, new UsingParty(organisation3), periodStart, 300m);

				var systemBill1 = new SystemBill(Factory);
				systemBill1.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1 });

				var systemBill2 = new SystemBill(Factory);
				systemBill2.PopulateFromSystemUsages(new SystemUsage[] { systemUsage2 });

				var systemBill3 = new SystemBill(Factory);
				systemBill3.PopulateFromSystemUsages(new SystemUsage[] { systemUsage3 });

				var organisationBill = new OrganisationBillForTesting(Factory, organisation1);
				organisationBill.DateTo = systemBill1.PeriodStart;

				organisationBill.AddSystemBill(systemBill1);
				organisationBill.AddSystemBill(systemBill2);
				organisationBill.AddSystemBill(systemBill3);

				var invoice = Factory.New<ARInvoice>();
				var attachments = invoice.DocManagerInfo.Files.Cast<IeDoc>();
				AssertEquals("Precondition: no attachments", 0, attachments.Count());

				var expectedExcelExtension = "xlsx";
				using (Globals.TemporaryOverrideForIsTest(false))
				{
					organisationBill.AddAttachments_Exposed(invoice);
				}
				attachments = invoice.DocManagerInfo.Files.Cast<IeDoc>();
				AssertEquals("Three attachments created", 6, attachments.Count());
				AssertEquals("Main summary in PDF", true, attachments.Any(x => x.DocType == "OD1" && x.FileName == "AAASYD Billing Summary.pdf"));
				AssertEquals("Secondary summary in PDF", true, attachments.Any(x => x.DocType == "OD2" && x.FileName == "BBBSYD Billing Summary.pdf"));
				AssertEquals("Secondary summary in PDF", true, attachments.Any(x => x.DocType == "OD2" && x.FileName == "CCCSYD Billing Summary.pdf"));
				AssertEquals("Main summary in excel", true, attachments.Any(x => x.DocType == "OD1" && x.FileName == "AAASYD Billing Summary." + expectedExcelExtension));
				AssertEquals("Secondary summary in excel", true, attachments.Any(x => x.DocType == "OD2" && x.FileName == "BBBSYD Billing Summary." + expectedExcelExtension));
				AssertEquals("Secondary summary in excel", true, attachments.Any(x => x.DocType == "OD2" && x.FileName == "CCCSYD Billing Summary." + expectedExcelExtension));

				var organisationWithNoUsage = BillingTestHelper.CreateOrganisation(Factory, "DDD");
				organisationBill = new OrganisationBillForTesting(Factory, organisationWithNoUsage);
				organisationBill.DateTo = systemBill1.PeriodStart;
				organisationBill.AddSystemBill(systemBill1);
				organisationBill.AddSystemBill(systemBill2);
				organisationBill.AddSystemBill(systemBill3);

				invoice = Factory.New<ARInvoice>();
				using (Globals.TemporaryOverrideForIsTest(false))
				{
					organisationBill.AddAttachments_Exposed(invoice);
					attachments = invoice.DocManagerInfo.Files.Cast<IeDoc>();
				}
				AssertEquals("Attachments created", 4, attachments.Count());
				AssertEquals("Main summary even when organisation has no usage in pdf", true, attachments.Any(x => x.DocType == "OD1" && x.FileName == "DDDSYD Billing Summary.pdf"));
				AssertEquals("Main summary even when organisation has no usage in excel", true, attachments.Any(x => x.DocType == "OD1" && x.FileName == "DDDSYD Billing Summary." + expectedExcelExtension));

				var zipDoc = attachments.First(x => x.FileName == "Billing Summaries (pdf).zip");
				AssertEquals("zip doc type", "OD2", zipDoc.DocType);
				using (var zipFile = new ZipFileCore(zipDoc.GetImageDataReader()))
				{
					AssertEquals("zip entries", 3, zipFile.Count);
					AssertEquals("AAASYD Billing Summary.pdf", zipFile[0].Name);
					AssertEquals("BBBSYD Billing Summary.pdf", zipFile[1].Name);
					AssertEquals("CCCSYD Billing Summary.pdf", zipFile[2].Name);
				}

				zipDoc = attachments.First(x => x.FileName == "Billing Summaries (" + expectedExcelExtension + ").zip");
				AssertEquals("zip doc type", "OD2", zipDoc.DocType);
				using (var zipFile = new ZipFileCore(zipDoc.GetImageDataReader()))
				{
					AssertEquals("zip entries", 3, zipFile.Count);
					AssertEquals("AAASYD Billing Summary." + expectedExcelExtension, zipFile[0].Name);
					AssertEquals("BBBSYD Billing Summary." + expectedExcelExtension, zipFile[1].Name);
					AssertEquals("CCCSYD Billing Summary." + expectedExcelExtension, zipFile[2].Name);
				}
			}
			catch (SqlException ex) when (ex.Number == 1222)
			{
				Assertion.Fail($@"{ex}
Locking Info:

{SqlBlockingObserver.GetLockingInfo()}

{SqlBlockingObserver.GetExtraDebugInformation()}");
			}
			finally
			{
				SqlBlockingObserver.Stop();
			}
		}

		public void TestAddAttachments_EmptyDoc()
		{
			var organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.CreateOrganisation(Factory, "BBB");
			BillingTestHelper.CreateOrganisation(Factory, "CCC");

			var systemBill1 = new SystemBill(Factory);

			var systemBill2 = new SystemBill(Factory);

			var systemBill3 = new SystemBill(Factory);

			var organisationBill = new OrganisationBillForTesting(Factory, organisation1);
			organisationBill.DateTo = systemBill1.PeriodStart;

			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
			organisationBill.AddSystemBill(systemBill3);

			var invoice = Factory.New<ARInvoice>();
			var attachments = invoice.DocManagerInfo.Files.Cast<IeDoc>();
			AssertEquals("Precondition: no attachments", 0, attachments.Count());

			using (Globals.TemporaryOverrideForIsTest(false))
			{
				organisationBill.AddAttachments_Exposed(invoice);
			}
			attachments = invoice.DocManagerInfo.Files.Cast<IeDoc>();
			AssertEquals("No attachments created", 0, attachments.Count());

			var organisationWithNoUsage = BillingTestHelper.CreateOrganisation(Factory, "DDD");
			organisationBill = new OrganisationBillForTesting(Factory, organisationWithNoUsage);
			organisationBill.DateTo = systemBill1.PeriodStart;
			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
			organisationBill.AddSystemBill(systemBill3);

			invoice = Factory.New<ARInvoice>();
			using (Globals.TemporaryOverrideForIsTest(false))
			{
				organisationBill.AddAttachments_Exposed(invoice);
			}
			attachments = invoice.DocManagerInfo.Files.Cast<IeDoc>();
			AssertEquals("No attachments created", 0, attachments.Count());
		}

		public void TestInvoicePkForThisMonth()
		{
			Assert("no usage", organisationBill.InvoicePkForThisMonth.IsEmpty);

			CreateTestBill();
			var invoice = Factory.New<ARInvoice>();
			var chargeableUsage1 = Factory.New<ClientChargeableUsage>();
			chargeableUsage1.U1_AH_Invoice = invoice.PK;
			organisationBill.SystemBills[0].SystemUsages[0].ChargeableUsagePKs.Add(chargeableUsage1.PK);
			organisationBill.CalculateAll(0);
			AssertEquals("this month", invoice.PK, organisationBill.InvoicePkForThisMonth);
			AssertEquals("number", invoice.AH_TransactionNum, organisationBill.InvoiceNumber);
			AssertEquals("Status", OrganisationBill.StatusMessages.Invoiced, organisationBill.Status);

			invoice.SetCancellationFlag(true);
			organisationBill.CalculateAll(0);
			AssertEquals(true, invoice.IsCancelled);
			Assert("invoice cancelled", organisationBill.InvoicePkForThisMonth.IsEmpty);
			AssertEquals("number", "", organisationBill.InvoiceNumber);
			AssertEquals("Status", OrganisationBill.StatusMessages.ReadyNew, organisationBill.Status);
		}

		public void TestInvoiceForLastMonth()
		{
			AssertNull("no usage", organisationBill.InvoiceForLastMonth);

			CreateTestBillWithOdpl();
			var invoice = Factory.New<ARInvoice>();
			var chargeableUsage1 = Factory.New<ClientChargeableUsage>();
			chargeableUsage1.U1_AH_Invoice = invoice.PK;
			chargeableUsage1.U1_PeriodStart = organisationBill.SystemBills[0].PeriodStart.AddMonths(-1);
			chargeableUsage1.U1_Code = organisationBill.SystemBills[0].SystemCode;
			chargeableUsage1.U1_LC = organisationBill.LicCompany.PK;

			AssertEquals("last month", invoice.PK, organisationBill.InvoiceForLastMonth.PK);

			CreateTestBillWithOdpl();
			chargeableUsage1.U1_LC = organisationBill.LicCompany.PK;
			chargeableUsage1.U1_PeriodStart = chargeableUsage1.U1_PeriodStart.AddMonths(-1);
			AssertNull("usage too old", organisationBill.InvoiceForLastMonth);

			CreateTestBillWithOdpl();
			chargeableUsage1.U1_LC = organisationBill.LicCompany.PK;
			chargeableUsage1.U1_PeriodStart = organisationBill.SystemBills[0].PeriodStart.AddMonths(-1);
			invoice.SetCancellationFlag(true);
			AssertEquals(true, invoice.IsCancelled);
			AssertNull("invoice cancelled", organisationBill.InvoiceForLastMonth);
		}

		public void TestMinimumAmountToBill()
		{
			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(90, "AUD");
			organisationBill.CalculateAll(0);
			AssertEquals("Precondition", 150m, organisationBill.Amount);
			AssertEquals("Precondition", 15m, organisationBill.DiscountAmount);
			AssertEquals("Precondition", 5m, organisationBill.SurchargeAmount);
			AssertEquals("Precondition", 140m, organisationBill.TotalAmount);
			AssertEquals("Precondition", 50m, organisationBill.TotalDue);

			organisationBill.CalculateAll(60m);
			AssertEquals("no usage removed", 2, organisationBill.SystemBills.Count);
			AssertEquals("total due not changed", 50m, organisationBill.TotalDue);
			AssertEquals(false, organisationBill.IsTooSmallToBill);

			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(135, "AUD");
			organisationBill.CalculateAll(60m);
			AssertEquals("all usage present", 2, organisationBill.SystemBills.Count);
			AssertEquals("total due", 5m, organisationBill.TotalDue);
			AssertEquals(false, organisationBill.IsTooSmallToBill);

			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(25, "AUD");
			var systemBill1 = new DummySystemBill(organisation, "DOM", 50m, 0m);
			organisationBill = NewBill();
			organisationBill.DateTo = systemBill1.PeriodStart;
			organisationBill.AddSystemBill(systemBill1);
			organisationBill.CalculateAll(0m);
			AssertEquals("Precondition", 25m, organisationBill.TotalDue);
			AssertEquals(false, organisationBill.IsTooSmallToBill);

			organisationBill.CalculateAll(25m);
			AssertEquals(false, organisationBill.IsTooSmallToBill);
			AssertEquals(true, organisationBill.CanInvoice);

			organisationBill.CalculateAll(25.01m);
			AssertEquals(true, organisationBill.IsTooSmallToBill);
			AssertEquals(false, organisationBill.CanInvoice);

			var partner = BillingTestHelper.CreateOrganisation(Factory, "PA1");
			partner.LicCompany.SelfBilling.L4_IsPartner = true;
			organisation.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = partner.PK;
			organisationBill = NewBill();
			organisationBill.DateTo = systemBill1.PeriodStart;
			organisationBill.AddSystemBill(systemBill1);
			organisationBill.CalculateAll(50.01m);
			AssertEquals("partner clients have no minimum amount", false, organisationBill.IsTooSmallToBill);
			AssertEquals(true, organisationBill.CanInvoice);

			// $100 usage with -$100 discount
			// plus $10 usage
			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(0, "AUD");
			organisation.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = ZGuid.Empty;
			systemBill1 = new DummySystemBill(organisation, BillingConstants.BillingSystem.ODM, 100m, 100m);
			var systemBill2 = new DummySystemBill(organisation, "DOM", 10m, 0m);
			organisationBill = NewBill();
			organisationBill.DateTo = systemBill1.PeriodStart;
			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
			organisationBill.CalculateAll(20m);
			AssertEquals("small usage removed", 1, organisationBill.SystemBills.Count);
			AssertEquals("total due", 0m, organisationBill.TotalDue);
			AssertEquals("too small", false, organisationBill.IsTooSmallToBill);

			systemBill1 = new DummySystemBill(organisation, BillingConstants.BillingSystem.ODM, 100m, 95m);
			organisationBill = NewBill();
			organisationBill.DateTo = systemBill1.PeriodStart;
			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
			organisationBill.CalculateAll(20m);
			AssertEquals("no usage removed", 2, organisationBill.SystemBills.Count);
			AssertEquals("total due is below min", 15m, organisationBill.TotalDue);
			AssertEquals(true, organisationBill.IsTooSmallToBill);
		}

		[DeveloperOnlyTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCanInvoice()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			Assert("CanInvoice", organisationBill.CanInvoice);

			CreateTestBillWithOdpl();
			organisationBill.AddRowError("Oops");
			Assert("CanInvoice", !organisationBill.CanInvoice);

			CreateTestBillWithOdpl();
			organisationBill.CreateInvoice(ZDateTime.Empty);
			Assert("CanInvoice", !organisationBill.CanInvoice);

			organisation.LicCompany.InvoiceDeliveries[0].L9_IsBilled = false;
			CreateTestBillWithOdpl();
			Assert("CanInvoice", !organisationBill.CanInvoice);

			// See TestMinimumAmountToBill() for IsTooSmallToBill case
		}

		public void TestStatus()
		{
			var partner = BillingTestHelper.CreateOrganisation(Factory, "PA1");
			partner.LicCompany.SelfBilling.L4_IsPartner = true;

			AssertEquals(OrganisationBill.StatusMessages.ReadyNew, organisationBill.Status);

			CreateTestBill();
			var invoiceThisMonth = Factory.New<ARInvoice>();
			var chargeableUsage1 = Factory.New<ClientChargeableUsage>();
			chargeableUsage1.U1_AH_Invoice = invoiceThisMonth.PK;
			organisationBill.SystemBills[0].SystemUsages[0].ChargeableUsagePKs.Add(chargeableUsage1.PK);
			organisationBill.CalculateAll(0);
			AssertEquals("this month", invoiceThisMonth.PK, organisationBill.InvoicePkForThisMonth);
			AssertEquals("Status", OrganisationBill.StatusMessages.Invoiced, organisationBill.Status);

			organisation.LicCompany.InvoiceDeliveries[0].L9_IsBilled = false;
			CreateTestBill();
			AssertEquals("Status", OrganisationBill.StatusMessages.NotBilled, organisationBill.Status);

			organisation.LicCompany.InvoiceDeliveries[0].L9_IsBilled = true;
			organisation.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = ZGuid.Empty;
			CreateTestBill();
			organisationBill.SystemBills.Remove(organisationBill.SystemBills[0]);
			organisationBill.CalculateAll(99999m);
			AssertEquals("Status", OrganisationBill.StatusMessages.TooSmall, organisationBill.Status);

			CreateTestBill();
			var invoiceLastMonth = Factory.New<ARInvoice>();
			var chargeableUsage2 = Factory.New<ClientChargeableUsage>();
			chargeableUsage2.U1_AH_Invoice = invoiceLastMonth.PK;
			chargeableUsage2.U1_PeriodStart = organisationBill.SystemBills[0].PeriodStart.AddMonths(-1);
			chargeableUsage2.U1_Code = organisationBill.SystemBills[0].SystemCode;
			chargeableUsage2.U1_LC = organisationBill.LicCompany.PK;
			organisationBill.CalculateAll(0m);
			AssertEquals("Status", OrganisationBill.StatusMessages.Ready, organisationBill.Status);

			organisation.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = partner.PK;
			CreateTestBill();
			organisationBill.CalculateAll(0m);
			AssertEquals("Status", OrganisationBill.StatusMessages.ReadyPartner, organisationBill.Status);

			organisation.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = ZGuid.Empty;
			CreateTestBill("");
			organisationBill.CalculateAll(0m);
			AssertEquals("Status", OrganisationBill.StatusMessages.Error + " - Invoicing currency is not defined.", organisationBill.Status);

			organisation.LicCompany.LicHeadersForAllDatabases[0].LA_AgreedLiveDate = ZDateTime.Empty;
			CreateTestBill();
			((DummySystemBill)organisationBill.SystemBills[0]).AddWarning = true;
			organisationBill.CalculateAll(0m);
			AssertEquals("Status", OrganisationBill.StatusMessages.Warning + " - Some row warning", organisationBill.Status);
		}

		public void TestLicenceMode()
		{
			var licence2 = BillingTestHelper.CreateDependentLicence(licence, "BBB");
			var user2 = new UsingParty(licence2);

			var systemBill1 = new OdplSystemBill(Factory);
			var odplUsage1 = new OdplUsage(Factory, user, new ZDateTime(2010, 10, 01));
			OdplUsageTest.AddModuleUsage(odplUsage1, "COR", 100, 1);
			odplUsage1.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;
			systemBill1.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1 });

			var systemBill2 = new OdplSystemBill(Factory);
			var odplUsage2 = new OdplUsage(Factory, user2, new ZDateTime(2010, 10, 01));
			OdplUsageTest.AddModuleUsage(odplUsage2, "COR", 100, 1);
			odplUsage2.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.STL;
			systemBill2.PopulateFromSystemUsages(new SystemUsage[] { odplUsage2 });

			organisationBill = NewBill();
			organisationBill.DateTo = systemBill1.PeriodStart;
			organisationBill.AddSystemBill(systemBill1);
			AssertEquals(MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl, organisationBill.LicenceMode);

			organisationBill = NewBill();
			organisationBill.DateTo = systemBill1.PeriodStart;
			organisationBill.AddSystemBill(systemBill2);
			AssertEquals(MonthlyUsageBilling.LicenceModeConstants.Codes.STL, organisationBill.LicenceMode);

			organisationBill = NewBill();
			organisationBill.DateTo = systemBill1.PeriodStart;
			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
			AssertEquals(MonthlyUsageBilling.LicenceModeConstants.Codes.ALL, organisationBill.LicenceMode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSystemMinimumFees()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "AAA", "SYD");
			var org1 = licence1.Company.Header;
			var user1 = new UsingParty(licence1);
			BillingTestHelper.SetInvoicing(org1, Env.CurrentBranch.PK, "AUD");

			var licence2 = BillingTestHelper.CreateLicence(Factory, "EEE", "BBB", "MEL");
			licence2.LA_SiteLiveDate = new ZDateTime(2015, 1, 1);
			var org2 = licence2.Company.Header;
			var user2 = new UsingParty(licence2);
			BillingTestHelper.SetInvoicing(org2, Env.CurrentBranch.PK, "AUD");
			var invoicing = licence2.Company.InvoiceDeliveries.AddNew();
			invoicing.L9_OH_InvoiceTo = org1.PK;

			var defaultDb = licence2.Company.LicEnterprise.Databases.First();
			var testdb = licence2.Company.LicEnterprise.Databases.AddNew();
			testdb.LD_ServerCode = "TST";
			testdb.LD_LicenceType = DatabaseTypes.Codes.Test;
			testdb.LD_LD_ParentDatabase = licence2.Database.PK;
			var testLicence = Factory.New<LicenceHeader>();
			testLicence.LA_LD = testdb.PK;
			testLicence.LA_LC = licence2.Company.PK;
			var dummyClientCompay = BillingTestHelper.CreateClientCompany(licence2.Database, "DM1");
			AssertEquals(2, licence2.Database.ClientCompanies.Count);

			var priceHeader1 = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_RX_NKCurrency = "AUD";
			priceHeader1.L6_ValidFrom = new ZDateTime(2016, 1, 1);
			priceHeader1.L6_TestDbPriceCode = "#NP";
			priceHeader1.L6_LiveMonthsUntilTestDbBilling = 3;
			BillingTestHelper.AddPriceItem(priceHeader1, "COR", BillingConstants.FeeType.Module, "", 1.0m);
			var mfPriceItem = BillingTestHelper.AddPriceItem(priceHeader1, "#MF", BillingConstants.FeeType.Database, "", 250m);
			mfPriceItem.L7_ChargeCode = "MFCHARGE";
			var npPriceItem = BillingTestHelper.AddPriceItem(priceHeader1, "#NP", BillingConstants.FeeType.Database, "", 250m);
			npPriceItem.L7_ChargeCode = "NPCHARGE";

			var discount = org1.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			discount.L5_Description = "DiscoCore";
			discount.L5_Discount = 10m;
			discount.L5_ModuleCode = "COR";
			discount.L5_StartDate = new ZDateTime(2016, 1, 1);

			var surcharge = org1.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			surcharge.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			surcharge.L5_Type = BillingConstants.DiscountType.Surcharge;
			surcharge.L5_Description = "Surcharge";
			surcharge.L5_Discount = -5m;
			surcharge.L5_StartDate = new ZDateTime(2016, 1, 1);

			var gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var mfCharge = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "MFCHARGE");
			var npCharge = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "NPCHARGE");

			Factory.Save();

			var odplUsage1 = new OdplUsage(Factory, user1, new ZDateTime(2016, 5, 1));
			odplUsage1.IsMinimumFeeOwner = true;
			OdplUsageTest.AddModuleUsage(odplUsage1, "COR", 100, 1);
			odplUsage1.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;
			var odplUsage2 = new OdplUsage(Factory, user2, new ZDateTime(2016, 5, 1));
			OdplUsageTest.AddModuleUsage(odplUsage2, "COR", 100, 1);
			odplUsage2.IsMinimumFeeOwner = true;
			odplUsage2.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;

			var systemBill1 = new OdplSystemBill(Factory);
			systemBill1.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2 });

			var context = new BillingRunContext(Factory, ZDateTime.Now, new ZDateTime(2016, 5, 31));
			context.SystemMinimumFeesService = new SystemMinimumFees(new[] { systemBill1 });
			var bill = new OrganisationBill(Factory, Env.CurrentBranch.PK, org1.PK, "AUD", ZDateTime.Now, context);
			bill.DateTo = new ZDateTime(2016, 5, 1);
			bill.AddSystemBill(systemBill1);

			bill.CalculateAll(0);
			AssertEquals("Amount", 200m, bill.Amount);
			AssertEquals("Discount", 20m, bill.DiscountAmount);
			AssertEquals("Surcharge", 10m, bill.SurchargeAmount);
			AssertEquals("System Minimum Fee", 560m, bill.SystemMinimumFeeAmount);
			AssertEquals("Total", 750m, bill.TotalAmount);
			AssertEquals("Due", 750m, bill.TotalDue);

			var invoice = bill.CreateInvoice(ZDateTime.Empty);
			var invoiceLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => !x.AL_Desc.IsEmpty).ToArray();
			var i = 0;
			AssertInvoiceLine(invoiceLines[i++], 0m, EDIDataRegistry.Instance.CommentChargeCode.Value, EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription.Value + " - " + bill.DateTo.ToString("MMMM yyyy"));
			AssertInvoiceLine(invoiceLines[i++], 200m, "ODPLMTHUSE", "On Demand Usage");
			AssertInvoiceLine(invoiceLines[i++], -20m, "DISCODPL", "On Demand Discount");
			AssertInvoiceLine(invoiceLines[i++], 10m, "SURCODPL", "On Demand Surcharge");
			AssertInvoiceLine(invoiceLines[i++], 310m, "MFCHARGE", "System Minimum Fee");
			AssertInvoiceLine(invoiceLines[i++], 250m, "NPCHARGE", "Non-Production System Fee");

			var allBilledUsages = Factory.Load<EdiBilledUsage>(new ZQuery());
			AssertEquals(5, allBilledUsages.Length);
			AssertEquals(invoice.AH_InvoiceAmount, allBilledUsages.Sum(x => x.BU9_TransactionAmountPostDiscount));
			var billedMfUsages = allBilledUsages.Where(x => new[] { mfCharge, npCharge }.Contains(x.AmountChargeCode)).OrderBy(x => x.AmountChargeCode.AC_Code)
				.ThenBy(x => x.BU9_LD == defaultDb.PK ? 0 : 1).ToArray();
			AssertEquals(3, billedMfUsages.Length);

			AssertEquals(billedMfUsages[0].BU9_AC_AmountChargeCode, mfCharge.PK);
			AssertEquals(billedMfUsages[0].BU9_AC_DiscountChargeCode, ZGuid.Empty);
			AssertEquals(billedMfUsages[0].BU9_AH_Invoice, invoice.PK);
			AssertEquals(billedMfUsages[0].BU9_UsageCode, BillingConstants.BillingSystem.ODM);
			AssertEquals(billedMfUsages[0].BU9_BillingModel, BillingConstants.PriceHeaderType.ODM);
			AssertEquals(billedMfUsages[0].BU9_UsageSubCode, "#MF");
			AssertEquals(billedMfUsages[0].BU9_LocalAmountPostDiscount, 155m);
			AssertEquals(billedMfUsages[0].BU9_LocalAmountPreDiscount, 155m);
			AssertEquals(billedMfUsages[0].BU9_LocalProcessingAmount, 0m);
			AssertEquals(billedMfUsages[0].BU9_PeriodStart, new ZDateTime(2016, 5, 1));
			AssertEquals(billedMfUsages[0].BU9_PriceCurrency, "AUD");
			AssertEquals(billedMfUsages[0].BU9_TransactionAmountPostDiscount, 155m);
			AssertEquals(billedMfUsages[0].BU9_TransactionAmountPreDiscount, 155m);
			AssertEquals(billedMfUsages[0].BU9_TransactionProcessingAmount, 0m);
			AssertEquals(billedMfUsages[0].BU9_LD, defaultDb.PK);
			AssertEquals(billedMfUsages[0].BU9_L7, mfPriceItem.PK);
			AssertEquals(billedMfUsages[0].BU9_PriceCode, "#MF");
			AssertEquals(billedMfUsages[0].BU9_UnitCount, 1m);
			AssertEquals(billedMfUsages[0].BU9_UnitPrice, 155m);

			AssertEquals(billedMfUsages[1].BU9_AC_AmountChargeCode, mfCharge.PK);
			AssertEquals(billedMfUsages[1].BU9_AC_DiscountChargeCode, ZGuid.Empty);
			AssertEquals(billedMfUsages[1].BU9_AH_Invoice, invoice.PK);
			AssertEquals(billedMfUsages[1].BU9_UsageCode, BillingConstants.BillingSystem.ODM);
			AssertEquals(billedMfUsages[1].BU9_BillingModel, BillingConstants.PriceHeaderType.ODM);
			AssertEquals(billedMfUsages[1].BU9_UsageSubCode, "#MF");
			AssertEquals(billedMfUsages[1].BU9_LocalAmountPostDiscount, 155m);
			AssertEquals(billedMfUsages[1].BU9_LocalAmountPreDiscount, 155m);
			AssertEquals(billedMfUsages[1].BU9_LocalProcessingAmount, 0m);
			AssertEquals(billedMfUsages[1].BU9_PeriodStart, new ZDateTime(2016, 5, 1));
			AssertEquals(billedMfUsages[1].BU9_PriceCurrency, "AUD");
			AssertEquals(billedMfUsages[1].BU9_TransactionAmountPostDiscount, 155m);
			AssertEquals(billedMfUsages[1].BU9_TransactionAmountPreDiscount, 155m);
			AssertEquals(billedMfUsages[1].BU9_TransactionProcessingAmount, 0m);
			AssertEquals(billedMfUsages[1].BU9_LD, licence1.Company.LicDatabases[0].PK);
			AssertEquals(billedMfUsages[1].BU9_L7, mfPriceItem.PK);
			AssertEquals(billedMfUsages[1].BU9_PriceCode, "#MF");
			AssertEquals(billedMfUsages[1].BU9_UnitCount, 1m);
			AssertEquals(billedMfUsages[1].BU9_UnitPrice, 155m);

			AssertEquals(billedMfUsages[2].BU9_AC_AmountChargeCode, npCharge.PK);
			AssertEquals(billedMfUsages[2].BU9_AC_DiscountChargeCode, ZGuid.Empty);
			AssertEquals(billedMfUsages[2].BU9_AH_Invoice, invoice.PK);
			AssertEquals(billedMfUsages[2].BU9_UsageCode, BillingConstants.BillingSystem.ODM);
			AssertEquals(billedMfUsages[2].BU9_BillingModel, BillingConstants.PriceHeaderType.ODM);
			AssertEquals(billedMfUsages[2].BU9_UsageSubCode, "#NP");
			AssertEquals(billedMfUsages[2].BU9_LocalAmountPostDiscount, 250m);
			AssertEquals(billedMfUsages[2].BU9_LocalAmountPreDiscount, 250m);
			AssertEquals(billedMfUsages[2].BU9_LocalProcessingAmount, 0m);
			AssertEquals(billedMfUsages[2].BU9_PeriodStart, new ZDateTime(2016, 5, 1));
			AssertEquals(billedMfUsages[2].BU9_PriceCurrency, "AUD");
			AssertEquals(billedMfUsages[2].BU9_TransactionAmountPostDiscount, 250m);
			AssertEquals(billedMfUsages[2].BU9_TransactionAmountPreDiscount, 250m);
			AssertEquals(billedMfUsages[2].BU9_TransactionProcessingAmount, 0m);
			AssertEquals(billedMfUsages[2].BU9_LD, testdb.PK);
			AssertEquals(billedMfUsages[2].BU9_L7, npPriceItem.PK);
			AssertEquals(billedMfUsages[2].BU9_PriceCode, "#NP");
			AssertEquals(billedMfUsages[2].BU9_UnitCount, 1m);
			AssertEquals(billedMfUsages[2].BU9_UnitPrice, 250m);
		}

		//Two client companies of a database billed to two different organizations with each bill under the minimum fee
		//but the combined total over the minimum fee. No fee should be applied.
		public void TestSystemMinimumFeesWith2ClientCompanies()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "EN1", "LC1", "DB1");
			var licence2 = BillingTestHelper.CreateLicence(Factory, "EN1", "LC2", "DB1");
			var licCompany1 = licence1.Company;
			var db = licCompany1.LicDatabases[0];
			var clientCompany1 = db.ClientCompanies[0];
			var org1 = clientCompany1.Org;
			var user1 = new UsingParty(clientCompany1);

			var licCompany2 = licence2.Company;
			var db2 = licCompany2.LicDatabases[0];
			var clientCompany2 = db2.ClientCompanies[1];
			var org2 = clientCompany2.Org;
			var user2 = new UsingParty(clientCompany2);

			AssertNotEquals(licCompany1.PK, licCompany2.PK);
			AssertEquals(db.PK, db2.PK);
			AssertNotEquals(clientCompany1.PK, clientCompany2.PK);
			AssertNotEquals(org1.PK, org2.PK);

			CreateMfPrice(licCompany1);
			CreateMfPrice(licCompany2);
			var gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "MFCHARGE");

			Factory.Save();

			var odplUsage1 = new OdplUsage(Factory, user1, new ZDateTime(2016, 5, 1));
			OdplUsageTest.AddModuleUsage(odplUsage1, "COR", 2, 100);
			odplUsage1.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;

			var odplUsage2 = new OdplUsage(Factory, user2, new ZDateTime(2016, 5, 1));
			OdplUsageTest.AddModuleUsage(odplUsage2, "COR", 2, 100);
			odplUsage2.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;

			var systemBill1 = new OdplSystemBill(Factory);
			systemBill1.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1 });

			var systemBill2 = new OdplSystemBill(Factory);
			systemBill2.PopulateFromSystemUsages(new SystemUsage[] { odplUsage2 });

			var context = new BillingRunContext(Factory, ZDateTime.Now, new ZDateTime(2016, 5, 31));
			context.SystemMinimumFeesService = new SystemMinimumFees(new[] { systemBill1, systemBill2 });

			var bill1 = new OrganisationBill(Factory, Env.CurrentBranch.PK, org1.PK, "AUD", ZDateTime.Now, context);
			bill1.DateTo = new ZDateTime(2016, 5, 1);
			bill1.AddSystemBill(systemBill1);
			bill1.CalculateAll(0);

			var bill2 = new OrganisationBill(Factory, Env.CurrentBranch.PK, org2.PK, "AUD", ZDateTime.Now, context);
			bill2.DateTo = new ZDateTime(2016, 5, 1);
			bill2.AddSystemBill(systemBill2);
			bill2.CalculateAll(0);

			AssertEquals("Amount", 200m, bill1.Amount);
			AssertEquals("Amount", 200m, bill2.Amount);
			AssertEquals("System Minimum Fee", 0m, bill1.SystemMinimumFeeAmount);
			AssertEquals("System Minimum Fee", 0m, bill2.SystemMinimumFeeAmount);
			AssertEquals("Total", 200m, bill1.TotalAmount);
			AssertEquals("Total", 200m, bill2.TotalAmount);
			AssertEquals("Due", 200m, bill1.TotalDue);
		}

		public void TestSystemMinimumFees_WithNoCoreUsage_WithStlUsers()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "AAA", "SYD");
			var org1 = licence1.Company.Header;
			var user1 = new UsingParty(licence1);
			BillingTestHelper.SetInvoicing(org1, Env.CurrentBranch.PK, "AUD");

			var priceHeader1 = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_RX_NKCurrency = "AUD";
			priceHeader1.L6_ValidFrom = new ZDateTime(2016, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader1, "COR", BillingConstants.FeeType.Module, "", 1.0m);
			var mfPriceItem = BillingTestHelper.AddPriceItem(priceHeader1, "#MF", BillingConstants.FeeType.Database, "", 250m);
			mfPriceItem.L7_ChargeCode = "MFCHARGE";

			var gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var mfCharge = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "MFCHARGE");
			var npCharge = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "NPCHARGE");

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licence1, 200);

			Factory.Save();

			var monthlyUsageBilling = new MonthlyUsageBilling(new BusinessObjectFactory());
			monthlyUsageBilling.BillingSystems.Clear();
			monthlyUsageBilling.BillingSystems.Add(new OdplBillingSystem());
			monthlyUsageBilling.DateTo = periodStart.AddMonths(1).AddDays(-1);
			monthlyUsageBilling.GenerateReport(null);
			AssertEquals("OrganisationBills created", 1, monthlyUsageBilling.OrganisationBills.Count);
			var bill = monthlyUsageBilling.OrganisationBills[0];
			CombineAssertions(() =>
			{
				AssertEquals("Amount", 0m, bill.Amount);
				AssertEquals("Discount", 0m, bill.DiscountAmount);
				AssertEquals("Surcharge", 0m, bill.SurchargeAmount);
				AssertEquals("System Minimum Fee", 250m, bill.SystemMinimumFeeAmount);
				AssertEquals("Total", 250m, bill.TotalAmount);
				AssertEquals("Due", 250m, bill.TotalDue);
			});
		}

		public void TestSystemMinimumFees_WithOnlyHostedStorageUsage()
		{
			EDIDataRegistry.Instance.HostingStorageBufferPercentage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var periodStart = BillingTestHelper.MonthToday;

			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "AAA", "SYD");
			var org1 = licence1.Company.Header;
			var user1 = new UsingParty(licence1);
			BillingTestHelper.SetInvoicing(org1, Env.CurrentBranch.PK, "AUD");

			var priceHeader1 = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_RX_NKCurrency = "AUD";
			priceHeader1.L6_ValidFrom = new ZDateTime(2016, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader1, "COR", BillingConstants.FeeType.Module, "", 1.0m);
			BillingTestHelper.AddPriceItem(priceHeader1, new UsageCodeKey("HOS", "#HA"), BillingConstants.FeeType.PerGBPerMonth, 80.0m);
			var mfPriceItem = BillingTestHelper.AddPriceItem(priceHeader1, "#MF", BillingConstants.FeeType.Database, "", 250m);
			mfPriceItem.L7_ChargeCode = "MFCHARGE";

			var gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var mfCharge = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "MFCHARGE");
			var npCharge = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "NPCHARGE");

			BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "#HA", periodStart, licence1, 7 * BillingConstants.Hosting.MBperGB);

			Factory.Save();

			var monthlyUsageBilling = new MonthlyUsageBilling(new BusinessObjectFactory());
			monthlyUsageBilling.BillingSystems.Clear();
			monthlyUsageBilling.BillingSystems.Add(new OdplBillingSystem());
			monthlyUsageBilling.BillingSystems.Add(new HostingStorageBillingSystem());
			monthlyUsageBilling.DateTo = periodStart.AddMonths(1).AddDays(-1);
			monthlyUsageBilling.GenerateReport(null);
			AssertEquals("OrganisationBills created", 1, monthlyUsageBilling.OrganisationBills.Count);
			var bill = monthlyUsageBilling.OrganisationBills[0];
			CombineAssertions(() =>
			{
				AssertEquals("Amount 7 x 80m", 560m, bill.Amount);
				AssertEquals("Discount", 0m, bill.DiscountAmount);
				AssertEquals("Surcharge", 0m, bill.SurchargeAmount);
				AssertEquals("System Minimum Fee", 250m, bill.SystemMinimumFeeAmount);
				AssertEquals("Total", 250m + 560m, bill.TotalAmount);
				AssertEquals("Due", 250m + 560m, bill.TotalDue);
			});
		}

		public void TestPrepaymentDiscountWithPredeterminedPrepaidBalance()
		{
			organisation.LicCompany.SelfBilling.L4_ProcessingFee = "PRE";
			organisation.LicCompany.SelfBilling.L4_ProcessingFeePercent = -5.0m;
			organisation.LicCompany.SetMonthlyUsageDepositBalanceForTest(0m, "AUD");
			organisationBill.CalculateAll(0);
			AssertEquals(0m, organisationBill.ProcessingFeeAmount);
			AssertEquals(0m, organisationBill.ProcessingFeePercent);
			AssertEquals(140m, organisationBill.TotalDue);

			BillingTestHelper.CreateChargeCode(Factory, null, "PPBC");
			EDIDataRegistry.Instance.PrepaidBalanceChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PPBC");
			var prepaidBalanceChargeCode = EDIDataRegistry.Instance.PrepaidBalanceChargeCode.Value;
			BillingTestHelper.SetDepositBalance(organisation.PK.ToGuid(), prepaidBalanceChargeCode, 100m, 0, "AUD");
			organisation.LicCompany.SelfBilling.L4_RX_NKPredeterminedPrepaidBalanceCurrency = "AUD";
			organisation.LicCompany.SelfBilling.L4_PredeterminedPrepaidBalance = 99m;
			Factory.Save();

			organisationBill.CalculateAll(0);
			AssertEquals(-7m, organisationBill.ProcessingFeeAmount);
			AssertEquals(-5m, organisationBill.ProcessingFeePercent);
			AssertEquals(133m, organisationBill.TotalDue);

			organisation.LicCompany.SelfBilling.L4_PredeterminedPrepaidBalance = 101m;
			Factory.Save();

			organisationBill.CalculateAll(0);
			AssertEquals(0m, organisationBill.ProcessingFeeAmount);
			AssertEquals(0m, organisationBill.ProcessingFeePercent);
			AssertEquals(140m, organisationBill.TotalDue);
		}

		protected override BusinessObject GetNewBusinessObject() => NewBill();

		protected override void SetUp()
		{
			base.SetUp();

			licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			organisation = licence.Company.Header;
			user = new UsingParty(licence);
			BillingTestHelper.SetInvoicing(organisation, Env.CurrentBranch.PK, "AUD");
			organisation.CompanyData.OB_IsDebtor = true;
			var discount = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			discount.L5_Description = "DiscoCore";
			discount.L5_Discount = 10m;
			discount.L5_ModuleCode = "COR";
			discount.L5_StartDate = new ZDateTime(2010, 1, 1);

			var surcharge = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			surcharge.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			surcharge.L5_Type = BillingConstants.DiscountType.Surcharge;
			surcharge.L5_Description = "Surcharge";
			surcharge.L5_Discount = -5m;
			surcharge.L5_StartDate = new ZDateTime(2010, 1, 1);

			var priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, "COR", BillingConstants.FeeType.Module, "", 1m);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();

			CreateTestBillWithOdpl();
		}

		void CreateMfPrice(LicenceCompany licCompany)
		{
			var priceHeader1 = licCompany.PriceHeaders.AddNew();
			priceHeader1.L6_RX_NKCurrency = "AUD";
			priceHeader1.L6_ValidFrom = new ZDateTime(2016, 1, 1);
			priceHeader1.L6_TestDbPriceCode = "#NP";
			priceHeader1.L6_LiveMonthsUntilTestDbBilling = 3;
			BillingTestHelper.AddPriceItem(priceHeader1, "COR", BillingConstants.FeeType.Module, "", 1.0m);
			var mfPriceItem = BillingTestHelper.AddPriceItem(priceHeader1, "#MF", BillingConstants.FeeType.Database, "", 250m);
			mfPriceItem.L7_ChargeCode = "MFCHARGE";
		}

		OrganisationBill NewBill() => NewBill("AUD");

		OrganisationBill NewBill(ZString currencyCode) => new OrganisationBill(Factory, Env.CurrentBranch.PK, organisation.PK, currencyCode, ZDateTime.Now);

		EDIOrgHeader organisation;
		UsingParty user;
		OrganisationBill organisationBill;
		LicenceHeader licence;

		void CreateTestBillWithOdpl()
		{
			CreateTestBillWithOdpl(new ZDateTime(2010, 10, 01));
		}

		void CreateTestBillWithOdpl(ZDateTime periodStart)
		{
			var systemBill1 = new OdplSystemBill(Factory);
			var odplUsage1 = new OdplUsage(Factory, user, periodStart);
			OdplUsageTest.AddModuleUsage(odplUsage1, "COR", 100, 1);
			systemBill1.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1 });
			var systemBill2 = new DummySystemBill(organisation, "ZZ1", 50m, 5m);
			organisationBill = NewBill();
			organisationBill.DateTo = systemBill1.PeriodStart;

			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
		}

		void CreateTestBill()
		{
			CreateTestBill("AUD");
		}

		void CreateTestBill(ZString currencyCode)
		{
			var systemBill1 = new DummySystemBill(organisation, BillingConstants.BillingSystem.ODM, 100m, 10m);
			var systemBill2 = new DummySystemBill(organisation, "ZZ1", 50m, 5m);
			organisationBill = NewBill(currencyCode);
			organisationBill.DateTo = systemBill1.PeriodStart;

			organisationBill.AddSystemBill(systemBill1);
			organisationBill.AddSystemBill(systemBill2);
		}

		void SetCurrencyAndExchangeRate(SystemBill systemBill, ZString currencyCode, ZDecimal exchangeRate)
		{
			((DummyUsage)systemBill.SystemUsages[0]).CurrencyCode_Exposed = currencyCode;

			var currency = systemBill.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
			var exchange = currency.ExchangeRates.AddNew();
			exchange.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchange.RE_SellRate = exchangeRate;
			exchange.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			exchange.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference CreateInvoice(out long factoryInstance)
		{
			var invoice = organisationBill.CreateInvoice(ZDateTime.Empty);
			factoryInstance = invoice.Factory._Instance;
			return new WeakReference(invoice.Factory);
		}

		void AssertInvoiceLine(ARInvoiceLine invoiceLine, ZDecimal amount, ZString amountChargeCodeName, ZString description)
		{
			CombineAssertions(() =>
			{
				AssertEquals(amount, invoiceLine.AL_OSExTaxAmount);
				AssertEquals(amountChargeCodeName, invoiceLine.ChargeCode.AC_Code);
				AssertEquals(BillingInvoicingHelper.GetChargeCodePK(invoiceLine.Invoice.Branch, amountChargeCodeName), invoiceLine.AL_AC);
				AssertEquals(description, invoiceLine.AL_Desc);
			});
		}

		sealed class OrganisationBillForTesting : OrganisationBill
		{
			public OrganisationBillForTesting(BusinessObjectFactory factory, EDIOrgHeader org)
				: base(factory, Env.CurrentBranch.PK, org.PK, "AUD", ZDateTime.Now)
			{
			}

			public void AddAttachments_Exposed(ARInvoice invoice)
			{
				AddAttachments(invoice);
			}
		}

		sealed class DummySystemBill : SystemBill
		{
			public DummySystemBill(EDIOrgHeader organisation, string systemCode, ZDecimal amount, ZDecimal discountAmount, decimal amountExemptProcessingFee = 0m)
				: base(organisation.Factory)
			{
				OrganisationPK = organisation.PK;
				SystemCode = systemCode;
				PeriodStart = new ZDateTime(2010, 10, 01);

				Amount = amount;
				DiscountAmount = discountAmount;
				AmountExemptProcessingFee = amountExemptProcessingFee;

				DummyUsage dummyUsage = new DummyUsage(Factory, new UsingParty(Organisation), PeriodStart);
				dummyUsage.Amount_Exposed = amount;
				SystemUsages.Add(dummyUsage);
			}

			protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
			{
				lines.AddRange(DummyLines);
				base.CreateInvoiceLinesCore(lines, dateForExchangeRate, invoice);
				TextNotifications.Append("CreateInvoiceLines");
			}

			internal List<BillLine> DummyLines = new List<BillLine>();

			protected override void OnInvoiceFactorySavingCore(ARInvoice invoice)
			{
				TextNotifications.Append("OnInvoiceFactorySavingCore");
				if (ThrowExceptionOnInvoiceSaving)
				{
					throw new Exception("Christmas exception from system bill");
				}
			}
			public bool ThrowExceptionOnInvoiceSaving;

			protected override void ValidateAllCore(BusinessObject notificationOwner)
			{
				TextNotifications.Append("ValidateAllCore");
				base.ValidateAllCore(notificationOwner);
				if (AddWarning)
				{
					notificationOwner.AddRowWarning("Some row warning");
				}
			}
			public bool AddWarning;

			public override CodeDescriptionPairList GetFreeTrials()
			{
				return FreeTrials;
			}

			public CodeDescriptionPairList FreeTrials;

			#region Implementation

			public readonly ZStringBuilder TextNotifications = new ZStringBuilder();

			public bool MethodWasCalled(string methodName)
			{
				return TextNotifications.ToStringWithNewLineBetweenAppends().Contains(methodName);
			}

			#endregion
		}
	}
}
