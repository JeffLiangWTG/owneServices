namespace Enterprise.Accounting.Registry.GUI
{
	partial class AuthorizationModeAndSettingsControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.AuthorizationRequirementsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AuthorizationModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AuthorizationRequirementsGrid)).BeginInit();
			this.AuthorizationRequirementsGrid.SuspendLayout();
			this.AuthorizationModeDropEdit.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.AuthorizationModeAndSettings);
			// 
			// AuthorizationRequirementsGrid
			// 
			this.AuthorizationRequirementsGrid.AllowNavigation = false;
			this.AuthorizationRequirementsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AuthorizationRequirementsGrid, "AuthorisationSettings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.AuthorizationModeAndSettings)(null)).AuthorisationSettings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.PaymentTwoLevelsAuthorisationSettings)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.AuthorizationModeAndSettings)(null)).AuthorisationSettings)).SyncRoot)).RangeLocalized)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.PaymentTwoLevelsAuthorisationSettings)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.AuthorizationModeAndSettings)(null)).AuthorisationSettings)).SyncRoot)).Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.PaymentTwoLevelsAuthorisationSettings)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.AuthorizationModeAndSettings)(null)).AuthorisationSettings)).SyncRoot)).AuthorisationRequirementLocalized)));
			this.AuthorizationRequirementsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("22B47291-24EE-4320-AD20-2A9DB1E2205F", "Range");
			zDropEditColumnStyleInfo1.ColumnName = "RangeLocalized";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("46946260-5174-4539-A995-50B237DE5253", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9d8da525-6646-4370-80f6-5bb1ddf2fb31", "Authorization Requirement");
			zDropEditColumnStyleInfo2.ColumnName = "AuthorisationRequirementLocalized";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.AuthorizationRequirementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AuthorizationRequirementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AuthorizationRequirementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AuthorizationRequirementsGrid.GridId = "d90fb514-b452-4f1c-8658-cfbbe832a9bb";
			this.AuthorizationRequirementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AuthorizationRequirementsGrid.LayoutKey = "zGrid1";
			this.AuthorizationRequirementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
			this.AuthorizationRequirementsGrid.Name = "AuthorizationRequirementsGrid";
			this.AuthorizationRequirementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 251, true);
			this.AuthorizationRequirementsGrid.TabIndex = 2;
			// 
			// AuthorizationModeDropEdit
			// 
			this.AuthorizationModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationModeDropEdit, "AuthorizationMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.AuthorizationModeAndSettings)(null)).AuthorizationMode)));
			this.AuthorizationModeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f505cc4b-368f-4af9-96c3-fbccb1de5374", "Authorization Mode");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.AuthorizationModeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.AuthorizationModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 23, true);
			this.AuthorizationModeDropEdit.Name = "AuthorizationModeDropEdit";
			this.AuthorizationModeDropEdit.ShouldResizeByMaxLength = true;
			this.AuthorizationModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 20, true);
			this.AuthorizationModeDropEdit.TabIndex = 1;
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.AuthorizationModeDropEdit);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 46, true);
			this.topPanel.TabIndex = 3;
			// 
			// AuthorizationModeAndSettingsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.AuthorizationRequirementsGrid);
			this.Name = "AuthorizationModeAndSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 303, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AuthorizationRequirementsGrid)).EndInit();
			this.AuthorizationRequirementsGrid.ResumeLayout(false);
			this.AuthorizationRequirementsGrid.PerformLayout();
			this.AuthorizationModeDropEdit.ResumeLayout(true);
			this.AuthorizationModeDropEdit.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid AuthorizationRequirementsGrid;
		private ZArchitecture.GUI.ZDropEdit AuthorizationModeDropEdit;
		private ZArchitecture.GUI.ZPanel topPanel;
	}
}
