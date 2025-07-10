using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.Testing
{
	public class AccountingVoucherPrintHelperTest : TestCaseWithFactory
	{
		public void TestGetAccountingVoucherPrintTask()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderAccountingVoucher.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var transactionHeaders = new TransactionHeader[3];
			var testARInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			var testARCreditNote = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			var testARAdjustmentNote = Factory.NewWithValidTestData(typeof(ARAdjustmentNote)) as ARAdjustmentNote;
			transactionHeaders[0] = testARInvoice;
			transactionHeaders[1] = testARCreditNote;
			transactionHeaders[2] = testARAdjustmentNote;
			var accountingVoucherPrintHelper = new AccountingVoucherPrintHelper();
			var task = accountingVoucherPrintHelper.GetAccountingVoucherPrintTask(transactionHeaders);
			AssertEquals("expect 1 document pack", 1, task.Count);
			AssertEquals("expect 3 documents in pack", 3, task.GetDocumentPacks().First().Count);
			AssertEquals("VoucherProviderDocumentSupporter", task[0].DocumentSupporter.GetType().Name);
			AssertEquals(testARInvoice, task[0].BusinessObjectToLogAgainst);
			AssertEquals("AccountingVoucher", task[0][0].MenuItem.SU_MenuName);
			AssertEquals("AccountingVoucher", task[0][1].MenuItem.SU_MenuName);
			AssertEquals("AccountingVoucher", task[0][2].MenuItem.SU_MenuName);

			DocumentsDataRegistry.Instance.UseNewDocBuilderAccountingVoucher.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.SetCountry("CN");

			task = accountingVoucherPrintHelper.GetAccountingVoucherPrintTask(transactionHeaders);
			AssertEquals("expect 1 document pack", 1, task.Count);
			AssertEquals("expect 3 documents in pack", 3, task.GetDocumentPacks().First().Count);
			AssertEquals("VoucherProviderDocumentSupporter", task[0].DocumentSupporter.GetType().Name);
			AssertEquals(testARInvoice, task[0].BusinessObjectToLogAgainst);
			AssertEquals("DocBuilder Accounting Voucher", task[0][0].MenuItem.SU_MenuName);
			AssertEquals("DocBuilder Accounting Voucher", task[0][1].MenuItem.SU_MenuName);
			AssertEquals("DocBuilder Accounting Voucher", task[0][2].MenuItem.SU_MenuName);

			GlbCompany.CurrentCompany.SetCountry("TW");

			task = accountingVoucherPrintHelper.GetAccountingVoucherPrintTask(transactionHeaders);
			AssertEquals("expect 1 document pack", 1, task.Count);
			AssertEquals("expect 3 documents in pack", 3, task.GetDocumentPacks().First().Count);
			AssertEquals("VoucherProviderDocumentSupporter", task[0].DocumentSupporter.GetType().Name);
			AssertEquals(testARInvoice, task[0].BusinessObjectToLogAgainst);
			AssertEquals("DocBuilder Accounting Voucher", task[0][0].MenuItem.SU_MenuName);
			AssertEquals("DocBuilder Accounting Voucher", task[0][1].MenuItem.SU_MenuName);
			AssertEquals("DocBuilder Accounting Voucher", task[0][2].MenuItem.SU_MenuName);
		}
	}
}
