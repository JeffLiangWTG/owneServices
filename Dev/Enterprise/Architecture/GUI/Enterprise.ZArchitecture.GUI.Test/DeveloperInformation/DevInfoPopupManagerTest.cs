using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Core.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DevInfoPopupManagerTest : TestCase
	{
		public void TestDeveloperInformationMode()
		{
			using (var form = new TestZForm { Name = "Form1" })
			using (var panel = new ZPanel { Name = "Panel1" })
			using (var label = new ZLabel { Name = "Label1" })
			using (DevInfoPopupManager.EnableDeveloperInformationModeForTest())
			{
				form.Controls.Add(panel);
				panel.Controls.Add(label);

				using (var manager = new DevInfoPopupManager(label))
				{
					var p = Point.Empty;
					manager.control.GetType().InvokeMember("OnMouseMove", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null, manager.control, new object[] { new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, 0) });
					manager.control.GetType().InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null, manager.control, new object[] { new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, 0) });
					manager.control.GetType().InvokeMember("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null, manager.control, new object[] { new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, 0) });
					manager.control.GetType().InvokeMember("OnClick", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null, manager.control, new object[] { EventArgs.Empty });
					var message = ZFormModaliser.LastFormShownDialogForTest.Text;
					AssertEquals("Should get the developer information message", true, message.Contains("Control Information"));
				}
			}
		}
	}
}
