using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.GenericTransaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.GUI;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GenericTransactionModule))]
	public class GenericTransactionModuleTest : ZModuleBasherTest
	{
		const string exportFilePattern = "CargoWise Export *.xlsx";
		public void TestExportExcelWithFiltersWhenGridIsEmpty()
		{
			try
			{
				using (var module = new GenericTransactionModule())
				{
					SystemDataRegistry.Instance.ExportToExcelFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ExportToExcelFormats.Code.Xlsx);

					foreach (var fileName in Directory.GetFiles(EnvProxy.Instance.TempPath, exportFilePattern))
					{
						File.Delete(fileName);
					}

					var creator = new TestObjectCreator(Factory);
					creator.CreateAPPayment(1m, 50, ZDateTime.Today, ZDateTime.Today, creator.Debtor.PK, creator.AUDBankAccount.PK);
					creator.CreateAPPayment(1m, 75, ZDateTime.Today, ZDateTime.Today, creator.Debtor.PK, creator.AUDBankAccount.PK);
					Factory.Save();

					var filter = (ModuleNumberRangeFilter)module.FilterBusinessObject.ModuleFilters["Total Amount"];
					filter.Property1 = 45m;
					filter.Property2 = 55m;
					filter.IsActive = true;

					module.HandleExportClick_ForTestOnly(null, EventArgs.Empty);

					var files = Directory.GetFiles(EnvProxy.Instance.TempPath, exportFilePattern);
					Assert("Excel file must be created", files.Length == 1);
				}
			}
			finally
			{
				foreach (var fileName in Directory.GetFiles(EnvProxy.Instance.TempPath, exportFilePattern))
				{
					File.Delete(fileName);
				}
			}
		}

		public void TestExportCanContinueWithExport()
		{
			using (var form = new ZForm())
			using (var module = new TestGenericTransactionModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((GenericTransactionFilterControl)module.EmbeddedControl).FirePerformSearch();
				AssertEquals(0, module.GridCollection.Count);
				AssertStartsWith("No record error message", "There are no records that match your search.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, ((ZDisplayGrid)module.DisplayGrid).CanContinueWithExportExposedForTest);
			}

			var header1 = Factory.NewWithValidTestData<ARInvoice>();
			((InvoicingLineBase)(header1.Lines.AddNew())).AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			var header2 = Factory.NewWithValidTestData<ARInvoice>();
			((InvoicingLineBase)(header2.Lines.AddNew())).AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();

			using (var form = new ZForm())
			using (var module = new TestGenericTransactionModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.ModifyMaxRowsToLoad(1);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((GenericTransactionFilterControl)module.EmbeddedControl).FirePerformSearch();
				AssertEquals(0, module.GridCollection.Count);
				AssertEquals("MaximumRowsLoaded error message", "This search returns more than the maximum number of records to display.\r\nThe number of search records to display can be defined in the registry up to a maximum value of 15,000 records.\r\n\r\nSee: Registry > Physical Server > Display Grid > Max No. of Records to Show\r\n\r\nThe current value is set to 1.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, ((ZDisplayGrid)module.DisplayGrid).CanContinueWithExportExposedForTest);
			}

			using (var form = new ZForm())
			using (var module = new TestGenericTransactionModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((GenericTransactionFilterControl)module.EmbeddedControl).FirePerformSearch();
				AssertEquals(2, module.GridCollection.Count);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, ((ZDisplayGrid)module.DisplayGrid).CanContinueWithExportExposedForTest);
			}
		}

		[ExpectNoExceptions]
		public void TestExportToExcel()
		{
			try
			{
				using (ZForm form = new ZForm())
				using (GenericTransactionModule module = new GenericTransactionModule())
				{
					SystemDataRegistry.Instance.ExportToExcelFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ExportToExcelFormats.Code.Xlsx);
					foreach (var fileName in Directory.GetFiles(EnvProxy.Instance.TempPath, exportFilePattern))
					{
						File.Delete(fileName);
					}

					APInvoice header = Factory.New<APInvoice>();
					header.AH_InvoiceAmount = header.AH_OSTotal = 500;
					header.AH_OutstandingAmount = 500;
					header.AH_Ledger = "AP";
					header.AH_TransactionType = "PAY";
					header.AH_TransactionNum = "00001000";
					AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
					header.AH_AB = bankAccount.PK;
					bankAccount.AB_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
					Factory.Save();
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					module.PerformSearch_ForTest();
					module.HandleExportClick_ForTestOnly(null, EventArgs.Empty);

					AssertEquals("search result found", 1, ((GenericTransactionFilterControl)module.EmbeddedControl).Grid.List.Count);
					var files = Directory.GetFiles(EnvProxy.Instance.TempPath, exportFilePattern);
					Assert("Excel file must be created", files.Length == 1);
				}
			}
			finally
			{
				foreach (var fileName in Directory.GetFiles(EnvProxy.Instance.TempPath, exportFilePattern))
				{
					File.Delete(fileName);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestExportSelectedRecordsToExcel()
		{
			try
			{
				using (ZForm form = new ZForm())
				using (GenericTransactionModule module = new GenericTransactionModule())
				{
					SystemDataRegistry.Instance.ExportToExcelFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ExportToExcelFormats.Code.Xlsx);
					foreach (var fileName in Directory.GetFiles(EnvProxy.Instance.TempPath, exportFilePattern))
					{
						File.Delete(fileName);
					}

					ARInvoice trnHeader1 = Factory.NewWithValidTestData<ARInvoice>();
					ARInvoiceLine trnLine1 = Factory.NewWithValidTestData<ARInvoiceLine>();
					trnLine1.AL_AH = trnHeader1.PK;
					trnLine1.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

					ARInvoice trnHeader2 = Factory.NewWithValidTestData<ARInvoice>();
					ARInvoiceLine trnLine2 = Factory.NewWithValidTestData<ARInvoiceLine>();
					trnLine2.AL_AH = trnHeader2.PK;
					trnLine2.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

					ARInvoice trnHeader3 = Factory.NewWithValidTestData<ARInvoice>();
					ARInvoiceLine trnLine3 = Factory.NewWithValidTestData<ARInvoiceLine>();
					trnLine3.AL_AH = trnHeader3.PK;
					trnLine3.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

					ARInvoice trnHeader4 = Factory.NewWithValidTestData<ARInvoice>();
					ARInvoiceLine trnLine4 = Factory.NewWithValidTestData<ARInvoiceLine>();
					trnLine4.AL_AH = trnHeader4.PK;
					trnLine4.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

					Factory.Save();
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					module.PerformSearch_ForTest();

					var grid = ((GenericTransactionFilterControl)module.EmbeddedControl).Grid;
					AssertEquals("search result found", 4, grid.List.Count);
					grid.Select(0);
					grid.Select(1);
					grid.Select(3);
					AssertEquals("should be selected", 3, grid.SelectedRowCount);
					module.HandleExportClick_ForTestOnly(null, EventArgs.Empty);

					var files = Directory.GetFiles(EnvProxy.Instance.TempPath, exportFilePattern);
					AssertEquals("Excel file must be created", 1, files.Length);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(files[0]);
						AssertEquals("4 rows are created included the header line", 4, excelInterface.WorkSheets[0].RowCount);
					}
				}
			}
			finally
			{
				foreach (var fileName in Directory.GetFiles(EnvProxy.Instance.TempPath, exportFilePattern))
				{
					File.Delete(fileName);
				}
			}
		}

		public void TestAllowExcelExport()
		{
			using (TestGenericTransactionModule module = new TestGenericTransactionModule())
			{
				AssertEquals("Excel export should be available.", true, module.ModuleDecisionProvider.AllowExcelExport);
			}
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert("Export To Excel functionality will only be done from the contents of the grid (not using a DataReader)", true);
		}

		protected override bool HasDefaultController()
		{
			return false;
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert(true);
		}

		public void TestGetNewController()
		{
			using (TestGenericTransactionModule module = new TestGenericTransactionModule())
			{
				AssertNull("Controller", module.GetNewController(null));

				AssertEquals("Controller", typeof(GLJournalController), module.GetNewController(Factory.New<GLJournal>()).GetType());
				AssertEquals("Controller", typeof(JCJournalController), module.GetNewController(Factory.New<JCJournalHeader>()).GetType());
				ARInvoice invoice = Factory.New<ARInvoice>();
				AssertEquals("Controller", AccountingControllerCreator.GetNewController(invoice).GetType(), module.GetNewController(invoice).GetType());

				BaseWIPAccrual line = Factory.New<WIP>();
				AssertEquals("Controller", typeof(WIPController), module.GetNewController(line).GetType());
				line = Factory.New<Accrual>();
				AssertEquals("Controller", typeof(AccrualController), module.GetNewController(line).GetType());
			}
		}

		public void TestShowViewForm()
		{
			using (TestGenericTransactionModule module = new TestGenericTransactionModule())
			{
				AssertNull("View Form", module.ShowViewForm(null));

				ARInvoice invoice = Factory.New<ARInvoice>();
				Factory.Save();
				GenericTransaction gT = new GenericTransaction(Factory);
				gT.VT_FK = invoice.PK;
				gT.VT_IsHeader = true;

				using (IZForm form = module.ShowViewForm(gT))
				{
					AssertNotNull("View Form", form);
				}
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GenericTransaction;
		}

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View => true;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			GenericTransaction gT = new GenericTransaction(collection.Factory);
			collection.Add(gT);
		}

		class TestGenericTransactionModule : GenericTransactionModule
		{
			public void ModifyMaxRowsToLoad(int maxRowsToLoad)
			{
				this.maxRowsToLoad = maxRowsToLoad;
			}

			protected override int MaxRowsToLoad => maxRowsToLoad ?? base.MaxRowsToLoad;
			int? maxRowsToLoad;

			public new IModuleDecisionProvider ModuleDecisionProvider
			{
				get { return base.ModuleDecisionProvider; }
			}

			public new ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				return base.GetNewController(selectedBusinessObject);
			}

			public new IZForm ShowViewForm(BusinessObject selectedBusinessObject)
			{
				return base.ShowViewForm(selectedBusinessObject);
			}
		}

		#endregion
	}
}
