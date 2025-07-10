namespace Enterprise.Registry.GUI
{
	partial class FindOverridesLevelForm
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
			this.baseLevelTreeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.overrideLevelTreeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.findButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 289, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 24, true);
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("13a1676c-1706-4b4d-9322-47d42122eee6", "Base Level");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabel1.TabIndex = 1;
			// 
			// baseLevelTreeView
			// 
			this.baseLevelTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.baseLevelTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.baseLevelTreeView.HideSelection = false;
			this.baseLevelTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 26, true);
			this.baseLevelTreeView.Name = "baseLevelTreeView";
			this.baseLevelTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 213, true);
			this.baseLevelTreeView.TabIndex = 3;
			// 
			// overrideLevelTreeView
			// 
			this.overrideLevelTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.overrideLevelTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.overrideLevelTreeView.HideSelection = false;
			this.overrideLevelTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 26, true);
			this.overrideLevelTreeView.Name = "overrideLevelTreeView";
			this.overrideLevelTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 213, true);
			this.overrideLevelTreeView.TabIndex = 4;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3bf59a9e-cf48-4842-a4fd-064a6aa99244", "Override Level");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabel2.TabIndex = 2;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4314eb78-ca81-4066-9616-5fe64c600899", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 260, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// findButton
			// 
			this.findButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.findButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("81d92b4c-f6b3-4358-9d8e-8d3450b934ba", "Find");
			this.findButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 260, true);
			this.findButton.Name = "findButton";
			this.findButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.findButton.TabIndex = 5;
			this.findButton.UseVisualStyleBackColor = true;
			this.findButton.Click += new System.EventHandler(this.findButton_Click);
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.baseLevelTreeView);
			this.kSplitContainer1.Panel1.Controls.Add(this.zLabel1);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.overrideLevelTreeView);
			this.kSplitContainer1.Panel2.Controls.Add(this.zLabel2);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 242, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			this.kSplitContainer1.TabIndex = 5;
			this.kSplitContainer1.TabStop = false;
			// 
			// FindOverridesLevelForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("dcb5d9ea-91e8-460b-8cd1-a1c3a95dc4c9", "Select Override Level");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 313, true);
			this.Controls.Add(this.kSplitContainer1);
			this.Controls.Add(this.findButton);
			this.Controls.Add(this.cancelButton);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 250, true);
			this.Name = "FindOverridesLevelForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.findButton, 0);
			this.Controls.SetChildIndex(this.kSplitContainer1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.GUI.ZTreeView baseLevelTreeView;
		private ZArchitecture.GUI.ZTreeView overrideLevelTreeView;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZButton findButton;
		private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
	}
}
