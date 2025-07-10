using System;
using System.Windows.Forms;
using static Enterprise.ZArchitecture.GUI.TranslationFeedbackManager;

namespace Enterprise.ZArchitecture.GUI
{
	public class DevInfoPopupManager : IDisposable
	{
		public DevInfoPopupManager(Control control, ClickMode clickMode = ClickMode.OnClick)
		{
		}

		public static void ShowDevelopInfoForm(Control control)
		{
		}

		public static bool InDevelopInformationMode() => false;

		public bool InMode => false;

		public bool HandleClick() => false;

		public void Dispose()
		{
		}
	}
}
