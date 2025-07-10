using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	class ZPluginTest : TestCaseWithDummy
	{
		[ExpectNoExceptions]
		public void TestUpdateTabPageMinimumAutoSized()
		{
			var plugInMock = new Mock<ZPlugIn>(new object[] { Dummy }) { CallBase = true };
			plugInMock.Setup(o => o.GetNewUserControl()).Returns(new Control());
			plugInMock.SetupGet(o => o.HasUserControl).Returns(true);
			using (var plugIn = plugInMock.Object)
			{
				var tabPagePlugInMock = new Mock<ZTabPagePlugIn>(new object[] { plugIn }) { CallBase = true };
				tabPagePlugInMock.SetupGet(o => o.IsAutoSized).Returns(false);
				var tabPagePlugIn = tabPagePlugInMock.Object;
				plugInMock.Setup(o => o.GetTabPage()).Returns(tabPagePlugIn);
				plugIn.Setup();
				plugIn.SetupUserControl();
			}
			plugInMock.Verify(o => o.UpdateTabPageMinimumAutoSized(), Times.Never);

			plugInMock = new Mock<ZPlugIn>(new object[] { Dummy }) { CallBase = true };
			plugInMock.Setup(o => o.GetNewUserControl()).Returns(new Control());
			plugInMock.SetupGet(o => o.HasUserControl).Returns(true);
			using (var plugIn = plugInMock.Object)
			{
				var tabPagePlugInMock = new Mock<ZTabPagePlugIn>(new object[] { plugIn }) { CallBase = true };
				tabPagePlugInMock.SetupGet(o => o.IsAutoSized).Returns(true);
				var tabPagePlugIn = tabPagePlugInMock.Object;
				plugInMock.Setup(o => o.GetTabPage()).Returns(tabPagePlugIn);
				plugIn.Setup();
				plugIn.SetupUserControl();
			}
			plugInMock.Verify(o => o.UpdateTabPageMinimumAutoSized());
		}

		public void TestDiscardCurrentUserControl()
		{
			using (var testForm = new TestPlugInForm2(Dummy))
			{
				testForm.PlugIns.Add(DummyControllerIDs.Dummy3);
				var plugIn = testForm.PlugIns.Instances[1] as DummyPlugIn3;
				plugIn.SetupUserControl();
				AssertEquals("UserControl should be instantiated as TestUserControl", typeof(DummyPlugIn3.TestUserControl), plugIn.UserControl.GetType());
				var userControl = (DummyPlugIn3.TestUserControl)plugIn.UserControl;
				var userControlWeakReference = new WeakReference(userControl);
				AssertEquals(userControl, plugIn.UserControl);
				UserIdleWorker.Flush();
				AssertEquals(true, plugIn.TabPage.Controls.Contains(userControl));
				AssertEquals(false, userControl.IsDisposed);
				plugIn.DiscardCurrentUserControl();

				AssertEquals(false, plugIn.fIsSetup);
				AssertEquals(null, plugIn.fUserControl);
				AssertEquals(false, plugIn.TabPage.Controls.Contains(userControl));
				AssertEquals(true, userControl.IsDisposed);
			}
		}

		public void TestHasUserControlInternal()
		{
			using (var plugIn = new DummyPlugIn1(Dummy))
			{
				plugIn.HasUserControlExposed = true;
				AssertEquals("HasUserControlInternal", true, plugIn.HasUserControlInternal);
				plugIn.HasUserControlExposed = false;
				AssertEquals("HasUserControlInternal", false, plugIn.HasUserControlInternal);
			}
		}

		public void TestShouldPlugInGUIAndBusinessEntityBeCreatedDefault()
		{
			using (var plugIn = new DummyPlugIn1(Dummy))
			{
				AssertEquals("ShouldPlugInGUIAndBusinessEntityBeCreated()", true, plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated());
			}
		}

		public void TestShouldPlugInGUIAndBusinessEntityBeCreated_WhenSecurityNotGranted()
		{
			using (var plugIn = new DummyPlugIn1(Dummy))
			{
				SecurityCheckpoint checkPoint = new DummyCheckPointWithSecuritySet(false);
				plugIn.SetSecurityCheckpoint(checkPoint, null);
				AssertEquals("ShouldPlugInGUIAndBusinessEntityBeCreated() without security granted", false, plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated());
			}
		}

		public void TestShouldPlugInGUIAndBusinessEntityBeCreated_DontAquireLicence()
		{
			using (var plugIn = new DummyPlugIn1(Dummy))
			{
				var licence = new Licences();
				licence.Core.AllowUsageForTest = false;
				plugIn.SetLicenceCheckPoint(licence.Core);
				AssertEquals("ShouldPlugInGUIAndBusinessEntityBeCreated() should not aquire licence", false, plugIn.LicenceCheckPointInternal.IsLoggedIn);
			}
		}

		public void TestQueryUserShouldPlugInGUIAndBusinessEntityBeCreatedDefault()
		{
			using (var plugIn = new DummyPlugIn1(Dummy))
			{
				AssertEquals("ShouldPlugInGUIAndBusinessEntityBeCreated()", true, plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated());
			}
		}

		public void TestShouldPluginDropdownMenuBeCreated_ShouldDefaultToTrue()
		{
			using (var plugin = new DummyPlugIn1(Dummy))
			{
				Assert(plugin.ShouldPluginDropdownMenuBeCreated());
			}
		}

		public void TestTopLevelMenu_PopupOrClick_ShouldNotQueryUserByDefault()
		{
			using (var plugin = new DummyPluginWhichThrowsOnUserQuery(Dummy))
			{
				plugin.TopLevelMenusInternal[0].PerformClick();
			}
		}

		class DummyPluginWhichThrowsOnUserQuery : DummyPlugIn1
		{
			public DummyPluginWhichThrowsOnUserQuery(IBusiness hostBusinessEntity)
				: base(hostBusinessEntity)
			{
			}

			protected internal override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
			{
				throw new InvalidOperationException("Should not query user by default when selecting top-level menu item");
			}

			protected internal override bool ShouldPluginDropdownMenuBeCreated()
			{
				Assert(true);
				return true;
			}
		}

		public void TestDefaultPlugInNotDisplayedMessage()
		{
			using (var plugIn = new DummyPlugIn1(Dummy))
			{
				AssertEquals("PlugInNotDisplayed", "PlugIn1 cannot be shown at this time.", plugIn.PlugInNotDisplayedMessage);
			}
		}

		[ExpectNoExceptions]
		public void TestPreSaveDialogsDoesNotRequireThePlugInToHaveABusinessEntity()
		{
			var mockBizO = new Mock<IBusiness>();
			var bizO = mockBizO.Object;

			var mock = new Mock<ZPlugIn>(bizO) { CallBase = true };
			var plugin = mock.Object;
			plugin.fIsSetup = true;
			plugin.fEnabled = true;
			using (plugin)
			{
				plugin.ShowPreSaveDialogs();
			}
		}

		[ExpectNoExceptions]
		public void TestShowPreSaveDialogsCallsBusinessObject()
		{
			var mockBizO = new Mock<IBusiness>();
			var bizO = mockBizO.Object;

			var mock = new Mock<ZPlugIn>(bizO) { CallBase = true };
			mock.Setup(o => o.GetBusinessEntityForPlugIn()).Returns(bizO);
			mock.SetupGet(o => o.HasUserControl).Returns(ZBool.False);

			var plugin = mock.Object;
			var loadedEntity = plugin.BusinessEntity;
			plugin.Setup();
			plugin.fEnabled = true;
			using (plugin)
			{
				mockBizO.SetupGet(o => o.CanContinueWithSave).Returns(true);
				plugin.ShowPreSaveDialogs();
				mockBizO.VerifyAll();
			}
		}

		public void TestShowPreSaveDialogsTranslatesBusinessObjectReturnValue()
		{
			var mockBizO = new Mock<IBusiness>();
			var bizO = mockBizO.Object;
			var mock = new Mock<ZPlugIn>(bizO) { CallBase = true };
			mock.Setup(o => o.GetBusinessEntityForPlugIn()).Returns(bizO);
			mock.SetupGet(o => o.HasUserControl).Returns(ZBool.False);

			var plugin = mock.Object;
			var loadedEntity = plugin.BusinessEntity;
			plugin.Setup();
			plugin.fEnabled = true;
			using (plugin)
			{
				mockBizO.SetupGet(o => o.CanContinueWithSave).Returns(true);
				AssertEquals(ContinueWithSave.Yes, plugin.ShowPreSaveDialogs());
				mockBizO.SetupGet(o => o.CanContinueWithSave).Returns(false);
				AssertEquals(ContinueWithSave.No, plugin.ShowPreSaveDialogs());
			}
		}

		[ExpectNoExceptions]
		public void TestShowPreSaveDialogsWhenInactive_WhenTrue()
		{
			TestShowPreSaveDialogsWhenInactive(true);
		}

		[ExpectNoExceptions]
		public void TestShowPreSaveDialogsWhenInactive_WhenFalse()
		{
			TestShowPreSaveDialogsWhenInactive(false);
		}

		void TestShowPreSaveDialogsWhenInactive(bool showPreSaveDialogsWhenInactive)
		{
			var mock = new Mock<ZPlugIn>(Mock.Of<IBusiness>()) { CallBase = true };
			mock.SetupGet(o => o.ShowPreSaveDialogsWhenInactive).Returns(showPreSaveDialogsWhenInactive);

			using (var plugin = mock.Object)
			{
				if (showPreSaveDialogsWhenInactive)
				{
					mock.Setup(o => o.ShowPreSaveDialogsCore()).Returns(ContinueWithSave.Yes);
				}
				plugin.ShowPreSaveDialogs();
				mock.Verify(o => o.ShowPreSaveDialogsCore(), showPreSaveDialogsWhenInactive ? Times.Once() : Times.Never());
			}
		}

		public void TestSetSecurityCheckpoint()
		{
			var mock = new Mock<ZPlugIn>(Mock.Of<IBusiness>()) { CallBase = true };

			using (var plugIn = mock.Object)
			{
				SecurityCheckpoint checkPoint = new DummyCheckPointWithSecuritySet(false);
				AssertNull("SecurityCheckpoint", plugIn.SecurityCheckpoint);
				SecurityCheckpoint checkPointEdit = new DummyCheckPointWithSecuritySet(true);
				AssertNull("SecurityCheckpoint", plugIn.SecurityCheckpointEdit);

				plugIn.SetSecurityCheckpoint(checkPoint, checkPointEdit);
				AssertEquals("SecurityCheckpoint", checkPoint, plugIn.SecurityCheckpoint);
				AssertEquals("SecurityCheckpoint", checkPointEdit, plugIn.SecurityCheckpointEdit);
			}
		}

		public void TestRegisterAsEditableUsesPropertyNotField()
		{
			using (var plugin = new DummyPlugIn1(Dummy))
			{
				AssertNull("Precondition: bizo should be null", plugin.fBusinessEntity);
				AssertEquals("Precondition: bizo should NOT be registered", false, plugin.IsBusinessObjectRegistered);

				plugin.RegisterPlugInAsEditable();
				AssertNotNull("Bizo should have been loaded", plugin.fBusinessEntity);
				AssertEquals("Bizo should have been registered", true, plugin.IsBusinessObjectRegistered);
			}
		}

		public void TestResetBusinessObjectAlsoUnregistersAsEditable()
		{
			using (var plugin = new DummyPlugIn1(Dummy))
			{
				AssertEquals("Precondition: bizo should NOT be registered", false, plugin.IsBusinessObjectRegistered);
				plugin.RegisterPlugInAsEditable();
				AssertEquals("Bizo should have been registered", true, plugin.IsBusinessObjectRegistered);

				plugin.ResetBusinessEntityToNullInternal();
				AssertEquals("Bizo should have been un-registered", false, plugin.IsBusinessObjectRegistered);
			}
		}
	}
}
