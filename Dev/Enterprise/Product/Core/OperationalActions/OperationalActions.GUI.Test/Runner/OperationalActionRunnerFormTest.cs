using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using TestingConstants = Enterprise.Services.OperationalActions.Business.Testing.TestingConstants;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	[TestedType(typeof(OperationalActionRunnerForm))]
	sealed class OperationalActionRunnerFormTest : ZFormBasherTest
	{
		public void TestMinSize()
		{
			Action.Context.Supporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			Action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithGUI);

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				form.Size = form.MinimumSize;
				form.Show();

				CombineAssertions(delegate
				{
					AssertEquals("Min Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(690), form.MinimumSize.Width);
					AssertEquals("Min Height", ControlDpiScalingHelper.ScaleToCurrentDpiY(375), form.MinimumSize.Height);

					AssertTabsFit(GetTabControl(form));
				});
			}
		}

		public void TestMinSize_Large()
		{
			Action.Context.Supporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			Action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithLargeGUI);

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				form.Size = form.MinimumSize;
				form.Show();

				AssertTabsFit(GetTabControl(form));
			}
		}

		public void TestMinSize_Oversize()
		{
			Action.Context.Supporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			Action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithOversizedGUI);

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				form.Size = form.MinimumSize;
				form.Show();

				CombineAssertions(delegate
				{
					AssertEquals("Min Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(900), form.MinimumSize.Width);
					AssertEquals("Min Height", ControlDpiScalingHelper.ScaleToCurrentDpiY(680), form.MinimumSize.Height);
				});
			}
		}

		public void TestDocumentsTabPageVisibility()
		{
			const string expectedTabPages1 =
				"Fields\n" +
				"Progress\n" +
				"";

			const string expectedTabPages2 =
				"Fields\n" +
				"Documents\n" +
				"Progress\n" +
				"";

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				AssertMultilineASCIIEquals("", expectedTabPages1, string.Join("\n", GetTabNames(form)));
			}

			Action.DocumentPivots.AddNew();

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				AssertMultilineASCIIEquals("", expectedTabPages2, string.Join("\n", GetTabNames(form)));
			}
		}

		public void TestFieldsTabPageVisibility()
		{
			const string expectedTabPages1 =
				"Fields\n" +
				"Progress\n" +
				"";

			const string expectedTabPages2 =
				"Progress\n" +
				"";

			OperationalActionRunner runner;

			runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				AssertMultilineASCIIEquals("", expectedTabPages1, string.Join("\n", GetTabNames(form)));
			}

			Action.FieldDescriptors.RemoveAndDeleteAll();

			runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				TabControl tabControl = GetTabControl(form);
				AssertMultilineASCIIEquals("", expectedTabPages2, string.Join("\n", GetTabNames(form)));
			}
		}

		public void TestMethodWithGuiAddsTab()
		{
			const string expectedTabPages =
				"Fields\n" +
				"Dummy Action Method With GUI\n" +
				"Progress\n" +
				"";

			Action.Context.Supporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			Action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithGUI);
			Action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithoutGUI);

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				TabControl tabControl = GetTabControl(form);

				string[] captions = new string[tabControl.TabPages.Count];
				for (int i = 0; i < tabControl.TabPages.Count; i++)
				{
					captions[i] = tabControl.TabPages[i].Text;
				}

				AssertMultilineASCIIEquals("", expectedTabPages, string.Join("\n", captions));
			}
		}

		public void TestShowWithoutLicence()
		{
			Action.Context.Supporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			Action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithGUI);
			Action.MethodDescriptors.AddNew(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithoutGUI);
			var checkpoint = Env.Instance.Licence.ShippingManagerBillOfLading;
			checkpoint.AllowUsageForTest = false;

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());

			OperationalActionRunnerForm.Show(runner);

			AssertNotEquals("", checkpoint.LastReasonForNotAllowing);
			AssertMultilineASCIIEquals("Should have shown the error.", "Error " + checkpoint.LastReasonForNotAllowing, UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals("Should not have shown the runner form", null, ZFormModaliser.LastFormShownDialogForTest);
			checkpoint.AllowUsageForTest = null;
		}

		public void TestCloseOnCompletion()
		{
			Action.SU_SE_NKDocumentEvent = Events.OriginalBillReceived.Code;

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			runner.Printer = ZGuid.NewZGuid();
			AssertNoNotifications("precondition:", runner);

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				form.Show();
				Application.DoEvents();

				runner.CloseOnCompletion = false;
				form.PerformClickOk();
				AssertEquals("Form should not have closed", true, form.Visible);

				runner.CloseOnCompletion = true;
				form.PerformClickOk();
				AssertEquals("Form should have closed", false, form.Visible);
			}
		}

		public void TestNoExceptionIfDisposedOrRunnerNull()
		{
			Dummy.Factory.Save();

			Action.SU_SE_NKDocumentEvent = Events.OriginalBillReceived.Code;

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords(Dummy.PK));
			runner.Printer = ZGuid.NewZGuid();
			runner.RunPreSaveValidation();
			runner.CloseOnCompletion = false;

			AssertNoNotifications("precondition:", runner);
			AssertEquals("precondition:", 0, CountEvents(Dummy, Events.OriginalBillReceived));

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				form.Show();
				Application.DoEvents();

				form.Dispose();

				form.PerformClickOk();

				Assert(String.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
			}

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				form.Show();
				Application.DoEvents();

				((KForm)form).DataSource = null;

				form.PerformClickOk();

				Assert(String.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
			}

			var loadingFactory = new BusinessObjectFactory();
			var reloadedDummy = loadingFactory.Load<DummyBusinessObjectWithDocumentSupport>(Dummy.PK);
			AssertEquals("No action ran", 0, CountEvents(reloadedDummy, Events.OriginalBillReceived));
		}

		public void TestShowDialog_Success()
		{
			Dummy.Factory.Save();

			Action.SU_SE_NKDocumentEvent = Events.OriginalBillReceived.Code;

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords(Dummy.PK));
			runner.Printer = ZGuid.NewZGuid();
			runner.RunPreSaveValidation();
			runner.CloseOnCompletion = false;

			AssertNoNotifications("precondition:", runner);
			AssertEquals("precondition:", 0, CountEvents(Dummy, Events.OriginalBillReceived));

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				form.Show();
				Application.DoEvents();

				form.PerformClickOk();

				const string expectedLog =
					"Starting Section: Events ...\r\n" +
					"Starting Section: Fields ...\r\n" +
					"Saving ...\r\n" +
					"Saved.\r\n" +
					"Done.\r\n" +
					"";

				AssertEquals("Question Are you sure you want to run this action on 1 record?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Form should not have closed", true, form.Visible);
				AssertMultilineASCIIEquals("", expectedLog, GetLogText(form));
			}

			var loadingFactory = new BusinessObjectFactory();
			var reloadedDummy = loadingFactory.Load<DummyBusinessObjectWithDocumentSupport>(Dummy.PK);
			AssertEquals("should have run the action", 1, CountEvents(reloadedDummy, Events.OriginalBillReceived));
		}

		public void TestShowDialog_SummaryLog()
		{
			Dummy.Factory.Save();

			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);

			action = Factory.New<OperationalAction>();
			Action.Context = Context;

			OperationalActionMethodDescriptor descriptor = Action.MethodDescriptors.AddNew();
			descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			descriptor.MethodID = TestingConstants.DummyActionMethodWithoutGUI;

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords(Dummy.PK));
			runner.Printer = ZGuid.NewZGuid();
			runner.CloseOnCompletion = false;
			runner.RunPreSaveValidation();

			AssertNoNotifications("precondition:", runner);
			AssertEquals("precondition:", 0, CountEvents(Dummy, Events.OriginalBillReceived));

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				form.Show();
				Application.DoEvents();

				form.PerformClickOk();

				const string expectedLog =
					"Starting Section: Dummy Action Method Without GUI ...\r\n" +
					"Saving ...\r\n" +
					"Saved.\r\n" +
					"Dummy Action Method Without GUI Summary:\r\n" +
					"1 event(s) added.\r\n" +
					"Done.\r\n" +
					"";

				AssertEquals("Question Are you sure you want to run this action on 1 record?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Form should not have closed", true, form.Visible);
				AssertMultilineASCIIEquals("", expectedLog, GetLogText(form));
			}

			var loadingFactory = new BusinessObjectFactory();
			var reloadedDummy = loadingFactory.Load<DummyBusinessObjectWithDocumentSupport>(Dummy.PK);
			AssertEquals("should have run the action", 1, CountEvents(reloadedDummy, Events.StaffVerbalWarningIssued));
		}

		public void TestShowDialog_Cancel()
		{
			Dummy.Factory.Save();

			Action.SU_SE_NKDocumentEvent = Events.OriginalBillReceived.Code;

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords(Dummy.PK));
			runner.Printer = ZGuid.NewZGuid();
			runner.RunPreSaveValidation();
			AssertNoNotifications("precondition:", runner);
			AssertEquals("precondition:", 0, CountEvents(Dummy, Events.OriginalBillReceived));

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.PerformClickOk();

				AssertEquals("Question Are you sure you want to run this action on 1 record?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Form should not have been closed", true, form.Visible);
				AssertMultilineASCIIEquals("Log should be empty - nothing done", "", GetLogText(form));
			}

			var loadingFactory = new BusinessObjectFactory();
			var reloadedDummy = loadingFactory.Load<DummyBusinessObjectWithDocumentSupport>(Dummy.PK);
			AssertEquals("should not have run the action", 0, CountEvents(reloadedDummy, Events.OriginalBillReceived));
		}

		public void TestShowDialog_ValidationError()
		{
			Dummy.Factory.Save();

			Action.SU_SE_NKDocumentEvent = Events.OriginalBillReceived.Code;
			Action.DocumentPivots.AddNew();

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords(Dummy.PK));
			runner.Printer = ZGuid.Empty;
			runner.RunPreSaveValidation();
			AssertHasNotifications("precondition:", runner.PrinterInfo);
			AssertEquals("precondition:", 0, CountEvents(Dummy, Events.OriginalBillReceived));

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				form.Show();
				Application.DoEvents();

				form.PerformClickOk();

				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Form should not have closed", true, form.Visible);
				AssertMultilineASCIIEquals("Log should be empty - nothing done", "", GetLogText(form));
			}

			var loadingFactory = new BusinessObjectFactory();
			var reloadedDummy = loadingFactory.Load<DummyBusinessObjectWithDocumentSupport>(Dummy.PK);
			AssertEquals("should not have run the action", 0, CountEvents(reloadedDummy, Events.OriginalBillReceived));
		}

		public void TestShowDialog_ZSaveExceptionInRunner()
		{
			var stmPrintQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			Factory.Save();

			Dummy.Factory.Save();

			Action.SU_SE_NKDocumentEvent = Events.OriginalBillReceived.Code;
			Action.DocumentPivots.AddNew();

			var runner = new MockOperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords(Dummy.PK));
			runner.Printer = stmPrintQueue.PK;

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				form.Show();
				Application.DoEvents();

				form.PerformClickOk();

				AssertEquals("Form should not have closed", true, form.Visible);
				AssertMultilineASCIIEquals("An CargoWise.EntityFramework.ZCannotSaveException occurred while saving: ZCannotSaveException In MockOperationalActionRunner.\r\nAborted.", GetLogText(form));
			}

			var loadingFactory = new BusinessObjectFactory();
			var reloadedDummy = loadingFactory.Load<DummyBusinessObjectWithDocumentSupport>(Dummy.PK);
			AssertEquals("should not have run the action", 0, CountEvents(reloadedDummy, Events.OriginalBillReceived));
		}

		class MockOperationalActionRunner : OperationalActionRunner
		{
			internal MockOperationalActionRunner(OperationalAction action, Type bizObjectType, ISelectedRecords selectedRecords) : base(action, bizObjectType, selectedRecords)
			{ }

			protected override void RunCore(IOperationalActionLog log, BusinessObjectFactory factoryForChanges, OperationalActionMethodUIMode uiMode, ZGuid[] pks)
			{
				throw new ZCannotSaveException("ZCannotSaveException In MockOperationalActionRunner.", "");
			}
		}

		public void TestShowDialog_Exception()
		{
			ErrorReporter.Clear();
			var stmPrintQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			Factory.Save();

			Dummy.Factory.Save();

			Action.SU_SE_NKDocumentEvent = AutoEvents.OriginalBillReceived.Code;
			Action.DocumentPivots.AddNew();

			var runner = new MockOperationalActionRunner_Exception(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords(Dummy.PK));
			runner.Printer = stmPrintQueue.PK;

			using (var form = new OperationalActionRunnerForm(runner))
			{
				form.Show();
				Application.DoEvents();

				form.PerformClickOk();

				AssertEquals("Form should not have closed", true, form.Visible);
				AssertMultilineASCIIEquals("An System.Exception occurred while saving: Exception\r\nAborted.", GetLogText(form));
			}

			var loadingFactory = new BusinessObjectFactory();
			var reloadedDummy = loadingFactory.Load<DummyBusinessObjectWithDocumentSupport>(Dummy.PK);
			AssertEquals("should not have run the action", 0, CountEvents(reloadedDummy, AutoEvents.OriginalBillReceived));
			AssertEquals("RunActionException_DummyBusinessObjectWithDocumentSupport_Exception", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}
		class MockOperationalActionRunner_Exception : OperationalActionRunner
		{
			internal MockOperationalActionRunner_Exception(OperationalAction action, Type bizObjectType, ISelectedRecords selectedRecords) : base(action, bizObjectType, selectedRecords)
			{ }

			protected override void RunCore(IOperationalActionLog log, BusinessObjectFactory factoryForChanges, OperationalActionMethodUIMode uiMode, ZGuid[] pks)
			{
				throw new Exception("Exception");
			}
		}

		public void TestShowDialog_SaveConflict()
		{
			Dummy.Factory.Save();

			Action.SU_SE_NKDocumentEvent = Events.OriginalBillReceived.Code;
			Action.FieldDescriptors.RemoveAndDeleteAll();

			OperationalActionFieldDescriptor descriptor = Action.FieldDescriptors.AddNew();
			descriptor.FieldName = DummyBizoSchema.Constants.Z0_Decimal;
			descriptor.FieldCaption = "Numeric Field";

			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords(Dummy.PK));
			runner.Printer = ZGuid.NewZGuid();
			runner.UnRegisterEditableChildObject(runner.Fields);

			RunnerNumericField field = (RunnerNumericField)runner.Fields[0];
			field.Property = 1111111111111111111m; // this should trigger a ZSaveException on saving.

			ErrorReporter.Clear();

			AssertEquals("precondition:", 0, CountEvents(Dummy, Events.OriginalBillReceived));

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				form.Show();
				Application.DoEvents();

				form.PerformClickOk();

				const string expectedLog =
					"Starting Section: Events ...\r\n" +
					"Starting Section: Fields ...\r\n" +
					"DummyBizo has the following errors:\r\n" +
					"Error - Z0_Decimal: The number 1,111,111,111,111,111,111 is too large, the maximum value allowed for Decimal is 999,999,999,999,999,999.\r\n" +
					"Aborted.\r\n" +
					"";

				AssertEquals("Form should not have closed", true, form.Visible);
				AssertMultilineASCIIEquals("", expectedLog, GetLogText(form));
			}

			var loadingFactory = new BusinessObjectFactory();
			var reloadedDummy = loadingFactory.Load<DummyBusinessObjectWithDocumentSupport>(Dummy.PK);
			AssertEquals("should not have run the action", 0, CountEvents(reloadedDummy, Events.OriginalBillReceived));
		}

		public void TestShowForm_DbHits()
		{
			var dbHits = new Dictionary<string, int>
			{
				{ DummyBizoSchema.Constants.TableName, 1 }
			};

			using (AssertDbHitsForAllFactories(dbHits, ignoreUnspecified: true))
			using (var dummyModule = new DummyFilterGridModule())
			{
				var moduleSelection = new ModuleSelection(dummyModule);
				var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();
				}
			}
		}

		public void TestRunOnRecordsRadioButtons_DefaultValueOnOpening()
		{
			using (var dummyModule = new DummyFilterGridModule())
			{
				var moduleSelection = new ModuleSelection(dummyModule);
				var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();

					AssertEquals(true, form.RunOnAllMatchingRecordsRadioButton.Checked);
					AssertEquals(false, form.RunOnSelectedRecordsRadioButton.Checked);
					AssertEquals(form.FormHeading, "Run '' on 0 records");
				}

				moduleSelection = new ModuleSelection(dummyModule);
				dummyModule.SelectedBusinessObjectsOverride = new[] { Dummy };

				runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();

					AssertEquals(false, form.RunOnAllMatchingRecordsRadioButton.Checked);
					AssertEquals(true, form.RunOnSelectedRecordsRadioButton.Checked);
					AssertEquals(form.FormHeading, "Run '' on 1 record");
				}
			}
		}

		public void TestRunOnRecordsRadioButtons_Visibility()
		{
			using (var dummyModule = new DummyFilterGridModule())
			{
				var moduleSelection = new ModuleSelection(dummyModule);
				var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				using (var form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();

					AssertEquals(true, form.RunOnAllMatchingRecordsRadioButton.Visible);
					AssertEquals(true, form.RunOnSelectedRecordsRadioButton.Visible);
				}

				var dumbSelection = new DumbTargetRecordSelection();
				runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), dumbSelection);
				using (var form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();

					AssertEquals(false, form.RunOnAllMatchingRecordsRadioButton.Visible);
					AssertEquals(false, form.RunOnSelectedRecordsRadioButton.Visible);
				}
			}
		}

		public void TestRunOnRecordsRadioButtons_FormHeadingUpdatesOnClick()
		{
			using (var dummyModule = new DummyFilterGridModule())
			{
				var moduleSelection = new ModuleSelection(dummyModule);
				Factory.New<DummyBusinessObjectWithDocumentSupport>();
				Factory.New<DummyBusinessObjectWithDocumentSupport>();
				Factory.Save();

				var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();

					AssertEquals(true, form.RunOnAllMatchingRecordsRadioButton.Checked);
					AssertEquals(false, form.RunOnSelectedRecordsRadioButton.Checked);
					AssertEquals(form.FormHeading, "Run '' on 2 records");

					form.RunOnSelectedRecordsRadioButton.PerformClick();
					AssertEquals(true, form.RunOnSelectedRecordsRadioButton.Checked);
					AssertEquals(form.FormHeading, "Run '' on 0 records");

					form.RunOnAllMatchingRecordsRadioButton.PerformClick();
					AssertEquals(true, form.RunOnAllMatchingRecordsRadioButton.Checked);
					AssertEquals(form.FormHeading, "Run '' on 2 records");
				}
			}
		}

		public void TestRunOnRecordsRadioButtons_ShowDialogWhenNoRecordsSelected()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			using (var dummyModule = new DummyFilterGridModule())
			{
				var moduleSelection = new ModuleSelection(dummyModule);
				Factory.New<DummyBusinessObjectWithDocumentSupport>();
				Factory.New<DummyBusinessObjectWithDocumentSupport>();
				Factory.Save();

				var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();

					form.RunOnSelectedRecordsRadioButton.PerformClick();
					AssertEquals(false, form.RunOnAllMatchingRecordsRadioButton.Checked);

					form.PerformClickOk();
					AssertEquals("Please select at least one record in the module screen, or select 'Run on all matching records'.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();

					form.RunOnAllMatchingRecordsRadioButton.PerformClick();
					AssertEquals(false, form.RunOnSelectedRecordsRadioButton.Checked);

					form.PerformClickOk();
					AssertEquals("Are you sure you want to run this action on 2 records?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestRunOnSelectedRecords()
		{
			Action.FieldDescriptors.RemoveAndDeleteAll();

			OperationalActionFieldDescriptor descriptor = Action.FieldDescriptors.AddNew();
			descriptor.FieldName = DummyBizoSchema.Constants.Z0_Description;
			descriptor.FieldCaption = "Generic Description";
			descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;

			UnitTestUserNotification.Instance.ClearMessages();

			var dummyBizos = new DummyBusinessObjectWithDocumentSupport[5];
			Enumerable.Range(0, 5).ForEach(x =>
			{
				dummyBizos[x] = Factory.New<DummyBusinessObjectWithDocumentSupport>();
				dummyBizos[x].Z0_Description = "I Can Change";
			});

			Factory.Save();

			using (var dummyModule = new DummyModuleWithActionsSupport<DummyBusinessObjectWithDocumentSupport>())
			{
				var moduleSelection = new ModuleSelection(dummyModule);

				dummyModule.SelectedBusinessObjectsOverride = new[] { dummyBizos[0], dummyBizos[2] };

				var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				var field = (RunnerTextField)runner.Fields[0];
				field.Property = "I Have Changed";

				using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();

					AssertEquals("Run '' on 2 records", form.FormHeading);
					AssertEquals(true, form.RunOnSelectedRecordsRadioButton.Checked);

					form.PerformClickOk();
					AssertEquals("Are you sure you want to run this action on 2 records?", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertMultilineASCIIEquals(@"
Starting Section: Fields ...
Saving ...
Saved.
Done.
", GetLogText(form));

					AssertEquals(false, form.progressControl.ShowMainProgressBar);
					AssertEquals(true, form.progressControl.ShowSectionProgressBar);

					var loadedDummyBizos = new DummyBusinessObjectWithDocumentSupport[5];
					var loadingFactory = new BusinessObjectFactory();
					Enumerable.Range(0, 5).ForEach(x =>
					{
						loadedDummyBizos[x] = loadingFactory.Load<DummyBusinessObjectWithDocumentSupport>(dummyBizos[x].PK);
					});

					CombineAssertions(() =>
					{
						AssertEquals("I Have Changed", loadedDummyBizos[0].Z0_Description);
						AssertEquals("I Can Change", loadedDummyBizos[1].Z0_Description);
						AssertEquals("I Have Changed", loadedDummyBizos[2].Z0_Description);
						AssertEquals("I Can Change", loadedDummyBizos[3].Z0_Description);
						AssertEquals("I Can Change", loadedDummyBizos[4].Z0_Description);
					});

					CombineAssertions("Original bizos should update since data refresh is enabled when running on selected records", () =>
					{
						AssertEquals("I Have Changed", dummyBizos[0].Z0_Description);
						AssertEquals("I Can Change", dummyBizos[1].Z0_Description);
						AssertEquals("I Have Changed", dummyBizos[2].Z0_Description);
						AssertEquals("I Can Change", dummyBizos[3].Z0_Description);
						AssertEquals("I Can Change", dummyBizos[4].Z0_Description);
					});
				}
			}
		}

		void RunOnAllRecords(int recordSize)
		{
			Env.Registry.OperationalActionsRecordBatchSize = 2;

			Action.FieldDescriptors.RemoveAndDeleteAll();

			OperationalActionFieldDescriptor descriptor = Action.FieldDescriptors.AddNew();
			descriptor.FieldName = DummyBizoSchema.Constants.Z0_Description;
			descriptor.FieldCaption = "Generic Description";
			descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;

			UnitTestUserNotification.Instance.ClearMessages();

			using (var dummyModule = new DummyFilterGridModule())
			{
				var moduleSelection = new ModuleSelection(dummyModule);

				var dummyBizos = new DummyBusinessObjectWithDocumentSupport[recordSize];
				Enumerable.Range(0, recordSize).ForEach(x =>
				{
					dummyBizos[x] = Factory.New<DummyBusinessObjectWithDocumentSupport>();
					dummyBizos[x].Z0_Description = "I Can Change";
				});

				Factory.Save();

				var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				var field = (RunnerTextField)runner.Fields[0];
				field.Property = "I Have Changed";

				using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();

					form.RunOnAllMatchingRecordsRadioButton.PerformClick();
					AssertEquals($"Run '' on {recordSize} records", form.FormHeading);
					AssertEquals(false, form.RunOnSelectedRecordsRadioButton.Checked);

					form.PerformClickOk();
					AssertEquals($"Are you sure you want to run this action on {recordSize} records?", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertMultilineASCIIEquals("batch size of 2, should run over five batches", @"
Starting Batch 1 of 5: ...
Starting Section: Fields ...
Saving ...
Saved.
Done.
Starting Batch 2 of 5: ...
Starting Section: Fields ...
Saving ...
Saved.
Done.
Starting Batch 3 of 5: ...
Starting Section: Fields ...
Saving ...
Saved.
Done.
Starting Batch 4 of 5: ...
Starting Section: Fields ...
Saving ...
Saved.
Done.
Starting Batch 5 of 5: ...
Starting Section: Fields ...
Saving ...
Saved.
Done.
", GetLogText(form));

					AssertEquals(true, form.progressControl.ShowMainProgressBar);
					AssertEquals(false, form.progressControl.ShowSectionProgressBar);

					var loadingFactory = new BusinessObjectFactory();
					Enumerable.Range(0, recordSize).ForEach(x =>
					{
						var loadedDummyBizo = loadingFactory.Load<DummyBusinessObjectWithDocumentSupport>(dummyBizos[x].PK);
						AssertEquals($"In record {x}", "I Have Changed", loadedDummyBizo.Z0_Description);
					});

					Enumerable.Range(0, recordSize).ForEach(x =>
					{
						AssertEquals("Do not update original bizos since data refresh is disabled when running on all records", "I Can Change", dummyBizos[x].Z0_Description);
					});
				}
			}
		}

		public void TestRunOnAllRecords_EnsureAllRecordsAreProcessed()
		{
			RunOnAllRecords(recordSize: 10);
		}

		public void TestRunOnAllRecords_EnsureAllRecordsAreProcessedAndBatchedCorrectly()
		{
			RunOnAllRecords(recordSize: 9);
		}

		void RunOnAllRecords_ModuleFilterApplied(bool useDumbDummyFilterGridModule)
		{
			Env.Registry.OperationalActionsRecordBatchSize = 2;

			Action.FieldDescriptors.RemoveAndDeleteAll();

			OperationalActionFieldDescriptor descriptor = Action.FieldDescriptors.AddNew();
			descriptor.FieldName = DummyBizoSchema.Constants.Z0_Description;
			descriptor.FieldCaption = "Generic Description";
			descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;

			UnitTestUserNotification.Instance.ClearMessages();

			using (var dummyModule = useDumbDummyFilterGridModule ? new DumbDummyFilterGridModule() : new DummyFilterGridModule())
			{
				var filter = dummyModule.FilterBusinessObject.ModuleFilters.AddNumberRangeFilter("Number", DummyBizoSchema.Z0_Number);
				filter.Property1 = 0;
				filter.Property2 = 5;
				filter.IsActive = true;
				dummyModule.FilterBusinessObject.Factory.Save();

				var moduleSelection = new ModuleSelection(dummyModule);

				var dummyBizos = new DummyBusinessObjectWithDocumentSupport[10];
				Enumerable.Range(0, 10).ForEach(x =>
				{
					dummyBizos[x] = Factory.New<DummyBusinessObjectWithDocumentSupport>();
					dummyBizos[x].Z0_Description = "I Can Change";
					dummyBizos[x].Z0_Number = x + 1;
				});

				Factory.Save();

				var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				var field = (RunnerTextField)runner.Fields[0];
				field.Property = "I Have Changed";

				using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();

					form.RunOnAllMatchingRecordsRadioButton.PerformClick();
					AssertEquals(false, form.RunOnSelectedRecordsRadioButton.Checked);
					AssertEquals("Run '' on 5 records", form.FormHeading);

					form.PerformClickOk();
					AssertEquals("Are you sure you want to run this action on 5 records?", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertMultilineASCIIEquals("5 records, batch size of 2, should run over three batches", @"
Starting Batch 1 of 3: ...
Starting Section: Fields ...
Saving ...
Saved.
Done.
Starting Batch 2 of 3: ...
Starting Section: Fields ...
Saving ...
Saved.
Done.
Starting Batch 3 of 3: ...
Starting Section: Fields ...
Saving ...
Saved.
Done.
", GetLogText(form));

					AssertEquals(true, form.progressControl.ShowMainProgressBar);
					AssertEquals(false, form.progressControl.ShowSectionProgressBar);

					var loadingFactory = new BusinessObjectFactory();
					Enumerable.Range(0, 5).ForEach(x =>
					{
						var loadedDummyBizo = loadingFactory.Load<DummyBusinessObjectWithDocumentSupport>(dummyBizos[x].PK);
						AssertEquals($"In record {x}", "I Have Changed", loadedDummyBizo.Z0_Description);
					});

					Enumerable.Range(5, 5).ForEach(x =>
					{
						var loadedDummyBizo = loadingFactory.Load<DummyBusinessObjectWithDocumentSupport>(dummyBizos[x].PK);
						AssertEquals($"In record {x}", "I Can Change", loadedDummyBizo.Z0_Description);
					});
				}
			}
		}

		public void TestRunOnAllRecords_WithModuleFilterApplied()
		{
			RunOnAllRecords_ModuleFilterApplied(useDumbDummyFilterGridModule: false);
		}

		[ExpectNoExceptions]
		public void TestRunOnAllRecords_ShouldNotUseFilterGridModuleTopLevelBusinessObject()
		{
			RunOnAllRecords_ModuleFilterApplied(useDumbDummyFilterGridModule: true);
		}

		public void TestRunOnAllRecords_ModuleWithRelationshipFilter()
		{
			Env.Registry.OperationalActionsRecordBatchSize = 2;

			Action.FieldDescriptors.RemoveAndDeleteAll();

			OperationalActionFieldDescriptor descriptor = Action.FieldDescriptors.AddNew();
			descriptor.FieldName = DummyBizoSchema.Constants.Z0_Description;
			descriptor.FieldCaption = "Generic Description";
			descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;

			UnitTestUserNotification.Instance.ClearMessages();

			using (var dummyModule = new DummyFilterGridModuleWithActiveBOC())
			{
				dummyModule.RelationshipFilter = new ZQuery(DummyBizoSchema.Z0_Bool, true);
				var moduleSelection = new ModuleSelection(dummyModule);

				var dummy1 = Factory.New<DummyBusinessObjectWithDocumentSupport>();
				dummy1.Z0_Bool = true;
				var dummy2 = Factory.New<DummyBusinessObjectWithDocumentSupport>();
				dummy2.Z0_Bool = false;
				var dummy3 = Factory.New<DummyBusinessObjectWithDocumentSupport>();
				dummy3.Z0_Bool = true;
				var dummy4 = Factory.New<DummyBusinessObjectWithDocumentSupport>();
				dummy4.Z0_Bool = true;
				var dummy5 = Factory.New<DummyBusinessObjectWithDocumentSupport>();
				dummy5.Z0_Bool = true;

				Factory.Save();

				var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), moduleSelection);
				var field = (RunnerTextField)runner.Fields[0];
				field.Property = "I Have Changed";

				using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();

					form.RunOnAllMatchingRecordsRadioButton.PerformClick();
					AssertEquals("Run '' on 4 records", form.FormHeading);
					AssertEquals(false, form.RunOnSelectedRecordsRadioButton.Checked);

					form.PerformClickOk();
					AssertEquals("Are you sure you want to run this action on 4 records?", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertMultilineASCIIEquals("batch size of 2, should run over 2 batches only", @"
Starting Batch 1 of 2: ...
Starting Section: Fields ...
Saving ...
Saved.
Done.
Starting Batch 2 of 2: ...
Starting Section: Fields ...
Saving ...
Saved.
Done.
", GetLogText(form));
				}
			}
		}

		public void TestRunOnRecords_Regex()
		{
			Env.Registry.OperationalActionsRecordBatchSize = 2;

			actionSupporter = new MockOperationalActionSupportable(typeof(DummyWithWorkflow)).OperationalActionSupporter;
			context = null;

			var action = Factory.New<OperationalAction>();
			action.Context = Context;

			var descriptor = action.FieldDescriptors.AddNew();
			descriptor.FieldName = "WorkflowItems." + AutoProcessTasks.Schema.P9_Type;
			descriptor.FieldCaption = "Generic Description";
			descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;
			descriptor.Filter = "WorkflowItems.P9_Type==\"TS?\"";

			UnitTestUserNotification.Instance.ClearMessages();

			using (var module = new DummyFilterGridModule())
			{
				var moduleSelection = new ModuleSelection(module);
				var temp = moduleSelection.Module.ModuleFilters;

				var bizos = new DummyWithWorkflow[10];
				Enumerable.Range(0, 10).ForEach(x =>
				{
					bizos[x] = Factory.New<DummyWithWorkflow>();
				});

				var bizoWorkflows = bizos.Cast<IWorkflowProvider>().ToArray();

				var tasks = new ProcessTask[10];
				Enumerable.Range(0, 10).ForEach(x =>
				{
					tasks[x] = bizoWorkflows[x].WorkflowItems.Tasks.AddNew();
					tasks[x].P9_Description = "Test" + x;
					tasks[x].P9_Sequence = x;
					tasks[x].P9_Type = "TS1";
					tasks[x].P9_Status = "ASN";
				});

				Factory.Save();

				var runner = new OperationalActionRunner(action, typeof(DummyWithWorkflow), moduleSelection);
				var field = (RunnerTextField)runner.Fields[0];
				field.Property = "UDF";

				using (var form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();

					form.RunOnAllMatchingRecordsRadioButton.PerformClick();
					AssertEquals($"Run '' on {10} records", form.FormHeading);
					AssertEquals(false, form.RunOnSelectedRecordsRadioButton.Checked);

					form.PerformClickOk();
					AssertEquals($"Are you sure you want to run this action on {10} records?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, form.progressControl.ShowMainProgressBar);
					AssertEquals(false, form.progressControl.ShowSectionProgressBar);

					var loadingFactory = new BusinessObjectFactory();
					Enumerable.Range(0, 10).ForEach(x =>
					{
						var loadedBizo = loadingFactory.Load<DummyWithWorkflow>(bizos[x].PK);
						var loadedBizoWorkflow = (IWorkflowProvider)loadedBizo;
						var loadedTask = loadedBizoWorkflow.WorkflowItems.Tasks[0];
						AssertEquals($"In record {x}", "UDF", loadedTask.P9_Type);
					});

					Enumerable.Range(0, 10).ForEach(x =>
					{
						AssertEquals("TS1", tasks[x].P9_Type);
					});
				}
			}
		}

		public void TestOverrideSelectionCount()
		{
			var action = Factory.New<OperationalAction>();
			action.Context = Context;
			action.SU_MenuName = "Test";

			using (var module = new DummyFilterGridModule())
			{
				var moduleSelection = new ModuleSelection(module);
				var runner = new OperationalActionRunner(action, typeof(DummyWithWorkflow), moduleSelection);
				runner.MethodApplicators.Add(new MockMethodApplicatorWithOverrideSelection("Test applicator", Factory));

				using (var form = new OperationalActionRunnerForm(runner))
				{
					form.Show();
					Application.DoEvents();

					AssertEquals("RunOnSelectedRecords", expected: false, runner.RunOnSelectedRecords);
					AssertEquals("RunOnAllMatchingRecords", expected: true, runner.RunOnAllMatchingRecords);
					AssertEquals("RunOnAllMatchingRecordsRadioButton.Visible", expected: false, form.RunOnAllMatchingRecordsRadioButton.Visible);
					AssertEquals("RunOnSelectedRecordsRadioButton.Visible", expected: false, form.RunOnSelectedRecordsRadioButton.Visible);
					AssertEquals("FormHeading", "Run 'Test'", form.FormHeading);
				}
			}
		}

		#region Implementation

		class MockMethodApplicatorWithOverrideSelection : OperationalActionMethodApplicator, IOverrideSelectionCount
		{
			public MockMethodApplicatorWithOverrideSelection(string name) : base(name)
			{
			}

			public MockMethodApplicatorWithOverrideSelection(string name, BusinessObjectFactory factory) : base(name, factory)
			{
			}

			public int SelectionCount => 5;

			public bool SupportRunningOnAllMatchedRecords => true;

			protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
			{
				throw new NotImplementedException();
			}
		}

		class DumbTargetRecordSelection : ITargetRecordSelection
		{
			public IList<string> ExclusionReasons => new List<string>();

			public int FilterRowCount => 0;

			public ISelectedRecords GetSelectedRecords() => new SelectedRecords();
		}

		class DumbDummyFilterGridModule : DummyFilterGridModule
		{
			protected override ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				LastController = new DumbDummyController();
				return LastController;
			}
		}

		class DumbDummyController : DummyController
		{
			public override Type TypeOfTopLevelBusinessObject => typeof(BusinessObject);
		}

		protected override Form GetFormToBashCore()
		{
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			return new OperationalActionRunnerForm(runner);
		}

		static string[] GetTabNames(OperationalActionRunnerForm form)
		{
			TabControl tabControl = GetTabControl(form);

			string[] captions = new string[tabControl.TabPages.Count];
			for (int i = 0; i < tabControl.TabPages.Count; i++)
			{
				captions[i] = tabControl.TabPages[i].Text;
			}
			return captions;
		}

		static TabControl GetTabControl(OperationalActionRunnerForm form)
		{
			return (TabControl)form.Controls["runnerTabControl"];
		}

		static string GetLogText(OperationalActionRunnerForm form)
		{
			if (form == null)
			{
				throw new ArgumentNullException(nameof(form));
			}

			var log = (ActionLog)typeof(OperationalActionRunnerForm).InvokeMember("progressControl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, form, null)
				?? throw new InvalidOperationException("unable to find the action log");

			if (log.IsDisposed)
			{
				throw new InvalidOperationException("action log already disposed");
			}

			var textBox = (RichTextBox)log.Controls["logTextBox"] ?? throw new InvalidOperationException("unable to find the rich text box on the action log");

			return textBox.Text;
		}

		void AssertTabsFit(TabControl tabControl)
		{
			foreach (TabPage page in tabControl.TabPages)
			{
				ActionMethodTabPage aPage = page as ActionMethodTabPage;
				if (aPage == null)
				{
					continue;
				}

				if (aPage.MinimumContentSize.Width > aPage.ClientRectangle.Width || aPage.MinimumContentSize.Height > aPage.ClientRectangle.Height)
				{
					Fail(string.Format("The control for \"{0}\" ({1}, {2}) does not fit within it's tab ({3}, {4}).",
						aPage.Text, aPage.MinimumContentSize.Width, aPage.MinimumContentSize.Height,
						aPage.ClientRectangle.Width, aPage.ClientRectangle.Height));
				}
			}

			Assert(true);
		}

		int CountEvents(BusinessObject bizObj, Event eventType)
		{
			StmALog[] logs = bizObj.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, eventType.Code));
			return logs.Length;
		}

		DummyBusinessObjectWithDocumentSupport Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyBusinessObjectWithDocumentSupport>()); }
		}
		DummyBusinessObjectWithDocumentSupport dummy;

		OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
					{
						OperationalActionFieldDescriptor descriptor = action.FieldDescriptors.AddNew();
						descriptor.FieldName = "Undefined Field";
						descriptor.FieldCaption = "Undefined Field";
					}

					int i = 0;
					foreach (OperationalActionFieldSupporter fieldSupporter in Context.FieldSupporters)
					{
						OperationalActionFieldDescriptor descriptor = action.FieldDescriptors.AddNew();
						descriptor.FieldName = fieldSupporter.Field;
						descriptor.FieldCaption = string.Format("Caption {0}", i++);
					}
				}
				return action;
			}
		}
		OperationalAction action;

		OperationalActionContext Context
		{
			get
			{
				if (context == null)
				{
					context = new OperationalActionContext(ActionSupporter, "Module Name");

					foreach (PropertyInfo info in ActionSupporter.RootType.GetProperties())
					{
						var a = context.FieldSupporters[info.Name];
					}
				}
				return context;
			}
		}
		OperationalActionContext context;

		OperationalActionSupporter ActionSupporter
		{
			get { return actionSupporter ?? (actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter); }
		}
		OperationalActionSupporter actionSupporter;

		#endregion
	}
}
