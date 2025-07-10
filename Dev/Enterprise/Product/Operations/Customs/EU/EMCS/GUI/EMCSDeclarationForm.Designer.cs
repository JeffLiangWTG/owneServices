namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class EMCSDeclarationForm 
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
		protected override void InitializeComponent()
		{
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EMCSCustomsBrokerageUserControl = new Enterprise.Customs.EU.EMCS.GUI.EMCSCustomsBrokerageUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.zPostingButtonsUserControl.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.EMCSCustomsBrokerageUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 663, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1216, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.zPostingButtonsUserControl);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 625, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1216, 38, true);
			this.zPanel1.TabIndex = 2;
			// 
			// zPostingButtonsUserControl
			// 
			this.zPostingButtonsUserControl.AllowDrop = true;
			this.zPostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(904, 8, true);
			this.zPostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.zPostingButtonsUserControl.Name = "zPostingButtonsUserControl";
			this.zPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.zPostingButtonsUserControl.TabIndex = 2;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.EMCSCustomsBrokerageUserControl);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1216, 625, true);
			this.zPanel2.TabIndex = 3;
			// 
			// EMCSCustomsBrokerageUserControl
			// 
			this.EMCSCustomsBrokerageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EMCSCustomsBrokerageUserControl, ".");
			this.EMCSCustomsBrokerageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EMCSCustomsBrokerageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EMCSCustomsBrokerageUserControl.Name = "EMCSCustomsBrokerageUserControl";
			this.EMCSCustomsBrokerageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1216, 625, true);
			this.EMCSCustomsBrokerageUserControl.TabIndex = 2;
			// 
			// EMCSDeclarationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1216, 687, true);
			this.Controls.Add(this.zPanel2);
			this.Controls.Add(this.zPanel1);
			this.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1224, 725, true);
			this.Name = "EMCSDeclarationForm";
			this.Text = "EMCS Declaration";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.zPanel2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zPostingButtonsUserControl.ResumeLayout(true);
			this.zPostingButtonsUserControl.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.EMCSCustomsBrokerageUserControl.ResumeLayout(true);
			this.EMCSCustomsBrokerageUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel zPanel1;
		private Core.Forms.ZPostingButtonsUserControl zPostingButtonsUserControl;
		private ZArchitecture.GUI.ZPanel zPanel2;
		private EMCSCustomsBrokerageUserControl EMCSCustomsBrokerageUserControl;
	}
}
