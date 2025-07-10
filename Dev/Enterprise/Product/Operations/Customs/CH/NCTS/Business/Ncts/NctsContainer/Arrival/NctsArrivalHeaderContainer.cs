using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsArrivalHeaderContainer : EU.NCTS.Business.NctsArrivalHeaderContainer
{
	public NctsArrivalHeaderContainer(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public NctsHeader NctsHeader => (NctsHeader)NctsArrival;

	public override bool ReadOnly
	{
		get => base.ReadOnly || (NctsHeader?.ArrivalMovementHeader.BM_NoChangesToReport ?? false) || (NctsHeader?.ArrivalMovementHeader.IsUnloadingRemarksReadOnly ?? false);
	}

	public new CusSealCollection Seals => (CusSealCollection)base.Seals;

	protected override EU.NCTS.Business.CusSealCollection GetAdditionalSealsCore() => new CusSealCollection(this);

	protected override Type CusSealTypeCore =>  typeof(CusSeal);

	public new IEnumerable<CusSeal> SealsForMessaging => Seals.Cast<CusSeal>().Where(s => s.BK_UnloadingState != NctsUnloadedStateList.Codes.DAM);

	protected override void EnableOrDisableSealsEditCore()
	{
	}
}
