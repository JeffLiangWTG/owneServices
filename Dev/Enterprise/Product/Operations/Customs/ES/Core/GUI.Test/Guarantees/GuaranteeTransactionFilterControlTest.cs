using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.GUI.Guarantees;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Test
{
	public class GuaranteeTransactionFilterControlTest : TestCaseWithFactory
	{
		public void TestNewContextMenu()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var collection = new CusGuaranteeLineTransactionCollection(guaranteeHeader);
			var filterStripBusinessObject = new Customs.GUI.Guarantees.GuaranteeTransactionFilterStripBusinessObject();

			using (var form = new ZForm(guaranteeHeader))
			using (var userControl = new GuaranteeTransactionFilterControl(collection, filterStripBusinessObject))
			{
				form.Controls.Add(userControl);
				form.Show();

				AssertNotNull("Context menu to Write-off transactions is available", userControl.Grid.ContextMenu.MenuItems.FindByText("Write-off transaction"));
			}
		}

		public void TestWriteOffSingleTransactionWhenValueIsLessThanZero()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, ZString.Empty, 50m, Customs.Business.PermitTransactionTypeList.Codes.OBL);
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, "22ES00999912345678", -10m);
			var collection = new CusGuaranteeLineTransactionCollection(guaranteeHeader);
			var filterStripBusinessObject = new Customs.GUI.Guarantees.GuaranteeTransactionFilterStripBusinessObject();
			LoadDateFilter(filterStripBusinessObject);
			var expectedMessage = @"You have selected to write-off debt for reference 22ES00999912345678. An automatic
transaction will be created to add 10.00 EUR to the guarantee’s balance.

To continue with this action, please enter the write-off date:";

			using (var form = new ZForm(guaranteeHeader))
			using (var userControl = new GuaranteeTransactionFilterControlForTesting(collection, filterStripBusinessObject))
			{
				form.Controls.Add(userControl);
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

				var writeOffMenuItem = userControl.Grid.ContextMenu.MenuItems.FindByText("Write-off transaction");

				userControl.FirePerformSearch();
				userControl.Grid.Select();
				userControl.Grid.Focus();

				CombineAssertions(() =>
				{
					writeOffMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					userControl.Grid.SelectAllElements();
					writeOffMenuItem.PerformClick();
					AssertEquals("Message when a single Transaction is less than Zero", expectedMessage, userControl.promtMessage);
					AssertEquals("Default date value when a single Transaction is less than Zero", ZDate.Today, userControl.requestDate);
					AssertEquals("Nothing has been done when cancel", 2, guaranteeHeader.CusGuaranteeLineTransactions.Count);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.requestDate = ZDateTime.BrettsBirthday;
					writeOffMenuItem.PerformClick();

					AssertEquals("New Line Transaction Added when Ok", 3, guaranteeHeader.CusGuaranteeLineTransactions.Count);
					var newLineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.Cast<CusGuaranteeLineTransaction>().FirstOrDefault(x => x.CPL_TransactionType == "TRA");
					AssertEquals("New Line Transaction reference is the same as the Line Transaction Selected", "22ES00999912345678", newLineTransaction.CPL_Reference);
					AssertEquals("New Line Transaction date is the same as the transaction date we introduced in the prompt", ZDateTime.BrettsBirthday, newLineTransaction.CPL_TransactionDate);
					AssertEquals("New Line Transaction comment is the message + reference", "Write-off 22ES00999912345678", newLineTransaction.CPL_Comment);
					AssertEquals("New Line Transaction value is the positive value from the Line Transaction Selected", 10.00m, newLineTransaction.CPL_TranValue);
				});
			}
		}

		public void TestWriteOffSingleTransactionWhenValueIsMoreThanZero()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, "22ES00999912345678", -5m);
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, "22ES00999912345678", 10m);
			var collection = new CusGuaranteeLineTransactionCollection(guaranteeHeader);
			var filterStripBusinessObject = new Customs.GUI.Guarantees.GuaranteeTransactionFilterStripBusinessObject();
			LoadDateFilter(filterStripBusinessObject);
			var expectedMessage = "Reference 22ES00999912345678 has a positive balance of 5.00 EUR. Please, check the existing transactions for this reference and create a manual adjustment if needed.";

			using (var form = new ZForm(guaranteeHeader))
			using (var userControl = new GuaranteeTransactionFilterControl(collection, filterStripBusinessObject))
			{
				form.Controls.Add(userControl);
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var writeOffMenuItem = userControl.Grid.ContextMenu.MenuItems.FindByText("Write-off transaction");

				userControl.FirePerformSearch();
				userControl.Grid.Select();
				userControl.Grid.Focus();

				CombineAssertions(() =>
				{
					writeOffMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					userControl.Grid.SelectAllElements();
					writeOffMenuItem.PerformClick();
					AssertEquals("Message when a single Transaction is more than Zero", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestWriteOffSingleTransactionWhenValueIsEqualsThanZero()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, "22ES00999912345678", 0m);
			var collection = new CusGuaranteeLineTransactionCollection(guaranteeHeader);
			var filterStripBusinessObject = new Customs.GUI.Guarantees.GuaranteeTransactionFilterStripBusinessObject();
			LoadDateFilter(filterStripBusinessObject);
			var notExpectedMessage = "Reference 22ES00999912345678 has a positive balance of 0.00 EUR. Please, check the existing transactions for this reference and create a manual adjustment if needed.";

			using (var form = new ZForm(guaranteeHeader))
			using (var userControl = new GuaranteeTransactionFilterControl(collection, filterStripBusinessObject))
			{
				form.Controls.Add(userControl);
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var writeOffMenuItem = userControl.Grid.ContextMenu.MenuItems.FindByText("Write-off transaction");

				userControl.FirePerformSearch();
				userControl.Grid.Select();
				userControl.Grid.Focus();

				CombineAssertions(() =>
				{
					writeOffMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					userControl.Grid.SelectAllElements();
					writeOffMenuItem.PerformClick();
					AssertEquals("Should have not a Message when a single Transaction is more than Zero", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(notExpectedMessage));
				});
			}
		}

		public void TestWriteOffMultipleTransactionsWhenValueIsLessThanZero()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, ZString.Empty, 50m, Customs.Business.PermitTransactionTypeList.Codes.OBL);
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, "22ES00999912345678", -10m);
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, "22ES00999912345678", -10m);
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, "22ES00999912345678", 5m);
			var collection = new CusGuaranteeLineTransactionCollection(guaranteeHeader);
			var filterStripBusinessObject = new Customs.GUI.Guarantees.GuaranteeTransactionFilterStripBusinessObject();
			LoadDateFilter(filterStripBusinessObject);
			var expectedMessage = @"You have selected to write-off debt for reference 22ES00999912345678. An automatic
