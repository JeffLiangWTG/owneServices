namespace Enterprise.Customs.AE.Manifest.GUI
{
	partial class BillDetailsUserControl
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
			this.SplitBillCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SplitBillNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ForwarderMPCITextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SplitBillNumberCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AE.Manifest.Business.AsycudaBill);
			// 
			// SplitBillCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SplitBillCheckBox, "ABL_SplitBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AE.Manifest.Business.AsycudaBill)(null)).ABL_SplitBill)));
			this.SplitBillCheckBox.Checked = true;
			this.SplitBillCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.SplitBillCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 20, true);
			this.SplitBillCheckBox.Name = "SplitBillCheckBox";
			this.SplitBillCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 24, true);
			this.SplitBillCheckBox.TabIndex = 0;
			this.SplitBillCheckBox.UseVisualStyleBackColor = true;
			// 
			// SplitBillNumberCodeFindBox
			// 
			this.SplitBillNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SplitBillNumberCodeFindBox, "ABL_SplitBillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Manifest.Business.AsycudaBill)(null)).ABL_SplitBillNumber)));
			this.SplitBillNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 55, true);
			this.SplitBillNumberCodeFindBox.Name = "SplitBillNumberCodeFindBox";
			this.SplitBillNumberCodeFindBox.ParentType = null;
			this.SplitBillNumberCodeFindBox.ShowDescriptionBox = false;
			this.SplitBillNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.SplitBillNumberCodeFindBox.TabIndex = 1;
			// 
			// ForwarderMPCITextBox
			// 
			this.BindingSource.SetBindingMember(this.ForwarderMPCITextBox, "ForwarderMPCI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.AE.Manifest.Business.AsycudaBill)(null)).ForwarderMPCI)));
			this.ForwarderMPCITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 102, true);
			this.ForwarderMPCITextBox.Name = "ForwarderMPCITextBox";
			this.ForwarderMPCITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 21, true);
			this.ForwarderMPCITextBox.TabIndex = 2;
			// 
			// BillDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ForwarderMPCITextBox);
			this.Controls.Add(this.SplitBillNumberCodeFindBox);
			this.Controls.Add(this.SplitBillCheckBox);
			this.Name = "BillDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 151, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitBillNumberCodeFindBox.ResumeLayout(true);
			this.SplitBillNumberCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCheckBox SplitBillCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox SplitBillNumberCodeFindBox;
		internal ZArchitecture.ZTextBox ForwarderMPCITextBox;
	}
}
