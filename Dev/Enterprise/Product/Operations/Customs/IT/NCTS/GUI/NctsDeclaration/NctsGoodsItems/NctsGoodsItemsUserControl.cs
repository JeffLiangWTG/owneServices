using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class NctsGoodsItemsUserControl : EU.NCTS.GUI.NctsGoodsItemsUserControl
{
	public NctsGoodsItemsUserControl()
	{
		InitializeComponent();
		SetUpGoodsItemsTabControlPages();
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		M2LinesTabPage.Enter += M2LinesTabPage_Enter;
		ItemPreviousDocumentsTabPage.Enter += ItemPreviousDocumentsTabPage_Enter;
	}

	protected override void OnCurrentDataItemChanged(EventArgs e)
	{
		base.OnCurrentDataItemChanged(e);
		UpdateEditableItemPreviousDocumentsTabPage();
	}

	void UpdateEditableItemPreviousDocumentsTabPage()
	{
		if (CurrentDataItem != null && CurrentDataItem.Header.IsNbRejectedAndMok)
		{
			var isEditable = CurrentDataItem.BY_Status == NctsTransitStatusList.Codes.NbRejected;
			ItemPreviousDocumentsTabPage.UpdateEditableIncludingChildren(isEditable);
		}
	}

	public new NctsDepartureCargoDesc CurrentDataItem => (NctsDepartureCargoDesc)base.CurrentDataItem;

	void M2LinesTabPage_Enter(object sender, EventArgs e)
	{
		GroupedPreviousDocumentsUserControl.Invalidate();
	}

	void ItemPreviousDocumentsTabPage_Enter(object sender, EventArgs e)
	{
		UpdateEditableItemPreviousDocumentsTabPage();
	}

	protected override Type GetPreviousDocumentsUserControlType() => typeof(NctsPreviousDocumentsUserControl);
	protected override Type GetItemDetailsUserControlType() => typeof(ItemDetailsUserControl);
	protected override Type GetSupportingDocumentsUserControl() => typeof(NctsSupportingDocumentsUserControl);
	protected override Type GetGoodsItemsGridUserControlType() => typeof(GoodsItemsGridUserControl);

	void SetUpGoodsItemsTabControlPages()
	{
		var expectedTabPagesInOrder = new Dictionary<ZInt, ZTabPage>()
		{
			{ 0, ItemDetailsTabPage },
			{ 1, ItemPackagesTabPage },
			{ 2, ItemContainersTabPage },
			{ 3, SupportingDocumentsTabPage },
			{ 4, ItemAdditionalInfosTabPage },
			{ 5, RemarksTabPage },
			{ 6, ItemPreviousDocumentsTabPage },
			{ 7, M2LinesTabPage },
			{ 8, ItemSecurityTabPage },
			{ 9, ItemTaxOrFeeTabPage },
		}.ToImmutableDictionary();
		GoodsItemsTabControl.OrderTabPages(expectedTabPagesInOrder);
	}
}
