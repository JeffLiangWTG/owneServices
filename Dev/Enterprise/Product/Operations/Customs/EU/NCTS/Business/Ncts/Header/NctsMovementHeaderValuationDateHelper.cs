using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business;

public static class NctsMovementHeaderValuationDateHelper
{
	public static void SetValuationDate(NctsDepartureMovementHeader movementHeader, bool isAmending)
	{
		Argument.NotNull(movementHeader, nameof(movementHeader));
		if (!movementHeader.IsPhase5)
		{
			throw new ArgumentException("Only Phase 5 is supported", nameof(movementHeader));
		}

		if (!isAmending || movementHeader.BM_ValuationDate.IsEmpty)
		{
			movementHeader.BM_ValuationDate = ZDateTime.Now;
		}
	}
}
