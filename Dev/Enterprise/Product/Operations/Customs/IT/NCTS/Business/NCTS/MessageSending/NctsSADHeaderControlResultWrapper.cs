using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSADHeaderControlResultWrapper : IETHeaderControlResult
{
	public NctsSADHeaderControlResultWrapper(NctsDepartureMovementHeader nctsMovementHeader)
	{
		this.nctsMovementHeader = Argument.NotNull(nctsMovementHeader, nameof(nctsMovementHeader));
	}
	readonly NctsDepartureMovementHeader nctsMovementHeader;

	public ZDate DateLimitOfArrivalNotification => nctsMovementHeader.BM_ExportDate.Date;

	public ZDate DateLimitForTheExitFromEC => ZDate.Empty;
}
