using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.ExportManifest.GUI
{
	partial class DEPHeaderDetails
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
			this.RecalculateTotalsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(705, 102, true);
			this.RecalculateTotalsButton.Visible = false;
			// 
			// CTOFindBox
			// 
			this.CTOFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 58, true);
			this.CTOFindBox.TabIndex = 2;
			// 
			// ContingencyCANTextBox
			// 
			this.ContingencyCANTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(633, 108, true);
			this.ContingencyCANTextBox.Visible = false;
			// 
			// ContingencyCANLabel
			// 
			this.ContingencyCANLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(513, 111, true);
			this.ContingencyCANLabel.Visible = false;
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(578, 6, true);
			this.VesselCodeFindBox.PreBoundMaxLength = 35;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 17, true);
			this.VesselCodeFindBox.TabIndex = 3;
			// 
			// CTOLabel
			// 
			this.CTOLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 61, true);
			// 
			// PortOfDestinationCodeFindBox
			// 
			this.PortOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(578, 80, true);
			this.PortOfDestinationCodeFindBox.TabIndex = 6;
			// 
			// PortOfDestinationLabel
			// 
			this.PortOfDestinationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 83, true);
			this.PortOfDestinationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 13, true);
			this.PortOfDestinationLabel.Text = "Next Port of Destination:";
			// 
			// TotalContainerCountLabel
			// 
			this.TotalContainerCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(543, 113, true);
			this.TotalContainerCountLabel.Visible = false;
			// 
			// TotalPackageCountCalcEdit
			// 
			this.TotalPackageCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(705, 110, true);
			this.TotalPackageCountCalcEdit.Visible = false;
			// 
			// VoyageFlightNoTextBox
			// 
			this.VoyageFlightNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(578, 55, true);
			this.VoyageFlightNoTextBox.TabIndex = 5;
			// 
			// VesselIDMasterAirWaybillLabel
			// 
			this.VesselIDMasterAirWaybillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 9, true);
			// 
			// PortOfDepartureCodeFindBox
			// 
			this.PortOfDepartureCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 110, true);
			this.PortOfDepartureCodeFindBox.Visible = false;
			// 
			// CountryOfDestainationLabel
			// 
			this.CountryOfDestainationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 114, true);
			this.CountryOfDestainationLabel.Visible = false;
			// 
			// ModeOfTransportLabel
			// 
			this.ModeOfTransportLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 10, true);
			// 
			// TotalContainerCountCalcEdit
			// 
			this.TotalContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(695, 109, true);
			this.TotalContainerCountCalcEdit.Visible = false;
			// 
			// DateOfDepartureLabel
			// 
			this.DateOfDepartureLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 36, true);
			// 
			// CANTextBox
			// 
			this.CANTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 117, true);
			this.CANTextBox.Visible = false;
			// 
			// DateOfDepartureDateEdit
			// 
			this.DateOfDepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 32, true);
			this.DateOfDepartureDateEdit.TabIndex = 1;
			// 
			// VoyageFlightNoLabel
			// 
			this.VoyageFlightNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 58, true);
			// 
			// TotalEmptyContainerCountCalcEdit
			// 
			this.TotalEmptyContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(665, 108, true);
			this.TotalEmptyContainerCountCalcEdit.Visible = false;
			// 
			// CountryOfDestinationCodeFindBox
			// 
			this.CountryOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 108, true);
			this.CountryOfDestinationCodeFindBox.Visible = false;
			// 
			// TotalEmptyContainerCountLabel
			// 
			this.TotalEmptyContainerCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(513, 108, true);
			this.TotalEmptyContainerCountLabel.Visible = false;
			// 
			// PortOfDepartureLabel
			// 
			this.PortOfDepartureLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 113, true);
			this.PortOfDepartureLabel.Visible = false;
			// 
			// TotalPackageCountLabel
			// 
			this.TotalPackageCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(553, 114, true);
			this.TotalPackageCountLabel.Visible = false;
			// 
			// CANLabel
			// 
			this.CANLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 121, true);
			this.CANLabel.Visible = false;
			// 
			// ModeOfTransportDropEdit
			// 
			this.ModeOfTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 6, true);
			this.ModeOfTransportDropEdit.TabIndex = 0;
			// 
			// LloydsIMOTextBox
			// 
			this.LloydsIMOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(578, 31, true);
			this.LloydsIMOTextBox.TabIndex = 4;
			// 
			// LloydsLabel
			// 
			this.LloydsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 33, true);
			// 
			// DEPHeaderDetails
			// 
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 82, true);
			this.Name = "DEPHeaderDetails";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 145, true);
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
