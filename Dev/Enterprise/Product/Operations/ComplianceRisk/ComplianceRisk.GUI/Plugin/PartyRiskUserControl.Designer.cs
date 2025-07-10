using Enterprise.ZArchitecture;

namespace Enterprise.ComplianceRisk.GUI
{
	partial class PartyRiskUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoPartyCode = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoPartyName = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoDescription = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PartyGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PartyGrid)).BeginInit();
			this.PartyGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject);

			this.BindingSource.SetBindingMember(this.PartyGrid, "Parties");
			//The line(s) below are a compile-time check for a binding member.Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPartyWrapperCollection)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPartyWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).Parties)).SyncRoot)).ScreeningStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPartyWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).Parties)).SyncRoot)).ScreeningStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPartyWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).Parties)).SyncRoot)).OrgCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPartyWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).Parties)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPartyWrapper)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).Parties)).SyncRoot)).ParentsDescription)));
			this.PartyGrid.AllowNavigation = false;
			this.PartyGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfoRiskStatus.ColumnName = "ScreeningStatusDescription";
			zTextBoxColumnStyleInfoRiskStatus.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfoRiskStatus.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("794e1f36-449c-4282-9882-80df2022f1fb", "Risk Status");
			zTextBoxColumnStyleInfoPartyCode.ColumnName = "OrgCode";
			zTextBoxColumnStyleInfoPartyCode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfoPartyCode.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("fa2e3059-a36c-424c-9012-d4f0dedefddb", "Party Code");
			zTextBoxColumnStyleInfoPartyName.ColumnName = "Code";
			zTextBoxColumnStyleInfoPartyName.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfoPartyName.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("f41af031-cff9-4b5b-b977-a68eaa22ed5a", "Party Name");
			zTextBoxColumnStyleInfoDescription.ColumnName = "ParentsDescription";
			zTextBoxColumnStyleInfoDescription.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zTextBoxColumnStyleInfoDescription.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("eef6558e-17ba-4039-b2dd-05abd907c1c1", "Description");
			this.PartyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoRiskStatus);
			this.PartyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoPartyCode);
			this.PartyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoPartyName);
			this.PartyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoDescription);
			this.PartyGrid.DisableImportDataMenuItem = true;
			this.PartyGrid.GridId = "297ddbe0-cca4-428f-b647-9ec0d90834b4";
			this.PartyGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PartyGrid.LayoutKey = "PartyGrid";
			this.PartyGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PartyGrid.Name = "PartyGrid";
			this.PartyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PartyGrid.TabIndex = 0;
			// 
			// PartyRiskUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PartyGrid);
			this.Name = "PartyRiskUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PartyGrid)).EndInit();
			this.PartyGrid.ResumeLayout(false);
			this.PartyGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected ZGrid PartyGrid;
	}
}
