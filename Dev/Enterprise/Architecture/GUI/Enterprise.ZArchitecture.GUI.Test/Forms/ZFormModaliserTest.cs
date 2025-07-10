using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Core.Modules;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms
{
	public class ZFormModaliserTest : TestCase
	{
		public void TestShowDialogsInTest()
		{
			AssertEquals("precondition: Static reset should have set ShowDialogsInTest to false", false, ZFormModaliser.ShowDialogsInTest);

			using (var form = new Form())
			{
				var showCount = 0;

				form.Shown += (sender, e) =>
				{
					showCount++;
				};

				ZFormModaliser.ShowDialogWithoutDispose(form);
				AssertEquals("Should not have shown the form", 0, showCount);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				AssertEquals("Should have shown the form", 1, showCount);
			}
		}

		public void TestSetDelegateToCallOnFormClosing()
		{
			var wasDelegateInvoked = false;
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(dialog =>
			{
				var form = (FormForSetDelegateToCallOnFormClosingTest)dialog;
				AssertEquals("The delegate should be invoked after the form is loaded, and yet...", true, form.OnLoadCompleted);
				AssertEquals("The delegate should be invoked during the OnClosing event, and yet...", true, form.OnClosingStarted);
				AssertEquals("The delegate should be invoked during the OnClosing event, and yet...", false, form.OnClosingCompleted);
				AssertEquals("The delegate should be invoked before the form is actually closed/disposed, and yet...", false, form.OnClosedStarted);
				wasDelegateInvoked = true;
			});

			using (var parentForm = new Form())
			using (var form = new FormForSetDelegateToCallOnFormClosingTest())
			{
				parentForm.Show();
				ZFormModaliser.Show(form, parentForm);
				Application.DoEvents();
				form.Close();
				Application.DoEvents();
			}

			AssertEquals("The delegate should have been invoked when the form was closed, and yet...", true, wasDelegateInvoked);
		}

		class FormForSetDelegateToCallOnFormClosingTest : Form
		{
			public bool OnLoadCompleted { get; private set; }
			public bool OnClosingStarted { get; private set; }
			public bool OnClosingCompleted { get; private set; }
			public bool OnClosedStarted { get; private set; }

			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);
				OnLoadCompleted = true;
			}

			protected override void OnClosing(CancelEventArgs e)
			{
				OnClosingStarted = true;
				base.OnClosing(e);
				OnClosingCompleted = true;
			}

			protected override void OnClosed(EventArgs e)
			{
				OnClosedStarted = true;
				base.OnClosed(e);
			}
		}

		public void TestEnableFormIsUsedByShow()
		{
			using (var parentForm = new ZForm())
			using (var childForm = new Form())
			{
				AssertEquals("Parent Form is enabled by default", true, ZFormModaliser.IsWindowEnabled(parentForm));

				for (var i = 0; i < 5; i++)
				{
					ZFormModaliser.EnableForm(parentForm, false);
					AssertEquals("Parent Form should be disabled", false, ZFormModaliser.IsWindowEnabled(parentForm));
				}
				ZFormModaliser.Show(childForm, parentForm);
				AssertEquals("Parent Form should be disabled", false, ZFormModaliser.IsWindowEnabled(parentForm));

				for (var i = 0; i < 5; i++)
				{
					AssertEquals("Parent Form should still be disabled", false, ZFormModaliser.IsWindowEnabled(parentForm));
					ZFormModaliser.EnableForm(parentForm, true);
				}

				childForm.Close();
				AssertEquals("Parent Form should be enabled again", true, ZFormModaliser.IsWindowEnabled(parentForm));
			}
		}

		public void TestDisableAllFormsButOne()
		{
			using (var form0 = new ZForm { Name = "form0" })
			using (var form1 = new Form { Name = "form1" })
			using (var form2 = new ZForm { Name = "form2" })
			using (var form3 = new ZForm { Name = "form3" })
			using (var form4 = new ZForm { Name = "form4" })
			using (var form5 = new Form { Name = "form5" })
			using (var form6 = new ZForm { Name = "form6" })
			{
				form1.Show();
				form2.Show();
				form3.Show();
				form3.Enabled = false;
				form4.Show();
				form4.Hide();
				form5.Show();
				Application.DoEvents();

				ZFormModaliser.EnableWindow(form5, false);
				ZFormModaliser.EnableWindow(form6, false);

				Assert(ZFormModaliser.IsWindowEnabled(form0));
				Assert(ZFormModaliser.IsWindowEnabled(form1));
				Assert(ZFormModaliser.IsWindowEnabled(form2));
				Assert(!ZFormModaliser.IsWindowEnabled(form3));
				Assert(ZFormModaliser.IsWindowEnabled(form4));
				Assert(!ZFormModaliser.IsWindowEnabled(form5));
				Assert(!ZFormModaliser.IsWindowEnabled(form6));

				using (ZFormModaliser.DisableAllFormsButOne(form0))
				{
					Assert("Active form", ZFormModaliser.IsWindowEnabled(form0));
					Assert(!ZFormModaliser.IsWindowEnabled(form1));
					Assert(!ZFormModaliser.IsWindowEnabled(form2));
					Assert(!ZFormModaliser.IsWindowEnabled(form3));
					Assert("Untouched hidden", ZFormModaliser.IsWindowEnabled(form4));
					Assert(!ZFormModaliser.IsWindowEnabled(form5));
					Assert(!ZFormModaliser.IsWindowEnabled(form6));
				}

				Assert(ZFormModaliser.IsWindowEnabled(form0));
				Assert(ZFormModaliser.IsWindowEnabled(form1));
				Assert(ZFormModaliser.IsWindowEnabled(form2));
				Assert(!ZFormModaliser.IsWindowEnabled(form3));
				Assert(ZFormModaliser.IsWindowEnabled(form4));
				Assert("Remains disabled", !ZFormModaliser.IsWindowEnabled(form5));
				Assert("Remains disabled", !ZFormModaliser.IsWindowEnabled(form6));

				ZFormModaliser.EnableWindow(form5, true);
				ZFormModaliser.EnableWindow(form6, true);
			}
		}

