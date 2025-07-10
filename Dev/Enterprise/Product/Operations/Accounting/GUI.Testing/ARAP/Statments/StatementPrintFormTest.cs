using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Statements.Testing
{
	[TestedType(typeof(StatementPrintForm))]
	public class StatementPrintFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new StatementPrintForm(Statement.New(GlbBranch.CurrentBranch));
		}

		protected override bool AllowFormSizeFixed => true;

		public void TestOnLoadBuildMenu()
		{
			Statement bizObj = Statement.New(GlbBranch.CurrentBranch);

			using (StatementPrintForm form = new StatementPrintForm(bizObj))
			{
				form.Show();

				var fileNewMenu = form.Menu.MenuItems.FindByText("File");
				AssertEquals("File menu should be hidden", false, fileNewMenu.Visible);
				var editMenu = form.Menu.MenuItems.FindByText("Edit");
				AssertEquals("Text menu should be hidden", true, editMenu.Visible);
				var actionMenu = form.Menu.MenuItems.FindByText("Actions");
				AssertEquals("Action menu should be hidden", false, actionMenu.Visible);
				var helpMenu = form.Menu.MenuItems.FindByText("Help");
				AssertEquals("Help menu should be hidden", false, helpMenu.Visible);
				var documentsMenu = form.Menu.MenuItems.FindByText("Documents");
				AssertEquals("Documents menu should be shown", true, documentsMenu.Visible);
			}
		}

		public void TestConstruction_PlugIns()
		{
			Statement bizObj = Statement.New(GlbBranch.CurrentBranch);

			using (StatementPrintForm form = new StatementPrintForm(bizObj))
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		public void TestControlsForStatementDocumentTypeChanged()
		{
			Statement bizObj = Statement.New(GlbBranch.CurrentBranch);

			using (StatementPrintForm form = new StatementPrintForm(bizObj))
			{
				form.Show();

				bizObj.DocumentToPrint = ZString.Empty;
				bizObj.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;

				AssertEquals("CutoffDateLabel Text", "Invoice Dates on or Before", form.CutoffDateEdit_ForTestOnly.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("OutstandingAmountLabel Text", "Statements as at end of selected period", form.OutstandingAmountCalcEdit_ForTestOnly.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("CutoffPeriod Visible", true, form.CutoffPeriodEdit_ForTestOnly.Visible);
				AssertEquals("OutstandingAmountCalcedit Visible", false, form.OutstandingAmountCalcEdit_ForTestOnly.Visible);
				AssertEquals("DisbursementInvoicesCheckBox_ForTestOnly Visible", true, form.DisbursementInvoicesCheckBox_ForTestOnly.Visible);

				bizObj.DocumentToPrint = Core.Constants.StatementCollectionLetterType.FirstReminder;
				AssertEquals("CutoffDateLabel Text", "Due Dates on or Before", form.CutoffDateEdit_ForTestOnly.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("OutstandingAmountLabel Text", "Local Outstanding Amount Greater than/Equal to", form.OutstandingAmountCalcEdit_ForTestOnly.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("CutoffPeriod Visible", false, form.CutoffPeriodEdit_ForTestOnly.Visible);
				AssertEquals("OutstandingAmountCalcedit Visible", true, form.OutstandingAmountCalcEdit_ForTestOnly.Visible);
				AssertEquals("DisbursementInvoicesCheckBox_ForTestOnly Visible", true, form.DisbursementInvoicesCheckBox_ForTestOnly.Visible);

				bizObj.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
				AssertEquals("CutoffDateLabel Text", "Invoice Dates on or Before", form.CutoffDateEdit_ForTestOnly.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("OutstandingAmountLabel Text", "Statements as at end of selected period", form.OutstandingAmountCalcEdit_ForTestOnly.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("CutoffPeriod Visible", true, form.CutoffPeriodEdit_ForTestOnly.Visible);
				AssertEquals("OutstandingAmountCalcedit Visible", false, form.OutstandingAmountCalcEdit_ForTestOnly.Visible);
				AssertEquals("DisbursementInvoicesCheckBox_ForTestOnly Visible", true, form.DisbursementInvoicesCheckBox_ForTestOnly.Visible);
			}
		}

		public void TestControlsForIssueBySettlementDocumentTypeChanged()
		{
			Statement bizObj = Statement.New(GlbBranch.CurrentBranch);
			using (StatementPrintForm form = new StatementPrintForm(bizObj))
			{
				form.Show();
				bizObj.IssueBySettlementGroup = false;
				AssertEquals("Organisation GroupBox Text", "Select which Debtors(s) to Print For", form.OrganisationGroupBox_ForTestOnly.Text);
				AssertEquals("Organisation Label Text", "Debtor", form.OrganisationGuidFindBox_ForTestOnly.GetExtension<LabelCaptionRenderer>().Caption);

				bizObj.IssueBySettlementGroup = true;
				AssertEquals("Organisation GroupBox Text", "Select which Settlement Group(s) to Print For", form.OrganisationGroupBox_ForTestOnly.Text);
				AssertEquals("Organisation Label Text", "Settlement Group", form.OrganisationGuidFindBox_ForTestOnly.GetExtension<LabelCaptionRenderer>().Caption);

				bizObj.IssueBySettlementGroup = false;
				AssertEquals("Organisation GroupBox Text", "Select which Debtors(s) to Print For", form.OrganisationGroupBox_ForTestOnly.Text);
				AssertEquals("Organisation Label Text", "Debtor", form.OrganisationGuidFindBox_ForTestOnly.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		public void TestControlsForIssueByTransactionBranchChanged()
		{
			Statement bizObj = Statement.New(GlbBranch.CurrentBranch);
			using (StatementPrintForm form = new StatementPrintForm(bizObj))
			{
				form.Show();
				form.Statement.TransactionBranch_PK = ZGuid.NewZGuid();
				bizObj.IssueByTransactionBranch = false;
				Assert("Transaction Branch FindBox must be disabled", !form.TransactionBranchFindBox_ForTestOnly.Enabled);
				AssertEquals("TransactionBranch_PK must be empty", ZGuid.Empty, form.Statement.TransactionBranch_PK);

				bizObj.IssueByTransactionBranch = true;
				Assert("Transaction Branch FindBox must be enabled", form.TransactionBranchFindBox_ForTestOnly.Enabled);
				AssertEquals("TransactionBranch_PK must be empty", ZGuid.Empty, form.Statement.TransactionBranch_PK);
				ZGuid newTransactionBranch_PK = ZGuid.NewZGuid();
				form.Statement.TransactionBranch_PK = newTransactionBranch_PK;
				AssertEquals("TransactionBranch_PK must have new value", newTransactionBranch_PK, form.Statement.TransactionBranch_PK);

				bizObj.IssueByTransactionBranch = false;
				Assert("Transaction Branch FindBox must be disabled", !form.TransactionBranchFindBox_ForTestOnly.Enabled);
				AssertEquals("TransactionBranch_PK must be empty", ZGuid.Empty, form.Statement.TransactionBranch_PK);

				bizObj.IssueByTransactionBranch = true;
				Assert("Transaction Branch FindBox must be enabled", form.TransactionBranchFindBox_ForTestOnly.Enabled);
				AssertEquals("TransactionBranch_PK must have previous value", newTransactionBranch_PK, form.Statement.TransactionBranch_PK);
			}
		}

		public void TestControlsForIssueByTransactionDepartmentChanged()
		{
			Statement bizObj = Statement.New(GlbBranch.CurrentBranch);
			using (StatementPrintForm form = new StatementPrintForm(bizObj))
			{
				form.Show();
				form.Statement.TransactionDepartment_PK = ZGuid.NewZGuid();
				bizObj.IssueByTransactionDepartment = false;
				Assert("Transaction Department FindBox must be disabled", !form.DepartmentGuidFindBox_ForTestOnly.Enabled);
				AssertEquals("TransactionDepartment_PK must be empty", ZGuid.Empty, form.Statement.TransactionDepartment_PK);

				bizObj.IssueByTransactionDepartment = true;
				Assert("Transaction Department FindBox must be enabled", form.DepartmentGuidFindBox_ForTestOnly.Enabled);
				AssertEquals("TransactionDepartment_PK must be empty", ZGuid.Empty, form.Statement.TransactionDepartment_PK);
				ZGuid newTransactionDepartment_PK = ZGuid.NewZGuid();
				form.Statement.TransactionDepartment_PK = newTransactionDepartment_PK;
				AssertEquals("TransactionDepartment_PK must have new value", newTransactionDepartment_PK, form.Statement.TransactionDepartment_PK);

				bizObj.IssueByTransactionDepartment = false;
				Assert("Transaction Department FindBox must be disabled", !form.TransactionBranchFindBox_ForTestOnly.Enabled);
				AssertEquals("TransactionBranch_PK must be empty", ZGuid.Empty, form.Statement.TransactionDepartment_PK);

				bizObj.IssueByTransactionDepartment = true;
				Assert("Transaction Department FindBox must be enabled", form.DepartmentGuidFindBox_ForTestOnly.Enabled);
				AssertEquals("TransactionDepartment_PK must have previous value", newTransactionDepartment_PK, form.Statement.TransactionDepartment_PK);
			}
		}

		public void TestCloseWithChangesDoesntAskSave()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Statement bizObj = Statement.New(GlbBranch.CurrentBranch);
			bizObj.OH_PK = ZGuid.NewZGuid();

			var form = new StatementPrintForm(bizObj);
			form.DisplayMode = ODisplayMode.NewSaved;
			form.Show();
			form.FormCancelButton_Click_ForTestOnly(this, EventArgs.Empty);

			AssertEquals(string.Format("No message should be shown to user message was:\r\n {0}", UnitTestUserNotification.Instance.LastMessage.Text), true, UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestMessageIsShownWhenThereAreNoStatementsToPrint()
		{
			StatementForTest bizObj = new StatementForTest(GlbBranch.CurrentBranch);

			using (StatementPrintForm form = new StatementPrintForm(bizObj))
			{
				form.Show();
				bizObj.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
				bizObj.ShowNoDocumentsToPrintMessageExposed();

				ZString expectedText = "Based on the given criteria, there are currently no statements to print.";
				AssertEquals("Last Message Shown", expectedText, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMessageIsNotShownWhenUnPublishedARInvoiceDocumentIsFound_LegacyInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			TestMessageIsNotShownWhenUnPublishedARInvoiceDocumentIsFound_Core("Invoice");
		}

		public void TestMessageIsNotShownWhenUnPublishedARInvoiceDocumentIsFound_DocBuilderInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestMessageIsNotShownWhenUnPublishedARInvoiceDocumentIsFound_Core("DocBuilder Invoice");
		}

		void TestMessageIsNotShownWhenUnPublishedARInvoiceDocumentIsFound_Core(string sU_MenuName)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(StmMenuItemSchema.SU_MenuName, sU_MenuName);
			query.AddToFilter(StmMenuItemSchema.SU_IsPublished, true);
			StmMenuItem[] invoiceMenuItems = Factory.Load<StmMenuItem>(query);
			foreach (StmMenuItem invoiceMenuItem in invoiceMenuItems)
			{
				invoiceMenuItem.SU_IsPublished = false;
			}

			OrgHeader header = TestObjectCreator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice invoice = (Invoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m);
			invoice.AH_OH = header.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 100);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 200);

			Factory.Save();

			Statement statement = Statement.New(GlbBranch.CurrentBranch);
			statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			statement.DoAccountFeeTransaction = false;

			using (StatementPrintForm form = new StatementPrintForm(statement))
			{
				form.Show();
				form.PrintButton_Click_ForTestOnly(this, EventArgs.Empty);
				AssertNull("there should be no message popup", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		class StatementForTest : Statement
		{
			public StatementForTest(GlbBranch branch)
				: base(branch)
			{
			}

			public void ShowNoDocumentsToPrintMessageExposed()
			{
				base.ShowNoDocumentsToPrintMessage();
			}
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
