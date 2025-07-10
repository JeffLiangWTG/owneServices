using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class PesticideUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CasNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UNDGGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ExceptProcessing2CheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UNDGGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			this.DetailsGroupBox.Controls.Add(this.CasNumberTextBox);
			this.DetailsGroupBox.Controls.Add(this.UNDGGuidFindBox);
			this.DetailsGroupBox.Controls.Add(this.ExceptProcessing2CheckBox);
			this.ExceptProcessing1CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 92, true);
			this.BatchLotNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 40, true);
			this.TradeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 66, true);
			this.BrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 14, true);
			this.BindingSource.SetBindingMember(this.ExceptProcessing1CheckBox, "CA_PES_SPCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_PES_SPCP)));
			this.ExceptProcessing1CheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4340E1C2-11EF-4165-93C1-76458420EA15", "PMRA Scheduled Pest Control Products");
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 152, true);
			// 
			// IntendedUseCodeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.IntendedUseCodeDropEdit, "CA_IntendedUseCodePES");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_IntendedUseCodePES)));
			// 
			// CategoryDropEdit
			// 
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "CA_CategoryPES");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_CategoryPES)));
			// 
			// CasNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CasNumberTextBox, "CA_CASNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_CASNumber)));
			this.CasNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("41CA8F0A-310C-46B2-8139-A88D05F6518F", "CAS", "CAS No.", "CAS number");
			this.CasNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 92, true);
			this.CasNumberTextBox.Name = "CasNumberTextBox";
			this.CasNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CasNumberTextBox.TabIndex = 12;
			// 
			// UNDGGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.UNDGGuidFindBox, "DangerousGoodsDGSubs");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).DangerousGoodsDGSubs)));
			this.UNDGGuidFindBox.AllowDrop = true;
			this.UNDGGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("524DD254-F179-41F5-B191-82A1A7A808F4", "UNDG");
			this.UNDGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 118, true);
			this.UNDGGuidFindBox.Name = "UNDGGuidFindBox";
			this.UNDGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.UNDGGuidFindBox.TabIndex = 6;
			// 
			// ExceptProcessing2CheckBox
			// 
			this.ExceptProcessing2CheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExceptProcessing2CheckBox, "CA_PES_EPCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_PES_EPCP)));
			this.ExceptProcessing2CheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("D5AA1F49-D537-4C47-883A-767685A6252C", "PMRA Exempt Pest Control Products");
			this.ExceptProcessing2CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExceptProcessing2CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 118, true);
			this.ExceptProcessing2CheckBox.Name = "ExceptProcessing1CheckBox";
			this.ExceptProcessing2CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ExceptProcessing2CheckBox.TabIndex = 11;
			this.ExceptProcessing2CheckBox.UseVisualStyleBackColor = true;
			this.CasNumberTextBox.TabIndex = 5;
			this.BrandNameTextBox.TabIndex = 7;
			this.BatchLotNumberTextBox.TabIndex = 8;
			this.TradeNameTextBox.TabIndex = 9;
			this.ExceptProcessing1CheckBox.TabIndex = 10;
			// 
			// PesticideUserControl
			// 
			this.Name = "PesticideUserControl";
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.UNDGGuidFindBox.ResumeLayout(true);
			this.UNDGGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		internal ZTextBox CasNumberTextBox;
		internal ZGuidFindBox UNDGGuidFindBox;
		internal ZArchitecture.GUI.ZCheckBox ExceptProcessing2CheckBox;
	}
}
