namespace Enterprise.Customs.GB.GUI.CDSCashPayments
{
	partial class CDSCashPaymentsForm
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
            this.CDSCashPaymentsUserControl = new Enterprise.Customs.GB.GUI.CDSCashPayments.CDSCashPaymentsUserControl();
            this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CDSCashPaymentsUserControl.SuspendLayout();
            this.PostingButtonsUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 269, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.CusEntryPayInfo);
            // 
            // CDSCashPaymentsUserControl
            // 
            this.CDSCashPaymentsUserControl.AllowDrop = true;
            this.CDSCashPaymentsUserControl.AutoScroll = true;
            this.BindingSource.SetBindingMember(this.CDSCashPaymentsUserControl, ".");
            this.CDSCashPaymentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CDSCashPaymentsUserControl.Name = "CDSCashPaymentsUserControl";
            this.CDSCashPaymentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 240, true);
            this.CDSCashPaymentsUserControl.TabIndex = 1;
            // 
            // PostingButtonsUserControl
            // 
            this.PostingButtonsUserControl.AllowDrop = true;
            this.PostingButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 244, true);
            this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
            this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
            this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 25, true);
            this.PostingButtonsUserControl.TabIndex = 2;
            // 
            // CDSCashPaymentsForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CDSCashPaymentsForm|e2ee66f2-c66b-433d-96a6-3978b51b1efc", "CDS Cash & PVA Payments");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 293, true);
            this.Controls.Add(this.PostingButtonsUserControl);
            this.Controls.Add(this.CDSCashPaymentsUserControl);
            this.DataSourceType = typeof(Enterprise.Customs.GB.Business.CusEntryPayInfo);
            this.Name = "CDSCashPaymentsForm";
            this.Controls.SetChildIndex(this.CDSCashPaymentsUserControl, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CDSCashPaymentsUserControl.ResumeLayout(true);
            this.CDSCashPaymentsUserControl.PerformLayout();
            this.PostingButtonsUserControl.ResumeLayout(true);
            this.PostingButtonsUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private GUI.CDSCashPayments.CDSCashPaymentsUserControl CDSCashPaymentsUserControl;
        private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
    }
}
