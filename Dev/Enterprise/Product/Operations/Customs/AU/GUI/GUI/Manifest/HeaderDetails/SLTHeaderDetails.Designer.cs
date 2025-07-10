using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.ExportManifest.GUI
{
	partial class SLTHeaderDetails
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CTOFindBox.SuspendLayout();
			this.VesselCodeFindBox.SuspendLayout();
			this.PortOfDestinationCodeFindBox.SuspendLayout();
			this.PortOfDepartureCodeFindBox.SuspendLayout();
			this.DateOfDepartureDateEdit.SuspendLayout();
			this.CountryOfDestinationCodeFindBox.SuspendLayout();
			this.ModeOfTransportDropEdit.SuspendLayout();
			this.PackDepotDocAddressControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// RecalculateTotalsButton
			// 
			this.RecalculateTotalsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 82, true);
			this.RecalculateTotalsButton.TabIndex = 6;
			// 
			// CTOFindBox
			// 
			this.CTOFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(620, 111, true);
			this.CTOFindBox.Visible = false;
			// 
			// ContingencyCANTextBox
			// 
			this.ContingencyCANTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 84, true);
			this.ContingencyCANTextBox.TabIndex = 10;
			// 
			// ContingencyCANLabel
			// 
			this.ContingencyCANLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 87, true);
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 6, true);
			this.VesselCodeFindBox.PreBoundMaxLength = 35;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 17, true);
			this.VesselCodeFindBox.TabIndex = 7;
			// 
			// CTOLabel
			// 
			this.CTOLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 116, true);
			this.CTOLabel.Visible = false;
			// 
			// PortOfDestinationCodeFindBox
			// 
			this.PortOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(630, 110, true);
			this.PortOfDestinationCodeFindBox.Visible = false;
			// 
			// PortOfDestinationLabel
			// 
			this.PortOfDestinationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 114, true);
			this.PortOfDestinationLabel.Visible = false;
			// 
			// TotalContainerCountLabel
			// 
			this.TotalContainerCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 35, true);
			// 
			// TotalPackageCountCalcEdit
			// 
			this.TotalPackageCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 6, true);
			this.TotalPackageCountCalcEdit.TabIndex = 3;
			// 
			// VoyageFlightNoTextBox
			// 
			this.VoyageFlightNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 58, true);
			this.VoyageFlightNoTextBox.TabIndex = 9;
			// 
			// VesselIDMasterAirWaybillLabel
			// 
			this.VesselIDMasterAirWaybillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 9, true);
			this.VesselIDMasterAirWaybillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 13, true);
			this.VesselIDMasterAirWaybillLabel.Text = "Optional Vessel:";
			// 
			// PortOfDepartureCodeFindBox
			// 
			this.PortOfDepartureCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 110, true);
			this.PortOfDepartureCodeFindBox.TabIndex = 11;
			// 
			// CountryOfDestainationLabel
			// 
			this.CountryOfDestainationLabel.Visible = false;
			// 
			// ModeOfTransportLabel
			// 
			this.ModeOfTransportLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 35, true);
			// 
			// TotalContainerCountCalcEdit
			// 
			this.TotalContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 32, true);
			this.TotalContainerCountCalcEdit.TabIndex = 4;
			// 
			// CANTextBox
			// 
			this.CANTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 6, true);
			// 
			// DateOfDepartureDateEdit
			// 
			this.DateOfDepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 58, true);
			// 
			// VoyageFlightNoLabel
			// 
			this.VoyageFlightNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 61, true);
			this.VoyageFlightNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 13, true);
			this.VoyageFlightNoLabel.Text = "Optional Voyage No:";
			// 
			// TotalEmptyContainerCountCalcEdit
			// 
			this.TotalEmptyContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 58, true);
			this.TotalEmptyContainerCountCalcEdit.TabIndex = 5;
			// 
			// CountryOfDestinationCodeFindBox
			// 
			this.CountryOfDestinationCodeFindBox.Visible = false;
			// 
			// TotalEmptyContainerCountLabel
			// 
			this.TotalEmptyContainerCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 61, true);
			// 
			// PortOfDepartureLabel
			// 
			this.PortOfDepartureLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 113, true);
			// 
			// TotalPackageCountLabel
			// 
			this.TotalPackageCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 9, true);
			// 
			// ModeOfTransportDropEdit
			// 
			this.ModeOfTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 32, true);
			// 
			// LloydsIMOTextBox
			// 
			this.LloydsIMOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 32, true);
			this.LloydsIMOTextBox.TabIndex = 8;
			// 
			// LloydsLabel
			// 
			this.LloydsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 35, true);
			// 
			// SLTHeaderDetails
			// 
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 111, true);
			this.Name = "SLTHeaderDetails";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 131, true);
			this.CTOFindBox.ResumeLayout(true);
			this.CTOFindBox.PerformLayout();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.PortOfDestinationCodeFindBox.ResumeLayout(true);
			this.PortOfDestinationCodeFindBox.PerformLayout();
			this.PortOfDepartureCodeFindBox.ResumeLayout(true);
			this.PortOfDepartureCodeFindBox.PerformLayout();
			this.DateOfDepartureDateEdit.ResumeLayout(true);
			this.DateOfDepartureDateEdit.PerformLayout();
			this.CountryOfDestinationCodeFindBox.ResumeLayout(true);
			this.CountryOfDestinationCodeFindBox.PerformLayout();
			this.ModeOfTransportDropEdit.ResumeLayout(true);
			this.ModeOfTransportDropEdit.PerformLayout();
			this.PackDepotDocAddressControl.ResumeLayout(true);
			this.PackDepotDocAddressControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
