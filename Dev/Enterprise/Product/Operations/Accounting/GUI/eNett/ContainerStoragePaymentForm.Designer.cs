using Enterprise.BatchProcessor.Accounting;
using Enterprise.Accounting.Business.eNett_Integration;
namespace Enterprise.Accounting.GUI.eNett
{
	partial class ContainerStoragePaymentForm
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

			if (DataSource != null)
			{
				DataSource.Dispose();
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
			this.components = new System.ComponentModel.Container();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.InvoiceDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.containerStorageInvoiceUserControl1 = new Enterprise.Accounting.GUI.ARAP.Invoicing.ContainerStorageInvoiceUserControl();
			this.zEventTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.InvoiceDetailsTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.containerStorageInvoiceUserControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 553, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1153, 24, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.eNett_Integration.StorageFeeInvoicePayment);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.InvoiceDetailsTabPage);
			this.MainTabControl.Controls.Add(this.zEventTabPage1);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1153, 577, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// InvoiceDetailsTabPage
			// 
			this.InvoiceDetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStoragePaymentForm|e594bf95-ae92-467d-9a81-891a6255a78b", "Invoice Details");
			this.InvoiceDetailsTabPage.Controls.Add(this.MainPanel);
			this.InvoiceDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InvoiceDetailsTabPage.Name = "InvoiceDetailsTabPage";
			this.InvoiceDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1145, 550, true);
			this.InvoiceDetailsTabPage.TabIndex = 0;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.PostButton);
			this.MainPanel.Controls.Add(this.CloseButton);
			this.MainPanel.Controls.Add(this.containerStorageInvoiceUserControl1);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1145, 550, true);
			this.MainPanel.TabIndex = 0;
			// 
			// PostButton
			// 
			this.PostButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(869, 498, true);
			this.PostButton.Name = "PostButton";
			this.PostButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.PostButton.TabIndex = 1;
			this.PostButton.UseVisualStyleBackColor = true;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1019, 498, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// containerStorageInvoiceUserControl1
			// 
			this.containerStorageInvoiceUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.containerStorageInvoiceUserControl1, ".");
			this.containerStorageInvoiceUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.containerStorageInvoiceUserControl1.Name = "containerStorageInvoiceUserControl1";
			this.containerStorageInvoiceUserControl1.ReadOnly = false;
			this.containerStorageInvoiceUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1145, 492, true);
			this.containerStorageInvoiceUserControl1.TabIndex = 0;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1145, 550, true);
			this.zEventTabPage1.TabIndex = 1;
			// 
			// ContainerStoragePaymentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStoragePaymentForm|43173a68-c855-48ea-a257-1afed406cebf", "Container Storage Charges");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1153, 577, true);
			this.Controls.Add(this.MainTabControl);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.eNett_Integration.StorageFeeInvoicePayment);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 613, true);
			this.Name = "ContainerStoragePaymentForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "StoragePaymentForm";
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.InvoiceDetailsTabPage.ResumeLayout(false);
			this.InvoiceDetailsTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.containerStorageInvoiceUserControl1.ResumeLayout(true);
			this.containerStorageInvoiceUserControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		public Enterprise.ZArchitecture.GUI.ZTabPage InvoiceDetailsTabPage;
		public Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zEventTabPage1;
		private Enterprise.Accounting.GUI.ARAP.Invoicing.ContainerStorageInvoiceUserControl containerStorageInvoiceUserControl1;
		private Enterprise.ZArchitecture.GUI.ZButton PostButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;


	}
}
