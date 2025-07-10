using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.Testing
{
	public abstract class AccQueryClaimFormTest : ZFormBasherTest
	{
		public void TestActionMenuItemsVisibility()
		{
			using (AccQueryClaimForm form = (AccQueryClaimForm)GetFormToBash())
			{
				form.Show();
				MenuItem reassign = GetActionsMenuItem(form, "&Reassign Claim");
				MenuItem cancel = GetActionsMenuItem(form, "&Cancel Claim Charges");
				Assert("Reassign claim menu should be visible", reassign.Visible);
				Assert("Cancel claim charges menu should NOT be visible", !cancel.Visible);

				form.TabControl_ForTestOnly.SelectTab(form.PlugIns.GetPlugIn(ControllerIDs.ClaimCharges).TabPage);
				Assert("Reassign claim menu should NOT be visible", !reassign.Visible);
				Assert("Cancel claim charges menu should be visible", cancel.Visible);
			}
		}

		public void TestActionReassignClaim()
		{
			using (AccQueryClaimForm form = (AccQueryClaimForm)GetFormToBash())
			{
				GetActionsMenuItem(form, "&Reassign Claim").PerformClick();
				Assert("Reassign Claim form should be shown.", ZFormModaliser.LastFormShownDialogForTest is AssignClaimPopupForm);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		public void TestActionCancelClaimCharges()
		{
			using (AccQueryClaimForm form = (AccQueryClaimForm)GetFormToBash())
			{
				form.Show();
				form.TabControl_ForTestOnly.SelectTab(form.PlugIns.GetPlugIn(ControllerIDs.ClaimCharges).TabPage);
				GetActionsMenuItem(form, "&Cancel Claim Charges").PerformClick();
				AssertEquals("The Claim will be saved after canceling Claim Charges.\r\nDo you really want to cancel the Claim Charges and save the Claim?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public virtual void TestInvoiceNumberControlUseCorrectCodeProperty()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			Factory.Save();
			AssertNotEquals("Precondition: AH_TransactionNum not equal to AH_ConsolidatedInvoiceRef.", invoice.AH_TransactionNum, invoice.AH_ConsolidatedInvoiceRef);
			using (var form = (AccQueryClaimForm)GetFormToBash())
			{
				form.Claim_ForTestOnly.AY_OH_Debtor = invoice.AH_OH;
				form.Show();

				var accQueryClaimUserControl = form.Controls.Find("accQueryClaimUserControl", true)[0];
				var invoiceFindBox = (ZGuidFindBox)accQueryClaimUserControl.Controls.Find("InvoiceGuidFindBox", true)[0];
				var contactGuidDropEdit = accQueryClaimUserControl.Controls.Find("ContactGuidDropEdit", true)[0];

				invoiceFindBox.Focus();
				invoiceFindBox.CodeBox.Text = invoice.AH_TransactionNum;
				contactGuidDropEdit.Focus();
				AssertEquals("AY_AH", invoice.PK, form.Claim_ForTestOnly.AY_AH);
			}
		}

		public void TestValidateAndSave_EnableUserContextTracer()
		{
			var invoice = CreateValidatedInvoice();
			using (var form = (AccQueryClaimForm)GetFormToBash())
			{
				form.Claim_ForTestOnly.AY_OH_Debtor = invoice.AH_OH;
				form.Claim_ForTestOnly.AY_AH = invoice.PK;
				form.Claim_ForTestOnly.AY_OC = invoice.Header.Contacts.First().PK;
				form.Claim_ForTestOnly.AY_QueryClaimAmount = 10M;
				form.Claim_ForTestOnly.AY_ShortDescriptionOfClaim = (NoResString)"AAA";
				form.Show();

				form.Claim_ForTestOnly.RunPreSaveValidation();
				AssertEquals("PreCondition, should not have error.", false, form.Claim_ForTestOnly.HasErrors);

				AssertNull("PreCondition, logger is not enabled before clicking save button.", Environment.Env.Instance.UserContextLogger);
				var haveClickedSaveButton = false;
				form.ValidatingForSave += (s,e) => {
					haveClickedSaveButton = true;

					var logger = Environment.Env.Instance.UserContextLogger;
					AssertNotNull("Have logger instance", logger);
					AssertType<Enterprise.Environment.UserContextSwitchLogger>("Logger type", logger);
				};
				form.FireSaveButton();

				AssertEquals("ValidatingForSave should be invoked.", true, haveClickedSaveButton);
			}
		}

		protected abstract InvoicingBase CreateValidatedInvoice();

		#region Implementation

		MenuItem GetActionsMenuItem(Form form, string text)
		{
			MenuItem actionsMenu = GetActionsMenu(form);
			foreach (MenuItem actionItem in actionsMenu.MenuItems)
			{
				if (actionItem.Text == text)
				{
					return actionItem;
				}
			}
			return null;
		}

		MenuItem GetActionsMenu(Form form)
		{
			foreach (MenuItem item in form.Menu.MenuItems)
			{
				if (item.Text == "Actio&ns")
				{
					return item;
				}
			}
			return null;
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
