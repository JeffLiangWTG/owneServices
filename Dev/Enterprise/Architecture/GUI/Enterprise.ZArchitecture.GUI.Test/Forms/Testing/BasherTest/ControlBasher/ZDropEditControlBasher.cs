using System.Windows.Forms;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDropEditControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			var dropEdit = (ZDropEdit)control;

			TestKeyStrokeHelper.SendKeyToControl(dropEdit.CodeBox, Keys.F4);
			TestKeyStrokeHelper.SendKeyToControl(dropEdit, Keys.Down);
			TestKeyStrokeHelper.SendKeyToControl(dropEdit.CodeBox, Keys.F4);

			// this test is flawed
		}
	}
}
