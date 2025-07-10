using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed partial class GoodsItemCustomsStatusUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
{
	public GoodsItemCustomsStatusUserControl()
	{
		InitializeComponent();
		Extensions = new DefaultControlExtensionCollection(this);
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);

		if (GoodsItem is NctsDepartureCargoDesc goodsItem)
		{
			goodsItem.UpdatedByDataRefresh += RefreshUIOnDataChange;
			goodsItem.BY_StatusInfo.ValueChanged += RefreshUIOnDataChange;
			if (goodsItem.MoveHeader is NctsDepartureMovementHeader movementHeader)
			{
				movementHeader.UpdatedByDataRefresh += RefreshUIOnDataChange;
				movementHeader.BM_MessageStatusInfo.ValueChanged += RefreshUIOnDataChange;
				movementHeader.BM_PhaseInfo.ValueChanged += RefreshUIOnDataChange;
			}
		}
		SetToggleButtonLayout();
	}

	protected override void OnCurrentDataItemChanged(EventArgs e)
	{
		base.OnCurrentDataItemChanged(e);

		SetToggleButtonLayout();
	}

	protected override void Dispose(bool disposing)
	{
		DeleteRestoreToggleButton.Click -= DeleteRestoreToggleButton_Click;

		if (GoodsItem is NctsDepartureCargoDesc goodsItem)
		{
			goodsItem.UpdatedByDataRefresh -= RefreshUIOnDataChange;
			goodsItem.BY_StatusInfo.ValueChanged -= RefreshUIOnDataChange;
			if (goodsItem.MoveHeader is NctsDepartureMovementHeader movementHeader)
			{
				movementHeader.UpdatedByDataRefresh -= RefreshUIOnDataChange;
				movementHeader.BM_MessageStatusInfo.ValueChanged -= RefreshUIOnDataChange;
				movementHeader.BM_PhaseInfo.ValueChanged -= RefreshUIOnDataChange;
			}
		}
		Extensions.Dispose();

		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Implementation

	void SetToggleButtonLayout()
	{
		if (GoodsItem is NctsDepartureCargoDesc goodsItem)
		{
			var button = DeleteRestoreToggleButton;

			button.Visible = ShouldToggleButtonBeVisible;
			button.CaptionResourceString = goodsItem.BY_Status.IsEmpty
				? Caption.DeleteRequest
				: Caption.RestoreItem;
			button.UpdateCaption();
		}
	}

	void RefreshUIOnDataChange(object sender, EventArgs e)
		 => SetToggleButtonLayout();

	void DeleteRestoreToggleButton_Click(object sender, EventArgs e)
	{
		var goodsItem = GoodsItem;

		if (goodsItem.BY_Status.IsEmpty)
		{
			goodsItem.SetAsCustomsDeletionRequest();
		}
		else if (goodsItem.IsCustomsStatusDeleted)
		{
			ReactivateDeletedGoodsItem(goodsItem);
		}
		else
		{
			goodsItem.ClearCustomsDeletionStatus();
		}

		SetToggleButtonLayout();
	}

	void ReactivateDeletedGoodsItem(NctsDepartureCargoDesc goodsItem)
	{
		if (ReactivationIsConfirmedByUser(goodsItem))
		{
			goodsItem.ClearCustomsDeletionStatus();
			_ = Globals.Message.Show(Caption.GetReactivationFeedback(goodsItem.LineNumber),
				Caption.ReactivationFeedbackTitle,
				MessageBoxButtons.OK,
				DialogResult.OK);
		}
	}

	bool ShouldToggleButtonBeVisible => GoodsItem?.Factory.GetCached(ref shouldToggleButtonBeVisibleCached, GetShouldToggleButtonBeVisible) ?? false;
	CachedProperty<bool> shouldToggleButtonBeVisibleCached;

	bool GetShouldToggleButtonBeVisible()
	{
		return GoodsItem?.MoveHeader is NctsDepartureMovementHeader movementHeader
				&& movementHeader.IsInAmendmentPhase
				&& movementHeader.BM_MessageStatus is ZString messageStatus
				&& (messageStatus.IsEmpty || messageStatus.ToString() is LogicalStatusList.Codes.Failed or LogicalStatusList.Codes.Error);
	}

	NctsDepartureCargoDesc GoodsItem => CurrentDataItem as NctsDepartureCargoDesc;

	Control IExtendedControl.Host => this;

	[Browsable(false)]
	public IControlExtensionCollection Extensions { get; }

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		Extensions.SetDataBinding(dataSource, dataMember);
	}

	string IResourceStringBindingMember.ResourceStringBindingMember => nameof(NctsDepartureCargoDesc.BY_Status);

	bool ReactivationIsConfirmedByUser(NctsDepartureCargoDesc goodsItem)
	{
		return Globals.Message.Show(
			Caption.GetRestoreDeletionUserPrompt(goodsItem.LineNumber),
			Caption.RestoreDeletionUserPromptTitle,
			MessageBoxButtons.YesNo,
			DialogResult.Yes) == DialogResult.Yes;
	}

	#endregion

	#region Captions

	static class Caption
	{
		public static ResourceStringData DeleteRequest => Res.GetData("9A67EECC-A3FE-4DD3-B41C-BE5EA93B9C0B", "Delete Request");
		public static ResourceStringData RestoreItem => Res.GetData("86931004-B7B2-48BA-B2FC-1110C6635A15", "Restore Item");
		public static string GetRestoreDeletionUserPrompt(int lineNumber)
			=> Res.GetString("2691EF21-5BDA-4A12-AAFC-4F59FE2E0BBC",
				"Are you sure you want to reactivate deleted (DEL) goods item number {0}?",
				lineNumber);
		public static string RestoreDeletionUserPromptTitle => Res.GetString("52E736A9-D075-4881-85C4-5A6C698603CE", "Confirm Reactivation");
		public static string ReactivationFeedbackTitle => Res.GetString("CF8885C4-E8E6-42E9-B857-1474A3A7136E", "Reactivation Feedback");
		public static string GetReactivationFeedback(int lineNumber)
			=> Res.GetString("C8E0A034-C268-4DF5-A3DA-30B4ED37AB90", "Goods Item number {0} is reactivated", lineNumber);
	}

	#endregion
}
