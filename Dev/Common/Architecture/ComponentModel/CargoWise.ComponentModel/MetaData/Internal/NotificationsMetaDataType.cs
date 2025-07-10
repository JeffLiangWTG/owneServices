
namespace CargoWise.ComponentModel
{
	internal class NotificationsMetaDataType : MetaDataType
	{
		public NotificationsMetaDataType()
			: base(MetaDataTypes.Notifications, typeof(NotificationCollection), NotificationCollection.Empty)
		{
		}

		public override bool AllowReintroduceMember
		{
			get { return false; }
		}
	}
}
