namespace Enterprise.Accounting.GUI.eNett
{
	partial class ContainerStoragePaymentOrgSelectionForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.containerStoragePaymentOrgSelectionControl1 = new Enterprise.Accounting.GUI.eNett.ContainerStoragePaymentOrgSelectionControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 389, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource);
			// 
			// containerStoragePaymentOrgSelectionControl1
			// 
			this.BindingSource.SetBindingMember(this.containerStoragePaymentOrgSelectionControl1, ".");
			this.containerStoragePaymentOrgSelectionControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerStoragePaymentOrgSelectionControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.containerStoragePaymentOrgSelectionControl1.Name = "containerStoragePaymentOrgSelectionControl1";
			this.containerStoragePaymentOrgSelectionControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 413, true);
			this.containerStoragePaymentOrgSelectionControl1.TabIndex = 1;
			// 
			// ContainerStoragePaymentOrgSelectionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 413, true);
			this.Controls.Add(this.containerStoragePaymentOrgSelectionControl1);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource);
			this.Name = "ContainerStoragePaymentOrgSelectionForm";
			this.Text = "ContainerStoragePaymentOrgSelectionForm";
			this.Controls.SetChildIndex(this.containerStoragePaymentOrgSelectionControl1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ContainerStoragePaymentOrgSelectionControl containerStoragePaymentOrgSelectionControl1;
	}
}
