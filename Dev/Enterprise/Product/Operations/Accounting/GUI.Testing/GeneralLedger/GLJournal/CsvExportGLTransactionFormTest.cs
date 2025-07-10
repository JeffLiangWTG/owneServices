using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
#if WINZOR
using CargoWise.Application;
using Enterprise.Integration.RemoteDesktopServices;
using Moq;
#endif
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(CsvExportGLTransactionForm))]
	public class CsvExportGLTransactionFormTest : ZFormBasherTest
	{
		public void TestFormVerbAndCaption()
		{
			using (CsvExportGLTransactionForm form = new CsvExportGLTransactionForm(new GLTransactionBusinessObject(Factory)))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Form verb", "", form.FormVerb);
				AssertEquals("Form caption", "Export GL Transactions", form.FormCaption);
			}
		}

		public void TestBrowseButton_Click()
		{
			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			using (CsvExportGLTransactionForm form = new CsvExportGLTransactionForm(bizObj))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.PathToSelectInShowCommonDialog = bizObj.ExportDirectory;
				form.BrowseButton_Click(null, null);

				CommonDialog dialog = ZFormModaliser.LastCommonDialogShownDialogForTest;
				AssertEquals("Dialog Type", typeof(FolderBrowserDialog), dialog.GetType());
				AssertEquals("Selected Path", bizObj.ExportDirectory, ((FolderBrowserDialog)dialog).SelectedPath);
			}
		}

		[TestDate(2006, 03, 29)]
		public void TestExportButton_Click()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));

#if WINZOR
			var mappedClientPathMock = new Mock<IMappedClientPath>();
			mappedClientPathMock.Setup(m => m.GetMappedPath(It.IsAny<string>())).Returns((string path) => path);
			ObjectFactory.Substitute(mappedClientPathMock.Object);
#endif

			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			bizObj.StartGLAccountPK = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			bizObj.EndGLAccountPK = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;

			using (CsvExportGLTransactionFormForTest form = new CsvExportGLTransactionFormForTest(bizObj))
			{
				form.Show();

				AssertEquals("PrimaryFiltersGroupBox should be enabled before export", true, form.PrimaryFiltersGroupBox.Enabled);
				AssertEquals("SortByGroupBox should be enabled before export", true, form.SortByGroupBoxExposed.Enabled);
				AssertEquals("DirectoryGroupBox should be enabled before export", true, form.DirectoryGroupBoxExposed.Enabled);
				AssertEquals("ExportButton should be enabled before export", true, form.ExportButton.Enabled);
				AssertEquals("CancelButton should be enabled before export", true, form.CancelButton.Enabled);
				AssertEquals("ExportButton should be visible before export", true, form.ExportButton.Visible);
				AssertEquals("CancelButton's text should be 'Cancel' before export", "Cancel", form.CancelButton.Text);

				try
				{
					form.ExportButton_Click(this, null);

					AssertEquals("PrimaryFiltersGroupBox should not be enabled before export", false, form.PrimaryFiltersGroupBox.Enabled);
					AssertEquals("SortByGroupBox should not be enabled before export", false, form.SortByGroupBoxExposed.Enabled);
					AssertEquals("DirectoryGroupBox should not be enabled before export", false, form.DirectoryGroupBoxExposed.Enabled);
					AssertEquals("ExportButton should not be enabled after export", false, form.ExportButton.Enabled);
					AssertEquals("CancelButton should be enabled after export", true, form.CancelButton.Enabled);
					AssertEquals("ExportButton should not be visible after export", false, form.ExportButton.Visible);
					AssertEquals("CancelButton's text should be 'Close' after export", "Close", form.CancelButton.Text);
				}
				finally
				{
					DeleteIfExists(Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329000000.csv"));
					GC.Collect();
				}
			}
		}

		[TestDate(2006, 03, 29)]
		public void TestExportButton_ClickForBatchExport()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));

#if WINZOR
			var mappedClientPathMock = new Mock<IMappedClientPath>();
			mappedClientPathMock.Setup(m => m.GetMappedPath(It.IsAny<string>())).Returns((string path) => path);
			ObjectFactory.Substitute(mappedClientPathMock.Object);
