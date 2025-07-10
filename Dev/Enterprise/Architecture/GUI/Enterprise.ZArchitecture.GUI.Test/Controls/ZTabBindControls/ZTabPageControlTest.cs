using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZTabPageControlTest : TestCaseWithDummy
	{
		public void TestGetEscapedTabPageText()
		{
			var testTabPageText = "United Kingdom - S&S GB";
			AssertEquals("Single & should become double", "United Kingdom - S&&S GB", ZTabPage.GetEscapedTabPageText(testTabPageText));
			testTabPageText = "United Kingdom - S&&S GB";
			AssertEquals("Double & should not change", "United Kingdom - S&&S GB", ZTabPage.GetEscapedTabPageText(testTabPageText));
		}

		public void TestPriorityOfNotifications()
		{
			TestTabPage.Controls.Add(TestControl1);
			TestTabPage.Controls.Add(TestControl2);
			TestTabPage.Controls.Add(TestControl3);

			AssertEquals("TestTabPage should have no Icon set", ExpectedDefaultTabIconIndex, TestTabPage.ImageIndex);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Warning, TestControl1);
			AssertEquals("TestTabPage should have Warning Icon set", Icons.GetImageIndex(IconTypes.Warning), TestTabPage.ImageIndex);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.EntityFramework.NotificationType.MessageError, TestControl2);
			AssertEquals("TestTabPage should have Message Error Icon set", Icons.GetImageIndex(IconTypes.MessageError), TestTabPage.ImageIndex);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Error, TestControl3);
			AssertEquals("TestTabPage should have Error Icon set", Icons.GetImageIndex(IconTypes.Error), TestTabPage.ImageIndex);
		}

		public void TestModifyingOneControlDoesNotModifyOtherControls()
		{
			TestTabPage.Controls.Add(TestControl1);
			TestTabPage.Controls.Add(TestControl2);
			TestTabPage.Controls.Add(TestControl3);

			AssertEquals("Precondition: No notifications", false, TestTabPage.HasErrors);
			AssertEquals("Precondition: No notifications", false, TestTabPage.HasWarnings);
			AssertEquals("Precondition: No notifications", false, TestTabPage.HasMessageErrors);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Warning, TestControl1);
			AssertEquals("Precondition: broadcast worked", true, TestTabPage.HasWarnings);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.EntityFramework.NotificationType.MessageError, TestControl2);
			AssertEquals("Precondition: broadcast worked", true, TestTabPage.HasMessageErrors);
			AssertEquals("Setting one control to message error doesn't affect other controls", true, TestTabPage.HasWarnings);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Error, TestControl3);
			AssertEquals("Precondition: broadcast worked", true, TestTabPage.HasErrors);
			AssertEquals("Setting one control to error doesn't affect other controls", true, TestTabPage.HasWarnings);
			AssertEquals("Setting one control to error doesn't affect other controls", true, TestTabPage.HasMessageErrors);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(null, TestControl3);
			AssertEquals("Precondition: broadcast worked", false, TestTabPage.HasErrors);
			AssertEquals("Setting one control to no notification doesn't affect other controls", true, TestTabPage.HasWarnings);
			AssertEquals("Setting one control to no notification doesn't affect other controls", true, TestTabPage.HasMessageErrors);
		}

		public void TestNotificationsClearedWhenControlDisposed()
		{
			TestTabPage.Controls.Add(TestControl1);

			AssertEquals("TestTabPage should have no Icon set", ExpectedDefaultTabIconIndex, TestTabPage.ImageIndex);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Error, TestControl1);
			AssertEquals("TestTabPage should have Error Icon set", Icons.GetImageIndex(IconTypes.Error), TestTabPage.ImageIndex);

			TestControl1.Dispose();
			ObjectFactory.Get<IIdleWorker>().Flush();
			AssertEquals("TestTabPage should have no Icon set", ExpectedDefaultTabIconIndex, TestTabPage.ImageIndex);
		}

		public void TestSetChildControlInError()
		{
			TestTabPage.Controls.Add(TestControl1);

			AssertEquals("TestTabPage should have no Icon set", ExpectedDefaultTabIconIndex, TestTabPage.ImageIndex);
			AssertEquals("Precondition: No notifications", false, TestTabPage.HasErrors);
			AssertEquals("Precondition: No notifications", false, TestTabPage.HasWarnings);
			AssertEquals("Precondition: No notifications", false, TestTabPage.HasMessageErrors);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Error, TestControl1);
			AssertEquals("TestTabPage should have Error Icon set", Icons.GetImageIndex(IconTypes.Error), TestTabPage.ImageIndex);
			AssertEquals("TestTabPage should have error", true, TestTabPage.HasErrors);
			AssertEquals("TestTabPage should not have warning", false, TestTabPage.HasWarnings);
			AssertEquals("TestTabPage should not have message error", false, TestTabPage.HasMessageErrors);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(null, TestControl1);
			AssertEquals("TestTabPage should have no Icon set", ExpectedDefaultTabIconIndex, TestTabPage.ImageIndex);
			AssertEquals("TestTabPage should not have error", false, TestTabPage.HasErrors);
			AssertEquals("TestTabPage should not have warning", false, TestTabPage.HasWarnings);
			AssertEquals("TestTabPage should not have message error", false, TestTabPage.HasMessageErrors);
		}

		public void TestSetChildControlInWarning()
		{
			TestTabPage.Controls.Add(TestControl1);

			AssertEquals("TestTabPage should have no Icon set", ExpectedDefaultTabIconIndex, TestTabPage.ImageIndex);
			AssertEquals("Precondition: No notifications", false, TestTabPage.HasErrors);
			AssertEquals("Precondition: No notifications", false, TestTabPage.HasWarnings);
			AssertEquals("Precondition: No notifications", false, TestTabPage.HasMessageErrors);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Warning, TestControl1);
			AssertEquals("TestTabPage should have Warning Icon set", Icons.GetImageIndex(IconTypes.Warning), TestTabPage.ImageIndex);
			AssertEquals("TestTabPage should not have error", false, TestTabPage.HasErrors);
			AssertEquals("TestTabPage should have warning", true, TestTabPage.HasWarnings);
			AssertEquals("TestTabPage should not have message error", false, TestTabPage.HasMessageErrors);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(null, TestControl1);
			AssertEquals("TestTabPage should have no Icon set", ExpectedDefaultTabIconIndex, TestTabPage.ImageIndex);
			AssertEquals("TestTabPage should not have error", false, TestTabPage.HasErrors);
			AssertEquals("TestTabPage should not have warning", false, TestTabPage.HasWarnings);
			AssertEquals("TestTabPage should not have message error", false, TestTabPage.HasMessageErrors);
		}

		public void TestSetChildControlInMessageError()
		{
			TestTabPage.Controls.Add(TestControl1);

			AssertEquals("TestTabPage should have no Icon set", ExpectedDefaultTabIconIndex, TestTabPage.ImageIndex);
			AssertEquals("Precondition: No notifications", false, TestTabPage.HasErrors);
			AssertEquals("Precondition: No notifications", false, TestTabPage.HasWarnings);
			AssertEquals("Precondition: No notifications", false, TestTabPage.HasMessageErrors);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.EntityFramework.NotificationType.MessageError, TestControl1);
			AssertEquals("TestTabPage should have Message Error Icon set", Icons.GetImageIndex(IconTypes.MessageError), TestTabPage.ImageIndex);
			AssertEquals("TestTabPage should not have error", false, TestTabPage.HasErrors);
			AssertEquals("TestTabPage should not have warning", false, TestTabPage.HasWarnings);
			AssertEquals("TestTabPage should have message error", true, TestTabPage.HasMessageErrors);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(null, TestControl1);
			AssertEquals("TestTabPage should have no Icon set", ExpectedDefaultTabIconIndex, TestTabPage.ImageIndex);
			AssertEquals("TestTabPage should not have error", false, TestTabPage.HasErrors);
			AssertEquals("TestTabPage should not have warning", false, TestTabPage.HasWarnings);
			AssertEquals("TestTabPage should not have message error", false, TestTabPage.HasMessageErrors);
		}

		public void TestNotifyAboutVisibilityChangeOfChildControl()
		{
			using (var form = new ZChildForm(new BusinessObjectFactory().New(typeof(DummyBusinessObject))))
			{
				var testTabPage1 = new ZTabPage();
				var testTextBox1 = new ZTextBox { BindTo = DummyBizoSchema.Z0_Description.Name };
				testTabPage1.Controls.Add(testTextBox1);

				var testTabControl = NewTabControl();
				testTabControl.TabPages.Add(testTabPage1);
				form.Controls.Add(testTabControl);
				form.Show();

				AssertEquals(true, testTabPage1.TabVisible);

				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Error, testTextBox1);
				testTabPage1.TabVisible = false;
				((IListenForNotifications)testTabPage1).NotifyAboutVisibilityChangeOfChildControl(testTextBox1);
				System.Windows.Forms.Application.DoEvents();
				AssertEquals("NotifyAboutVisibilityChangeOfChildControl should make visible testTabPage1 which has a control in error", true, testTabPage1.TabVisible);
			}
		}

		public void TestMinimumAutoSizedWidthAndHeight()
		{
			using (var page = new ZTabPage())
			{
				AssertEquals("MinimumAutoSizedWidth", ControlDpiScalingHelper.ScaleToCurrentDpiX(835), page.MinimumAutoSizedWidth);
				AssertEquals("MinimumAutoSizedHeight", ControlDpiScalingHelper.ScaleToCurrentDpiY(400), page.MinimumAutoSizedHeight);
			}
		}

		public void TestCodeDomSerializer()
		{
			var attr = (DesignerSerializerAttribute)TypeDescriptor.GetAttributes(typeof(ZTabPage))[typeof(DesignerSerializerAttribute)];
			AssertEquals("Tab page should use " + nameof(ControlCodeDomSerializerWithDelayedTabCreate) + " for runtime performance", true, attr.SerializerTypeName.StartsWith(typeof(ControlCodeDomSerializerWithDelayedTabCreate).FullName));
		}

		public void TestInvisibleControlShouldNotContainNotification()
		{
			using (var form = new ZChildForm(new BusinessObjectFactory().New(typeof(DummyBusinessObject))))
			{
				var testTabPage = new ZTabPage();
				var testTextBox = new ZTextBox { BindTo = DummyBizoSchema.Z0_Description.Name };
				testTabPage.Controls.Add(testTextBox);
				testTabPage.Text = "Test2";

				var testTabControl = NewTabControl();
				form.Controls.Add(testTabControl);
				form.Show();
				testTabControl.TabPages.Add(testTabPage);
				testTextBox.Visible = false;
				AssertEquals(testTabPage.Controls.Count, 1);
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Error, testTextBox);
				AssertEquals("invisible control Should not be in error", -1, testTabPage.ImageIndex);
			}
		}

		public void TestChangingNotificationsOnNonVisibleZTabs()
		{
			using (var form = new ZChildForm(new BusinessObjectFactory().New(typeof(DummyBusinessObject))))
			{
				var testTabPage1 = new ZTabPage();
				var testTabPage2 = new ZTabPage();
				var testTabPage3 = new ZTabPage();
				var testTextBox1 = new ZTextBox { BindTo = DummyBizoSchema.Z0_Description.Name };
				testTabPage2.Controls.Add(testTextBox1);

				testTabPage1.Text = "Test1";
				testTabPage2.Text = "Test2";

				var testTabControl = NewTabControl();
				form.Controls.Add(testTabControl);
				form.Show();
				testTabControl.TabPages.Add(testTabPage1);
				testTabControl.TabPages.Add(testTabPage2);

				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.EntityFramework.NotificationType.MessageError, testTextBox1);
				System.Windows.Forms.Application.DoEvents();
				AssertEquals("Should be in message error", Icons.GetImageIndex(IconTypes.MessageError), testTabPage2.ImageIndex);

				testTabControl.TabPages.Insert(testTabPage3, 0);
				testTabPage2.TabVisible = false;
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(null, testTextBox1);
				System.Windows.Forms.Application.DoEvents();
				testTabControl.TabPages.Add(testTabPage2);
				AssertEquals("No error", -1, testTabPage2.ImageIndex);

				testTabPage2.TabVisible = false;
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.EntityFramework.NotificationType.MessageError, testTextBox1);
				System.Windows.Forms.Application.DoEvents();
				AssertEquals("Should be in message error", Icons.GetImageIndex(IconTypes.MessageError), testTabPage2.ImageIndex);
				AssertEquals("Should be made visible by setting a child control in error", true, testTabPage2.TabVisible);
			}
		}

		public void TestNotifyAboutStateOfChildControl_ZTabPageNotVisible_ClearControlNotification_DoesNotMakeZTabPageVisible()
		{
			using (var form = new ZChildForm(new BusinessObjectFactory().New(typeof(DummyBusinessObject))))
			using (var testTabControl = NewTabControl())
			using (var testTabPage = new ZTabPage())
			using (var testTextBox = new ZTextBox { BindTo = DummyBizoSchema.Z0_Description.Name })
			{
				testTabPage.Controls.Add(testTextBox);
				form.Controls.Add(testTabControl);
				form.Show();
				testTabControl.TabPages.Add(testTabPage);

				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Warning, testTextBox);
				Assert("Precondition: TabPage should have notification", testTabPage.HasNotifications);
				Assert("Precondition: TabPage should be visible", testTabPage.TabVisible);

				testTabPage.TabVisible = false;
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(null, testTextBox);
				AssertEquals("Precondition: TabPage should not have notification", false, testTabPage.HasNotifications);
				AssertEquals("TabPage should remain invisible", false, testTabPage.TabVisible);
			}
		}

		public void TestBindingWithErrorsDoesNotBindTwice()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ExceptionReporterTestListener.Instance.Clear();
			using (var form = new ZChildForm(Factory.New<DummyBusinessObject>()))
			{
				var testTabPage1 = new ZTabPage();
				var testTabPage2 = new ZTabPage();
				var testTabControl = NewTabControl();
				var testUserControl = new ZUserControl();
				var grid = new ZGrid();

				testTabPage2.Controls.Add(testUserControl);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(testUserControl, ".");
				testUserControl.Controls.Add(grid);
				grid.BindTo = "Collection";
				var textBoxColumnStyle = new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Description" };
				grid.ColumnStyles.Add(textBoxColumnStyle);
				grid.AfterBind += delegate
				{ throw new Exception(); };

				testTabControl.TabPages.Add(testTabPage1);
				testTabControl.TabPages.Add(testTabPage2);
				form.Controls.Add(testTabControl);

				form.Show();
				testTabControl.SelectedIndex = 1;

				AssertNotNull("ExceptionReporter should have caught the exception from the AfterBind() event", ExceptionReporterTestListener.Instance[0]);
				ErrorReporter.Clear();

				testTabControl.SelectedIndex = 0;
				testTabControl.SelectedIndex = 1;
				AssertNull("Should not have shown any further messages to user from binding", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should be no exceptions reported from AfterBind() event", 0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		public void TestDelayBinding()
		{
			var bindingTabPage = TestTabPage as ZBindingTabPage;
			AssertEquals("Correct default - DelayBinding", false, bindingTabPage != null && bindingTabPage.DelayBinding);
		}

		public void TestAllowShowImage()
		{
			TestTabPage.Controls.Add(TestControl1);

			AssertEquals("TestTabPage should have no Icon set", ExpectedDefaultTabIconIndex, TestTabPage.ImageIndex);

			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.EntityFramework.NotificationType.MessageError, TestControl1);

			AssertEquals("TestTabPage should have Message Error Icon set", Icons.GetImageIndex(IconTypes.MessageError), TestTabPage.ImageIndex);
			TestTabPage.AllowShowImage = false;
			AssertEquals("TestTabPage should have no Icon set", ExpectedDefaultTabIconIndex, TestTabPage.ImageIndex);
			TestTabPage.AllowShowImage = true;
			AssertEquals("TestTabPage should have Message Error Icon set", Icons.GetImageIndex(IconTypes.MessageError), TestTabPage.ImageIndex);
		}

		public void TestText_RemovesShortcutAmpersands()
		{
			TestTabPage.Text = "My &TestTabPage";
			AssertEquals("My TestTabPage", TestTabPage.Text);

			TestTabPage.Text = "Workflow && Tracking";
			AssertEquals("Workflow && Tracking", TestTabPage.Text);

			TestTabPage.Text = "My TestTabPage&";
			AssertEquals("My TestTabPage&", TestTabPage.Text);

			TestTabPage.Text = "My && &TestTabPage";
			AssertEquals("My && TestTabPage", TestTabPage.Text);

			TestTabPage.Text = "My && &TestTabPage &&";
			AssertEquals("My && TestTabPage &&", TestTabPage.Text);

			TestTabPage.Text = "My && &TestTab &Page";
			AssertEquals("My && TestTab Page", TestTabPage.Text);
		}

		public void TestNotifyAboutStateOfChildControlShouldIgnoreZTabPageItSelf()
		{
			using (var testTabPage = NewTabPage())
			using (var control1 = new Control())
			using (var control2 = new Control())
			{
				testTabPage.Controls.Add(control1);
				testTabPage.Controls.Add(control2);
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.EntityFramework.NotificationType.MessageError, control1);
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.EntityFramework.NotificationType.MessageError, control2);
				Assert("TabPage should have message error", testTabPage.HasMessageErrors);

				NotificationBroadcaster.Instance.BroadcastNotificationsChange(null, control1);
				Assert("TabPage should still have message error coz it still has 1 control error", testTabPage.HasMessageErrors);
			}
		}

		public void TestCheckForNotifications()
		{
			TestTabPage.Controls.Add(TestControl1);

			TestTabPage.CheckForNotifications = false;
			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Error, TestControl1);

			AssertEquals("TestTabPage should have no Icon set, because CheckForNotifications was false when it was notified of the state of the child control", ExpectedDefaultTabIconIndex, TestTabPage.ImageIndex);

			TestTabPage.CheckForNotifications = true;
			NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Error, TestControl1);

			AssertEquals("TestTabPage should have Error Icon set, because CheckForNotifications was true when it was notified of the state of the child control", Icons.GetImageIndex(IconTypes.Error), TestTabPage.ImageIndex);
		}

		public void TestCheckForChildrenControlsVisibilityChange()
		{
			using (var form = new ZChildForm(new BusinessObjectFactory().New(typeof(DummyBusinessObject))))
			{
				var testTabPage1 = new ZTabPage();
				var testTextBox1 = new ZTextBox { BindTo = DummyBizoSchema.Z0_Description.Name };
				testTabPage1.Controls.Add(testTextBox1);

				var testTabControl = NewTabControl();
				testTabControl.TabPages.Add(testTabPage1);
				form.Controls.Add(testTabControl);
				form.Show();

				AssertEquals(true, testTabPage1.TabVisible);

				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Error, testTextBox1);
				System.Windows.Forms.Application.DoEvents();
				testTabPage1.TabVisible = false;
				testTabPage1.CheckForChildrenControlsVisibilityChange = false;
				((IListenForNotifications)testTabPage1).NotifyAboutVisibilityChangeOfChildControl(testTextBox1);
				System.Windows.Forms.Application.DoEvents();
				AssertEquals("NotifyAboutVisibilityChangeOfChildControl should not make visible testTabPage1 which has a control in error, because CheckForChildrenControlsVisibilityChange is false", false, testTabPage1.TabVisible);

				testTabPage1.CheckForChildrenControlsVisibilityChange = true;
				((IListenForNotifications)testTabPage1).NotifyAboutVisibilityChangeOfChildControl(testTextBox1);
				System.Windows.Forms.Application.DoEvents();
				AssertEquals("NotifyAboutVisibilityChangeOfChildControl should make visible testTabPage1 which has a control in error, because CheckForChildrenControlsVisibilityChange is true", true, testTabPage1.TabVisible);
			}
		}

		#region Licencing

		public void TestLicencing()
		{
			using (var tabPage = new ZTabPage())
			{
				tabPage.TabVisible = false;

				var dummyLicenceCheckpoint = new DummyLicenceCheckpoint();
				tabPage.LicenceCheckpoint = dummyLicenceCheckpoint;

				AssertEquals(0, tabPage.Controls.Count);

				dummyLicenceCheckpoint.LoginResponseOverride = LicenceLoginResponse.Granted;
				tabPage.Show();
				AssertEquals(0, tabPage.Controls.Count);

				tabPage.Hide();
				dummyLicenceCheckpoint.LoginResponseOverride = LicenceLoginResponse.Denied;
				tabPage.Show();
				AssertEquals(1, tabPage.Controls.Count);
				AssertEquals(typeof(ZLabel), tabPage.Controls[0].GetType());
			}
		}

		#endregion

		#region Security

		public void TestSetupSecurity()
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			var checkpoint1 = new SecurityCheckpoint("TS1", (NoResString)"Test 1", null, security.ZSecurityInstance);
			var checkpoint2 = new SecurityCheckpoint("TS2", (NoResString)"Test 2", null, security.ZSecurityInstance);

			checkpoint1.IsAllowed = false;
			checkpoint2.IsAllowed = false;
			using (var tabPage = new ZTabPage())
			{
				tabPage.SetupSecurity(new SecurityCheckpoint[] { checkpoint1, checkpoint2 });
				AssertEquals("coveringLabel", tabPage.Controls[0].Name);
			}

			checkpoint1.IsAllowed = true;
			using (var tabPage = new ZTabPage())
			{
				tabPage.SetupSecurity(new SecurityCheckpoint[] { checkpoint1, checkpoint2 });
				AssertEquals("coveringLabel", tabPage.Controls[0].Name);
			}

			checkpoint2.IsAllowed = true;
			using (var tabPage = new ZTabPage())
			{
				tabPage.SetupSecurity(new SecurityCheckpoint[] { checkpoint1, checkpoint2 });
				AssertEquals(0, tabPage.Controls.Count);
			}
		}

		#endregion

		#region TabInitialized

		public void TestTabInitialized()
		{
			using (var form = new ZForm(Dummy))
			using (var tabControl = new ZTabControl())
			using (var page = new TestBindingTabPage())
			{
				var tabInitialized = false;
				page.TabInitialized += delegate
				{ tabInitialized = true; };
				tabControl.TabPages.Add(page);
				form.Controls.Add(tabControl);

				AssertEquals("Not fired initially", false, tabInitialized);
				page.SetDataBinding(Factory.New<DummyBusinessObject>(), "");
				AssertEquals("Fired after binding", true, tabInitialized);
			}
		}

		public void TestRunWhenTabInitialized_FiredAlwaysWhenDesigning()
		{
			DesignModeFinder.SetIsDesigningForTest(true);
			try
			{
				var runWhenTabInitializedDelegateFired = false;
				using (var page = new ZTabPage())
				{
					page.RunWhenTabInitialized(delegate
					{ runWhenTabInitializedDelegateFired = true; });
					AssertEquals("Fired always when designing", true, runWhenTabInitializedDelegateFired);
				}
			}
			finally
			{
				DesignModeFinder.SetIsDesigningForTest(false);
			}
		}

		[RequiresSTA]
		public void TestRunWhenTabInitialized()
		{
			using (var form = new ZForm(Dummy))
			using (var tabControl = new ZTabControl())
			using (var page = new ZTabPage())
			{
				var runWhenTabInitializedDelegateFired = false;
				tabControl.TabPages.Add(new ZTabPage());
				tabControl.TabPages.Add(page);
				form.Controls.Add(tabControl);
				form.Show();
				Application.DoEvents();
				page.RunWhenTabInitialized(delegate
				{ runWhenTabInitializedDelegateFired = true; });
				Application.DoEvents();
				AssertEquals("Not fired initially", false, runWhenTabInitializedDelegateFired);

				tabControl.SelectedTab = page;
				Application.DoEvents();
				AssertEquals("Fired immediately after bind", true, runWhenTabInitializedDelegateFired);

				runWhenTabInitializedDelegateFired = false;
				page.RunWhenTabInitialized(delegate
				{ runWhenTabInitializedDelegateFired = true; });
				AssertEquals("Fired when bind has completed", true, runWhenTabInitializedDelegateFired);
			}
		}

		#endregion

		#region BindingOrFirstShown

		[RequiresSTA]
		public void TestBindingOrFirstShown_OnFirstShown()
		{
			using (var form = new ZForm(Dummy))
			using (var tabControl = new ZTabControl())
			using (var page = new ZTabPage())
			{
				var bindingOrFirstShownFired = false;
				page.BindingOrFirstShown += delegate
				{ bindingOrFirstShownFired = true; };
				tabControl.TabPages.Add(page);
				form.Controls.Add(tabControl);

				AssertEquals("Not fired initially", false, bindingOrFirstShownFired);
				form.Show();
				Application.DoEvents();
				AssertEquals("Fired after first show", true, bindingOrFirstShownFired);
				bindingOrFirstShownFired = false;

				form.Hide();
				form.Show();
				Application.DoEvents();
				AssertEquals("Not fired a second time ever", false, bindingOrFirstShownFired);
			}
		}

		public void TestBindingOrFirstShown_OnBind()
		{
			using (var form = new ZForm(Dummy))
			using (var tabControl = new ZTabControl())
			using (ZBindingTabPage page = new TestBindingTabPage())
			{
				var bindingOrFirstShownFired = false;
				page.BindingOrFirstShown += delegate
				{ bindingOrFirstShownFired = true; };
				tabControl.TabPages.Add(page);
				form.Controls.Add(tabControl);

				AssertEquals("Not fired initially", false, bindingOrFirstShownFired);
				page.SetDataBinding(Factory.New<DummyBusinessObject>(), "");
				AssertEquals("Fired after bind", true, bindingOrFirstShownFired);
			}
		}

		public void TestRunWhenBindingOrFirstShown_FiredAlwaysWhenDesigning()
		{
			DesignModeFinder.SetIsDesigningForTest(true);
			try
			{
				var runWhenBindingOrFirstShownDelegateFired = false;
				using (var page = new ZTabPage())
				{
					page.RunWhenBindingOrFirstShown(delegate
					{ runWhenBindingOrFirstShownDelegateFired = true; });
					AssertEquals("Fired always when designing", true, runWhenBindingOrFirstShownDelegateFired);
				}
			}
			finally
			{
				DesignModeFinder.SetIsDesigningForTest(false);
			}
		}

		[RequiresSTA]
		public void TestRunWhenBindingOrFirstShown()
		{
			using (var form = new ZForm(Dummy))
			using (var tabControl = new ZTabControl())
			using (var page = new ZTabPage())
			{
				var runWhenBindingOrFirstShownDelegateFired = false;
				tabControl.TabPages.Add(new ZTabPage());
				tabControl.TabPages.Add(page);
				form.Controls.Add(tabControl);
				form.Show();
				Application.DoEvents();
				page.RunWhenBindingOrFirstShown(delegate
				{ runWhenBindingOrFirstShownDelegateFired = true; });
				Application.DoEvents();
				AssertEquals("Not fired initially", false, runWhenBindingOrFirstShownDelegateFired);

				tabControl.SelectedTab = page;
				Application.DoEvents();
				AssertEquals("Fired immediately after bind", true, runWhenBindingOrFirstShownDelegateFired);

				runWhenBindingOrFirstShownDelegateFired = false;
				page.RunWhenBindingOrFirstShown(delegate
				{ runWhenBindingOrFirstShownDelegateFired = true; });
				AssertEquals("Fired when bind has completed", true, runWhenBindingOrFirstShownDelegateFired);
			}
		}

		#endregion

		#region TestParentTabControl

		public void TestParentTabControl()
		{
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			{
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(tabPage);

				AssertEquals(tabControl, tabPage.Parent);
				AssertEquals(tabControl, tabPage.ParentTabControl);

				tabPage.TabVisible = false;

				AssertNull(tabPage.Parent);
				AssertEquals(tabControl, tabPage.ParentTabControl);
			}
		}

		#endregion

		#region TabRelevant

		public void TestTabRelevant()
		{
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			using (var tabControl = new ZTabControl())
			using (var tabPage1 = new ZTabPage())
			using (var tabPage2 = new ZTabPage())
			{
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(tabPage1);
				tabControl.TabPages.Add(tabPage2);
				tabControl.CanTabPageBeVisibleOnSetRelevantDelegate = (tab) => { return tab == tabPage2; };

				form.Show();

				tabPage1.TabRelevant = false;
				Assert("TabRelevant", !tabPage1.TabRelevant);
				Assert("TabVisible", !tabPage1.TabVisible);
				tabPage1.TabRelevant = true;
				Assert("TabRelevant", tabPage1.TabRelevant);
				Assert("TabVisible", !tabPage1.TabVisible);

				tabPage2.TabRelevant = false;
				Assert("TabRelevant", !tabPage2.TabRelevant);
				Assert("TabVisible", !tabPage2.TabVisible);
				tabPage2.TabRelevant = true;
				Assert("TabRelevant", tabPage2.TabRelevant);
				Assert("TabVisible", tabPage2.TabVisible);
			}
		}

		#endregion

		#region ZGrid in ZTabPage

		[RequiresSTA]
		public void TestZTabPage_ZGrid_ColumnStylesSameAsColumns()
		{
			using (var form = new DummyForm(Dummy))
			{
				AssertEquals("Not fired Binding", false, form.bindingOrFirstShownFired);
				AssertEquals("Not fired TabInitialized", false, form.runWhenTabInitializedDelegateFired);
				AssertEquals(form.executionOrder, 0);
				ZForm.QuietlyShowForm(form);
				AssertEquals("Binding after form shown", true, form.bindingOrFirstShownFired);
				AssertEquals("TabInitialized after form shown", true, form.runWhenTabInitializedDelegateFired);
				AssertEquals("Binding -> TabInitialized", form.executionOrder, 2);
				AssertEquals("Grid ColumnStyles count", form.innerPage.grid.ColumnStyles.Count, 2);
				AssertEquals("Grid Columns count", form.innerPage.grid.Columns.Count, 2);
			}
		}

		#endregion

		#region Test Classes

		class TestBindingTabPage : ZBindingTabPage { }

		public class SecurityForTest : SecurityCore
		{
			public SecurityForTest(IZGlbSecurityCollection securityCollection, object staffMember, Guid branchPK, Guid departmentPK, Guid companyPK)
				: base(securityCollection, staffMember, branchPK, departmentPK, companyPK)
			{
			}

			public IZSecurity ZSecurityInstance
			{
				get { return SecurityInstance; }
			}
		}

		class DummyLicenceCheckpoint : LicenceCheckpoint
		{
			public override LicenceLoginResponse Login(ILicensedComponent licensedComponent)
			{
				LastLoginComponent = licensedComponent;
				return LoginResponseOverride;
			}

			internal LicenceLoginResponse LoginResponseOverride;
			internal ILicensedComponent LastLoginComponent;
		}

		class DummyZUserControl : ZUserControl
		{
			internal ZTabPage page;
			internal ZGrid grid;
			TabControl tabControl;
			internal ZPanel panel;

			internal ZDateEditColumnStyleInfo column1;
			internal ZDateEditColumnStyleInfo column2;
			internal ZDateEditColumnStyleInfo column3;

			public DummyZUserControl()
			{
				panel = new ZPanel();
				this.Controls.Add(panel);

				tabControl = new ZTabControl();
				panel.Controls.Add(tabControl);

				page = new ZTabPage();
				tabControl.TabPages.Add(page);

				page.CaptionResourceString = new ResourceStringData("2", "UserControl Tab");
				page.RunWhenBindingOrFirstShown(delegate
				{
					grid = new ZGrid();
					column1 = new ZDateEditColumnStyleInfo();
					column1.ColumnName = "column1";
					column2 = new ZDateEditColumnStyleInfo();
					column2.ColumnName = "column2";
					column3 = new ZDateEditColumnStyleInfo();
					column3.ColumnName = "column3";
					grid.ColumnStyles.Add(column1);
					grid.ColumnStyles.Add(column2);
					grid.ColumnStyles.Add(column3);
					page.Controls.Add(grid);

					this.BindingSource.SetBindingMember(grid, ".");
					grid.Name = "grid";
				});
				CaptionRenderingEnabled = true;
			}

			protected override void Dispose(bool disposing)
			{
				if (page != null)
				{
					page.Dispose();
					page = null;
				}
				if (grid != null)
				{
					grid.Dispose();
					grid = null;
				}
				if (tabControl != null)
				{
					tabControl.Dispose();
					tabControl = null;
				}
				base.Dispose(disposing);
			}
		}

		class DummyForm : ZForm
		{
			ZTabControl tabControl;
			ZTabPage page;
			internal DummyZUserControl innerPage;

			internal bool bindingOrFirstShownFired;
			internal bool runWhenTabInitializedDelegateFired;
			internal int executionOrder;

			public DummyForm(BusinessObject businessEntity) : base(businessEntity)
			{
				bindingOrFirstShownFired = false;
				runWhenTabInitializedDelegateFired = false;
				executionOrder = 0;

				SetDataBinding(businessEntity, "");
			}

			protected override void InitialiseForm()
			{
				base.InitialiseForm();

				InitializeComponent();
			}

			new void InitializeComponent()
			{
				tabControl = new ZTabControl();
				page = new ZTabPage();
				tabControl.TabPages.Add(page);
				this.Controls.Add(tabControl);

				page.RunWhenBindingOrFirstShown(delegate
				{
					bindingOrFirstShownFired = true;
					if (executionOrder == 0)
					{
						executionOrder = 1;
					}
					else if (executionOrder == 4)
					{
						executionOrder = 5;
					}

					innerPage = new DummyZUserControl();
					page.Controls.Add(innerPage);
					this.BindingSource.SetBindingMember(innerPage, ".");
				});
				page.RunWhenTabInitialized(delegate
				{
					runWhenTabInitializedDelegateFired = true;
					if (executionOrder == 1)
					{
						executionOrder = 2;
					}
					else if (executionOrder == 0)
					{
						executionOrder = 4;
					}
				});
				page.CaptionResourceString = new ResourceStringData("1", "Form Tab");
				this.CaptionRenderingEnabled = true;
			}

			public override void SetDataBinding(object dataSource, string dataMember)
			{
				base.SetDataBinding(dataSource, dataMember);

				if (dataSource != null)
				{
					page.RunWhenTabInitialized((sender, args) =>
					{
						innerPage.page.RunWhenTabInitialized(delegate
						{
							innerPage.grid.ColumnStyles.Remove(innerPage.column1);
						});
					});
				}
			}

			protected override void Dispose(bool disposing)
			{
				if (tabControl != null)
				{
					tabControl.Dispose();
					tabControl = null;
				}
				if (page != null)
				{
					page.Dispose();
					page = null;
				}
				if (innerPage != null)
				{
					innerPage.Dispose();
					innerPage = null;
				}
				base.Dispose(disposing);
			}
		}

		#endregion

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZTabPage.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZTabPage)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZTabPage).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}

		#region Implementation

		protected virtual int ExpectedDefaultTabIconIndex
		{
			get { return -1; }
		}

		protected virtual ZTabControl NewTabControl()
		{
			return new ZTabControl();
		}

		protected virtual ZTabPage NewTabPage()
		{
			return new TestBindingTabPage();
		}
#if DEBUG
		public
#endif
		ZTabControl TestTabControl;
#if DEBUG
	public
#endif
		ZTabPage TestTabPage;
		Control TestControl1;
		Control TestControl2;
		Control TestControl3;

		protected override void SetUp()
		{
			base.SetUp();

			TestTabControl = NewTabControl();
			TestTabPage = NewTabPage();
			TestTabControl.TabPages.Add(TestTabPage);
			TestControl1 = new Control();
			TestControl2 = new Control();
			TestControl3 = new Control();
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (TestTabPage != null)
			{
				TestTabPage.Dispose();
			}

			if (TestTabControl != null)
			{
				TestTabControl.Dispose();
			}

			if (TestControl1 != null)
			{
				TestControl1.Dispose();
			}

			if (TestControl2 != null)
			{
				TestControl2.Dispose();
			}

			if (TestControl3 != null)
			{
				TestControl3.Dispose();
			}
		}

#endregion
	}
}
