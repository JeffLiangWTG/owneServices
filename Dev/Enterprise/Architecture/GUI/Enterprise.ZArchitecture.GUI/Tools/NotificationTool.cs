using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.DevTools
{
	class NotificationTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public string Name
		{
			get { return "ZNotifications Form"; }
		}

		public bool AddAsButton
		{
			get { return false; }
		}

		public void Show(Form form)
		{
			var zWinForm = form as ZForm;
			if (zWinForm != null)
			{
				zWinForm.ShowZNotificationForm();
			}
		}
	}
}