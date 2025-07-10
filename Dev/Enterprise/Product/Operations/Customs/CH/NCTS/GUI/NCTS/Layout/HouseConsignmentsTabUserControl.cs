using System;
using Enterprise.Customs.CH.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class HouseConsignmentsTabUserControl : EU.NCTS.GUI.HouseConsignmentsTabUserControl
{
	NctsDepartureMovementHeader MovementHeader => DataSource?.MovementHeader as NctsDepartureMovementHeader;

	protected override void OnCurrentDataItemChanging(EventArgs e)
	{
		base.OnCurrentDataItemChanging(e);
		UnhookEvents();
	}

	protected override void OnCurrentDataItemChanged(EventArgs e)
	{
		base.OnCurrentDataItemChanged(e);
		HookEvents();
		SetHouseConsignmentsSupplyChainActorsTabPageVisibility();
	}

	void HookEvents()
	{
		if (MovementHeader != null)
		{
			MovementHeader.BM_InBondEntryTypeInfo.ValueChanged += BM_InBondEntryType_ValueChanged;
		}
	}

	void UnhookEvents()
	{
		if (MovementHeader != null)
		{
			MovementHeader.BM_InBondEntryTypeInfo.ValueChanged -= BM_InBondEntryType_ValueChanged;
		}
	}

	void BM_InBondEntryType_ValueChanged(object sender, EventArgs e) => SetHouseConsignmentsSupplyChainActorsTabPageVisibility();

	void SetHouseConsignmentsSupplyChainActorsTabPageVisibility()
	{
		if (HouseConsignmentSupplyChainActorsTabPage != null)
		{
			HouseConsignmentSupplyChainActorsTabPage.TabVisible = !(MovementHeader?.IsNationalTransitSwitzerland ?? false);
		}
	}
}
