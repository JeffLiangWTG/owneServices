using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class HumanReadableNotificationCollector : ZNotificationCollector
	{
		public HumanReadableNotificationCollector(IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude)
		: base(business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
		{
		}

		protected override INotification GetNotification(BusinessObject bizObj, INotification notification, string replacedMessage)
		{
			if (notification is PropertyNotification)
			{
				var name = bizObj.HumanReadableName;

				if (!string.IsNullOrEmpty(name))
				{
					replacedMessage = Res.GetString("HumanReadableNotificationCollector.FormatProperty", "[{0}] {1}", name, replacedMessage);
				}
			}

			return base.GetNotification(bizObj, notification, replacedMessage);
		}
	}
}
