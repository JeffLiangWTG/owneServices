using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	partial class ExportInvoiceDetailsUserControl
	{
		void InitializeComponent()
		{
			this.FreeOfChargeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader);
			// 
			// FreeOfChargeCheckBox
			// 
			this.FreeOfChargeCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreeOfChargeCheckBox, "JZ_FreeOfCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader)(null)).JZ_FreeOfCharge)));
			this.FreeOfChargeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 15, true);
			this.FreeOfChargeCheckBox.Name = "FreeOfChargeCheckBox";
			this.FreeOfChargeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 22, true);
			this.FreeOfChargeCheckBox.TabIndex = 0;
			// 
			// ExportInvoiceDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FreeOfChargeCheckBox);
			this.Name = "ExportInvoiceDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 52, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZCheckBox FreeOfChargeCheckBox;
	}
}
