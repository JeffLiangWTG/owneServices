namespace Enterprise.Customs.IE.GUI
{
	public partial class ExportInvoiceLineDetailsUserControl
	{
		void InitializeComponent()
		{
			this.IsMainPackCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine);
			// 
			// IsMainPackCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsMainPackCheckBox, "ZG_IsMainPack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).ZG_IsMainPack)));
			this.IsMainPackCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 56, true);
			this.IsMainPackCheckBox.Name = "IsMainPackCheckBox";
			this.IsMainPackCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.IsMainPackCheckBox.TabIndex = 0;
			// 
			// ExportInvoiceLineDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IsMainPackCheckBox);
			this.Name = "ExportInvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 525, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZCheckBox IsMainPackCheckBox;
	}
}
