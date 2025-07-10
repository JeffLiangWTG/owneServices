using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.TemporaryStorage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine;
using CusTempStorageRegLineTransaction = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransaction;
using CusTempStorageRegLineTransactionStatusList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionStatusList;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

sealed class TempStorageRegTransactionFilterControlTest : TestCaseWithFactory
{
	[RequiresSTA]
	public void TestTransactionsGrid()
	{
		var regHeader = SetUpRegHeader();
		regHeader.SRH_Reference = "REF1";
		var line = regHeader.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction1.SRT_TransactionStatus = "DEL";
		var collection = line.CusTempStorageRegLineTransactions;
		var filterStripBusinessObject = new TempStorageRegTransactionFilterStripBusinessObject();

		using var form = new ZForm(line);
		using var control = new TempStorageRegTransactionFilterControl(collection, filterStripBusinessObject);

		form.Controls.Add(control);
		form.Show();

		System.Windows.Forms.Application.DoEvents();

		var transactionGrid = control.Grid;
		CombineAssertions(() =>
		{
			AssertEquals("Column Count 13", 13, transactionGrid.ColumnStyles.Count);
			AssertEquals("SRT_PhysicalInOutDate", "Physical In/Out Date", transactionGrid.GetColumnStyle(nameof(CusTempStorageRegLineTransaction.PhysicalInOutDate)).CaptionResourceString.Caption);
			AssertEquals("SRT_TransactionDate", "Transaction Date", transactionGrid.GetColumnStyle(nameof(CusTempStorageRegLineTransaction.TransactionDate)).CaptionResourceString.Caption);
			AssertEquals("SRT_TransactionType", "Transaction Type", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_TransactionType).CaptionResourceString.Caption);
			AssertEquals("TransactionTypeDescription", "Transaction Type Description", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.TransactionTypeDescription).CaptionResourceString.Caption);
			AssertEquals("SRT_GrossWeight", "Gross Weight in KGs", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_GrossWeight).CaptionResourceString.Caption);
			AssertEquals("SRT_PackageQty", "Package Quantity", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_PackageQty).CaptionResourceString.Caption);
			AssertEquals("SRT_BondAmount", "Bond Amount", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_BondAmount).CaptionResourceString.Caption);
			AssertEquals("SRT_InternalReferenceType", "Internal Reference Type", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_InternalReferenceType).CaptionResourceString.Caption);
			AssertEquals("SRT_InternalReferenceNumber", "Internal Reference No.", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_InternalReferenceNumber).CaptionResourceString.Caption);
			AssertEquals("SRT_ReferenceType", "Reference Type", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_ReferenceType).CaptionResourceString.Caption);
			AssertEquals("SRT_Reference", "Reference Number", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_Reference).CaptionResourceString.Caption);
			AssertEquals("SRT_Comments", "Comments", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_Comments).CaptionResourceString.Caption);
			AssertEquals("SRT_TransactionStatus", "Transaction Status", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_TransactionStatus).CaptionResourceString.Caption);
		});
	}

	[RequiresSTA]
	public void TestTransactionsGridColumns()
	{
		var regHeader = SetUpRegHeader();
		regHeader.SRH_Reference = "REF1";
		var line = regHeader.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction1.SRT_TransactionStatus = "DEL";
		var collection = line.CusTempStorageRegLineTransactions;
		var filterStripBusinessObject = new TempStorageRegTransactionFilterStripBusinessObject();

		using var form = new ZForm(line);
		using var control = new TempStorageRegTransactionFilterControl(collection, filterStripBusinessObject);

		form.Controls.Add(control);
		form.Show();

		System.Windows.Forms.Application.DoEvents();

		CombineAssertions(() =>
		{
			var transactionGrid = control.Grid;
			AssertEquals("columns count 13", OrderedColumnNamesAndColumnStyleTypesTransactionsGrid.Count(), transactionGrid.ColumnStyles.Count);

			foreach (var (columnName, columnType) in OrderedColumnNamesAndColumnStyleTypesTransactionsGrid)
			{
				var column = transactionGrid.Columns.SingleOrDefault(x => x.ColumnName == columnName);
				if (column == null)
				{
					Assert($"Column: {columnName} does not exists", condition: false);
				}
				else
				{
					var actualColumnStyleType = column.ColumnStyle.GetType();
					AssertEquals($"Column: {columnName}: columnStyleType", columnType, actualColumnStyleType);
					Assert($"Column: {columnName}: visible", column.IsVisible);
				}
			}
		});
	}

	IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnNamesAndColumnStyleTypesTransactionsGrid => new (string, Type)[]
	{
		(nameof(CusTempStorageRegLineTransaction.PhysicalInOutDate), typeof(ZDateEditColumnStyle)),
		(nameof(CusTempStorageRegLineTransaction.TransactionDate), typeof(ZDateEditColumnStyle)),
		(CusTempStorageRegLineTransaction.Schema.SRT_TransactionType, typeof(ZDropEditColumnStyle)),
		(CusTempStorageRegLineTransaction.Schema.TransactionTypeDescription, typeof(ZTextBoxColumnStyle)),
		(CusTempStorageRegLineTransaction.Schema.SRT_GrossWeight, typeof(ZCalcEditColumnStyle)),
		(CusTempStorageRegLineTransaction.Schema.SRT_PackageQty, typeof(ZCalcEditColumnStyle)),
		(CusTempStorageRegLineTransaction.Schema.SRT_BondAmount, typeof(ZCalcEditColumnStyle)),
		(CusTempStorageRegLineTransaction.Schema.SRT_InternalReferenceType, typeof(ZDropEditColumnStyle)),
		(CusTempStorageRegLineTransaction.Schema.SRT_InternalReferenceNumber, typeof(ZTextBoxColumnStyle)),
		(CusTempStorageRegLineTransaction.Schema.SRT_ReferenceType, typeof(ZDropEditColumnStyle)),
		(CusTempStorageRegLineTransaction.Schema.SRT_Reference, typeof(ZTextBoxColumnStyle)),
		(CusTempStorageRegLineTransaction.Schema.SRT_Comments, typeof(ZTextBoxColumnStyle)),
		(CusTempStorageRegLineTransaction.Schema.SRT_TransactionStatus, typeof(ZDropEditColumnStyle)),
	};

	[RequiresSTA]
	public void TestTransactionsGridMenuItems()
	{
		var testHeader = SetUpRegHeader();
		var regLine = testHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;

		var filterStripBusinessObject = new TempStorageRegTransactionFilterStripBusinessObject();
		using var form = new ZForm(regLine);
		using var userControl = new TempStorageRegTransactionFilterControl(regLine.CusTempStorageRegLineTransactions, filterStripBusinessObject);
		form.Controls.Add(userControl);
		form.Show();

		AssertNotNull("Context menu to Update In/Out Date exists", userControl.Grid.ContextMenu.MenuItems.FindByText("Update In/Out Date"));
	}

	#region Update In/Out Date

	[RequiresSTA]
	public void TestUpdateTransactionDateClick_Cancel()
	{
		var testHeader = SetUpRegHeader();
		var regLine = testHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		var transaction = AddRegLineTransaction(regLine);
		Factory.Save();

		var filterStripBusinessObject = new TempStorageRegTransactionFilterStripBusinessObject();
		using var form = new ZForm(regLine);
		using var userControl = new TempStorageRegTransactionFilterControl(regLine.CusTempStorageRegLineTransactions, filterStripBusinessObject);
		form.Controls.Add(userControl);
		form.Show();
		userControl.FirePerformSearch();

		ZFormModaliser.ShowDialogsInTest = false;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		var transactionGrid = userControl.Grid;

		var updateDateMenuItem = transactionGrid.ContextMenu.MenuItems.FindByText("Update In/Out Date");

		transactionGrid.Select();
		_ = transactionGrid.Focus();

		updateDateMenuItem.PerformClick();

		CombineAssertions(() =>
		{
			AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

			transactionGrid.SelectAllElements();
			updateDateMenuItem.PerformClick();
			AssertEquals("Transaction SRT_PhysicalInOutDate has not been updated", ZDateTimeOffset.Empty, transaction.SRT_PhysicalInOutDate);
		});
	}

	[RequiresSTA]
	public void TestUpdateTransactionDateClick_DateLessThanPrompt()
	{
		var testHeader = SetUpRegHeader();
		var regLine = testHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		var transaction = AddRegLineTransaction(regLine, transactionDate: ZDateTime.BrettsBirthday.AddDays(1).ToOffset());
		Factory.Save();

		var filterStripBusinessObject = new TempStorageRegTransactionFilterStripBusinessObject();
		using var form = new ZForm(regLine);
		using var userControl = new TempStorageRegTransactionFilterControlForTesting(regLine.CusTempStorageRegLineTransactions, filterStripBusinessObject);
		form.Controls.Add(userControl);
		form.Show();
		userControl.FirePerformSearch();

		ZFormModaliser.ShowDialogsInTest = false;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		var transactionGrid = userControl.Grid;

		var updateDateMenuItem = transactionGrid.ContextMenu.MenuItems.FindByText("Update In/Out Date");
		transactionGrid.Select();
		_ = transactionGrid.Focus();

		transactionGrid.SelectAllElements();
		updateDateMenuItem.PerformClick();

		CombineAssertions(() =>
		{
			AssertEquals("Error is shown when Date is greater than the entered on the prompt", "OTH/Test1: Physical In/Out Date cannot be older than Transaction Date", UnitTestUserNotification.Instance.PreviousMessages.LastOrDefault(x => x.WasError).Text);
			AssertEquals("Transaction SRT_PhysicalInOutDate has not been updated if Date is greater than the entered on the promp", ZDateTimeOffset.Empty, transaction.SRT_PhysicalInOutDate);
		});
	}

	[RequiresSTA]
	public void TestUpdateTransactionDateClick_SingleLine()
	{
		var testHeader = SetUpRegHeader();
		var regLine = testHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		var transaction = AddRegLineTransaction(regLine);
		Factory.Save();

		var filterStripBusinessObject = new TempStorageRegTransactionFilterStripBusinessObject();
		using var form = new ZForm(regLine);
		using var userControl = new TempStorageRegTransactionFilterControlForTesting(regLine.CusTempStorageRegLineTransactions, filterStripBusinessObject);
		form.Controls.Add(userControl);
		form.Show();
		userControl.FirePerformSearch();

		ZFormModaliser.ShowDialogsInTest = false;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		var transactionGrid = userControl.Grid;

		var updateDateMenuItem = transactionGrid.ContextMenu.MenuItems.FindByText("Update In/Out Date");
		transactionGrid.Select();
		_ = transactionGrid.Focus();

		transactionGrid.SelectAllElements();
		updateDateMenuItem.PerformClick();

		CombineAssertions(() =>
		{
			AssertNull("Error is not shown when Date is less than the entered on the prompt", UnitTestUserNotification.Instance.PreviousMessages.LastOrDefault(x => x.WasError));
			AssertEquals("Transaction SRT_PhysicalInOutDate has been updated if all is correct", ZDateTime.BrettsBirthday.ToOffset(), transaction.SRT_PhysicalInOutDate);
		});
	}

	[RequiresSTA]
	public void TestUpdateTransactionDateClick_MultipleLines()
	{
		var testHeader = SetUpRegHeader();
		var regLine = testHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		var transaction1 = AddRegLineTransaction(regLine, status: "PND");
		var transaction2 = AddRegLineTransaction(regLine);
		var transaction3 = AddRegLineTransaction(regLine, transactionDate: ZDateTimeOffset.Empty);
		Factory.Save();

		var filterStripBusinessObject = new TempStorageRegTransactionFilterStripBusinessObject();
		using var form = new ZForm(regLine);
		using var userControl = new TempStorageRegTransactionFilterControlForTesting(regLine.CusTempStorageRegLineTransactions, filterStripBusinessObject);
		form.Controls.Add(userControl);
		form.Show();
		userControl.FirePerformSearch();

		ZFormModaliser.ShowDialogsInTest = false;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		var transactionGrid = userControl.Grid;

		var updateDateMenuItem = transactionGrid.ContextMenu.MenuItems.FindByText("Update In/Out Date");
		transactionGrid.Select();
		_ = transactionGrid.Focus();

		transactionGrid.SelectAllElements();
		updateDateMenuItem.PerformClick();

		CombineAssertions(() =>
		{
			AssertEquals("Transaction SRT_PhysicalInOutDate has not been updated if Status is not CON", ZDateTimeOffset.Empty, transaction1.SRT_PhysicalInOutDate);
			AssertEquals("Transaction SRT_PhysicalInOutDate has been updated if all is correct", ZDateTime.BrettsBirthday.ToOffset(), transaction2.SRT_PhysicalInOutDate);
			AssertEquals("Transaction SRT_PhysicalInOutDate has not been updated if Date is empty", ZDateTimeOffset.Empty, transaction3.SRT_PhysicalInOutDate);
		});
	}

	#endregion

	[RequiresSTA]
	public void TestNewTransactionButton() => CombineAssertions(() =>
	{
		var testHeader = SetUpRegHeader();
		var regLine = testHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;

		var filterStripBusinessObject = new TempStorageRegTransactionFilterStripBusinessObject();
		using var form = new ZForm(regLine);
		using var userControl = new TempStorageRegTransactionFilterControl(regLine.CusTempStorageRegLineTransactions, filterStripBusinessObject);
		form.Controls.Add(userControl);
		form.Show();
		var newTransactionButton = userControl.NewTransactionButton;
		AssertNotNull("NewTransactionButton is not null", newTransactionButton);
		AssertNotNull("NewTransactionButton has image", newTransactionButton.Image);
		AssertEquals("Caption", "New Transaction", newTransactionButton.CaptionResourceString.Caption);
		var filterStripsPanel = userControl.Controls.Find("FilterStripsPanel", searchAllChildren: true).FirstOrDefault();
		AssertEquals("FilterStripsPanel has new Size to be able to not overlap button", 690, filterStripsPanel.Width);
	});

	public void TestNewTransactionButtonVisibility() => CombineAssertions(() =>
	{
		var testHeader = SetUpRegHeader();
		var premises = testHeader.Premises;
		var regLine = testHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		regLine.CusTempStorageRegLineTransactions.SetReadOnlyIncludingChildren(false);

		AssertNewTransactionButton(true);
		AssertNewTransactionButton(false);

		void AssertNewTransactionButton(bool isPremisesActive)
		{
			premises.SRP_IsActive = isPremisesActive;

			using var form = new ZForm(regLine);
			using var userControl = new TempStorageRegTransactionUserControl();
			form.Controls.Add(userControl);
			form.Show();
			var newTransactionButton = userControl.TransactionsFilterControl.NewTransactionButton;
			AssertEquals($"Visibility when SRP_IsActive is {isPremisesActive}", isPremisesActive, newTransactionButton.Visible);
		}
	});

	[RequiresSTA]
	public void TestGrossWeightAndPackagesRemainingCalculated() => CombineAssertions(() =>
	{
		var regHeader = SetUpRegHeader();
		regHeader.SRH_Reference = "REF1";

		var line = regHeader.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;

		var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		transaction1.SRT_PackageQty = 2;
		transaction1.SRT_GrossWeight = 3;
		transaction1.SRT_TransactionType = "ADJ";

		var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		transaction2.SRT_PackageQty = 4;
		transaction2.SRT_GrossWeight = 2;
		transaction2.SRT_TransactionType = "OBL";

		var transaction3 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
		transaction3.SRT_PackageQty = 3;
		transaction3.SRT_GrossWeight = 1;
		transaction3.SRT_TransactionType = "OBL";

		AssertEquals("PackagesRemainingCalculated = 6, before click new transaction button", 6, line.PackagesRemainingCalculated);
		AssertEquals("GrossWeightRemainingCalculated = 5, before click new transaction button", 5m, line.GrossWeightRemainingCalculated);

		var collection = line.CusTempStorageRegLineTransactionsForFilter;
		var filterStripBusinessObject = new TempStorageRegTransactionFilterStripBusinessObject();

		using var form = new ZForm(line);
		using var control = new TempStorageRegTransactionFilterControlForTesting(collection, filterStripBusinessObject);
		control.RemoveRegLineForTest = true;
		form.Controls.Add(control);
		form.Show();
		control.FirePerformSearch();

		AssertEquals("Total Transactions match the filter, before click new transaction button and before add a new filter", 2, collection.Count);

		LoadDeclarationCollectionTextFilter("TransactionType", "ADJ", filterStripBusinessObject);
		control.FirePerformSearch();

		AssertEquals("Total Transactions match the filter, before click new transaction button and after add a new filter", 1, collection.Count);

		control.ExposedNewTransactionButton_Click();
		AssertEquals("PackagesRemainingCalculated = 6, after click new transaction button", 6, line.PackagesRemainingCalculated);
		AssertEquals("GrossWeightRemainingCalculated = 5, after click new transaction button", 5m, line.GrossWeightRemainingCalculated);
		AssertEquals("Total Transactions match the filter, after click new transaction button", 1, collection.Count);
	});

	void LoadDeclarationCollectionTextFilter(ZString filterField, ZString filterValue, TempStorageRegTransactionFilterStripBusinessObject stripBO, string comparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith)
	{
		var filter = (ModuleTextFilter)stripBO[filterField];
		filter.Property = filterValue;
		filter.ComparisonOperator = comparisonOperator;
		filter.IsActive = true;
	}

	class TempStorageRegTransactionFilterControlForTesting : TempStorageRegTransactionFilterControl
	{
		public TempStorageRegTransactionFilterControlForTesting(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
		: base(gridCollection, filterStripBusinessObject)
		{
		}

		readonly ZDateTime requestDate = ZDateTime.BrettsBirthday;

		protected override ZDateTime GetRequestDateForm() => requestDate;

		public void ExposedNewTransactionButton_Click() => NewTransactionButton_Click(null, null);

		protected override void ShowNewTransactionForm(CusTempStorageRegLine regLine, CusTempStorageRegLineTransaction transaction, CusTempStorageRegLineTransactionFormEditable newTransaction)
		{
			if (RemoveRegLineForTest)
			{
				newTransaction.SRT_PackageQty = 0;
				newTransaction.Delete();
				transaction.Delete();
				regLine.Factory.Save();
			}
			else
			{
				base.ShowNewTransactionForm(regLine, transaction, newTransaction);
			}
		}

		public bool RemoveRegLineForTest {  get; set; }
	}

	CusTempStorageRegHeader SetUpRegHeader()
	{
		var testHeader = Factory.New<CusTempStorageRegHeader>();
		testHeader.SRH_AppCode = "123";
		testHeader.SRH_Status = "OK";
		testHeader.SRH_Reference = "TEST";
		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "DESC";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "TestAddress";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		testHeader.SRH_SRP_Premises = premises.PK;

		return testHeader;
	}

	CusTempStorageRegLineTransaction AddRegLineTransaction(CusTempStorageRegLine regLine, string status = "CON", ZDateTimeOffset? transactionDate = null)
	{
		var transaction = regLine.CusTempStorageRegLineTransactions.AddNew();
		transaction.SRT_InternalReferenceNumber = "Test1";
		transaction.SRT_InternalReferenceType = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
		transaction.SRT_TransactionType = "OBL";
		transaction.SRT_TransactionStatus = status;
		transaction.SRT_TransactionDate = transactionDate ?? ZDateTime.BrettsBirthday.AddDays(-1).ToOffset();
		return transaction;
	}
}
