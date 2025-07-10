using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZErrorNotificationIcon : ZNotificationIcon
	{
		#region Constructors

		public ZErrorNotificationIcon(IEnumerable<INotification> notifications)
			: base(notifications)
		{
		}

		#endregion

		#region Overrides

		public override string AlternateText => Res.GetString("6353C76A-A5F2-46A6-84EF-12F478081A0F", "Error");

		protected override string GetResourceName() => "Error.ico";

		protected override IEnumerable<INotification> GetNotificationMessages() => Notifications.GetErrors();

		#endregion
	}
}
