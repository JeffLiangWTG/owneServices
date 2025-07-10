namespace Enterprise.Customs.CA.GUI
{
	partial class PGARequirementsControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PGARequirementsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.programCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PGARequirementsGrid)).BeginInit();
			this.PGARequirementsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.programCodesGrid)).BeginInit();
			this.programCodesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.PGARequirementCollection);
			// 
			// PGARequirementsGrid
			// 
			this.PGARequirementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PGARequirementsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.PGARequirement)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.PGARequirement)(null)).AgencyCodeWithDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.PGARequirement)(null)).Program)));
			this.PGARequirementsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("PGARequirements|ae51d353-5dbf-411d-8a94-4abd31d87aa5", "Agency");
			zTextBoxColumnStyleInfo1.ColumnName = "AgencyCodeWithDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(270);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7a1b67e2-1ed9-4aad-8109-00f180091a0b", "Program");
			zTextBoxColumnStyleInfo2.ColumnName = "Program";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			this.PGARequirementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PGARequirementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PGARequirementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PGARequirementsGrid.GridId = "b014f906-8903-45d0-ae37-8a43cf9eaf20";
			this.PGARequirementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PGARequirementsGrid.LayoutKey = "PGARequirementsGrid";
			this.PGARequirementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PGARequirementsGrid.Name = "PGARequirementsGrid";
			this.PGARequirementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 302, true);
			this.PGARequirementsGrid.TabIndex = 2;
			// 
			// programCodesGrid
			// 
			this.programCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.programCodesGrid, "ProgramCodeRequirements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.PGARequirement)(null)).ProgramCodeRequirements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.PGAProgramRequirement)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.PGARequirement)(null)).ProgramCodeRequirements)).SyncRoot)).ProgramCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.PGAProgramRequirement)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.PGARequirement)(null)).ProgramCodeRequirements)).SyncRoot)).DeclareYes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.PGAProgramRequirement)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.PGARequirement)(null)).ProgramCodeRequirements)).SyncRoot)).DeclareNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.PGAProgramRequirement)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.PGARequirement)(null)).ProgramCodeRequirements)).SyncRoot)).DeclareNotApplicable)));
			this.programCodesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0e462519-236c-4c13-91be-f0351660d811", "Program Code Description");
			zTextBoxColumnStyleInfo3.ColumnName = "ProgramCodeDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ddb2ddee-383f-4d83-bdd2-650df521e5c0", "Yes");
			zCheckBoxColumnStyleInfo1.ColumnName = "DeclareYes";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("415b569d-0f1f-4783-a07b-16f600324512", "No");
			zCheckBoxColumnStyleInfo2.ColumnName = "DeclareNo";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f4292671-8da1-4a05-98de-8fd28ffef9bf", "N/A");
			zCheckBoxColumnStyleInfo3.ColumnName = "DeclareNotApplicable";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.programCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.programCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.programCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.programCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.programCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.programCodesGrid.GridId = "b014f906-8903-45d0-ae37-8a43cf9eaf20";
			this.programCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.programCodesGrid.LayoutKey = "PGARequirementsGrid";
			this.programCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.programCodesGrid.Name = "programCodesGrid";
			this.programCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 302, true);
			this.programCodesGrid.TabIndex = 3;
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.PGARequirementsGrid);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.programCodesGrid);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 302, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(531);
			this.kSplitContainer1.TabIndex = 4;
			// 
			// PGARequirementsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.kSplitContainer1);
			this.Name = "PGARequirementsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 302, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PGARequirementsGrid)).EndInit();
			this.PGARequirementsGrid.ResumeLayout(false);
			this.PGARequirementsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.programCodesGrid)).EndInit();
			this.programCodesGrid.ResumeLayout(false);
			this.programCodesGrid.PerformLayout();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid PGARequirementsGrid;
		private ZArchitecture.ZGrid programCodesGrid;
		private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
	}
}
