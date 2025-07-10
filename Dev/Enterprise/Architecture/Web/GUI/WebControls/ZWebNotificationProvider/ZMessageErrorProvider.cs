using System.Collections.Generic;
using System.Web.UI;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// MessageError provider on Web forms.
	/// </summary>
	public class ZMessageErrorProvider : ZWebNotificationProvider
	{
		public ZMessageErrorProvider(Control control, IEnumerable<INotification> notifications) : base(control, notifications)
		{
		}

		protected override string IconResourceName
		{
			get { return "MessageError.ico"; }
		}

		protected override IEnumerable<INotification> NotificationMessages
		{
			get { return Notifications.GetMessageErrors(); }
		}
	}
}
