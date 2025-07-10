using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridFindBoxTest : TestCaseWithFactory, DummyFilterGridModule.IDummyFilterGridModuleListener
	{
		public void TestShowEditForm()
		{
			DummyFilterGridModule.DummyFilterGridModuleListener = this;

			try
			{
				var info = new ZCodeFindBoxColumnStyleInfo();
				info.ModuleID = DummyModuleIDs.Dummy;

				using (var columnStyle = new ZCodeFindBoxColumnStyle(info))
				{
					var testFindBox = (ZGridFindBox)(columnStyle.EditControl);
					testFindBox.List = new DummyBusinessObjectCollection(Factory);

					KeySender.PostKeyDown(testFindBox.CodeBox, Keys.F3);
					Application.DoEvents();

					AssertNotNull("Module should be created by F3", module);
					AssertNotNull("Controller should be created by F3", module.LastController);
					AssertNotNull("Form should be created by F3", module.LastController.LastFormCreated);
				}
			}
			finally
			{
				DummyFilterGridModule.DummyFilterGridModuleListener = null;

				if (module != null)
				{
					module.LastController.LastFormCreated.Close();
					module.LastController.LastFormCreated = null;
				}
			}
		}

		public void TestShouldProcessCmdKey()
		{
			using (var columnStyle = new ZCodeFindBoxColumnStyle(new ZCodeFindBoxColumnStyleInfo()))
			{
				var m = new Message();
				ICustomKeyHandlingGridColumn customKeyHandlingGridColumn = columnStyle;

				Assert("No processing by default", !customKeyHandlingGridColumn.ShouldProcessCmdKey(ref m, Keys.F3));

				columnStyle.ReadOnly = true;

				Assert("Process for readonly column", customKeyHandlingGridColumn.ShouldProcessCmdKey(ref m, Keys.F3));

				columnStyle.ReadOnly = false;
				columnStyle.isCurrentCellReadOnlyForTest = true;

				Assert("Process for readonly cell", customKeyHandlingGridColumn.ShouldProcessCmdKey(ref m, Keys.F3));
			}
		}

		#region Implementation

		DummyFilterGridModule module;

		#region IDummyFilterGridModuleListener Members

		void DummyFilterGridModule.IDummyFilterGridModuleListener.SetReference(DummyFilterGridModule reference)
		{
			if (module == null)
			{
				module = reference; //should only be set once, because the module gets instantiated twice as when the form is shown, the module is instantiated again for licence checks.
			}
		}

		#endregion

		#endregion
	}
}
