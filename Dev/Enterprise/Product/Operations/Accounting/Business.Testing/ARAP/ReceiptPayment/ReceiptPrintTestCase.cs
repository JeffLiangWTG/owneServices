using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public class ReceiptPrintTestCase : TestCaseWithFactory
	{
		public void TestPrintReceiptMatchingReportUsesDocBuilderTemplate()
		{
			Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderReceiptDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderMatchingDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptJournal, AccountingUtils.AccountingDocumentTitles.ReceiptMatching);
			PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptMatching, AccountingUtils.AccountingDocumentTitles.ReceiptMatching);

			Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderReceiptDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptJournal, AccPrintingUtility.DocBuilderReceiptDocumentName);
			PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptMatching, AccountingUtils.AccountingDocumentTitles.ReceiptMatching);

			Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderMatchingDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptJournal, AccPrintingUtility.DocBuilderReceiptDocumentName);
			PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptMatching, AccPrintingUtility.DocBuilderMatchingDocumentName);
		}

		public void TestPrintUsesDocBuilderTemplate()
		{
			Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderReceiptDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderMatchingDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptJournal, AccountingUtils.AccountingDocumentTitles.ReceiptMatching);
			PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptMatching, AccountingUtils.AccountingDocumentTitles.ReceiptMatching);

			Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderReceiptDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			PrintAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptJournal, AccPrintingUtility.DocBuilderReceiptDocumentName, 1);
			PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptMatching, AccountingUtils.AccountingDocumentTitles.ReceiptMatching);

			Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderMatchingDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			PrintAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptJournal, AccPrintingUtility.DocBuilderReceiptDocumentName, 1);
			PrintAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptMatching, AccPrintingUtility.DocBuilderMatchingDocumentName, 1);

			var receipt2 = Factory.NewWithValidTestData<ARReceipt>();
			Factory.Save();
			AssertEquals("Both Receipts are for the same Org", Receipt.AH_OH, receipt2.AH_OH);

			PrintAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptJournal, AccPrintingUtility.DocBuilderReceiptDocumentName, 1, receipt2);
			PrintAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptMatching, AccPrintingUtility.DocBuilderMatchingDocumentName, 1, receipt2);

			var receipt3 = Factory.NewWithValidTestData<ARReceipt>();
			var org = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>();
			receipt3.AH_OH = org.PK;
			Factory.Save();

			PrintAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptJournal, AccPrintingUtility.DocBuilderReceiptDocumentName, 2, receipt3);
			PrintAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptMatching, AccPrintingUtility.DocBuilderMatchingDocumentName, 2, receipt3);
		}

		public void TestNoExceptionThrowWhenInvalidDuplicateDocBuilderMenuExists()
		{
			var arReceipt = Factory.NewWithValidTestData<ARReceipt>();
			var menu1 = TestObjectCreator.CreateDuplicateMenuItem("DocBuilder Receipt Document", arReceipt);
			var menu2 = TestObjectCreator.CreateDuplicateMenuItem("DocBuilder Matching Document", arReceipt);
			Factory.Save();

			DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderReceiptDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderMatchingDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptJournal, AccountingUtils.AccountingDocumentTitles.ReceiptMatching);
			PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptMatching, AccountingUtils.AccountingDocumentTitles.ReceiptMatching);

			DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderReceiptDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderMatchingDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertNoExceptionThrown("Expect no exception even if there is an invalid duplicate menu", () => PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptJournal, AccPrintingUtility.DocBuilderReceiptDocumentName));
			AssertNoExceptionThrown("Expect no exception even if there is an invalid duplicate menu", () => PrintReceiptMatchingReportAndAssertTemplate(AccountingUtils.AccountingDocumentTitles.ReceiptMatching, AccPrintingUtility.DocBuilderMatchingDocumentName));
		}

		#region Implementation

		void PrintReceiptMatchingReportAndAssertTemplate(string templateName, string expectedTemplateName)
		{
			Print.Test_TemplateNames.Clear();
			Print.PrintReceiptMatchingReport(Receipt, templateName);
			Assert(string.Format("For given '{0}' should use '{1}' template", templateName, expectedTemplateName), Print.Test_TemplateNames.Contains(expectedTemplateName));
		}

		void PrintAndAssertTemplate(string templateName, string expectedTemplateName, int expectedNumberOfPacks = 0, params ARReceipt[] additionalReceipts)
		{
			Print.Test_TemplateNames.Clear();

			var printTasks = new List<PrintTask>(Print.PaymentDocumentPacks.Values);
			Print.PaymentDocumentPacks.Clear();
			foreach (var task in printTasks)
			{
				task.Dispose();
			}
			printTasks.Clear();

			var pks = new List<Guid>();
			pks.Add(Receipt.PK.ToGuid());
			pks.AddRange(additionalReceipts.Select(x => x.PK.ToGuid()));

			Print.Print(templateName, pks.ToArray());
			Assert(string.Format("For given '{0}' should use '{1}' template", templateName, expectedTemplateName), Print.Test_TemplateNames.Contains(expectedTemplateName));
			AssertEquals("Expected number of DocumentPacks", expectedNumberOfPacks, Print.PaymentDocumentPacks[expectedTemplateName].Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Receipt = Factory.NewWithValidTestData<ARReceipt>();
			Factory.Save();

			Print = new ReceiptPrint();
		}

		protected override void TearDown()
		{
			base.TearDown();
			Print.Dispose();
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		ARReceipt Receipt;
		ReceiptPrint Print;

		#endregion
	}
}