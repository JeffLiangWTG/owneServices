namespace Enterprise.Customs.BR.GUI
{
	partial class OrgSupplierTariffDetailsUserControl
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
			this.NveGridLayout = new Enterprise.Customs.BR.GUI.NveUserControl();
			this.TariffDetachLayout = new Enterprise.Customs.BR.GUI.TariffDetachUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NveGridLayout.SuspendLayout();
			this.TariffDetachLayout.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.CusClassPartPivot);
			// 
			// NveGridLayout
			// 
			this.NveGridLayout.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NveGridLayout, "NveCusCodeDataCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.BR.Business.NveCusCodeDataCollection)(((Enterprise.Customs.BR.Business.CusClassPartPivot)(null)).NveCusCodeDataCollection)));
			this.NveGridLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NveGridLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.NveGridLayout.Name = "NveGridLayout";
			this.NveGridLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 151, true);
			this.NveGridLayout.TabIndex = 10;
			// 
			// TariffDetachLayout
			// 
			this.TariffDetachLayout.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffDetachLayout, ".");
			this.TariffDetachLayout.Dock = System.Windows.Forms.DockStyle.Top;
			this.TariffDetachLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TariffDetachLayout.Name = "TariffDetachLayout";
			this.TariffDetachLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 50, true);
			this.TariffDetachLayout.TabIndex = 9;
			// 
			// OrgSupplierTariffDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NveGridLayout);
			this.Controls.Add(this.TariffDetachLayout);
			this.Name = "OrgSupplierTariffDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 201, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NveGridLayout.ResumeLayout(true);
			this.NveGridLayout.PerformLayout();
			this.TariffDetachLayout.ResumeLayout(true);
			this.TariffDetachLayout.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal NveUserControl NveGridLayout;
		internal TariffDetachUserControl TariffDetachLayout;

		#endregion
	}
}
