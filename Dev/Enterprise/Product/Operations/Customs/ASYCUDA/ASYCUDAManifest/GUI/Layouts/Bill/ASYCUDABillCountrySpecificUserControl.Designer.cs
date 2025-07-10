namespace Enterprise.Customs.ASYCUDAManifest.GUI
{
	partial class ASYCUDABillCountrySpecificUserControl
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
			this.BDEGMSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.SADOfficeCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SADRegistrationSerialTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SADRegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SADRegistrationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BDEGMSeparatorUserControl.SuspendLayout();
			this.SADOfficeCodeDropEdit.SuspendLayout();
			this.SADRegistrationDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDAManifest.Business.AsycudaBill);
			// 
			// BDEGMSeparatorUserControl
			// 
			this.BDEGMSeparatorUserControl.AllowDrop = true;
			this.BDEGMSeparatorUserControl.CaptionResourceString = Enterprise.Customs.ASYCUDAManifest.GUI.Res.GetData("3A8838F7-7440-440D-AD6D-5DA7064BCDB5", "Export General Manifest only");
			this.BDEGMSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 17, true);
			this.BDEGMSeparatorUserControl.Name = "BDEGMSeparatorUserControl";
			this.BDEGMSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.BDEGMSeparatorUserControl.TabIndex = 1;
			// 
			// SADOfficeCodeDropEdit
			// 
			this.SADOfficeCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SADOfficeCodeDropEdit, "SADOfficeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDAManifest.Business.AsycudaBill)(null)).SADOfficeCode)));
			this.SADOfficeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 50, true);
			this.SADOfficeCodeDropEdit.Name = "SADOfficeCodeDropEdit";
			this.SADOfficeCodeDropEdit.PreBoundMaxLength = 3;
			this.SADOfficeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 15, true);
			this.SADOfficeCodeDropEdit.TabIndex = 2;
			// 
			// SADRegistrationSerialTextBox
			// 
			this.BindingSource.SetBindingMember(this.SADRegistrationSerialTextBox, "SADRegistrationSerial");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDAManifest.Business.AsycudaBill)(null)).SADRegistrationSerial)));
			this.SADRegistrationSerialTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 74, true);
			this.SADRegistrationSerialTextBox.Name = "SADRegistrationSerialTextBox";
			this.SADRegistrationSerialTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
			this.SADRegistrationSerialTextBox.TabIndex = 3;
			// 
			// SADRegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.SADRegistrationNumberTextBox, "SADRegistrationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDAManifest.Business.AsycudaBill)(null)).SADRegistrationNumber)));
			this.SADRegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 98, true);
			this.SADRegistrationNumberTextBox.Name = "SADRegistrationNumberTextBox";
			this.SADRegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
			this.SADRegistrationNumberTextBox.TabIndex = 4;
			// 
			// SADRegistrationDateEdit
			// 
			this.SADRegistrationDateEdit.AllowDrop = true;
			this.SADRegistrationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.SADRegistrationDateEdit, "SADRegistrationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDAManifest.Business.AsycudaBill)(null)).SADRegistrationDate)));
			this.SADRegistrationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 123, true);
			this.SADRegistrationDateEdit.Name = "SADRegistrationDateEdit";
			this.SADRegistrationDateEdit.TabIndex = 5;
			// 
			// ASYCUDABillCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BDEGMSeparatorUserControl);
			this.Controls.Add(this.SADOfficeCodeDropEdit);
			this.Controls.Add(this.SADRegistrationSerialTextBox);
			this.Controls.Add(this.SADRegistrationNumberTextBox);
			this.Controls.Add(this.SADRegistrationDateEdit);
			this.Name = "ASYCUDABillCountrySpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 182, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BDEGMSeparatorUserControl.ResumeLayout(true);
			this.BDEGMSeparatorUserControl.PerformLayout();
			this.SADOfficeCodeDropEdit.ResumeLayout(true);
			this.SADOfficeCodeDropEdit.PerformLayout();
			this.SADRegistrationDateEdit.ResumeLayout(true);
			this.SADRegistrationDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl BDEGMSeparatorUserControl;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit SADOfficeCodeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox SADRegistrationSerialTextBox;
		internal Enterprise.ZArchitecture.ZTextBox SADRegistrationNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit SADRegistrationDateEdit;
	}
}
