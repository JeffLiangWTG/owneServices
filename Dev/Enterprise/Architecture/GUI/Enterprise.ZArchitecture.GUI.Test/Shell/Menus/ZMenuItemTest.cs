using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.GUI.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZMenuItemTest : TestCase
	{
		public void TestActionSourceCode()
		{
			var menuItem = new ZMenuItem((NoResString)"&New");
			AssertEquals("New", menuItem.ActionSourceCode);
		}

		public void TestProcessTemplateValidationManager()
		{
			var count = 0;
			using var form = new TestZForm(Mock.Of<IBusiness>());
			var menuItem = (ZMenuItem)form.Menu.MenuItems[0];
			var mockDialogService = new Mock<IProcessTemplateValidationManager>();
			mockDialogService.Setup(x => x.Validate(It.IsAny<ZString>(), It.IsAny<IBusiness>(), It.IsAny<Action>())).Callback(() =>
			{
				count++;
			});
			using var substitute = ObjectFactory.Substitute(mockDialogService.Object);
			menuItem.PerformClick();
			AssertEquals(1, count);
		}

		public void TestIMenuItemParent_BeforeClick()
		{
			using (var form = new ZChildForm())
			using (var control = new DummyParentControl())
			{
				control.ContextMenu = new ContextMenu();

				Action assertions = null;
				var menuItem = new ZMenuItem("Hello world");
				menuItem.Click += (o, e) => assertions();
				menuItem.Shortcut = Shortcut.CtrlP;

				control.ContextMenu.MenuItems.Add(menuItem);

				form.Show();
				Application.DoEvents();

				assertions = () => AssertEquals("BeforeProcessingMenuItem should be invoked before the menu item is clicked", 1, control.BeforeProcessingMenuItemCount);
				KeySender.SendKeyDownToProcessCmdKey(control, (int)(Keys.Control | Keys.P));

				assertions = () => AssertEquals("BeforeProcessingMenuItem should be invoked each time the menu item is clicked", 2, control.BeforeProcessingMenuItemCount);
				KeySender.SendKeyDownToProcessCmdKey(control, (int)(Keys.Control | Keys.P));
			}
		}

		public void TestMenuItems_DontSwallowUnhandledExceptions()
		{
			using (var form = new TestZForm())
			{
				var menuItem = form.Menu.MenuItems[0] as ZMenuItem;
				AssertNotNull(menuItem);
				menuItem.Click += (o, e) => throw new Exception("");
				AssertExceptionThrown<Exception>(() => menuItem.PerformClick());
			}
		}

		public void TestMenuItems_UseZFormForSaveExceptionHandling()
		{
			using (var form = new TestZForm())
			{
				var menuItem = form.Menu.MenuItems[0] as ZMenuItem;
				AssertNotNull(menuItem);
				menuItem.Click += (o, e) => throw new ZCannotSaveException("Error message", "");
				AssertNoExceptionThrown(() => menuItem.PerformClick());
				AssertEquals("Error message", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class DummyParentControl : ZUserControl, IMenuParent
		{
			public int BeforeProcessingMenuItemCount { get; private set; }

			public void BeforeProcessingMenuItem() => BeforeProcessingMenuItemCount++;
		}
	}
}
