using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class MessageSendingNotificationHelper : BaseMessageSendingNotificationHelper
	{
		public MessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		public override ZString GetNotifications()
		{
			var extraNotification = GetExtraMessageSendingNotification();
			if (!extraNotification.IsEmpty)
			{
				return extraNotification;
			}

			header.Validation.ValidateAMA_ManifestType();
			if (header.AMA_ManifestTypeInfo.HasErrors())
			{
				return ValidationConstants.MustHaveManifestType;
			}

			return ZString.Empty;
		}
	}
}
