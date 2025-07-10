using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZMessageErrorNotificationIcon : ZNotificationIcon
	{
		#region Constructors

		public ZMessageErrorNotificationIcon(IEnumerable<INotification> notifications)
			: base(notifications)
		{
		}

		#endregion

		#region Overrides

		public override string AlternateText => Res.GetString("54FDD44D-9D41-48C9-B58A-71923B6103B0", "Message Error");

		protected override string GetResourceName() => "MessageError.ico";

		protected override IEnumerable<INotification> GetNotificationMessages() => Notifications.GetMessageErrors();

		#endregion
	}
}
