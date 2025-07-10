using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class InventorySealDataProvider : SealDataProvider
{
	public static IEnumerable<InventorySealDataProvider> NewCollection(IEnumerable<CusSeal> seals)
	{
		return seals?
			.Cast<CusSeal>()
			.Where(x => x.BK_UnloadingState.IsUnloadingStateNEWorMISorDIF())
			.Select(s => new InventorySealDataProvider(s));
	}

	InventorySealDataProvider(CusSeal seal) : base(seal.BK_SequenceNumber, seal.BK_SealNumber)
	{
		isMIS = seal.BK_UnloadingState == NctsUnloadedStateList.Codes.MIS;
		unloadingRemarks = seal.UnloadingRemarksText;
	}
	readonly bool isMIS;
	readonly string unloadingRemarks;

	public override string Identifier => isMIS ? null : base.Identifier;

	public override string UnloadingRemarkText => unloadingRemarks;
}