#endif

			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			bizObj.StartGLAccountPK = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			bizObj.EndGLAccountPK = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;

			using (CsvExportGLTransactionFormForTest form = new CsvExportGLTransactionFormForTest(bizObj))
			{
				form.Show();

				AssertEquals("PrimaryFiltersGroupBox should be enabled before export", true, form.PrimaryFiltersGroupBox.Enabled);
				AssertEquals("SortByGroupBox should be enabled before export", true, form.SortByGroupBoxExposed.Enabled);
				AssertEquals("BatchGroupBox should be enabled before export", true, form.BatchGroupBoxExposed.Enabled);
				AssertEquals("DirectoryGroupBox should be enabled before export", true, form.DirectoryGroupBoxExposed.Enabled);
				AssertEquals("ExportButton should be enabled before export", true, form.ExportButton.Enabled);
				AssertEquals("CancelButton should be enabled before export", true, form.CancelButton.Enabled);
				AssertEquals("ExportButton should be visible before export", true, form.ExportButton.Visible);
				AssertEquals("CancelButton's text should be 'Cancel' before export", "Cancel", form.CancelButton.Text);

				string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329000000 " + GlbCompany.CurrentCompany.GC_Code + " batch 1001.csv");
				try
				{
					AssertEquals("Precondition: ", 0, bizObj.BatchNumber);
					bizObj.CreateAndExportBatch = true;

					AssertEquals("PrimaryFiltersGroupBox should be enabled before export", true, form.PrimaryFiltersGroupBox.Enabled);
					AssertEquals("SortByGroupBox should be enabled before export", true, form.SortByGroupBoxExposed.Enabled);
					AssertEquals("BatchGroupBox should be enabled before export", true, form.BatchGroupBoxExposed.Enabled);
					AssertEquals("DirectoryGroupBox should be enabled before export", true, form.DirectoryGroupBoxExposed.Enabled);
					AssertEquals("ExportButton should be enabled before export", true, form.ExportButton.Enabled);
					AssertEquals("CancelButton should be enabled before export", true, form.CancelButton.Enabled);
					AssertEquals("ExportButton should be visible before export", true, form.ExportButton.Visible);
					AssertEquals("CancelButton's text should be 'Cancel' before export", "Cancel", form.CancelButton.Text);

					form.ExportButton_Click(this, null);

					bizObj.FromPeriod = 0;
					bizObj.ToPeriod = 0;
					bizObj.StartGLAccountPK = ZGuid.Empty;
					bizObj.EndGLAccountPK = ZGuid.Empty;

					ZGuid glAccount = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.Equal, "4710.00.00")).PK;
					AccGLHeader apSuspenseControl = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountType, "BSH"));
					TestObjectCreator testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
					Invoice invoice = (Invoice)testObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany.LocalCurrency.CurrentBuyRate);

					AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apSuspenseControl.PK.ToGuid());

					invoice.AH_InvoiceAmount = 333m;
					invoice.AH_AG = glAccount;
					invoice.AH_PostDate = new ZDateTime(2006, 3, 10);
					invoice.AH_Ledger = "AP";
					invoice.AH_TransactionType = "INV";
					invoice.AH_PostToGL = "Y";
					invoice.AH_ExchangeRate = 1m;

					InvoiceLine invoiceLine = (InvoiceLine)testObjectCreator.CreateInvoiceLine(invoice, 333m, GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany.LocalCurrency.CurrentBuyRate);

					invoiceLine.AL_AG = glAccount;
					invoiceLine.AL_LineAmount = 333m;
					invoiceLine.AL_OSAmount = 333m;
					invoiceLine.AL_PostDate = new ZDateTime(2006, 3, 10);
					AssertNotNull("reverse should be set when post is set due to new reve recognition", invoiceLine.AL_ReverseDate);

					testObjectCreator.Factory.Save();

					form.ExportButton_Click(this, null);

					AssertEquals("PrimaryFiltersGroupBox should not be enabled before export", false, form.PrimaryFiltersGroupBox.Enabled);
					AssertEquals("SortByGroupBox should not be enabled before export", false, form.SortByGroupBoxExposed.Enabled);
					AssertEquals("BatchGroupBox should be enabled before export", false, form.BatchGroupBoxExposed.Enabled);
					AssertEquals("DirectoryGroupBox should not be enabled before export", false, form.DirectoryGroupBoxExposed.Enabled);
					AssertEquals("ExportButton should not be enabled after export", false, form.ExportButton.Enabled);
					AssertEquals("CancelButton should be enabled after export", true, form.CancelButton.Enabled);
					AssertEquals("ExportButton should not be visible after export", false, form.ExportButton.Visible);
					AssertEquals("CancelButton's text should be 'Close' after export", "Close", form.CancelButton.Text);
					AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));
				}
				finally
				{
					DeleteIfExists(exportFileName);
					GC.Collect();
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new CsvExportGLTransactionForm(new GLTransactionBusinessObject(Factory));
		}

		class CsvExportGLTransactionFormForTest : CsvExportGLTransactionForm
		{
			public CsvExportGLTransactionFormForTest(GLTransactionBusinessObject businessEntity) : base(businessEntity) { }

			public ZGroupBox DirectoryGroupBoxExposed
			{
				get { return base.DirectoryGroupBox; }
			}

			public ZGroupBox SortByGroupBoxExposed
			{
				get { return base.SortByGroupBox; }
			}

			public ZGroupBox BatchGroupBoxExposed
			{
				get { return base.BatchGroupBox; }
			}
		}

		#endregion
	}
}
