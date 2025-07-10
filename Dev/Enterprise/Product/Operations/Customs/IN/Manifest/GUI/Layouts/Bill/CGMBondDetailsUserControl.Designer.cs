namespace Enterprise.Customs.IN.Manifest.GUI;

partial class CGMBondDetailsUserControl
{
	void InitializeComponent()
	{
		this.BondNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.BondDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.BondHolderOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
		this.MLOCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.BondDetailsGroupBox.SuspendLayout();
		this.BondHolderOrganisationFindBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill);
		// 
		// BondNumberTextBox
		// 
		this.BondNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.BondNumberTextBox, "BondNumber");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill)(null)).BondNumber)));
		this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.BondNumberTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
		this.BondNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 65, true);
		this.BondNumberTextBox.Name = "BondNumberTextBox";
		this.BondNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 21, true);
		this.BondNumberTextBox.TabIndex = 2;
		// 
		// BondDetailsGroupBox
		// 
		this.BondDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
		this.BondDetailsGroupBox.CaptionResourceString = Enterprise.Customs.IN.Manifest.GUI.Res.GetData("59ace4b5-3bba-47c4-82d1-38f14b8c0d19", "Bond Details");
		this.BondDetailsGroupBox.Controls.Add(this.BondHolderOrganisationFindBox);
		this.BondDetailsGroupBox.Controls.Add(this.MLOCodeTextBox);
		this.BondDetailsGroupBox.Controls.Add(this.BondNumberTextBox);
		this.BondDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 2, true);
		this.BondDetailsGroupBox.Name = "BondDetailsGroupBox";
		this.BondDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 92, true);
		this.BondDetailsGroupBox.TabIndex = 30;
		this.BondDetailsGroupBox.TabStop = false;
		// 
		// BondHolderOrganisationFindBox
		// 
		this.BondHolderOrganisationFindBox.AllowDrop = true;
		this.BondHolderOrganisationFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.BondHolderOrganisationFindBox, "ABL_OH_BondHolder");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill)(null)).ABL_OH_BondHolder)));
		this.BondHolderOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 15, true);
		this.BondHolderOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
		this.BondHolderOrganisationFindBox.Name = "BondHolderOrganisationFindBox";
		this.BondHolderOrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
		this.BondHolderOrganisationFindBox.ParentType = null;
		this.BondHolderOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 21, true);
		this.BondHolderOrganisationFindBox.TabIndex = 0;
		// 
		// MLOCodeTextBox
		// 
		this.MLOCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.MLOCodeTextBox, "MLOCode");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill)(null)).MLOCode)));
		this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.MLOCodeTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
		this.MLOCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 40, true);
		this.MLOCodeTextBox.Name = "MLOCodeTextBox";
		this.MLOCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 21, true);
		this.MLOCodeTextBox.TabIndex = 1;
		// 
		// BondDetailsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.SystemColors.Control;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.BondDetailsGroupBox);
		this.Name = "BondDetailsUserControl";
		this.ShouldSerializeTabPageMethods = false;
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 96, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.BondDetailsGroupBox.ResumeLayout(false);
		this.BondDetailsGroupBox.PerformLayout();
		this.BondHolderOrganisationFindBox.ResumeLayout(true);
		this.BondHolderOrganisationFindBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	internal ZArchitecture.ZTextBox BondNumberTextBox;
	private ZArchitecture.GUI.ZGroupBox BondDetailsGroupBox;
	internal ZArchitecture.ZTextBox MLOCodeTextBox;
	private MasterFiles.GUI.ZOrganisationFindBox BondHolderOrganisationFindBox;
}
