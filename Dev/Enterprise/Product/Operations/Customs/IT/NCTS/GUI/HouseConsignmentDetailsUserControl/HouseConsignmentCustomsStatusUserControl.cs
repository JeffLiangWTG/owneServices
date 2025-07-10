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

public partial class HouseConsignmentCustomsStatusUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
{
	public HouseConsignmentCustomsStatusUserControl()
	{
		InitializeComponent();
		Extensions = new DefaultControlExtensionCollection(this);
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);

		if (HouseConsignment is NctsBill houseConsignment)
		{
			houseConsignment.UpdatedByDataRefresh += RefreshUIOnDataChange;
			houseConsignment.B0_BillStatusInfo.ValueChanged += RefreshUIOnDataChange;
			if (houseConsignment.Header.MovementHeader is NctsDepartureMovementHeader movementHeader)
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

		if (HouseConsignment is NctsBill houseConsignment)
		{
			houseConsignment.UpdatedByDataRefresh -= RefreshUIOnDataChange;
			houseConsignment.B0_BillStatusInfo.ValueChanged -= RefreshUIOnDataChange;
			if (houseConsignment.Header.MovementHeader is NctsDepartureMovementHeader movementHeader)
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
		if (HouseConsignment is NctsBill houseConsignment)
		{
			var button = DeleteRestoreToggleButton;

			button.Visible = ShouldToggleButtonBeVisible;
			button.CaptionResourceString = houseConsignment.B0_BillStatus.IsEmpty
				? Caption.DeleteRequest
				: Caption.RestoreRequest;

			button.UpdateCaption();
		}
	}

	bool ShouldToggleButtonBeVisible => HouseConsignment?.Factory.GetCached(ref shouldToggleButtonBeVisibleCached, GetShouldToggleButtonBeVisible) ?? false;
	CachedProperty<bool> shouldToggleButtonBeVisibleCached;

	bool GetShouldToggleButtonBeVisible()
	{
		return HouseConsignment.IsPhaseInAmendment
			&& HouseConsignment.Header.MovementHeader.BM_MessageStatus is ZString messageStatus
			&& (messageStatus.IsEmpty || messageStatus.ToString() is LogicalStatusList.Codes.Failed or LogicalStatusList.Codes.Error);
	}

	void RefreshUIOnDataChange(object sender, EventArgs e) => SetToggleButtonLayout();

	void DeleteRestoreToggleButton_Click(object sender, EventArgs e)
	{
		var houseConsignment = HouseConsignment;

		if (houseConsignment.B0_BillStatus.IsEmpty)
		{
			houseConsignment.SetAsCustomsDeletionRequest();
		}
		else if (houseConsignment.IsCustomsStatusDeleted)
		{
			ReactivateDeletedHouseConsignment(houseConsignment);
		}
		else
		{
			houseConsignment.ClearCustomsDeletionStatus();
		}

		SetToggleButtonLayout();
	}

	string IResourceStringBindingMember.ResourceStringBindingMember => nameof(NctsBill.B0_BillStatus);

	void ReactivateDeletedHouseConsignment(NctsBill houseConsignment)
	{
		if (ReactivationIsConfirmedByUser(houseConsignment))
		{
			houseConsignment.ClearCustomsDeletionStatus();
			_ = Globals.Message.Show(Caption.GetReactivationFeedback(houseConsignment.SequenceNumber),
				Caption.ReactivationFeedbackTitle,
				MessageBoxButtons.OK,
				DialogResult.OK);
		}
	}

	bool ReactivationIsConfirmedByUser(NctsBill houseConsignment)
	{
		return Globals.Message.Show(
			Caption.GetRestoreDeletionUserPrompt(houseConsignment.SequenceNumber),
			Caption.RestoreDeletionUserPromptTitle,
			MessageBoxButtons.YesNo,
			DialogResult.Yes) == DialogResult.Yes;
	}
	NctsBill HouseConsignment => CurrentDataItem as NctsBill;

	Control IExtendedControl.Host => this;

	[Browsable(false)]
	public IControlExtensionCollection Extensions { get; }

	#endregion

	#region Caption

	static class Caption
	{ 
		public static ResourceStringData DeleteRequest => Res.GetData("3671B474-EBA4-4F23-ADF5-FF82871AC117", "Delete Request");
		public static ResourceStringData RestoreRequest => Res.GetData("4F9B4FF3-99E8-470F-B2F0-1E66DCE87171", "Restore Item");
		public static string GetRestoreDeletionUserPrompt(int lineNumber)
			=> Res.GetString("26C783AB-31A6-4559-89AD-198C654FBDC7",
				"Are you sure you want to reactivate deleted (DEL) House Consignment number {0}?",
				lineNumber);
		public static string RestoreDeletionUserPromptTitle => Res.GetString("F26B2943-D69D-4C99-AC43-CEAA210E6773", "Confirm Reactivation");
		public static string ReactivationFeedbackTitle => Res.GetString("D130AFF4-A287-412D-B6A4-D4C31E6162ED", "Reactivation Feedback");
		public static string GetReactivationFeedback(int lineNumber)
			=> Res.GetString("E39FE7C2-3260-473F-BE08-4A916F835E15", "House Consignment number {0} is reactivated", lineNumber);
	}

	#endregion
}
