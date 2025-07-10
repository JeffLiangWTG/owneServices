using System.Windows.Forms;
using CargoWise.Interop;

namespace Enterprise.ZArchitecture.GUI
{
	public class MenuFeedback
	{
		public void WndProc(Control form, ref Message m)
		{
			switch (m.Msg)
			{
				case WindowsMessage.WM_EXITMENULOOP:
					if (form.Cursor == Cursors.Cross)
					{
						form.Cursor = Cursors.Default;
					}
					break;

				case WindowsMessage.WM_MENUSELECT:
					if (TranslationFeedbackManager.InTranslationFeedbackMode())
					{
						form.Cursor = Cursors.Cross;
					}
					else if (form.Cursor == Cursors.Cross)
					{
						form.Cursor = Cursors.Default;
					}
					break;
			}
		}
	}
}
