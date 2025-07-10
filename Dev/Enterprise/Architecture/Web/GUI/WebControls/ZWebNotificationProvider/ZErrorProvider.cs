using System.Collections.Generic;
using System.Web.UI;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Error provider on Web forms
	/// </summary>
	public class ZErrorProvider : ZWebNotificationProvider
	{
		public ZErrorProvider(Control ctrl, IEnumerable<INotification> notes) : base(ctrl, notes)
		{
		}

		protected override string IconResourceName
		{
			get { return "Error.ico"; }
		}

		protected override IEnumerable<INotification> NotificationMessages
		{
			get { return Notifications.GetErrors(); }
		}
	}
}
