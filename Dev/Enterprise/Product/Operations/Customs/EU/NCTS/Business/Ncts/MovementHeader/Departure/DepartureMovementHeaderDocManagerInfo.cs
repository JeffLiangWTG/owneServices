using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business;

class DepartureMovementHeaderDocManagerInfo : DocManagerInfo
{
	public DepartureMovementHeaderDocManagerInfo(NctsDepartureMovementHeader nctsDepartureMovementHeader) : base(nctsDepartureMovementHeader, Core.Constants.DocManagerCodes.NctsMoveHeader)
	{
	}
}
