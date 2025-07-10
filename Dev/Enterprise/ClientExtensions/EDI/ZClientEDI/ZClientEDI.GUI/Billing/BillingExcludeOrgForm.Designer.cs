namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class BillingExcludeOrgForm
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OrgsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgsGrid)).BeginInit();
			this.OrgsGrid.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 439, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingExcludeOrgUpdater);
			// 
			// OrgsGrid
			// 
			this.OrgsGrid.AllowNavigation = false;
			this.OrgsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrgsGrid, "ExcludeOrgs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingExcludeOrgUpdater)(null)).ExcludeOrgs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingExcludeOrg)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingExcludeOrgUpdater)(null)).ExcludeOrgs)).SyncRoot)).CEX_BillingSystem)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingExcludeOrg)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingExcludeOrgUpdater)(null)).ExcludeOrgs)).SyncRoot)).CEX_LicenceCode)));
			this.OrgsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "Billing System";
			zDropEditColumnStyleInfo1.ColumnName = "CEX_BillingSystem";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.Caption = "Licence Code";
			zTextBoxColumnStyleInfo1.ColumnName = "CEX_LicenceCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			this.OrgsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrgsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrgsGrid.CopySelectedRowsAllowed = true;
			this.OrgsGrid.GridId = "b91766ad-d7b6-405c-8dbe-c9989f3cddb6";
			this.OrgsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgsGrid.LayoutKey = "OrgsGrid";
			this.OrgsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.OrgsGrid.Name = "OrgsGrid";
			this.OrgsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 393, true);
			this.OrgsGrid.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 409, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 25, true);
			this.PostingButtonsUserControl.TabIndex = 4;
			// 
			// BillingExcludeOrgForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 463, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.OrgsGrid);
			this.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingExcludeOrgUpdater);
			this.Name = "BillingExcludeOrgForm";
			this.Text = "Non-chargeable Licence Codes";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OrgsGrid, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgsGrid)).EndInit();
			this.OrgsGrid.ResumeLayout(false);
			this.OrgsGrid.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid OrgsGrid;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
	}
}