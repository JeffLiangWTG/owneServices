namespace Enterprise.Registry.GUI
{
	partial class RegistryComparisonForm
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
		protected new void InitializeComponent()
		{
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.treeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.overrideLevelPanel = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.baseLevelPanel = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ActivePluginControl = new Enterprise.ZArchitecture.ZLabel();
			this.btnClose = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.baseLevelPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 609, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1246, 24, true);
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f833cb78-da63-4b8e-9472-decee81e3305", "Select a Registry Item");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 24, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 13, true);
			this.zLabel1.TabIndex = 2;
			// 
			// treeView
			// 
			this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeView.CheckBoxes = true;
			this.treeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 40, true);
			this.treeView.Name = "treeView";
			this.treeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 534, true);
			this.treeView.TabIndex = 1;
			this.treeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterCheck);
			this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
			// 
			// overrideLevelPanel
			// 
			this.overrideLevelPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.overrideLevelPanel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b4c03d1e-972a-4182-83e4-35be8ac3bb69", "Value At Override Level");
			this.overrideLevelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(783, 34, true);
			this.overrideLevelPanel.Name = "overrideLevelPanel";
			this.overrideLevelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(451, 540, true);
			this.overrideLevelPanel.TabIndex = 3;
			this.overrideLevelPanel.TabStop = false;
			// 
			// baseLevelPanel
			// 
			this.baseLevelPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.baseLevelPanel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("95b55bf6-bc69-4bd2-86a1-383e80464f9f", "Value At Base Level");
			this.baseLevelPanel.Controls.Add(this.ActivePluginControl);
			this.baseLevelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 34, true);
			this.baseLevelPanel.Name = "baseLevelPanel";
			this.baseLevelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(451, 540, true);
			this.baseLevelPanel.TabIndex = 2;
			this.baseLevelPanel.TabStop = false;
			// 
			// ActivePluginControl
			// 
			this.ActivePluginControl.AutoSize = true;
			this.ActivePluginControl.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d69282ef-0cfc-4889-872e-726c16da2c17", "Select a registry item to see its value");
			this.ActivePluginControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.ActivePluginControl.Name = "ActivePluginControl";
			this.ActivePluginControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 13, true);
			this.ActivePluginControl.TabIndex = 0;
			// 
			// btnClose
			// 
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClose.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("05a31fea-bee3-47bd-8ed5-c2f011a5f767", "Close");
			this.btnClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1159, 580, true);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnClose.TabIndex = 10;
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// RegistryComparisonForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1246, 633, true);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.baseLevelPanel);
			this.Controls.Add(this.overrideLevelPanel);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.treeView);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 670, true);
			this.Name = "RegistryComparisonForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.treeView, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.overrideLevelPanel, 0);
			this.Controls.SetChildIndex(this.baseLevelPanel, 0);
			this.Controls.SetChildIndex(this.btnClose, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.baseLevelPanel.ResumeLayout(false);
			this.baseLevelPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZTreeView treeView;
		internal ZArchitecture.GUI.ZGroupBox overrideLevelPanel;
		internal ZArchitecture.GUI.ZGroupBox baseLevelPanel;
		internal ZArchitecture.GUI.ZButton btnClose;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel ActivePluginControl;
	}
}
