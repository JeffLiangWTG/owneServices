using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.Core.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTabControlTest : ZControlBaseTestCase<ZTabControl>
	{
		#region Security

		public void TestLicence()
		{
			using (var form = new ZForm())
			using (var tabControl = new ZTabControl())
			using (var tabPage1 = new ZTabPage())
			using (var licencedTabPage = new ZTabPage())
			{
				form.Controls.Add(tabControl);
				tabControl.Controls.Add(tabPage1);
				tabControl.Controls.Add(licencedTabPage);
				form.Show();

				var dummyLicenceCheckpoint = new DummyLicenceCheckpoint();
				licencedTabPage.LicenceCheckpoint = dummyLicenceCheckpoint;

				tabControl.SelectedTab = tabPage1;
				dummyLicenceCheckpoint.LastLoginComponent = null;
				dummyLicenceCheckpoint.LoginResponseOverride = LicenceLoginResponse.Granted;
				ZFormModaliser.LastFormShownDialogForTest = null;

				tabControl.SelectedTab = licencedTabPage;
				AssertEquals("Should change tab if licence granted", licencedTabPage, tabControl.SelectedTab);
				AssertEquals(licencedTabPage, dummyLicenceCheckpoint.LastLoginComponent);
				AssertNull("Should not show any forms", ZFormModaliser.LastFormShownDialogForTest);

				tabControl.SelectedTab = tabPage1;
				dummyLicenceCheckpoint.LastLoginComponent = null;
				dummyLicenceCheckpoint.LoginResponseOverride = LicenceLoginResponse.Denied;
				ZFormModaliser.LastFormShownDialogForTest = null;

				tabControl.SelectedTab = licencedTabPage;
				AssertEquals("Should not change tab if licence denied", licencedTabPage, tabControl.SelectedTab);
				AssertEquals(1, licencedTabPage.Controls.Count);
				AssertEquals(typeof(ZLabel), licencedTabPage.Controls[0].GetType());
			}
		}

		public void TestSecureTabs()
		{
			var dummy = Factory.New<ReadOnlyDummy>();
			using (var testForm = new ZForm(dummy))
			{
				var security = CreateSecurityInstance();
				var root = CreateRootCheckPoint(security);

				var tabPage1 = new TestBindingTabPage { Name = "Tab1" };
				TabControl.TabPages.Add(tabPage1);

				var textBox1 = new ZTextBox { Name = "TextBox1", BindTo = "Z0_Code" };
				tabPage1.Controls.Add(textBox1);

				var tabPage2 = new TestBindingTabPage { Name = "Tab2" };
				TabControl.TabPages.Add(tabPage2);

				var textBox2 = new ZTextBox { Name = "TextBox2", BindTo = "Z0_Description" };
				tabPage2.Controls.Add(textBox2);

				testForm.SecurityToken = "Screen";
				TabControl.Secured = true;
				TabControl.Security = security;

				new FormSecurityBuilder(root, security, true, true)
					.Form(testForm.SecurityToken)
					.Tab(tabPage1.Name)
					.Tab(tabPage2.Name);

				security.FindCheckPoint(testForm.SecurityToken + ".Edit." + tabPage1.Name).IsAllowed = false;
				security.FindCheckPoint(testForm.SecurityToken + ".Edit." + tabPage2.Name).IsAllowed = false;

				testForm.Controls.Add(TabControl);
				testForm.Show();

				Application.DoEvents();
				UserIdleWorker.Flush();

				Assert(TabControl.SelectedTab == tabPage1);
				Assert("TextBox1 should be set read only", textBox1.ReadOnly);
				Assert("TextBox2 should not be set read only", !textBox2.ReadOnly);

				TabControl.SelectedTab = tabPage2;
				Application.DoEvents();
				UserIdleWorker.Flush();

				Assert(TabControl.SelectedTab == tabPage2);
				Assert("TextBox2 should be set read only", textBox2.ReadOnly);

				dummy.Z0_Code_ReadOnly = false;
				dummy.Z0_Description_ReadOnly = false;

				Assert("TextBox1 should remain read only", textBox1.ReadOnly);
				Assert("TextBox2 should remain read only", textBox2.ReadOnly);

				TabControl.SelectedTab = tabPage1;
				Assert(TabControl.SelectedTab == tabPage1);

				// deny security
				security.FindCheckPoint(testForm.SecurityToken + ".View." + tabPage2.Name).IsAllowed = false;

				Assert("covering label not shown", tabPage2.coveringLabel == null);
				TabControl.SelectedTab = tabPage2;
				Assert("Should be allowed to select unauthorized tab", TabControl.SelectedTab == tabPage2);
				Assert("covering label shown", tabPage2.coveringLabel != null);
			}
		}

		static SecurityCheckpoint CreateRootCheckPoint(IZSecurity security)
		{
			return new SecurityCheckpoint("Root", (NoResString)"Root", null, security);
		}

		static IZSecurity CreateSecurityInstance()
		{
			return new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}

		class ReadOnlyDummy : DummyBusinessObject
		{
			public ReadOnlyDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
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

		#endregion
		public void TestTabOrderExtendedToTabPages()
		{
			AssertEquals(true, ((ITabOrderExtendedToTabPages)TabControl).TabOrderExtendedToTabPages);
		}

		#region TestFormIsResizedByTabPage

		public void TestFormIsResizedByTabPage()
		{
			var formMock = new Mock<ZForm>(new object[] { Dummy }) { CallBase = true };
			formMock.SetupGet(o => o.IsResizableByTabPageAllowed).Returns(true);
			using (var form = formMock.Object)
			{
				form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
				var tabPageMock = new Mock<ZBindingTabPage>() { CallBase = true };
				tabPageMock.SetupGet(o => o.IsAutoSized).Returns(true);
				var tabPage = tabPageMock.Object;
				const int minimumAutoSizedWidth = 600;
				const int minimumAutoSizedHeight = 500;
				tabPage.MinimumAutoSizedWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(minimumAutoSizedWidth);
				tabPage.MinimumAutoSizedHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(minimumAutoSizedHeight);
				var tabControl = new ZTabControl();
				form.Controls.Add(tabControl);
				tabControl.Dock = DockStyle.Fill;
				ZBindingTabPage previousTabPage = new TestBindingTabPage();
				previousTabPage.SetDataBinding(Dummy, "");
				tabPage.SetDataBinding(Dummy, "");
				form.Show();
				tabControl.TabPages.Add(previousTabPage);
				tabControl.TabPages.Add(tabPage);
				form.Size = new Size(400, 300);
				tabControl.Location = new Point(5, 6);
				tabPage.Top = 23;
				tabPage.Left = 4;
				tabControl.SelectedTab = previousTabPage;
				AssertEquals("Width", 400, form.Width);
				AssertEquals("Height", 300, form.Height);
				AssertEquals("FormBorderStyle", FormBorderStyle.SizableToolWindow, form.FormBorderStyle);
				tabControl.SelectedTab = tabPage;
				AssertEquals("Width should be greater than 400", true, form.Width > 400);
				AssertEquals("Height should be greater than 300", true, form.Height > 300);
				AssertEquals("FormBorderStyle", FormBorderStyle.Sizable, form.FormBorderStyle);
				tabControl.SelectedTab = previousTabPage;
				AssertEquals("Width", 400, form.Width);
				AssertEquals("Height", 300, form.Height);
				AssertEquals("FormBorderStyle", FormBorderStyle.SizableToolWindow, form.FormBorderStyle);

				form.Size = new Size(800, 800);
				tabControl.SelectedTab = tabPage;
				AssertEquals("Width", 800, form.Width);
				AssertEquals("Height", 800, form.Height);
				form.Size = new Size(900, 900);
				tabControl.FormResizeComplete(null, null);
				tabControl.SelectedTab = previousTabPage;
				AssertEquals("Width", 900, form.Width);
				AssertEquals("Height", 900, form.Height);

				tabPageMock.VerifyAll();
			}
			formMock.VerifyAll();
		}

		public void TestFormIsResizedByTabPageInMaximizedState()
		{
			using (var testForm = new ResizableFormForTest(Dummy))
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var resizeableTabPage = new ResizableTabPage { MinimumAutoSizedWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(600), MinimumAutoSizedHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(500) })
			{
				testForm.FormBorderStyle = FormBorderStyle.Sizable;

				testForm.Controls.Add(tabControl);
				tabControl.Dock = DockStyle.Fill;

				tabControl.TabPages.Add(tabPage);
				tabControl.TabPages.Add(resizeableTabPage);

				testForm.Show();
				Application.DoEvents();

				var expectedWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
				var expectedHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
				testForm.Width = expectedWidth;
				testForm.Height = expectedHeight;

				AssertEquals(FormWindowState.Normal, testForm.WindowState);
				AssertEquals(expectedWidth, testForm.Width);
				AssertEquals(expectedHeight, testForm.Height);
				AssertEquals(expectedWidth, testForm.RestoreBounds.Width);
				AssertEquals(expectedHeight, testForm.RestoreBounds.Height);
				AssertEquals(0, testForm.MinimumSize.Width);
				AssertEquals(0, testForm.MinimumSize.Height);

				testForm.WindowState = FormWindowState.Maximized;

				AssertEquals(FormWindowState.Maximized, testForm.WindowState);
				AssertNotEquals(expectedWidth, testForm.Width);
				AssertNotEquals(expectedHeight, testForm.Height);
				AssertEquals(expectedWidth, testForm.RestoreBounds.Width);
				AssertEquals(expectedHeight, testForm.RestoreBounds.Height);
				AssertEquals(0, testForm.MinimumSize.Width);
				AssertEquals(0, testForm.MinimumSize.Height);

				var maximizedSize = testForm.Size;

				tabControl.SelectedIndex = 1;

				AssertEquals(FormWindowState.Maximized, testForm.WindowState);
				AssertEquals(maximizedSize.Width, testForm.Width);
				AssertEquals(maximizedSize.Height, testForm.Height);
				Assert(testForm.RestoreBounds.Width > 600);
				Assert(testForm.RestoreBounds.Height > 500);
				AssertEquals(testForm.RestoreBounds.Width, testForm.MinimumSize.Width);
				AssertEquals(testForm.RestoreBounds.Height, testForm.MinimumSize.Height);

				testForm.WindowState = FormWindowState.Normal;

				AssertEquals(FormWindowState.Normal, testForm.WindowState);
				AssertNotEquals(maximizedSize.Width, testForm.Width);
				AssertNotEquals(maximizedSize.Height, testForm.Height);
				Assert(testForm.Width > 600);
				Assert(testForm.Height > 500);
				AssertEquals(testForm.Width, testForm.RestoreBounds.Width);
				AssertEquals(testForm.Height, testForm.RestoreBounds.Height);
				AssertEquals(testForm.RestoreBounds.Width, testForm.MinimumSize.Width);
				AssertEquals(testForm.RestoreBounds.Height, testForm.MinimumSize.Height);

				tabControl.SelectedIndex = 0;

				AssertEquals(FormWindowState.Normal, testForm.WindowState);
				AssertEquals(expectedWidth, testForm.Width);
				AssertEquals(expectedHeight, testForm.Height);
				AssertEquals(expectedWidth, testForm.RestoreBounds.Width);
				AssertEquals(expectedHeight, testForm.RestoreBounds.Height);
				AssertEquals(0, testForm.MinimumSize.Width);
				AssertEquals(0, testForm.MinimumSize.Height);
			}
		}

		class ResizableFormForTest : ZForm
		{
			public ResizableFormForTest(object dataSource) : base(dataSource) { }

			public override bool IsResizableByTabPageAllowed
			{
				get { return true; }
			}
		}

		class ResizableTabPage : ZTabPage
		{
			protected internal override bool IsAutoSized
			{
				get { return true; }
			}
		}

		#endregion

		public void TestTabPagesCollectionType()
		{
			AssertEquals("TabPages collection should be set to an instance of ZTabControl.TabPagesCollection Type", typeof(ZTabControl.TabPageCollection), TabControl.TabPages.GetType());
		}

		public void TestMinWidth()
		{
			Form.Controls.Add(TabControl);
			Form.Show();
			Application.DoEvents();
			AssertEquals("Min tab width set", 10, TabControl.MinTabWidth);
		}

		public void TestUpdateInitialTabImageCalled()
		{
			var tabPage1 = new TestBindingTabPage();
			TabControl.TabPages.Add(tabPage1);
			var tabPage2 = new TestBindingTabPage();
			TabControl.TabPages.Add(tabPage2);

			Form.Controls.Add(TabControl);
			Form.Show();
			Application.DoEvents();
			UserIdleWorker.Flush();
			AssertEquals("ZTabPage.UpdateInitialTabImage called", true, tabPage1.UpdateInitialTabImageCalled);
			AssertEquals("ZTabPage.UpdateInitialTabImage called only when the tab control handle was created", true, tabPage1.UpdateInitialTabImageCalled_TabControlParentVisible);
			AssertEquals("ZTabPage.UpdateInitialTabImage called", true, tabPage2.UpdateInitialTabImageCalled);
			AssertEquals("ZTabPage.UpdateInitialTabImage called only when the tab control handle was created", true, tabPage2.UpdateInitialTabImageCalled_TabControlParentVisible);
			tabPage1.UpdateInitialTabImageCalled = false;
			tabPage2.UpdateInitialTabImageCalled = false;

			TabControl.SelectedTab = tabPage1;
			TabControl.SelectedTab = tabPage2;
			TabControl.SelectedTab = tabPage1;
			UserIdleWorker.Flush();
			AssertEquals("ZTabPage.UpdateInitialTabImage not called a second time", false, tabPage1.UpdateInitialTabImageCalled);
			AssertEquals("ZTabPage.UpdateInitialTabImage not called a second time", false, tabPage2.UpdateInitialTabImageCalled);
		}

		public void TestDataSourceNullIsBoundIsReset()
		{
			using (var form = new ZForm())
			using (var tabPage = new TestBindingTabPage())
			{
				Form.Controls.Add(TabControl);
				TabControl.TabPages.Add(tabPage);

				tabPage.SetDataBinding(new Object(), string.Empty);

				AssertEquals(true, tabPage.IsBound);

				tabPage.ResetBinding();

				AssertEquals(false, tabPage.IsBound);
			}
		}

		public void TestBindSelectedBindingTabNullRef()
		{
			using (var form = new ZForm())
			using (var tabControl = new TestZTabControlForNullRef())
			using (var tabPage = new TestZBindingTabPageForNullRef())
			using (var tabPage2 = new TestZBindingTabPageForNullRef())
			{
				Form.Controls.Add(tabControl);
				tabControl.TabPages.Add(tabPage);
				tabControl.TabPages.Add(tabPage2);
				tabControl.SelectedTab = tabPage;
				Form.Show();

				AssertNoExceptionThrown(() =>
					{
						tabControl.BindSelectedBindingTab_Exposed();
					}
				);
			}
		}

		public void TestIExtendedControlMembers()
		{
			using (var tabControl = new ZTabControl())
			{
				CombineAssertions(() =>
				{
					var extendedControl = (IExtendedControl)tabControl;
					AssertType<ControlExtensionCollection>("Extensions", tabControl.Extensions);
					AssertSame("Host", tabControl, extendedControl.Host);
				});
			}
		}

		#region PlugIns On Sub-TabControls

		public void TestPlugInsOnUserControlWhichGetAddedOnLoadedGetTheirMenusAddedWhenGoToTab()
		{
			using (var form = new PlugInTestFormWithUserControl(Dummy))
			{
				form.Show();

				Assert("PlugIn not added yet", form.PlugInContainer.SubTabControl.PlugIns.Instances.Length == 0);

				form.TabControl.SelectedIndex = 1;
				var plugIn1 = (DummyPlugIn1)form.PlugInContainer.SubTabControl.PlugIns.Instances[0];
				Assert("Menu will be added", HasMenu(form, plugIn1.TopLevelMenu));
			}
		}

		public void TestTabPageAdded()
		{
			using (var control = new ZTabControl())
			{
				var added = false;
				var page = new ZTabPage();
				page.Added += delegate
				{ added = true; };
				control.Controls.Add(page);
				Assert(added);
			}
		}

		public void TestPlugInsGetTheirMenusAdded()
		{
			using (var form = new PlugInTestForm(Dummy))
			{
				form.Show();
				Application.DoEvents();

				var plugIn1 = (DummyPlugIn1)form.SubTabControl.PlugIns.Instances[0];
				var plugIn2 = (DummyPlugIn2)form.SubTabControl.PlugIns.Instances[1];
				var plugIn3 = (DummyPlugIn3)form.SubTabControl.PlugIns.Instances[2];

				Assert("Menu visible", HasMenu(form, plugIn1.TopLevelMenu));
				Assert("Menu visible", HasMenu(form, plugIn2.TopLevelMenu));
				Assert("Menu visible", HasMenu(form, plugIn3.TopLevelMenusInternal[0]));
				Assert("Menu visible", HasMenu(form, plugIn3.TopLevelMenusInternal[1]));

				form.TabControl.SelectedIndex = 1;

				Assert("Menu visible", HasMenu(form, plugIn1.TopLevelMenu));
				Assert("Menu visible", HasMenu(form, plugIn2.TopLevelMenu));
				Assert("Menu visible", HasMenu(form, plugIn3.TopLevelMenusInternal[0]));
				Assert("Menu visible", HasMenu(form, plugIn3.TopLevelMenusInternal[1]));
			}
		}

		public void TestPlugInsGetTheirMenusShownEvenIfTheyAreOnTheFirstTab()
		{
			using (var form = new PlugInTestForm(Dummy))
			{
				form.TabControl.TabPages.Remove(form.TabControl.TabPages[0]);
				form.Show();

				var plugIn1 = (DummyPlugIn1)form.SubTabControl.PlugIns.Instances[0];
				Assert("Menu visible", HasMenu(form, plugIn1.TopLevelMenu));
			}
		}

		static bool HasMenu(ZForm form, MenuItem menu)
		{
			return form.Menu.MenuItems.Contains(menu) && menu.Visible;
		}

		public void TestPlugInsOnSubTabPageGetCreated()
		{
			using (var form = new PlugInTestForm(Dummy))
			{
				form.Show();
				AssertEquals("Lazy created.", 0, form.SubTabControl.TabCount);

				form.TabControl.SelectedIndex = 1;
				AssertEquals("All there.", 3, form.SubTabControl.TabCount);
				AssertEquals("All there", 3, form.SubTabControl.PlugIns.Instances.Length);
			}
		}

		public void TestAddPlugInTabPagesWithIndex()
		{
			using (var form = new PlugInlessTestForm(Dummy))
			{
				var normalTabPage = new ZTabPage();
				normalTabPage.Text = "Normal";
				form.SubTabControl.TabPages.Add(normalTabPage);
				form.SubTabControl.PlugIns.AddPlugInAtTabPageIndex(DummyControllerIDs.Dummy1, 0);
				form.Show();
				form.TabControl.SelectedIndex = 1;
				Application.DoEvents();
				AssertEquals("First tab page should be plug in", "PlugIn1", form.SubTabControl.TabPages[0].Text);
				AssertEquals("Second tab page should be normal tab page", normalTabPage, form.SubTabControl.TabPages[1]);
			}
		}

		public void TestPlugInsGetBound()
		{
			using (var form = new PlugInTestForm(Dummy))
			{
				form.Show();
				Application.DoEvents();
				form.TabControl.SelectedIndex = 1;

				var plugIn1 = (DummyPlugIn1)form.SubTabControl.PlugIns.Instances[0];
				var plugIn2 = (DummyPlugIn2)form.SubTabControl.PlugIns.Instances[1];

				AssertNotNull("PlugIn1 is Visible", plugIn1.fUserControl);
				AssertNull("PlugIn2 not Visible", plugIn2.fUserControl);

				var dummy1 = (DummyBusinessObject)plugIn1.BusinessEntity;
				var dummy2 = (DummyBusinessObject)plugIn2.BusinessEntity;

				dummy1.Z0_Description = "HIFROMPLUGIN1";
				dummy2.Z0_Description = "HIFROMPLUGIN2";
				Application.DoEvents();
				AssertEquals("Bound values copied", dummy1.Z0_Description, ((DummyPlugIn1.TestUserControl)plugIn1.UserControl).TextBox.Text);

				plugIn2.SelectTabPage();
				AssertNotNull("PlugIn1 is Visible", plugIn2.fUserControl);
				AssertEquals("Bound values copied", dummy2.Z0_Description, ((DummyPlugIn2.TestUserControl)plugIn2.UserControl).TextBox.Text);
			}
		}

		public void TestBindTabAndShowNotifications_BindsNotifications()
		{
			using (var form = new ZTestForm(Dummy))
			{
				var tabPage1 = new ZTabPage { Text = "TabPage1" };
				form.TabControl.TabPages.Add(tabPage1);
				var tabPage2 = new ZTabPage { Text = "TabPage2" };
				form.TabControl.TabPages.Add(tabPage2);

				var textBox = new ZTextBox { BindTo = DummyBizoSchema.Z0_Description.Name };
				tabPage2.Controls.Add(textBox);

				using (Dummy.SuspendValidationTesting())
				{
					var info = Dummy.ZPropertyInfoHash[DummyBizoSchema.Z0_Description.Name];
					Dummy.Z0_Description = "error";

					form.Show();
					Application.DoEvents();
					form.TabControl.SelectedTab = tabPage2;

					var extension = textBox.Extensions.Get<INotificationExtension>();
					Assert("Notifications should be queried", extension.Notifications.HasErrors());
				}
			}
		}

		public void TestGetTopLevelBusinessEntityForPlugIns()
		{
			using (var form = new DifferentBOSubTabTestForm(Dummy))
			{
				var plugIn = form.SubTabControl.PlugIns.Instances[0];
				AssertEquals("Should be different from form's top level object",
					typeof(DifferentBOSubTabTestForm.DifferentParentTabControl.SomeOtherBusinessObject), ((DummyPlugIn1)plugIn).HostBusinessEntity.GetType());
			}
		}

		#region Helper Classes

		class PlugInTestForm : PlugInlessTestForm
		{
			public PlugInTestForm(BusinessObject bO)
				: base(bO)
			{
				SubTabControl.PlugIns.Add(DummyControllerIDs.Dummy1);
				SubTabControl.PlugIns.Add(DummyControllerIDs.Dummy2);
				SubTabControl.PlugIns.Add(DummyControllerIDs.Dummy3);
			}
		}

		class PlugInlessTestForm : ZTestForm
		{
			public PlugInlessTestForm(BusinessObject bO)
				: base(bO)
			{
				var junkPage = new ZTabPage();
				junkPage.Text = "StandardTabPage";
				TabControl.TabPages.Insert(junkPage, 0);

				var tabPage = new ZTabPage();
				tabPage.Text = "IHaveASubTab";
				SubTabControl = GetNewSubTabControl();
				tabPage.Controls.Add(SubTabControl);
				TabControl.TabPages.Insert(tabPage, 1);
			}

			protected virtual ZTabControl GetNewSubTabControl()
			{
				return new ZTabControl();
			}

			public ZTabControl SubTabControl;
		}

		class DifferentBOSubTabTestForm : PlugInTestForm
		{
			public DifferentBOSubTabTestForm(BusinessObject bO)
				: base(bO)
			{
			}

			internal class DifferentParentTabControl : ZTabControl
			{
				protected override IBusiness GetTopLevelBusinessEntityForPlugIns()
				{
					if (TopBO == null)
					{
						TopBO = new SomeOtherBusinessObject();
					}

					return TopBO;
				}

				public class SomeOtherBusinessObject : NonPersistentBusinessObject
				{
				}

				IBusiness TopBO;
			}

			protected override ZTabControl GetNewSubTabControl()
			{
				return new DifferentParentTabControl();
			}
		}

		class PlugInTestFormWithUserControl : ZTestForm
		{
			public PlugInContainerControl PlugInContainer;

			public PlugInTestFormWithUserControl(BusinessObject bO)
				: base(bO)
			{
				PlugInContainer = new PlugInContainerControl();

				var junkPage = new ZTabPage();
				junkPage.Text = "Some junk";
				TabControl.TabPages.Insert(junkPage, 0);

				var tabPage = new ZTabPage();
				tabPage.Text = "IHaveASubTab";
				tabPage.Controls.Add(PlugInContainer);
				TabControl.TabPages.Insert(tabPage, 1);

				Load += new EventHandler(PlugInTestFormWithUserControl_Load);
			}

			internal class PlugInContainerControl : ZUserControl
			{
				public ZTabControl SubTabControl;

				public PlugInContainerControl()
				{
					SubTabControl = new ZTabControl();
					Controls.Add(SubTabControl);
				}

				protected override void OnLoad(EventArgs e)
				{
					SubTabControl.PlugIns.Add(DummyControllerIDs.Dummy1);
					base.OnLoad(e);
				}
			}

			public bool LoadEventFired;

			void PlugInTestFormWithUserControl_Load(object sender, EventArgs e)
			{
				LoadEventFired = true;
			}
		}

		#endregion

		#endregion

		#region OnBindingTab / OnBoundTab

		public void TestDelayBinding()
		{
			var tabControl = new ZTabControl();
			var tabPageWithDelayBind = new TestBindingTabPage();
			tabPageWithDelayBind.DelayBindingEnabled = true;
			var tabPageWithoutDelayBind = new TestBindingTabPage();
			tabPageWithoutDelayBind.DelayBindingEnabled = false;

			tabControl.TabPages.Add(tabPageWithDelayBind);
			tabControl.TabPages.Add(tabPageWithoutDelayBind);
			Form.Controls.Add(tabControl);
			Form.Show();
			tabControl.SelectedTab = tabPageWithDelayBind;
			tabControl.SelectedTab = tabPageWithoutDelayBind;

			AssertEquals("Without delay should be bound", true, tabPageWithoutDelayBind.IsBound);
			AssertEquals("With delay should not be bound", false, tabPageWithDelayBind.IsBound);
		}

		public void TestDefaultDelayBinding()
		{
			using (ZBindingTabPage tabPage = new TestBindingTabPage())
			{
				AssertEquals("default delay binding", false, tabPage.DelayBinding);
			}
		}

		#endregion

		#region TestSetupPlugInMenus

		public void TestSetupPlugInMenus()
		{
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			using (var tabControl = new ZTabControl())
			{
				Assert("Plugin menus are not setup initially", !tabControl.PlugInMenusSetup);

				form.Controls.Add(tabControl);
				Application.DoEvents();

				Assert("Plugin menus are setup in OnParentChanged", tabControl.PlugInMenusSetup);

				tabControl.PlugIns.Add(DummyControllerIDs.Dummy1);

				Assert("Plugin menus setup flag is reset by new plugin", !tabControl.PlugInMenusSetup);

				form.Show();
				Application.DoEvents();

				Assert("Plugin menus are setup in OnHandleCreated", tabControl.PlugInMenusSetup);
			}
		}

		#endregion

		#region Test Classes

		class TestTabControl : ZTabControl
		{
			public IZSecurity Security;

			protected internal override IZSecurity GetSecurity()
			{
				return Security;
			}
#if !WINZOR
			public int MinTabWidth
			{
				get { return minTabWidth; }
			}
			int minTabWidth;

			protected override void WndProc(ref Message m)
			{
				base.WndProc(ref m);
				if (m.Msg == SafeNativeMethods.TCM_SETMINTABWIDTH)
				{
					unchecked
					{
						minTabWidth = m.LParam.ToInt32();
					}
				}
			}
#else
			public int MinTabWidth => WinzorMinTabWidth;
#endif
		}

		sealed class TestBindingTabPage : ZBindingTabPage
		{
			public bool? DelayBindingEnabled;
			internal override bool DelayBinding
			{
				get { return DelayBindingEnabled ?? base.DelayBinding; }
			}

			public bool UpdateInitialTabImageCalled;
			public bool UpdateInitialTabImageCalled_TabControlParentVisible;
			protected override void UpdateInitialTabImageCore()
			{
				base.UpdateInitialTabImageCore();
				if (!UpdateInitialTabImageCalled)
				{
					UpdateInitialTabImageCalled = true;
					UpdateInitialTabImageCalled_TabControlParentVisible = Parent != null && Parent.Parent != null && Parent.Parent.Visible;
				}
			}
		}

		#endregion

		#region Implementation

		TestTabControl TabControl
		{
			get
			{
				if (tabControl == null)
				{
					tabControl = new TestTabControl();
				}
				return tabControl;
			}
		}
		TestTabControl tabControl;

		ZChildForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZChildForm(Dummy);
				}
				return form;
			}
		}
		ZChildForm form;

		protected override bool IsDragDropHandledByEDocs
		{
			get { return true; }
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (tabControl != null)
			{
				tabControl.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion

		#region NullRefTest
		class TestZBindingTabPageForNullRef : ZBindingTabPage
		{
			protected override void SetDataBindingCore(object dataSource, string dataMember)
			{
				(this.Parent as TabControl).TabPages.Clear();
			}

			protected override object DataSource
			{
				get
				{
					return dataSource;
				}
			}

			readonly DataTable dataSource = new DataTable();
		}

		class TestZTabControlForNullRef : ZTabControl
		{
			public void BindSelectedBindingTab_Exposed()
			{
				this.BindSelectedBindingTab();
			}
		}
		#endregion

#if !WINZOR
		#region InvalidOperationExceptionOnHandleCreated

		public void TestHandleCreatedInOnHandleCreated()
		{
			using (var tabControl = new ZTabControl())
			{
				AssertEquals(false, tabControl.IsHandleCreated);
				typeof(TabControl).GetMethod("OnHandleCreated", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(tabControl, new object[] { new EventArgs() });
				AssertEquals(true, tabControl.IsHandleCreated);
			}
		}

		public void TestErrorReportForInvalidOperationExceptionOnHandleCreated()
		{
			var errorReporterMock = new Mock<IErrorReporter>();
			using var tabControl = new ZTabControlInvalidOperationExceptionOnHandleCreated();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				AssertExceptionThrown<Win32Exception>(() => tabControl.CreateControl());
				errorReporterMock.Verify(reporter => reporter.Report("InvalidOperationException_OnHandleCreated", It.Is<string>(str => str.Contains(@"Message: Invoke or BeginInvoke cannot be called on a control until the window handle has been created.
Selected Control Path:  (ZTabControlInvalidOperationExceptionOnHandleCreated)
Select Tab: 
IsHandleCreated: False
IsDisposed: False
IsDisposing: False
Invoke Required: False
Visible: True
TerminalService.IsCitrix: False, TerminalService.IsRemoteAppSession: False, TerminalService.IsWTSSession: False
TerminalService.LastWin32Error: 0
DestroyHandleStackTrace: 
")), It.IsAny<InvalidOperationException>()), Times.Once);
			}
		}

		class ZTabControlInvalidOperationExceptionOnHandleCreated : ZTabControl
		{
			protected override void OnHandleCreated(EventArgs e)
			{
				DestroyHandle();
				base.OnHandleCreated(e);
			}
		}

		#endregion
#endif

	}
}
