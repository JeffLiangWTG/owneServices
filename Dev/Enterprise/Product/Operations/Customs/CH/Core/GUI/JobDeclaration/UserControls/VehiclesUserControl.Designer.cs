namespace Enterprise.Customs.CH.GUI;

partial class VehiclesUserControl
{
    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
        this.VehiclesGrid = new Enterprise.ZArchitecture.ZGrid();
        this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
        this.VehicleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.ModelNameFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
        this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.TopPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.VehiclesGrid)).BeginInit();
        this.VehiclesGrid.SuspendLayout();
        this.BottomPanel.SuspendLayout();
        this.VehicleGroupBox.SuspendLayout();
        this.ModelNameFindBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.CusVehicle);
        // 
        // TopPanel
        // 
        this.TopPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TopPanel.Controls.Add(this.VehiclesGrid);
        this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.TopPanel.Name = "TopPanel";
        this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 311, true);
        this.TopPanel.TabIndex = 0;
        // 
        // VehiclesGrid
        // 
        this.VehiclesGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.VehiclesGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusVehicle)(null)))));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusVehicle)(null)).CVH_ModelName)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusVehicle)(null)).CVH_VehicleIdentificationNumber)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusVehicle)(null)).CVH_RegistrationNumber)));
        this.VehiclesGrid.CaptionVisible = false;
        zCodeFindBoxColumnStyleInfo1.ColumnName = "CVH_ModelName";
        zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo1.ColumnName = "CVH_VehicleIdentificationNumber";
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
        zTextBoxColumnStyleInfo2.ColumnName = "CVH_RegistrationNumber";
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        this.VehiclesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
        this.VehiclesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.VehiclesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.VehiclesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.VehiclesGrid.GridId = "82ef5be5-70ef-487d-bcf7-d1af6bd48be2";
        this.VehiclesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.VehiclesGrid.LayoutKey = "VehiclesGrid";
        this.VehiclesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.VehiclesGrid.Name = "VehiclesGrid";
        this.VehiclesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 311, true);
        this.VehiclesGrid.TabIndex = 0;
        // 
        // BottomPanel
        // 
        this.BottomPanel.Controls.Add(this.VehicleGroupBox);
        this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 311, true);
        this.BottomPanel.Name = "BottomPanel";
        this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 96, true);
        this.BottomPanel.TabIndex = 1;
        // 
        // VehicleGroupBox
        // 
        this.VehicleGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("1277b39f-7853-472b-840f-bf98044c0dcd", "Vehicle Information");
        this.VehicleGroupBox.Controls.Add(this.DescriptionTextBox);
        this.VehicleGroupBox.Controls.Add(this.ModelNameFindBox);
        this.VehicleGroupBox.Controls.Add(this.ReferenceNumberTextBox);
        this.VehicleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.VehicleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.VehicleGroupBox.Name = "VehicleGroupBox";
        this.VehicleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 96, true);
        this.VehicleGroupBox.TabIndex = 0;
        this.VehicleGroupBox.TabStop = false;
        // 
        // DescriptionTextBox
        // 
        this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CVH_RegistrationNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusVehicle)(null)).CVH_RegistrationNumber)));
        this.DescriptionTextBox.CaptionResourceString = null;
        this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 69, true);
        this.DescriptionTextBox.Name = "DescriptionTextBox";
        this.DescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
        this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
        this.DescriptionTextBox.TabIndex = 4;
        // 
        // ModelNameFindBox
        // 
        this.ModelNameFindBox.AllowDrop = true;
        this.ModelNameFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.ModelNameFindBox, "CVH_ModelName");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusVehicle)(null)).CVH_ModelName)));
        this.ModelNameFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 17, true);
        this.ModelNameFindBox.Name = "ModelNameFindBox";
        this.ModelNameFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
        this.ModelNameFindBox.ParentType = null;
        this.ModelNameFindBox.PreBoundMaxLength = 2;
        this.ModelNameFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
        this.ModelNameFindBox.TabIndex = 0;
        // 
        // ReferenceNumberTextBox
        // 
        this.ReferenceNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
        this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CVH_VehicleIdentificationNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusVehicle)(null)).CVH_VehicleIdentificationNumber)));
        this.ReferenceNumberTextBox.CaptionResourceString = null;
        this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 43, true);
        this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
        this.ReferenceNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
        this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
        this.ReferenceNumberTextBox.TabIndex = 2;
        // 
        // VehiclesUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.TopPanel);
        this.Controls.Add(this.BottomPanel);
        this.Name = "VehiclesUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 407, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.TopPanel.ResumeLayout(false);
        this.TopPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.VehiclesGrid)).EndInit();
        this.VehiclesGrid.ResumeLayout(false);
        this.VehiclesGrid.PerformLayout();
        this.BottomPanel.ResumeLayout(false);
        this.BottomPanel.PerformLayout();
        this.VehicleGroupBox.ResumeLayout(false);
        this.VehicleGroupBox.PerformLayout();
        this.ModelNameFindBox.ResumeLayout(true);
        this.ModelNameFindBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    protected Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
    protected Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
    protected Enterprise.ZArchitecture.ZGrid VehiclesGrid;
    protected Enterprise.ZArchitecture.GUI.ZGroupBox VehicleGroupBox;
    protected Enterprise.ZArchitecture.ZTextBox ReferenceNumberTextBox;
    protected ZArchitecture.GUI.ZCodeFindBox ModelNameFindBox;
    protected ZArchitecture.ZTextBox DescriptionTextBox;
}
