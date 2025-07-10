using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(ChangeTransactionDatesMessageBox))]
	class ChangeTransactionDatesMessageBoxBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new ChangeTransactionDatesMessageBox(new ChangeTransactionDatesBusinessObject(Env.Security.None, Factory));
		}

		class TestChangeTransactionDateMessageBox : ChangeTransactionDatesMessageBox
		{
			public TestChangeTransactionDateMessageBox(ChangeTransactionDatesBusinessObject businessObject)
				: base(businessObject)
			{
			}

			public ZButton YesButtonOnYesNoAllPanelExposed
			{
				get { return YesButtonOnYesNoAllPanel; }
			}

			public ZButton YesButtonOnYesNoCancelPanelExposed
			{
				get { return YesButtonOnYesNoCancelPanel; }
			}

			public ZButton YesToAllButtonExposed
			{
				get { return YesToAllButton; }
			}

			public ZButton NoButtonOnYesNoAllPanelExposed
			{
				get { return NoButtonOnYesNoAllPanel; }
			}

			public ZButton NoButtonOnYesNoCancelPanelExposed
			{
				get { return NoButtonOnYesNoCancelPanel; }
			}

			public ZButton NoToAllButtonExposed
			{
				get { return NoToAllButton; }
			}

			public ZButton CancelButtonOnYesNoCancelPanelExposed
			{
				get { return CancelButtonOnYesNoCancelPanel; }
			}

			public ZPanel YesNoAllPanelExposed
			{
				get { return YesNoAllPanel; }
			}

			public ZPanel YesNoCancelPanelExposed
			{
				get { return YesNoCancelPanel; }
			}
		}

		TestChangeTransactionDateMessageBox GetNewTestForm()
		{
			return new TestChangeTransactionDateMessageBox(TestBizo);
		}

		ChangeTransactionDatesBusinessObject TestBizo
		{
			get { return TestBizo_internalValue ?? (TestBizo_internalValue = new ChangeTransactionDatesBusinessObject(null, Factory)); }
		}
		ChangeTransactionDatesBusinessObject TestBizo_internalValue;

		#endregion

		public void TestDialogResults()
		{
			TestBizo.InvoiceDate = ZDateTime.Now;
			TestBizo.PostDate = TestBizo.InvoiceDate;
			new TestObjectCreator(Factory).CreateTestPeriods(TestBizo.InvoiceDate);
			Factory.Save();

			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.Show();
				AssertEquals("Precondition: ", YesNoYesAllNoAllMessageBoxResult.None, form.YesNoAllResult);
				AssertEquals("Precondition: ", DialogResult.None, form.DialogResult);

				form.YesButtonOnYesNoAllPanelExposed.PerformClick();
				Application.DoEvents();
				AssertEquals(false, form.Visible);
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.Yes, form.YesNoAllResult);
				AssertEquals(DialogResult.None, form.DialogResult);
			}

			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.Show();
				AssertEquals("Precondition: ", YesNoYesAllNoAllMessageBoxResult.None, form.YesNoAllResult);
				AssertEquals("Precondition: ", DialogResult.None, form.DialogResult);

				form.NoButtonOnYesNoAllPanelExposed.PerformClick();
				Application.DoEvents();
				AssertEquals(false, form.Visible);
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.No, form.YesNoAllResult);
				AssertEquals(DialogResult.None, form.DialogResult);
			}

			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.Show();
				AssertEquals("Precondition: ", YesNoYesAllNoAllMessageBoxResult.None, form.YesNoAllResult);
				AssertEquals("Precondition: ", DialogResult.None, form.DialogResult);

				form.YesToAllButtonExposed.PerformClick();
				Application.DoEvents();
				AssertEquals(false, form.Visible);
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.YesToAll, form.YesNoAllResult);
				AssertEquals(DialogResult.None, form.DialogResult);
			}

			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.Show();
				AssertEquals("Precondition: ", YesNoYesAllNoAllMessageBoxResult.None, form.YesNoAllResult);
				AssertEquals("Precondition: ", DialogResult.None, form.DialogResult);

				form.NoToAllButtonExposed.PerformClick();
				Application.DoEvents();
				AssertEquals(false, form.Visible);
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.NoToAll, form.YesNoAllResult);
				AssertEquals(DialogResult.None, form.DialogResult);
			}

			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.Show();
				AssertEquals("Precondition: ", YesNoYesAllNoAllMessageBoxResult.None, form.YesNoAllResult);
				AssertEquals("Precondition: ", DialogResult.None, form.DialogResult);

				form.YesButtonOnYesNoCancelPanelExposed.PerformClick();
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.None, form.YesNoAllResult);
				AssertEquals(DialogResult.Yes, form.DialogResult);
			}

			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.Show();
				AssertEquals("Precondition: ", YesNoYesAllNoAllMessageBoxResult.None, form.YesNoAllResult);
				AssertEquals("Precondition: ", DialogResult.None, form.DialogResult);

				form.NoButtonOnYesNoCancelPanelExposed.PerformClick();
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.None, form.YesNoAllResult);
				AssertEquals(DialogResult.No, form.DialogResult);
			}

			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.Show();
				AssertEquals("Precondition: ", YesNoYesAllNoAllMessageBoxResult.None, form.YesNoAllResult);
				AssertEquals("Precondition: ", DialogResult.None, form.DialogResult);

				form.CancelButtonOnYesNoCancelPanelExposed.PerformClick();
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.None, form.YesNoAllResult);
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestShowYesNoAllButtons()
		{
			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.ShowYesNoAllButtons = false;
				form.Show();
				Application.DoEvents();

				AssertEquals(false, form.YesNoAllPanelExposed.Visible);
				AssertEquals(true, form.YesNoCancelPanelExposed.Visible);
			}
			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.ShowYesNoAllButtons = true;
				form.Show();
				Application.DoEvents();

				AssertEquals(true, form.YesNoAllPanelExposed.Visible);
				AssertEquals(false, form.YesNoCancelPanelExposed.Visible);
			}
		}

		public void TestClosing()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();

			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.ShowYesNoAllButtons = true;
				form.Show();

				form.Close();
				Application.DoEvents();
				AssertEquals(true, form.Visible);
				AssertContains("You must choose one of the answers by pressing the buttons at the bottom of the window.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.YesButtonOnYesNoAllPanelExposed.PerformClick();
				Application.DoEvents();
				AssertEquals(true, TestBizo.HasErrors());
				AssertEquals(true, form.Visible);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.YesToAllButtonExposed.PerformClick();
				Application.DoEvents();
				AssertEquals(true, TestBizo.HasErrors());
				AssertEquals(true, form.Visible);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.YesButtonOnYesNoCancelPanelExposed.PerformClick();
				form.Close();
				Application.DoEvents();
				AssertEquals(true, TestBizo.HasErrors());
				AssertEquals(true, form.Visible);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				TestBizo.InvoiceDate = ZDateTime.Now;
				TestBizo.PostDate = TestBizo.InvoiceDate;
				new TestObjectCreator(Factory).CreateTestPeriods(TestBizo.InvoiceDate);
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.YesButtonOnYesNoAllPanelExposed.PerformClick();
				Application.DoEvents();
				AssertEquals(false, TestBizo.HasErrors());
				AssertEquals(false, form.Visible);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDefaultAction()
		{
			AccountingConfigurationRegistry.Instance.DefaultAllowUsersToBackDateInvoicesSetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.ShowYesNoAllButtons = true;
				form.Show();
				Application.DoEvents();

				AssertEquals(form.NoButtonOnYesNoAllPanelExposed, form.AcceptButton);
				AssertNull(form.CancelButton);
			}
			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.ShowYesNoAllButtons = false;
				form.Show();
				Application.DoEvents();

				AssertEquals(form.NoButtonOnYesNoCancelPanelExposed, form.AcceptButton);
				AssertEquals(form.CancelButtonOnYesNoCancelPanelExposed, form.CancelButton);
			}

			AccountingConfigurationRegistry.Instance.DefaultAllowUsersToBackDateInvoicesSetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.ShowYesNoAllButtons = true;
				form.Show();
				Application.DoEvents();

				AssertEquals(form.YesButtonOnYesNoAllPanelExposed, form.AcceptButton);
				AssertNull(form.CancelButton);
			}
			using (TestChangeTransactionDateMessageBox form = GetNewTestForm())
			{
				form.ShowYesNoAllButtons = false;
				form.Show();
				Application.DoEvents();

				AssertEquals(form.YesButtonOnYesNoCancelPanelExposed, form.AcceptButton);
				AssertEquals(form.CancelButtonOnYesNoCancelPanelExposed, form.CancelButton);
			}
		}
	}
}
