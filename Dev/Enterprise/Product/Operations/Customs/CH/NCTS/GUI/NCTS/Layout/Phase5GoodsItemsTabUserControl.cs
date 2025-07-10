using System;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class Phase5GoodsItemsTabUserControl : EU.NCTS.GUI.Phase5GoodsItemsTabUserControl
{
	NctsDepartureMovementHeader MovementHeader => DataSource?.MovementHeader as NctsDepartureMovementHeader;

	ZTabPage restrictionsTabPage;

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		restrictionsTabPage = GoodsItemTabControl.GetTabPage(nameof(RestrictionsTabPage));
		SetTabPagesVisibility();
	}

	protected override void OnCurrentDataItemChanging(EventArgs e)
	{
		base.OnCurrentDataItemChanging(e);
		UnhookEvents();
	}

	protected override void OnCurrentDataItemChanged(EventArgs e)
	{
		base.OnCurrentDataItemChanged(e);
		HookEvents();
		SetTabPagesVisibility();
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

	void BM_InBondEntryType_ValueChanged(object sender, EventArgs e) => SetTabPagesVisibility();

	void SetTabPagesVisibility()
	{
		var movementHeader = DataSource?.MovementHeader as NctsDepartureMovementHeader;
		if (GoodsItemSupplyChainActorsTabPage != null)
		{
			GoodsItemSupplyChainActorsTabPage.TabVisible = !(movementHeader?.IsNationalTransitSwitzerland ?? false);
		}
		if (restrictionsTabPage != null)
		{
			restrictionsTabPage.TabVisible = movementHeader?.IsNationalTransitSwitzerland ?? false;
		}
	}
}
