using System.Windows.Forms;
using CargoWise.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[CodeAlive("Will be used in the future")]
	sealed class ZDateTimeOffsetEditControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			var dateEdit = (ZDateTimeOffsetEdit)control;
			TestKeyStrokeHelper.SendKeyToControl(dateEdit.DateTextBox, Keys.F5);
		}
	}
}
