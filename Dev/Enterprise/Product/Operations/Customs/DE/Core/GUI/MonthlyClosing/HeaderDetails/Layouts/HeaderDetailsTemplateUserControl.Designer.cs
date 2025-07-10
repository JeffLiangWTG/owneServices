namespace Enterprise.Customs.DE.GUI
{
	partial class HeaderDetailsTemplateUserControl
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
			this.IsDeclarantImporterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsFinalizedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.UnlinkedDeclarationsNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusReconDeclaration);
			// 
			// IsDeclarantImporterCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsDeclarantImporterCheckBox, "IsDeclarantImporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).IsDeclarantImporter)));
			this.IsDeclarantImporterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 20, true);
			this.IsDeclarantImporterCheckBox.Name = "IsDeclarantImporterCheckBox";
			this.IsDeclarantImporterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 20, true);
			this.IsDeclarantImporterCheckBox.TabIndex = 16;
			this.IsDeclarantImporterCheckBox.UseVisualStyleBackColor = true;
			// 
			// RegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegistrationNumberTextBox, "RegistrationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).RegistrationNumber)));
			this.RegistrationNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 46, true);
			this.RegistrationNumberTextBox.Name = "RegistrationNumberTextBox";
			this.RegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.RegistrationNumberTextBox.TabIndex = 17;
			// 
			// IsFinalizedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsFinalizedCheckBox, "IsFinalized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).IsFinalized)));
			this.IsFinalizedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 72, true);
			this.IsFinalizedCheckBox.Name = "IsFinalizedCheckBox";
			this.IsFinalizedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.IsFinalizedCheckBox.TabIndex = 18;
			this.IsFinalizedCheckBox.UseVisualStyleBackColor = true;
			// 
			// BranchGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "CRD_GB_Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CRD_GB_Branch)));
			this.BranchGuidFindBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("A1D7EE50-8529-4158-A99E-A0FF04C2AA62", "Branch");
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 98, true);
			this.BranchGuidFindBox.ReadOnly = true;
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.IsFinalizedCheckBox.TabIndex = 19;
			// 
			// UnlinkedDeclarationsNumberLabel
			// 
			this.BindingSource.SetBindingMember(this.UnlinkedDeclarationsNumberLabel, "UnlinkedDeclarationsNumberMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).UnlinkedDeclarationsNumberMessage)));
			this.UnlinkedDeclarationsNumberLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UnlinkedDeclarationsNumberLabel.ForeColor = System.Drawing.Color.Red;
			this.UnlinkedDeclarationsNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 99, true);
			this.UnlinkedDeclarationsNumberLabel.Name = "UnlinkedDeclarationsNumberLabel";
			this.UnlinkedDeclarationsNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 23, true);
			this.UnlinkedDeclarationsNumberLabel.TabIndex = 19;
			this.UnlinkedDeclarationsNumberLabel.Text = "999 unlinked declarations found";
			this.UnlinkedDeclarationsNumberLabel.UseMnemonic = false;
			// 
			// HeaderDetailsTemplateUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnlinkedDeclarationsNumberLabel);
			this.Controls.Add(this.IsFinalizedCheckBox);
			this.Controls.Add(this.RegistrationNumberTextBox);
			this.Controls.Add(this.IsDeclarantImporterCheckBox);
			this.Controls.Add(this.BranchGuidFindBox);
			this.Name = "HeaderDetailsTemplateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 225, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZCheckBox IsDeclarantImporterCheckBox;
		internal ZArchitecture.ZTextBox RegistrationNumberTextBox;
		internal ZArchitecture.GUI.ZCheckBox IsFinalizedCheckBox;
		internal ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		internal ZArchitecture.ZLabel UnlinkedDeclarationsNumberLabel;
	}
}
