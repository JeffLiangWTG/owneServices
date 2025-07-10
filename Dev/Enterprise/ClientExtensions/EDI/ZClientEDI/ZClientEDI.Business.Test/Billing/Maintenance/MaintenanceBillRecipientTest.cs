using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Client.EDI.Billing.Business.Maintenance;
using Enterprise.Client.EDI.Billing.Business.Maintenance.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(MaintenanceBillRecipient))]
	internal class MaintenanceBillRecipientTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddNewBill()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			var recipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", ZDateTime.Now);
			var bill = recipient.AddNewBill(licHeader, licHeader.Company.InvoiceDeliveries[0]);
			AssertEquals("bill count", 1, recipient.Bills.Count);
			AssertNotNull(bill);
			AssertEquals(bill, recipient.Bills[0]);

			var bill2 = recipient.AddNewBill(licHeader, licHeader.Company.InvoiceDeliveries[0]);
			AssertEquals("bill count", 2, recipient.Bills.Count);
			AssertNotNull(bill2);
			AssertEquals(bill2, recipient.Bills[1]);
		}

		public void TestCanInvoice()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			ClientLicencePriceHeader prices = BillingTestHelper.CreatePriceList(licHeader);
			prices.L6_SystemCode = BillingConstants.BillingSystem.Maintenance;
			Factory.Save();

			var recipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", ZDateTime.Now);
			AssertEquals("!CanInvoice - no bills", false, recipient.CanInvoice);
			recipient.AddNewBill(licHeader, licHeader.Company.InvoiceDeliveries[0]);
			AssertEquals("CanInvoice", true, recipient.CanInvoice);
			licHeader.Billing.L0_NextMaintenancePercent = 99;
			recipient.AddNewBill(licHeader, licHeader.Company.InvoiceDeliveries[0]);
			AssertEquals("!CanInvoice - needs saving", false, recipient.CanInvoice);
		}

		public void TestCanForceInvoice()
		{
			BillingTestHelper.CreateChargeCode(Factory, null, "UPGASS");
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			ClientLicencePriceHeader prices = BillingTestHelper.CreatePriceList(licHeader);
			prices.L6_SystemCode = BillingConstants.BillingSystem.Maintenance;
			var fee = BillingTestHelper.CreateMaintenanceFee(licHeader.Company, "Fee 1", 111m, "UPGASS");
			Factory.Save();

			var recipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", ZDateTime.Now);
			recipient.AddNewBill(licHeader, licHeader.Company.InvoiceDeliveries[0]);
			recipient.AddNewFee(fee, ZDateTime.Empty, licHeader.Company.InvoiceDeliveries[0], Enumerable.Empty<ClientChargeableUsage>());
			Assert("not invoiced so can't force another", !recipient.Fees[0].CanForceInvoice);
			Assert("not invoiced so can't force another", !recipient.Bills[0].CanForceInvoice);
			Assert("can't be forced since no charge can be forced", !recipient.CanForceInvoice);

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = licHeader.Company.LC_OH;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_RX_NKTransactionCurrency = "AUD";

			recipient.Fees[0].PopulateInvoice(invoice);

			Assert("is invoiced so can force another", recipient.Fees[0].CanForceInvoice);
			Assert("is not invoiced so can't force another", !recipient.Bills[0].CanForceInvoice);
			Assert("can be forced since at least one charge can be forced", recipient.CanForceInvoice);

			var delivery2 = licHeader.Company.InvoiceDeliveries.AddNew();
			delivery2.L9_IsBilled = false;
			recipient.AddNewBill(licHeader, delivery2);

			Assert("can't invoice since not billed", !recipient.Bills[1].CanInvoice);
			Assert("can't be forced since a charge can't be invoiced", !recipient.CanForceInvoice);
		}

		[TestDate(2011, 6, 1)]
		public void TestCreateInvoice()
		{
			RefCurrency usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			RefExchangeRate usdExchange = usd.ExchangeRates.AddNew();
			usdExchange.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			usdExchange.RE_SellRate = 2;
			usdExchange.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			usdExchange.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);
			Factory.Save();

			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			ClientLicencePriceHeader prices = BillingTestHelper.CreatePriceList(licHeader);
			var fee = BillingTestHelper.CreateMaintenanceFee(licHeader.Company, "Fee", 100m, "UPGASS");
			fee.L8_RX_NKCurrency = "USD";
			prices.L6_SystemCode = BillingConstants.BillingSystem.Maintenance;
			Factory.Save();

			// Maintenance + Upgrade Assurance invoice
			ZDateTime dueDate = ZDateTime.Now.Date.ToZDateTime().AddDays(60);
			var recipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", dueDate, ZDateTime.Today);
			MaintenanceBillForTest bill = new MaintenanceBillForTest(licHeader, recipient, licHeader.Company.InvoiceDeliveries[0]);
			recipient.Bills.Add(bill);
			FeeDeliveryForTest feeDelivery = new FeeDeliveryForTest(Factory, recipient, fee, null);
			recipient.Fees.Add(feeDelivery);
			ARInvoice invoice = recipient.CreateInvoices(null).FirstOrDefault();
			AssertEquals(MaintenanceBill.GetInvoiceDescription(recipient.Bills.ToArray<MaintenanceBill>()), invoice.AH_Desc);
			AssertEquals("AH_OH", licHeader.Company.LC_OH, invoice.AH_OH);
			AssertEquals("AH_DueDate", TestDateAttribute.Date, invoice.AH_DueDate);
			AssertEquals("AH_TransactionCategory", Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice, invoice.AH_TransactionCategory);
			AssertEquals("AH_RX_NKTransactionCurrency", "AUD", invoice.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_GB", Env.CurrentBranch.PK, invoice.AH_GB);
			AssertEquals("AH_ExchangeRate", 1m, invoice.AH_ExchangeRate);
			AssertEquals("bill.PopulateInvoiceCalled", 1, bill.PopulateInvoiceCalled);
			Assert("line count", invoice.Lines.Count > 0);

			AssertEquals("Exchange rate", "Exchange rate: 1 USD = 0.50000 AUD", invoice.Lines[invoice.Lines.Count - 1].AL_Desc);
			AssertEquals(EDIDataRegistry.Instance.CommentChargeCode.Value, invoice.Lines[invoice.Lines.Count - 1].ChargeCode.AC_Code);

			AssertEquals("fee.PopulateInvoiceCalled", 1, feeDelivery.PopulateInvoiceCalled);

			// Upgrade assurance only invoice
			dueDate = ZDateTime.Now.Date.ToZDateTime().AddDays(-10);
			recipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", dueDate, ZDateTime.Today);
			feeDelivery = new FeeDeliveryForTest(Factory, recipient, fee, null);
			recipient.Fees.Add(feeDelivery);
			invoice = recipient.CreateInvoices(null).FirstOrDefault();
			AssertEquals("Upgrade Assurance - " + dueDate.ToString("MMMM yyyy"), invoice.AH_Desc);
			AssertEquals("AH_OH", licHeader.Company.LC_OH, invoice.AH_OH);
			AssertEquals("AH_DueDate is today if due date in the past", ZDateTime.Today.Date, invoice.AH_DueDate);
			Assert(invoice.IsInDatabase);

			var mock = new Mock<ITaxProcessor>();
			mock.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns("ProcessTaxesOnPosting");
			using (ObjectFactory.Substitute(mock.Object))
			{
				var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
				config.ETC_ParentId = invoice.Company.PK;
				Factory.Save();
				AssertExceptionThrown<InvalidOperationException>("AssertExceptionThrown", "ProcessTaxesOnPosting", () => recipient.CreateInvoices(null));
			}
		}

		[TestDate(2022, 6, 1)]
		public void TestCreateInvoice_SplitInvoice()
		{
			BillingTestHelper.CreateChargeCode(Factory, null, "UPGASS");
			RefCurrency usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			RefExchangeRate usdExchange = usd.ExchangeRates.AddNew();
			usdExchange.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			usdExchange.RE_SellRate = 2;
			usdExchange.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			usdExchange.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);
			var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			var prices = BillingTestHelper.CreatePriceList(licHeader);
			var fee = BillingTestHelper.CreateMaintenanceFee(licHeader.Company, "Fee", 100m, "UPGASS");
			fee.L8_RX_NKCurrency = "USD";
			prices.L6_SystemCode = BillingConstants.BillingSystem.Maintenance;
			Factory.Save();

			// Maintenance + Upgrade Assurance invoice
			ZDateTime dueDate = ZDateTime.Now.Date.ToZDateTime().AddDays(60);
			var recipient = new MaintenanceBillRecipientForTest(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", dueDate, ZDateTime.Today);
			MaintenanceBillForTest bill = new MaintenanceBillForTest(licHeader, recipient, licHeader.Company.InvoiceDeliveries[0]);
			recipient.Bills.Add(bill);
			FeeDeliveryForTest feeDelivery = new FeeDeliveryForTest(Factory, recipient, fee, null);
			feeDelivery.CallBasePopulateInvoice = true;
			recipient.Fees.Add(feeDelivery);

			var invoices = recipient.CreateInvoices(null).ToArray();
			AssertEquals(2, invoices.Length);

			foreach (var invoice in invoices)
			{
				AssertEquals("AH_OH", licHeader.Company.LC_OH, invoice.AH_OH);
				AssertEquals("AH_DueDate", TestDateAttribute.Date, invoice.AH_DueDate);
				AssertEquals("AH_TransactionCategory", Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice, invoice.AH_TransactionCategory);
				AssertEquals("AH_RX_NKTransactionCurrency", "AUD", invoice.AH_RX_NKTransactionCurrency);
				AssertEquals("AH_GB", Env.CurrentBranch.PK, invoice.AH_GB);
				AssertEquals("AH_ExchangeRate", 1m, invoice.AH_ExchangeRate);
				Assert("line count", invoice.Lines.Count > 0);
				AssertEquals(true, Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_AH_Invoice, invoice.PK)).Any());
			}

			AssertEquals("ediEnterprise Application Services Renewal - June 2022 - 1 of 2", invoices[0].AH_Desc);
			AssertEquals("ediEnterprise Application Services Renewal - June 2022 - 2 of 2", invoices[1].AH_Desc);

			AssertEquals(4.5m, invoices[0].AH_OSTotalAmount);
			AssertEquals(50.0m, invoices[1].AH_OSTotalAmount);

			AssertEquals("00001000, 00001001", recipient.InvoiceNumbers);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MaintenanceBillRecipient(Factory, ZGuid.Empty, ZGuid.Empty, "", ZDateTime.Now);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BillingTestHelper.CreateChargeCode(Factory, null, "ANNMAINT");
			var chargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, null, EDIDataRegistry.Instance.CommentChargeCode.Value);
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Factory.Save();
		}

		class MaintenanceBillRecipientForTest : MaintenanceBillRecipient
		{
			public MaintenanceBillRecipientForTest(BusinessObjectFactory factory, ZGuid branchPK, ZGuid orgPK, ZString currencyCode, ZDateTime dateForExchangeRate) : base(factory, branchPK, orgPK, currencyCode, dateForExchangeRate)
			{
			}

			public MaintenanceBillRecipientForTest(BusinessObjectFactory factory, ZGuid branchPK, ZGuid orgPK, ZString currencyCode, ZDateTime dueDate, ZDateTime dateForExchangeRate) : base(factory, branchPK, orgPK, currencyCode, dueDate, dateForExchangeRate)
			{
			}

			public MaintenanceBillRecipientForTest(BusinessObjectFactory factory, ZGuid branchPK, EDIOrgHeader org, LicenceCompany licCompany, ClientLicenceBilling licBilling, ZDateTime dueDate, ZString currencyCode, ZDecimal localExchangeRate, ZDateTime dateForExchangeRate, EDIOrgHeader invoicingPartner) : base(factory, branchPK, org, licCompany, licBilling, dueDate, currencyCode, localExchangeRate, dateForExchangeRate, invoicingPartner)
			{
			}

			protected override EDIARInvoiceTaxProcessor GetInvoiceTaxProcessor() => new EDIARInvoiceTaxProcessorTest.EDIARInvoiceTaxProcessorForTest();
		}

		#endregion
	}
}
