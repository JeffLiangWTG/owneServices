using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec;

namespace Enterprise.Customs.CH.Business;

public class EdecNotificationDataProvider : IEdecNotification
{
	public static IEnumerable<EdecNotificationDataProvider> NewCollection(CusEntryLine entryLine)
	{
		return entryLine?.RandomLine.NotifyCustomsOffices.Cast<NotifyCustomsOffice>().Select(notifyOffice => new EdecNotificationDataProvider(notifyOffice))
			?? Enumerable.Empty<EdecNotificationDataProvider>();
	}

	EdecNotificationDataProvider(NotifyCustomsOffice notifyCustomsOffice)
	{
		this.notifyCustomsOffice = Argument.NotNull(notifyCustomsOffice, nameof(notifyCustomsOffice));
	}
	readonly NotifyCustomsOffice notifyCustomsOffice;

	public string NotificationCode => notifyCustomsOffice.CY_Data;
}
