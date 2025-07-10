using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDeparturePayInfo : EU.NCTS.Business.NctsDeparturePayInfo
{
	public NctsDeparturePayInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new NctsDepartureMovementHeader MoveHeader => (NctsDepartureMovementHeader)base.MoveHeader;
}
