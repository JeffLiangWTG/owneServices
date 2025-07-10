
namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class StlBillingMilestoneSetterForm
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
			this.ButtonReset = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LicenceDatabaseFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BillingPeriodTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LicenceDatabaseFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 79, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 0, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.StlBillingMilestoneSetter);
			// 
			// ButtonReset
			// 
			this.ButtonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonReset.IsCaptionOverridden = true;
			this.ButtonReset.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 45, true);
			this.ButtonReset.Name = "ButtonReset";
			this.ButtonReset.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 23, true);
			this.ButtonReset.TabIndex = 3;
			this.ButtonReset.Text = "Reset";
			this.ButtonReset.ToolTipCaption = null;
			this.ButtonReset.UseVisualStyleBackColor = true;
			this.ButtonReset.Click += new System.EventHandler(this.ButtonReset_Click);
			// 
			// LicenceDatabaseFindBox
			// 
			this.LicenceDatabaseFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicenceDatabaseFindBox, "DatabasePk");
			this.LicenceDatabaseFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 47, true);
			this.LicenceDatabaseFindBox.Name = "LicenceDatabaseFindBox";
			this.LicenceDatabaseFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LicenceDatabaseFindBox.ParentType = null;
			this.LicenceDatabaseFindBox.ShowDescriptionBox = false;
			this.LicenceDatabaseFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.LicenceDatabaseFindBox.TabIndex = 2;
			// 
			// BillingPeriodTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillingPeriodTextBox, "BillingPeriod");
			this.BillingPeriodTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BillingPeriodTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 12, true);
			this.BillingPeriodTextBox.MaxLength = 6;
			this.BillingPeriodTextBox.Name = "BillingPeriodTextBox";
			this.BillingPeriodTextBox.PlaceHolderText = "yyyyMM";
			this.BillingPeriodTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.BillingPeriodTextBox.TabIndex = 1;
			this.BillingPeriodTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BillingPeriodTextBox_KeyPress);
			// 
			// StlBillingMilestoneSetterForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 79, true);
			this.Controls.Add(this.BillingPeriodTextBox);
			this.Controls.Add(this.LicenceDatabaseFindBox);
			this.Controls.Add(this.ButtonReset);
			this.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.StlBillingMilestoneSetter);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "StlBillingMilestoneSetterForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.Text = "STL Billing Milestone Reset";
			this.Controls.SetChildIndex(this.ButtonReset, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.LicenceDatabaseFindBox, 0);
			this.Controls.SetChildIndex(this.BillingPeriodTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LicenceDatabaseFindBox.ResumeLayout(true);
			this.LicenceDatabaseFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton ButtonReset;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox LicenceDatabaseFindBox;
		private ZArchitecture.ZTextBox BillingPeriodTextBox;
	}
}
