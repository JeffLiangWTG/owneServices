using System.Collections.Generic;
using System.Web.UI;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Error provider on Web forms
	/// </summary>
	public class ZWarningProvider : ZWebNotificationProvider
	{
		public ZWarningProvider(Control ctrl, IEnumerable<INotification> notes)
			: base(ctrl, notes)
		{
		}

		protected override string IconResourceName
		{
			get { return "Warning.ico"; }
		}

		protected override IEnumerable<INotification> NotificationMessages
		{
			get { return Notifications.GetWarnings(); }
		}
	}
}
