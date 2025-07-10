using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Builder.DataUpgradeSetup;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.SDF.Testing
{
	[TestedType(typeof(StmSystemDefinedFieldForm))]
	sealed class StmSystemDefinedFieldFormTest : ZFormBasherTest
	{
		public void TestFormDefaults()
		{
			using (StmSystemDefinedFieldForm form = GetFormToBash())
			{
				form.Show();
				AssertEquals("FormVerb", "Edit", form.FormVerb);
				AssertEquals("FormCaption", "System Defined Fields", form.FormCaption);
				AssertEquals("DefaultDropEdit.Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(200), form.DefaultValueDropEdit.Width);
				AssertIsCheckedOut(form, false);
			}
		}

		public void TestCheckout()
		{
			using (StmSystemDefinedFieldForm form = GetFormToBash())
			{
				form.Show();

				AssertNull("Precondition: There should not be an error message.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.CheckoutButton.PerformClick();
				AssertEquals("A question should be asked.", "Checking out will override all data in your database with data from SourceControl. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				MoqController.Verify(m => m.FullCheckOut(), Times.Never());

				MoqController.Setup(m => m.FullCheckOut());
				MoqController.Setup(m => m.FullCheckOut()).Throws(new Exception("Cannot Checkout!"));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.CheckoutButton.PerformClick();

				AssertEquals("An error message should be shown.", "System Defined Fields cannot be checked out at the moment. The error is:\r\n\r\nCannot Checkout!", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsCheckedOut(form, false);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				MoqController.Setup(m => m.FullCheckOut());
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.CheckoutButton.PerformClick();

				AssertEquals("There should be no additional message apart from the checkout confirmation question.", "Checking out will override all data in your database with data from SourceControl. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsCheckedOut(form, true);
			}
		}

		[RequiresSTA]
		public void TestUndoCheckout()
		{
			using (StmSystemDefinedFieldForm form = GetFormToBash())
			{
				form.Show();

				MoqController.Setup(m => m.FullCheckOut());
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.CheckoutButton.PerformClick();

				AssertEquals("There should be no additional message apart from the checkout confirmation question.", "Checking out will override all data in your database with data from SourceControl. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsCheckedOut(form, true);

				Manager.Fields.RemoveAndDeleteAll();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				MoqController.Invocations.Clear();

				form.UndoCheckoutButton.PerformClick();
				MoqController.Verify(m => m.FullCheckOut(), Times.Never());
				AssertIsCheckedOut(form, true);
				AssertEquals("A question should be asked.", "Do you really want to undo checkout and lose all changes?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				MoqController.Setup(m => m.FullUndoCheckOut());

				form.UndoCheckoutButton.PerformClick();
				AssertIsCheckedOut(form, false);
			}
		}

		public void TestSaveForCheckIn()
		{
			using (StmSystemDefinedFieldForm form = GetFormToBash())
			{
				form.Show();

				MoqController.Setup(m => m.FullCheckOut());
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.CheckoutButton.PerformClick();
				MoqController.Verify();

				CreateTestData();
				AssertEquals("There should be no additional message apart from the checkout confirmation question.", "Checking out will override all data in your database with data from SourceControl. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);

				form.SaveButton.PerformClick();
				MoqController.Verify(m => m.FullSave(), Times.Never());
				AssertEquals("An error message should be shown.", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsCheckedOut(form, true);

				PrepareTestDataForSaving();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				MoqController.Invocations.Clear();
				MoqController.Setup(m => m.FullSave());

				form.SaveButton.PerformClick();
				AssertNull("There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsCheckedOut(form, false);
			}
		}

		public void TestCannotCloseIfCheckedOut()
		{
			using (StmSystemDefinedFieldForm form = GetFormToBash())
			{
				form.Show();
				form.Closed += new EventHandler(Form_Closed);

				try
				{
					AssertNull("Precondition: There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);

					MoqController.Setup(m => m.FullCheckOut());
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.CheckoutButton.PerformClick();

					form.Close();
					AssertEquals("An error message should be shown.", "You must undo checkout or check in first before closing this form.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Form should not be closed.", false, IsFormClosed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.UndoCheckoutButton.PerformClick();
					form.Close();
					AssertEquals("Do you really want to undo checkout and lose all changes?", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("No other messages", UnitTestUserNotification.Instance.PreviousMessages[1].WasNone);
					AssertEquals("Form should be closed.", true, IsFormClosed);
				}
				finally
				{
					form.Closed -= new EventHandler(Form_Closed);
				}
			}
		}

		public void TestEditWithoutCheckout()
		{
			using (var form = new StmSystemDefinedFieldForm(MoqManager.Object))
			{
				form.Show();
				AssertIsEditingWithoutCheckout(form, false);

				MoqManager.Protected().Setup("EditWithoutCheckoutCore");
				form.EditWithoutCheckoutButton.PerformClick();
				MoqManager.Protected().Verify<ISetupController>("GetNewController", Times.Never());

				AssertIsEditingWithoutCheckout(form, true);
			}
		}

		public void TestSaveWithoutCheckIn()
		{
			CreateTestData();

			using (var form = new StmSystemDefinedFieldForm(MoqManager.Object))
			{
				form.Show();
				form.EditWithoutCheckoutButton.PerformClick();
				AssertIsEditingWithoutCheckout(form, true);
				AssertNull("Precondition: There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.SaveWithoutCheckInButton.PerformClick();
				MoqManager.Protected().Verify<ISetupController>("GetNewController", Times.Never());
				MoqManager.Protected().Verify("SaveWithoutCheckInCore", Times.Never());

				AssertIsEditingWithoutCheckout(form, true);
				AssertEquals("An error message should be shown.", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				PrepareTestDataForSaving();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				MoqManager.Invocations.Clear();
				MoqManager.Protected().Setup("SaveWithoutCheckInCore");
				form.SaveWithoutCheckInButton.PerformClick();
				AssertNull("There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertIsEditingWithoutCheckout(form, false);
			}
		}

		#region Implementation

		new StmSystemDefinedFieldForm GetFormToBash()
		{
			return (StmSystemDefinedFieldForm)base.GetFormToBash();
		}

		protected override Form GetFormToBashCore()
		{
			return new StmSystemDefinedFieldForm(MoqManager.Object);
		}

		void CreateTestData()
		{
			Manager.Fields.RemoveAll();
			Field1 = Manager.Fields.AddNew();
			Field2 = Manager.Fields.AddNew();
			Field3 = Manager.Fields.AddNew();
		}

		void PrepareTestDataForSaving()
		{
			Field1.S1_Order = 1;
			Field2.S1_Order = 2;
			Field3.S1_Order = 3;

			Field1.S1_Name = "1";
			Field2.S1_Name = "2";
			Field3.S1_Name = "3";

			Field1.S1_Category = "1";
			Field2.S1_Category = "2";
			Field3.S1_Category = "3";

			Field1.S1_Hint = "1";
			Field2.S1_Hint = "2";
			Field3.S1_Hint = "3";

			Field1.S1_Type = "TXT";
			Field2.S1_Type = "TXT";
			Field3.S1_Type = "TXT";
		}

		void AssertIsCheckedOut(StmSystemDefinedFieldForm form, bool isCheckedOut)
		{
			AssertAllowRemovingInGrids(form, isCheckedOut);
			AssertEquals("IsCheckedOut", isCheckedOut, form.IsCheckedOut);
			AssertEquals("CheckoutButton.Enabled", !isCheckedOut, form.CheckoutButton.Enabled);
			AssertEquals("SaveButton.Enabled", isCheckedOut, form.SaveButton.Enabled);
			AssertEquals("UndoCheckoutButton.Enabled", isCheckedOut, form.UndoCheckoutButton.Enabled);
			AssertEquals("CloseButton.Enabled", !isCheckedOut, form.CloseButton.Enabled);
			AssertEquals("EditWithoutCheckoutButton.Enabled", !isCheckedOut, form.EditWithoutCheckoutButton.Enabled);
			AssertEquals("SaveWithoutCheckInButton.Enabled", false, form.SaveWithoutCheckInButton.Enabled);
		}

		void AssertIsEditingWithoutCheckout(StmSystemDefinedFieldForm form, bool isEditing)
		{
			AssertAllowRemovingInGrids(form, isEditing);
			AssertEquals("IsCheckedOut", false, form.IsCheckedOut);
			AssertEquals("EditWithoutCheckoutButton.Enabled", !isEditing, form.EditWithoutCheckoutButton.Enabled);
			AssertEquals("SaveButton.Enabled", false, form.SaveButton.Enabled);
			AssertEquals("SaveWithoutCheckInButton.Enabled", isEditing, form.SaveWithoutCheckInButton.Enabled);
			AssertEquals("CheckoutButton.Enabled", !isEditing, form.CheckoutButton.Enabled);
			AssertEquals("UndoCheckoutButton.Enabled", false, form.UndoCheckoutButton.Enabled);
			AssertEquals("CloseButton.Enabled", true, form.CloseButton.Enabled);
		}

		void AssertAllowRemovingInGrids(StmSystemDefinedFieldForm form, bool isAllowed)
		{
			RemoveAction expectedRemoveAction = (isAllowed) ? RemoveAction.RemoveAndDelete : RemoveAction.NoRemovePossible;
			AssertEquals("FieldsGrid.RemoveAction", expectedRemoveAction, form.FieldsGrid.RemoveAction);
			AssertEquals("SpecificCountriesGrid.RemoveAction", expectedRemoveAction, form.SpecificCountriesGrid.RemoveAction);
			AssertEquals("FieldColumnsGrid.RemoveAction", expectedRemoveAction, form.FieldColumnsGrid.RemoveAction);
		}

		void Form_Closed(object sender, EventArgs e)
		{
			IsFormClosed = true;
		}

		StmSystemDefinedFieldManager Manager
		{
			get { return MoqManager.Object; }
		}

		bool IsFormClosed;
		StmSystemDefinedField Field1;
		StmSystemDefinedField Field2;
		StmSystemDefinedField Field3;

		Mock<StmSystemDefinedFieldManager> MoqManager
		{
			get
			{
				if (fMoqManager == null)
				{
					fMoqManager = new Mock<StmSystemDefinedFieldManager>(new object[] { new MockDocSupportBizO() }) { CallBase = true };
					fMoqManager.Protected().Setup<ISetupController>("GetNewController").Returns(MoqController.Object);
				}
				return fMoqManager;
			}
		}

		Mock<ISetupController> MoqController
		{
			get
			{
				if (fMoqController == null)
				{
					fMoqController = new Mock<ISetupController> { CallBase = true };
				}
				return fMoqController;
			}
		}

		Mock<StmSystemDefinedFieldManager> fMoqManager;
		Mock<ISetupController> fMoqController;

		#endregion
	}
}
