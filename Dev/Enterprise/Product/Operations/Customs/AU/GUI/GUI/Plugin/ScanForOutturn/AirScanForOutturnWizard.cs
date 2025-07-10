using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.AU.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public class AirScanForOutturnWizard : ScanForOutturnWizard
	{
		internal AirScanForOutturnManager manager
		{
			get { return (AirScanForOutturnManager)scanManager; }
		}

		public AirScanForOutturnWizard(ScanCusMAWB hostBO)
			: base(new AirScanForOutturnManager(hostBO))
		{
			if (manager.IsStandaloneShipment)
			{
				ReportZeroLandedButton.Text = Res.GetString("6c23fca4-cabb-404c-a54a-16bab0341d2c", "Zero land");
			}
			this.ReportZeroLandedButton.Click += new EventHandler(this.ReportZeroLandedButton_Click);
		}

		protected override void OnAfterSelectingUnderbond()
		{
			manager.ScanWizardDataSource.RefreshShipmentStatuses();
			//ReportZeroLandedButton.Visible = true;
			//Zero landing option removed for standalone jobs. I don't see a need for it, and it may not work correctly.
			//If the requirement returns then just set to true here, but verify correct opertaion.
			//ReportZeroLandedButton.Visible = !scanManager.IsStandaloneShipment;
			ReportZeroLandedButton.Visible = !manager.IsStandaloneShipment;
		}

		protected override void UnderbondSelectionTabPage_InitializeTab(object sender, EventArgs e)
		{
			base.UnderbondSelectionTabPage_InitializeTab(sender, e);
			this.UnderbondSelectionGrid.RemoveFromAvailableColumns(AutoUnderbondSelectorLine.Schema.ContainerNo);
			this.UnderbondSelectionGrid.RemoveFromAvailableColumns(AutoUnderbondSelectorLine.Schema.OutturnStatusText);
		}

		static AirScanForOutturnWizard()
		{
			ZFormStrategy.AddFormTypeThatCanBeCreatedDuringDbTransaction(typeof(AirScanForOutturnWizard));
		}

		void ReportZeroLandedButton_Click(object sender, EventArgs e)
		{
			if (!ValidateSelectedUnderbond())
			{
				return;
			}

			if (!ValidateSelectedShipment())
			{
				return;
			}

			string errorMessage = manager.CanSendZeroOutturns();
			if (!string.IsNullOrEmpty(errorMessage))
			{
				Globals.Message.Show(
							   errorMessage,
							   Res.GetString("22ec7290-97bd-48c9-89fe-ef7d3c83ee31", "Report Zero Landed Error"),
							   MessageBoxButtons.OK,
							   MessageBoxIcon.Error);
				return;
			}

			errorMessage = manager.SendZeroOutturnsForSelectedShipments(new SendsMessagesToCustomsGUI());

			if (!string.IsNullOrEmpty(errorMessage))
			{
				Globals.Message.Show(
							   errorMessage,
							   Res.GetString("f007a70d-1afb-436d-9b10-a0319789947f", "Send Zero Outturn"),
							   MessageBoxButtons.OK,
							   MessageBoxIcon.Information);
			}

			manager.ScanWizardDataSource.ClearShipmentSelection();
			manager.ScanWizardDataSource.RefreshShipmentStatuses();
			manager.ScanWizardDataSource.RefreshUnderbondStatuses();
		}
	}
}
