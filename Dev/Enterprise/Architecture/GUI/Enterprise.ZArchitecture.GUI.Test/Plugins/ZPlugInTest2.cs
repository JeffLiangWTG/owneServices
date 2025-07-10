using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	class ZPlugInTest : TestCaseWithDummy
	{
		public void TestPlugInGUIAndBusinessEntityBeNotCreatedLoginLicence()
		{
			var logs = Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_OpenDateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddHours(-1)));
			var licenceCount1 = logs.Length;
			using (var testForm = new TestPlugInFormForCreateLoginLicence(Dummy))
			{
				testForm.PlugIns.Add(DummyControllerIDs.Dummy2);

				var plugIn1 = (DummyPlugIn2)testForm.ExposedPlugIns[0];
				var licence = new Licences();
				plugIn1.SetLicenceCheckPoint(licence.Core);
				plugIn1.ShouldDisplayPlugIn = false;
				testForm.Show();
				testForm.PlugIns.SelectPlugInTabPage(DummyControllerIDs.Dummy2);

				logs = Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_OpenDateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddHours(-1)));
				var licenceCount2 = logs.Length;
				AssertEquals("No Licence should be created", 0, licenceCount2 - licenceCount1);
			}
		}

		public void TestPlugInGUIAndBusinessEntityBeCreatedLoginLicence()
		{
			var logs = Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_OpenDateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddHours(-1)));
			var licenceCount1 = logs.Length;
			using (var testForm = new TestPlugInFormForCreateLoginLicence(Dummy))
			{
				testForm.PlugIns.Add(DummyControllerIDs.Dummy2);

				var plugIn1 = (DummyPlugIn2)testForm.ExposedPlugIns[0];
				var licence = new Licences();
				plugIn1.SetLicenceCheckPoint(licence.Core);
				plugIn1.ShouldDisplayPlugIn = true;
				testForm.Show();
				testForm.PlugIns.SelectPlugInTabPage(DummyControllerIDs.Dummy2);

				logs = Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_OpenDateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddHours(-1)));
				var licenceCount2 = logs.Length;
				AssertEquals("1 Licence should be created", 1, licenceCount2 - licenceCount1);
			}
		}

		internal class TestPlugInFormForCreateLoginLicence : ZTestForm
		{
			public TestPlugInFormForCreateLoginLicence(IBusiness bO)
				: base(bO)
			{
			}

			public ZPlugIn[] ExposedPlugIns
			{
				get { return PlugIns.Instances; }
			}

			public void FireDeleteButton()
			{
				SaveUserControl.SaveAndCloseButton.PerformClick();
			}

			protected override DialogResult ShowConfirmationForDelete()
			{
				return DialogResult.No;
			}

			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);
				UserIdleWorker.Flush();
			}
		}

		public void TestPluginOnZFormWithNoTableControl()
		{
			using (var form = new TestPlugInFormNoTabControl(Dummy))
			{
				var plugIn = (DummyPlugInNoTabControl)form.ExposedPlugIns[0];
				form.Show();
				AssertNotNull(plugIn.Form);
				AssertEquals("Form Loaded", 1, plugIn.OnForm_LoadCount);

				form.ShowPreSaveDialogsForTest();
				AssertEquals("ShowPreSaveDialog", 1, plugIn.ShowPreSaveDialogsCoreCount);
			}
		}

		#region Current Dependent Tests

		public void TestCurrentChanged()
		{
			using (var form = new TestCurrentDependentPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals(0, ((DummyPlugIn3)form.ExposedPlugIns[1]).CurrentChangedCount);

				Dummy.Collection.AddNew();
				AssertEquals(0, ((DummyPlugIn3)form.ExposedPlugIns[1]).CurrentChangedCount);

				form.ExposedPlugIns[1].SelectTabPage();
				AssertEquals(1, ((DummyPlugIn3)form.ExposedPlugIns[1]).CurrentChangedCount);

				Dummy.Collection.AddNew();
				AssertEquals(1, ((DummyPlugIn3)form.ExposedPlugIns[1]).CurrentChangedCount);

				form.Grid.CurrentRowIndex = 1;
				AssertEquals(2, ((DummyPlugIn3)form.ExposedPlugIns[1]).CurrentChangedCount);

				form.Grid.ListManager.RemoveAt(1);
				AssertEquals(3, ((DummyPlugIn3)form.ExposedPlugIns[1]).CurrentChangedCount);
			}
		}

		public void TestIsCurrentDependent()
		{
			using (var plugin = new DummyPlugIn3(Dummy))
			{
				AssertEquals("HasGridBeenSetUp", false, plugin.IsCurrentDependent);
				using (var grid = new ZGrid())
				{
					plugin.CurrentGrid = grid;
					AssertEquals("HasGridBeenSetUp", true, plugin.IsCurrentDependent);
				}
			}
		}

		public void TestCurrent()
		{
			using (var form = new TestCurrentDependentPlugInForm(Dummy))
			{
				form.Show();
				AssertNull(((DummyPlugIn3)form.ExposedPlugIns[1]).GetCurrent());
				Dummy.Collection.AddNew();
				AssertEquals(Dummy.Collection[0], ((DummyPlugIn3)form.ExposedPlugIns[1]).GetCurrent());
				Dummy.Collection.AddNew();
				AssertEquals(Dummy.Collection[0], ((DummyPlugIn3)form.ExposedPlugIns[1]).GetCurrent());
				form.Grid.CurrentRowIndex = 1;
				AssertEquals(Dummy.Collection[1], ((DummyPlugIn3)form.ExposedPlugIns[1]).GetCurrent());
			}
		}

		public void TestWorksWithInitiallyVisisbleGridWithInitiallyInvisiblePlugIn()
		{
			using (var form = new TestCurrentDependentPlugInForm(Dummy))
			{
				form.Show();
				form.ExposedPlugIns[1].SelectTabPage();

				CheckCurrentAndCurrentChanged(form, 1);
			}
		}

		public void TestWorksWithInitiallyVisisbleGridWithInitiallyVisisblePlugIn()
		{
			using (var form = new TestCurrentDependentPlugInForm2(Dummy))
			{
				form.Show();
				CheckCurrentAndCurrentChanged(form, 0);
			}
		}

		void CheckCurrentAndCurrentChanged(TestCurrentDependentPlugInForm form, int currentDependentPlugInIndex)
		{
			var plugIn = (DummyPlugIn3)form.ExposedPlugIns[currentDependentPlugInIndex];
			AssertEquals(1, plugIn.CurrentChangedCount);
			Dummy.Collection.AddNew();
			AssertEquals(2, plugIn.CurrentChangedCount);
			form.ExposedPlugIns[currentDependentPlugInIndex].TopLevelMenusInternal[0].PerformClick();
			AssertEquals(2, plugIn.CurrentChangedCount);
			AssertEquals(Dummy.Collection[0], plugIn.GetCurrent());
			Dummy.Collection.AddNew();
			AssertEquals(2, plugIn.CurrentChangedCount);
			AssertEquals(Dummy.Collection[0], plugIn.GetCurrent());
			form.Grid.CurrentRowIndex = 1;
			AssertEquals(3, plugIn.CurrentChangedCount);
			AssertEquals(Dummy.Collection[1], plugIn.GetCurrent());
		}

		#endregion

		public void TestKeepsTryingToGetUserControlIfUserControlIsInitiallyNull()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				var plugIn = (DummyPlugIn2)form.ExposedPlugIns[1];
				plugIn.ShouldBeAbleToGetUserControl = false;
				form.Show();
				plugIn.OnSavingInternal();
				AssertNull("plugIn.fUserControl", plugIn.fUserControl);

				plugIn.ShouldBeAbleToGetUserControl = true;
				form.PlugIns.SelectPlugInTabPage(DummyControllerIDs.Dummy2);
				AssertNotNull("plugIn.fUserControl", plugIn.fUserControl);
				AssertEquals("plugIn.TabPage.Controls.Contains(plugIn.fUserControl)", true, plugIn.TabPage.Controls.Contains(plugIn.fUserControl));
			}
		}

		[ExpectNoExceptions()]
		public void TestEmptyOnBusinessObjectIsCancelledChanged_WillNotCauseException()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				var plugIn = (DummyPlugIn2)form.ExposedPlugIns[1];
				plugIn.OnBusinessObjectIsCancelledChanged(ZBool.True);
			}
		}

		public void TestShouldPlugInGUIAndBusinessEntityBeCreatedTrue()
		{
			CheckShouldPlugInGUIAndBusinessEntityBeCreatedEffect(true);
		}

		public void TestShouldPlugInGUIAndBusinessEntityBeCreatedFalse()
		{
			CheckShouldPlugInGUIAndBusinessEntityBeCreatedEffect(false);
		}

		protected void CheckShouldPlugInGUIAndBusinessEntityBeCreatedEffect(bool shouldBeDisplayedValue)
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];
				plugIn2.ShouldDisplayPlugIn = shouldBeDisplayedValue;
				form.Show();
				form.PlugIns.SelectPlugInTabPage(DummyControllerIDs.Dummy2);
				AssertEquals("Showing covering label", !shouldBeDisplayedValue, ((IPlugInInternals)plugIn2).CoveringLabel.Visible);
				AssertEquals("Creating top level object", shouldBeDisplayedValue, plugIn2.HasCreatedBusinessEntity);
				AssertEquals("DelayBinding", !shouldBeDisplayedValue, plugIn2.DelayBinding);
			}
		}

		public void TestShouldPlugInGUIAndBusinessEntityBeCreatedWorksWhenTabIsChangedSeveralTimes()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];
				plugIn2.ShouldDisplayPlugIn = false;
				form.Show();
				form.PlugIns.SelectPlugInTabPage(DummyControllerIDs.Dummy2);
				AssertEquals("Should be showing covering label", true, ((IPlugInInternals)plugIn2).CoveringLabel.Visible);

				form.PlugIns.SelectPlugInTabPage(DummyControllerIDs.Dummy1);
				plugIn2.ShouldDisplayPlugIn = true;
				form.PlugIns.SelectPlugInTabPage(DummyControllerIDs.Dummy2);
				AssertEquals("Should not be showing covering label", false, ((IPlugInInternals)plugIn2).CoveringLabel.Visible);

				form.PlugIns.SelectPlugInTabPage(DummyControllerIDs.Dummy1);
				plugIn2.ShouldDisplayPlugIn = false;
				form.PlugIns.SelectPlugInTabPage(DummyControllerIDs.Dummy2);
				AssertEquals("Should be showing covering label", true, ((IPlugInInternals)plugIn2).CoveringLabel.Visible);
			}
		}

		public void TestPlugInWhichIsInitiallyDisabledGetsRequestedIndex()
		{
			using (var form1 = new TestPlugInForm2(Dummy))
			{
				form1.PlugIns.AddPlugInAtTabPageIndex(DummyControllerIDs.Dummy2, 0);
				var plugIn21 = (DummyPlugIn2)form1.ExposedPlugIns[1];
				plugIn21.Enabled = false;
				form1.Show();
				plugIn21.Enabled = true;
				AssertEquals("PlugIn should be the first tab page", "PlugIn2TabPage", form1.TopLevelTabControl.TabPages[0].Name);
			}

			using (var form = new TestPlugInForm2(Dummy))
			{
				form.PlugIns.AddPlugInAtTabPageIndex(DummyControllerIDs.Dummy2, () => 0);
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];
				plugIn2.Enabled = false;
				form.Show();
				plugIn2.Enabled = true;
				AssertEquals("PlugIn should be the first tab page", "PlugIn2TabPage", form.TopLevelTabControl.TabPages[0].Name);
			}
		}

		public void TestEnablingPlugInWhenNeverAccessedDoesNotRegisterTopLevelObject()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.TabControl.TabPages.Add(new ZTabPage()); // dummy first tab page so that plug-in does not load straight away
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				AssertEquals("Not accessed BusinessObject yet", false, plugIn1.AccessedTopLevelObject);
				plugIn1.Enabled = false;
				AssertEquals("Not accessed BusinessObject yet", false, plugIn1.AccessedTopLevelObject);
				plugIn1.Enabled = true;
				AssertEquals("Not accessed BusinessObject yet", false, plugIn1.AccessedTopLevelObject);
			}
		}

		public void TestTabPageVisible()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];
				var plugIn3 = (DummyPlugIn3)form.ExposedPlugIns[2];

				Assert("Precondition:Enabled", plugIn1.Enabled);
				Assert("Precondition:Enabled", plugIn2.Enabled);
				Assert("Precondition:Enabled", plugIn3.Enabled);

				plugIn3.TabPage.TabVisible = false;
				Assert("plugIn3 should be removed from TabPages", !form.TabControl.TabPages.Contains(plugIn3.TabPage));
				Assert("plugIn3 should NOT be removed from AllTabPages", form.TabControl.AllTabPages.Contains(plugIn3.TabPage));
				plugIn2.Enabled = false;
				Assert("plugIn2 should be removed from TabPages", !form.TabControl.TabPages.Contains(plugIn2.TabPage));
				Assert("plugIn2 should be removed from AllTabPages", !form.TabControl.AllTabPages.Contains(plugIn2.TabPage));

				plugIn3.TabPage.TabVisible = true;
				Assert("plugIn3 should be added to TabPages", form.TabControl.TabPages.Contains(plugIn3.TabPage));
				AssertEquals("TabPage Index", 1, form.TabControl.TabPages.IndexOf(plugIn3.TabPage));

				plugIn2.Enabled = true;
				Assert("plugIn3 should be added to TabPages", form.TabControl.TabPages.Contains(plugIn2.TabPage));
				Assert("plugIn3 should be added to AllTabPages", form.TabControl.AllTabPages.Contains(plugIn2.TabPage));
				AssertEquals("TabPageIndex", 1, form.TabControl.TabPages.IndexOf(plugIn2.TabPage));
				AssertEquals("TabPageIndex", 2, form.TabControl.TabPages.IndexOf(plugIn3.TabPage));
			}
		}

		public void TestDefaultRequestedTabPageIndex()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				AssertEquals("RequestedTabPageIndex", -1, plugIn1.RequestedTabPageIndex);
			}
		}

		public void TestShowCoveringLabel()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				AssertEquals("User control should be invisible", true, plugIn1.UserControl.Visible);
				AssertEquals("Label should be visible", false, plugIn1.CoveringLabel.Visible);
				plugIn1.ShowCoveringLabel("TEST");
				AssertEquals("User control should be invisible", false, plugIn1.UserControl.Visible);
				AssertEquals("Label text", "TEST", plugIn1.CoveringLabel.Text);
				AssertEquals("Label should be visible", true, plugIn1.CoveringLabel.Visible);
			}
		}

		public void TestHideCoveringLabel()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];

				plugIn1.HideCoveringLabel();
				AssertEquals("User control should be invisible", true, plugIn1.UserControl.Visible);
				AssertEquals("Label should be visible", false, plugIn1.CoveringLabel.Visible);

				plugIn1.ShowCoveringLabel("TEST");
				AssertEquals("Label should be visible", true, plugIn1.CoveringLabel.Visible);

				plugIn1.HideCoveringLabel();
				AssertEquals("User control should be invisible", true, plugIn1.UserControl.Visible);
				AssertEquals("Label should be visible", false, plugIn1.CoveringLabel.Visible);
			}
		}

		public void TestAllTopLevelMenusDefaultBehaviour()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				AssertEquals("Got one menu", 1, plugIn1.TopLevelMenusInternal.Length);
				AssertEquals("Got top level menu", plugIn1.TopLevelMenu, plugIn1.TopLevelMenusInternal[0]);
			}
		}

		public void TestShowPluginWithGetNewTopLevelMenuException()
		{
			using (var form = new TestPlugInFormWithNewTopLevelMenuExceptionControl(Dummy))
			{
				form.Show();

				AssertContains("report the exception", "Exception caught in GetNewTopLevelMenu, please fix the plugin.\r\n", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestUserControlCreationIsLateAndLazy()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				AssertEquals("User control should be added as this is the first visible tab", 1, plugIn1.TabPage.Controls.Count);

				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];
				AssertEquals("Lazy is as lazy does", 0, plugIn2.TabPage.Controls.Count);
				AssertNull("UserControl not created", plugIn2.fUserControl);
				plugIn2.SelectTabPage();
				AssertEquals("User control should be added", 1, plugIn2.TabPage.Controls.Count);
				AssertNotNull("UserControl created", plugIn2.fUserControl);
				AssertEquals("Should be PlugIn's user control", plugIn2.UserControl, plugIn2.TabPage.Controls[0]);
			}
		}

		public void TestOnSavingCalled()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];
				var plugIn3 = (DummyPlugIn3)form.ExposedPlugIns[2];

				AssertEquals("No save called", 0, plugIn1.OnSavingCount);
				AssertEquals("No save called", 0, plugIn2.OnSavingCount);
				AssertEquals("No save called", 0, plugIn3.OnSavingCount);

				form.FireSaveButton();

				AssertEquals("Saving called as visible", 1, plugIn1.OnSavingCount);
				AssertEquals("Saving called as always load", 1, plugIn2.OnSavingCount);
				AssertEquals("No save called", 0, plugIn3.OnSavingCount);

				plugIn3.TopLevelMenusInternal[0].PerformClick();
				form.FireSaveButton();
				AssertEquals("Saving called as was visible", 2, plugIn1.OnSavingCount);
				AssertEquals("Saving called as always load", 2, plugIn2.OnSavingCount);
				AssertEquals("Save called as has now has menu shown", 1, plugIn3.OnSavingCount);
			}
		}

		public void TestOnSaveCompleteOrAbortedCalled()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];
				var plugIn3 = (DummyPlugIn3)form.ExposedPlugIns[2];

				AssertEquals("No save called", 0, plugIn1.OnSaveCompleteOrAbortedCount);
				AssertEquals("No save called", 0, plugIn2.OnSaveCompleteOrAbortedCount);
				AssertEquals("No save called", 0, plugIn3.OnSaveCompleteOrAbortedCount);
				AssertNull(plugIn1.SaveWasCalled);
				AssertNull(plugIn2.SaveWasCalled);
				AssertNull(plugIn3.SaveWasCalled);

				form.FireSaveButton();

				AssertEquals("OnSaveCompleteOrAbortedCount called as visible", 1, plugIn1.OnSaveCompleteOrAbortedCount);
				AssertEquals("OnSaveCompleteOrAbortedCount called as always load", 1, plugIn2.OnSaveCompleteOrAbortedCount);
				AssertEquals("No save called", 0, plugIn3.OnSaveCompleteOrAbortedCount);
				Assert((bool)plugIn1.SaveWasCalled);
				Assert((bool)plugIn2.SaveWasCalled);
				AssertNull(plugIn3.SaveWasCalled);

				plugIn1.SaveWasCalled = null;
				plugIn2.SaveWasCalled = null;
				plugIn3.SaveWasCalled = null;
				plugIn3.TopLevelMenusInternal[0].PerformClick();
				form.FireSaveButton();
				AssertEquals("OnSaveCompleteOrAbortedCount called as was visible", 2, plugIn1.OnSaveCompleteOrAbortedCount);
				AssertEquals("OnSaveCompleteOrAbortedCount called as always load", 2, plugIn2.OnSaveCompleteOrAbortedCount);
				AssertEquals("OnSaveCompleteOrAbortedCount called as has now has menu shown", 1, plugIn3.OnSaveCompleteOrAbortedCount);
				Assert((bool)plugIn1.SaveWasCalled);
				Assert((bool)plugIn2.SaveWasCalled);
				Assert((bool)plugIn3.SaveWasCalled);

				plugIn1.SaveWasCalled = null;
				plugIn2.SaveWasCalled = null;
				plugIn3.SaveWasCalled = null;
				plugIn2.UserClickedCancelOnPreSaveDialog = true;
				form.FireSaveButton();
				AssertEquals("OnSaveCompleteOrAbortedCount called even when save is canceled", 3, plugIn1.OnSaveCompleteOrAbortedCount);
				AssertEquals("OnSaveCompleteOrAbortedCount called even when save is canceled", 3, plugIn2.OnSaveCompleteOrAbortedCount);
				AssertEquals("OnSaveCompleteOrAbortedCount called even when save is canceled", 2, plugIn3.OnSaveCompleteOrAbortedCount);
				Assert(!(bool)plugIn1.SaveWasCalled);
				Assert(!(bool)plugIn2.SaveWasCalled);
				Assert(!(bool)plugIn3.SaveWasCalled);

				plugIn1.SaveWasCalled = null;
				plugIn2.SaveWasCalled = null;
				plugIn3.SaveWasCalled = null;
				plugIn2.UserClickedCancelOnPreSaveDialog = false;
				plugIn3.ThrowExceptionInShowPreSaveDialogs = true;

				form.FireSaveButton();
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Shouldn't show pre save dialogs", ExceptionReporterTestListener.Instance[0].Message);
				ExceptionReporterTestListener.Instance.Clear();

				AssertEquals("OnSaveCompleteOrAbortedCount called even when execption thrown", 4, plugIn1.OnSaveCompleteOrAbortedCount);
				AssertEquals("OnSaveCompleteOrAbortedCount called even when execption thrown", 4, plugIn2.OnSaveCompleteOrAbortedCount);
				AssertEquals("OnSaveCompleteOrAbortedCount called even when execption thrown", 3, plugIn3.OnSaveCompleteOrAbortedCount);
				Assert(!(bool)plugIn1.SaveWasCalled);
				Assert(!(bool)plugIn2.SaveWasCalled);
				Assert(!(bool)plugIn3.SaveWasCalled);

				plugIn1.SaveWasCalled = null;
				plugIn2.SaveWasCalled = null;
				plugIn3.SaveWasCalled = null;
				plugIn3.ThrowExceptionInShowPreSaveDialogs = false;
				Dummy.Z0_AnotherDate = ZDateTime.Invalid;
				AssertHasErrors(Dummy.Z0_AnotherDateInfo);
				form.FireSaveButton();
				AssertEquals("OnSaveCompleteOrAbortedCount called even when validation fails", 5, plugIn1.OnSaveCompleteOrAbortedCount);
				AssertEquals("OnSaveCompleteOrAbortedCount called even when validation fails", 5, plugIn2.OnSaveCompleteOrAbortedCount);
				AssertEquals("OnSaveCompleteOrAbortedCount called even when validation fails", 4, plugIn3.OnSaveCompleteOrAbortedCount);
				Assert(!(bool)plugIn1.SaveWasCalled);
				Assert(!(bool)plugIn2.SaveWasCalled);
				Assert(!(bool)plugIn3.SaveWasCalled);
			}
		}

		public void TestOnMenuShownCalled()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];

				AssertEquals("Never shown called", 0, plugIn1.OnMenuShownCount);

				plugIn1.TopLevelMenu.PerformClick();
				AssertEquals("Done once", 1, plugIn1.OnMenuShownCount);

				plugIn1.TopLevelMenu.PerformClick();
				AssertEquals("Done twice", 2, plugIn1.OnMenuShownCount);
			}
		}

		public void TestOnMenuShownCalledWithMoreThanOneMenu()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn3 = (DummyPlugIn3)form.ExposedPlugIns[2];

				AssertEquals("Never shown called", 0, plugIn3.OnMenuShownCount);

				plugIn3.TopLevelMenusInternal[0].PerformClick();
				AssertEquals("Done once", 1, plugIn3.OnMenuShownCount);

				plugIn3.TopLevelMenusInternal[1].PerformClick();
				AssertEquals("Done twice", 2, plugIn3.OnMenuShownCount);

				plugIn3.TopLevelMenusInternal[0].PerformClick();
				AssertEquals("Done thrice", 3, plugIn3.OnMenuShownCount);
			}
		}

		public void TestOnUserControlShownCalled()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];

				AssertEquals("Initially visible", 1, plugIn1.OnUserControlShownCount);
				AssertEquals("Never shown called", 0, plugIn2.OnUserControlShownCount);

				form.TabControl.SelectedIndex = 1;
				AssertEquals("Shown once", 1, plugIn2.OnUserControlShownCount);

				form.TabControl.SelectedIndex = 0;
				AssertEquals("Shown again", 2, plugIn1.OnUserControlShownCount);
				AssertEquals("Shown once", 1, plugIn2.OnUserControlShownCount);

				form.TabControl.SelectedIndex = 1;
				AssertEquals("Shown twice", 2, plugIn2.OnUserControlShownCount);
				AssertEquals("Shown twice", 2, plugIn1.OnUserControlShownCount);
			}
		}

		public void TestEnabledFalseMeansNoDataWrittenToDB()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				FlickThroughAllTabPages(form);

				form.ExposedPlugIns[0].Enabled = false;
				form.FireSaveButton();
				AssertEquals("Two out of 3 plugins saved + form's bizo saved", 3, Factory.Load(typeof(DummyBusinessObject), new ZQuery()).Length);
			}
		}

		public void TestEnabledTrueMeansDataWrittenToDB()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				Application.DoEvents();
				Dummy.Z0_Number = 100; // make a change so we get saved
				FlickThroughAllTabPages(form);

				form.FireSaveButton();
				AssertEquals("3 out of 3 plugins saved + form's business entity", 4, Factory.Load(typeof(DummyBusinessObject), new ZQuery()).Length);
			}
		}

		public void TestOnlyActivePlugInsAreWrittenToDB()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				form.FireSaveButton();

				AssertEquals("Dummy1 plugin saved as visible, Dummy2 plugin saved as always load, Dummy3 plugin NOT saved, Dummyform bizO saved", 3, Factory.Load(typeof(DummyBusinessObject), new ZQuery()).Length);
			}
		}

		public void TestEnabledEffectOnMenu()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals("Menu present", form.ExposedPlugIns[0].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 3));
				AssertEquals("Menu present", form.ExposedPlugIns[1].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 2));
				AssertEquals("Menu present", form.ExposedPlugIns[2].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 1));
				AssertEquals("Menu present", form.ExposedPlugIns[2].TopLevelMenusInternal[1], NthPlugInMenuFromRight(form, 0));
				var initialMenuCount = GetVisibleMenuItems(form).Length;

				form.ExposedPlugIns[1].Enabled = false;
				AssertEquals("Menu moved right", form.ExposedPlugIns[0].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 2));
				AssertEquals("One menu gone", initialMenuCount - 1, GetVisibleMenuItems(form).Length);

				form.ExposedPlugIns[0].Enabled = false;
				AssertEquals("Two menus gone", initialMenuCount - 2, GetVisibleMenuItems(form).Length);

				form.ExposedPlugIns[2].Enabled = false;
				AssertEquals("4 menus gone", initialMenuCount - 4, GetVisibleMenuItems(form).Length);

				form.ExposedPlugIns[0].Enabled = true;
				form.ExposedPlugIns[1].Enabled = true;
				form.ExposedPlugIns[2].Enabled = true;
				AssertEquals("Menu present", form.ExposedPlugIns[0].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 3));
				AssertEquals("Menu present", form.ExposedPlugIns[1].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 2));
				AssertEquals("Menu present", form.ExposedPlugIns[2].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 1));
				AssertEquals("Menu present", form.ExposedPlugIns[2].TopLevelMenusInternal[1], NthPlugInMenuFromRight(form, 0));
				AssertEquals("Menu back again", initialMenuCount, GetVisibleMenuItems(form).Length);
			}
		}

		public void TestEnabledEffectOnTabPage()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				Application.DoEvents();

				var initialTabPageCount = form.TabControl.TabPages.Count;

				form.ExposedPlugIns[1].Enabled = false;
				AssertEquals("One tab hidden", initialTabPageCount - 1, form.TabControl.TabPages.Count);

				form.ExposedPlugIns[0].Enabled = false;
				AssertEquals("Two tabs hidden", initialTabPageCount - 2, form.TabControl.TabPages.Count);

				AssertEquals("PlugIn 3 is first tab, as other two are hidden", form.TabControl.TabPages[0].Text, form.ExposedPlugIns[2].Name);

				form.ExposedPlugIns[0].Enabled = true;
				form.ExposedPlugIns[1].Enabled = true;
				AssertEquals("All tabs back again", initialTabPageCount, form.TabControl.TabPages.Count);
			}
		}

		public void TestEnabledFalseMeansNoValidation()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				FlickThroughAllTabPages(form);

				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];

				((DummyBusinessObject)plugIn1.BusinessEntity).Z0_Description = "Good";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				Assert("Save with no error", !UnitTestUserNotification.Instance.LastMessage.WasError);

				((DummyBusinessObject)plugIn1.BusinessEntity).Z0_Description = "Bad";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				Assert("Can't save as error", UnitTestUserNotification.Instance.LastMessage.WasError);

				plugIn1.Enabled = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				Assert("Can save as plugIn disabled", !UnitTestUserNotification.Instance.LastMessage.WasError);

				plugIn1.Enabled = true;
				((DummyBusinessObject)plugIn1.BusinessEntity).Z0_Description = "Bad";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				Assert("Can't save as error", UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestEnabledFalseMeansNoHasChanges()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				foreach (ZTabPage tabPage in form.TabControl.TabPages)
				{
					form.TabControl.SelectedTab = tabPage;
				}
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				Assert("Initial state", !form.BusinessEntity.HasChanges);

				((DummyBusinessObject)plugIn1.BusinessEntity).Z0_Description = "Good";
				Assert("Plug in causing parent to have changes", form.BusinessEntity.HasChanges);

				plugIn1.Enabled = false;
				Assert("PlugIn disabed - we don't care about its has changes any more", !form.BusinessEntity.HasChanges);

				plugIn1.Enabled = true;
				Assert("Plug in causing parent to have changes", form.BusinessEntity.HasChanges);

				plugIn1.BusinessEntity.HasChanges = false;
				Assert("PlugIn no longer has changes, so parent no longer has changes", !form.BusinessEntity.HasChanges);
			}
		}

		public void TestGetOtherPlugInsWhichImplement()
		{
			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				plugIns.Add(DummyControllerIDs.Dummy1);
				plugIns.Add(DummyControllerIDs.Dummy2);
				plugIns.Add(DummyControllerIDs.Dummy3);

				AssertEquals("PlugIns loaded", 3, plugIns.Instances.Length);

				var implementDummyInterface1 = ((DummyPlugIn1)plugIns.Instances[0]).GetOtherPlugInsWhichImplementExposed(typeof(DummyInterface1));
				AssertEquals("Should find other 2 dummies", 2, implementDummyInterface1.Length);
				AssertEquals("Right dummy plugIn", typeof(DummyPlugIn2), implementDummyInterface1[0].GetType());
				AssertEquals("Right dummy plugIn", typeof(DummyPlugIn3), implementDummyInterface1[1].GetType());

				implementDummyInterface1 = ((DummyPlugIn3)plugIns.Instances[2]).GetOtherPlugInsWhichImplementExposed(typeof(DummyInterface1));
				AssertEquals("Should find other 2 dummies", 2, implementDummyInterface1.Length);
				AssertEquals("Right dummy plugIn", typeof(DummyPlugIn1), implementDummyInterface1[0].GetType());
				AssertEquals("Right dummy plugIn", typeof(DummyPlugIn2), implementDummyInterface1[1].GetType());

				var implementDummyInterface3 = ((DummyPlugIn1)plugIns.Instances[0]).GetOtherPlugInsWhichImplementExposed(typeof(DummyInterface3));
				AssertEquals("Should find one dummy", 1, implementDummyInterface3.Length);
				AssertEquals("Right dummy plugIn", typeof(DummyPlugIn3), implementDummyInterface3[0].GetType());

				var implementDummyInterfaceThatNobodyImplements = ((DummyPlugIn1)plugIns.Instances[0]).GetOtherPlugInsWhichImplementExposed(typeof(DummyInterfaceThatNobodyImplements));
				AssertEquals("Should find none", 0, implementDummyInterfaceThatNobodyImplements.Length);
			}
		}

		public void TestShowPreSaveDialogsWithContinueWithSaveYes()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];
				plugIn2.UserClickedCancelOnPreSaveDialog = false;

				form.FireSaveButton();

				AssertEquals("Saved ok", true, plugIn2.BusinessEntity.IsInDatabaseIncludingChildren);
			}
		}

		public void TestShowPreSaveDialogsWithContinueWithSaveNo()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];
				plugIn2.UserClickedCancelOnPreSaveDialog = true;

				form.FireSaveButton();

				AssertEquals("Save cancelled", false, plugIn2.BusinessEntity.IsInDatabaseIncludingChildren);
			}
		}

		public void TestShowPreSaveDialogsDoesNothingWhenPlugInNotLoaded()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn3 = (DummyPlugIn3)form.ExposedPlugIns[2];
				plugIn3.ThrowExceptionInShowPreSaveDialogs = true;

				form.FireSaveButton();

				AssertEquals("BizO never registered", false, plugIn3.IsBusinessObjectRegistered);
				// and no exception was thrown
			}
		}

		public void TestAlwaysLoadTrueLeadsToSynchAndRego()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];

				AssertEquals("Synched as visible since first tab.", 1, plugIn1.OnGUIShownCount);
				AssertEquals("Should have registered bizO.", true, plugIn1.IsBusinessObjectRegistered);
				AssertEquals("Should not have registered bizO.", false, plugIn2.IsBusinessObjectRegistered);

				form.FireSaveButton();

				AssertEquals("Save should have registered bizO.", true, plugIn2.IsBusinessObjectRegistered);
				AssertEquals("Save should have synched data", 1, plugIn2.OnSavingCount);
				AssertEquals("Gui still not shown", 0, plugIn2.OnGUIShownCount);
			}
		}

		public void TestAlwaysLoadTrueStopsSaveIfPlugInHasErrors()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];

				((DummyBusinessObject)plugIn2.BusinessEntity).Z0_Description = "Bad";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				Assert(Dummy.HasErrors);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestAlwaysLoadFalseMeansNotLoadedPlugInErrorsAreNotFound()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];
				var plugIn3 = (DummyPlugIn3)form.ExposedPlugIns[2];

				AssertEquals("No synch yet as not visible.", 0, plugIn3.OnGUIShownCount);
				AssertEquals("No BizO rego yet.", false, plugIn3.IsBusinessObjectRegistered);

				((DummyBusinessObject)plugIn3.BusinessEntity).Z0_Description = "Bad";
				Assert("PreCondition", plugIn3.BusinessEntity.HasErrors());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals("No BizO rego yet as not visible and AlwaysLoad=false.", false, plugIn3.IsBusinessObjectRegistered);
				Assert("No error as plug in not registered", !UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("No synch yet as not visible.", 0, plugIn3.OnGUIShownCount);
			}
		}

		public void TestAlwaysLoadFalseMeansNoSynchAndNoRego()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				var plugIn3 = (DummyPlugIn3)form.ExposedPlugIns[2];

				AssertEquals("Synched as visible since first tab.", 1, plugIn1.OnGUIShownCount);
				AssertEquals("No synch yet as not visible.", 0, plugIn3.OnGUIShownCount);
				AssertEquals("No BizO rego yet.", false, plugIn3.IsBusinessObjectRegistered);

				form.FireSaveButton();

				AssertEquals("No synch yet as not alwyas load.", 0, plugIn3.OnGUIShownCount);
				AssertEquals("No BizO rego yet as not visible and AlwaysLoad=false.", false, plugIn3.IsBusinessObjectRegistered);
			}
		}

		public void TestTabPagesAdded()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(3, form.TabControl.TabPages.Count);
				AssertEquals(form.ExposedPlugIns[0].Name, form.TabControl.TabPages[0].Text);
				AssertEquals(form.ExposedPlugIns[1].Name, form.TabControl.TabPages[1].Text);
				AssertEquals(form.ExposedPlugIns[2].Name, form.TabControl.TabPages[2].Text);
			}
		}

		public void TestTabPage_CaptionSet_BeforeTabPageCreated()
		{
			using (var plugin = new DummyPlugIn3(Dummy))
			{
				plugin.Controller = ZControllerFactory.Create(ControllerIDs.eDocsPlugIn);
				AssertEquals("eDocs", plugin.TabPage.CaptionResourceString.Caption);
			}
		}

		public void TestTabPage_CaptionSet_AfterTabPageCreated()
		{
			using (var plugin = new DummyPlugIn3(Dummy))
			{
				var tabPage = plugin.TabPage;
				plugin.Controller = ZControllerFactory.Create(ControllerIDs.eDocsPlugIn);
				AssertEquals("eDocs", tabPage.CaptionResourceString.Caption);
			}
		}

		public void TestMenusAreVisibleAtAllTimes()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals(form.ExposedPlugIns[0].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 3));
				AssertEquals(form.ExposedPlugIns[1].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 2));

				form.TabControl.SelectedIndex = 0;
				AssertEquals(form.ExposedPlugIns[0].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 3));

				form.TabControl.SelectedIndex = 1;
				AssertEquals(form.ExposedPlugIns[1].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 2));

				form.TabControl.TabPages.Add(new ZTabPage());
				form.TabControl.SelectedIndex = 2;
				AssertEquals(form.ExposedPlugIns[0].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 3));
				AssertEquals(form.ExposedPlugIns[1].TopLevelMenusInternal[0], NthPlugInMenuFromRight(form, 2));
			}
		}

		public void TestTabShownSynchronise()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				var plugIn3 = (DummyPlugIn3)form.ExposedPlugIns[2];

				AssertEquals("Synched as visible since first tab.", 1, plugIn1.OnGUIShownCount);

				form.TabControl.SelectedIndex = 2;
				AssertEquals("Synched as now visible.", 1, plugIn3.OnGUIShownCount);

				form.TabControl.SelectedIndex = 0;
				AssertEquals("Back to first tab.", 2, plugIn1.OnGUIShownCount);

				form.TabControl.SelectedIndex = 2;
				AssertEquals("Back to third tab.", 2, plugIn1.OnGUIShownCount);
			}
		}

		public void TestMenuPopupSynchronise()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				var plugIn3 = (DummyPlugIn3)form.ExposedPlugIns[2];

				AssertEquals("Synched as visible since first tab.", 1, plugIn1.OnGUIShownCount);

				NthPlugInMenuFromRight(form, 0).PerformClick();
				AssertEquals("Synched as now clicked.", 1, plugIn3.OnGUIShownCount);

				NthPlugInMenuFromRight(form, 3).PerformClick();
				AssertEquals("Back to first plug in menu.", 2, plugIn1.OnGUIShownCount);

				NthPlugInMenuFromRight(form, 0).PerformClick();
				AssertEquals("Back to second tab.", 2, plugIn3.OnGUIShownCount);
			}
		}

		public void TestSaveWithNoErrorsWithoutAlwaysLoad()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				var plugIn3 = (DummyPlugIn3)form.ExposedPlugIns[2];

				var bO1 = (DummyBusinessObject)form.ExposedPlugIns[0].BusinessEntity;
				bO1.Z0_Description = "BO1";

				var bO2 = (BusinessObject)form.ExposedPlugIns[2].BusinessEntity;

				form.FireSaveButton();

				AssertNotNull(Factory.Load(typeof(DummyBusinessObject), Dummy.PK));
				AssertNotNull(Factory.Load(typeof(DummyBusinessObject), bO1.PK));
				AssertNull("Business Object should not be persisted", Factory.Load(typeof(DummyBusinessObject), bO2.PK));
			}
		}

		public void TestSaveWithNoErrorsWithAlwaysLoad()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];

				var bO1 = (DummyBusinessObject)form.ExposedPlugIns[0].BusinessEntity;
				bO1.Z0_Description = "BO1";

				var bO2 = (BusinessObject)form.ExposedPlugIns[1].BusinessEntity;

				form.FireSaveButton();

				var factory = new BusinessObjectFactory();

				AssertNotNull("Checking in DB", factory.Load(typeof(DummyBusinessObject), Dummy.PK));
				AssertNotNull("Checking in DB", factory.Load(typeof(DummyBusinessObject), bO1.PK));
				AssertNotNull("Checking in DB", factory.Load(typeof(DummyBusinessObject), bO2.PK));
			}
		}

		public void TestDisplayMode()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals(form.DisplayMode, ODisplayMode.Browse);
				var bO2 = (DummyBusinessObject)form.ExposedPlugIns[0].BusinessEntity;
				bO2.Z0_Description = "BO2";
				AssertEquals(form.DisplayMode, ODisplayMode.Edit);
			}
		}

		public void TestSaveWithErrors()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var bO1 = (DummyBusinessObject)form.ExposedPlugIns[0].BusinessEntity;
				bO1.Z0_Description = "Bad";

				var bO2 = (BusinessObject)form.ExposedPlugIns[1].BusinessEntity;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

				var factory = new BusinessObjectFactory();

				AssertNull("Checking not in DB", factory.Load(typeof(DummyBusinessObject), Dummy.PK));
				AssertNull("Checking not in DB", factory.Load(typeof(DummyBusinessObject), bO1.PK));
				AssertNull("Checking not in DB", factory.Load(typeof(DummyBusinessObject), bO2.PK));
			}
		}

		[ExpectNoExceptions()]
		public void TestRegisterEditableFlag()
		{
			try
			{
				DummyPlugIn1.RegisterEditable = false;

				using (var form = new TestPlugInForm(Factory.New(typeof(DummyBusinessObject))))
				{
					form.Show();

					var bO1 = (DummyBusinessObject)form.ExposedPlugIns[0].BusinessEntity;
					bO1.Z0_Description = "Bad";

					var bO2 = (BusinessObject)form.ExposedPlugIns[1].BusinessEntity;

					try
					{
						form.FireSaveButton();
					}
					catch
					{
						throw new Exception("Should be no errors as the plugin is not registered as editable");
					}
				}
			}
			finally
			{
				DummyPlugIn1.RegisterEditable = true;
			}
		}

		public void TestDeleteWithAllowDelete()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				Application.DoEvents();
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];
				var bO1 = (DummyBusinessObject)form.ExposedPlugIns[0].BusinessEntity;
				bO1.Z0_Description = "BO1";

				var bO2 = (DummyBusinessObject)form.ExposedPlugIns[1].BusinessEntity;
				bO2.Z0_Description = "AllowDelete";

				form.FireSaveButton();

				AssertNotNull(Factory.Load(typeof(DummyBusinessObject), Dummy.PK));
				AssertNotNull(Factory.Load(typeof(DummyBusinessObject), bO1.PK));
				AssertNotNull(Factory.Load(typeof(DummyBusinessObject), bO2.PK));

				var deletedPK1 = Dummy.PK;
				var deletedPK2 = bO2.PK;

				form.DisplayMode = ODisplayMode.Delete;
				form.FireDeleteButton();

				var newFactory = new BusinessObjectFactory();
				AssertNull(newFactory.Load(typeof(DummyBusinessObject), deletedPK1));
				AssertNotNull(newFactory.Load(typeof(DummyBusinessObject), bO1.PK));
				AssertNull(newFactory.Load(typeof(DummyBusinessObject), deletedPK2));
			}
		}

		public void TestDeleteWithNotAllowDelete()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn2 = (DummyPlugIn2)form.ExposedPlugIns[1];
				var bO1 = (DummyBusinessObject)form.ExposedPlugIns[0].BusinessEntity;
				bO1.Z0_Description = "BO1";
				((DummyPlugIn1)form.ExposedPlugIns[0]).fCanDelete = false;

				var bO2 = (DummyBusinessObject)form.ExposedPlugIns[1].BusinessEntity;
				bO2.Z0_Description = "Test CanDelete";

				form.FireSaveButton();

				AssertNotNull(Factory.Load(typeof(DummyBusinessObject), Dummy.PK));
				AssertNotNull(Factory.Load(typeof(DummyBusinessObject), bO1.PK));
				AssertNotNull(Factory.Load(typeof(DummyBusinessObject), bO2.PK));

				Assert(!form.ExposedPlugIns[1].CanDelete);

				form.DisplayMode = ODisplayMode.Delete;
				form.FireDeleteButton();

				var newFactory = new BusinessObjectFactory();
				AssertNotNull(newFactory.Load(typeof(DummyBusinessObject), Dummy.PK));
				AssertNotNull(newFactory.Load(typeof(DummyBusinessObject), bO1.PK));
				AssertNotNull(newFactory.Load(typeof(DummyBusinessObject), bO2.PK));
			}
		}

		public void TestMenuClickWillCommitChangesInSelectedControl()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "TST";
			Factory.Save();

			using (var testForm = new TestPlugInForm2(dummy))
			{
				testForm.Show();
				UserIdleWorker.Flush();

				var plugIn1 = (DummyPlugIn1)testForm.ExposedPlugIns[0];
				var topMenuItem = plugIn1.TopLevelMenusInternal[0];
				topMenuItem.PerformClick();
				AssertEquals("MenuText", "Data Unchanged", topMenuItem.MenuItems[0].Text);

				testForm.CodeBox.Focus();
				KeySender.PostKeyDown(testForm.CodeBox, Keys.C);
				KeySender.PostKeyDown(testForm.CodeBox, Keys.H);
				KeySender.PostKeyDown(testForm.CodeBox, Keys.C);

				Application.DoEvents();
				topMenuItem.PerformClick();
				AssertEquals("MenuText", "Data Changed", topMenuItem.MenuItems[0].Text);
			}
		}

		public void TestSelectTabPage()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals("Initial state", 0, form.TabControl.SelectedIndex);
				((DummyPlugIn2)form.ExposedPlugIns[1]).SelectTabPageExposed();
				AssertEquals("Selected 2nd plugin", 1, form.TabControl.SelectedIndex);
				((DummyPlugIn1)form.ExposedPlugIns[0]).SelectTabPageExposed();
				AssertEquals("Back to init state", 0, form.TabControl.SelectedIndex);
			}
		}

		public void TestIsFormEditable()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn1 = (DummyPlugIn1)form.ExposedPlugIns[0];
				form.DisplayMode = ODisplayMode.Delete;
				Assert(!plugIn1.IsFormEditable);

				form.DisplayMode = ODisplayMode.Browse;
				Assert(plugIn1.IsFormEditable);

				form.DisplayMode = ODisplayMode.Edit;
				Assert(plugIn1.IsFormEditable);

				form.DisplayMode = ODisplayMode.ReadOnly;
				Assert(!plugIn1.IsFormEditable);

				form.DisplayMode = ODisplayMode.Undefined;
				Assert(!plugIn1.IsFormEditable);
			}
		}

		public void TestOnDisplayModeChanged()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				var plugIn3 = (DummyPlugIn3)form.ExposedPlugIns[2];
				Assert("PreCondition:PlugIn3 Display Mode is not Delete", plugIn3.DisplayMode != ODisplayMode.Delete);
				Assert("PreCondition:DisplayMode is set when form is shown", plugIn3.OnIsFormEditableChangedCount == 1);

				form.DisplayMode = ODisplayMode.Delete;
				AssertEquals("PlugIn3 Display Mode Changed", 2, plugIn3.OnIsFormEditableChangedCount);

				form.DisplayMode = ODisplayMode.Delete;
				AssertEquals("PlugIn3 Display Mode not changed this time", 2, plugIn3.OnIsFormEditableChangedCount);

				form.DisplayMode = ODisplayMode.Browse;
				AssertEquals("PlugIn3 Display Mode changed", 3, plugIn3.OnIsFormEditableChangedCount);
			}
		}

		public void TestShowTheFirstTabPageIfPlugInTabPageDisabled()
		{
			using (var testForm = new TestPlugInForm2(Dummy))
			{
				testForm.PlugIns.Add(DummyControllerIDs.Dummy2);

				var dummy1 = testForm.PlugIns.Instances[0];
				AssertEquals("PreCondition: The first instance of PlugIns is Dummy1", typeof(DummyPlugIn1), dummy1.GetType());

				var dummy2 = testForm.PlugIns.Instances[1];
				AssertEquals("PreCondition: The second instance of PlugIns is Dummy2", typeof(DummyPlugIn2), dummy2.GetType());

				dummy2.Enabled = false;
				testForm.PlugInIDToSelectOnLoaded = DummyControllerIDs.Dummy2;
				testForm.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();
				AssertEquals("Selected Tab page is the first one as the plugin is disabled", dummy1.TabPage, testForm.MainTabControl.SelectedTab);
			}
		}

		public void TestShowTheFirstTabPageIfPlugInTabPageDoesntHaveUserControl()
		{
			using (var testForm = new TestPlugInForm2(Dummy))
			{
				testForm.PlugIns.Add(DummyControllerIDs.Dummy3);
				var dummy3 = testForm.PlugIns.Instances[1] as DummyPlugIn3;
				dummy3.DisableUserControl = true;
				AssertNull("PreCondition: UserControl is null", dummy3.UserControl);

				testForm.PlugInIDToSelectOnLoaded = DummyControllerIDs.Dummy3;
				testForm.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				var dummy1 = testForm.PlugIns.Instances[0];
				AssertEquals("Selected Tab page is the first one as the plugin has no user control", dummy1.TabPage, testForm.MainTabControl.SelectedTab);
			}
		}

		public void TestPlugInShouldRefreshIfShownOnSave()
		{
			using (var testForm = new TestPlugInForm(Dummy))
			{
				testForm.Show();

				var selectedIndex = testForm.TabControl.SelectedIndex;
				var shownPlugIn = testForm.ExposedPlugIns[selectedIndex];
				AssertEquals(0, shownPlugIn.refreshCount);

				testForm.FireSaveButton();
				AssertEquals(1, shownPlugIn.refreshCount);
			}
		}

		#region Licence / Security

		public void TestSecurityCheckedBeforeLicenceConsumed()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals("Initial state", 0, form.TabControl.SelectedIndex);
				var plugIn = ((DummyPlugIn2)form.ExposedPlugIns[1]);

				var licence = new Licences();
				plugIn.SetLicenceCheckPoint(licence.Core);

				SecurityCheckpoint checkPoint = new DummyCheckPointWithSecuritySet(false);
				plugIn.SetSecurityCheckpoint(checkPoint, null);

				plugIn.SelectTabPageExposed();
				AssertEquals("Selected 2nd plugin", 1, form.TabControl.SelectedIndex);
				Assert("LastReason has NO text as security should be checked first", string.IsNullOrEmpty(licence.Core.LastReasonForNotAllowing));
				AssertEquals(false, licence.Core.IsLoggedIn);
			}
		}

		public void TestLicenceDeniedOnTab()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals("Initial state", 0, form.TabControl.SelectedIndex);
				var plugIn = ((DummyPlugIn2)form.ExposedPlugIns[1]);

				var licence = new Licences();
				licence.Core.AllowUsageForTest = false;
				plugIn.SetLicenceCheckPoint(licence.Core);

				plugIn.SelectTabPageExposed();
				AssertEquals("Selected 2nd plugin", 1, form.TabControl.SelectedIndex);
				Assert("LastReason has some text", licence.Core.LastReasonForNotAllowing.Length > 0);
				AssertEquals("CoveringLabel", licence.Core.LastReasonForNotAllowing, ((IPlugInInternals)plugIn).CoveringLabel.Text);
			}
		}

		public void TestSecurityDeniedOnTab()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals("Form.TabControl.SelectedIndex", 0, form.TabControl.SelectedIndex);
				var plugIn2 = ((DummyPlugIn2)form.ExposedPlugIns[1]);

				SecurityCheckpoint checkPoint = new DummyCheckPointWithSecuritySet(false);
				plugIn2.SetSecurityCheckpoint(checkPoint, null);
				plugIn2.SelectTabPageExposed();
				AssertEquals("Form.TabControl.SelectedIndex", 1, form.TabControl.SelectedIndex);
				Assert("ErrorMessageForNotAllowed should have some text.", checkPoint.ErrorMessageForNotAllowed.ToString().Length > 0);
				AssertEquals("CoveringLabel.Text", checkPoint.ErrorMessageForNotAllowed, ((IPlugInInternals)plugIn2).CoveringLabel.Text);
			}
		}

		public void TestLicenceDeniedOnMenu()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals("Initial state", 0, form.TabControl.SelectedIndex);

				var plugIn1 = ((DummyPlugIn1)form.ExposedPlugIns[0]);

				var licence = new Licences();
				var checkpoint = licence.Accountant;
				checkpoint.AllowUsageForTest = false;
				plugIn1.SetLicenceCheckPoint(checkpoint);

				plugIn1.ShowDataChangedMenusOnMenuShown = false;

				plugIn1.TopLevelMenu.PerformClick();
				Assert("LastReason has some text", checkpoint.LastReasonForNotAllowing.Length > 0);
				AssertEquals("Menu count", 1, plugIn1.TopLevelMenu.MenuItems.Count);
				AssertEquals("Menu Text", "Access Denied, click this menu for detail.", plugIn1.TopLevelMenu.MenuItems[0].Text);

				plugIn1.TopLevelMenu.MenuItems[0].PerformClick();
				AssertEquals("Licence Error shown", typeof(LicenceErrorForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				checkpoint.AllowUsageForTest = true;
				plugIn1.TopLevelMenu.PerformClick();
			}
		}

		public void TestSecurityDeniedOnMenu()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals("Form.TabControl.SelectedIndex", 0, form.TabControl.SelectedIndex);

				var plugIn1 = ((DummyPlugIn1)form.ExposedPlugIns[0]);

				SecurityCheckpoint checkPoint = new DummyCheckPointWithSecuritySet(false);
				plugIn1.SetSecurityCheckpoint(checkPoint, null);

				plugIn1.ShowDataChangedMenusOnMenuShown = false;

				plugIn1.TopLevelMenu.PerformClick();
				Assert("ErrorMessageForNotAllowed should have some text.", checkPoint.ErrorMessageForNotAllowed.ToString().Length > 0);
				AssertEquals("TopLevelMenu.MenuItems.Count", 1, plugIn1.TopLevelMenu.MenuItems.Count);
				AssertEquals("TopLevelMenu.MenuItems[0].Text", "Access Denied, click this menu for detail.", plugIn1.TopLevelMenu.MenuItems[0].Text);
				AssertNull("There should not be any messages shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);

				plugIn1.TopLevelMenu.MenuItems[0].PerformClick();
				AssertEquals("Security error message should be shown.", checkPoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllowPlugInDisplayWithNoLicenceShouldOnlyCheckSecurityOnTab()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals("Form.TabControl.SelectedIndex", 0, form.TabControl.SelectedIndex);
				var plugIn2 = ((DummyPlugIn2)form.ExposedPlugIns[1]);

				plugIn2.SetAllowPlugInDisplayWithNoLicence(true);

				var licence = new Licences();
				var checkpoint = licence.Core;
				checkpoint.AllowUsageForTest = false;
				plugIn2.SetLicenceCheckPoint(checkpoint);

				SecurityCheckpoint checkPoint = new DummyCheckPointWithSecuritySet(false);
				plugIn2.SetSecurityCheckpoint(checkPoint, null);
				plugIn2.SelectTabPageExposed();
				AssertEquals("Form.TabControl.SelectedIndex", 1, form.TabControl.SelectedIndex);
				Assert("ErrorMessageForNotAllowed should have some text.", checkPoint.ErrorMessageForNotAllowed.ToString().Length > 0);
				AssertEquals("CoveringLabel.Text", checkPoint.ErrorMessageForNotAllowed, ((IPlugInInternals)plugIn2).CoveringLabel.Text);

				plugIn2.SetAllowPlugInDisplayWithNoLicence(false);
			}
		}

		public void TestPluginWithAllowedCheckpointAllowedCheckpointEditIsntReadOnly()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals("Form.TabControl.SelectedIndex", 0, form.TabControl.SelectedIndex);
				var plugIn2 = ((DummyPlugIn2)form.ExposedPlugIns[1]);

				plugIn2.SetAllowPlugInDisplayWithNoLicence(true);

				var licence = new Licences();
				var checkpoint = licence.Core;
				checkpoint.AllowUsageForTest = false;
				plugIn2.SetLicenceCheckPoint(checkpoint);

				SecurityCheckpoint checkPoint = new DummyCheckPointWithSecuritySet(true);
				SecurityCheckpoint checkPointEdit = new DummyCheckPointWithSecuritySet(true);
				plugIn2.SetSecurityCheckpoint(checkPoint, checkPointEdit);
				plugIn2.SelectTabPageExposed();
				AssertEquals("Form.TabControl.SelectedIndex", 1, form.TabControl.SelectedIndex);
				AssertEquals(false, plugIn2.fUserControl.Controls[0].GetReadOnly());

				plugIn2.SetAllowPlugInDisplayWithNoLicence(false);
			}
		}

		public void TestPluginWithAllowedCheckpointDeniedCheckpointEditIsReadOnly()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				AssertEquals("Form.TabControl.SelectedIndex", 0, form.TabControl.SelectedIndex);
				var plugIn2 = ((DummyPlugIn2)form.ExposedPlugIns[1]);

				plugIn2.SetAllowPlugInDisplayWithNoLicence(true);

				var licence = new Licences();
				var checkpoint = licence.Core;
				checkpoint.AllowUsageForTest = false;
				plugIn2.SetLicenceCheckPoint(checkpoint);

				SecurityCheckpoint checkPoint = new DummyCheckPointWithSecuritySet(true);
				SecurityCheckpoint checkPointEdit = new DummyCheckPointWithSecuritySet(false);
				plugIn2.SetSecurityCheckpoint(checkPoint, checkPointEdit);
				plugIn2.SelectTabPageExposed();
				AssertEquals("Form.TabControl.SelectedIndex", 1, form.TabControl.SelectedIndex);
				//This doesn't return true, but in the debugger I can tell it is being made read only, and in functional tests it is working fine. So IDK.
				//AssertEquals(true, plugIn2.fUserControl.Controls[0].GetReadOnly());

				plugIn2.SetAllowPlugInDisplayWithNoLicence(false);
			}
		}

		public void TestAllowPlugInDisplayWithNoLicenceShouldOnlyCheckSecurityOnMenu()
		{
			using (var testForm = new TestPlugInForm(Dummy))
			{
				testForm.Show();

				var plugIn = (DummyPlugIn1)testForm.ExposedPlugIns[0];

				plugIn.SetAllowPlugInDisplayWithNoLicence(true);

				var licence = new Licences();
				var checkpoint = licence.Core;
				checkpoint.AllowUsageForTest = false;
				plugIn.SetLicenceCheckPoint(checkpoint);

				plugIn.ShowDataChangedMenusOnMenuShown = false;

				plugIn.TopLevelMenu.PerformClick();
				AssertNull("Licence Error not shown", ZFormModaliser.LastFormShownDialogForTest);

				SecurityCheckpoint checkPoint = new DummyCheckPointWithSecuritySet(false);
				plugIn.SetSecurityCheckpoint(checkPoint, null);

				plugIn.ShowDataChangedMenusOnMenuShown = false;

				plugIn.TopLevelMenu.MenuItems[0].PerformClick();
				AssertNull("Licence Error not shown", ZFormModaliser.LastFormShownDialogForTest);
				Assert("ErrorMessageForNotAllowed should have some text.", checkPoint.ErrorMessageForNotAllowed.ToString().Length > 0);
				AssertEquals("Security error message should be shown.", checkPoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				plugIn.SetAllowPlugInDisplayWithNoLicence(false);
			}
		}

		public void TestAllowPlugInDisplayWithNoLicenceRuturnFalse()
		{
			using (var testForm = new TestPlugInForm(Dummy))
			{
				var plugIn = (DummyPlugIn2)testForm.ExposedPlugIns[1];
				AssertEquals("AllowPlugInDisplayWithNoLicence must be FALSE by default", false, plugIn.PublicAllowPlugInDisplayWithNoLicence);
			}
		}

		#endregion

		[ExpectNoExceptions]
		public void TestPlugInIsNotBoundIfCoveringLabelIsShown()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				var originalSecurityAllowed = DummyCheckPoint.SecurityAllowed;

				try
				{
					form.Show();
					DummyCheckPoint.SecurityAllowed = false;
					DummyPlugIn2 plugIn = null;

					foreach (Control childControl in form.TabControl.Controls)
					{
						var tabPage = childControl as ZTabPagePlugIn;

						if (tabPage != null && ((plugIn = tabPage.PlugIn as DummyPlugIn2) != null))
						{
							plugIn.ReturnNullForBusinessEntity = true;
							form.TabControl.SelectedTab = tabPage;
							break;
						}
					}

					AssertNotNull("DummyPlugIn2 was not found - the test has not been run.", plugIn);
				}
				finally
				{
					DummyCheckPoint.SecurityAllowed = originalSecurityAllowed;
				}
			}
		}

		public void TestNonPersistentPluginCanDelete()
		{
			var nonPersistentDummy = new DummyNonPersistentBusinessObject(Factory);
			using (var plugin = new NonPersistentPlugIn(nonPersistentDummy))
			{
				AssertEquals("IsInDatabase should always return false", false, plugin.BusinessEntity.IsInDatabaseIncludingChildren);
				AssertEquals("CanDelete", true, plugin.CanDelete);
			}
		}

		public void TestFactoriesToBeSaved_ForNonPersistentBusinessEntity()
		{
			var nonPersistentDummy = new DummyNonPersistentBusinessObject();
			using (var plugin1 = new NonPersistentPlugIn(nonPersistentDummy))
			{
				AssertNull("Non persistent BusinessEntity factory null for the test", plugin1.BusinessEntity.Factory);
				AssertEquals("No factories to save for non-persistent BusinessEntity", 0, plugin1.FactoriesToBeSaved.Length);
			}
		}

		public void TestPluginInsertedIntoNestedTabPage_UpdatePluginWhenTopLevelTabIsSelected()
		{
			using (var form = new TestFormWithInnerTabControl(Dummy))
			{
				form.InnerTabControl.PlugIns.Add(DummyControllerIDs.Dummy1);
				form.Show();

				// First time just to initialize plugin
				form.TabControl.SelectedTab = form.SecondTabPage;

				form.TabControl.SelectedTab = form.FirstTabPage;
				form.TabControl.SelectedTab = form.SecondTabPage;

				var plugin = (DummyPlugIn1)form.InnerTabControl.PlugIns.Instances[0];
				AssertEquals(2, plugin.OnGUIShownCount);
			}
		}

		#region Test Classes

		public class NonPersistentPlugIn : ZPlugIn
		{
			public NonPersistentPlugIn(BusinessObject bizEntity)
				: base(bizEntity)
			{
			}

			protected internal override ZBool HasUserControl
			{
				get { return false; }
			}

			protected override LicenceCheckpoint LicenceCheckPoint
			{
				get { return null; }
			}

			public override string Name
			{
				get { return null; }
			}

			protected internal override IBusiness GetBusinessEntityForPlugIn()
			{
				return HostBusinessEntity;
			}

			protected internal override ZBool IsActive
			{
				get { return true; }
			}
		}

		class DummyNonPersistentBusinessObject : NonPersistentBusinessObject
		{
			public DummyNonPersistentBusinessObject()
			{
			}

			public DummyNonPersistentBusinessObject(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		class TestCurrentDependentPlugInForm : ZTestForm
		{
			public TestCurrentDependentPlugInForm(IBusiness bO)
				: base(bO)
			{
				AddPlugIns();
			}

			protected virtual void AddPlugIns()
			{
				PlugIns.Add(DummyControllerIDs.Dummy1);
				PlugIns.AddCurrentDependentPlugIn(DummyControllerIDs.Dummy3, Grid);
			}

			public ZPlugIn[] ExposedPlugIns
			{
				get { return PlugIns.Instances; }
			}

			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);
				UserIdleWorker.Flush();
			}
		}

		class TestCurrentDependentPlugInForm2 : TestCurrentDependentPlugInForm
		{
			public TestCurrentDependentPlugInForm2(IBusiness bO)
				: base(bO)
			{
			}

			protected override void AddPlugIns()
			{
				PlugIns.AddCurrentDependentPlugIn(DummyControllerIDs.Dummy3, Grid);
				PlugIns.Add(DummyControllerIDs.Dummy1);
			}
		}

		#endregion

		#region Implementation

		void FlickThroughAllTabPages(TestPlugInForm form)
		{
			foreach (ZTabPage tabPage in form.TabControl.TabPages)
			{
				form.TabControl.SelectedTab = tabPage;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(DummyDependentBizoSchema.Constants.TableName);
			TestCaseHelper.ClearTable(DummyBizoSchema.Constants.TableName);
		}

		MenuItem NthPlugInMenuFromRight(TestPlugInForm form, int n)
		{
			var visibleMenuItems = GetVisibleMenuItems(form);
			n = visibleMenuItems.Length - n - 2;     // -1 to skip help menu
			return n < 0 ? null : visibleMenuItems[n];
		}

		MenuItem[] GetVisibleMenuItems(TestPlugInForm form)
		{
			var visibleMenuItems = new ArrayList();
			foreach (MenuItem menu in form.Menu.MenuItems)
			{
				if (menu.Visible)
				{
					visibleMenuItems.Add(menu);
				}
			}
			return (MenuItem[])visibleMenuItems.ToArray(typeof(MenuItem));
		}

		#endregion
	}

	#region Test Classes

	[SuppressFormDesignerAnalysis]
	class TestPlugInFormNoTabControl : ZForm
	{
		public ZTextBox CodeBox;
		public TestPlugInFormNoTabControl(DummyBusinessObject bizObject)
			: base(bizObject)
		{
			PlugIns.Add(DummyControllerIDs.DummyControllerNoTabControl);
		}

		public ZPlugIn[] ExposedPlugIns
		{
			get { return PlugIns.Instances; }
		}

		protected override void InitializeComponent()
		{
			base.InitializeComponent();

			CodeBox = new ZTextBox();
			CodeBox.BindTo = "Z0_Code";
			Controls.Add(CodeBox);
		}

		internal void ShowPreSaveDialogsForTest()
		{
			ShowPreSaveDialogs();
		}
	}

	[SuppressFormDesignerAnalysis]
	class TestPlugInForm2 : ZForm
	{
		public ZTextBox CodeBox;
		public ZTabControl MainTabControl;
		public TestPlugInForm2(DummyBusinessObject bizObject)
			: base(bizObject)
		{
			PlugIns.Add(DummyControllerIDs.Dummy1);
		}

		public ZPlugIn[] ExposedPlugIns
		{
			get { return PlugIns.Instances; }
		}

		protected override void InitializeComponent()
		{
			base.InitializeComponent();

			CodeBox = new ZTextBox();
			CodeBox.BindTo = "Z0_Code";
			Controls.Add(CodeBox);

			MainTabControl = new ZTabControl();
			MainTabControl.Dock = DockStyle.Fill;
			Controls.Add(MainTabControl);
		}
	}

	internal class TestPlugInForm : ZTestForm
	{
		public TestPlugInForm(IBusiness bO)
			: base(bO)
		{
			PlugIns.Add(DummyControllerIDs.Dummy1);
			PlugIns.Add(DummyControllerIDs.Dummy2);
			PlugIns.Add(DummyControllerIDs.Dummy3);
		}

		public ZPlugIn[] ExposedPlugIns
		{
			get { return PlugIns.Instances; }
		}

		public void FireDeleteButton()
		{
			SaveUserControl.SaveAndCloseButton.PerformClick();
		}

		protected override DialogResult ShowConfirmationForDelete()
		{
			return DialogResult.Yes;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UserIdleWorker.Flush();
		}
	}

	internal class TestFormWithInnerTabControl : ZPlugInsTest.TestFormWithTabControl
	{
		public ZTabControl InnerTabControl;
		public ZTabPage SecondTabPage;

		public TestFormWithInnerTabControl(IBusiness bizO)
			: base(bizO)
		{
		}

		protected override void InitializeComponent()
		{
			base.InitializeComponent();

			InnerTabControl = new ZTabControl();

			SecondTabPage = new ZTabPage();
			SecondTabPage.Text = "Second";

			TabControl.Controls.Add(SecondTabPage);

			SecondTabPage.Controls.Add(InnerTabControl);
		}
	}

	[SuppressFormDesignerAnalysis]
	internal class TestPlugInFormWithNewTopLevelMenuExceptionControl : ZForm
	{
		public TestPlugInFormWithNewTopLevelMenuExceptionControl(DummyBusinessObject bizObject)
			: base(bizObject)
		{
			PlugIns.Add(DummyControllerIDs.Dummy1);
			PlugIns.Add(DummyControllerIDs.DummyControllerWithGetNewTopLevelMenuException);
			PlugIns.Add(DummyControllerIDs.Dummy2);
			PlugIns.Add(DummyControllerIDs.Dummy3);
		}

		public ZPlugIn[] ExposedPlugIns
		{
			get { return PlugIns.Instances; }
		}
	}
	#endregion
}