#if !WINZOR
		public void TestDisableAllFormsButOneDoesNotAttemptCrossThreadFormAccess()
		{
			var systemCheckForIllegalCrossThreadCallsSetting = Control.CheckForIllegalCrossThreadCalls;
			Control.CheckForIllegalCrossThreadCalls = true;
			var threadFormShownEvent = new ManualResetEvent(false);
			ZForm threadForm = null;
			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					threadForm = new ZForm() { Name = "threadForm " };
					using (var timer = new System.Windows.Forms.Timer())
					{
						threadForm.Shown += (object sender, EventArgs e) => threadFormShownEvent.Set();
						threadForm.ShowDialog();
					}
				}
			});
			thread.Start();
			try
			{
				threadFormShownEvent.WaitOne();

				using (var form = new ZForm { Name = "form" })
				{
					form.Show();
					Application.DoEvents();
					AssertNoExceptionThrown(() =>
					{
						using (ZFormModaliser.DisableAllFormsButOne(form))
						{
						}
					});
				}
			}
			finally
			{
				Control.CheckForIllegalCrossThreadCalls = systemCheckForIllegalCrossThreadCallsSetting;
				threadForm.Invoke(new VoidParameterlessDelegate(threadForm.Dispose));
			}
		}

		void Form_Shown(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
#endif

		public void TestDeveloperErrorWhenPreShowDelegateSetTooManyTimes()
		{
			try
			{
				for (var i = 0; i <= ZFormModaliser.MaximumDelegatesToStack + 1; i++)
				{
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate { });
				}
				AssertEquals("Developer error shown", 1, ExceptionReporterTestListener.Instance.Count);
				ExceptionReporterTestListener.Instance.Clear();
			}
			finally
			{
				for (var i = 0; i <= ZFormModaliser.MaximumDelegatesToStack + 1; i++)
				{
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:Don't use System.Windows.Forms dialogs", Justification = "Test Cases")]
		public void TestShowCommonDialog()
		{
			AssertEquals("PreCondition: StaticResetter should set LastCommonDialogShownDialogForTest to null", null, ZFormModaliser.LastCommonDialogShownDialogForTest);
			AssertEquals("PreCondition: StaticResetter should set ResultToReturnFromShowDialog to None", DialogResult.None, ZFormModaliser.ResultToReturnFromShowDialog);

			using (var dialog = new OpenFileDialog())
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				var result = ZFormModaliser.ShowCommonDialogWithoutDispose(dialog);
				AssertEquals(dialog, ZFormModaliser.LastCommonDialogShownDialogForTest);
				AssertEquals(result, DialogResult.Cancel);
			}

			using (var dialog = new OpenFileDialog())
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Abort;
				var result = ZFormModaliser.ShowCommonDialogWithoutDispose(dialog);
				AssertEquals(dialog, ZFormModaliser.LastCommonDialogShownDialogForTest);
				AssertEquals(result, DialogResult.Abort);
			}

			using (var tempFile = TempFile.New())
			{
				using (var dialog = new OpenFileDialog())
				{
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					var result = ZFormModaliser.ShowCommonDialogWithoutDispose(dialog);
					AssertEquals(result, DialogResult.OK);

					AssertEquals(tempFile.Filename, dialog.FileName);
					using (var tempFileStream = dialog.OpenFile())
					{
						AssertNotNull(tempFileStream);
					}
				}
			}
		}

		public void TestShowDialog()
		{
			AssertEquals("PreCondition: StaticResetter should set LastFormShownDialogForTest to null", null, ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("PreCondition: StaticResetter should set ResultToReturnFromShowDialog to None", DialogResult.None, ZFormModaliser.ResultToReturnFromShowDialog);

			using (var form = new Form())
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				var result = ZFormModaliser.ShowDialogWithoutDispose(form);
				AssertEquals(form, ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(result, DialogResult.Cancel);
			}

			using (var form = new Form())
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Abort;
				var result = ZFormModaliser.ShowDialogWithoutDispose(form);
				AssertEquals(form, ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(result, DialogResult.Abort);
			}
		}

		public void TestShowDialoginInNonUI()
		{
			AssertEquals("PreCondition: StaticResetter should set LastFormShownDialogForTest to null", null, ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("PreCondition: StaticResetter should set ResultToReturnFromShowDialog to None", DialogResult.None, ZFormModaliser.ResultToReturnFromShowDialog);

			using (var form = new Form())
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var originalIsInterative = Globals.IsUserInteractive;
				using (new DisposableAction(() => Globals.IsUserInteractive = originalIsInterative))
				{
					Globals.IsUserInteractive = false;
					var result1 = ZFormModaliser.ShowDialogWithoutDispose(form);
					AssertEquals("No form should be shown in non-UI mode", null, ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Result should be Cancel", DialogResult.Cancel, result1);

					AssertEquals("ShowModalFormInNonUI_Form", ErrorReporter.LastKeyReported);
					ErrorReporter.Clear();
				}

				using (new DisposableAction(() => Globals.IsUserInteractive = originalIsInterative))
				{
					Globals.IsUserInteractive = true;
					var result2 = ZFormModaliser.ShowDialogWithoutDispose(form);
					AssertEquals(form, ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals(DialogResult.OK, result2);
				}
			}
		}

		public void TestShowModalMessageBox()
		{
			try
			{
				using (var form = new Form())
				{
					form.Show();
					Application.DoEvents();
					TestFormModaliser.SetApplicationActiveForm(form);

					using (var dialog = new TestModalDialog())
					{
						ZFormModaliser.ShowMessageBoxWithoutDispose(dialog);
#if !WINZOR
						var eventList = (EventHandlerList)typeof(Component).InvokeMember("Events", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty, null, dialog, null);
						var closingEventKey = typeof(Form).InvokeMember("EVENT_CLOSING", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.GetField, null, null, null);
						AssertEquals("The Closing event should not linger around", null, eventList[closingEventKey]);
#endif

						AssertEquals("Error Count", 0, ExceptionReporterTestListener.Instance.Count);
						AssertNull("LastActiveForm should be null after dialog closes", ZFormModaliser.GetActiveMessageBoxParentForm());

						ZFormModaliser.ShowMessageBoxWithoutDispose(null);
						AssertEquals("Error Count", 1, ExceptionReporterTestListener.Instance.Count);
						ExceptionReporterTestListener.Instance.Clear();

						ZFormModaliser.ShowMessageBoxWithoutDispose(dialog);
						AssertEquals("Error Count", 0, ExceptionReporterTestListener.Instance.Count);
					}
				}
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestActiveForm()
		{
			using (var parentForm = new Form())
			{
				parentForm.Show();
				using (var childForm = new Form())
				{
					ZFormModaliser.Show(childForm, parentForm);
					AssertEquals(childForm, ZFormModaliser.LastFormShownForTest);
				}
			}
		}

		public void TestShow_ParentEnabled()
		{
			using (var parentForm = new Form())
			{
				parentForm.Show();
				using (var childForm = new Form())
				{
					ZFormModaliser.Show(childForm, parentForm);
				}
				AssertEquals(true, ZFormModaliser.IsWindowEnabled(parentForm));
			}
			// Same for ZForm parent
			using (var parentForm = new ZForm())
			{
				parentForm.Show();
				using (var childForm = new Form())
				{
					ZFormModaliser.Show(childForm, parentForm);
				}
				AssertEquals(true, ZFormModaliser.IsWindowEnabled(parentForm));
			}
		}

		[ExpectNoExceptions]
		public void TestPerformanceStatisticsExcludedDuringModalShow()
		{
			var mock = new Mock<IPerformanceStatisticsCollector>(MockBehavior.Strict);
			mock.SetupGet(o => o.StatisticMode).Returns(EnabledState.Detailed);
			mock.Setup(o => o.Exclude()).Returns(DisposableAction.NoAction);

			using (ObjectFactory.Substitute(mock.Object))
			{
				PerformanceStatisticsCollector.ResetInstance();
				try
				{
					var f = new Form();
					ZFormModaliser.ShowDialogAndDispose(f);
					mock.Verify();
				}
				finally
				{
					PerformanceStatisticsCollector.ResetInstance();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestShow_NullForms()
		{
			using (var parentForm = new Form())
			{
				ZFormModaliser.Show(null, parentForm);
				parentForm.Dispose();
				ZFormModaliser.Show(null, parentForm);
				ZFormModaliser.Show(parentForm, parentForm);
			}

			ZFormModaliser.Show(null, null);
		}

		[ExpectNoExceptions]
		public void TestParentChildTrackerConstructor_ParentFormDisposed()
		{
			using (var parentForm = new ZForm())
			{
				parentForm.Dispose();
				using (var form = new ZForm())
				{
					var pct = new ZFormModaliser.ParentChildTracker(form, parentForm);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestParentChildTrackerConstructor_ParentFormDisposing()
		{
			using (var parentForm = new ZForm())
			{
				parentForm.Disposed += ParentForm_Disposed;
				parentForm.Dispose();
			}
		}

		void ParentForm_Disposed(object sender, EventArgs e)
		{
			using (var form = new ZForm())
			{
				var pct = new ZFormModaliser.ParentChildTracker(form, (Form)sender);
			}
		}

		public void TestParentChildTracker_ChildFormOwnerShouldBeSetNullInOrderToPreventOccursOutOfMemoryException()
		{
			AssertChildFormOwnerShouldBeSetNullInOrderToPreventOccursOutOfMemoryException(false);
			AssertChildFormOwnerShouldBeSetNullInOrderToPreventOccursOutOfMemoryException(true);
		}

		void AssertChildFormOwnerShouldBeSetNullInOrderToPreventOccursOutOfMemoryException(bool isRemoteAppSession)
		{
#if !WINZOR
			if (isRemoteAppSession)
			{
				ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest(isRemoteAppSession: isRemoteAppSession));
			}
#endif

			using (var parentForm = new ZForm())
			{
				parentForm.Show();
				using (var childForm = new ZFormForTest() { Text = "childForm" })
				{
					ZFormModaliser.Show(childForm, parentForm);
					childForm.Close();
					AssertNull("Owner should be set to null in order to prevent occurs OutOfMemoryException", childForm.Owner_Test);
				}
			}
		}

		class ZFormForTest : ZForm
		{
			protected override void Dispose(bool disposing)
			{
				owner = Owner;
				base.Dispose(disposing);
			}

			Form owner;
			public Form Owner_Test => owner;
		}

		public void TestSetTemporaryDelegateToCallBeforeShowingFormsOrDialogs()
		{
			using (var form = new ZForm())
			{
				var delegateCalledCount = 0;
				var tempCalledCount = 0;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f => delegateCalledCount++);

				using (ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs(f => tempCalledCount++))
				{
					ZFormModaliser.ShowDialogAndDispose(form);

					AssertEquals(1, delegateCalledCount);
					AssertEquals(1, tempCalledCount);
				}

				ZFormModaliser.ShowDialogAndDispose(form);

				AssertEquals(2, delegateCalledCount);
				AssertEquals(1, tempCalledCount);

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}
		}

		#region Cancellable Closing Modalized Form

		public void TestCancellableClosingModalizedForm()
		{
			using (var parentForm = new Form())
			using (var childForm = new CancellableClosingForm())
			{
				parentForm.Show();

				ZFormModaliser.Show(childForm, parentForm);
				Assert(!ZFormModaliser.IsWindowEnabled(parentForm));

				childForm.CancelClosing = true;
				childForm.Close();
				Assert("Parent form should not be activated yet", !ZFormModaliser.IsWindowEnabled(parentForm));

				childForm.CancelClosing = false;
				childForm.Close();
				Assert("Parent form should be activated", ZFormModaliser.IsWindowEnabled(parentForm));
			}
			// Same for ZForm parent
			using (var parentForm = new ZForm())
			using (var childForm = new CancellableClosingForm())
			{
				parentForm.Show();

				ZFormModaliser.Show(childForm, parentForm);
				Assert(!ZFormModaliser.IsWindowEnabled(parentForm));

				childForm.CancelClosing = true;
				childForm.Close();
				Assert("Parent form should not be activated yet", !ZFormModaliser.IsWindowEnabled(parentForm));

				childForm.CancelClosing = false;
				childForm.Close();
				Assert("Parent form should be activated", ZFormModaliser.IsWindowEnabled(parentForm));
			}
		}

		class CancellableClosingForm : Form
		{
			protected override void OnClosing(CancelEventArgs e)
			{
				e.Cancel = CancelClosing;
				base.OnClosing(e);
			}

			public bool CancelClosing { get; set; }
		}

		#endregion

		#region Stacked SetDelegateToCallBeforeShowingFormsOrDialogs

		public void TestStackedSetDelegateToCallBeforeShowingFormsOrDialogs()
		{
			using (var masterForm = new ZForm())
			using (var form1 = new Form())
			using (var form2 = new Form())
			using (var form3 = new Form())
			{
				masterForm.Show();
				string delegateResult;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(o => delegateResult = "aaa");
				delegateResult = null;
				ZFormModaliser.Show(form1, masterForm);
				AssertEquals("aaa", delegateResult);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(o => delegateResult = "bbb");
				delegateResult = null;
				ZFormModaliser.Show(form2, masterForm);
				AssertEquals("bbb", delegateResult);

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				delegateResult = null;
				ZFormModaliser.Show(form3, masterForm);
				AssertEquals("aaa", delegateResult);

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}
		}

		#endregion

		#region ShowModally without parent form

		public void TestSetAndRemoveApplicationActiveForm()
		{
			AssertEquals("Starting with no active form override", Form.ActiveForm, TestModaliser.ApplicationActiveForm);

			var testForm = new TestMainForm();
			ZFormModaliser.SetApplicationActiveForm(testForm);
			AssertEquals(testForm, TestModaliser.ApplicationActiveForm);

			ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(testForm);
			AssertEquals(Form.ActiveForm, TestModaliser.ApplicationActiveForm);
		}

		public void TestRemoveActiveFormWhenAnotherFormIsActive()
		{
			AssertEquals("Starting with no active form override", Form.ActiveForm, TestModaliser.ApplicationActiveForm);

			var testForm1 = new TestMainForm();
			var testForm2 = new TestMainForm();

			ZFormModaliser.SetApplicationActiveForm(testForm1);
			ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(testForm2);
			AssertEquals(testForm1, TestModaliser.ApplicationActiveForm);
		}

		public void TestGetMainFormAsBestParentFormForModalShow()
		{
			using (var testForm1 = new ZForm())
			using (var testForm2 = new TestMainForm())  //Showing an IMainForm just in case the real MainForm isn't showing for some reason
			{
				testForm1.Show();
				testForm2.Show();
				Assert(TestModaliser.GetBestParentFormForModalShow() is IMainForm);
			}
		}

		public void TestGetActiveFormAsBestParentFormForModalShow()
		{
			using (var testForm = new ZForm())
			{
				ZFormModaliser.SetApplicationActiveForm(testForm);
				AssertEquals(testForm, TestModaliser.GetBestParentFormForModalShow());
			}
		}

		#endregion

		#region Test Classes

		class TestModalDialog : Form
		{
		}

		class TestFormModaliser : ZFormModaliser
		{
			public new static void SetInstanceForTest(ZFormModaliser newInstance)
			{
				ZFormModaliser.SetInstanceForTest(newInstance);
			}
		}

		class TestMainForm : Form, IMainForm
		{
			void IMainForm.UpdateToolBarDeleteButton(Modules.ZEmbeddedModule module)
			{
			}

			INamedModule IMainForm.CurrentModule
			{
				get { return null; }
			}

			string IMainForm.CurrentModuleLicenceCheckPointName
			{
				get { return string.Empty; }
			}
		}

		#endregion

		#region Implementation

		TestFormModaliser TestModaliser;

		protected override void SetUp()
		{
			base.SetUp();
			TestModaliser = new TestFormModaliser();
			TestFormModaliser.SetInstanceForTest(TestModaliser);
			ZFormModaliser.SetApplicationActiveForm(null);
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestFormModaliser.SetInstanceForTest(null);
		}

		#endregion
	}
}
