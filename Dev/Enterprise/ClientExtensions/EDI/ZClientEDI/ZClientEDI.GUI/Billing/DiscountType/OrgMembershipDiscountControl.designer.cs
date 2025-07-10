namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class OrgMembershipDiscountControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.MembershipGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MembershipGrid)).BeginInit();
			this.MembershipGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.OrgMembershipDiscount);
			// 
			// MembershipGrid
			// 
			this.MembershipGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MembershipGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.OrgMembershipDiscount)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.OrgMembershipDiscountLine)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.OrgMembershipDiscount)(null)).Lines)).SyncRoot)).MembershipType)));
			this.MembershipGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("e07775fb-c451-4cd3-816f-8b4ecec55c0f", "Membership");
			zDropEditColumnStyleInfo1.ColumnName = "MembershipType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.MembershipGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MembershipGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MembershipGrid.GridId = "64b63608-520a-441a-b34d-82aed06e4b8a";
			this.MembershipGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MembershipGrid.LayoutKey = "MembershipGrid";
			this.MembershipGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MembershipGrid.Name = "MembershipGrid";
			this.MembershipGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 219, true);
			this.MembershipGrid.TabIndex = 2;
			// 
			// OrgMembershipDiscountControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MembershipGrid);
			this.Name = "OrgMembershipDiscountControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 219, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MembershipGrid)).EndInit();
			this.MembershipGrid.ResumeLayout(false);
			this.MembershipGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid MembershipGrid;
	}
}
