using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderControlResult
{
	[MessageLayout(Order = 0)]
	[MessageFieldDateDDMMYYRepresentation]
	public ZDate? DateLimitOfArrivalNotification => null;

	[MessageLayout(Order = 1)]
	[MessageFieldDateDDMMYYRepresentation]
	public ZDate? DateLimitForTheExitFromEC => null;
}
