using System;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;

namespace Enterprise.Customs.CH.NCTS;

public class TransportAndPackagingTabUserControl : Phase5TransportAndPackagingTabUserControl
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
		SetTransportBorderGroupBoxVisibility();
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

	void BM_InBondEntryType_ValueChanged(object sender, EventArgs e) => SetTransportBorderGroupBoxVisibility();

	void SetTransportBorderGroupBoxVisibility()
	{
		TransportBorderGroupBox.Visible = !(MovementHeader?.IsNationalTransitSwitzerland ?? false);
	}
}
