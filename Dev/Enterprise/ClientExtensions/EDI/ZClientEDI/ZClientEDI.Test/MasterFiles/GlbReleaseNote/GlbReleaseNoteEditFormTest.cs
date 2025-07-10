using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Global.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Gui
{
	[TestedType(typeof(GlbReleaseNoteEditForm))]
	class GlbReleaseNoteEditFormTest : GlbReleaseNoteTestCase
	{
		public void TestDefaultFormLayout()
		{
			using (GlbReleaseNoteEditForm form = GetFormToBash())
			{
				form.Show();
				AssertIsCheckedOut(form, false);
			}
		}

		public void TestCheckout()
		{
			using (GlbReleaseNoteEditForm form = GetFormToBash())
			{
				form.Show();
				AssertNull("Precondition: There should not be an error message.", UnitTestUserNotification.Instance.LastMessage.Text);
				Manager.CheckoutErrorMessage = "Cannot Checkout!";
				Manager.CheckoutResult = false;
				form.CheckoutButton.PerformClick();
				string expectedErrorMessage = "Update notes cannot be checked out at the moment. The error is:" + System.Environment.NewLine + System.Environment.NewLine + "Cannot Checkout!";
				AssertEquals("An error message should be shown.", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsCheckedOut(form, false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Manager.CheckoutResult = true;
				form.CheckoutButton.PerformClick();
				AssertNull("There should not be an error message.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsCheckedOut(form, true);
			}
		}

		public void TestUndoCheckout()
		{
			PrepareTestData();
			Factory.Save();
			using (GlbReleaseNoteEditForm form = GetFormToBash())
			{
				form.Show();
				form.Closed += new EventHandler(Form_Closed);
				try
				{
					Manager.CheckoutResult = true;
					form.Checkout();
					Manager.ReleaseNotes.DeleteAll();
					AssertIsCheckedOut(form, true);
					AssertNull("Precondition: There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					form.UndoCheckoutButton.PerformClick();
					AssertEquals("A question should be asked.", "Do you really want to undo checkout, lose all changes and close this form?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("BusinessEntity.UndoCheckout() should not be called.", false, Manager.UndoCheckoutCalled);
					AssertIsCheckedOut(form, true);
					AssertEquals("Form should not be closed.", false, IsFormClosed);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.UndoCheckoutButton.PerformClick();
					AssertEquals("BusinessEntity.UndoCheckout() should be called.", true, Manager.UndoCheckoutCalled);
					AssertEquals("Form should be closed.", true, IsFormClosed);
				}
				finally
				{
					form.Closed -= new EventHandler(Form_Closed);
				}
			}

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertNotNull("Note1 should not have been deleted.", newFactory.Load<GlbReleaseNote>(Note1.PK));
			AssertNotNull("Note2 should not have been deleted.", newFactory.Load<GlbReleaseNote>(Note2.PK));
			AssertNotNull("Note3 should not have been deleted.", newFactory.Load<GlbReleaseNote>(Note3.PK));
		}

		public void TestCheckIn()
		{
			PrepareTestData();
			using (GlbReleaseNoteEditForm form = GetFormToBash())
			{
				form.Show();
				Manager.CheckoutResult = true;
				form.CheckoutButton.PerformClick();
				AssertNull("Precondition: There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				form.CheckInButton.PerformClick();
				AssertEquals("An error message should be shown.", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsCheckedOut(form, true);
				AssertEquals("BusinessEntity.CheckIn() should not be called.", false, Manager.CheckInCalled);
				Note1.GF_RN_NKCountryForReleaseNote = "";
				Note2.GF_RN_NKCountryForReleaseNote = "";
				Note3.GF_RN_NKCountryForReleaseNote = "";
				Note1.GF_Summary = "Note1";
				Note2.GF_Summary = "Note2";
				Note3.GF_Summary = "Note3";
				Note1.GF_Category = Note1.Lookups.Categories[1].Code;
				Note2.GF_Category = Note1.Lookups.Categories[1].Code;
				Note3.GF_Category = Note1.Lookups.Categories[1].Code;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.CheckInButton.PerformClick();
				AssertNull("There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsCheckedOut(form, false);
				AssertEquals("BusinessEntity.CheckIn() should be called.", true, Manager.CheckInCalled);
			}
		}

		public void TestCannotCloseIfCheckedOut()
		{
			using (GlbReleaseNoteEditForm form = GetFormToBash())
			{
				form.Show();
				form.Closed += new EventHandler(Form_Closed);
				try
				{
					AssertNull("Precondition: There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);
					Manager.CheckoutResult = true;
					form.CheckoutButton.PerformClick();
					form.Close();
					AssertEquals("An error message should be shown.", "You must undo checkout or check in first before closing this form.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Form should not be closed.", false, IsFormClosed);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.CheckInButton.PerformClick();
					form.Close();
					AssertNull("There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Form should be closed.", true, IsFormClosed);
				}
				finally
				{
					form.Closed -= new EventHandler(Form_Closed);
				}
			}
		}

		#region Implementation

		protected override void BashControl(Control controlToBash)
		{
			//the bash on SummaryTextBox may trigger amnesty failure duo to the fact that the control has TranslatableDataFieldAttribute
			if (controlToBash.Name != "SummaryTextBox")
			{
				base.BashControl(controlToBash);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new GlbReleaseNoteEditForm(Manager);
		}

		protected override GlbReleaseNoteManager GetNewManager()
		{
			return new DummyGlbReleaseNoteManagerForSourceSafe(Factory);
		}

		protected new GlbReleaseNoteEditForm GetFormToBash()
		{
			return (GlbReleaseNoteEditForm)base.GetFormToBash();
		}

		protected new DummyGlbReleaseNoteManagerForSourceSafe Manager
		{
			get
			{
				return (DummyGlbReleaseNoteManagerForSourceSafe)base.Manager;
			}
		}

		void AssertIsCheckedOut(GlbReleaseNoteEditForm form, bool isCheckedOut)
		{
			RemoveAction expectedRemoveAction = (isCheckedOut) ? RemoveAction.RemoveAndDelete : RemoveAction.NoRemovePossible;
			AssertEquals("IsCheckedOut", isCheckedOut, form.IsCheckedOut);
			AssertEquals("ReleaseNotesGrid.RemoveAction", expectedRemoveAction, form.InternalReleaseNotesGrid.RemoveAction);
			AssertEquals("CheckoutButton.Enabled", !isCheckedOut, form.CheckoutButton.Enabled);
			AssertEquals("CheckInButton.Enabled", isCheckedOut, form.CheckInButton.Enabled);
			AssertEquals("UndoCheckoutButton.Enabled", isCheckedOut, form.UndoCheckoutButton.Enabled);
			AssertEquals("CloseButton.Enabled", !isCheckedOut, form.InternalCloseButton.Enabled);
		}

		void Form_Closed(object sender, EventArgs e)
		{
			IsFormClosed = true;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.EnterpriseCDDate = ZDateTime.Today.AddMonths(-6).ToDateTime();
		}

		bool IsFormClosed;

		#region class DummyGlbReleaseNoteManagerForSourceSafe

		protected class DummyGlbReleaseNoteManagerForSourceSafe : Business.GlbReleaseNoteManagerForSourceSafe
		{
			public DummyGlbReleaseNoteManagerForSourceSafe(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override void CheckInCore()
			{
				CheckInCalled = true;
			}

			protected override bool CheckoutCore(out string errorMessage)
			{
				errorMessage = CheckoutErrorMessage;
				return CheckoutResult;
			}

			protected override void UndoCheckoutCore()
			{
				UndoCheckoutCalled = true;
			}

			protected override void SetDefaultValues()
			{
				using (SuspendSettingHasChanges())
				{
					base.SetDefaultValues();
				}
			}

			public bool CheckInCalled;
			public bool CheckoutResult;
			public string CheckoutErrorMessage;
			public bool UndoCheckoutCalled;
		}

		#endregion
		#endregion
	}
}
