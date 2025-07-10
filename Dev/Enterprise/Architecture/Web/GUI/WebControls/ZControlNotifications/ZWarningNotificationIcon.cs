using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZWarningNotificationIcon : ZNotificationIcon
	{
		#region Constructors

		public ZWarningNotificationIcon(IEnumerable<INotification> notifications)
			: base(notifications)
		{
		}

		#endregion

		#region Overrides

		public override string AlternateText => Res.GetString("6B7296DD-F415-488A-8106-52E045A889D1", "Warning");

		protected override string GetResourceName() => "Warning.ico";

		protected override IEnumerable<INotification> GetNotificationMessages() => Notifications.GetWarnings();

		#endregion
	}
}