transaction will be created to add 15.00 EUR to the guarantee’s balance.

To continue with this action, please enter the write-off date:";

			using (var form = new ZForm(guaranteeHeader))
			using (var userControl = new GuaranteeTransactionFilterControlForTesting(collection, filterStripBusinessObject))
			{
				form.Controls.Add(userControl);
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

				var writeOffMenuItem = userControl.Grid.ContextMenu.MenuItems.FindByText("Write-off transaction");

				userControl.FirePerformSearch();
				userControl.Grid.Select();
				userControl.Grid.Focus();

				CombineAssertions(() =>
				{
					writeOffMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					userControl.Grid.SelectAllElements();
					writeOffMenuItem.PerformClick();
					AssertEquals("Message when a single Transaction is less than Zero", expectedMessage, userControl.promtMessage);
					AssertEquals("Default date value when a single Transaction is less than Zero", ZDate.Today, userControl.requestDate);
					AssertEquals("Nothing has been done when cancel", 4, guaranteeHeader.CusGuaranteeLineTransactions.Count);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.requestDate = ZDateTime.BrettsBirthday;
					writeOffMenuItem.PerformClick();

					AssertEquals("New Line Transaction Added when Ok", 5, guaranteeHeader.CusGuaranteeLineTransactions.Count);
					var newLineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.Cast<CusGuaranteeLineTransaction>().FirstOrDefault(x => x.CPL_TransactionType == "TRA");
					AssertEquals("New Line Transaction reference is the same as the Line Transaction Selected", "22ES00999912345678", newLineTransaction.CPL_Reference);
					AssertEquals("New Line Transaction date is the same as the transaction date we introduced in the prompt", ZDateTime.BrettsBirthday, newLineTransaction.CPL_TransactionDate);
					AssertEquals("New Line Transaction comment is the message + reference", "Write-off 22ES00999912345678", newLineTransaction.CPL_Comment);
					AssertEquals("New Line Transaction value is the positive value from the Line Transaction Selected", 15.00m, newLineTransaction.CPL_TranValue);
				});
			}
		}

		public void TestWriteOffMultipleTransactionsWhenValueIsMoreThanZero()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, "22ES00999912345678", 15m);
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, "22ES00999912345678", -5m);
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, "22ES00999912345678", -9m);
			var collection = new CusGuaranteeLineTransactionCollection(guaranteeHeader);
			var filterStripBusinessObject = new Customs.GUI.Guarantees.GuaranteeTransactionFilterStripBusinessObject();
			LoadDateFilter(filterStripBusinessObject);
			var expectedMessage = "Reference 22ES00999912345678 has a positive balance of 1.00 EUR. Please, check the existing transactions for this reference and create a manual adjustment if needed.";

			using (var form = new ZForm(guaranteeHeader))
			using (var userControl = new GuaranteeTransactionFilterControl(collection, filterStripBusinessObject))
			{
				form.Controls.Add(userControl);
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var writeOffMenuItem = userControl.Grid.ContextMenu.MenuItems.FindByText("Write-off transaction");

				userControl.FirePerformSearch();
				userControl.Grid.Select();
				userControl.Grid.Focus();

				CombineAssertions(() =>
				{
					writeOffMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					userControl.Grid.SelectAllElements();
					writeOffMenuItem.PerformClick();
					AssertEquals("Message when multiple Transactions are more than Zero ", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestWriteOffMultipleTransactionsWhenValueIsEqualsThanZero()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, "22ES00999912345678", -10m);
			AddCusGuaranteeLineTransaction(guaranteeHeader, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, "22ES00999912345678", 10m);
			var collection = new CusGuaranteeLineTransactionCollection(guaranteeHeader);
			var filterStripBusinessObject = new Customs.GUI.Guarantees.GuaranteeTransactionFilterStripBusinessObject();
			LoadDateFilter(filterStripBusinessObject);
			var notExpectedMessage = "Reference 22ES00999912345678 has a positive balance of 0,00 EUR. Please, check the existing transactions for this reference and create a manual adjustment if needed.";

			using (var form = new ZForm(guaranteeHeader))
			using (var userControl = new GuaranteeTransactionFilterControl(collection, filterStripBusinessObject))
			{
				form.Controls.Add(userControl);
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var writeOffMenuItem = userControl.Grid.ContextMenu.MenuItems.FindByText("Write-off transaction");

				userControl.FirePerformSearch();
				userControl.Grid.Select();
				userControl.Grid.Focus();

				CombineAssertions(() =>
				{
					writeOffMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					userControl.Grid.SelectAllElements();
					writeOffMenuItem.PerformClick();
					AssertEquals("Should have not a Message when multiple Transactions are more than Zero", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(notExpectedMessage));
				});
			}
		}

		void AddCusGuaranteeLineTransaction(CusGuaranteeHeader guaranteeHeader, string transactionStatus, ZString reference, ZDecimal value, string transactionType = Customs.Business.PermitTransactionTypeList.Codes.ADJ)
		{
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_TransactionType = transactionType;
			transaction.CPL_TransactionStatus = transactionStatus;
			transaction.CPL_Reference = reference;
			transaction.CPL_TranValue = value;
			transaction.CPL_TransactionDate = new ZDate(2022, 01, 01);
		}

		void LoadDateFilter(Customs.GUI.Guarantees.GuaranteeTransactionFilterStripBusinessObject stripBO)
		{
			var filter = (ModuleDateFilter)stripBO["Transaction Date"];
			filter.PropertySearch = ModuleDateFilter.Past;
			filter.Property1 = ZDate.Empty;
			filter.Property2 = ZDate.Empty;
			filter.IsActive = true;
		}

		class GuaranteeTransactionFilterControlForTesting : GuaranteeTransactionFilterControl
		{
			public GuaranteeTransactionFilterControlForTesting(CusGuaranteeLineTransactionCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(gridCollection, filterStripBusinessObject)
			{
			}

			public ZDateTime requestDate = ZDateTime.Today;
			public ZString promtMessage => promtMessageCore;

			ZString promtMessageCore { get; set; }

			protected override ZDateTime GetRequestDateForm(ZString message, ZDateTime defaultDate)
			{
				promtMessageCore = message;
				return base.GetRequestDateForm(message, requestDate);
			}
		}
	}
}
