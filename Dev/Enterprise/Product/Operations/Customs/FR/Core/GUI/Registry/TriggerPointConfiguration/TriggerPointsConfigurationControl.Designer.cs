namespace Enterprise.Customs.FR.GUI.Registry
{
	partial class TriggerPointsConfigurationControl
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
            this.zTriggerPointConfigurationGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zEnableAutomatedValidationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.zImportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.zExportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zTriggerPointConfigurationGroupbox.SuspendLayout();
            this.zImportDropEdit.SuspendLayout();
            this.zExportDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Registry.TriggerPointsConfiguration);
            // 
            // zTriggerPointConfigurationGroupbox
            // 
            this.zTriggerPointConfigurationGroupbox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("TriggerPointConfigurationControl|D3B3409D-167F-4483-9922-0DE52D7C9DF9", "Trigger points");
            this.zTriggerPointConfigurationGroupbox.Controls.Add(this.zEnableAutomatedValidationCheckBox);
            this.zTriggerPointConfigurationGroupbox.Controls.Add(this.zImportDropEdit);
            this.zTriggerPointConfigurationGroupbox.Controls.Add(this.zExportDropEdit);
            this.zTriggerPointConfigurationGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zTriggerPointConfigurationGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zTriggerPointConfigurationGroupbox.Name = "zTriggerPointConfigurationGroupbox";
            this.zTriggerPointConfigurationGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
            this.zTriggerPointConfigurationGroupbox.TabIndex = 0;
            this.zTriggerPointConfigurationGroupbox.TabStop = false;
            // 
            // zEnableAutomatedValidationCheckBox
            // 
            this.BindingSource.SetBindingMember(this.zEnableAutomatedValidationCheckBox, "EnableAutomatedValidation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.FR.Registry.TriggerPointsConfiguration)(null)).EnableAutomatedValidation)));
            this.zEnableAutomatedValidationCheckBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("611534ed-e5fc-4c3d-98ac-8721f766b3a8", "Enable automated validation");
            this.zEnableAutomatedValidationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.zEnableAutomatedValidationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 36, true);
            this.zEnableAutomatedValidationCheckBox.Name = "zEnableAutomatedValidationCheckBox";
            this.zEnableAutomatedValidationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 24, true);
            this.zEnableAutomatedValidationCheckBox.TabIndex = 0;
            // 
            // zImportDropEdit
            // 
            this.zImportDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zImportDropEdit, "ImportTriggerPoint");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Registry.TriggerPointsConfiguration)(null)).ImportTriggerPoint)));
            this.zImportDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("TriggerPointConfigurationControl|B44B0920-8264-4994-8058-DBCB04E3816C", "Import");
            this.zImportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 66, true);
            this.zImportDropEdit.Name = "zImportDropEdit";
            this.zImportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.zImportDropEdit.TabIndex = 1;
            // 
            // zExportDropEdit
            // 
            this.zExportDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zExportDropEdit, "ExportTriggerPoint");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Registry.TriggerPointsConfiguration)(null)).ExportTriggerPoint)));
            this.zExportDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("TriggerPointConfigurationControl|D0F96735-D5CD-4322-B7CE-0DE71AD8F186", "Export");
            this.zExportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 92, true);
            this.zExportDropEdit.Name = "zExportDropEdit";
            this.zExportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.zExportDropEdit.TabIndex = 2;
            // 
            // TriggerPointsConfigurationControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.zTriggerPointConfigurationGroupbox);
            this.Name = "TriggerPointsConfigurationControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zTriggerPointConfigurationGroupbox.ResumeLayout(false);
            this.zTriggerPointConfigurationGroupbox.PerformLayout();
            this.zImportDropEdit.ResumeLayout(true);
            this.zImportDropEdit.PerformLayout();
            this.zExportDropEdit.ResumeLayout(true);
            this.zExportDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox zTriggerPointConfigurationGroupbox;
		private ZArchitecture.GUI.ZCheckBox zEnableAutomatedValidationCheckBox;
		private ZArchitecture.GUI.ZDropEdit zImportDropEdit;
		private ZArchitecture.GUI.ZDropEdit zExportDropEdit;
	}
}
