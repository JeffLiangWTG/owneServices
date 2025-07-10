using Enterprise.ZArchitecture;

namespace Enterprise.ComplianceRisk.GUI
{
	partial class LocationRiskUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ComplianceRisk.GUI.ComplianceRiskColumnStyleInfo zTextBoxColumnStyleInfoRiskStatus = new Enterprise.ComplianceRisk.GUI.ComplianceRiskColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoCountryCode = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoCountryName = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoDescription = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.LocationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LocationGrid)).BeginInit();
			this.LocationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject);
			// 
			// LocationGrid
			// 
			this.LocationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LocationGrid, "Locations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).Locations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskLocationWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).Locations)).SyncRoot)).Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskLocationWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).Locations)).SyncRoot)).LocationDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskLocationWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).Locations)).SyncRoot)).RiskStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskLocationWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).Locations)).SyncRoot)).Description)));
			this.LocationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfoRiskStatus.ColumnName = "RiskStatus";
			zTextBoxColumnStyleInfoRiskStatus.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfoCountryCode.ColumnName = "Location";
			zTextBoxColumnStyleInfoCountryCode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfoCountryName.ColumnName = "LocationDescription";
			zTextBoxColumnStyleInfoCountryName.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfoDescription.ColumnName = "Description";
			zTextBoxColumnStyleInfoDescription.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			this.LocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoRiskStatus);
			this.LocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoCountryCode);
			this.LocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoCountryName);
			this.LocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoDescription);
			this.LocationGrid.DisableImportDataMenuItem = true;
			this.LocationGrid.GridId = "03FBCA70-E77F-4A35-ABD6-6A19318820EA";
			this.LocationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LocationGrid.LayoutKey = "LocationGrid";
			this.LocationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LocationGrid.Name = "LocationGrid";
			this.LocationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocationGrid.TabIndex = 0;
			// 
			// LocationRiskUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LocationGrid);
			this.Name = "LocationRiskUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LocationGrid)).EndInit();
			this.LocationGrid.ResumeLayout(false);
			this.LocationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected ZGrid LocationGrid;
	}
}
