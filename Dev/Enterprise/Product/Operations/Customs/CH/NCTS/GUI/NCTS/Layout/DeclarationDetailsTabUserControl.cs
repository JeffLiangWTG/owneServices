using System;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public partial class DeclarationDetailsTabUserControl : Phase5DeclarationDetailsTabUserControl
{
	public DeclarationDetailsTabUserControl()
	{
		InitializeComponent();
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
		SetTabPageVisibility();
	}

	void HookEvents()
	{
		if (DataSource?.MovementHeader is NctsDepartureMovementHeader movementHeader)
		{
			movementHeader.BM_InBondEntryTypeInfo.ValueChanged += BM_InBondEntryType_ValueChanged;
		}
	}

	void UnhookEvents()
	{
		if (DataSource?.MovementHeader is NctsDepartureMovementHeader movementHeader)
		{
			movementHeader.BM_InBondEntryTypeInfo.ValueChanged -= BM_InBondEntryType_ValueChanged;
		}
	}

	void BM_InBondEntryType_ValueChanged(object sender, EventArgs e) => SetTabPageVisibility();

	void SetTabPageVisibility()
	{
		if (DataSource?.MovementHeader is NctsDepartureMovementHeader movementHeader)
		{
			var isNotNationalTransitSwitzerland = !movementHeader.IsNationalTransitSwitzerland;
			SupplyChainActorTabPage.TabVisible = isNotNationalTransitSwitzerland;
			CountryOfRoutingTabPage.TabVisible = isNotNationalTransitSwitzerland;
		}
	}
}
