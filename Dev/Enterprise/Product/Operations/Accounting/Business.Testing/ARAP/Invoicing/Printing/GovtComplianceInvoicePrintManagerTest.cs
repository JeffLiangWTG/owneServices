using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing.Testing
{
	public class GovtComplianceInvoicePrintManagerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPrintIDGovtComplianceInvoicesWhenConstraintsMeet()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);

			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice1.AH_GSTAmount = 0;
			TestObjectCreator.Factory.Save();
			TransactionHeader[] transactions = new TransactionHeader[] { invoice1 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.GovtTaxInvoice);

			AssertEquals("Fail to meet Indonesia constraint - Transaction must have an amount of tax.", printManager.LastGovernmentPrintTaskErrorMessage);
			AssertNull(printManager.lastPrintTaskForTesting);
		}

		public void TestPrintGovtComplianceInvoicesWithSameDebtor_NoPrinterSetup()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			TransactionHeader[] transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.GovtTaxInvoice);

			PrintTask task = printManager.lastPrintTaskForTesting;
			AssertEquals("task should have 1 docpacks", 1, task.Count);
			DocumentPack pack1 = task[0];
			AssertEquals("pack 1 should have 2 report", 2, pack1.Count);
			AssertEquals("should be Govt Compliance Inv", "Govt Compliance Inv", ((Report)pack1[0]).MenuItem.SU_MenuName);
			AssertEquals("should be Govt Compliance Crd", "Govt Compliance Crd", ((Report)pack1[1]).MenuItem.SU_MenuName);

			AssertEquals(2, pack1.NumberOfDocumentNeedToBeConsolidated);
			AssertEquals("Eagle Datamation International - BN - AUBNE - Multiple Invoices - ABIGAS - Total 2", pack1.EmailSubjectForConsolidateReports);
		}

		public void TestPrintGovtComplianceInvoicesWithDifferentDebtor_NoPrinterSetup()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			TransactionHeader[] transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.GovtTaxInvoice);

			PrintTask task = printManager.lastPrintTaskForTesting;
			AssertEquals("task should have 2 docpacks", 2, task.Count);
			DocumentPack pack1 = task[0];
			AssertEquals("pack 1 should have 1 report", 1, pack1.Count);
			AssertEquals("should be Govt Compliance Inv", "Govt Compliance Inv", ((Report)pack1[0]).MenuItem.SU_MenuName);
			AssertEquals(1, pack1.NumberOfDocumentNeedToBeConsolidated);
			AssertEquals(null, pack1.EmailSubjectForConsolidateReports);

			DocumentPack pack2 = task[1];
			AssertEquals("pack 2 should have 1 report", 1, pack2.Count);
			AssertEquals("should be Govt Compliance Crd", "Govt Compliance Crd", ((Report)pack2[0]).MenuItem.SU_MenuName);
			AssertEquals(1, pack2.NumberOfDocumentNeedToBeConsolidated);
			AssertEquals(null, pack2.EmailSubjectForConsolidateReports);
		}

		public void TestPrintStandardInvoicesWithSameDebtor_NoPrinterSetup()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			TransactionHeader[] transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.EnterpriseInvoice);

			PrintTask task = printManager.lastPrintTaskForTesting;
			AssertEquals("task should have 1 docpacks", 1, task.Count);
			DocumentPack pack1 = task[0];
			AssertEquals("pack 1 should have 2 report", 2, pack1.Count);
			AssertEquals("should be standard Inv", "Invoice", ((Report)pack1[0]).MenuItem.SU_MenuName);
			AssertEquals("should be standard Inv", "Invoice", ((Report)pack1[1]).MenuItem.SU_MenuName);

			AssertEquals(2, pack1.NumberOfDocumentNeedToBeConsolidated);
			AssertEquals("Eagle Datamation International - BN - AUBNE - Multiple Invoices - ABIGAS - Total 2", pack1.EmailSubjectForConsolidateReports);
		}

		public void TestPrintStandardInvoicesWithDifferentDebtor_NoPrinterSetup()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			TransactionHeader[] transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.EnterpriseInvoice);

			PrintTask task = printManager.lastPrintTaskForTesting;
			AssertEquals("task should have 2 docpacks", 2, task.Count);
			DocumentPack pack1 = task[0];
			AssertEquals("pack 1 should have 1 report", 1, pack1.Count);
			AssertEquals("should be Standard Inv", "Invoice", ((Report)pack1[0]).MenuItem.SU_MenuName);
			AssertEquals(1, pack1.NumberOfDocumentNeedToBeConsolidated);
			AssertEquals(null, pack1.EmailSubjectForConsolidateReports);

			DocumentPack pack2 = task[1];
			AssertEquals("pack 2 should have 1 report", 1, pack2.Count);
			AssertEquals("should be Standard Inv", "Invoice", ((Report)pack2[0]).MenuItem.SU_MenuName);
			AssertEquals(1, pack2.NumberOfDocumentNeedToBeConsolidated);
			AssertEquals(null, pack2.EmailSubjectForConsolidateReports);
		}

		public void TestPrintBothStandardAndGovtComplianceInvoicesWithSameDebtor_NoPrinterSetup()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			TransactionHeader[] transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);

			PrintTask task = printManager.lastPrintTaskForTesting;
			AssertEquals("task should have 1 docpacks", 1, task.Count);
			DocumentPack pack1 = task[0];
			AssertEquals("pack 1 should have 4 report", 4, pack1.Count);
			AssertEquals("should be Govt Compliance Inv", "Govt Compliance Inv", ((Report)pack1[0]).MenuItem.SU_MenuName);
			AssertEquals("should be standard invoice", "Invoice", ((Report)pack1[1]).MenuItem.SU_MenuName);
			AssertEquals("should be Govt Compliance Crd", "Govt Compliance Crd", ((Report)pack1[2]).MenuItem.SU_MenuName);
			AssertEquals("should be standard invoice", "Invoice", ((Report)pack1[3]).MenuItem.SU_MenuName);
		}

		public void TestPrintBothStandardAndGovtComplianceInvoicesWithDifferentDebtor_NoPrinterSetup()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			TransactionHeader[] transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);

			PrintTask task = printManager.lastPrintTaskForTesting;
			AssertEquals("task should have 2 docpacks", 2, task.Count);
			DocumentPack pack1 = task[0];
			AssertEquals("pack 1 should have 2 report", 2, pack1.Count);
			AssertEquals("should be Govt Compliance Inv", "Govt Compliance Inv", ((Report)pack1[0]).MenuItem.SU_MenuName);
			AssertEquals("should be standard invoice", "Invoice", ((Report)pack1[1]).MenuItem.SU_MenuName);
			DocumentPack pack2 = task[1];
			AssertEquals("pack 2 should have 2 report", 2, pack2.Count);
			AssertEquals("should be Govt Compliance Crd", "Govt Compliance Crd", ((Report)pack2[0]).MenuItem.SU_MenuName);
			AssertEquals("should be standard invoice", "Invoice", ((Report)pack2[1]).MenuItem.SU_MenuName);
		}

		public void TestPrintGovtComplianceInvoicesWithSameDebtor_WithPrinterSetup()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			sequenceTXI.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			sequenceTCR.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			Factory.Save();

			TransactionHeader[] transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.GovtTaxInvoice);

			AssertEquals("expect 2 transactions have been sent to printer", 2, printManager.transactionsSentToPrinterForTesting.Count);
			AssertEquals(invoice1.PK, printManager.transactionsSentToPrinterForTesting[0]);
			AssertEquals(invoice2.PK, printManager.transactionsSentToPrinterForTesting[1]);
			AssertNull(printManager.lastPrintTaskForTesting);
		}

		public void TestPrintGovtComplianceInvoicesWithDifferentDebtor_WithPrinterSetup()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			sequenceTXI.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			Factory.Save();

			TransactionHeader[] transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.GovtTaxInvoice);

			AssertEquals("expect 1 transactions have been sent to printer", 1, printManager.transactionsSentToPrinterForTesting.Count);
			AssertEquals(invoice1.PK, printManager.transactionsSentToPrinterForTesting[0]);
			AssertNotNull(printManager.lastPrintTaskForTesting);
			PrintTask task = printManager.lastPrintTaskForTesting;
			AssertEquals("task should have 1 docpacks", 1, task.Count);
			DocumentPack pack1 = task[0];
			AssertEquals("pack 1 should have 1 report", 1, pack1.Count);
			AssertEquals("should be Govt Compliance Crd", "Govt Compliance Crd", ((Report)pack1[0]).MenuItem.SU_MenuName);
		}

		public void TestPrintStandardInvoicesWithSameDebtor_WithPrinterSetup()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			sequenceTXI.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			sequenceTCR.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			Factory.Save();

			TransactionHeader[] transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.EnterpriseInvoice);

			AssertEquals("expect 0 transactions have been sent to printer", 0, printManager.transactionsSentToPrinterForTesting.Count);
			AssertNotNull(printManager.lastPrintTaskForTesting);
			PrintTask task = printManager.lastPrintTaskForTesting;
			AssertEquals("task should have 1 docpacks", 1, task.Count);
			DocumentPack pack1 = task[0];
			AssertEquals("pack 1 should have 2 report", 2, pack1.Count);
			AssertEquals("should be standard Inv", "Invoice", ((Report)pack1[0]).MenuItem.SU_MenuName);
			AssertEquals("should be standard Inv", "Invoice", ((Report)pack1[1]).MenuItem.SU_MenuName);
		}

		public void TestPrintStandardInvoicesWithDifferentDebtor_WithPrinterSetup()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			sequenceTXI.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			sequenceTCR.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			Factory.Save();

			TransactionHeader[] transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.EnterpriseInvoice);

			AssertEquals("expect 0 transactions have been sent to printer", 0, printManager.transactionsSentToPrinterForTesting.Count);
			AssertNotNull(printManager.lastPrintTaskForTesting);
			PrintTask task = printManager.lastPrintTaskForTesting;
			AssertEquals("task should have 2 docpacks", 2, task.Count);
			DocumentPack pack1 = task[0];
			AssertEquals("pack 1 should have 1 report", 1, pack1.Count);
			AssertEquals("should be standard Inv", "Invoice", ((Report)pack1[0]).MenuItem.SU_MenuName);
			DocumentPack pack2 = task[1];
			AssertEquals("pack 2 should have 1 report", 1, pack2.Count);
			AssertEquals("should be standard Inv", "Invoice", ((Report)pack2[0]).MenuItem.SU_MenuName);
		}

		public void TestPrintBothStandardAndGovtComplianceInvoicesWithSameDebtor_WithPrinterSetup()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			sequenceTXI.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			sequenceTCR.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			Factory.Save();

			TransactionHeader[] transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);

			AssertEquals("expect 2 transactions have been sent to printer", 2, printManager.transactionsSentToPrinterForTesting.Count);
			AssertEquals(invoice1.PK, printManager.transactionsSentToPrinterForTesting[0]);
			AssertEquals(invoice2.PK, printManager.transactionsSentToPrinterForTesting[1]);
			AssertNotNull(printManager.lastPrintTaskForTesting);
			PrintTask task = printManager.lastPrintTaskForTesting;
			AssertEquals("task should have 1 docpacks", 1, task.Count);
			DocumentPack pack1 = task[0];
			AssertEquals("pack 1 should have 2 report", 2, pack1.Count);
			AssertEquals("should be standard Inv", "Invoice", ((Report)pack1[0]).MenuItem.SU_MenuName);
			AssertEquals("should be standard Inv", "Invoice", ((Report)pack1[1]).MenuItem.SU_MenuName);
			AssertEquals(2, pack1.NumberOfDocumentNeedToBeConsolidated);
			AssertEquals("Eagle Datamation International - BN - AUBNE - Multiple Invoices - ABIGAS - Total 2", pack1.EmailSubjectForConsolidateReports);
		}

		public void TestPrintBothStandardAndGovtComplianceInvoicesWithDifferentDebtor_WithPrinterSetup()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			sequenceTXI.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			sequenceTCR.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			Factory.Save();

			TransactionHeader[] transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);

			AssertEquals("expect 2 transactions have been sent to printer", 2, printManager.transactionsSentToPrinterForTesting.Count);
			AssertEquals(invoice1.PK, printManager.transactionsSentToPrinterForTesting[0]);
			AssertEquals(invoice2.PK, printManager.transactionsSentToPrinterForTesting[1]);
			AssertNotNull(printManager.lastPrintTaskForTesting);
			PrintTask task = printManager.lastPrintTaskForTesting;
			AssertEquals("task should have 2 docpacks", 2, task.Count);

			DocumentPack pack1 = task[0];
			AssertEquals("pack 1 should have 1 report", 1, pack1.Count);
			AssertEquals("should be standard Inv", "Invoice", ((Report)pack1[0]).MenuItem.SU_MenuName);
			AssertEquals(1, pack1.NumberOfDocumentNeedToBeConsolidated);
			AssertEquals(null, pack1.EmailSubjectForConsolidateReports);

			DocumentPack pack2 = task[1];
			AssertEquals("pack 2 should have 1 report", 1, pack2.Count);
			AssertEquals("should be standard Inv", "Invoice", ((Report)pack2[0]).MenuItem.SU_MenuName);
			AssertEquals(1, pack2.NumberOfDocumentNeedToBeConsolidated);
			AssertEquals(null, pack2.EmailSubjectForConsolidateReports);
		}

		public void TestPrintTaskDeliveryInstructionsPK()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
			sequenceTXI.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			sequenceTCR.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			Factory.Save();
			var transactions = new TransactionHeader[] { invoice1, invoice2 };
			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
			AssertEquals("expect 2 transactions have been sent to printer", 2, printManager.transactionsSentToPrinterForTesting.Count);
			AssertEquals(invoice1.PK, printManager.transactionsSentToPrinterForTesting[0]);
			AssertEquals(invoice2.PK, printManager.transactionsSentToPrinterForTesting[1]);
			AssertNotNull(printManager.lastPrintTaskForTesting);
			var task = printManager.lastPrintTaskForTesting;
			AssertEquals("Task delivery instructions PK", printManager.ClassAInvoicePreprintedMenuPK, task.DeliveryInstructionsDefaultPK);
		}

		public void TestPrintTaskWithoutCustomNotificationWhenHasPrintQueue()
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			TestObjectCreator.Factory.Save();

			Factory.Save();
			var transactions = new TransactionHeader[] { invoice1 };

			Assert("Precondition: XD_SQ_DocumentPrintQueue should be empty.", sequenceTXI.XD_SQ_DocumentPrintQueue.IsEmpty);

			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.GovtTaxInvoice);
			AssertNull(printManager.lastPrintTaskForTesting[0].Parent.CustomNotifications);

			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
			AssertNull(printManager.lastPrintTaskForTesting[0].Parent.CustomNotifications);

			printManager.PrintGovtComplianceInvoices(transactions, GovtTaxInvoicePrintTask.EnterpriseInvoice);
			AssertNull(printManager.lastPrintTaskForTesting[0].Parent.CustomNotifications);
		}

		protected override void SetUp()
		{
			base.SetUp();
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);

			menuPK1 = TestObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Inv");
			sequenceTXI = TestObjectCreator.SetupComplianceSequence(menuPK1, "TXI", "ABC", 1, 100, 1);

			menuPK2 = TestObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Crd");
			sequenceTCR = TestObjectCreator.SetupComplianceSequence(menuPK2, "TCR", "XYZ", 1000, 2000, 1000);

			printManager = new GovtComplianceInvoicePrintManager();
		}
		protected string StoredCountry;

		protected override void TearDown()
		{
			if (!string.IsNullOrEmpty(StoredCountry) && StoredCountry != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			}

			base.TearDown();
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;

		GovtComplianceInvoicePrintManager printManager;
		ZGuid menuPK1, menuPK2;
		ARInvoice invoice1, invoice2;
		AccComplianceSequence sequenceTXI, sequenceTCR;
	}
}
