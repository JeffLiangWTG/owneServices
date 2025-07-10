using CargoWise.Types;
using Enterprise.Customs.IT.Messaging;

namespace Enterprise.Customs.IT.Business;

public class ETHeaderControlResultWrapper : IETHeaderControlResult
{
	public ZDate DateLimitOfArrivalNotification => ZDate.Empty;

	public ZDate DateLimitForTheExitFromEC => ZDate.Empty;
}
