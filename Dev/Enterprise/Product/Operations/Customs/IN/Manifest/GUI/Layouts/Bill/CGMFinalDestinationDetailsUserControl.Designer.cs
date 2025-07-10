namespace Enterprise.Customs.IN.Manifest.GUI;

partial class CGMFinalDestinationDetailsUserControl
{
	void InitializeComponent()
	{
		this.LocationInformationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.CustomsFinalDestinationPortTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.CustomsFinalDestinationPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.LocationInformationDropEdit.SuspendLayout();
		this.CustomsFinalDestinationPortCodeFindBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill);
		// 
		// LocationInformationDropEdit
		// 
		this.LocationInformationDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.LocationInformationDropEdit, "ABL_LocationInformation");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill)(null)).ABL_LocationInformation)));
		this.LocationInformationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 0, true);
		this.LocationInformationDropEdit.Name = "LocationInformationDropEdit";
		this.LocationInformationDropEdit.ShowDescriptionBox = false;
		this.LocationInformationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 21, true);
		this.LocationInformationDropEdit.TabIndex = 1;
		this.LocationInformationDropEdit.UseFullWidthForCodeBox = true;
		// 
		// CustomsFinalDestinationPortTextBox
		// 
		this.CustomsFinalDestinationPortTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.CustomsFinalDestinationPortTextBox, "ABL_CustomsFinalDestinationPort");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill)(null)).ABL_CustomsFinalDestinationPort)));
		this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.CustomsFinalDestinationPortTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
		this.CustomsFinalDestinationPortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 0, true);
		this.CustomsFinalDestinationPortTextBox.Name = "CustomsFinalDestinationPortTextBox";
		this.CustomsFinalDestinationPortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
		this.CustomsFinalDestinationPortTextBox.TabIndex = 2;
		// 
		// CustomsFinalDestinationPortCodeFindBox
		// 
		this.CustomsFinalDestinationPortCodeFindBox.AllowDrop = true;
		this.CustomsFinalDestinationPortCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.CustomsFinalDestinationPortCodeFindBox, "ABL_CustomsFinalDestinationPort");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill)(null)).ABL_CustomsFinalDestinationPort)));
		this.CustomsFinalDestinationPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 0, true);
		this.CustomsFinalDestinationPortCodeFindBox.Name = "CustomsFinalDestinationPortCodeFindBox";
		this.CustomsFinalDestinationPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
		this.CustomsFinalDestinationPortCodeFindBox.ParentType = null;
		this.CustomsFinalDestinationPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
		this.CustomsFinalDestinationPortCodeFindBox.TabIndex = 3;
		// 
		// CGMFinalDestinationDetailsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.SystemColors.Control;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.CustomsFinalDestinationPortCodeFindBox);
		this.Controls.Add(this.CustomsFinalDestinationPortTextBox);
		this.Controls.Add(this.LocationInformationDropEdit);
		this.Name = "CGMFinalDestinationDetailsUserControl";
		this.ShouldSerializeTabPageMethods = false;
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 20, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.LocationInformationDropEdit.ResumeLayout(true);
		this.LocationInformationDropEdit.PerformLayout();
		this.CustomsFinalDestinationPortCodeFindBox.ResumeLayout(true);
		this.CustomsFinalDestinationPortCodeFindBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	internal ZArchitecture.GUI.ZDropEdit LocationInformationDropEdit;
	internal ZArchitecture.ZTextBox CustomsFinalDestinationPortTextBox;
	internal ZArchitecture.GUI.ZCodeFindBox CustomsFinalDestinationPortCodeFindBox;
}
