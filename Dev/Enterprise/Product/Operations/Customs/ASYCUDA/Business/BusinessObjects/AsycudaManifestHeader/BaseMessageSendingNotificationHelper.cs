using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class BaseMessageSendingNotificationHelper
	{
		public BaseMessageSendingNotificationHelper(AsycudaManifestHeader header)
		{
			this.header = header;
		}
		protected readonly AsycudaManifestHeader header;

		public virtual ZString GetNotifications()
		{
			return ZString.Empty;
		}

		public virtual IEnumerable<ZString> GetConfirmations()
		{
			return Array.Empty<ZString>();
		}

		public ZString GetExtraMessageSendingNotification()
		{
			return GetExtraMessageSendingNotificationCore();
		}

		protected virtual ZString GetExtraMessageSendingNotificationCore()
		{
			return ZString.Empty;
		}
	}
}
