namespace Enterprise.Customs.AR.Manifest.GUI
{
	partial class ARBillCountrySpecificUserControl
	{
		private void InitializeComponent()
		{
			this.IsInformedToRenarCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsMonitoredTransitCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IsInformedToRenarCheckBox.SuspendLayout();
			this.IsMonitoredTransitCheckBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AR.Manifest.Business.AsycudaBill);
			// 
			// IsInformedToRenarCheckBox
			//
			this.BindingSource.SetBindingMember(this.IsInformedToRenarCheckBox, "ABL_IsInformedToRenar");
			this.IsInformedToRenarCheckBox.AutoSize = true;
			this.IsInformedToRenarCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsInformedToRenarCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsInformedToRenarCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 68, true);
			this.IsInformedToRenarCheckBox.Name = "IsInformedToRenarCheckBox";
			this.IsInformedToRenarCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsInformedToRenarCheckBox.TabIndex = 8;
			this.IsInformedToRenarCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsMonitoredTransitCheckBox
			//
			this.BindingSource.SetBindingMember(this.IsMonitoredTransitCheckBox, "ABL_IsMonitoredTransit");
			this.IsMonitoredTransitCheckBox.AutoSize = true;
			this.IsMonitoredTransitCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsMonitoredTransitCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsMonitoredTransitCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 68, true);
			this.IsMonitoredTransitCheckBox.Name = "IsMonitoredTransitCheckBox";
			this.IsMonitoredTransitCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsMonitoredTransitCheckBox.TabIndex = 8;
			this.IsMonitoredTransitCheckBox.UseVisualStyleBackColor = true;
			// 
			// ARBillCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.IsInformedToRenarCheckBox);
			this.Controls.Add(this.IsMonitoredTransitCheckBox);
			this.Name = "ARBillCountrySpecificUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IsInformedToRenarCheckBox.ResumeLayout(true);
			this.IsInformedToRenarCheckBox.PerformLayout();
			this.IsMonitoredTransitCheckBox.ResumeLayout(true);
			this.IsMonitoredTransitCheckBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZArchitecture.GUI.ZCheckBox IsInformedToRenarCheckBox;
		internal ZArchitecture.GUI.ZCheckBox IsMonitoredTransitCheckBox;
	}
}
