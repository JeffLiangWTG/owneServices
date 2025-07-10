using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class BusinessObjectNotificationsViewerLine : NonPersistentBusinessObject
	{
		public BusinessObjectNotificationsViewerLine(BusinessObject bizObj)
			: base(bizObj.Factory)
		{
			PK = bizObj.PK;
		}

		public new ZGuid PK { get; }

		#region Properties

		#region HumanReadableColumn1

		public ZString HumanReadableColumn1
		{
			get => fHumanReadableColumn1;
			set => SetNonPersistentPropertyValue(HumanReadableColumn1Info, ref fHumanReadableColumn1, value);
		}
		ZString fHumanReadableColumn1;

		public ZPropertyInfo HumanReadableColumn1Info => GetZPropertyInfo(nameof(HumanReadableColumn1));

		#endregion

		#region HumanReadableColumn2

		public ZString HumanReadableColumn2
		{
			get => fHumanReadableColumn2;
			set => SetNonPersistentPropertyValue(HumanReadableColumn2Info, ref fHumanReadableColumn2, value);
		}
		ZString fHumanReadableColumn2;

		public ZPropertyInfo HumanReadableColumn2Info => GetZPropertyInfo(nameof(HumanReadableColumn2));

		#endregion

		#region NotificationType

		public ZString NotificationType
		{
			get => fNotificationType;
			set => SetNonPersistentPropertyValue(NotificationTypeInfo, ref fNotificationType, value);
		}
		ZString fNotificationType;

		public ZPropertyInfo NotificationTypeInfo => GetZPropertyInfo(nameof(NotificationType));

		#endregion

		#region NotificationMessage

		public ZString NotificationMessage
		{
			get => fNotificationMessage;
			set => SetNonPersistentPropertyValue(NotificationMessageInfo, ref fNotificationMessage, value);
		}
		ZString fNotificationMessage;

		public ZPropertyInfo NotificationMessageInfo => GetZPropertyInfo(nameof(NotificationMessage));

		#endregion

		#endregion
	}
}
