using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public sealed class ConfirmationDialogDescriptor : NonPersistentBusinessObject
	{
		public ConfirmationDialogDescriptor(string actionName, string text, params ConfirmationNotification[] notifications)
		{
			ActionName = actionName;
			Text = text;
			ConfirmationNotifications = notifications;
		}

		public string ActionName { get; private set; }
		public ZString Text { get; private set; }
		public ICollection<ConfirmationNotification> ConfirmationNotifications { get; private set; }

		public ZDialogResult Result { get; set; }
	}
}
