using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging;

public interface IETHeaderControlResult
{
	ZDate DateLimitOfArrivalNotification { get; }
	ZDate DateLimitForTheExitFromEC { get; }
}

