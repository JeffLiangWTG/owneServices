using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.AccountingVoucherPrint;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.AccountingVoucherPrinting.Testing
{
	[TestedType(typeof(AccoutingVoucherPrintForm))]
	public class AccountingVoucherPrintFormTest : ZFormBasherTest
	{
		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "TransactionCheckedListBox" ||
				control.Name == "LedgerCheckedListBox")
			{
				return true;
			}
			return base.ShouldIgnoreMissingBindingMember(control);
		}

		protected override Form GetFormToBashCore()
		{
			return new AccoutingVoucherPrintForm(new AccountingVoucherPrintWrapper());
		}

		public void TestAttachButtonDoesNotNeedSecurityCheckPointEnabled()
		{
			AccountingVoucherPrintWrapper testWrapper = new AccountingVoucherPrintWrapper();
			using (AccoutingVoucherPrintForm testForm = new AccoutingVoucherPrintForm(testWrapper))
			{
				testForm.Show();

				Env.Security.BranchModify.IsAllowed = false;

				testForm.ZModuleButtonGrid1_ForTestOnly.AttachButtonForTest.PerformClick();

				AssertNull("No security error should be shown when user does not have edit permission to branch", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestGenerateButton_ClickIfValidationFail()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();
			var formMock = new Mock<AccoutingVoucherPrintForm>(testWrapper) { CallBase = true };
			AccoutingVoucherPrintForm testForm = formMock.Object;

			formMock.Protected().Setup<bool>("HasWrapperValidationError").Returns(true);

			using (testForm)
			{
				testForm.GenerateButton_Click_ForTestOnly(null, new EventArgs());
				formMock.VerifyAll();
			}
		}

		[ExpectNoExceptions]
		public void TestGenerateButton_ClickIfValidationSucceeds()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();
			var formMock = new Mock<AccoutingVoucherPrintForm>(new object[] { testWrapper }) { CallBase = true };
			AccoutingVoucherPrintForm testForm = formMock.Object;

			formMock
				.Protected()
				.Setup<bool>("HasWrapperValidationError")
				.Returns(false);
			formMock
				.Protected()
				.Setup("PrintAccountingVouchers");

			using (testForm)
			{
				testForm.GenerateButton_Click_ForTestOnly(null, new EventArgs());
				formMock.VerifyAll();
			}
		}

		public void TestOnLoadedCallsCorrectMethods()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();
			var formMock = new Mock<AccoutingVoucherPrintForm>(new object[] { testWrapper }) { CallBase = true };
			AccoutingVoucherPrintForm testForm = formMock.Object;

			var count = 0;
			formMock
				.Protected()
				.Setup("SetCheckedListBoxItems", ItExpr.IsAny<OptionalFilterCriteriaList>(), ItExpr.IsAny<CheckedListBox>())
				.Callback((OptionalFilterCriteriaList ledgerList, CheckedListBox listBox) =>
				{
					AssertEquals("Number of Ledgers in list.", count == 0 ? 5 : 0, ledgerList.Count);
					AssertNotNull("Number of docs in pack", listBox);
					count++;
				});

			using (testForm)
			{
				testForm.OnLoad_ForTestOnly(new EventArgs());
				formMock.VerifyAll();
			}
		}

		[ExpectNoExceptions]
		public void TestPrintAccountingVouchers()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();
			var formMock = new Mock<AccoutingVoucherPrintForm>(new object[] { testWrapper }) { CallBase = true };
			AccoutingVoucherPrintForm testForm = formMock.Object;

			formMock
				.Protected()
				.Setup("SynchroniseListCheckedStatus", ItExpr.IsAny<OptionalFilterCriteriaList>(), ItExpr.IsAny<CheckedListBox>());
			formMock
				.Protected()
				.Setup("GenerateVoucherDocument");
			formMock
				.Protected()
				.Setup<bool>("CanPrintVoucherProceed")
				.Returns(true);
			formMock
				.Protected()
				.Setup("PrintVoucherDocument");

			using (testForm)
			{
				testForm.PrintAccountingVouchers_ForTestOnly();
				formMock.VerifyAll();
			}
		}

		[ExpectNoExceptions]
		public void TestPrintAccountingVouchersIfPrintCannotProceed()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();
			var formMock = new Mock<AccoutingVoucherPrintForm>(new object[] { testWrapper }) { CallBase = true };
			var testForm = formMock.Object;

			formMock
				.Protected()
				.Setup("SynchroniseListCheckedStatus", ItExpr.IsAny<OptionalFilterCriteriaList>(), ItExpr.IsAny<CheckedListBox>());
			formMock
				.Protected()
				.Setup("GenerateVoucherDocument");
			formMock
				.Protected()
				.Setup<bool>("CanPrintVoucherProceed")
				.Returns(false);

			using (testForm)
			{
				testForm.PrintAccountingVouchers_ForTestOnly();
				formMock
					.Protected()
					.Verify("PrintVoucherDocument", Times.Never());
				formMock.VerifyAll();
			}
		}

		public void TestFormClose()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();
			using (var testForm = new AccoutingVoucherPrintForm(testWrapper))
			{
				testForm.Show();
				Assert(testForm.Visible);
				testForm.CloseButton_Click_ForTestOnly(this, new EventArgs());
				Assert(!testForm.Visible);
			}
		}

		public void TestSelectedGenerateLedgerList()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();
			using (var testForm = new AccoutingVoucherPrintForm(testWrapper))
			{
				testForm.SetCheckedListBoxItems_ForTestOnly(testWrapper.LedgerTypeList, testForm.LedgerCheckedListBox_ForTestOnly);

				AssertEquals("Ledger Types", 5, testForm.LedgerCheckedListBox_ForTestOnly.Items.Count);
				AssertEquals(testWrapper.LedgerTypeList[0].Description, ((OptionalFilterCriteria)testForm.LedgerCheckedListBox_ForTestOnly.Items[0]).Description);
			}
		}

		public void TestSetCheckedListBox()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();

			testWrapper.LedgerTypeList[0].Enabled = true;
			testWrapper.LedgerTypeList[3].Enabled = true;

			using (var testForm = new AccoutingVoucherPrintForm(testWrapper))
			{
				testForm.SetCheckedListBoxItems_ForTestOnly(testWrapper.LedgerTypeList, testForm.LedgerCheckedListBox_ForTestOnly);

				AssertEquals("Ledger Types", 5, testForm.LedgerCheckedListBox_ForTestOnly.Items.Count);
				AssertEquals(CheckState.Checked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(0));
				AssertEquals(CheckState.Unchecked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(1));
				AssertEquals(CheckState.Unchecked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(2));
				AssertEquals(CheckState.Checked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(3));
				AssertEquals(CheckState.Unchecked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(4));
			}
		}

		public void TestSetCheckedStateToTheLedgerList()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();
			using (var testForm = new AccoutingVoucherPrintForm(testWrapper))
			{
				testForm.SetCheckedListBoxItems_ForTestOnly(testWrapper.LedgerTypeList, testForm.LedgerCheckedListBox_ForTestOnly);

				testForm.LedgerCheckedListBox_ForTestOnly.SetItemChecked(0, true);
				testForm.LedgerCheckedListBox_ForTestOnly.SetItemChecked(3, true);

				Assert(!testWrapper.LedgerTypeList[0].Enabled);
				Assert(!testWrapper.LedgerTypeList[1].Enabled);
				Assert(!testWrapper.LedgerTypeList[2].Enabled);
				Assert(!testWrapper.LedgerTypeList[3].Enabled);
				Assert(!testWrapper.LedgerTypeList[4].Enabled);

				testForm.SynchroniseListCheckedStatus_ForTestOnly(testWrapper.LedgerTypeList, testForm.LedgerCheckedListBox_ForTestOnly);

				Assert(testWrapper.LedgerTypeList[0].Enabled);
				Assert(!testWrapper.LedgerTypeList[1].Enabled);
				Assert(!testWrapper.LedgerTypeList[2].Enabled);
				Assert(testWrapper.LedgerTypeList[3].Enabled);
				Assert(!testWrapper.LedgerTypeList[4].Enabled);
			}
		}

		public void TestClearList()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();
			using (var testForm = new AccoutingVoucherPrintForm(testWrapper))
			{
				testForm.SetCheckedListBoxItems_ForTestOnly(testWrapper.LedgerTypeList, testForm.LedgerCheckedListBox_ForTestOnly);

				AssertEquals("Ledger Types", 5, testForm.LedgerCheckedListBox_ForTestOnly.Items.Count);
				AssertEquals(testWrapper.LedgerTypeList[0].Description, ((OptionalFilterCriteria)testForm.LedgerCheckedListBox_ForTestOnly.Items[0]).Description);

				testForm.ClearCheckedListBox_ForTestOnly(testForm.LedgerCheckedListBox_ForTestOnly);
				AssertEquals("Item Cleared", 0, testForm.LedgerCheckedListBox_ForTestOnly.Items.Count);
			}
		}

		public void TestSelectAllList()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();
			using (var testForm = new AccoutingVoucherPrintForm(testWrapper))
			{
				testForm.SetCheckedListBoxItems_ForTestOnly(testWrapper.LedgerTypeList, testForm.LedgerCheckedListBox_ForTestOnly);

				AssertEquals("Ledger Types", 5, testForm.LedgerCheckedListBox_ForTestOnly.Items.Count);
				AssertEquals(CheckState.Unchecked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(0));
				AssertEquals(CheckState.Unchecked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(1));
				AssertEquals(CheckState.Unchecked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(2));
				AssertEquals(CheckState.Unchecked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(3));
				AssertEquals(CheckState.Unchecked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(4));

				testForm.SelectAllList_ForTestOnly(testForm.LedgerCheckedListBox_ForTestOnly);

				AssertEquals(CheckState.Checked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(0));
				AssertEquals(CheckState.Checked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(1));
				AssertEquals(CheckState.Checked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(2));
				AssertEquals(CheckState.Checked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(3));
				AssertEquals(CheckState.Checked, testForm.LedgerCheckedListBox_ForTestOnly.GetItemCheckState(4));
			}
		}

		public void TestzModuleButtonGridColumnIsReadOnly()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();
			using (var testForm = new AccoutingVoucherPrintForm(testWrapper))
			{
				AssertEquals(2, testForm.ZModuleButtonGrid1_ForTestOnly.ColumnStyles.Count);
				Assert(((ZTextBoxColumnStyleInfo)testForm.ZModuleButtonGrid1_ForTestOnly.ColumnStyles[0]).IsReadOnly);
				Assert(((ZTextBoxColumnStyleInfo)testForm.ZModuleButtonGrid1_ForTestOnly.ColumnStyles[1]).IsReadOnly);
			}
		}

		public void TestPrintOptionsGroupBox()
		{
			var testWrapper = new AccountingVoucherPrintWrapper();

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderAccountingVoucher.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var testForm = new AccoutingVoucherPrintForm(testWrapper))
				{
					Assert("If is use legacy AccountingVoucher, AccoutingVoucherPrintForm should have PrintOptionsGroupBox_ForTestOnly", testForm.MainPanel_ForTestOnly.Controls.Contains(testForm.PrintOptionsGroupBox_ForTestOnly));
				}
			}

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderAccountingVoucher.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var testForm = new AccoutingVoucherPrintForm(testWrapper))
				{
					Assert("If is use DocBuilder AccountingVoucher, AccoutingVoucherPrintForm should not have PrintOptionsGroupBox_ForTestOnly", !testForm.MainPanel_ForTestOnly.Controls.Contains(testForm.PrintOptionsGroupBox_ForTestOnly));
				}
			}
		}
	}
}
