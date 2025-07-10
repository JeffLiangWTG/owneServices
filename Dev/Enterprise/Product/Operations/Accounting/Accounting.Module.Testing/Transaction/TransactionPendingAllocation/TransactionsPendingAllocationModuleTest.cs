using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Transaction.Testing
{
	[TestedType(typeof(TransactionsPendingAllocationModule))]
	public class TransactionsPendingAllocationModuleTest : ZModuleBasherTest
	{
		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var transaction = (AccTransactionHeader)factory.NewWithValidTestData(businessObjectType);
			transaction.AH_OH = TestObjectCreator.ABIGAS.PK;

			return transaction;
		}

		protected override void CustomiseFilterForFilterStripsHelperTests(FilterStripBusinessObject filterBusinessObject)
		{
			base.CustomiseFilterForFilterStripsHelperTests(filterBusinessObject);

			var filter = (OrgWithAddressFilter)filterBusinessObject["Creditor and Address"];
			filter.IsActive = true;
			filter.Organization = TestObjectCreator.ABIGAS.PK;
			filter.Address = ZGuid.Empty;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);
			collection.Add(Factory.NewWithValidTestData<TransactionPendingAllocation>(TestBusinessObjectKind.MinimumRequiredToSave));
		}

		public void TestTransactionPendingAllocationEReportingColumnsExist()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var filterControl = new TransactionsPendingAllocationFilterControl(null, new TransactionsPendingAllocationFilterBusinessObject()))
			{
				AssertColumnAvailable(filterControl.Grid.GetColumnStyle(TransactionPendingAllocation.Schema.EInvoicingStatus), true, false);
				AssertColumnAvailable(filterControl.Grid.GetColumnStyle(TransactionPendingAllocation.Schema.EInvoicingError), true, false);
				AssertColumnAvailable(filterControl.Grid.GetColumnStyle(TransactionPendingAllocation.Schema.EInvoicingGovernmentAllocatedNumber), true, false);
			}
		}

		public void TestTransactionPendingAllocationEReportingColumnsNotExist()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var filterControl = new TransactionsPendingAllocationFilterControl(null, new TransactionsPendingAllocationFilterBusinessObject()))
			{
				AssertColumnAvailable(filterControl.Grid.GetColumnStyle(TransactionPendingAllocation.Schema.EInvoicingStatus), false, null);
				AssertColumnAvailable(filterControl.Grid.GetColumnStyle(TransactionPendingAllocation.Schema.EInvoicingError), false, null);
				AssertColumnAvailable(filterControl.Grid.GetColumnStyle(TransactionPendingAllocation.Schema.EInvoicingGovernmentAllocatedNumber), false, null);
			}
		}

		void AssertColumnAvailable(ZGridColumnInfo column, bool isAvailable, bool? isVisible)
		{
			AssertNotNull(column);

			AssertEquals("Column is not available: " + column.ColumnName, isAvailable, !column.IsUnavailable);

			if (isVisible.HasValue)
			{
				AssertEquals("Column visibility does not match: " + column.ColumnName, isVisible.Value, column.IsVisible);
			}
		}

		public void TestTransactionPendingAllocationColumns()
		{
			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				AssertEquals("Count of Columns", 31, module.DisplayGrid.ColumnStyles.Count);

				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_SystemCreateUser), isColumnVisible: false));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_SystemCreateTimeUtc), isColumnVisible: false));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_SystemCreateBranch), isColumnVisible: false));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_SystemCreateDepartment), isColumnVisible: false));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_SystemLastEditUser), isColumnVisible: false));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_SystemLastEditTimeUtc), isColumnVisible: false));

				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_OH)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_DueDate)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_InvoiceDate)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_DocumentReceivedDate)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_GB)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_GE)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_PostDate)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_Desc)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_TransactionNum)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_RX_NKTransactionCurrency)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_OSExTaxAmount)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_OSTaxAmount)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_LocalExTaxAmount)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_LocalTaxAmount)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_LocalTotalAmount)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_OSTotalAmount)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_TransactionType)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_ExchangeRate)));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.DisplayInvoiceAddressOverride), isColumnVisible: false));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.DisplayInvoiceContactOverride), isColumnVisible: false));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.ApprovalRequestStatus), isColumnVisible: false));
				Assert(ColumnExistsInTheGrid(module.DisplayGrid, nameof(TransactionPendingAllocation.Schema.AH_ComplianceSubType), isColumnVisible: false));
			}
		}

		ZBool ColumnExistsInTheGrid(ZFilterGrid grid, ZString columnName, bool? isColumnVisible = true)
		{
			var columnStyle = grid.GetColumnStyle(columnName);

			return columnStyle != null && columnStyle.ColumnName == columnName
				&& (!isColumnVisible.HasValue || columnStyle.IsVisible == isColumnVisible.Value);
		}

		public void TestHasTypeErrorForSelectedBusinessObjects()
		{
			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				var transaction = Factory.New<TransactionPendingAllocation>();
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-1));
				transaction.AH_TransactionNum = "CASSAUD121101";
				transaction.AH_OH = TestObjectCreator.AALSHI.PK;
				transaction.AH_OSExTaxAmount = 100m;
				transaction.AH_PostDate = ZDateTime.Today;
				transaction.AH_InvoiceDate = ZDateTime.Today.AddDays(-1);
				transaction.AH_DueDate = ZDateTime.Today.AddDays(1);
				transaction.AH_OSTaxAmount = 10m;
				transaction.AH_Desc = "Lorem ipsum dolor sit amet";
				transaction.RunPreSaveValidation();
				AssertNoErrors(transaction);
				Factory.Save();
				AssertEquals("Ledger Type", "PA", transaction.AH_Ledger);

				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var invoiceTransaction = (APInvoice)TransactionAllocationConverter.ConvertUnallocatedToAP(transaction).Invoice;
				var line = (InvoicingLineBase)invoiceTransaction.Lines.AddNew();
				line.AL_OSExTaxAmount = invoiceTransaction.AH_OSExTaxAmount;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				invoiceTransaction.Factory.Save();

				AssertEquals("Ledger Type chaged", "PA", transaction.AH_Ledger);
				AssertEquals("Ledger Type chaged", "AP", invoiceTransaction.AH_Ledger);

				module.GridCollection.Add(transaction);
				module.DisplayGrid.SelectAllElements();
				var editMenuItem = module.FormActionMenu.FindByText("Edit");
				editMenuItem.PerformClick();
				AssertEquals("Error should be shown", "The selected transaction is no longer valid. Please refresh the grid and try again.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				var deleteMenuItem = module.FormActionMenu.FindByText("Delete");
				deleteMenuItem.PerformClick();
				AssertEquals("Error should be shown", "The selected transaction is no longer valid. Please refresh the grid and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHasTypeErrorForSelectedBusinessObjects_WithDeletedTransactionPendingAllocation()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			Factory.Save();

			using (var form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var reloadedTransaction = newFactory.Load<TransactionPendingAllocation>(transaction.PK);
				reloadedTransaction.Delete();
				reloadedTransaction.Factory.Save();

				UnitTestUserNotification.Instance.ClearMessages();
				module.GridCollection.Add(transaction);
				module.DisplayGrid.SelectAllElements();
				var editMenuItem = module.FormActionMenu.FindByText("Edit");

				AssertNoExceptionThrown(() => editMenuItem.PerformClick());
				AssertEquals("Error should be shown", "The selected transaction is no longer valid. Please refresh the grid and try again.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				var deleteMenuItem = module.FormActionMenu.FindByText("Delete");
				AssertNoExceptionThrown(() => deleteMenuItem.PerformClick());
				AssertEquals("Error should be shown", "Unable to display the selected record. It may have been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDontAllocateCanceledTransaction()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-1));
			transaction.AH_PostDate = ZDateTime.Today;
			transaction.RunPreSaveValidation();
			AssertNoErrors(transaction);
			Factory.Save();

			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);
				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
				AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
				module.LastFormSwitchTo_ForTestOnly = null;
				allocateTransactionsMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				var lastForm = (ZForm)module.LastFormSwitchTo_ForTestOnly;
				AssertType<InvoiceForm>(lastForm);
				lastForm.Close();

				transaction.IsCancelled = true;
				Factory.Save();
				module.LastFormSwitchTo_ForTestOnly = null;
				allocateTransactionsMenuItem.PerformClick();
				AssertEquals("Cannot Allocate transaction that has been canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(module.LastFormSwitchTo_ForTestOnly);
			}
		}

		public void TestDontAllocateTransactionWithEditFormOpened()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			Factory.Save();
			// with the new behave ; any new transaction has a approval request. Here we simulate the old way where we coudl find a record without any approval request
			TestCaseHelper.ClearTable(GenApprovalRequest.Schema.TableName);
			UnitTestUserNotification.Instance.ClearMessages();

			Func<Type, Form[]> getFormsByType = (type) =>
		   {
			   var forms = Application.OpenForms.Cast<Form>().Where(f => f.GetType() == type).ToArray();
			   return forms;
		   };

			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);

				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
				AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);

				var editTransactionsMenuItem = module.FormActionMenu.FindByText("Edit");
				AssertNotNull("Precondition: allocateTransactionsMenuItem", editTransactionsMenuItem);

				var editForms = getFormsByType(typeof(TransactionPendingAllocationForm));
				AssertEquals(0, editForms.Length);
				var apForms = getFormsByType(typeof(InvoiceForm));
				AssertEquals(0, apForms.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				editTransactionsMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				editForms = getFormsByType(typeof(TransactionPendingAllocationForm));
				AssertEquals(1, editForms.Length);
				apForms = getFormsByType(typeof(InvoiceForm));
				AssertEquals(0, apForms.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				allocateTransactionsMenuItem.PerformClick();
				AssertEquals("This transaction cannot be allocated because it is open in another form. Please close that form before continuing.", UnitTestUserNotification.Instance.LastMessage.Text);

				editForms = getFormsByType(typeof(TransactionPendingAllocationForm));
				AssertEquals(1, editForms.Length);
				apForms = getFormsByType(typeof(InvoiceForm));
				AssertEquals(0, apForms.Length);

				editForms[0].Close();
			}
		}

		public void TestDontEditTransactionWithAllocationFormOpened()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			Factory.Save();
			// with the new behave ; any new transaction has a approval request. Here we simulate the old way where we coudl find a record without any approval request
			TestCaseHelper.ClearTable(GenApprovalRequest.Schema.TableName);
			UnitTestUserNotification.Instance.ClearMessages();

			Func<Type, Form[]> getFormsByType = (type) =>
			{
				var forms = Application.OpenForms.Cast<Form>().Where(f => f.GetType() == type).ToArray();
				return forms;
			};

			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);

				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
				AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);

				var editTransactionsMenuItem = module.FormActionMenu.FindByText("Edit");
				AssertNotNull("Precondition: allocateTransactionsMenuItem", editTransactionsMenuItem);

				var editForms = getFormsByType(typeof(TransactionPendingAllocationForm));
				AssertEquals(0, editForms.Length);
				var apForms = getFormsByType(typeof(InvoiceForm));
				AssertEquals(0, apForms.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				allocateTransactionsMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				editForms = getFormsByType(typeof(TransactionPendingAllocationForm));
				AssertEquals(0, editForms.Length);
				apForms = getFormsByType(typeof(InvoiceForm));
				AssertEquals(1, apForms.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				editTransactionsMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				editForms = getFormsByType(typeof(TransactionPendingAllocationForm));
				AssertEquals(0, editForms.Length);
				apForms = getFormsByType(typeof(InvoiceForm));
				AssertEquals(1, apForms.Length);

				apForms[0].Close();
			}
		}

		public void TestDontAllocateTransactionWithERRRequest()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);

			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Error;
			Factory.Save();

			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);
				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
				AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
				module.LastFormSwitchTo_ForTestOnly = null;
				allocateTransactionsMenuItem.PerformClick();
				AssertEquals("This transaction cannot be allocated because it has validation errors. The only way to allocate this transaction is to edit and save it. Editing and saving will cancel the current ‘Transaction Pending Allocation Request’ and will create a new item that can be approved for allocation.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(module.LastFormSwitchTo_ForTestOnly);
			}
		}

		public void TestDontAllocateTransactionWithStatusARR()
			=> AssertDontAllocateTransaction(Constants.GenApprovalRequestApprovalStatus.ApprovalRequested);

		public void TestDontAllocateTransactionWithStatusRRQ()
			=> AssertDontAllocateTransaction(Constants.GenApprovalRequestApprovalStatus.RejectionRequested);

		void AssertDontAllocateTransaction(string xp_ApprovalStatus)
		{
			var expectedMessage = "Transaction has started approving/rejection process. You cannot allocate it.";

			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV001", TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);
			request.XP_ApprovalStatus = xp_ApprovalStatus;

			Factory.Save();

			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);
				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
				AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
				module.LastFormSwitchTo_ForTestOnly = null;
				allocateTransactionsMenuItem.PerformClick();
				Assert("There is no error message while allocating transactions.", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(module.LastFormSwitchTo_ForTestOnly);
			}
		}

		public void TestPreventCreationOfCreditNotePendingAllocationTurnedOff()
		{
			using (AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var transaction = TestObjectCreator.CreateTransactionPendingAllocation("1", TestObjectCreator.Creditor1, -100);
				var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
				request.Initialize(transaction);
				Factory.Save();

				using (ZForm form = new ZForm())
				using (var module = new TransactionsPendingAllocationModule())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
					module.DisplayGrid.SelectAllElements();
					AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);
					var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
					AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
					module.LastFormSwitchTo_ForTestOnly = null;
					allocateTransactionsMenuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					var lastForm = (ZForm)module.LastFormSwitchTo_ForTestOnly;
					lastForm.Close();
				}
			}
		}

		public void TestPreventCreationOfCreditNotePendingAllocationTurnedOn()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("2", TestObjectCreator.Creditor1, -100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				using (ZForm form = new ZForm())
				using (var module = new TransactionsPendingAllocationModule())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
					module.DisplayGrid.SelectAllElements();
					AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);
					var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
					AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
					module.LastFormSwitchTo_ForTestOnly = null;
					allocateTransactionsMenuItem.PerformClick();
					string preventCreationOfCreditNoteMessage = "This transaction cannot be allocated because Posting of Credit Notes is prevented. This is controlled by the registry setting Accounting -> Payable Defaults -> Default Settings -> Prevent Creation of Credit Notes.";
					AssertEquals(preventCreationOfCreditNoteMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull(module.LastFormSwitchTo_ForTestOnly);
				}
			}
		}

		public void TestAllocationIsCanceledAsNoRights()
		{
			Env.Security.TransactionsPendingAllocationAllocate.IsAllowed = false;
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-1));
			transaction.AH_PostDate = ZDateTime.Today;
			transaction.RunPreSaveValidation();
			AssertNoErrors(transaction);
			Factory.Save();

			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);
				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
				AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
				AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				module.LastFormSwitchTo_ForTestOnly = null;
				allocateTransactionsMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				var lastForm = (ZForm)module.LastFormSwitchTo_ForTestOnly;
				AssertType<InvoiceForm>(lastForm);
				lastForm.Close();

				AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				module.LastFormSwitchTo_ForTestOnly = null;
				allocateTransactionsMenuItem.PerformClick();
				AssertEquals("Allocation is not allowed.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(module.LastFormSwitchTo_ForTestOnly);
			}
		}

		public void TestAllocationIsCanceledAsNoRightsForAllocateAsReceivable()
		{
			Env.Security.TransactionsPendingAllocationAllocate.IsAllowed = false;
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-1));
			transaction.AH_PostDate = ZDateTime.Today;
			transaction.RunPreSaveValidation();
			AssertNoErrors(transaction);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);
				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate as Receivable");
				AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
				AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				module.LastFormSwitchTo_ForTestOnly = null;
				allocateTransactionsMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				var lastForm = (ZForm)module.LastFormSwitchTo_ForTestOnly;
				AssertType<CreditNoteForm>(lastForm);
				lastForm.Close();

				AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				module.LastFormSwitchTo_ForTestOnly = null;
				allocateTransactionsMenuItem.PerformClick();
				AssertEquals("Allocation is not allowed.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(module.LastFormSwitchTo_ForTestOnly);
			}
		}

		public void TestAllocationWithApprovalRequestCreation()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.TransactionsPendingAllocationAllocate.IsAllowed = false;
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-1));
			transaction.AH_PostDate = ZDateTime.Today;
			transaction.RunPreSaveValidation();
			AssertNoErrors(transaction);
			Factory.Save();
			// with the new behave ; any new transaction has a approval request. Here we simulate the old way where we coudl find a record without any approval request
			TestCaseHelper.ClearTable(GenApprovalRequest.Schema.TableName);

			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);
				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");

				AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
				AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				module.LastFormSwitchTo_ForTestOnly = null;
				Env.Instance.Registry.ShowSaveProgressBox = false;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					if (dialog is LoginForm)
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
					}
					else
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});
				allocateTransactionsMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(module.LastFormSwitchTo_ForTestOnly);
				AssertEquals("Transaction is not allocated", LedgerTypes.TransactionsPendingAllocation, transaction.AH_Ledger);
				Assert("Transaction HasApprovalRequest", transaction.HasApprovalRequest);
				AssertNotNull("TransactionApprovalRequest", transaction.TransactionApprovalRequest);
				Assert("TransactionApprovalRequest is saved", transaction.TransactionApprovalRequest.IsInDatabase);
				AssertEquals("TransactionApprovalRequest.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, transaction.TransactionApprovalRequest.XP_ApprovalStatus);
			}
		}

		public void TestAllocationSetsPostedStatusToApprovalRequest()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.TransactionsPendingAllocationAllocate.IsAllowed = false;
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 110);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();
			Assert("Precondition: HasApprovalRequest", transaction.HasApprovalRequest);

			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);
				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
				AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
				module.LastFormSwitchTo_ForTestOnly = null;
				allocateTransactionsMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				var lastForm = (ZForm)module.LastFormSwitchTo_ForTestOnly;
				AssertType<InvoiceForm>(lastForm);

				var invoice = (InvoicingBase)lastForm.BusinessEntity;
				TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);
				invoice.RunPreSaveValidation();
				AssertNoErrors("Precondition", invoice);

				var saveAsIncompleteMenuItem = ((IFileMenuItemsProvider)lastForm).ActionsMenuItem.MenuItems.FindByText("Save As 'Incomplete'");
				AssertNotNull("saveAsIncompleteMenuItem", saveAsIncompleteMenuItem);
				saveAsIncompleteMenuItem.PerformClick();
				lastForm.Close();
				var transactionInDB = new BusinessObjectFactory().Load<TransactionPendingAllocation>(transaction.PK);
				AssertEquals("Transaction is allocated", LedgerTypes.IncompleteTransactions, transactionInDB.AH_Ledger);
				AssertEquals("Transaction ApprovalRequestStatus", Constants.GenApprovalRequestApprovalStatus.Posted, transactionInDB.ApprovalRequestStatus);
			}
		}

		public void TestEdtLogWithPostedReferenceAfterAddEdoc()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-1));
			transaction.AH_PostDate = ZDateTime.Today;
			transaction.RunPreSaveValidation();
			AssertNoErrors(transaction);
			Factory.Save();

			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				module.DisplayGrid.SelectAllElements();
				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
				module.LastFormSwitchTo_ForTestOnly = null;
				allocateTransactionsMenuItem.PerformClick();
				var lastForm = (ZForm)module.LastFormSwitchTo_ForTestOnly;
				AssertType<InvoiceForm>(lastForm);

				var invoice = (InvoicingBase)lastForm.BusinessEntity;

				var document = invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 1 }, "test", "COR") as DocumentScanning.Business.StorageDocsBase;
				document.EnableAddEventLogsForNewDocument();
				invoice.DocManagerInfo.Save();
				TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);
				Factory.Save();

				Assert("log added", invoice.Logs.HasLogWith(StmALogSchema.SL_Reference, "AP|INV|Posted"));
				lastForm.Close();
			}
		}

		public void TestAllocationSetsContextForTransactionAllocation()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.TransactionsPendingAllocationAllocate.IsAllowed = false;
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 110);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();
			Assert("Precondition: HasApprovalRequest", transaction.HasApprovalRequest);

			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);
				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
				AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
				module.LastFormSwitchTo_ForTestOnly = null;
				allocateTransactionsMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(module.LastFormSwitchTo_ForTestOnly);
				var lastForm = (ZForm)module.LastFormSwitchTo_ForTestOnly;
				AssertType<InvoiceForm>(lastForm);

				var invoice = (InvoicingBase)lastForm.BusinessEntity;
				Assert("Invoice has context 'APInvoiceForm'", invoice.Factory.HasContext(BusinessContext.APInvoiceForm));
				Assert("Invoice has context 'AllocatingTransaction'", invoice.Factory.HasContext(BusinessContext.AllocatingTransaction));

				lastForm.Close();
				Assert("Invoice is no longer in 'AllocatingTransaction' context", !invoice.Factory.HasContext(BusinessContext.AllocatingTransaction));
			}
		}

		public void TestReallocateTransactionPending()
		{
			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				var unallocatedTransaction = Factory.New<TransactionPendingAllocation>();
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-1));
				unallocatedTransaction.AH_TransactionNum = "CASSAUD121101";
				unallocatedTransaction.AH_OH = TestObjectCreator.AALSHI.PK;
				unallocatedTransaction.AH_OSExTaxAmount = 100m;
				unallocatedTransaction.AH_PostDate = ZDateTime.Today;
				unallocatedTransaction.AH_InvoiceDate = ZDateTime.Today.AddDays(-1);
				unallocatedTransaction.AH_DueDate = ZDateTime.Today.AddDays(1);
				unallocatedTransaction.AH_OSTaxAmount = 10m;
				unallocatedTransaction.AH_Desc = "Lorem ipsum dolor sit amet";
				unallocatedTransaction.RunPreSaveValidation();
				AssertNoErrors(unallocatedTransaction);
				Factory.Save();

				APInvoice invoiceTransaction = (APInvoice)TransactionAllocationConverter.ConvertUnallocatedToAP(unallocatedTransaction).Invoice;
				InvoicingLineBase line = (InvoicingLineBase)invoiceTransaction.Lines.AddNew();
				line.AL_OSExTaxAmount = invoiceTransaction.AH_OSExTaxAmount;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				invoiceTransaction.Factory.Save();

				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.GridCollection.Add(unallocatedTransaction);

				AssertEquals("GridCollection Should have 1 unallocatedTransaction", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("GridCollection Should have 1 unallocatedTransaction selected", 1, module.DisplayGrid.SelectedElements.Length);

				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
				AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
				allocateTransactionsMenuItem.PerformClick();
				AssertContains("User should be shown an error", "Cannot allocate transaction that has been allocated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllocationAPInvoice_AH_ComplianceSubTypeDropEdit_ReadOnly()
		{
			var company = TestObjectCreator.CreateNewCompany("DCO", "CO");
			company.GC_Name = "Colombia";
			var branch = TestObjectCreator.CreateBranch("BGT", "branch for CO", company);
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.TransactionsPendingAllocationAllocate.IsAllowed = false;
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
				var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
				request.Initialize(transaction);
				request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
				Factory.Save();
				Assert("Precondition: HasApprovalRequest", transaction.HasApprovalRequest);

				using (var form = new ZForm())
				using (var module = new TransactionsPendingAllocationModule())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
					module.DisplayGrid.SelectAllElements();
					AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);
					var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
					AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
					module.LastFormSwitchTo_ForTestOnly = null;
					allocateTransactionsMenuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					var lastForm = (ZForm)module.LastFormSwitchTo_ForTestOnly;
					AssertType<InvoiceForm>(lastForm);

					AssertEquals(true, ((InvoiceForm)lastForm).InvoiceDetails.AH_ComplianceSubTypeDropEdit.Visible);
					AssertEquals(false, ((InvoiceForm)lastForm).InvoiceDetails.AH_ComplianceSubTypeDropEdit.ReadOnly);

					lastForm.Close();
				}
			}
		}

		public void TestAllocationAPCreditNote_AH_ComplianceSubTypeDropEdit_ReadOnly()
		{
			var company = TestObjectCreator.CreateNewCompany("DCO", "CO");
			company.GC_Name = "Colombia";
			var branch = TestObjectCreator.CreateBranch("BGT", "branch for CO", company);
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.TransactionsPendingAllocationAllocate.IsAllowed = false;
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, -100);
				var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
				request.Initialize(transaction);
				request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
				Factory.Save();
				Assert("Precondition: HasApprovalRequest", transaction.HasApprovalRequest);

				using (var form = new ZForm())
				using (var module = new TransactionsPendingAllocationModule())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
					module.DisplayGrid.SelectAllElements();
					AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);
					var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
					AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
					module.LastFormSwitchTo_ForTestOnly = null;
					allocateTransactionsMenuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					var lastForm = (ZForm)module.LastFormSwitchTo_ForTestOnly;
					AssertType<CreditNoteForm>(lastForm);

					AssertEquals(true, ((CreditNoteForm)lastForm).InvoiceDetails.AH_ComplianceSubTypeDropEdit.Visible);
					AssertEquals(false, ((CreditNoteForm)lastForm).InvoiceDetails.AH_ComplianceSubTypeDropEdit.ReadOnly);

					lastForm.Close();
				}
			}
		}

		public void TestShowTransactionsPendingAllocationWithoutNewButton()
		{
			using (var form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var newBulkMenuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("New Bulk Unallocated Transactions");
				AssertNotNull(newBulkMenuItem);

				newBulkMenuItem.PerformClick();

				var transactionPendingAllocationForm = ZFormModaliser.LastFormShownForTest;
				AssertNotNull(transactionPendingAllocationForm);
				AssertType<TransactionsPendingAllocationForm>(transactionPendingAllocationForm);

				var applyButton = ((IPostingButtonsProvider)transactionPendingAllocationForm).CommandButtonApply;
				AssertEquals("&Save", applyButton.Text);

				transactionPendingAllocationForm.Close();
			}
		}

		public void TestAllocateTransactionMenuItemIsExist()
		{
			using (var form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
				AssertNotNull(allocateTransactionsMenuItem);
			}
		}

		public void TestAllocateAsPayableAndAsReceivableMenuItemsAreExistForTurkeyCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			using (var form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
				AssertNull(allocateTransactionsMenuItem);

				var allocateAsPayableMenuItem = module.FormActionMenu.FindByText("Allocate as Payable");
				AssertNotNull(allocateAsPayableMenuItem);

				var allocateAsReceivableMenuItem = module.FormActionMenu.FindByText("Allocate as Receivable");
				AssertNotNull(allocateAsReceivableMenuItem);
			}
		}

		public void TestAllocationAPCreditNote_HandleMutexException()
		{
			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "SGSIN";

				for (var i = 0; i < 2; i++)
				{
					consol.Shipments.AddNew();
				}

				Factory.Save();

				var unallocatedTransaction = TestObjectCreator.CreateTransactionPendingAllocation(consol.JK_UniqueConsignRef, TestObjectCreator.AALSHI, 100);
				var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
				request.Initialize(unallocatedTransaction, xml, true);
				Assert("Precondition: HasUniversalTransaction", unallocatedTransaction.IsImportedFromUniversalXML);
				unallocatedTransaction.AllocationApprovalRequest.PostingDetails.IsCrossLedgerImportFromXML = false;

				Factory.Save();

				var listing = new ApportionmentListing(Factory, consol);

				try
				{
					var consolCost = listing.CostsCollection.TryAddNew();
					consolCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
					consolCost.E6_OSCostAmount = 10m;
					consolCost.CostExchangeRate.Currency = Constants.CurrencyCodes.UnitedStates;
					consolCost.CostExchangeRate.Rate = 1m;

					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					module.GridCollection.Add(unallocatedTransaction);

					AssertEquals("GridCollection Should have 1 unallocatedTransaction", 1, module.GridCollection.Count);
					module.DisplayGrid.SelectAllElements();
					AssertEquals("GridCollection Should have 1 unallocatedTransaction selected", 1, module.DisplayGrid.SelectedElements.Length);

					var allocateTransactionsMenuItem = module.FormActionMenu.FindByText("Allocate Transactions");
					AssertNotNull("Precondition: allocateTransactionsMenuItem", allocateTransactionsMenuItem);
					AssertNoExceptionThrown(allocateTransactionsMenuItem.PerformClick);
					AssertContains("User should be shown an error", @"haven't saved it yet.
Please close or save other forms", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					listing.ReleaseMutexes();
				}
			}
		}

		public void TestDontDeleteAllocatedTransaction()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			Factory.Save();

			using (ZForm form = new ZForm())
			using (var module = new TransactionsPendingAllocationModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition: SelectedElements.Length", 1, module.DisplayGrid.SelectedElements.Length);

				APInvoice invoiceTransaction = (APInvoice)TransactionAllocationConverter.ConvertUnallocatedToAP(transaction).Invoice;
				InvoicingLineBase line = (InvoicingLineBase)invoiceTransaction.Lines.AddNew();
				line.AL_OSExTaxAmount = invoiceTransaction.AH_OSExTaxAmount;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				invoiceTransaction.Factory.Save();

				var expectedErrorMessage = "The selected transaction is no longer valid. Please refresh the grid and try again.";

				var deleteTransactionsMenuItem = module.FormActionMenu.FindByText("Delete");
				AssertNotNull("Precondition: DeleteMenuItem", deleteTransactionsMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				deleteTransactionsMenuItem.PerformClick();
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUniversalCopyIsntAccessableInMenuForTPA()
		{
			using (var testModule = new TransactionsPendingAllocationModule())
			{
				using (var form1 = new ZForm())
				{
					form1.Controls.Add(testModule.EmbeddedControl);
					form1.Show();
					testModule.PerformSearch_ForTest();
					AssertNull(testModule.SetupAndGetGrid().ContextMenu.MenuItems.FindByName("UniversalCopy"));
				}
			}
		}

		#region Unallocated Transaction XML

		const string xml = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Ledger>AP</Ledger>
    <OSExGSTVATAmount>-600.0000</OSExGSTVATAmount>
    <Number>C00001000</Number>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>AALSHI</OrganizationCode>
    </OrganizationAddress>
    <PostingJournalCollection>
      <PostingJournal>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>Consol1</Key>
        </CostSource>
        <Job>
          <Key>Job1</Key>
          <Type>Job</Type>
        </Job>
        <OSAmount>-100.0000</OSAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
		</VATTaxID>
      </PostingJournal>
      <PostingJournal>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>Consol1</Key>
        </CostSource>
        <OSAmount>-200.0000</OSAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
		</VATTaxID>
      </PostingJournal>
      <PostingJournal>
        <ChargeCode>
          <Code>FRT</Code>
        </ChargeCode>
        <CostSource>
          <Type>ForwardingConsol</Type>
          <Key>Consol1</Key>
        </CostSource>
        <Job>
          <Key>Job2</Key>
          <Type>Job</Type>
        </Job>
        <OSAmount>-300.0000</OSAmount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
		</VATTaxID>
      </PostingJournal>
    </PostingJournalCollection>
    <ShipmentCollection>
	  <Shipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>Job1</Key>
            </DataSource>
          </DataSourceCollection>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S00001001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      </Shipment>
	  <Shipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>Job2</Key>
            </DataSource>
          </DataSourceCollection>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S00001000</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      </Shipment>
      <Shipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingConsol</Type>
              <Key>Consol1</Key>
            </DataSource>
          </DataSourceCollection>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
              <Key>C00001000</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      </Shipment>
    </ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>
";

		#endregion

		#region Implementation

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		protected override BusinessObject GetNewBusinessObjectForLoadingInCorrectThreadTests()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			transaction.Factory.Save();

			return transaction;
		}

		#endregion

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.TransactionsPendingAllocation;
		}
	}
}
