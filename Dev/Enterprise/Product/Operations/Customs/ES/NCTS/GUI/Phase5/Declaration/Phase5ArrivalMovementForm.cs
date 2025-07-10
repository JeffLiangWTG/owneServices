using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI;

public partial class Phase5ArrivalMovementForm : EU.NCTS.GUI.Phase5ArrivalMovementForm
{
	public Phase5ArrivalMovementForm(NctsHeader nctsMovement) : base(nctsMovement)
	{
		InitializeComponent();
		InitializeValueChangesEvents();
		HookStatusChangeEvents();
	}

	void InitializeValueChangesEvents()
	{
		DataSource.ESNctsHeader.CEN_TNNArrivalInfo.ValueChanged -= TNNArrivalInfo_ValueChanged;
		DataSource.ESNctsHeader.CEN_TNNArrivalInfo.ValueChanged += TNNArrivalInfo_ValueChanged;

		TNNArrivalInfo_ValueChanged(null, null);
	}

	void TNNArrivalInfo_ValueChanged(object sender, EventArgs e)
	{
		ChangeTNNTabVisibility();
	}

	public void ChangeTNNTabVisibility()
	{
		var associatedTNN = DataSource.ArrivalMovementHeader.HeaderTNN;
		if (associatedTNN != null)
		{
			TNNTabPage.TabVisible = true;
		}
		else
		{
			TNNTabPage.TabVisible = false;
		}
	}

	protected new NctsHeader DataSource => base.DataSource as NctsHeader;
	ZString currentArrivalMrnFromUser;

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		currentArrivalMrnFromUser = DataSource.ArrivalMrnFromUser;
		ChangeTNNTabVisibility();
		ToggleArrivalNotificationTabEditableState();
		ToggleTNNTabEditableState();
	}

	public void ToggleTNNTabEditableState()
	{
		var associatedTNN = DataSource.ArrivalMovementHeader?.HeaderTNN;
		if (associatedTNN != null)
		{
			var isDepartureTabEditable = !associatedTNN.IsDepartureTabReadOnly;
			TNNTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable);
		}
	}

	protected override ContinueWithSave ValidateAndSave()
	{
		var continueWithSave = base.ValidateAndSave();

		if (continueWithSave == ContinueWithSave.Yes && !currentArrivalMrnFromUser.Equals(DataSource.ArrivalMrnFromUser))
		{
			currentArrivalMrnFromUser = DataSource.ArrivalMrnFromUser;
		}

		return continueWithSave;
	}

	protected override ContinueWithSave ShowPreSaveDialogs()
	{
		var continueWithSave = base.ShowPreSaveDialogs();
		var associatedTNN = DataSource.ArrivalMovementHeader.HeaderTNN;
		if (continueWithSave == ContinueWithSave.Yes && associatedTNN != null && !currentArrivalMrnFromUser.Equals(DataSource.ArrivalMrnFromUser))
		{
			var message = Res.GetString("37CB6875-D2A8-4256-ADCC-AEE230EE996E", "Departure {0} is an associated TNN, so if you modify this MRN, the TNN's MRN will also be modified. Do you want to modify it?", associatedTNN.BH_JobReference);
			var dialogResult = Globals.Message.Show(message, ZString.Empty, MessageBoxButtons.YesNo, DialogResult.No);
			continueWithSave = dialogResult == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;
			if (continueWithSave == ContinueWithSave.Yes)
			{
				associatedTNN.MovementReferenceEntryNumber.CE_EntryNum = DataSource.ArrivalMrnFromUser;
				currentArrivalMrnFromUser = DataSource.ArrivalMrnFromUser;
			}
		}
		return continueWithSave;
	}

	void HookStatusChangeEvents()
	{
		DataSource.ArrivalMovementHeader.BM_CustomsStatusInfo.ValueChanged += StatusInfo_ValueChanged;
		DataSource.ArrivalMovementHeader.BM_MessageStatusInfo.ValueChanged += StatusInfo_ValueChanged;
	}

	void UnhookStatusChangeEvents()
	{
		DataSource.ArrivalMovementHeader.BM_CustomsStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
		DataSource.ArrivalMovementHeader.BM_MessageStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
	}

	void StatusInfo_ValueChanged(object sender, EventArgs e)
	{
		ToggleArrivalNotificationTabEditableState();
		ToggleIncidentsTabEditableState();
	}

	public void ToggleArrivalNotificationTabEditableState()
	{
		MainTabPage.UpdateEditableIncludingChildren(!DataSource.IsArrivalDetailsReadOnly, ArrivalNotificationTabPageControlsAllowedToRemainEditableAfterSending);
	}

	public void ToggleIncidentsTabEditableState()
	{
		IncidentsTabPage.UpdateEditableIncludingChildren(!DataSource.IsIncidentsReadOnly);
	}

	string[] ArrivalNotificationTabPageControlsAllowedToRemainEditableAfterSending => new string[] { nameof(EU.NCTS.GUI.ArrivalNotificationDetailsUserControl.LocalReferenceNumberTextBox), nameof(ArrivalNotificationDetailsUserControl.CertificateDropEdit), nameof(ArrivalNotificationDetailsUserControl.BrokerCodeFindBox) };

	protected override string[] ArrivalUnloadingRemarksTabPageControlsAllowedToRemainEditableAfterSending => new string[] { nameof(EU.NCTS.GUI.NctsGoodsItemsUserControl.ItemPackagesTabPage), nameof(Phase5UnloadingDifferencesTabUserControl.GuaranteeGroupBoxDynamicLayoutPanel), nameof(LiabilityCalculationTabPage) };

	const string LiabilityCalculationTabPage = "LiabilityCalculationTabPage";
}
