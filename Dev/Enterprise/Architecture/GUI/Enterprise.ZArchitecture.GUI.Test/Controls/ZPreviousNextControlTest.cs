using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
#pragma warning disable WTG1001 // Do not use the 'private' keyword.
namespace Enterprise.ZArchitecture.GUI.Testing
{
	internal class ZPreviousNextControlTest : TestCaseWithFactory
	{
		public void TestTabStop()
		{
			using (var control = new ZPreviousNextControl(new ModuleResultsBusinessObject(new ZPKCollection(new List<ZGuid>())), null))
			{
				AssertEquals(false, control.TabStop);
			}
		}

		public void TestToolTipOnButtonsDisplayed()
		{
			Form = Module.ShowNewForm() as ZDummyForm;
			Form.WindowState = FormWindowState.Maximized;
			Form.Show();

			AssertEquals("The form is not maximased", FormWindowState.Maximized, Form.WindowState);
			Assert("Next button enabled", Form.PreviousNextControlForTesting.NextButtonForTesting.Enabled);
			Assert("Previous button disabled", Form.PreviousNextControlForTesting.PreviousButtonForTesting.Enabled);

			Form.PreviousNextControlForTesting.NextButtonForTesting.MouseHover += new EventHandler(TestNextButton_MouseHover);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1046:DoNotSpecifyTooltipsManuallyRule", Justification = "Testing")]
		private void TestNextButton_MouseHover(object sender, EventArgs e)
		{
			ZButton current = Form.PreviousNextControlForTesting.NextButtonForTesting;
			current.ToolTipCaption = (NoResString)"Next";
			AssertEquals(true, ToolTipService.HasToolTip(current));
			AssertEquals("Next", ToolTipService.GetToolTip(current));
		}

		public void TestKeepsSameTabPageSelectedWhenMoveToNewForm()
		{
			Module.ShowEditForm(Dummy);
			Form = (ZDummyForm)Module.LastController.LastShownForm;
			Form.TopLevelTabControl.SelectedIndex = 1;
			var oldSelectedTabName = Form.TopLevelTabControl.SelectedTab.Name;
			GoNext();
			AssertEquals("Same tab selected", oldSelectedTabName, Form2.TopLevelTabControl.SelectedTab.Name);
		}

		public void TestNewHasNoPreviousNextButtonsUntilSave()
		{
			Form = Module.ShowNewForm() as ZDummyForm;
			Form.Show();

			Assert("Invisible on new", !Form.PreviousNextControlForTesting.Visible);

			Form.FireSaveButton();
			Assert("Visible now as it's in the list", Form.PreviousNextControlForTesting.Visible);
		}

		public void TestGoingNext()
		{
			((IFilterGridModuleInternalsForTesting)Module).GridCollection.ApplySort(new SortInfo(DummyBusinessObject.Schema.Z0_Code, ListSortDirection.Ascending));
			Module.ShowEditForm(Dummy);

			Form = Module.LastController.LastShownForm as ZDummyForm;

			Assert("Next button enabled", Form.PreviousNextControlForTesting.NextButtonForTesting.Enabled);
			Assert("Previous button disabled", !Form.PreviousNextControlForTesting.PreviousButtonForTesting.Enabled);

			Form2 = OpenedFormCache.GetInstance().GetForm(Dummy2.PK.ToGuid(), DummyModuleIDs.Dummy.ToString()) as ZDummyForm;
			AssertNull("Should  not have found next opened form", Form2);

			Form.PreviousNextControlForTesting.FireNextButtonForTesting();
			WaitForClose(Form);
			Form2 = OpenedFormCache.GetInstance().GetForm(Dummy2.PK.ToGuid(), DummyModuleIDs.Dummy.ToString()) as ZDummyForm;
			AssertNotNull("Should have found next opened form", Form2);

			Assert("First form should be killed", Form.IsDisposed);
		}

		public void TestGoingPrevious()
		{
			((IFilterGridModuleInternalsForTesting)Module).GridCollection.ApplySort(new SortInfo(DummyBusinessObject.Schema.Z0_Code, ListSortDirection.Descending));

			Module.ShowEditForm(Dummy);

			Form = Module.LastController.LastShownForm as ZDummyForm;
			Assert("Next button disabled", !Form.PreviousNextControlForTesting.NextButtonForTesting.Enabled);
			Assert("Previous button enabled", Form.PreviousNextControlForTesting.PreviousButtonForTesting.Enabled);

			Form2 = OpenedFormCache.GetInstance().GetForm(Dummy2.PK.ToGuid(), DummyModuleIDs.Dummy.ToString()) as ZDummyForm;
			AssertNull("Should  not have found next opened form", Form2);

			Form.PreviousNextControlForTesting.FirePreviousButtonForTesting();
			WaitForClose(Form);
			Form2 = OpenedFormCache.GetInstance().GetForm(Dummy2.PK.ToGuid(), DummyModuleIDs.Dummy.ToString()) as ZDummyForm;
			AssertNotNull("Should have found next opened form", Form2);

			Assert("First form should be killed", Form.IsDisposed);
		}

#if !WINZOR
		public void TestNoNotSupportedExceptionThrown_GoingNext()
		{
			((IFilterGridModuleInternalsForTesting)Module).GridCollection.ApplySort(new SortInfo(DummyBusinessObject.Schema.Z0_Code, ListSortDirection.Ascending));
			Module.ShowEditForm(Dummy);

			Form = Module.LastController.LastShownForm as ZDummyForm;
			using (ZDummyForm.ThrowSqlExceptionWhenSetVisible())
			{
				AssertExceptionThrown<SqlException>(() => Form.PreviousNextControlForTesting.FireNextButtonForTesting());
				AssertEquals("First form was not killed", false, Form.IsDisposed);
			}

			AssertNoExceptionThrown("No NotSupportedException thrown", () => Form.PreviousNextControlForTesting.FireNextButtonForTesting());
			WaitForClose(Form);

			Form2 = OpenedFormCache.GetInstance().GetForm(Dummy2.PK.ToGuid(), DummyModuleIDs.Dummy.ToString()) as ZDummyForm;
			AssertNotNull("Should have found next opened form", Form2);
			AssertEquals("First form should be killed", true, Form.IsDisposed);
		}

		public void TestNoNotSupportedExceptionThrown_GoingPrevious()
		{
			((IFilterGridModuleInternalsForTesting)Module).GridCollection.ApplySort(new SortInfo(DummyBusinessObject.Schema.Z0_Code, ListSortDirection.Descending));
			Module.ShowEditForm(Dummy);

			Form = Module.LastController.LastShownForm as ZDummyForm;
			using (ZDummyForm.ThrowSqlExceptionWhenSetVisible())
			{
				AssertExceptionThrown<SqlException>(() => Form.PreviousNextControlForTesting.FirePreviousButtonForTesting());
				AssertEquals("First form was not killed", false, Form.IsDisposed);
			}

			AssertNoExceptionThrown("No NotSupportedException thrown", () => Form.PreviousNextControlForTesting.FirePreviousButtonForTesting());
			WaitForClose(Form);

			Form2 = OpenedFormCache.GetInstance().GetForm(Dummy2.PK.ToGuid(), DummyModuleIDs.Dummy.ToString()) as ZDummyForm;
			AssertNotNull("Should have found next opened form", Form2);
			AssertEquals("First form should be killed", true, Form.IsDisposed);
		}
#endif

		public void TestEnteringRecordNumberToJumpTo()
		{
			((IFilterGridModuleInternalsForTesting)Module).GridCollection.ApplySort(new SortInfo(DummyBusinessObject.Schema.Z0_Code, ListSortDirection.Ascending));
			Module.ShowEditForm(Dummy);

			Form = Module.LastController.LastShownForm as ZDummyForm;

			Form2 = OpenedFormCache.GetInstance().GetForm(Dummy2.PK.ToGuid(), DummyModuleIDs.Dummy.ToString()) as ZDummyForm;
			AssertNull("Should  not have found next opened form", Form2);

			Form.ModuleResultsBusinessObject.CurrentRecordNumberAllowingInvalid = 2;
			WaitForClose(Form);
			Form2 = OpenedFormCache.GetInstance().GetForm(Dummy2.PK.ToGuid(), DummyModuleIDs.Dummy.ToString()) as ZDummyForm;
			AssertNotNull("Should have found next opened form", Form2);
			Assert("Next button disabled", !Form2.PreviousNextControlForTesting.NextButtonForTesting.Enabled);
			Assert("Previous button enabled", Form2.PreviousNextControlForTesting.PreviousButtonForTesting.Enabled);

			Assert("First form should be killed", Form.IsDisposed);
		}

		public void TestMaintainsDisplayModeEdit()
		{
			Module.ShowEditForm(Dummy);

			CheckMatchingDisplayModeOnSwapForms();
		}

		public void TestMaintainsDisplayModeView()
		{
			Module.ShowViewForm(Dummy);

			CheckMatchingDisplayModeOnSwapForms();
		}

		public void TestMaintainsDisplayModeDelete()
		{
			Module.ShowDeleteForm(Dummy);

			CheckMatchingDisplayModeOnSwapForms();
		}

		void CheckMatchingDisplayModeOnSwapForms()
		{
			GoNext();
			AssertEquals("Same display mode for new form", Form.DisplayMode, Form2.DisplayMode);
		}

		void GoNext()
		{
			((IFilterGridModuleInternalsForTesting)Module).GridCollection.ApplySort(new SortInfo(DummyBusinessObject.Schema.Z0_Code, ListSortDirection.Ascending));
			Form = Module.LastController.LastShownForm as ZDummyForm;
			Form.PreviousNextControlForTesting.FireNextButtonForTesting();
			WaitForClose(Form);

			Form2 = OpenedFormCache.GetInstance().GetForm(Dummy2.PK.ToGuid(), DummyModuleIDs.Dummy.ToString()) as ZDummyForm;
		}

		public void TestWarnOfChangesAndUserCancels()
		{
			Module.ShowEditForm(Dummy);
			((IFilterGridModuleInternalsForTesting)Module).GridCollection.ApplySort(new SortInfo(DummyBusinessObject.Schema.Z0_Code, ListSortDirection.Ascending));
			Form = Module.LastController.LastShownForm as ZDummyForm;
			((DummyBusinessObject)Form.BusinessEntity).Z0_AnotherNumber = 35235;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			Form.PreviousNextControlForTesting.FireNextButtonForTesting();
			WaitForClose(Form);

			Form2 = OpenedFormCache.GetInstance().GetForm(Dummy2.PK.ToGuid(), DummyModuleIDs.Dummy.ToString()) as ZDummyForm;

			AssertNull("Should not have moved to next form", Form2);
			Assert("First form still alive", !Form.IsDisposed);
		}

		public void TestWarnOfChangesAndUserSaves()
		{
			Module.ShowEditForm(Dummy);
			((IFilterGridModuleInternalsForTesting)Module).GridCollection.ApplySort(new SortInfo(DummyBusinessObject.Schema.Z0_Code, ListSortDirection.Ascending));
			Form = Module.LastController.LastShownForm as ZDummyForm;
			((DummyBusinessObject)Form.BusinessEntity).Z0_AnotherNumber = 35235;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Form.PreviousNextControlForTesting.FireNextButtonForTesting();
			WaitForClose(Form);

			Form2 = OpenedFormCache.GetInstance().GetForm(Dummy2.PK.ToGuid(), DummyModuleIDs.Dummy.ToString()) as ZDummyForm;

			Assert("Saved", !Dummy.HasChanges);
			AssertNotNull("Should have moved to next form", Form2);
			Assert("First form dead", Form.IsDisposed);
		}

		public void TestWarnOfChangesAndUserDoesNotSave()
		{
			Module.ShowEditForm(Dummy);
			((IFilterGridModuleInternalsForTesting)Module).GridCollection.ApplySort(new SortInfo(DummyBusinessObject.Schema.Z0_Code, ListSortDirection.Ascending));
			Form = Module.LastController.LastShownForm as ZDummyForm;
			((DummyBusinessObject)Form.BusinessEntity).Z0_AnotherNumber = 35235;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			Form.PreviousNextControlForTesting.FireNextButtonForTesting();
			WaitForClose(Form);

			Form2 = OpenedFormCache.GetInstance().GetForm(Dummy2.PK.ToGuid(), DummyModuleIDs.Dummy.ToString()) as ZDummyForm;

			Assert("Not saved", ((IBusiness)Form.LastDataSourceForTest).HasChanges);
			AssertNotNull("Should have moved to next form", Form2);
			Assert("First form dead", Form.IsDisposed);
		}

		#region DoNoOverrideZPreviousNextControlButtonEditableModeInViewOnly

		public void TestDoNoOverrideZPreviousNextControlButtonEditableMode()
		{
			using (var previousNextControl = new ZPreviousNextControl { Name = "TestPreviousNextControl" })
			using (var form = new ZForm())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				((DummyFilterGridModule)form.GetModule()).SetAllowEdit(false);
				previousNextControl.NextButtonForTesting.Enabled = false;
				previousNextControl.PreviousButtonForTesting.Enabled = false;
				previousNextControl.NextButtonForTesting.DoNoOverrideMyEditableMode = true;
				previousNextControl.PreviousButtonForTesting.DoNoOverrideMyEditableMode = true;
				form.Controls.Add(previousNextControl);

				previousNextControl.SetReadOnlyIncludingChildren();

				Assert("Next button disabled", !previousNextControl.NextButtonForTesting.Enabled);
				Assert("Previous button disabled", !previousNextControl.PreviousButtonForTesting.Enabled);

				form.Controls.Remove(previousNextControl);

				previousNextControl.NextButtonForTesting.DoNoOverrideMyEditableMode = false;
				previousNextControl.PreviousButtonForTesting.DoNoOverrideMyEditableMode = false;
				form.Controls.Add(previousNextControl);

				previousNextControl.SetReadOnlyIncludingChildren();

				Assert("Next button enabled", previousNextControl.NextButtonForTesting.Enabled);
				Assert("Previous button enabled", previousNextControl.PreviousButtonForTesting.Enabled);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			ZDummyForm.ResetPreviousNextOverrideValues();
			Assert("initial state", !ZDummyForm.OverrideDefaultAddPreviousNextValue);
			Assert("initial state", ZDummyForm.OverridenAddPreviousNextValue);

			SetupDummies();
			SetupModule();
		}

		protected override void TearDown()
		{
			base.TearDown();
			Module.Dispose();
			KillForms();
		}

		ZDummyForm Form;
		ZDummyForm Form2;
		void KillForms()
		{
			if (Form != null)
			{
				Form.Dispose();
			}

			if (Form2 != null)
			{
				Form2.Dispose();
			}
		}

		DummyFilterGridModule Module;
		void SetupModule()
		{
			Module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy);

			using (var moduleForm = new ZChildForm())
			{
				moduleForm.Text = "G'day!";
				moduleForm.Controls.Add(Module.EmbeddedControl);
				moduleForm.Show();
			}

			((IFilterGridModuleInternalsForTesting)Module).PerformSearch();

			AssertEquals("Loaded 2", 2, ZModuleResults.Instance.GetPKCollectionForModule(DummyModuleIDs.Dummy).Count);
		}

		DummyBusinessObject Dummy;
		DummyBusinessObject Dummy2;
		void SetupDummies()
		{
			Dummy = Factory.New<DummyBusinessObject>();
			Dummy.Z0_Code = "A";
			Dummy2 = Factory.New<DummyBusinessObject>();
			Dummy2.Z0_Code = "B";
			Factory.Save();
		}

		void WaitForClose(ZForm form)
		{
			if (Form.PreviousNextControlForTesting.closingThread != null)
			{
				Form.PreviousNextControlForTesting.closingThread.Join();
			}

			Application.DoEvents();
		}

		#endregion
	}
}
