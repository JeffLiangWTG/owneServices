using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5UnloadingDifferencesTabUserControl : ZUserControl
	{
		internal ZUserControl ArrivalTransportInfosGridUserControl { get; private set; }

		public Phase5UnloadingDifferencesTabUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (DataSource is NctsHeader header)
				{
					header.ArrivalMovementHeader.BM_StateOfSealsBooleanInfo.ValueChanged -= BM_StateOfSealsBooleanValueChanged;
					header.ArrivalMovementHeader.BM_NoChangesToReportInfo.ValueChanged -= BM_NoChangesToReportValueChanged;
					header.ArrivalMovementHeader.OnFactorySavingAndGrossWeightUnloadedGreaterThanTotalGrossMassInKilograms -= ShowRecalculateTotalGrossWeightsDialog;
				}
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetUnloadingDetailsPanelLayout();
			SetGuaranteeGroupBoxPanelLayout();
			SetUnloadingDifferencesDeclaredValuePanelLayout();
			SetUnloadingDifferencesUnloadedValuePanelLayout();
			InitializeTransportInfoGrid();
			if (DataSource is NctsHeader header)
			{
				header.ArrivalMovementHeader.BM_StateOfSealsBooleanInfo.ValueChanged -= BM_StateOfSealsBooleanValueChanged;
				header.ArrivalMovementHeader.BM_StateOfSealsBooleanInfo.ValueChanged += BM_StateOfSealsBooleanValueChanged;
				header.ArrivalMovementHeader.BM_NoChangesToReportInfo.ValueChanged -= BM_NoChangesToReportValueChanged;
				header.ArrivalMovementHeader.BM_NoChangesToReportInfo.ValueChanged += BM_NoChangesToReportValueChanged;
				header.ArrivalMovementHeader.OnFactorySavingAndGrossWeightUnloadedGreaterThanTotalGrossMassInKilograms -= ShowRecalculateTotalGrossWeightsDialog;
				header.ArrivalMovementHeader.OnFactorySavingAndGrossWeightUnloadedGreaterThanTotalGrossMassInKilograms += ShowRecalculateTotalGrossWeightsDialog;
			}
		}

		protected void BM_StateOfSealsBooleanValueChanged(object sender, EventArgs e)
		{
			if (DataSource is NctsHeader header && header.ArrivalMovementHeader.BM_StateOfSealsBoolean && !header.AllArrivalSealStateAreDEC)
			{
				var message = Res.GetString("59D0E3B4-54FF-461D-9F71-D1AE527768CB", "Not all seals have the state DEC.\r\nPress CANCEL if you want to check or change the seals or its state.\r\nIf you press OK, all seals with the state blanks, MIS or DAM will be set to DEC. The seals with state NEW remain as is so that they can be sent to customs.");
				if (Globals.Message.Show(message, Caption, ZMessageBoxButtons.OKCancel, ZDialogResult.Cancel) == ZDialogResult.OK)
				{
					header.SetUpUnloadingStateOfTargetArrivalSeals();
				}
				else
				{
					header.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
				}
			}
		}

		protected void BM_NoChangesToReportValueChanged(object sender, EventArgs e)
		{
			if (DataSource is NctsHeader header && header.ArrivalMovementHeader.BM_NoChangesToReport && header.ExistNonDECEntry)
			{
				if (Globals.Message.Show(ChangeAllToDECConfirmationMessage, Caption, ZMessageBoxButtons.OKCancel, ZDialogResult.Cancel) == ZDialogResult.OK)
				{
					header.SetAllUnloadedStateToDEC();
				}
				else
				{
					header.ArrivalMovementHeader.BM_NoChangesToReport = false;
				}
			}
		}

		protected virtual string ChangeAllToDECConfirmationMessage => Res.GetString("AD2787DA-9F15-45AE-A034-EBCF341717AD", "Not all entries in the 'House consignment' tab have the value DEC.\r\nPress CANCEL if you want to check the unloaded state of the house consignments, goods items, packages and documents.\r\nIf you press OK, all entries with the state blanks, MIS or DIF will be set to DEC. The entries with state NEW will be removed.");

		INctsPhase5LayoutProvider LayoutProvider => layoutProvider ?? (layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode));
		INctsPhase5LayoutProvider layoutProvider;

		void SetUnloadingDetailsPanelLayout()
		{
			UnloadingDetailsDynamicLayoutPanel.UpdateLayout(LayoutProvider.UnloadingDetailsPanelLayout);
		}

		void SetGuaranteeGroupBoxPanelLayout()
		{
			GuaranteeGroupBoxDynamicLayoutPanel.UpdateLayout(LayoutProvider.GuaranteeForArrivalGroupBoxPanelLayout);
		}

		void SetUnloadingDifferencesDeclaredValuePanelLayout()
		{
			BindingSource.SetBindingMember(UnloadingDifferencesDeclaredValueDynamicLayoutPanel, nameof(NctsHeader.ArrivalMovementHeader));
			UnloadingDifferencesDeclaredValueDynamicLayoutPanel.UpdateLayout(LayoutProvider.UnloadingDifferencesDeclaredValuePanelLayout);
		}

		void SetUnloadingDifferencesUnloadedValuePanelLayout()
		{
			BindingSource.SetBindingMember(UnloadingDifferencesUnloadedValueDynamicLayoutPanel, nameof(NctsHeader.ArrivalMovementHeader));
			UnloadingDifferencesUnloadedValueDynamicLayoutPanel.UpdateLayout(LayoutProvider.UnloadingDifferencesUnloadedValuePanelLayout);
		}

		void InitializeTransportInfoGrid()
		{
			var arrivalTransportInfoGridType = LayoutProvider.ArrivalTransportInfoGridType;
			ArrivalTransportInfosGridUserControl = (ZUserControl)Activator.CreateInstance(arrivalTransportInfoGridType);
			ArrivalTransportInfoGroupBox.Controls.Add(ArrivalTransportInfosGridUserControl);
			BindingSource.SetBindingMember(ArrivalTransportInfosGridUserControl, nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.ArrivalTransportInfos));
			ArrivalTransportInfosGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			ArrivalTransportInfosGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 75, true);
			ArrivalTransportInfosGridUserControl.Dock = DockStyle.Fill;
		}

		void ShowRecalculateTotalGrossWeightsDialog(object sender, EventArgs e)
		{
			if (RecalculateTotalGrossWeightsDialog == DialogResult.OK)
			{
				((NctsArrivalMovementHeader)sender).UpdateGrossWeightsUnloaded();
			}
		}

		DialogResult RecalculateTotalGrossWeightsDialog => Globals.Message.Show(
			Res.GetString("4083239A-20A0-47FA-88B3-1B8B90EAAAC3", "Unloaded Gross Weights have changed. Recalculate Total Gross Weights in Unloading Remarks and House Consignments?"),
			Caption,
			MessageBoxButtons.OKCancel,
			MessageBoxIcon.Warning);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string Caption = "Warning";
	}
}
