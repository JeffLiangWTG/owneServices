using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Main.Startup.Tools;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Tools.Testing
{
	[TestedType(typeof(HotKeyMonitorForm))]
	sealed class HotKeyMonitorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new HotKeyMonitorForm();
		}

		public void TestFormShouldNotMissComponents()
		{
			using (var form = (HotKeyMonitorForm)GetFormToBash())
			{
				AssertNotNull(form.Controls.Find("PNLButtons", true).Single());
				AssertNotNull(form.Controls.Find("SpanTimes", true).Single());
				AssertNotNull(form.Controls.Find("TxtKeyPressSpans", true).Single());
				AssertNotNull(form.Controls.Find("TxtAdditionalMsg", true).Single());
				AssertNotNull(form.Controls.Find("BtnCopy", true).Single());
				AssertNotNull(form.Controls.Find("BtnClear", true).Single());
				AssertNotNull(form.Controls.Find("BtnStart", true).Single());
				AssertNotNull(form.Controls.Find("OnlyShowProcessedKeysCheckBox", true).Single());
			}
		}

		static bool DummyProcessor(object sender, Keys key)
		{
			return true;
		}

		[GuiTest]
		[DeveloperOnlyTest]
		public void TestHotKeyMonitorFormOperations()
		{
			var hotKeyRegister = new HotkeyRegister();
			hotKeyRegister.RegisterHotKey(Keys.A, DummyProcessor, "The A key");

			var hotKeyMonitor = HotKeyMonitorProvider.GetHotKeyMonitor();

			var form1 = new Form();
			form1.Name = "Form 1";

			var form2 = new Form();
			form2.Name = "Form 2";

			var staff = Factory.New<IGlbStaff>();
			staff.GS_Code = "ABC";
			using (EnvProxy.Instance.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = (HotKeyMonitorForm)GetFormToBash())
			{
				form.Show();

				var checkbox = (ZCheckBox)form.Controls.Find("OnlyShowProcessedKeysCheckBox", true).Single();
				AssertEquals(false, checkbox.Checked);

				var btnStart = (ZButton)form.Controls.Find("BtnStart", true).Single();
				btnStart.PerformClick();
				AssertEquals(true, hotKeyMonitor.Enabled);

				hotKeyRegister.ProcessCmdKey(form1, Keys.A);
				hotKeyRegister.ProcessCmdKey(form1, Keys.B);
				Thread.Sleep(100);
				hotKeyRegister.ProcessCmdKey(form2, Keys.C);

				btnStart.PerformClick();
				AssertEquals(false, hotKeyMonitor.Enabled);

				AssertEquals(2, hotKeyMonitor.GetAllSpans().Count);
				checkbox.Checked = true;
				AssertEquals(1, hotKeyMonitor.GetFilteredSpans().Count);

				var additionalMsg = (ZTextBox)form.Controls.Find("TxtAdditionalMsg", true).Single();
				AssertEquals("You can add more details here...", additionalMsg.Text);

				var btnClear = (ZButton)form.Controls.Find("btnClear", true).Single();
				btnClear.PerformClick();
				AssertEquals(0, hotKeyMonitor.GetAllSpans().Count);
			}
		}
	}
}
