using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsCommonMovementHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var result = typeof(NctsCommonMovementHeader);
			if (row != null)
			{
				var type = new ZString(row[NctsCommonMovementHeader.Schema.BM_SubApplicationCode]);
				if (type == Common.EU.NctsMoveHeaderType.Codes.Departure)
				{
					result = new NctsDepartureMovementHeaderTypeDecider().GetTypeForLoad(row, factory);
				}
				else if (type == Common.EU.NctsMoveHeaderType.Codes.Arrival)
				{
					result = new NctsArrivalMovementHeaderTypeDecider().GetTypeForLoad(row, factory);
				}
				else if (type == Common.EU.NctsMoveHeaderType.Codes.Unloading)
				{
					result = new NctsUnloadingMovementHeaderTypeDecider().GetTypeForLoad(row, factory);
				}
			}
			return result;
		}

		public override Type GetTypeForNew() => typeof(NctsCommonMovementHeader);

		public override Type GetTypeForBinding() => typeof(NctsCommonMovementHeader);
	}
}
