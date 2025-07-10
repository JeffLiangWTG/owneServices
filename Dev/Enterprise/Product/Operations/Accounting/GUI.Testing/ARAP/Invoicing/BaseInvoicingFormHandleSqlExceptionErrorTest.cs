using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class BaseInvoicingFormHandleSqlExceptionErrorTest : TestCaseWithFactory
	{
		public void TestHandleSqlExceptionError()
		{
			var creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriods(ZDateTime.Today);

			var testInvoice = creator.CreateInvoiceWithLine(typeof(APInvoice), "001", creator.AUD, 1.0m, 20m, 0.0m, 20m, 0.0m);
			testInvoice.SaveAsIncomplete();
			using (var testForm = new TestBaseInvoiceForm(testInvoice))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Assert("Set true on initialization", testForm.IsLastSaveSuccessful);
				try
				{
					testForm.Show();
					Application.DoEvents();
					testForm.FireSaveButton();
					Fail("Should stop execution. The exception is caught higher in the stack.");
				}
				catch { }
				Assert("Set false on sql Exception error", !testForm.IsLastSaveSuccessful);
				AssertEquals("Form becomes readonly", ODisplayMode.ReadOnly, testForm.DisplayMode);
				Assert("Save as incomplete is available", testForm.IsSaveAsIncompleteEnabled);
			}
			var test = UnitTestUserNotification.Instance.LastMessage;
			AssertContains("Should contain extra message", "You can also try to save the transaction as 'incomplete'", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestARInvoicePreviewInvoiceSecurity()
		{
			var creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriods(ZDateTime.Today);
			var invoice = creator.CreateInvoice(typeof(ARInvoice), creator.AUD, 1.0m);
			invoice.AH_OH = creator.TestOrganisation.PK;
			creator.CreateInvoiceLine(invoice, creator.GLHeader1.PK, 100);
			var securityHelper = new JobInvoicingSecurityHelper(Env.Security.NewReceivablesInvoice);
			AssertInvoiceSecurities(invoice, securityHelper);
		}

		public void TestARCreditNotePreviewInvoiceSecurity()
		{
			var creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriods(ZDateTime.Today);
			var invoice = creator.CreateInvoice(typeof(ARCreditNote), creator.AUD, 1.0m);
			invoice.AH_OH = creator.TestOrganisation.PK;
			creator.CreateInvoiceLine(invoice, creator.GLHeader1.PK, 100);
			var securityHelper = new JobInvoicingSecurityHelper(Env.Security.NewReceivablesCreditNote);
			AssertInvoiceSecurities(invoice, securityHelper);
		}

		public void TestARAdjustmentNotePreviewInvoiceSecurity()
		{
			var creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriods(ZDateTime.Today);
			var invoice = creator.CreateInvoice(typeof(ARAdjustmentNote), creator.AUD, 1.0m);
			invoice.AH_OH = creator.TestOrganisation.PK;
			creator.CreateInvoiceLine(invoice, creator.GLHeader1.PK, 100);
			var securityHelper = new JobInvoicingSecurityHelper(Env.Security.NewReceivablesAdjustmentNote);
			AssertInvoiceSecurities(invoice, securityHelper);
		}

		void AssertInvoiceSecurities(InvoicingBase invoice, JobInvoicingSecurityHelper securityHelper)
		{
			var previewInvoicesCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewInvoices);
			var previewOnlyCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewOnly);

			using (var invForm = new BaseInvoicingForm(invoice))
			{
				invForm.Show();

				previewInvoicesCheckpoint.IsAllowed = false;
				previewOnlyCheckpoint.IsAllowed = false;
				invForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Preview Invoice").PerformClick();
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, previewOnlyCheckpoint.ErrorMessageForNotAllowed);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				previewInvoicesCheckpoint.IsAllowed = true;
				invForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Preview Invoice").PerformClick();
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, previewOnlyCheckpoint.ErrorMessageForNotAllowed);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				previewOnlyCheckpoint.IsAllowed = true;
				invForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Preview Invoice").PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				CloseOpenedForms();
			}
		}

		void CloseOpenedForms()
		{
			List<Form> previewForms = new List<Form>();
			for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
			{
				var previewForm = Application.OpenForms[i] as DocumentEngine.GUI.XLSPreviewForm;
				if (previewForm != null)
				{
					previewForm.Close();
				}
			}
		}

		#region Implementation

		public class TestBaseInvoiceForm : BaseInvoicingForm
		{
			public TestBaseInvoiceForm(InvoicingBase businessEntity) : base(businessEntity)
			{
			}

			protected override ContinueWithSave ValidateAndSave()
			{
				var result = base.ValidateAndSave();
				CreateAndThrowSqlException();
				return result;
			}

			public bool IsLastSaveSuccessful { get { return LastSaveSuccessful; } }

			public bool IsSaveAsIncompleteEnabled { get { return SaveAsIncompleteMenuItem.Enabled; } }

			public void CreateAndThrowSqlException()
			{
				var table = new DataTable("BlahBlah");
				var col = new DataColumn("PK", typeof(Guid));
				table.Columns.Add(col);
				table.PrimaryKey = new DataColumn[] { col };
				var row = table.NewRow();
				var invoice = (Invoice)BusinessEntity;
				try
				{
					#pragma warning disable CW1116 // Test purpose
					var conn = new SqlConnection(@"Data Source=.;Database=GUARANTEED_TO_FAIL;Connection Timeout=1");  // On connection will be created, just to throw the exception
					#pragma warning restore CW1116 // Test purpose
					conn.Open();
				}
				catch (System.Data.Common.DbException ex)
				{
					var innerEx = new ZDataException(ex, row, Db.Connection);
					var saveException = new ZSaveException(innerEx, invoice.Factory);
					throw saveException;
				}
			}
		}

		#endregion
	}
}
