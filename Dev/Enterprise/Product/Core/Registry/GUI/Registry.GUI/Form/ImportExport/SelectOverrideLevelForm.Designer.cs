namespace Enterprise.Registry.GUI
{
	partial class SelectOverrideLevelForm
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
			this.treeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.compareButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 223, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 24, true);
			// 
			// treeView
			// 
			this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeView.HideSelection = false;
			this.treeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 26, true);
			this.treeView.Name = "treeView";
			this.treeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 167, true);
			this.treeView.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("eb14f3d3-2cb7-4b33-80eb-d97ab38df939", "Please select a level to compare to");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 14, true);
			this.zLabel1.TabIndex = 2;
			// 
			// compareButton
			// 
			this.compareButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.compareButton.AutoSize = true;
			this.compareButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("82de0cb8-5fef-4143-8d75-f87f9a15bf13", "Compare");
			this.compareButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 198, true);
			this.compareButton.Name = "compareButton";
			this.compareButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.compareButton.TabIndex = 1;
			this.compareButton.UseVisualStyleBackColor = true;
			this.compareButton.Click += new System.EventHandler(this.compareButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.AutoSize = true;
			this.cancelButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("673f67d5-f984-497f-a579-a26b4d36d04c", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 198, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// SelectOverrideLevelForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("57e14f23-da7f-4a58-aad3-3ee3efa88b99", "Select Level Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 247, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.compareButton);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.treeView);
			this.Name = "SelectOverrideLevelForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.treeView, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.compareButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZTreeView treeView;
		private ZArchitecture.ZLabel zLabel1;
		internal ZArchitecture.GUI.ZButton compareButton;
		internal ZArchitecture.GUI.ZButton cancelButton;
	}
}
