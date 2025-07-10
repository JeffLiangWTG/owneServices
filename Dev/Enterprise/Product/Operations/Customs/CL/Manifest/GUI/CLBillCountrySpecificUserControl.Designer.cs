namespace Enterprise.Customs.CL.Manifest.GUI
{
	partial class CLBillCountrySpecificUserControl
	{
		private void InitializeComponent()
		{
			this.RoRoCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RoRoCheckBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CL.Manifest.Business.AsycudaBill);
			// 
			// RoRoCheckBox
			//
			this.BindingSource.SetBindingMember(this.RoRoCheckBox, "ABL_RoRo");
			this.RoRoCheckBox.AutoSize = true;
			this.RoRoCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
			this.RoRoCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RoRoCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 68, true);
			this.RoRoCheckBox.Name = "RoRoCheckBox";
			this.RoRoCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.RoRoCheckBox.TabIndex = 8;
			this.RoRoCheckBox.UseVisualStyleBackColor = true;
			// 
			// CLBillCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.RoRoCheckBox);
			this.Name = "CLBillCountrySpecificUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RoRoCheckBox.ResumeLayout(true);
			this.RoRoCheckBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZArchitecture.GUI.ZCheckBox RoRoCheckBox;
	}
}
