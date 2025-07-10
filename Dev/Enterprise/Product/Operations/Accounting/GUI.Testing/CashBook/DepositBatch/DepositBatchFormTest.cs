using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn.Internal;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.CashBook.DepositBatch.Testing
{
	[TestedType(typeof(DepositBatchForm))]
	public class DepositBatchFormTest : AccountingZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DepositBatchForm(new DepositBatchParent(Factory));
		}

		public override void TestFormVerb()
		{
			using (DepositBatchForm testForm = (DepositBatchForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				AssertEquals("Verb should be 'Reverse'", "Reverse", testForm.FormVerb);
				testForm.DisplayMode = ODisplayMode.New;
				AssertEquals("Verb should be 'new'", "New", testForm.FormVerb);
			}
		}

		public override void TestDeleteButtonText()
		{
			using (DepositBatchForm testForm = (DepositBatchForm)GetFormToBashCore())
			{
				AssertEquals("Delete Button Text should be '&Cancel'", "&Cancel", ZFormPostingButtonsStrategy.DeleteButtonText(testForm).Text);
			}
		}

		public void TestPostAndApplyButtonText()
		{
			using (DepositBatchForm testForm = (DepositBatchForm)GetFormToBashCore())
			{
				AssertEquals("Post Button Text should be 'P&ost && Close'", "P&ost && Close", ZFormPostingButtonsStrategy.PostButtonText(testForm).Text);
				AssertEquals("Apply Button Text should be '&Post'", "&Post", ZFormPostingButtonsStrategy.ApplyButtonText(testForm).Text);
			}

			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			Business.CashBook.DepositBatch.DepositBatch testDepositBatch = testDepositBatchParent.DepositBatchLines[0];
			Factory.Save();

			using (DepositBatchForm testForm = new DepositBatchForm(new DepositBatchParent(Factory, testDepositBatch)))
			{
				AssertEquals("Post Button Text should be 'S&ave && Close'", "S&ave && Close", ZFormPostingButtonsStrategy.PostButtonText(testForm).Text);
				AssertEquals("Apply Button Text should be '&Save'", "&Save", ZFormPostingButtonsStrategy.ApplyButtonText(testForm).Text);
			}
		}

		public void TestCancelText()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);

			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			Business.CashBook.DepositBatch.DepositBatch testDepositBatch = testDepositBatchParent.DepositBatchLines[0];
			testDepositBatch.AH_IsCancelled = true;
			Factory.Save();

			DepositBatchParent loadedBatchParent = new DepositBatchParent(Factory, testDepositBatch);

			using (DepositBatchForm testForm = new DepositBatchForm(loadedBatchParent))
			{
				testForm.Show();
				AssertEquals(true, testForm.CancelReasonLabel_ForTestOnly.Visible);
			}
		}

		public void TestReloadThisFormMenuItemDisabledWhenCancel()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			Business.CashBook.DepositBatch.DepositBatch testDepositBatch = testDepositBatchParent.DepositBatchLines[0];
			Factory.Save();

			using (DepositBatchForm testForm = new DepositBatchForm(new DepositBatchParent(Factory, testDepositBatch)))
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				Application.DoEvents();

				Assert("Reload menu should be disabled when cancel deposit batch", !testForm.Menu.MenuItems["FileMenuItem"].MenuItems["FileReloadMenuItem"].Enabled);
			}
		}

		public void TestSelectUnSelectAllButtons()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount2.PK);

			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);

			using (DepositBatchForm testForm = new DepositBatchForm(testDepositBatchParent))
			{
				testForm.Show();

				testForm.BankUnSelectAllButton_ForTestOnly.PerformClick();
				AssertEquals(false, testDepositBatchParent.DepositBatchLines[0].IsSelected);
				AssertEquals(false, testDepositBatchParent.DepositBatchLines[1].IsSelected);

				testForm.BankSelectAllButton_ForTestOnly.PerformClick();
				AssertEquals(true, testDepositBatchParent.DepositBatchLines[0].IsSelected);
				AssertEquals(true, testDepositBatchParent.DepositBatchLines[1].IsSelected);

				AssertEquals(testDepositBatchParent.DepositBatchLines[0].BankCode, testForm.BankCodeLabel_ForTestOnly.Text);

				testForm.UnSelectAllButton_ForTestOnly.PerformClick();
				AssertEquals(false, testDepositBatchParent.DepositBatchLines[0].IsSelected);
				AssertEquals(true, testDepositBatchParent.DepositBatchLines[1].IsSelected);

				testForm.SelectAllButton_ForTestOnly.PerformClick();
				AssertEquals(true, testDepositBatchParent.DepositBatchLines[0].IsSelected);
				AssertEquals(true, testDepositBatchParent.DepositBatchLines[1].IsSelected);
			}
		}

		public void TestSelectUnSelectAllButtonsOnExistingBatch()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Now.Year, 1, 1));
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount2.PK);

			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals("Should be 2 lines", 2, testDepositBatchParent.DepositBatchLines.Count);

			using (DepositBatchForm testForm = new DepositBatchForm(testDepositBatchParent))
			{
				testForm.Show();
				testForm.ValidateAndSave_ForTestOnly();

				AssertSelectUnSelectButtonsDisabled(testForm);
			}

			using (DepositBatchForm testForm = new DepositBatchForm(testDepositBatchParent))
			{
				testForm.Show();

				AssertSelectUnSelectButtonsDisabled(testForm);
			}
		}

		void AssertSelectUnSelectButtonsDisabled(DepositBatchForm testForm)
		{
			DepositBatchParent testDepositBatchParent = testForm.DataSource as DepositBatchParent;
			AssertNotNull(testDepositBatchParent);
			Assert("Should be Existing Batch", testDepositBatchParent.IsExistingBatch);
			AssertEquals("Should be 2 lines", 2, testDepositBatchParent.DepositBatchLines.Count);
			AssertEquals("BankSelectAllButton_ForTestOnly should be Disabled", false, testForm.BankSelectAllButton_ForTestOnly.Enabled);
			AssertEquals("BankUnSelectAllButton_ForTestOnly should be Disabled", false, testForm.BankUnSelectAllButton_ForTestOnly.Enabled);
			AssertEquals("SelectAllButton_ForTestOnly should be Disabled", false, testForm.SelectAllButton_ForTestOnly.Enabled);
			AssertEquals("UnSelectAllButton_ForTestOnly should be Disabled", false, testForm.UnSelectAllButton_ForTestOnly.Enabled);

			testForm.BankUnSelectAllButton_ForTestOnly.PerformClick();
			AssertEquals("Should not have any effect", true, testDepositBatchParent.DepositBatchLines[0].IsSelected);
			AssertEquals("Should not have any effect", true, testDepositBatchParent.DepositBatchLines[1].IsSelected);

			AssertEquals(testDepositBatchParent.DepositBatchLines[0].BankCode, testForm.BankCodeLabel_ForTestOnly.Text);

			testForm.UnSelectAllButton_ForTestOnly.PerformClick();
			AssertEquals("Should not have any effect", true, testDepositBatchParent.DepositBatchLines[0].IsSelected);
			AssertEquals("Should not have any effect", true, testDepositBatchParent.DepositBatchLines[1].IsSelected);
		}

		public void TestTabPages()
		{
			var batchParent = new DepositBatchParent(Factory);
			Factory.Save();

			using (var form = new DepositBatchForm(batchParent))
			{
				form.Show();
				var pages = form.Controls["BatchTabControl"].Controls.OfType<Control>();
				Assert(pages.Any(x => x.Text == "Logs"));
			}
		}

		public void TestNoEDocsPageWhenCreateNewDepositBatch()
		{
			var batchParent = new DepositBatchParent(Factory);
			Factory.Save();

			using (var form = new DepositBatchForm(batchParent))
			{
				form.Show();
				var pages = form.Controls["BatchTabControl"].Controls.OfType<Control>();
				Assert(!pages.Any(x => x.Text == "eDocs"));
			}
		}

		public void TestEDocsPageAvailableWhenViewDepositBatch()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			Business.CashBook.DepositBatch.DepositBatch testDepositBatch = testDepositBatchParent.DepositBatchLines[0];
			Factory.Save();

			using (DepositBatchForm testForm = new DepositBatchForm(new DepositBatchParent(Factory, testDepositBatch)))
			{
				testForm.Show();
				var tabControl = (ZTemplateTabControl)testForm.Controls["BatchTabControl"];
				tabControl.SelectTab("eDocsTabPage");
				Application.DoEvents();

				Assert("Covering label should not be visible.", !((IPlugInInternals)testForm.PlugIns.Instances[0]).CoveringLabel.Visible);
			}
		}

		public void TestPromptPrintDepositSlipWhenPost()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Now.Year, 1, 1));
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			Business.CashBook.DepositBatch.DepositBatch testDepositBatch = testDepositBatchParent.DepositBatchLines[0];
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.DepositBatch);
			using (DepositBatchForm testForm = (DepositBatchForm)controller.ShowFormForNewEntity(testDepositBatchParent))
			{
				testForm.Show();
				Application.DoEvents();
				AssertEquals("precondition", ODisplayMode.New, testForm.DisplayMode);

				testForm.FireSaveButton();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Do you want to print the Deposit Slip now?"));
			}
		}

		public void TestNotPromptPrintDepositSlipWhenSave()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Now.Year, 1, 1));
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			Business.CashBook.DepositBatch.DepositBatch testDepositBatch = testDepositBatchParent.DepositBatchLines[0];
			Factory.Save();

			using (DepositBatchForm testForm = new DepositBatchForm(new DepositBatchParent(Factory, testDepositBatch)))
			{
				testForm.Show();
				Application.DoEvents();
				AssertEquals("precondition", ODisplayMode.Browse, testForm.DisplayMode);

				testForm.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEDocsShouldBeEditableAfterSave()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Now.Year, 1, 1));
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent depositBatchParent = new DepositBatchParent(Factory);
			Business.CashBook.DepositBatch.DepositBatch depositBatch = depositBatchParent.DepositBatchLines[0];
			Factory.Save();

			using (DepositBatchForm testForm = new DepositBatchForm(new DepositBatchParent(Factory, depositBatch)))
			{
				testForm.Show();
				Application.DoEvents();

				var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
				StorageMain storageMain = masterFactory.New<StorageMain>();
				storageMain.SM_DB = 1;
				storageMain.SM_ParentFK = depositBatch.PK;
				depositBatch.RegisterEditableChildObject(storageMain);

				testForm.FireSaveButton();
				Assert(!storageMain.ReadOnly);
				Assert(depositBatch.ReadOnly);
			}
		}

		public void TestTabStopOnRadios()
		{
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);

			using (DepositBatchForm testForm = new DepositBatchForm(testDepositBatchParent))
			{
				testForm.InitialiseForm_ForTestOnly();
				testForm.Show();

				CheckTabStopRecursive(testForm.Controls);
			}
		}

		static void CheckTabStopRecursive(Control.ControlCollection controls)
		{
			List<Control> hasTabStop = new List<Control>();
			List<Control> noTabStop = new List<Control>();

			CheckTabStopRecursive(controls, hasTabStop, noTabStop);
		}

		static void CheckTabStopRecursive(Control.ControlCollection controls, List<Control> hasTabstop, List<Control> noTabstop)
		{
			foreach (Control control in controls)
			{
				if (control is RadioButton)
				{
					RadioButton radioControl = (RadioButton)control;
					Control parentControl = radioControl.Parent;

					if (parentControl is GroupBox)
					{
						if (radioControl.TabStop)
						{
							Assert("There can be only one RadioButton in the GroupBox with TabStop set to true: " + radioControl.Name, !hasTabstop.Contains(parentControl));
							hasTabstop.Add(parentControl);
							noTabstop.Remove(parentControl);
						}
						else if (!hasTabstop.Contains(parentControl))
						{
							noTabstop.Add(parentControl);
						}
						// else the GroupBox already has one radio with TabStop set to true
					}
					// else we assume it is a standalone RadioButton and TabStop can be set to anything
				}
				else
				{
					CheckTabStopRecursive(control.Controls, hasTabstop, noTabstop);
				}
			}
			Assert("At least one RadioButton in a GroupBox should have TabStop set to true", noTabstop.Count == 0);
		}

		[ExpectNoExceptions()]
		public void TestPrintDepositSlip()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);
			Factory.Save();

			using (DepositBatchForm testForm = new DepositBatchForm(testDepositBatchParent))
			{
				testForm.Show();
				testForm.PrintDepositSlip_ForTestOnly();
			}
		}

		public void TestIdentifierForPersistingForm()
		{
			var testReceipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			var testReceipt2 = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount2.PK);

			Factory.Save();
			var testDepositBatchParent = new DepositBatchParent(Factory);

			using (var testForm = new DepositBatchForm(testDepositBatchParent))
			{
				testForm.BankSelectAllButton_ForTestOnly.PerformClick();
				testForm.SelectAllButton_ForTestOnly.PerformClick();
				testForm.FPostButton_ForTestOnly.PerformClick();

				AssertEquals("IdentifierForPersistingForm should be DepositBatch PK.", ((DepositBatchParent)testForm.BusinessEntity).DepositBatchLines.FirstOrDefault().PK, testForm.IdentifierForPersistingForm);
			}
		}

		#region Implementation

		protected override bool ShouldTestForReversingReason
		{
			get { return false; }
		}

		#endregion
	}
}
