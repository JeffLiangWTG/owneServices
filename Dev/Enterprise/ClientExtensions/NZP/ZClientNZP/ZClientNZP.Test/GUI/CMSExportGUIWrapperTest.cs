using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Client.NZP.CMS;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.NZP.GUI.Testing
{
	[TestedType(typeof(CMSExportGUIWrapper))]
	public class CMSExportGUIWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDataExporter()
		{
			AccountingTransactionsDataExporter exporter = CMSExportGUIWrapper.DataExporter;
			AssertEquals("Expected type of CMSCashSalesExporter", typeof(CMSCashSalesExporter), exporter.GetType());
			AssertSame("Data Exporter should be lazy loaded", exporter, CMSExportGUIWrapper.DataExporter);
		}

		public void TestFromAndToDatesAreCorrectlySet()
		{
			NZPDataRegistry.Instance.CMSLastDateExported = new ZDateTime(2006, 10, 10);
			CMSExportGUIWrapper.SetFromAndToDates();
			AssertEquals(NZPDataRegistry.Instance.CMSLastDateExported, CMSExportGUIWrapper.DateFrom);
			AssertEquals(new ZDateTime(2006, 10, 16).AddDays(1).AddMilliseconds(-1), CMSExportGUIWrapper.DateTo);
		}

		[TestDate(2006, 5, 11, 8, 39, 47)]
		[SuspendINTransactionHeaderHasPostedLinesCriticalValidation]
		public void TestExport()
		{
			NZPDataRegistry.Instance.CMSLastDateExported = new ZDateTime(2006, 5, 2);
			CMSExportGUIWrapper = new CMSExportGUIWrapperTestClass(Factory);
			TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory, false);

			helper.ARInvoice = (ARInvoice)helper.PopulateInvoice(typeof(ARInvoice), 100.00M, 10.00M, 0.50M, helper.ObjectCreator.USD);
			helper.ARInvoice.AH_PostDate = new ZDateTime(2006, 5, 4, 17, 23, 53);
			helper.ARInvoice.AH_InvoiceDate = new ZDateTime(2006, 5, 3, 12, 45, 12);
			helper.ARInvoice.AH_DueDate = new ZDateTime(2006, 5, 5, 5, 2, 8);
			Factory.Save();
			AssertEquals("DateFrom should be ActualLastDate", NZPDataRegistry.Instance.CMSLastDateExported, CMSExportGUIWrapper.DateFrom);
			AssertEquals("DateTo Should be CMSLastDate plus 6 days", new ZDateTime(2006, 5, 8).AddDays(1).AddMilliseconds(-1), CMSExportGUIWrapper.DateTo);
			CMSExportGUIWrapper.Export();
			AssertExportOfFiles();
			AssertEquals("Last Date Exported should be DateTo", new DateTime(2006, 5, 8), NZPDataRegistry.Instance.CMSLastDateExported);
			AssertEquals("DateFrom on wrapper should be updated to today", new DateTime(2006, 5, 8), CMSExportGUIWrapper.DateFrom);
			AssertEquals("DateTo on wrapper should be updated Today", new DateTime(2006, 5, 11).AddDays(1).AddMilliseconds(-1), CMSExportGUIWrapper.DateTo);
		}

		[TestDate(2006, 5, 11, 8, 39, 47)]
		[SuspendINTransactionHeaderHasPostedLinesCriticalValidation]
		public void TestExport_Today()
		{
			NZPDataRegistry.Instance.CMSLastDateExported = new ZDateTime(2006, 5, 6);
			CMSExportGUIWrapper = new CMSExportGUIWrapperTestClass(Factory);
			TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory, false);
			helper.ARInvoice = (ARInvoice)helper.PopulateInvoice(typeof(ARInvoice), 100.00M, 10.00M, 0.50M, helper.ObjectCreator.USD);
			helper.ARInvoice.AH_PostDate = new ZDateTime(2006, 5, 11, 17, 23, 53);
			helper.ARInvoice.AH_InvoiceDate = new ZDateTime(2006, 5, 3, 12, 45, 12);
			helper.ARInvoice.AH_DueDate = new ZDateTime(2006, 5, 5, 5, 2, 8);
			Factory.Save();

			AssertEquals("DateFrom should be ActualLastDate", NZPDataRegistry.Instance.CMSLastDateExported, CMSExportGUIWrapper.DateFrom);
			AssertEquals("DateTo Should be today", new ZDateTime(2006, 5, 11).AddDays(1).AddMilliseconds(-1), CMSExportGUIWrapper.DateTo);
			CMSExportGUIWrapper.Export();
			AssertExportOfFiles();
			AssertEquals("Last Date Exported should be today", new DateTime(2006, 5, 11), NZPDataRegistry.Instance.CMSLastDateExported);
			AssertEquals("DateFrom on wrapper should be updated to today", new DateTime(2006, 5, 11), CMSExportGUIWrapper.DateFrom);
			AssertEquals("DateTo on wrapper should be updated Today", new DateTime(2006, 5, 11).AddDays(1).AddMilliseconds(-1), CMSExportGUIWrapper.DateTo);
		}

		[TestDate(2006, 5, 11, 8, 39, 47)]
		public void TestExport_BatchNumberNotEqualToZero()
		{
			NZPDataRegistry.Instance.CMSLastDateExported = new ZDateTime(2006, 5, 1);
			CMSExportGUIWrapper = new CMSExportGUIWrapperTestClass(Factory);
			TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory);
			helper.ObjectCreator.CreateGenExportBatchSequenceHeader(1, helper.Invoice.PK, 1);
			Factory.Save();
			AssertEquals("DateFrom should be ActualLastDate", NZPDataRegistry.Instance.CMSLastDateExported, CMSExportGUIWrapper.DateFrom);
			AssertEquals("DateTo should be the nominal date plus 6 days", new ZDateTime(2006, 5, 7).AddDays(1).AddMilliseconds(-1), CMSExportGUIWrapper.DateTo);
			CMSExportGUIWrapper.ExistingBatchNumberToExport = 1;
			CMSExportGUIWrapper.Export();
			AssertExportOfFiles();
			AssertEquals("CMSLastDateExported should not be updated", new ZDateTime(2006, 5, 1), NZPDataRegistry.Instance.CMSLastDateExported);
			AssertEquals("From Date should not be updated", NZPDataRegistry.Instance.CMSLastDateExported, CMSExportGUIWrapper.DateFrom);
			AssertEquals("To Date should not be updated", new ZDateTime(2006, 5, 7).AddDays(1).AddMilliseconds(-1), CMSExportGUIWrapper.DateTo);
		}

		[TestDate(2005, 11, 8, 13, 56, 23)]
		public void TestExportWithInvalidExportDirectory()
		{
			ZDateTime lastExportDate = new ZDateTime(2005, 1, 1, 12, 12, 12);
			NZPDataRegistry.Instance.CMSLastDateExported = lastExportDate;
			UnitTestUserNotification notification = UnitTestUserNotification.Instance;
			notification.ClearMessagesAndAnswers();
			NZPDataRegistry.Instance.CMSExportDirectory = "blah";
			CMSExportGUIWrapper.Export();
			string expectedMessage = string.Format("CMS Export Directory: {0} is not specified or does not exists.{1}Please specify a valid Export Directory in Registry -> NZ Post Client Extensions", "blah", System.Environment.NewLine);
			Assert("Cash Sales File should not have been created/exported", !File.Exists(CashSalesFile));
			Assert("Credit Sales File should not have been created/exported", !File.Exists(CreditSalesFile));
			Assert("Crdit Notes File should not have been created/exported", !File.Exists(CreditNotesFile));
			AssertEquals("Last Date Export should not be updated", lastExportDate, NZPDataRegistry.Instance.CMSLastDateExported);
			AssertEquals("Error Message", expectedMessage, notification.LastMessage.Text);
		}

		[TestDate(2015, 7, 10)]
		public void TestDateFrom()
		{
			NZPDataRegistry.Instance.CMSLastDateExported = new ZDateTime();
			CMSExportGUIWrapper.SetFromAndToDates();
			AssertEquals(false, NZPDataRegistry.Instance.CMSLastDateExported.IsValid);
			AssertEquals(new ZDateTime(2015, 7, 10).AddDays(-6), CMSExportGUIWrapper.DateFrom);
			CMSExportGUIWrapper.DateFrom = new ZDate(2006, 10, 17);
			AssertEquals(new ZDateTime(2006, 10, 17), CMSExportGUIWrapper.DateFrom);
			AssertEquals(false, NZPDataRegistry.Instance.CMSLastDateExported.IsValid);
		}

		[TestDate(2006, 10, 17)]
		public void TestDateTo()
		{
			NZPDataRegistry.Instance.CMSLastDateExported = new ZDateTime();
			CMSExportGUIWrapper.SetFromAndToDates();
			AssertEquals(false, NZPDataRegistry.Instance.CMSLastDateExported.IsValid);
			AssertEquals(new ZDateTime(2006, 10, 17).AddDays(1).AddMilliseconds(-1), CMSExportGUIWrapper.DateTo);
			CMSExportGUIWrapper.DateTo = new ZDate(2006, 1, 1);
			AssertEquals(new ZDateTime(2006, 1, 1).AddDays(1).AddMilliseconds(-1), CMSExportGUIWrapper.DateTo);
			CMSExportGUIWrapper.DateTo = new ZDate(2006, 10, 17);
			AssertEquals(new ZDateTime(2006, 10, 17).AddDays(1).AddMilliseconds(-1), CMSExportGUIWrapper.DateTo);
			CMSExportGUIWrapper.DateTo = new ZDate(2006, 10, 20);
			AssertEquals(new ZDateTime(2006, 10, 17).AddDays(1).AddMilliseconds(-1), CMSExportGUIWrapper.DateTo);
			AssertEquals(false, NZPDataRegistry.Instance.CMSLastDateExported.IsValid);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CMSExportGUIWrapper gUIWrapper = new CMSExportGUIWrapper(Factory);
			return gUIWrapper;
		}

		void AssertExportOfFiles()
		{
			Assert("Cash Sales File was not created/exported", File.Exists(CashSalesFile));
			Assert("Credit Sales File was not created/exported", File.Exists(CreditSalesFile));
			Assert("Crdit Notes File was not created/exported", File.Exists(CreditNotesFile));
			Assert("Progress Form was not shown", CMSExportGUIWrapper.ProgressFormWasShown);
			Assert("Progress Form was not disposed", CMSExportGUIWrapper.ProgressForm.IsDisposed);
		}

		#region Setup
		string CashSalesFile;
		string CreditSalesFile;
		string CreditNotesFile;
		string ExportDirectory;
		protected override void SetUp()
		{
			base.SetUp();
			NZPDataRegistry.Instance.CMSLastDateExported = ZDateTime.Now;
			CMSExportGUIWrapper = new CMSExportGUIWrapperTestClass(Factory);
			ExportDirectory = Path.Combine(Env.TempPath, "CMS");
			CashSalesFile = Path.Combine(ExportDirectory, "200605110839_DIEL_T_I.lst");
			CreditSalesFile = Path.Combine(ExportDirectory, "200605110839_DIEL_S_I.lst");
			CreditNotesFile = Path.Combine(ExportDirectory, "200605110839_DIEL_X_I.lst");
			NZPDataRegistry.Instance.CMSExportDirectory = ExportDirectory;
			CreateDirectory(ExportDirectory);
		}

		#endregion
		#region TearDown
		protected override void TearDown()
		{
			TempDirectory.DeleteDirectory(ExportDirectory);
			base.TearDown();
		}

		#endregion
		#region CMSExportGUIWrapperTestClass
		CMSExportGUIWrapperTestClass CMSExportGUIWrapper;
		class CMSExportGUIWrapperTestClass : CMSExportGUIWrapper
		{
			public CMSExportGUIWrapperTestClass(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new AccountingTransactionsDataExporter DataExporter
			{
				get
				{
					return base.DataExporter;
				}
			}

			public new ProgressForm ProgressForm
			{
				get
				{
					return base.ProgressForm;
				}
			}

			public bool ProgressFormWasShown;
			protected override void ShowProgressForm()
			{
				base.ShowProgressForm();
				ProgressFormWasShown = true;
			}

			public new void SetFromAndToDates()
			{
				base.SetFromAndToDates();
			}
		}
		#endregion
	}
}
