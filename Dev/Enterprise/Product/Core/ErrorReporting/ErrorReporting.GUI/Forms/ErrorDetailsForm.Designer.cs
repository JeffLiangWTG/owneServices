namespace Enterprise.ErrorReporting.GUI
{
	partial class ErrorDetailsForm
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
		private new void InitializeComponent()
		{
			this.zPostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.zTreeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 468, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 24, true);
			// 
			// zPostingButtonsUserControl
			// 
			this.zPostingButtonsUserControl.AllowDrop = true;
			this.zPostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 437, true);
			this.zPostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.zPostingButtonsUserControl.Name = "zPostingButtonsUserControl";
			this.zPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.zPostingButtonsUserControl.TabIndex = 1;
			// 
			// zTreeView
			// 
			this.zTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zTreeView.BackColor = System.Drawing.SystemColors.Control;
			this.zTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.zTreeView.Name = "zTreeView";
			this.zTreeView.ReadOnly = true;
			this.zTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 418, true);
			this.zTreeView.TabIndex = 2;
			// 
			// ErrorDetailsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 492, true);
			this.Controls.Add(this.zTreeView);
			this.Controls.Add(this.zPostingButtonsUserControl);
			this.Name = "ErrorDetailsForm";
			this.Controls.SetChildIndex(this.zPostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zTreeView, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Core.Forms.ZPostingButtonsUserControl zPostingButtonsUserControl;
		private ZArchitecture.GUI.ZTreeView zTreeView;
	}
}