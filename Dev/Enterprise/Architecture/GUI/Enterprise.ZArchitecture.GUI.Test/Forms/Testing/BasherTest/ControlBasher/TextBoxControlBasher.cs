using System.Windows.Forms;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TextBoxControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			TestKeyStrokeHelper.SendJunkKeyStrokesToControl((TextBox)control);
		}
	}
}
