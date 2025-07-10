using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStoragePackagesControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.PackTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.PackDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PackDetailsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackTabControl.SuspendLayout();
			this.PackDetailsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// PackTabControl
			// 
			this.PackTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PackTabControl.Controls.Add(this.PackDetailsTabPage);
			this.PackTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackTabControl.Name = "PackTabControl";
			this.PackTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 393, true);
			this.PackTabControl.TabIndex = 0;
			// 
			// PackDetailsTabPage
			// 
			this.PackDetailsTabPage.CaptionResourceString = Res.GetData("FCDCF574-B97D-4115-A22B-610739837118", "Pack Details");
			this.PackDetailsTabPage.Controls.Add(this.PackDetailsLayoutPanel);
			this.PackDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.PackDetailsTabPage.Name = "PackDetailsTabPage";
			this.PackDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1283, 369, true);
			this.PackDetailsTabPage.TabIndex = 1;
			// 
			// PackDetailsLayoutPanel
			// 
			this.PackDetailsLayoutPanel.AllowDrop = true;
			this.PackDetailsLayoutPanel.AutoScroll = true;
			this.PackDetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackDetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackDetailsLayoutPanel.Name = "PackDetailsLayoutPanel";
			this.PackDetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1283, 369, true);
			this.PackDetailsLayoutPanel.TabIndex = 2;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 584, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.PackTabControl);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(370);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(183);
			this.SplitContainer.SplitterWidth = 12;
			this.SplitContainer.TabIndex = 21;
			// 
			// UCC6TemporaryStoragePackagesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "UCC6TemporaryStoragePackagesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 584, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackTabControl.ResumeLayout(false);
			this.PackTabControl.PerformLayout();
			this.PackDetailsTabPage.ResumeLayout(false);
			this.PackDetailsTabPage.PerformLayout();
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion Component Designer generated code

		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl PackTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage PackDetailsTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel PackDetailsLayoutPanel;
	}
}
